#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>//atoi
#include <string.h>
#include <sys/stat.h>
#include <fcntl.h>
#include<syslog.h>
#include "LocoParser.h"

int Get_LOCOMATE_Options(LocomateOptions *conf)
{
	FILE *fdrd;
	char mline[200]="";
	char *sts;
	char *token = NULL;
	
	fdrd = fopen(conf->filename, "r");
	if (fdrd <=0) {
     	    printf("Error opening %s file\n", conf->filename);
     	    return -1;
  	}
  	while( (sts = fgets(mline, sizeof(mline), fdrd)) != NULL ) {
	    if (mline[0] != '#' && mline[0] != ';' && mline[0] != ' '){
	        token = strtok(mline, "=");
                
                if(strcasecmp(token, "TTC") == 0 ){
            	    token = strtok(NULL,"\r\n ");
		    conf->TimeToContact = (uint16_t)atoi(token);
		}
                else if(strcasecmp(token, "LANEWIDTH") == 0 ){
            	    token = strtok(NULL,"\r\n ");
		    conf->LaneWidth = (int)atoi(token);
		}
		else if(strcasecmp(token, "EEBL") == 0 ){
		    token = strtok (NULL,"=");
		    strncpy(conf->msg1,token,strlen(token)-1);
		    //printf ("conf message EEBL:%s\n",conf->msg1);
			
		}
		else if(strcasecmp(token, "CSW") == 0 ){
                    token = strtok (NULL,"=");
                    strncpy(conf->msg2,token,strlen(token)-1);
		    //printf("conf message CSW:%s\n",conf->msg2);
            	}
		else if(strcasecmp(token, "FCW") == 0 ){
		    token = strtok (NULL,"=");
                    strncpy(conf->msg3,token,strlen(token)-1);
                    //printf("conf message FCW:%s\n",conf->msg3);
                }  
		else if(strcasecmp(token, "DISPLAY_TYPE") == 0 ){
                    token = strtok(NULL,"\r\n");
                    sscanf(token,"%s",conf->Display_Type);
                    //printf("conf display type:%s \n",conf->Display_Type);
                }
		else if(strcasecmp(token, "SPAT_ENABLE") == 0 ){
                    token = strtok(NULL,"\r\n");
                    sscanf(token,"%s",conf->SpatDisplay);
                    //printf("Spat Display Type:%s \n",conf->SpatDisplay);
                }
	    }
	    if(conf->LaneWidth == 0.0){
		conf->LaneWidth = 5.0;
		}
	    if(conf->TimeToContact == 0){
		conf->TimeToContact = 5;
		}
	    memset(mline, 0, sizeof(mline));
        }
	fclose(fdrd);
	return 1;
}

int fetch_vehicleData(LocomateVehicleSpecs *veh_specs){

    FILE *fdrd;//, *app_arg_ptr;
    uint32_t mod_dep=0;
    char read_line[200]="";//,tmp[50],temp_buf[MAX_SIZE];
    char *token = NULL, *sts = NULL;
    char conf_file[]="/tmp/usb/ModelDeploymentConfigurationItems/ModelDeploymentRemovable.conf";

    /* open the config file in read mode */
    fdrd = fopen(conf_file, "r");
    if (fdrd == NULL) {
	//syslog(LOG_INFO,"Error opening %s file in %s\n", conf_file,__func__);
        return -1;
    }
    while( (sts = fgets(read_line, sizeof(read_line), fdrd)) != NULL ) {
        if (read_line[0] != '#' && read_line[0] != ';' && read_line[0] != ' '){
            token = strtok(read_line, "=");

            if (strcasecmp(token, "TemporaryIDControl") == 0) {
                token = strtok(NULL, " ");
                veh_specs->temp_id = atoi(token);
                //printf("temp_id = %d \n",veh_specs->temp_id);
            }
            if (strcasecmp(token, "ModelDeploymentDeviceID") == 0) {
                token = strtok(NULL, " ");
                sscanf(token,"0x%x",&mod_dep);
		veh_specs->mod_depl = mod_dep;
                //mod_depl = atoi(token);
                //printf("mod_depl = 0x%x\n",veh_specs->mod_depl);
            }
	    if (strcasecmp(token, "MemoryDeviceMountTimeDate") == 0) {
                token = strtok(NULL, " ");
		sscanf(token,"%s",veh_specs->mount_time);
		//printf("mount_time = %s \n",veh_specs->mount_time);
	    }
            if (strcasecmp(token, "VehicleType") == 0) {
                token = strtok(NULL, " ");
                veh_specs->veh_type = atoi(token);
                //printf("veh_type = %d \n",veh_specs->veh_type);
            }
            if (strcasecmp(token, "VehicleLength") == 0) {
                token = strtok(NULL, " ");
                veh_specs->veh_length = atoi(token);
                //printf("veh_length = %d \n",veh_specs->veh_length);
            }
            if (strcasecmp(token, "VehicleWidth") == 0) {
                token = strtok(NULL, " ");
                veh_specs->veh_width = atoi(token);     
                //printf("veh_width = %d \n",veh_specs->veh_width);
            }
        }
        memset(read_line, 0, sizeof(read_line));
    }
    fclose(fdrd);
    return 1;
}
