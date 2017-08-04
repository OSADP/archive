/*
 * Application to parse the configuration file (ModelDeployment.conf) and apply the changes 
 * to database(/var/config)
*/
#include <stdio.h>
#include <ctype.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include<syslog.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <time.h>
#include <sys/wait.h> //for WIFEXITED

#define MAX_SIZE  512

char deploy_file[]="/tmp/usb/ModelDeploymentConfigurationItems/ModelDeploymentRemovable.conf";
int temp_id,lat_offset,long_offset,elv_offset,veh_type,veh_length,vel_width,mac_stat;
uint32_t mod_depl = 0;
char mount_time[50];

void fetch_offset(void);
void Check_RemovableConf();

void fetch_offset(){
    FILE *fdrd;
    char read_line[200],tmp[50],temp_buf[MAX_SIZE];
    char *token = NULL, *sts = NULL;
    double conv=0;

    /* open the config file in read mode */
    fdrd = fopen(deploy_file, "r");
    if (fdrd == NULL) {
	syslog(LOG_INFO,"Error opening %s file in config_parser\n", deploy_file);
        return;
    }
    memset(read_line, 0, sizeof(read_line));
    while( (sts = fgets(read_line, sizeof(read_line), fdrd)) != NULL ) {
        if (read_line[0] != '#' && read_line[0] != ';' && read_line[0] != ' '){
	    token = strtok(read_line, "=");
            if (strcasecmp(token, "antOffsetX") == 0) {
                token = strtok(NULL, " ");
                sscanf(token,"%s",tmp);
                conv = strtod(tmp,NULL);
                conv = conv/100.0; //converting centimeters to meters   

		if (conv < 0)
		    sprintf(temp_buf,"conf_set system:timeSettings:gpsLonOffset \"\\%lf\"",conv);
		else 
		    sprintf(temp_buf,"conf_set system:timeSettings:gpsLonOffset %lf",conv);
		
		system(temp_buf);
		//printf("lon_offset = %s \n",tmp);
            }
	    if (strcasecmp(token, "antOffsetY") == 0) {
                token = strtok(NULL, " ");
		sscanf(token,"%s",tmp);
		conv = strtod(tmp,NULL);
		conv = conv/100.0; //converting centimeters to meters   

		if (conv < 0)
		    sprintf(temp_buf,"conf_set system:timeSettings:gpsLatOffset \"\\%lf\"",conv);
		else 
		    sprintf(temp_buf,"conf_set system:timeSettings:gpsLatOffset %lf",conv);
		
		system(temp_buf);
		//printf("lat_offset = %s \n",tmp);
            }
	    if (strcasecmp(token, "antOffsetZ") == 0) {
                token = strtok(NULL, " ");
		sscanf(token,"%s",tmp);
		conv = strtod(tmp,NULL);
		conv = conv/100.0; //converting centimeters to meters

		if (conv < 0)
		    sprintf(temp_buf,"conf_set system:timeSettings:gpsElevOffset \"\\%lf\"",conv);
		else 
		    sprintf(temp_buf,"conf_set system:timeSettings:gpsElevOffset %lf",conv);
		
		system(temp_buf);
		//printf("elev_offset = %s \n",tmp);
            }
        }
        memset(read_line, 0, sizeof(read_line));
    }
    system("conf_save");
    fclose(fdrd);
}

void Check_RemovableConf(){
    char cmd[100],tmp_buf[100]="";
    FILE *mountfd=NULL;
    int ret = -1, err = -1;
    system("/bin/mkdir -p /tmp/usb");
    ret = system("/bin/mount -t vfat /dev/sda1 /tmp/usb/ > /dev/null 2>&1");
    if((WIFEXITED(ret)) && (WEXITSTATUS(ret)==0)) {
	sprintf(cmd, "/bin/ls %s 2> /dev/null", deploy_file);
	mountfd = popen(cmd,"r");
	if(mountfd != NULL){
	    fscanf(mountfd,"%s",tmp_buf);
            pclose(mountfd);
            if(strstr(tmp_buf,"ModelDeploymentRemovable.conf") != NULL){
		//syslog(LOG_INFO,"ModelDeploymentRemovable.conf found\n");
		fetch_offset();	// fetches offset for database entry
	    }
	    else{
		//syslog(LOG_INFO,"ModelDeploymentRemovable.conf not found\n");
	    }
	}
        ret = system("/bin/umount /dev/sda1");
    }
}

int main(void)
{
    FILE *ProductIDfd = NULL;
    char ProductID[50];

#if 1
    if((ProductIDfd = popen("cat /tmp/productid","r")) != NULL){
        fscanf(ProductIDfd,"%s",ProductID);
        pclose(ProductIDfd);
	if(!(strncmp(ProductID,"LOCOMATE-200-RSU",16))){
            return 0;
	}
    }
    else{
        syslog(LOG_ALERT,"conf_parser: Product-ID not found: \n");
    }
    Check_RemovableConf();
#endif

    return 0;
}
