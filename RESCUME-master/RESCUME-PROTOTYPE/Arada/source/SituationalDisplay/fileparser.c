#include <stdio.h>
#include <ctype.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <fcntl.h>
#include <time.h>
#include "fileparser.h"

char msgstart;
float pktdelaymsecf;
uint8_t start_month,start_day,start_min,start_hour,size;
uint8_t stop_month,stop_day,stop_min,stop_hour;
uint16_t start_year,stop_year;

int char_to_int(char digit){
        int asciiOffset, digitValue;
        if (digit >= 48 && digit <= 57){// code for '0' through '9'
                asciiOffset = 48;
                digitValue = digit - asciiOffset;
                return digitValue;
        }
         else if (digit >= 65 && digit <= 70){ // digit is 'A' through 'F'
                asciiOffset = 55;
                digitValue = digit - asciiOffset;
                return digitValue;
        }
         else if (digit >= 97 && digit <= 102){ // digit is 'a' through 'f'
                asciiOffset = 87;
                digitValue = digit - asciiOffset;
                return digitValue;
        }
	return 0;
}


int convert (char *hexNumber){
	uint8_t hexval;
        char highOrderDig = *(hexNumber+0);
        char lowOrderDig  = *(hexNumber+1);
        uint8_t lowOrderValue  = char_to_int(lowOrderDig);// convert lowOrderDig to number from 0 to 15
        uint8_t highOrderValue = char_to_int(highOrderDig);// convert highOrderDig to number from 0 to 15
        hexval = (lowOrderValue + 16 * highOrderValue);
        return hexval;
}

	
void get_RSE_options(ActiveMsg *msg)
{
	FILE *fdrd;
	int i=0,j=0,q=0,k=0;
	char rdfile[100];
	char mline[200];
	char tempstr[2];
	uint8_t hexa;
	char *sts;
	char *token = NULL,*subtoken1=NULL,*subtoken2,*subtoken3,*token1;
	char *svptr1,*svptr2,*svptr3;
	struct tm *all;
	all = (struct tm *)calloc(1,sizeof(struct tm));
	strcpy(rdfile, msg->actfile); 

	fdrd = fopen(rdfile, "r");
	if (fdrd <=0) {
     		printf("Error opening %s file\n", rdfile);
     		exit(0);
  	}
  	memset(mline, 0, sizeof(mline));
  	while( (sts = fgets(mline, sizeof(mline), fdrd)) != NULL ) {
	    if (mline[0] != '#' && mline[0] != ';' && mline[0] != ' '){
	        token = strtok(mline, "=");
                
                if(strcasestr(token, "Version")){
            	// for future use 
                }

	   	else if (strcasestr(token, "MessageType") || strcasestr(token, "Type") ) {
	   	    token = strtok(NULL, "\r\n ");
                    if(strcasestr(token, "TIM") )
                        msg->MessageType_rse = 16;//TIM
            	    else if(strcasestr(token, "SPAT") )
                        msg->MessageType_rse = 13;//SPAT
            	    else if(strcasestr(token, "MAP") )
                		msg->MessageType_rse = 7;//MAP
	   	}
	   	else if (strcasestr(token, "MessagePSID")  || strcasestr(token, "PSID") ) {
	   	    token = strtok(NULL, " ");
		    sscanf(token, "%x", &msg->app_psid_rse);
	   	}
	   	else if (strcasestr(token, "MessagePriority")  || strcasestr(token, "Priority") ) {
	   	    token = strtok(NULL, " ");
		    msg->priority_rse = atoi(token);
	   	}
		else if(strcasestr(token,"TransmissionMode") || strcasestr(token, "TxMode") ) {
		    token = strtok(NULL, "\r\n ");
		    if(strstr(token,"CONT")) {
			msg->ChannelAcess_rse = 0;//continuos
		    }
		    else {
		        msg->ChannelAcess_rse = 1; //alternate	
		    }
		}
		else if(strcasestr(token,"TransmissionChannel") || strcasestr(token, "Txchannel") ) {
		    token = strtok(NULL, " ");
		    sscanf(token, "%u",&msg->txChan_rse);
		}
		else if((strcasestr(token,"TransmissionBroadcastInterval")) || (strcasestr(token,"TxInterval"))) {
		    token = strtok(NULL, " ");
		    sscanf(token, "%f",&pktdelaymsecf);
		    msg->pktdelaymsecs_rse = 1000*pktdelaymsecf;
			//msg->bcastintrvl_rse = (msg->pktdelaymsecs_rse)/100;
		}
		else if(strcasestr(token,"MessageDeliveryStart") || strcasestr(token,"DeliveryStart")) {
		    token = strtok(NULL, ",\r\n ");
                    if(token==NULL)
                        continue;
                    subtoken1 = strtok_r(token, "/",&svptr1);
		    subtoken2 = strtok_r(svptr1, "/",&svptr2);
		    subtoken3 = strtok_r(svptr2, ",",&svptr3);
		    all->tm_mon = atoi(subtoken1)-1;
		    all->tm_mday = atoi(subtoken2);
		    all->tm_year = atoi(subtoken3)-1900;
		    token = strtok(NULL," ");
		    token1 = strtok_r(token,":",&svptr1);
		    all->tm_hour = atoi(token1);	
		    token = strtok_r(svptr1," ",&svptr2);
		    all->tm_min = atoi(token);
		    msg->start_utctime_sec=mktime(all);
                }
		else if(strcasestr(token,"MessageDeliveryStop") || strcasestr(token,"DeliveryStop")) {
		    token = strtok(NULL, ",\r\n ");
                    if(token==NULL)
                        continue;
                    subtoken1 = strtok_r(token, "/",&svptr1);
		    subtoken2 = strtok_r(svptr1, "/",&svptr2);
		    subtoken3 = strtok_r(svptr2, ",",&svptr3);
		    all->tm_mon = atoi(subtoken1)-1;
		    all->tm_mday = atoi(subtoken2);
		    all->tm_year = atoi(subtoken3)-1900;
		    token = strtok(NULL," ");
		    token1 = strtok_r(token,":",&svptr1);
		    all->tm_hour = atoi(token1);	
		    token = strtok_r(svptr1," ",&svptr2);
		    all->tm_min = atoi(token);
		    msg->stop_utctime_sec=mktime(all);
		}

	    	else if(strcasestr(token,"MessageSignature") ||strcasestr(token,"Signature")) {
	    	    token = strtok(NULL, "\r\n");
	    	    if(strcasestr(token,"True")){ 
	    	        msg->SecurityType_rse =1;
	            }
	    	    else if(strcasestr(token,"False")){ 
	    	        msg->SecurityType_rse =0;
	            }
		}
		else if(strcasestr(token,"MessageEncryption") ||strcasestr(token,"Encryption") ) {
		    token = strtok(NULL, "\r\n");
		    if(strcasestr(token,"True")) {
		    	msg->SecurityType_rse =2;
		    }
		}
		
		else if(strcasestr(token,"MessagePayload") || strcasestr(token,"Payload")){
			subtoken1 = strtok(NULL, "");
			for(j=0;(*(subtoken1+j))!='\0';j++){
				if(((*(subtoken1+j))!= ' ') && (*(subtoken1+j))!='\n'){
					tempstr[q]=(*(subtoken1+j));
					q++;
					if(q==2){
						hexa = convert(tempstr);
						msg->payload[i]= hexa;
						q=0;
						i++;
					}
					
						
					}
				}			
		}
		else {
				for(k=0;(*(token+k))!='\0';k++){
					if(((*(token+k))!= ' ')&&((*(token+k))!= '\n')){
						tempstr[q]=(*(token+k));
						q++;
					if(q==2){
						hexa = convert(tempstr);
						msg->payload[i]= hexa;
						q=0;
						i++;
					}
						}
				}
			}
	}
	memset(mline, 0, sizeof(mline));
  }
	msg->payload[i]='\0';
	msg->payload_size = i;
	fclose(fdrd);
	free(all);
}


