#include <unistd.h>
#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <syslog.h>
#include <string.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#ifdef SDK_NEW
#define WPS_OFF 8
#endif

/* Application called on user joining IP-service
   argv[1]=defaultgw from WRA
   argv[2]=primaryDNS from WRA
   argv[3]=prefix-length
 */

void set_ipv6(char *ipv6)
{
    char old_ipv6[50] = {0}, cmd[100] = {0};
    FILE *fp = NULL;
    fp = popen("ifconfig brwifi | grep -i inet6 | grep -i global | awk '{print $3}'", "r");
    if(fp != NULL)
    {
        fscanf(fp, "%s", old_ipv6);
        sprintf(cmd, "ip a d %s dev brwifi",old_ipv6);
        system(cmd);
        sprintf(cmd, "ip r d %s dev brwifi",old_ipv6);
        system(cmd);
        pclose(fp);
    }
    bzero(cmd,100);    
    sprintf(cmd, "ip -6 route del 2000::/3 dev brwifi");
    system(cmd);
    sprintf(cmd, "ip a a %s dev brwifi", ipv6);
    system(cmd);
    return;
}

int stringtomac(uint8_t*mac,char* macstr)
{
    uint8_t number,i;
    char ch;
    const char cSep = ':';
    for (i = 0; i < 6; ++i)
    {
        ch = tolower (*macstr++);
        if ((ch < '0' || ch > '9') && (ch < 'a' || ch > 'f'))
        {
            return 0;
        }
        number = isdigit (ch) ? (ch - '0') : (ch - 'a' + 10);
        ch = tolower (*macstr);
        if ((i < 5 && ch != cSep) || (i == 5 && ch != '\0' && !isspace (ch)))
        {
            if ((ch < '0' || ch > '9') && (ch < 'a' || ch > 'f'))
            {
                return 0;
            }
            number <<= 4;
            number += isdigit (ch) ? (ch - '0') : (ch - 'a' + 10);
            ++macstr;
            ch = *macstr;
            if (i < 5 && ch != cSep)
            {
                return 0;
            }
            mac[i] = number;

        }
        ++macstr;
    }
    return 1;
}

int main(int argc, char* argv[])
{
    // int i,status;
    int fd = 0;
    FILE *mac_fp=NULL;
    char mac_addr[50]={0};
    uint8_t mac[6]={0};
    char buff[1024]={0};
    char gateway[50]={0};
    char new_addr[100]={0};
    char cmd[100]={0};
    uint8_t prefix_length;
    uint8_t octets,offset;
    prefix_length=atoi(argv[3]);
    if(prefix_length == 0)
    {
        syslog(LOG_INFO,"Asm.bin killed restarting Apps. Initiated from wsmpdev\n");
    	system("/usr/local/bin/asmrestart.sh &");
        sleep(2);
        return 0;
    }
    octets=prefix_length/16;
//---------------generate address----------------------------------
    memcpy(gateway,argv[1],strlen(argv[1]));
    memcpy(new_addr,gateway,(octets*5));
    offset =octets*5;
    if(octets < 4)
    {
        sprintf(new_addr+offset,":");
        offset++;
    }
//---------------ADDRESS CONFIGURATION-----------------------------
//make a mac address from gw and prefix length
    mac_fp=popen("ifconfig brwifi | grep -i hwaddr | awk '{print $5}'","r");
    if(mac_fp!=NULL)
    {
        fscanf(mac_fp,"%s",mac_addr);
        pclose(mac_fp);
    }
    else
        syslog(LOG_INFO,"unable to find brwifi mac address\n");
    if(stringtomac(mac,mac_addr)==0)
        bzero(mac,6);
    sprintf(new_addr+offset,"%02x%02x:%02xff:fe%0x:%02x%02x/%d",(2^mac[0]),mac[1],mac[2],mac[3],mac[4],mac[5],prefix_length);
    (void)set_ipv6(new_addr); //set new global ipv6 address here and flushall previous routes
//---------------GATEWAY-----------------------------
    sprintf(cmd,"route -A inet6 add 2000::/3 gw %s",gateway);
    system(cmd);
    syslog(LOG_INFO,"user joined IP service-- %s",gateway);
//---------------DNS-----------------------------
    strcpy(buff, ";generated on IP-Service User join\n");
    strcat(buff, "nameserver  ");
    strcat(buff, argv[2]);
    strcat(buff, "\n");
    strcat(buff, "\n");
    fd = open("/tmp/resolv.conf_tmp", O_RDWR | O_CREAT | O_TRUNC);
    write(fd, buff, strlen(buff));
    close(fd);

    system("mv /tmp/resolv.conf_tmp /etc/resolv.conf");
    chmod("/etc/resolv.conf",0644);

    return 0;
}

