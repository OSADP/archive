#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <syslog.h>
#include<sys/types.h>
#include<unistd.h>
#include<sys/time.h> // for gettimeofday
#include <sys/ioctl.h>
#include <sys/select.h>
#include <unistd.h>
#include <ctype.h>
#include <math.h>
#include <errno.h>
#include <assert.h>
#include <signal.h>
#include <stdbool.h>
#include <getopt.h>
#include <dirent.h>
#include <fcntl.h>
#include <sys/stat.h>
#include <sys/syscall.h>


char filename[255];
char ip[255];
char syscmd[250];
char username[250];


int ipflag=0;
int onload=0;
int offload=0;
int proto=0;
int limit=15;

void usage() {
    printf("\nusage: usb-deamon\n");
    printf(" \n******** Options ******\n");
    printf("\n\t -h:\tThis help \n");
    printf("\t -l:\t To List the ActiveMessageList)\n");
    printf("\t -g:\tFile name to On-load/Addition\n");
    printf("\t -u:\tUser Name for SCP\n");
    printf("\t -i:\tIP Address to on-load/off-load/addition\n");
    printf("\t -d:\tFile name to delete)\n");
    printf("\t -o:\tFile name to off-load)\n");
    printf("\t -p:\tprotocol for on-load/addition/off-load. SCP=0, TFTP=1)\n");
	exit(0);
}

void Options(int argc, char *argv[])
{
    int index = 0;
    int t;
    static struct option opts[] =
    {
        {"help", no_argument, 0, 'h'},
        {"List", no_argument, 0, 'l'},
        {"On-load/Addition", required_argument, 0, 'g'},
        {"IP Address", required_argument, 0, 'i'},
        {"Delete", required_argument, 0, 'd'},
        {"Off-Load", required_argument, 0, 'o'},
        {"Protocol", required_argument, 0, 'p'},
        {"File Limit", required_argument, 0, 'a'},
        {"SCP User", required_argument, 0, 'u'},
        {0, 0, 0, 0}
    };
    while(1) {
        t = getopt_long(argc, argv, "hlu:i:d:o:p:a:g:", opts, &index);
        if(t < 0) {
            break;
        }
        switch(t) {
            case 'h':
                usage();
	            break;

            case 'l':
                //strcpy(mntpath, optarg);
                sprintf(syscmd, "find /var/AML/* -exec awk \'BEGIN {printf(\"Filename: *** %%s ***\\n\",ARGV[1]);} /=/ {print $0} END {print ""}\'   \'{}\' \';\'");
                system(syscmd);
                break;

            case 'g':
                strcpy(filename, optarg);
				onload=1;
            	break;

            case 'u':
                strcpy(username, optarg);
            	break;

            case 'i':
                strcpy(ip, optarg);
				ipflag = 1;
            	break;

            case 'd':
                strcpy(filename, optarg);
				sprintf(syscmd,"rm -f /var/AML/%s",filename);//remove from usb
                system(syscmd);
				syslog(LOG_INFO, "AML: Deleted %s\n",filename);
            	break;

            case 'o':
                strcpy(filename, optarg);
				offload=1;
            	break;

            case 'p':
                proto = atoi(optarg);
            	break;

            case 'a':
                limit = atoi(optarg);
            	break;

            default:
               usage();
            	break;
        }
    }
}

int main(int argc, char*argv[])
{
	DIR * dirp;
    struct dirent * entry;
	int i=0,count=0;
	
	strcpy(username,"root");
	Options(argc ,argv);
	if (onload && ipflag){

        dirp = opendir("/var/AML/"); /* There should be error handling after this */
        while ((entry = readdir(dirp)) != NULL) {
            if (entry->d_type == DT_REG) /* If the entry is a regular file */
                count++;
        }
		if ((count >= limit) && (strstr(filename,".tar")==NULL)){
			printf("limit excedded\n");
			return 0;
		}
			
		if(proto){
			sprintf(syscmd,"tftp -g -l /tmp/%s -r %s %s",filename,filename,ip);
            system(syscmd);
		}
		else if(proto == 0){
			sprintf(syscmd,"scp -r %s@%s:%s /tmp",username,ip,filename);
        	system(syscmd);
		}
		if (strstr(filename,".tar")!=NULL){
			sprintf(syscmd,"cd /var/AML; rm -f *; tar xf /tmp/%s; rm -f /tmp/%s",filename,filename);
        	system(syscmd);
		}
		else{
			sprintf(syscmd,"mv /tmp/%s /var/AML/",filename);
       		system(syscmd);
		}
		syslog(LOG_INFO, "AML: on-load/Added %s from %s\n",filename,ip);
	}
	else if (offload && ipflag){
        sprintf(syscmd,"cd /var/AML; tar -cvf %s *",filename);
        system(syscmd);
        if(proto){
            sprintf(syscmd,"tftp -p -l /var/AML/%s -r %s %s",filename,filename,ip);
            system(syscmd);
        }
        else if(proto == 0){
            sprintf(syscmd,"scp /var/AML/%s %s@%s:",filename,username,ip);
            system(syscmd);
        }
        system("rm -f /var/AML/*.tar");
		syslog(LOG_INFO, "AML: off-load to %s\n",ip);
	}

    return 0;

}

