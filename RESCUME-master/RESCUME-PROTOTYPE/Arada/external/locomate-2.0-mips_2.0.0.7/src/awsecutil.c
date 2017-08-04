#include "awsec.h"
#include "queue.h"
#include <getopt.h>


extern ECKeyData_t MyKeyInfo;
extern LCM_store lcmStore;
struct lcm_conf_options lcmoptions;
FILE *lcm_logfile;
char lcmlogfilename[30];
pthread_mutex_t LcmLogLock = PTHREAD_MUTEX_INITIALIZER;
extern uint8_t iptype;
extern char* StageString[];
char cert_path[] ="/tmp/usb/lcm/"; 

double R1 = 40680631590769.0; //square of constant 6378137
double R2 = 40408299984087.05552164;//square of constant 6356752.3142
double R2divR1 = 0.99330562;
double pidiv180 = 0.017453293;
double one80divpi = 57.295779513;
double pi = 3.14159265358979323846;

Certificate RootCAcert;
UINT8 rootDigest[32], rootCA_cert[512];
UINT32 rootDigestLen;
UINT8 root_ca_dec[512];
UINT32 root_decLen = 0;
struct asm_conf_options asmoptions;
char serverPath[255]="/tmp";
char securityFile[255]="security.conf";
uint8_t versionNo[15]="1.0.0";
uint32_t debugPrints=0;
uint8_t lcm_log_level=0;
uint8_t asm_log_level=0;
FILE *asmlogfile;
WSApsidPermission psidPermission;
char asmlogfilename[30];
int ASMLOG(uint8_t flag,char * buf,uint8_t *data, uint32_t dL);
int LCMLOG(uint8_t flag,char * buf,uint8_t *data, uint32_t dL);
int AWSEC_LOG(uint8_t caller,uint8_t *data, uint32_t len,const char *fmt, ...);

pthread_mutex_t AsmLogLock = PTHREAD_MUTEX_INITIALIZER;


UINT8 *BUFPUT32(UINT8 *cp, UINT32 val);
UINT32 AWSec_ecies_encrypt(UINT8 *PubKey, UINT8 *pData, UINT32 DataLen, UINT8 *pKeyData, UINT32 *keyLen, UINT8 *pCipher, UINT32 *CipherLen, UINT8 *pMac, UINT32 *macLen, UINT32 uiTagLen, UINT32 CypherAlgo, uint8_t *errorCode);
UINT32 AWSec_ecies_decrypt(UINT8 *key, UINT8 *pKeyData, UINT32 keyLen, char *pCipher, UINT32 length, UINT8 *mac, UINT32 macLen, UINT32 uiTagLen, UINT32 CypherAlgo, UINT8 *pOutBuf, UINT32 *outlen, uint8_t *errorCode); 
ECDSA_SIG * AWSecSignData(UINT8 *pDigest, UINT32 uiDigestLen, EC_KEY *ECKey, UINT8 *pSignData, UINT32 *uiSignDataLen);
ECDSA_SIG *Fast_ecdsa_do_sign(const unsigned char *dgst, int dgst_len,const BIGNUM *in_kinv, const BIGNUM *in_r, EC_KEY *eckey, int *yInfo);
EC_KEY * ECKeyFromPrivKey(UINT32 GroupType, EC_KEY *ECKey, UINT8 *pPrivKey, UINT32 uiKeyLen);
UINT32 AWSecDigestAndVerifyData(UINT8 *pData, UINT32 uiDataLen, UINT8 * pSignature, UINT8 * PubKey);
uint32_t getPsidbyLen(uint8_t *addr,int *retIdx, uint8_t *buff);
void ReconstructPub(UINT8 * pPrivKey, EC_GROUP *pGroup);
Time32 get_cur_time2004(void);
void distance_cal(location *);

UINT32 ComputeRevokeID(UINT32 i, UINT8 *link1, UINT8 * link2, UINT32 max_i, UINT32 DayTimeI, UINT32 DayTimeJ, UINT8 *pRevokeID)
{
  UINT8 linkID1[32], linkID2[32], pTmpBuf1[32], pTmpBuf2[32], maxJ[4];
  UINT32 k, uiDigestLen, uiCipherTextLen, iRet;
  if((i < DayTimeI) || (DayTimeI > max_i))
  {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"ComputeRevokeID():Day Index is out of the range\n");
    pRevokeID = NULL;
    return SUCCESS;
  }
  for(k = i; k < DayTimeI; k++)
  {
    if (AWSecHash(HASH_ALGO_SHA256, link1, 16, linkID1, &uiDigestLen, 0) == NULL)
    {
          if(asm_log_level >= LOG_CRITICAL)
	        AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));	  
    }
    else
      memcpy(link1, &linkID1[16], 16);
    if (AWSecHash(HASH_ALGO_SHA256, link2, 16, linkID2, &uiDigestLen, 0) == NULL)
    {
          if(asm_log_level >= LOG_CRITICAL)
	        AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));	  
    }
    else
      memcpy(link2, &linkID2[16], 16);
  }
  memcpy(maxJ, &DayTimeJ, 4);
  linkID1[3] = maxJ[0];
  linkID1[2] = maxJ[1];
  linkID1[1] = maxJ[2];
  linkID1[0] = maxJ[3];
  for (k = 4; k < 16; k++)
    linkID1[k] = 12;
  memcpy(linkID2, linkID1, 16);
  if (AWSecEncrypt(CIPHER_ALGO_AES128_ECB, link1, NULL, 0, linkID1, 16, pTmpBuf1, &uiCipherTextLen, 0) == FAILURE)
  {
      if(asm_log_level >= LOG_CRITICAL)
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecEncrypt(): %s\n",__FILE__,__LINE__, strerror(errno));    
  }
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,pTmpBuf1,uiCipherTextLen,"pTmpBuf1:");
  }
  if (AWSecEncrypt(CIPHER_ALGO_AES128_ECB, link2, NULL, 0, linkID2, 16, pTmpBuf2, &uiCipherTextLen, 0) == FAILURE)
  {
      if(asm_log_level >= LOG_CRITICAL)
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecEncrypt(): %s\n",__FILE__,__LINE__, strerror(errno));
  }
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,pTmpBuf2,uiCipherTextLen,"pTmpBuf2:");
  }
  for(k = 0; k < 10; k++)
  {
    pRevokeID[k] = pTmpBuf1[6 + k] ^ pTmpBuf2[6 + k];
  }
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,pRevokeID,10,"pRevokeID: \n");
  }
  return SUCCESS;
}
 
Time64 ntohll( const Time64 htime)
{
    UINT32 x = 1;
    if (*(char *) &x == 1) {
        return ((((Time64)ntohl(htime)) << 32) | ntohl(htime >> 32));
    }
    return htime;

}

void usage() {
    printf("\nusage: Security server\n");
    printf(" \n******** Options ******\n");
    printf("\n\t -h:\tThis help \n");
    printf("\t -c:\tServer Start Directory(Specify Path Ending With)\n");
    printf("\t -f:\tWhich Configuration file to be loaded\n");
    printf("\t -v:\tDisplay Version Number \n");
    printf("\t -d:\tEnable or Disable Debug prints \n");
    exit(0);
}

void Options(int argc, char *argv[])
{
    int index = 0;
    int t;
    static struct option opts[] = {
        {"help", no_argument, 0, 'h'},
        {"Version Number", no_argument, 0, 'v'},
        {"Directory Path", required_argument, 0, 'c'},
        {"Security File", required_argument, 0, 'f'},
        {"Debug Prints", required_argument, 0, 'd'},
        {0, 0, 0, 0}
    };
    while(1) {
        t = getopt_long(argc, argv, "hvc:f:d:", opts, &index);
        if(t < 0) {
            break;
        }
        switch(t) {
            case 'h':
                usage();
                break;
            case 'c':
                strcpy(serverPath, optarg);
                break;
            case 'f':
                strcpy(securityFile, optarg);
                break;
            case 'v':
                printf("Arada Security Module Version %s\n",versionNo);
  		if(asm_log_level >= LOG_DEBUG){
                    AWSEC_LOG(ASM,NULL,0,"Arada Security Module Version Number %s #####\n",versionNo);
		}
                break;
            case 'd':
                debugPrints = atoi(optarg);
                break;
            default:
               usage();
               break;
        }
    }
}


int asm_conf_parse(void)
{
    FILE *fdrd;
    char rdfile[100],mline[200];
    char *sts;
    char *token = NULL;
    int ret =0;
    struct in6_addr tmp_addr;
    

    strcpy(rdfile,ASM_CONF_FILE);
    fdrd = fopen(rdfile, "r");
    if (fdrd <=0)
    {
        syslog(LOG_INFO,"Error opening %s file\n", rdfile);
        exit(0);
    }

    memset(mline, 0, sizeof(mline));

    while( (sts = fgets(mline, sizeof(mline), fdrd)) != NULL ) {

        if (mline[0] != '#' && mline[0] != ';' && mline[0] != ' ') {
            token = strtok(mline, "=");

            if (strcasestr(token, "SM_SOCKET_TYPE")) {
                token = strtok(NULL, ";");
                asmoptions.SM_SOCKET_TYPE = atoi(token);
                //printf("SM_SOCKET_TYPE is:%d\n",asmoptions.SM_SOCKET_TYPE);
            }
            else if (strcasestr(token, "SM_PORT_NUMBER")) {
                token = strtok(NULL, ";");
                asmoptions.SM_PORT_NUMBER = atoi(token);
                //printf("SM_PORT_NUMBER is:%d\n",asmoptions.SM_PORT_NUMBER);
            }

            else if(strcasestr(token,"SM_SOCKET_PATH")) {
                token = strtok(NULL, ";");
                sscanf(token, "%s",asmoptions.SM_SOCKET_PATH);
                //printf("SM_SOCKET_PATH :%s \n",asmoptions.SM_SOCKET_PATH);
            }

            else if (strcasestr(token,"MSG_TOLERANCE_TIME")) {
                token = strtok(NULL, ";");
                asmoptions.MSG_TOLERANCE_TIME = atoi(token);
                //printf("MSG_TOLERANCE_TIME:%d \n",asmoptions.MSG_TOLERANCE_TIME);
            }

            else if (strcasestr(token,"CERT_CHECK_PERIOD")) {
                token = strtok(NULL, ";");
                asmoptions.CERT_CHECK_PERIOD = atoi(token);
                //printf("CERT_CHECK_PERIOD:%d \n",asmoptions.CERT_CHECK_PERIOD);
            }

            else if (strcasestr(token,"MSG_QUEUE_TIME")) {
                token = strtok(NULL, ";");
                asmoptions.MSG_QUEUE_TIME = atoi(token);
                //printf("MSG_QUEUE_TIME:%d \n",asmoptions.MSG_QUEUE_TIME);
            }

            else if (strcasestr(token,"MSG_FRAG_TIME_OUT")) {
                token = strtok(NULL, ";");
                asmoptions.MSG_FRAG_TIME_OUT = atoi(token);
                //printf("MSG_FRAG_TIME_OUT:%d \n",asmoptions.MSG_FRAG_TIME_OUT);
            }
            else if (strcasestr(token,"USE_CRL_FILE")) {
                token = strtok(NULL, ";");
                asmoptions.USE_CRL_FILE = atoi(token);
                //printf("USE_CRL_FILE:%d \n",asmoptions.USE_CRL_FILE);
            }

            else if(strcasestr(token,"CRL_FILE_NAME")) {
                token = strtok(NULL, ";");
                sscanf(token, "%s",asmoptions.CRL_FILE_NAME);
                //printf("CRL_FILE_NAME:%s \n",asmoptions.CRL_FILE_NAME);
            }

            else if(strcasestr(token,"LOG_OUTPUT_STREAM")) {
                token = strtok(NULL,";");
                asmoptions.LOG_OUTPUT_STREAM = atoi(token);
                //printf("LOG_OUTPUT_STREAM:%d \n",asmoptions.LOG_OUTPUT_STREAM);
            }

            else if(strcasestr(token,"LOG_FILE_SIZE")) {
                token = strtok(NULL,";");
                asmoptions.LOG_FILE_SIZE = atoi(token);
                //printf("LOG_FILE_SIZE:%d \n",asmoptions.LOG_FILE_SIZE);
            }

            else if(strcasestr(token,"LOG_FILE_NAME")) {
                token = strtok(NULL, ";");
                sscanf(token, "%s",asmoptions.LOG_FILE_NAME);
                //printf("LOG_FILE_NAME:%s \n",asmoptions.LOG_FILE_NAME);
            }

            else if(strcasestr(token,"SET_SYSTEM_TIME")) {
                token = strtok(NULL, ";");
                asmoptions.SET_SYSTEM_TIME = atoi(token);
                //printf("SET_SYSTEM_TIME:%d \n",asmoptions.SET_SYSTEM_TIME);
            }
            else if(strcasestr(token,"LOCAL_TIME_CONFIDENCE")) {
                token = strtok(NULL, ";");
                asmoptions.LOCAL_TIME_CONFIDENCE = atoi(token);
                //printf("LOCAL_TIME_CONFIDENCE:%d \n",asmoptions.LOCAL_TIME_CONFIDENCE);
            }
            else if(strcasestr(token,"LOCAL_LOCATION_CONFIDENCES")) {
                token = strtok(NULL, ";");
                asmoptions.LOCAL_LOCATION_CONFIDENCES = atoi(token);
                //printf("LOCAL_LOCATION_CONFIDENCES:%d \n",asmoptions.LOCAL_LOCATION_CONFIDENCES);
            }

            else if(strcasestr(token,"MAX_CERT_CHAIN_LENGTH")) {
                token = strtok(NULL, ";");
                asmoptions.MAX_CERT_CHAIN_LENGTH = atoi(token);
                //printf("MAX_CERT_CHAIN_LENGTH:%d \n",asmoptions.MAX_CERT_CHAIN_LENGTH);
            }
            if(strcasestr(token,"KEY_CONF_DIR")) {
                token = strtok(NULL, ";");
                sscanf(token, "%s",asmoptions.KEY_CONF_DIR);
                //printf("KEY_CONF_DIR:%s \n",asmoptions.KEY_CONF_DIR);
            }

            else if(strcasestr(token,"GEOGRAPHIC_CONSISTENCY_CHECK")) {
                token = strtok(NULL, ";");
                asmoptions.GEOGRAPHIC_CONSISTENCY_CHECK = atoi(token); 
                //printf("GEOGRAPHIC_CONSISTENCY_CHECK:%d \n",asmoptions.GEOGRAPHIC_CONSISTENCY_CHECK);
            }

            else if(strcasestr(token,"DEVICE_MODE")) {
                token = strtok(NULL, ";");
                asmoptions.DEVICE_MODE = atoi(token);
                //printf("DEVICE_MODE :%d \n",asmoptions.DEVICE_MODE);
            }

            else if(strcasestr(token,"RELOAD_PERIOD")) {
                token = strtok(NULL, ";");
                asmoptions.RELOAD_PERIOD = atoi(token);
                //printf("RELOAD_PERIOD:%d \n",asmoptions.RELOAD_PERIOD);
            }

            else if(strcasestr(token,"FPGA_DELAY_TIME1")) {
                token = strtok(NULL, ";");
                asmoptions.FPGA_DELAY_TIME1 = atoi(token);
                //printf("FPGA_DELAY_TIME1:%d \n",asmoptions.FPGA_DELAY_TIME1);
            }

            else if(strcasestr(token,"FPGA_DELAY_TIME2")) {
                token = strtok(NULL, ";");
                asmoptions.FPGA_DELAY_TIME2 = atoi(token);
                //printf("FPGA_DELAY_TIME2:%d \n",asmoptions.FPGA_DELAY_TIME2);
            }

            else if(strcasestr(token,"MSG_BUFFER_SIZE")) {
                token = strtok(NULL, ";");
                asmoptions.MSG_BUFFER_SIZE = atoi(token);
                //printf("MSG_BUFFER_SIZE:%d \n",asmoptions.MSG_BUFFER_SIZE);
            }

            else if(strcasestr(token,"MSG_QUEUE_SIZE")) {
                token = strtok(NULL, ";");
                asmoptions.MSG_QUEUE_SIZE = atoi(token);
                //printf("MSG_QUEUE_SIZE:%d \n",asmoptions.MSG_QUEUE_SIZE);
            }
            else if(strcasestr(token,"PROC_THREAD_COUNT")) {
                token = strtok(NULL, ";");
                asmoptions.PROC_THREAD_COUNT = atoi(token);
                //printf("PROC_THREAD_COUNT:%d \n",asmoptions.PROC_THREAD_COUNT);
            }
            
            else if(strcasestr(token,"FPGA_QUEUE_SIZE")) {
                token = strtok(NULL, ";");
                asmoptions.FPGA_QUEUE_SIZE = atoi(token);
                //printf("FPGA_QUEUE_SIZE:%d \n",asmoptions.FPGA_QUEUE_SIZE);
            }

            else if(strcasestr(token,"MSG_FRESHNESS_CHECK")) {
                token = strtok(NULL, ";");
                asmoptions.MSG_FRESHNESS_CHECK = atoi(token);
                //printf("MSG_FRESHNESS_CHECK:%d \n",asmoptions.MSG_FRESHNESS_CHECK);
            }

            else if(strcasestr(token,"MSG_REPLAY_CHECK")) {
                token = strtok(NULL, ";");
                asmoptions.MSG_REPLAY_CHECK = atoi(token);
                //printf("MSG_REPLAY_CHECK:%d \n",asmoptions.MSG_REPLAY_CHECK);
            }

            else if(strcasestr(token,"MSG_LOCATION_CHECK")) {
                token = strtok(NULL, ";");
                asmoptions.MSG_LOCATION_CHECK = atoi(token);
                //printf("MSG_LOCATION_CHECK:%d \n",asmoptions.MSG_LOCATION_CHECK);
            }

            else if(strcasestr(token,"TCP_MAX_PEND_CONNECTION")) {
                token = strtok(NULL, ";");
                asmoptions.TCP_MAX_PEND_CONNECTION = atoi(token);
                //printf("TCP_MAX_PEND_CONNECTION:%d \n",asmoptions.TCP_MAX_PEND_CONNECTION);
            }

        }

        memset(mline, 0, sizeof(mline));

    }
    fclose(fdrd);
    return 1;
}

int get_rootCA_Data()
{
    INT32 uiTmpBufLen,bFound = 0,i=0;  
    char tmp_str[256];
    struct issuerCertInfo *issuerntry;
    struct WSAissuerCertInfo *WSAissuerntry;
    
/*** Reading root_ca.cert and adding it to Issuer List *****/    
    
    bzero(tmp_str,256);
    sprintf(tmp_str,"%s/%s/root_ca.cert",serverPath,asmoptions.KEY_CONF_DIR);
    uiTmpBufLen = read_file(tmp_str, rootCA_cert, sizeof(rootCA_cert));
    RootCAcert.certLen = certParse(rootCA_cert, &RootCAcert);
    memcpy(RootCAcert.certInfo, rootCA_cert, RootCAcert.certLen);
    if (AWSecHash(HASH_ALGO_SHA256, RootCAcert.certInfo, RootCAcert.certLen, rootDigest, &rootDigestLen, 8) == NULL){
        if(asm_log_level >= LOG_CRITICAL)
	    AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
	return FAILURE;
    }
    bFound = 0;
    LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
    if(asm_log_level >= LOG_INF)
        AWSEC_LOG(ASM,NULL,0,"How many times the Issuer list was executed\n");    
        if (!(memcmp(issuerntry->pCertInfo.hash, rootDigest, 8))){
    	    if(asm_log_level >= LOG_INF)
                AWSEC_LOG(ASM,NULL,0,"Root Certificate found in the Issuer list\n");
            bFound = 1;
            break;
        }
    }
    if (!bFound){
        issuerntry = (struct issuerCertInfo *)malloc(sizeof(struct issuerCertInfo));
        memcpy(issuerntry->pCertInfo.hash, rootDigest, 8);
        issuerntry->pCertInfo.slotCert.certLen = certParse(RootCAcert.certInfo, &issuerntry->pCertInfo.slotCert);
        memcpy(issuerntry->pCertInfo.slotCert.certInfo, RootCAcert.certInfo, issuerntry->pCertInfo.slotCert.certLen);      
        issuerntry->pCertInfo.certCheckFlag = 0x00;
	issuerntry->processTime = get_cur_time2004();
        LIST_INSERT_HEAD(&issuerListHead, issuerntry, issuer_list);
    	if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"NAZEER: Root Certificate not found in the Cert Store(Issuer) List, Adding it to Issuer List\n");
    }
    else{
        issuerntry->processTime = get_cur_time2004();
    	if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"NAZEER: Root Certificate found in the Cert Store(Issuer) List\n");
    }
    
/*** Reading root_ca.cert and adding it to WSA Issuer List , If Device Mode is 2 *****/    
    
        bFound = 0;
        LIST_FOREACH(WSAissuerntry, &WSAissuerListHead, WSAissuer_list){
        if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"How many times the WSA Issuer list was executed\n");
            if (!(memcmp(WSAissuerntry->pCertInfo.hash, rootDigest, 8))){
                if(asm_log_level >= LOG_INF)
                    AWSEC_LOG(ASM,NULL,0,"Root Certificate found in the WSA Issuer list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
            WSAissuerntry = (struct WSAissuerCertInfo *)malloc(sizeof(struct WSAissuerCertInfo));
            memcpy(WSAissuerntry->pCertInfo.hash, rootDigest, 8);
            WSAissuerntry->pCertInfo.slotCert.certLen = certParse(RootCAcert.certInfo, &WSAissuerntry->pCertInfo.slotCert);
            memcpy(WSAissuerntry->pCertInfo.slotCert.certInfo, RootCAcert.certInfo, WSAissuerntry->pCertInfo.slotCert.certLen);      
            WSAissuerntry->pCertInfo.certCheckFlag = 0x00;
	    WSAissuerntry->processTime = get_cur_time2004();
            LIST_INSERT_HEAD(&WSAissuerListHead, WSAissuerntry, WSAissuer_list);
            if(asm_log_level >= LOG_INF)
                AWSEC_LOG(ASM,NULL,0,"NAZEER: Root Certificate not found in the Cert Store(WSA Issuer) List, Adding it to WSA Issuer List\n");
        }
        else{
            WSAissuerntry->processTime = get_cur_time2004();
            if(asm_log_level >= LOG_INF)
                AWSEC_LOG(ASM,NULL,0,"NAZEER: Root Certificate found in the Cert Store(WSA Issuer) List\n");
        }
    
/*** Reading root_ca_dec.key and Storing it to root_ca_dec for further use *****/    

    bzero(tmp_str,256);
    sprintf(tmp_str,"%s/%s/root_ca_dec.key",serverPath,asmoptions.KEY_CONF_DIR);
    root_decLen = read_file(tmp_str, root_ca_dec, sizeof(root_ca_dec));
    return 0;
}
 
/* Reading Psid Permission For WSA certificate and storing them to psidPermission Structure */ 

int read_psidPermission_from_cert(certStore *cert)
{
    int i = 0, offset = 0 , retIDx =0;

    for(i = 0; i < NO_OF_PSID_PERMISSION; i++){
        psidPermission.WSApsid[i] = getPsidbyLen(&cert->slotCert.certInfo[cert->slotCert.psid_array_permOffset + offset],&retIDx,NULL);
        offset += retIDx;
        psidPermission.WSApriority[i] = cert->slotCert.certInfo[cert->slotCert.psid_array_permOffset + offset];
        offset += 1;
        psidPermission.WSAsspString[i] = cert->slotCert.certInfo[cert->slotCert.psid_array_permOffset + offset];
        offset += 1;
    }
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,(UINT8 *)psidPermission.WSApsid,NO_OF_PSID_PERMISSION,"##### %s : %d : psidPermission.WSApsid: #####\n",__func__,__LINE__);
        AWSEC_LOG(ASM,(UINT8 *)psidPermission.WSApriority,NO_OF_PSID_PERMISSION,"##### %s : %d : psidPermission.WSApriority: #####\n",__func__,__LINE__);
        AWSEC_LOG(ASM,(UINT8 *)psidPermission.WSAsspString,NO_OF_PSID_PERMISSION,"##### %s : %d : psidPermission.WSAsspString: #####\n",__func__,__LINE__);
    }
    return 0;
}


int get_asm_logfilename()
{
   
  strcpy(asmlogfilename,serverPath);
  strcat(asmlogfilename,"/");
  strcat(asmlogfilename,asmoptions.LOG_FILE_NAME);
  return 0;
}

int ASMLOG(uint8_t flag,char * buf,uint8_t *data, uint32_t len)
{
    time_t nowtime;
    char *str;
    char timestr[18]={0};
    FILE * asmlogfile;
    char tmp_str[256];
    struct stat sbuf;
    if((asmoptions.LOG_OUTPUT_STREAM == 2) && flag)
    {
pthread_mutex_lock( &AsmLogLock );
    asmlogfile=fopen(asmlogfilename, "a+");
        time(&nowtime);
        str=ctime(&nowtime);
        memcpy(timestr,str+4,16);
        fprintf(asmlogfile,"%s: %s",timestr,buf);
	
	if(data!=NULL && len != 0){
		int i=0;
        	fprintf(asmlogfile," Data: ",buf);
		for(i=0; i<len; i++)
        		fprintf(asmlogfile,"%02x ",data[i]);
        	fprintf(asmlogfile,"\n");
	}
		
        fflush(asmlogfile);
    fclose(asmlogfile);
    if (stat(asmlogfilename, &sbuf) == -1) {
        perror("stat");
	return -1;
    }
    if(sbuf.st_size >= asmoptions.LOG_FILE_SIZE){
        bzero(tmp_str,256);
        sprintf(tmp_str,"rm -f %s",asmlogfilename);
        system(tmp_str);
    }
pthread_mutex_unlock( &AsmLogLock );
      return 1;
    }
    return -1;
}


int AWSEC_LOG(uint8_t caller,uint8_t *data, uint32_t len,const char *fmt, ...)
{
    time_t nowtime;
    char *str;
    char timestr[18]={0};
    FILE *awsec_logfile;
    char tmp_str[256];
    struct stat sbuf;
    if((caller == LCM) && (lcmoptions.LCMLogEnable))
    {
        va_list ap;
        pthread_mutex_lock( &LcmLogLock );
        awsec_logfile=fopen(lcmlogfilename, "a+");
        time(&nowtime);
        str=ctime(&nowtime);
        memcpy(timestr,str+4,16);
        fprintf(awsec_logfile,"%s: ",timestr);
	va_start(ap,fmt);
        vfprintf(awsec_logfile,fmt,ap);
	va_end(ap);
	
	if(data!=NULL && len != 0){
	    int i=0;
            fprintf(awsec_logfile," Data: ");
	    for(i=0; i<len; i++)
        	fprintf(awsec_logfile,"%02x ",data[i]);
            fprintf(awsec_logfile,"\n");
	}
        fflush(awsec_logfile);
        fclose(awsec_logfile);

        if (stat(lcmlogfilename, &sbuf) == -1) 
        {
            perror("stat");
            return -1;
        }

        if(sbuf.st_size >= LCM_LOG_SIZE)
        {
            bzero(tmp_str,256);
            sprintf(tmp_str,"rm -f %s",lcmlogfilename);
            system(tmp_str);
        }
        pthread_mutex_unlock( &LcmLogLock );
    }

    else if((caller == ASM) && (asmoptions.LOG_OUTPUT_STREAM == 2) ) 
    {
        va_list ap;
        pthread_mutex_lock( &AsmLogLock );
        awsec_logfile=fopen(asmlogfilename, "a+");
        time(&nowtime);
        str=ctime(&nowtime);
        memcpy(timestr,str+4,16);
        fprintf(awsec_logfile,"%s: ",timestr);
	va_start(ap,fmt);
        vfprintf(awsec_logfile,fmt,ap);
	va_end(ap);

	if(data!=NULL && len != 0){
	    int i=0;
            fprintf(awsec_logfile," Data: ");
	    for(i=0; i<len; i++)
         	fprintf(awsec_logfile,"%02x ",data[i]);
            fprintf(awsec_logfile,"\n");
	}
        fflush(awsec_logfile);
        fclose(awsec_logfile);

        if (stat(asmlogfilename, &sbuf) == -1) {
            perror("stat");
	    return -1;
        }

        if(sbuf.st_size >= asmoptions.LOG_FILE_SIZE){
            bzero(tmp_str,256);
            sprintf(tmp_str,"rm -f %s",asmlogfilename);
            system(tmp_str);
        }
        pthread_mutex_unlock( &AsmLogLock );
    }	
    else
	return 1;
    
    return -1;
}


int lcm_conf_parse(void)
{
    FILE *fdrd;
    char rdfile[100],mline[200];
    char *sts;
    char *token = NULL;
    int ret =0;
    strcpy(rdfile,LCM_CONF_FILE);
    fdrd = fopen(rdfile, "r");
    struct in6_addr tmp_addr;
    if (fdrd <=0)
    {
        syslog(LOG_INFO,"Error opening %s file\n", rdfile);
	exit(-1);
    }

    memset(mline, 0, sizeof(mline));

    while( (sts = fgets(mline, sizeof(mline), fdrd)) != NULL ) {

        if (mline[0] != '#' && mline[0] != ';' && mline[0] != ' ') {
            token = strtok(mline, "=");
            if (strcasecmp(token, "RA_ADDRESS") == 0) {
                token = strtok(NULL, " ");
                sscanf(token, "%s",lcmoptions.RA_ADDRESS);
//                printf("RA_ADDRESS is: %s\n",lcmoptions.RA_ADDRESS);
                 ret = inet_pton(AF_INET,lcmoptions.RA_ADDRESS, &tmp_addr );
                if( ret == 1 )
                    iptype = 4;
                else
                {
                    ret = inet_pton(AF_INET6, lcmoptions.RA_ADDRESS,&tmp_addr );
                    if( ret == 1 )
                        iptype = 6;
                }
  //              printf("iptype =%d\n",iptype);
            }

            else if (strcasecmp(token, "RA_PORT") == 0) {
                token = strtok(NULL, " ");
  //              printf("RA port token is :%s \n",token);
                lcmoptions.RA_PORT = atoi(token);
            }

            else if (strcasecmp(token,"PSID") == 0) {
                token = strtok(NULL, " ");
                lcmoptions.PSID = atoi(token);
  //              printf("PSID is :%d \n",lcmoptions.PSID);
            }

            else if (strcasecmp(token,"Storage_Space") == 0) {
                token = strtok(NULL, " ");
                lcmoptions.Storage_Space = atoi(token);
  //              printf("Storage space :%d \n",lcmoptions.Storage_Space);
            }

            else if (strcasecmp(token,"Bootstrap_Request_Timeout") == 0) {
                token = strtok(NULL, " ");
                lcmoptions.Bootstrap_Request_Timeout = atoi(token);
  //              printf("Bootstrap_Request_Timeout :%d \n",lcmoptions.Bootstrap_Request_Timeout);
            }

            else if (strcasecmp(token,"Device_Specific_ID") == 0) {
                token = strtok(NULL, " ");
                sscanf(token, "%s",lcmoptions.Device_Specific_ID);
  //              printf("Device_Specific_ID :%s \n",lcmoptions.Device_Specific_ID);
            }

            else if (strcasecmp(token,"Batch_Duration_Units") == 0) {
                token = strtok(NULL, " ");
                lcmoptions.Batch_Duration_Units = atoi(token);
  //              printf("Batch_Duration_Units :%d \n",lcmoptions.Batch_Duration_Units);
            }

            else if (strcasecmp(token,"Batch_Duration_Value") == 0) {
                token = strtok(NULL, " ");
                lcmoptions.Batch_Duration_Value = atoi(token);
  //              printf("Batch_Duration_Value :%d \n",lcmoptions.Batch_Duration_Value);
            }
            else if (strcasecmp(token,"Certificate_Request_Status_Inquiry_Interval") == 0) {
                token = strtok(NULL, " ");
                lcmoptions.Certificate_Request_Status_Inquiry_Interval = atoi(token);
  //              printf("Certificate_Request_Status_Inquiry_Interval :%d \n",lcmoptions.Certificate_Request_Status_Inquiry_Interval);
            }

            else if (strcasecmp(token,"Certificate_Request_Confirmation_Timeout") == 0) {
                token = strtok(NULL, " ");
                lcmoptions.Certificate_Request_Confirmation_Timeout = atoi(token);
  //              printf("Certificate_Request_Confirmation_Timeout :%d \n",lcmoptions.Certificate_Request_Confirmation_Timeout);
            }

            else if(strcasecmp(token,"Decryption_Key_Request_Interval")==0) {
                token = strtok(NULL," ");
                lcmoptions.Decryption_Key_Request_Interval = atoi(token);
  //              printf("Decryption_Key_Request_Interval :%d \n",lcmoptions.Decryption_Key_Request_Interval);
            }

            else if(strcasecmp(token,"Maximum_Certificate_Storage_Time")==0) {
                token = strtok(NULL," ");
                lcmoptions.Maximum_Certificate_Storage_Time = atoi(token);
  //              printf("Maximum_Certificate_Storage_Time: %d \n",lcmoptions.Maximum_Certificate_Storage_Time);
            }

            else if(strcasecmp(token,"Request_Certificates_Time")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Request_Certificates_Time = atoi(token);
            }

            else if(strcasecmp(token,"Request_Decryption_Key_Time")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Request_Decryption_Key_Time = atoi(token);
  //              printf("Request_Decryption_Key_Time: %d \n",lcmoptions.Request_Decryption_Key_Time);
            }

            else if(strcasecmp(token,"LCM_NAME")==0) {
                token = strtok(NULL, " ");
                sscanf(token, "%s",lcmoptions.LCM_NAME);
   //             printf("LCM_NAME :%s \n",lcmoptions.LCM_NAME);
            }

            else if(strcasecmp(token,"Connection_Retry_Interval")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Connection_Retry_Interval = atoi(token);
  //              printf("Connection_Retry_Interval :%d \n",lcmoptions.Connection_Retry_Interval);
            }

            else if(strcasecmp(token,"LCMLogEnable")==0) {
                token = strtok(NULL, " ");
                lcmoptions.LCMLogEnable = atoi(token);
   //             printf("LCMLogEnable :%d \n",lcmoptions.LCMLogEnable);
            }
            if(strcasecmp(token,"LogFileDirectory")==0) {
                token = strtok(NULL, " ");
                sscanf(token, "%s",lcmoptions.LogFileDirectory);
   //             printf("LogFileDirectory :%s \n",lcmoptions.LogFileDirectory);
            }

            else if(strcasecmp(token,"LogUseSimpleName")==0) {
                token = strtok(NULL, " ");
                sscanf(token,"%s",lcmoptions.LogUseSimpleName);
   //             printf("LogUseSimpleName :%s \n",lcmoptions.LogUseSimpleName);
            }
            else if(strcasecmp(token,"LogEnableAdditionalInfo")==0) {
                token = strtok(NULL, " ");
                //sscanf(token,"%s",lcmoptions.LogEnableAdditionalInfo);
                lcmoptions.LogEnableAdditionalInfo = atoi(token); 
		if(lcmoptions.LogEnableAdditionalInfo)
		    lcmoptions.totalLogFlag |= 0x7800; //totalLogFlag reflect Bit Postions: CRL_CFM,CRL_REQ,MISBEHAVIOURREPORT_ACK,MISBEHAVIOURREPORT_REQ,DECRYPTIONKEY_ACK,DECRYPTIONKEY_CFM,DECRYPTIONKEY_REQ,ANONCERTRESP_ACK,ANONCERTREQ_STS_CFM,ANONCERTREQ_STS_REQ,ANONCERTREQ_CFM,ANONCERTREQ_REQ,BOOTSTRAP_ACK,BOOTSTRAP_CFM,BOOTSTRAP_REQ=0,
  //              printf("LogEnableAdditionalInfo :%d \n",lcmoptions.LogEnableAdditionalInfo);
            }

            else if(strcasecmp(token,"Log_Bootstrap_Request")==0) {
                token = strtok(NULL, " ");
//                sscanf(token,"%s",lcmoptions.Log_Bootstrap_Request);
                lcmoptions.Log_Bootstrap_Request = atoi(token);
		if(lcmoptions.Log_Bootstrap_Request)
		    lcmoptions.totalLogFlag |= 1;
  //              printf("Log_Bootstrap_Request :%d \n",lcmoptions.Log_Bootstrap_Request);
            }

            else if(strcasecmp(token,"Log_Bootstrap_Confirm")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Log_Bootstrap_Confirm = atoi(token);
		if(lcmoptions.Log_Bootstrap_Confirm)
		    lcmoptions.totalLogFlag |= (1<<1);
  //              printf("Log_Bootstrap_Confirm :%d \n",lcmoptions.Log_Bootstrap_Confirm);
            }

            else if(strcasecmp(token,"Log_Bootstrap_Ack")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Log_Bootstrap_Ack = atoi(token);
		if(lcmoptions.Log_Bootstrap_Ack)
		    lcmoptions.totalLogFlag |= (1<<2);
  //              printf("Log_Bootstrap_Ack :%d \n",lcmoptions.Log_Bootstrap_Ack);
            }

            else if(strcasecmp(token,"Log_CertRequest_Req")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Log_CertRequest_Req = atoi(token);
		if(lcmoptions.Log_CertRequest_Req)
		    lcmoptions.totalLogFlag |= (1<<3);
   //             printf("Log_CertRequest_Req :%d \n",lcmoptions.Log_CertRequest_Req);
            }

            else if(strcasecmp(token,"Log_CertRequest_Confirm")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Log_CertRequest_Confirm = atoi(token);
		if(lcmoptions.Log_CertRequest_Confirm)
		    lcmoptions.totalLogFlag |= (1<<4);
   //             printf("Log_CertRequest_Confirm :%d \n",lcmoptions.Log_CertRequest_Confirm);
            }

            else if(strcasecmp(token,"Log_CertStatus_Confirm")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Log_CertStatus_Confirm = atoi(token);
		if(lcmoptions.Log_CertStatus_Confirm)
		    lcmoptions.totalLogFlag |= (1<<6);
   //             printf("Log_CertStatus_Confirm :%d \n",lcmoptions.Log_CertStatus_Confirm);
            }
	    else if(strcasecmp(token,"Log_CertStatus_Confirm_Data")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Log_CertStatus_Confirm_Data = atoi(token);
                if(lcmoptions.Log_CertStatus_Confirm_Data)
                    lcmoptions.totalLogFlag |= (1<<6);//master flag is stage flag i.e CertStatus_Confirm
    //            printf("Log_CertStatus_Confirm_Data :%d \n",lcmoptions.Log_CertStatus_Confirm_Data);
            }

            else if(strcasecmp(token,"Log_CertStatus_Req")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Log_CertStatus_Req = atoi(token);
		if(lcmoptions.Log_CertStatus_Req)
		    lcmoptions.totalLogFlag |= (1<<5);
    //            printf("Log_CertStatus_Req :%d \n",lcmoptions.Log_CertStatus_Req);
            }
            
            else if(strcasecmp(token,"Log_CertResponse_Ack")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Log_CertResponse_Ack = atoi(token);
		if(lcmoptions.Log_CertResponse_Ack)
		    lcmoptions.totalLogFlag |= (1<<7);
     //           printf("Log_CertResponse_Ack :%d \n",lcmoptions.Log_CertResponse_Ack);
            }

            else if(strcasecmp(token,"Log_DecryptKey_Request")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Log_DecryptKey_Request = atoi(token);
		if(lcmoptions.Log_DecryptKey_Request)
		    lcmoptions.totalLogFlag |= (1<<8);
   //             printf("Log_DecryptKey_Request :%d \n",lcmoptions.Log_DecryptKey_Request);
            }

            else if(strcasecmp(token,"Log_DecryptKey_Confirm")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Log_DecryptKey_Confirm = atoi(token);
		if(lcmoptions.Log_DecryptKey_Confirm)
		    lcmoptions.totalLogFlag |= (1<<9);
   //             printf("Log_DecryptKey_Confirm :%d \n",lcmoptions.Log_DecryptKey_Confirm);
            }

            else if(strcasecmp(token,"Log_DecryptKey_Ack")==0) {
                token = strtok(NULL, " ");
                lcmoptions.Log_DecryptKey_Ack = atoi(token);
		if(lcmoptions.Log_DecryptKey_Ack)
		    lcmoptions.totalLogFlag |= (1<<10);
   //             printf("Log_DecryptKey_Ack :%d \n",lcmoptions.Log_DecryptKey_Ack);
            }

            else if(strcasecmp(token,"Log_SignEncrypt_Input")==0) {
                token = strtok(NULL, " ");
                //sscanf(token,"%s",lcmoptions.Log_SignEncrypt_Input);
                lcmoptions.Log_SignEncrypt_Input = atoi(token);
   //             printf("Log_SignEncrypt_Input :%d \n",lcmoptions.Log_SignEncrypt_Input);
            }

            else if(strcasecmp(token,"Log_SignEncrypt_Before_Encrypt")==0) {
                token = strtok(NULL, " ");
                //sscanf(token,"%s",lcmoptions.Log_SignEncrypt_Before_Encrypt);
                lcmoptions.Log_SignEncrypt_Before_Encrypt = atoi(token);
   //             printf("Log_SignEncrypt_Before_Encrypt :%d \n",lcmoptions.Log_SignEncrypt_Before_Encrypt);
            }

            else if(strcasecmp(token,"Log_SignEncrypt_After_Encrypt")==0) {
                token = strtok(NULL, " ");
                //sscanf(token,"%s",lcmoptions.Log_SignEncrypt_After_Encrypt);
                lcmoptions.Log_SignEncrypt_After_Encrypt = atoi(token);
   //             printf("Log_SignEncrypt_After_Encrypt :%d \n",lcmoptions.Log_SignEncrypt_After_Encrypt);
            }

            else if(strcasecmp(token,"default_Timeout")==0) {
                token = strtok(NULL, " ");
                //sscanf(token,"%s",lcmoptions.Log_Imported_File);
                lcmoptions.Default_Timeout = atoi(token);
   //             printf("default_Timeout :%d \n",lcmoptions.Default_Timeout);
            }
            else if(strcasecmp(token,"Log_Imported_File")==0) {
                token = strtok(NULL, " ");
                //sscanf(token,"%s",lcmoptions.Log_Imported_File);
                lcmoptions.Log_Imported_File = atoi(token);
  //              printf("Log_Imported_File :%d \n",lcmoptions.Log_Imported_File);
            }
            else if(strcasecmp(token,"DecryptKey_Retry")==0) {
                token = strtok(NULL, " ");
                //sscanf(token,"%s",lcmoptions.Log_Imported_File);
                lcmoptions.DecryptKey_Retry = atoi(token);
   //             printf("DecryptKey_Retry :%d \n",lcmoptions.DecryptKey_Retry);
            }
            else if(strcasecmp(token,"Superbatch_Duration")==0) {
                token = strtok(NULL, " ");
                //sscanf(token,"%s",lcmoptions.Log_Imported_File);
                lcmoptions.Superbatch_Duration = atoi(token);
   //             printf("Superbatch_Duration :%d \n",lcmoptions.Superbatch_Duration);
            }
            else if(strcasecmp(token,"Superbatch_Duration_units")==0) {
                token = strtok(NULL, " ");
                //sscanf(token,"%s",lcmoptions.Log_Imported_File);
                lcmoptions.Superbatch_Duration_units = atoi(token);
   //             printf("Superbatch_Duration_units :%d \n",lcmoptions.Superbatch_Duration_units);
            }
            else if(strcasecmp(token,"LcmDebugFile")==0) {
                token = strtok(NULL, " \n\r");
                sscanf(token, "%s",lcmoptions.Lcm_DebugFile);
  //              printf("Lcm_debug :%s \n",lcmoptions.Lcm_DebugFile);
            }
            else if(strcasecmp(token,"Enable_Crl_Processing")==0) {
                token = strtok(NULL, " \n\r");
		lcmoptions.crlEnable = atoi(token);
 //               printf("CrlEnable :%d \n",lcmoptions.crlEnable);
            }
        }

        memset(mline, 0, sizeof(mline));

    }
    fclose(fdrd);
//                printf("LogFileDirectory :%s \n",lcmoptions.LogFileDirectory);
 //               printf("totalLogFlag :0x%x \n",lcmoptions.totalLogFlag);
    return 1;
}

int get_lcm_logfilename()
{
    strcpy(lcmlogfilename,lcmoptions.LogFileDirectory);
    strcat(lcmlogfilename,"/");
    strcat(lcmlogfilename,lcmoptions.LogUseSimpleName);
    return 0;
}

int LCM_LOG(uint8_t flag,char * buf,uint8_t *data, uint32_t len)
{
    time_t nowtime;
    char *str;
    char timestr[18]={0};
    FILE * lcm_logfile;
    char tmp_str[256];
    struct stat sbuf;
    if( lcmoptions.LCMLogEnable && flag)
    {
pthread_mutex_lock( &LcmLogLock );
    lcm_logfile=fopen(lcmlogfilename, "a+");
        time(&nowtime);
        str=ctime(&nowtime);
        memcpy(timestr,str+4,16);
        fprintf(lcm_logfile,"%s: %s",timestr,buf);
	
	if(data!=NULL && len != 0){
		int i=0;
        	fprintf(lcm_logfile," Data: ",buf);
		for(i=0; i<len; i++)
        		fprintf(lcm_logfile,"%02x ",data[i]);
        	fprintf(lcm_logfile,"\n");
	}
		
        fflush(lcm_logfile);
    fclose(lcm_logfile);
    if (stat(lcmlogfilename, &sbuf) == -1) {
        perror("stat");
        return -1;
    }
    if(sbuf.st_size >= LCM_LOG_SIZE){
        bzero(tmp_str,256);
        sprintf(tmp_str,"rm -f %s",lcmlogfilename);
        system(tmp_str);
    }

pthread_mutex_unlock( &LcmLogLock );
        return 1;
    }
    return -1;
}

#ifdef NAZEER
uint8_t hashCert(char *str) 
{
  uint16_t sum = 0;
  uint8_t i;
  for (i = 0;i < 8; i++)  
  {
    sum += str[i];
  }
  return sum % HASHSIZE;
}

void listCertInfo(void)
{
  struct CertInfo* ntry;
  int hash, i = 0;

  for (hash = 0; hash < HASHSIZE; hash++) 
  {
    LIST_FOREACH(ntry, &TrustCert.st_hash[hash], si_hash) 
    {
      printf("\nhashIndex = %d\n" , ntry->hashIndex);
      printf("\nhash = %0x\n" , hash);
      if (ntry->CertHash != NULL)
      {
      printf("\nSigner ID: ");
      for(i = 0; i < 8; i++)
        printf("0x%02x, ", ntry->SignerID[i]);
      printf("\n");
      }
      printf("\nCertificate Hash: ");
      for(i = 0; i < 8; i++)
        printf("0x%02x, ", ntry->CertHash[i]);
      printf("\n");
    }
  }
}

struct CertInfo* findCertInfo(char *pData, char *pIssuer) 
{
  struct CertInfo *ntry;
  uint8_t hash;

  hash = hashCert(pIssuer);
  if(asm_log_level >= LOG_DEBUG){
    AWSEC_LOG(ASM,NULL,0,"%s: hash = %x\n", __FUNCTION__, hash);
  }
  LIST_FOREACH(ntry, &TrustCert.st_hash[hash], si_hash) 
  {
    if (!(memcmp(ntry->CertHash, pData, 8))) 
    {
      return ntry;
    }
  }
  return NULL;
}

struct CertInfo* addCertInfo(char *pData, char *pIssuer)
{	
  struct CertInfo *ntry;
  uint8_t hash;

  ntry = (struct CertInfo *) malloc( sizeof( struct CertInfo));
  if (ntry == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"Unable to allocate memory for CertInfo structure\n");
    return NULL;
  }
  else
  {
    //memcpy(ntry->SignerID, pIssuer, 8);
    memcpy(ntry->CertHash, pData, 8);
  }
  hash = hashCert(pIssuer);
  TAILQ_INSERT_TAIL(&TrustCert.st_cert, ntry, si_list);
  LIST_INSERT_HEAD(&TrustCert.st_hash[hash], ntry, si_hash);
  return ntry;
}

uint32_t delCertInfo(char *pData, char *pIssuer)
{
  struct CertInfo *ntry;
  uint8_t hash;

  hash = hashCert(pIssuer);
  if(asm_log_level >= LOG_DEBUG){
    AWSEC_LOG(ASM,NULL,0,"%s: hash = %x\n", __FUNCTION__, hash);
  }
  LIST_FOREACH(ntry, &TrustCert.st_hash[hash], si_hash)
  {
    if (!(memcmp(ntry->CertHash, pData, 8)))
    {
      LIST_REMOVE(ntry, si_hash);
      free(ntry);
      return SUCCESS;
    }
  }
  return FAILURE;
}
#endif

/* ----------------------  moved from bootstrap.c ----------------------------------*/

int get_date_yyyymmdd(int Tdate)
{
    char *token = NULL;
    char *str = NULL;
    char *temp = NULL;
    int ret_date = 0;
    time_t date;
    int i = 0, month_num = 0, day =0, year = 0;
    char mon[12][4]={ {"Jan\0"},
                      {"Feb\0"},
                      {"Mar\0"},
                      {"Apr\0"},
                      {"May\0"},
                      {"Jun\0"},
                      {"Jul\0"},
                      {"Aug\0"},
                      {"Sep\0"},
                      {"Oct\0"},
                      {"Nov\0"},
                      {"Dec\0"}
                    };
    date = Tdate;
    str = ctime(&date);
    temp = (char *)malloc(10);
    token = strtok(str," ");                //week_day
    token = strtok(NULL," ");               //month
    sscanf(token,"%s",temp);
    for(i=1; i <= 12; i++){
        if(!strcmp(mon[i-1],temp))
            month_num = i;
    }

    token = strtok(NULL," ");               //date
    sscanf(token,"%s",temp);
    day = atoi(temp);
    token = strtok(NULL,":");               //hour
    token = strtok(NULL,":");               //min
    token = strtok(NULL," ");               //sec
    token = strtok(NULL," ");               //year
    sscanf(token,"%s",temp);
    year = atoi(temp);
    ret_date = (year * 10000) + (month_num * 100) + (day)  ;
    free(temp);
    return ret_date;
}

int deleteExpiredCertFrmUsb(int today_epoch)
{
    int cur_date = 0, rmvSts = -1;
    int timeunits = today_epoch - DAY_SEC;
    char cert_name[200] = "";
    do {
        cur_date = get_date_yyyymmdd(timeunits);
        sprintf(cert_name,"/tmp/usb/lcm/Certificates/ShortLived%d.crt", cur_date);
        rmvSts = remove(cert_name);
        timeunits = timeunits - DAY_SEC;
        } while(rmvSts == 0);

    return 0;
}


Time32 get_cur_time2004(void)
{
    struct timeval tv;
    gettimeofday(&tv, NULL);
    return(tv.tv_sec - REFERENCE_TIME);
}
static Time64 get_base_time(void)
{
    UINT64 b_time = 0;
    struct tm b_time_info;
    b_time_info.tm_year = 2004 - 1900;
    b_time_info.tm_mon  = 1 - 1;
    b_time_info.tm_mday = 1;
    b_time_info.tm_hour = 0;
    b_time_info.tm_min = 0;
    b_time_info.tm_sec = 0;
    b_time_info.tm_isdst = -1;
    b_time = timegm(&b_time_info);
    b_time = b_time * 1000 * 1000;
    return b_time;
}

Time64 generate_current_time(void)
{
    struct timeval tv;
    Time64 base_time = 0;
    Time64 time_us = 0;
    if (0 != gettimeofday(&tv, NULL)) {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"gettimeofday() failed\n");
        exit(1);
    }
    time_us = tv.tv_sec;
    time_us = time_us * 1000 * 1000;
    time_us += tv.tv_usec;
    if (base_time == 0) {
        base_time = get_base_time();
    }
    return time_us - base_time - NUMBER_OF_LEAP_MICROSECONDS_TODAY;
}

int validate_expiry_time(Time64 exp_time)
{
    Time64 current_time;
    current_time = generate_current_time();
    if(exp_time >= current_time)
	return 1; 

    return 0;
}	
uint8_t validate_generation_time(Time64 gen_time,Time64 validity)
{
    Time64 current_time;
    Time64 endtime = gen_time + validity;
    current_time = generate_current_time();
    if(asm_log_level >= LOG_DEBUG)
	AWSEC_LOG(ASM,NULL,0,"%s,times->Generation:%llu Current:%llu End:%llu Validity:%llu\n",__func__,gen_time,current_time,endtime,validity);    
    if((gen_time <= current_time) && (current_time <= endtime))
	return 1;//verified success
    else if(current_time < gen_time){
	return 2;//0x59
    }
    else if(endtime < current_time)
	return 3;//0x37
    else
	return 0;//failed
}

void epoch_to_time(char *str, char *file_name, char *path)
{
    char *token = NULL;
    char *string = str;
    int i = 0;
    int start_month = 0, start_day = 0, start_year = 0, start_hour = 0;
    char mon[12][4]={ {"Jan\0"},
                      {"Feb\0"},
                      {"Mar\0"},
                      {"Apr\0"},
                      {"May\0"},
                      {"Jun\0"},
                      {"Jul\0"},
                      {"Aug\0"},
                      {"Sep\0"},
                      {"Oct\0"},
                      {"Nov\0"},
                      {"Dec\0"}
                    };



    token = (char *)strtok(string, " ");	//week-day

    token = (char *)strtok(NULL, " ");		//month
    for(i=1; i <= 12; i++)
    {
    	if(!strcmp(mon[i-1],token)) {
       	    start_month = i;
	}
    }

    token = (char *)strtok(NULL, " ");		//date
    start_day = atoi(token);

    token = (char *)strtok(NULL, ":");		//hour
    start_hour = atoi(token);

    token = (char *)strtok(NULL, ":");		//min

    token = (char *)strtok(NULL, " ");		//sec

    token = (char *)strtok(NULL, " ");		//year
    start_year = atoi(token);

    sprintf(file_name,"%sShortLived%04d%02d%02d.crt", path,start_year, start_month, start_day);
    return;
}

uint32_t convertTo_seconds(uint16_t units, uint16_t duration)
{
    uint32_t secs = 0;

    if(units == 0)
        secs = duration; //secs
    if(units == 1)
        secs = duration * 60; //min
    if(units == 2)
        secs = duration * NUMSECS_PERHOUR; //hours
    if(units == 3)
        secs = duration * 60 * NUMSECS_PERHOUR; //60hrs
    if(units == 4)
        secs = duration * 31536000; //years

    return secs;
}

uint32_t epoch_time_from_2004(Time32 *start, Time32 *end, CertificateDuration units, CertificateDuration SBduration)
{
    Time32 tmp_time=0,retTime=0,secs=0;
	    
    putenv("TZ=UTC");
    
    if(units == 0)
	secs = SBduration; //secs
    if(units == 1)
	secs = SBduration * 60; //min
    if(units == 2)
	secs = SBduration * NUMSECS_PERHOUR; //hours
    if(units == 3)
	secs = SBduration * 60 * NUMSECS_PERHOUR; //60hrs
    if(units == 4)
	secs = SBduration * 31536000; //years

	tmp_time = get_cur_time2004();
	if(tmp_time < lcmStore.endReqTime)
	    tmp_time = lcmStore.endReqTime + 1;
	retTime = swap_32(tmp_time); //later should be changed to htobe32()
	memcpy(start, &retTime, 4);
	tmp_time +=  secs;
		retTime = swap_32(tmp_time); //later should be changed to htobe32()
	memcpy(end, &retTime, 4);
    return 0;
}

int32_t decode_length(uint8_t *addr , int32_t *retIdx)
{
   int32_t recv_size = 0;

   if ((addr[0] & 0x80) == 0x00) {
       recv_size = addr[0];
       *retIdx = 1;
   }
   else if ((addr[0] & 0xc0) == 0x80) {
       recv_size = (addr[0] << 8)|(addr[1]);
	   recv_size = recv_size & 0x3fff;
       *retIdx = 2;
   }
   else if ((addr[0] & 0xe0) == 0xc0) {
       recv_size = (addr[0] << 16)|(addr[1] << 8)|(addr[2]);
	   recv_size = recv_size & 0x1fffff;
       *retIdx = 3;
   }
   else if ((addr[0] & 0xf0) == 0xe0) {
       recv_size = (addr[0] << 24)|(addr[1] << 16)|(addr[2] << 8)|(addr[3]);
	   recv_size = recv_size & 0x0fffffff;
       *retIdx = 4;
   }
   return recv_size;
}

/* addr: buffer in which encoded length will be copied
 * val: 32bit length value
 * retIdx: no.of bytes to be incremented for buffer index
 */
int32_t encode_length(uint8_t *addr ,int32_t val , int32_t *retIdx)
{
   int32_t retval = 0;
   uint8_t *p = NULL;
    retval =swap_32(val); //later should be changed to htobe32()
    p=(uint8_t *)&retval;
   if (val <= 0x7F) {
       *retIdx = 1;
	   addr[0] = p[3];
   }
   else if (val >= 0x80 && val <= 0x3FFF) {
       *retIdx = 2;
	   addr[0] = p[2] | 0x80;
	   addr[1] = p[3];
   }
   else if (val >= 0x4000 && val <= 0x1FFFFF) {
       *retIdx = 3;
	   addr[0] = p[1] | 0xd0;
	   addr[1] = p[2];
	   addr[2] = p[3];
   }
   else if (val >= 0x200000 && val <= 0xFFFFFFF) {
       *retIdx = 4;
	   addr[0] = p[0] | 0xe0;
	   addr[1] = p[1];
	   addr[2] = p[2];
	   addr[3] = p[3];
   }
}

int32_t get_file_name(char *file,char *retfile)
{
    FILE *fp=NULL;
    char tmp_str[256];
    char tmp_buff[150];

    sprintf(tmp_buff,"ls %s 2> /dev/null",file);
    fp = NULL;
    fp = popen(tmp_buff,"r");
    if(fp != NULL){
        fscanf(fp,"%s",retfile);
        pclose(fp);
//        printf("%d-%s:%s found \n",__LINE__,__func__,retfile); keep this print in caller
    }
    return 0;
}

int32_t create_certFile(uint8_t *buf,  uint32_t len, char *file_name)
{
    char tmp[8];
    int32_t i=0;
    int fd = -1, wr_sts = -1; 

    fd = open(file_name, O_WRONLY | O_CREAT | O_APPEND, S_IRWXU | S_IRWXO | S_IRWXG);    
    if(fd < 0)
	return -1;

    if(fd > 0)
    {
    	while (i < len){
    	    sprintf(tmp, "%02x ", buf[i]);
	    wr_sts = write(fd, tmp, 3);
	    if(wr_sts < 0) {
		close(fd);
		return -1;
	    }
	    i++;
    	}
    }
    close(fd);
    return 0;
}

int32_t mapFile(char *toFile, uint8_t **data, uint8_t closeFlag, uint8_t rmFlag)
{
	int fd;
    	struct stat sbuf;
	char logstring[255];
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"mapfile close %d rm %d\n", closeFlag, rmFlag);
        }
	if(!closeFlag){
                if(asm_log_level >= LOG_INF){
                    AWSEC_LOG(ASM,NULL,0,"mapfile1: %p\n",*data);
                }
 		fd = open(toFile, O_RDWR,(mode_t)0777);
 		if (fd == -1) {
        		perror("open");
		        return -1;
    		}
    		if (stat(toFile, &sbuf) == -1) {
        		perror("stat");
		        return -1;
    		}
	    	*data = (uint8_t *)mmap(0, sbuf.st_size, PROT_READ| PROT_WRITE, MAP_SHARED, fd, 0);
    		if (data == MAP_FAILED) {
        		perror("mmap");
		        return -1;
    		}
                if(asm_log_level >= LOG_INF){
                    AWSEC_LOG(ASM,NULL,0,"mapfile: %p\n",*data);
                }
	        close(fd);
		return sbuf.st_size;
	}
	else if(closeFlag){
    		if (stat(toFile, &sbuf) == -1) {
        		perror("stat");
		        return -1;
    		}
                if(asm_log_level >= LOG_INF){
                    AWSEC_LOG(ASM,NULL,0,"mapfile3: %p\n",*data);
                }
		if (munmap (*data, sbuf.st_size) == -1) {
		    perror ("munmap");
		    return -1;
		}
		if(rmFlag){
	            sprintf(logstring,"rm -rf %s",toFile);
	            system(logstring);
		}
	}
	return 0;
}


int32_t createZeroFile(char *toFile, int32_t size)
{
	char cmd[255];
	sprintf(cmd,"dd if=/dev/zero of=%s bs=%d count=1",toFile,size);
	system(cmd);
	return 0;
}

int32_t certParse(uint8_t *cert,Certificate * parsedcert)
{
    int32_t certparseoffset =0;
    int32_t ll =0, len;
    int32_t scope_namelen=0;
    int16_t permitted_subject_type=0;
    int32_t psid_array_permissionslen=0;
    uint8_t psid_array_type=0;
    uint8_t geographic_region_type=0;
    int16_t polylen=0;
    int16_t crllen=0;
    uint8_t version_and_type =cert[0];
    uint8_t subject_type =cert[1];
    uint8_t cf =cert[2];
    int i=0;
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"%s: %d\n", __FUNCTION__, __LINE__);
    }
    parsedcert->version_and_type =version_and_type;
    parsedcert->subject_type  =subject_type;
    parsedcert->cf  =cf;
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"##### %s : %d : version %d : Subject Type %d #####\n",__func__,__LINE__,version_and_type,subject_type);
        AWSEC_LOG(ASM,NULL,0,"##### %s : %d parsedcert->cf %d #####\n",__func__,__LINE__,parsedcert->cf);
    }
    certparseoffset +=3;
    if(asm_log_level >= LOG_DEBUG)
        AWSEC_LOG(ASM,NULL,0,"%s: %d\n", __FUNCTION__, __LINE__);
    
    if(subject_type != root_ca)
    {
        parsedcert->signer_id= certparseoffset; //signer_id
        if(asm_log_level >= LOG_DEBUG){
            AWSEC_LOG(ASM,&cert[certparseoffset],DIGESTLEN,"***** Start Of Signer ID *****");
	    AWSEC_LOG(ASM,NULL,0,"*****  End Of Signer ID  ******\n");
        }
        certparseoffset += DIGESTLEN;
        parsedcert->signature_alg = cert[certparseoffset]; //sig_alg
        if(asm_log_level >= LOG_DEBUG){
            AWSEC_LOG(ASM,NULL,0,"##### %s : %d parsedcert->signature_alg %d #####\n",__func__,__LINE__,parsedcert->signature_alg);
        }
        certparseoffset +=1;
    }
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"%s: %d\n", __FUNCTION__, __LINE__);
    }
//certspecificdata parsing
    if((subject_type != crl_signer) && (subject_type != message_anonymous)) {
        ll=0;
        scope_namelen = decode_length((cert + certparseoffset), &ll);
        parsedcert->scopelen = scope_namelen;
        if(asm_log_level >= LOG_DEBUG){
            AWSEC_LOG(ASM,NULL,0,"##### %s : %d parsedcert->scopelen %d #####\n",__func__,__LINE__,parsedcert->scopelen);
        }
        certparseoffset += ll;
        parsedcert->scope = certparseoffset;
        certparseoffset += scope_namelen;
        if(asm_log_level >= LOG_DEBUG){
            AWSEC_LOG(ASM,NULL,0,"%d - %d\n",ll,certparseoffset);
        }
    }	
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"subjecttype --%d\n",subject_type);
    }
    switch(subject_type)
    {

    case root_ca:
        ll=0;
	 //len = (int16_t)decode_length((cert + certparseoffset), &ll);
	 
        permitted_subject_type =(int16_t)decode_length((cert + certparseoffset), &ll);
        certparseoffset +=ll;
        //permitted_subject_type = cert[certparseoffset];
		
        //certparseoffset ++;
        if(permitted_subject_type & 0x024f) //parse properly later
        {
		psid_array_type =cert[certparseoffset]; //*(uint8_t *)(cert + certparseoffset);
            certparseoffset +=1; //psid_array_type
            if(psid_array_type ==specified) {
                ll=0;
                psid_array_permissionslen= decode_length((cert + certparseoffset), &ll);
                certparseoffset +=ll;
                parsedcert->psid_array_permLength = psid_array_permissionslen;
                parsedcert->psid_array_permOffset = certparseoffset;
                certparseoffset += psid_array_permissionslen;
            }
        }
        if(permitted_subject_type & 0x00b0) //parse properly later
        {
            psid_array_type =cert[certparseoffset]; //*(uint8_t *)(cert + certparseoffset);
            certparseoffset +=1; //psid_array_type
            if(psid_array_type ==specified) {
                ll=0;
                psid_array_permissionslen= decode_length((cert + certparseoffset), &ll);
                certparseoffset +=ll;
                parsedcert->psid_array_permLength = psid_array_permissionslen;
                parsedcert->psid_array_permOffset = certparseoffset;
                certparseoffset += psid_array_permissionslen;
            	}
        }
        //geographic_region
        geographic_region_type = cert[certparseoffset];//*(uint8_t*)(cert + certparseoffset);
        certparseoffset +=1;     //region_type
         
        switch(geographic_region_type)
        {
        case from_issuer_region:
        case none:
            parsedcert->geographic_length = 0;
            parsedcert->geographic_region = 0 ;
            break;
        case circle:
            parsedcert->geographic_region = certparseoffset ;
            parsedcert->geographic_length = 10;
            certparseoffset+= (sizeof(TwoDLocation) +sizeof(uint16_t));  //circularregionsize
            break;
        case rectangle:
        case polygon:
            ll=0;
            polylen= (int16_t)decode_length((cert + certparseoffset), &ll);
            parsedcert->geographic_length = polylen;
            certparseoffset+= ll;
            certparseoffset+= polylen;
            parsedcert->geographic_region = certparseoffset ;
            break;
        }
        break;
    case message_ca:
    case message_ra:
    case message_csr:
    case wsa_ca:
    case wsa_csr:
    case message_identified_not_localized:
    case message_identified_localized:
    case message_anonymous:
    case wsa:
        //subject_type
        if(subject_type == message_ca || subject_type == message_ra || subject_type == message_csr) {
            ll=0;
            permitted_subject_type =(int16_t)decode_length((cert + certparseoffset), &ll);
            if(asm_log_level >= LOG_DEBUG){
                AWSEC_LOG(ASM,NULL,0,"##### %s : %d permitted_subject_type %02x : ll %d #####\n",__func__,__LINE__,permitted_subject_type,ll);
            }
            certparseoffset +=ll;
            if(asm_log_level >= LOG_DEBUG){
                AWSEC_LOG(ASM,NULL,0,"%d - %d\n",ll,certparseoffset);
            }
        }
        //additional_data for message_anonymous
#if 1 
     if(subject_type == message_anonymous) {
            ll=0;
			len=0;
            len =(int16_t)decode_length((cert + certparseoffset), &ll);
            certparseoffset +=ll;
			parsedcert->ad_len = len;
			parsedcert->ad = certparseoffset;
            certparseoffset +=len;
            if(asm_log_level >= LOG_DEBUG){
                AWSEC_LOG(ASM,NULL,0,"****ll=%d - len=%d - ad_start=%d - ff=%d\n",ll,len,parsedcert->ad,certparseoffset);
            }
        }
#endif
        //psid-array
        psid_array_type =cert[certparseoffset];
        if(asm_log_level >= LOG_DEBUG){
            AWSEC_LOG(ASM,NULL,0,"##### %s : %d psid_array_type %d #####\n",__func__,__LINE__,psid_array_type);
        }
        certparseoffset +=1;     //psid_array_type
        if(psid_array_type ==specified) {
            ll=0;
            psid_array_permissionslen= decode_length((cert + certparseoffset), &ll);
            if(asm_log_level >= LOG_DEBUG){
                AWSEC_LOG(ASM,NULL,0,"##### %s : %d psid_array_permissionslen %d : ll %d #####\n",__func__,__LINE__,psid_array_permissionslen,ll);
            }
            certparseoffset +=ll;
            parsedcert->psid_array_permLength = psid_array_permissionslen;
            parsedcert->psid_array_permOffset = certparseoffset;
            if(asm_log_level >= LOG_DEBUG){
                AWSEC_LOG(ASM,&cert[certparseoffset],psid_array_permissionslen,"**** Start of PSID Array Permission ****\n");
                AWSEC_LOG(ASM,NULL,0,"****  End of PSID Array Permission  ****\n");
            }
            certparseoffset += psid_array_permissionslen;
            if(asm_log_level >= LOG_DEBUG){
                AWSEC_LOG(ASM,NULL,0,"%d - %d\n",ll,certparseoffset);
            }
        }
        if(subject_type != message_identified_not_localized)
        {
            //geographic_region
            geographic_region_type = cert[certparseoffset];
            if(asm_log_level >= LOG_DEBUG){
                AWSEC_LOG(ASM,NULL,0,"##### %s : %d geographic_region_type %d #####\n",__func__,__LINE__,geographic_region_type);
            }
            certparseoffset +=1; //region_type

            switch(geographic_region_type)
            {
            case from_issuer_region:
            case none:
                parsedcert->geographic_length = 0;
                parsedcert->geographic_region = 0 ;
                break;
            case circle:
                parsedcert->geographic_region = certparseoffset ;
                parsedcert->geographic_length = 10;
                certparseoffset+= (sizeof(TwoDLocation) +sizeof(uint16_t)); //circularregionsize
                break;
            case rectangle:
            case polygon:
                ll=0;
                polylen= (uint16_t)decode_length((cert + certparseoffset), &ll);
                parsedcert->geographic_length = polylen;
                certparseoffset+= ll;
                parsedcert->geographic_region = certparseoffset ;
                certparseoffset+= polylen;
                break;
            }
        }
        break;
    default: //default includes crl_signer
        ll=0;
        crllen= (int16_t)decode_length((cert + certparseoffset), &ll);
        if(asm_log_level >= LOG_DEBUG){
            AWSEC_LOG(ASM,NULL,0,"##### %s : %d crllen %d : ll %d #####\n",__func__,__LINE__,crllen,ll);
        }
        certparseoffset+= ll;
        if(asm_log_level >= LOG_DEBUG){
            AWSEC_LOG(ASM,&cert[certparseoffset],crllen,"***** Start Of Crl Signer *****");
            AWSEC_LOG(ASM,NULL,0,"*****  End Of Crl Signer  *****");
        }
        certparseoffset+= crllen;

        break;
    }
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"%d - %d\n",ll,certparseoffset);
    }
    parsedcert->expiration = certparseoffset; //expiration
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,&cert[certparseoffset],4,"***** Start Of Expiration Time *****\n");
        AWSEC_LOG(ASM,NULL,0,"*****  End Of Expiration Time  *****\n");
    }
    certparseoffset +=4; //sizeof Time32
	if(cf&0x01){
	    parsedcert->start_validity_or_lft = certparseoffset; //startvalidity_or_lft
	    if((cf&0x02)==2){
                if(asm_log_level >= LOG_DEBUG){
                    AWSEC_LOG(ASM,&cert[certparseoffset],2,"***** Start Of Start Validity *****\n");
                    AWSEC_LOG(ASM,NULL,0,"*****  End Of Start Validity  *****\n");
                }
    	        certparseoffset+=2; //sizeof Time32
            }
	    else{
                if(asm_log_level >= LOG_DEBUG){
                    AWSEC_LOG(ASM,&cert[certparseoffset],4,"***** Start Of Start Validity *****\n");
                    AWSEC_LOG(ASM,NULL,0,"*****  End Of Start Validity  *****\n");
                }
    		certparseoffset+=4; //sizeof Time32
            }
	}
        if(asm_log_level >= LOG_DEBUG){
            AWSEC_LOG(ASM,NULL,0,"%d - %d\n",ll,certparseoffset);
        }
    parsedcert->crl_series = certparseoffset; //startvalidity_or_lft
	if(asm_log_level >= LOG_DEBUG){
	    AWSEC_LOG(ASM,&cert[certparseoffset],4,"***** Start Of Crl Series *****\n");
	    AWSEC_LOG(ASM,NULL,0,"*****  End Of Crl Series  *****\n");
	}
    certparseoffset+=4; //sizeof Time32
    //if(subject_type == root_ca)
//	certparseoffset+=2;
        if(asm_log_level >= LOG_DEBUG){
            AWSEC_LOG(ASM,NULL,0,"%d - %d\n",ll,certparseoffset);
        }
    if(version_and_type == 2) { //assuming only case 2 or case 3
        if(asm_log_level >= LOG_DEBUG){
            AWSEC_LOG(ASM,NULL,0,"##### %s : %d PKAlgorithm %d #####\n",__func__,__LINE__,cert[certparseoffset]);
        }
        certparseoffset+=1; //PKAlgorithm
        parsedcert->verification_key =certparseoffset;
	if(asm_log_level >= LOG_DEBUG){
	    AWSEC_LOG(ASM,&cert[certparseoffset],33,"***** Start Of Verification Key *****\n");
	    AWSEC_LOG(ASM,NULL,0,"*****  End Of Verification Key  *****\n");
	}
        certparseoffset +=33; //assuming SHA256
    }
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"%d - %d\n",parsedcert->verification_key,certparseoffset);
    }
    if(cf & 0x04) //cf encryption
    {
        if(asm_log_level >= LOG_DEBUG){
            AWSEC_LOG(ASM,NULL,0,"\n##### %s : %d PKAlgorithm %d #####\n",__func__,__LINE__,cert[certparseoffset]);
        }
        certparseoffset+=1; //PKAlgorithm
        if(asm_log_level >= LOG_DEBUG){
            AWSEC_LOG(ASM,NULL,0,"\n##### %s : %d SymmAlgorithm %d #####\n",__func__,__LINE__,cert[certparseoffset]);
        }
        certparseoffset+=1; //SymmAlgorithm
        parsedcert->encryption_key =certparseoffset;
	if(asm_log_level >= LOG_DEBUG){
	    AWSEC_LOG(ASM,&cert[certparseoffset],33,"***** Start Of Encryption Key *****\n");
	    AWSEC_LOG(ASM,NULL,0,"*****  End Of Encryption Key  *****\n");
	}
        certparseoffset +=33; //assuming SHA256

    }
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"%d - %d\n",parsedcert->verification_key,certparseoffset);
    }
    //TobeSignedCert done

    parsedcert->sig_RV = certparseoffset;
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"##### %s : %d parsedcert->sig_RV %02x #####\n",__func__,__LINE__,parsedcert->sig_RV);
    }
    switch(version_and_type) //assuming SHA256 and ignoring private_use
    {
    case 2:
	if(asm_log_level >= LOG_DEBUG){
	    AWSEC_LOG(ASM,&cert[certparseoffset],65,"***** Start Of CASE 2 *****\n");
	    AWSEC_LOG(ASM,NULL,0,"*****  End Of CASE 2  *****\n");
	}
        certparseoffset +=65;
        break;
    case 3:
	if(asm_log_level >= LOG_DEBUG){
	    AWSEC_LOG(ASM,&cert[certparseoffset],33,"***** Start Of CASE 3 *****\n");
	    AWSEC_LOG(ASM,NULL,0,"*****  End Of CASE 3  *****\n");
	}
        certparseoffset +=33;
        break;
    default:
        if(asm_log_level >= LOG_CRITICAL)
	    AWSEC_LOG(ASM,NULL,0,"INVALID version and type\n");
    }
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"Verification key:%d - Certparseoffset:%d\n",parsedcert->verification_key,certparseoffset);
        AWSEC_LOG(ASM,NULL,0,"##### %s : %d : Certificate End Offset %d #####\n",__func__,__LINE__,certparseoffset);
    }
    return certparseoffset;
}
static Time64 base_time = 0;

static Time64
get_current_time(void)
{
    struct timeval tv;
    Time64 time_us = 0;
    if (0 != gettimeofday(&tv, NULL)) {
	if(asm_log_level >= LOG_CRITICAL){
	    AWSEC_LOG(ASM,NULL,0,"gettimeofday() failed. errno=[%d]\n", errno);
	}
        exit(1);
    }
    time_us = tv.tv_sec;
    time_us = time_us * 1000 * 1000;
    time_us += tv.tv_usec;
    if (base_time == 0) {
        base_time = get_base_time();
    }
    return time_us - base_time - NUMBER_OF_LEAP_MICROSECONDS_TODAY;
}


/* if encContentType is valied value, then signed message is encapsulated in ToBeEncrypted
 * if encContentType is 0xff then this function return only signedMessage.
 */
int32_t signInToBeEncrypted(UINT32 cmd, uint8_t *SignedData, uint8_t encContentType, uint8_t identifierType, uint8_t *certData, uint8_t *ToBeSignedDataBuf, int32_t dataLen, UINT8 *pPrivKeyBuf, INT32 GroupType, UINT8 *pOptData, UINT32 OptLen,UINT32 checkFlag,uint8_t sign_with_fast_verification, uint8_t *errorCode)
{
  INT32 uiTmpBufLen,bFound = 0;
  UINT32 uiDigestLen,uiSignDataLen;
  int32_t SIGNoffset = 0, uiRetLen, offset, i,yinfo=0;
  struct issuerCertInfo *issuerntry;
  UINT8 pDigest[SHA256_DIGEST_LENGTH], pSignData[256];
  ECDSA_SIG *pSig = NULL;
  EC_KEY *ECKey =NULL;
  uint32_t psidLen = 0;
  uint32_t certChainLen =0;
  UINT8 *pTmpBuf = NULL;
  //BIGNUM *in_kinv = NULL, *in_r = NULL; 
    uint32_t flag = 0;
    //int32_t *certNo[10];
    struct issuerCertInfo *certNo[10];
    int32_t noOfCert =0;
  if(checkFlag){
      if(lcm_log_level >= LOG_DEBUG){
          flag=(LCM && lcmoptions.Log_SignEncrypt_Input && (lcmoptions.totalLogFlag & (1<<cmd))) ? LCM : 0;
          AWSEC_LOG(flag,ToBeSignedDataBuf,dataLen,StageString[cmd]," Input"); 
      }
  }
  else{
      if(asm_log_level >= LOG_DEBUG){
          AWSEC_LOG(ASM,ToBeSignedDataBuf,dataLen,StageString[cmd],"*** Start Of Unsigned Data ***\n"); 
          AWSEC_LOG(ASM,NULL,0,"*******  End Of Unsigned Data  ******\n"); 
      }
      if(asm_log_level >= LOG_INF){
          AWSEC_LOG(ASM,NULL,0,"##### %s : %d : Server Path %s : Key Directory %s #####\n",__func__,__LINE__,serverPath,asmoptions.KEY_CONF_DIR);
      }
  }
  if(encContentType != 0xff)                                                     // check 
  {
    //start Encrypted Message		
    SignedData[SIGNoffset] = encContentType; //ToBeEncrypted->ContentType
    SIGNoffset += 1;
  }
  //start Signed Message		
  SignedData[SIGNoffset] = identifierType; //SignerIdentifierType->certificate
  SIGNoffset += 1;
  certStore *curCertInfo = (certStore*)certData;
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,(UINT8 *)curCertInfo->slotCert.certInfo,curCertInfo->slotCert.certLen,"******* Current Cert Data ******\n");
      AWSEC_LOG(ASM,NULL,0,"*******  End Of the Current Cert Data ******\n");
  }
  if(identifierType == certificate_digest_with_ecdsap256)
  {
    if(asm_log_level >= LOG_INF)
          AWSEC_LOG(ASM,NULL,0,"NAZEER: Cert digest copied into Sign message\n");
    
    if(checkFlag){
           memcpy((SignedData + SIGNoffset), curCertInfo->hash, 8);
           SIGNoffset += 8;
    }
    else{
        if(!(curCertInfo->certCheckFlag & IS_CERT_REVOKED)){
           memcpy((SignedData + SIGNoffset), curCertInfo->hash, 8);
           SIGNoffset += 8;
        }
        else{
           if(asm_log_level >= LOG_CRITICAL)
               AWSEC_LOG(ASM,NULL,0,"Invalid Slot certificate received to calculate the Certificate digest\n");
           *errorCode = CMD_ERR_INVALID_INPUT;
           return FAILURE;
        }
    }
		
  }
  else if (identifierType == certificate)
  {
    if(checkFlag){
        memcpy((SignedData + SIGNoffset), curCertInfo->slotCert.certInfo, curCertInfo->slotCert.certLen);
        SIGNoffset += curCertInfo->slotCert.certLen;
    }
    else{
        if(!(curCertInfo->certCheckFlag & IS_CERT_REVOKED)){
            if(asm_log_level >= LOG_DEBUG){
                AWSEC_LOG(ASM,(UINT8 *)curCertInfo->slotCert.certInfo,curCertInfo->slotCert.certLen,"NAZEER: Cert copied into Sign message\n");
		AWSEC_LOG(ASM,NULL,0,"Length of the cert is :%d\n",curCertInfo->slotCert.certLen);
                AWSEC_LOG(ASM,curCertInfo->privSignKey,32,"NAZEER: Private Key\n");
                AWSEC_LOG(ASM,NULL,0," End of Private Key\n");
            }
	  	
	    memcpy((SignedData + SIGNoffset), curCertInfo->slotCert.certInfo, curCertInfo->slotCert.certLen);
            SIGNoffset += curCertInfo->slotCert.certLen;
        }
        else{
           if(asm_log_level >= LOG_CRITICAL)
               AWSEC_LOG(ASM,NULL,0,"Invalid Slot certificate received to calculate the Certificate\n");
           *errorCode = CMD_ERR_INVALID_INPUT;
           return FAILURE;
        }
    }
  }
  else if (identifierType == certificate_chain)
  {
      /** Certificate Chain needs to constructed and accessed to copy the certificate chain **/
#if 0
      Certificate *cert = (Certificate *)certData;
      printf("Got the certificate chain\n");
      memcpy((SignedData + SIGNoffset), cert->certInfo, cert->certLen); 
      SIGNoffset += cert->certLen;
#endif
      certStore *cert = (certStore *)certData;
      if(asm_log_level >= LOG_INF)
          AWSEC_LOG(ASM,NULL,0,"Got the certificate chain\n");
      bFound = 0;
      LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
          if(asm_log_level >= LOG_INF)
              AWSEC_LOG(ASM,NULL,0,"How many times the list was executed\n");
          //if (!(memcmp(issuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
          if (!(memcmp(issuerntry->pCertInfo.hash, &cert->slotCert.certInfo[cert->slotCert.signer_id], 8))){
              if(asm_log_level >= LOG_INF)
                  AWSEC_LOG(ASM,NULL,0,"Root Certificate found in the list\n");
              bFound = 1;
              break;
          }
      }
      if (!bFound){
          *errorCode = CMD_ERR_CERT_NOT_FOUND;
           goto err;
      }
      else{
          certNo[noOfCert] = issuerntry;
          certChainLen += issuerntry->pCertInfo.slotCert.certLen;
          //certChainLen += certNo[noOfCert]->pCertInfo.slotCert.certLen;
          issuerntry->processTime = get_cur_time2004();
          if(asm_log_level >= LOG_INF)
              AWSEC_LOG(ASM,NULL,0,"NAZEER: Certificate found in the Cert Store List\n");
      }

      while(certNo[noOfCert]->pCertInfo.slotCert.certInfo[1] != root_ca){
          bFound = 0;
          LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
              if(asm_log_level >= LOG_INF)
                  AWSEC_LOG(ASM,NULL,0,"How many times the list was executed\n");
              //if (!(memcmp(issuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
              if (!(memcmp(issuerntry->pCertInfo.hash, &certNo[noOfCert]->pCertInfo.slotCert.certInfo[certNo[noOfCert]->pCertInfo.slotCert.signer_id], 8))){
                  if(asm_log_level >= LOG_INF)
                      AWSEC_LOG(ASM,NULL,0,"Root Certificate found in the list\n");
                  bFound = 1;
                  break;
              }
          }
          if (!bFound){
	      *errorCode = CMD_ERR_CERT_NOT_FOUND;
	      goto err;
          }
          else{
              noOfCert += 1;
              certNo[noOfCert] = issuerntry;
              certChainLen += issuerntry->pCertInfo.slotCert.certLen;
              issuerntry->processTime = get_cur_time2004();
              if(asm_log_level >= LOG_INF)
                  AWSEC_LOG(ASM,NULL,0,"NAZEER: Certificate found in the Cert Store List\n");
          }
      }
      certChainLen += cert->slotCert.certLen;
      encode_length(&SignedData[SIGNoffset], certChainLen, &uiRetLen);
      SIGNoffset += uiRetLen;
      while(noOfCert >= 0){
          if(asm_log_level >= LOG_DEBUG){
              AWSEC_LOG(ASM,certNo[noOfCert]->pCertInfo.slotCert.certInfo,certNo[noOfCert]->pCertInfo.slotCert.certLen,"**** Start Of certNo[noOfCert]->pCertInfo.slotCert.certInfo ****\n");
              AWSEC_LOG(ASM,NULL,0,"**** End Of certNo[noOfCert]->pCertInfo.slotCert.certInfo ****\n");
          }
          memcpy((SignedData + SIGNoffset),certNo[noOfCert]->pCertInfo.slotCert.certInfo,certNo[noOfCert]->pCertInfo.slotCert.certLen );
          SIGNoffset += certNo[noOfCert]->pCertInfo.slotCert.certLen;
          noOfCert -=1; 
      } 
      // memcpy((SignedData + SIGNoffset), RootCAcert.certInfo, RootCAcert.certLen);
      // SIGNoffset += RootCAcert.certLen; 
      memcpy((SignedData + SIGNoffset), cert->slotCert.certInfo, cert->slotCert.certLen); 
      SIGNoffset += cert->slotCert.certLen;
      if(asm_log_level >= LOG_DEBUG){
          AWSEC_LOG(ASM,(UINT8 *) RootCAcert.certInfo,RootCAcert.certLen,"Current root Cert Data\n");
          AWSEC_LOG(ASM,NULL,0,"End of the root Cert Data\n");
      }
      if(asm_log_level >= LOG_DEBUG){
          AWSEC_LOG(ASM,(UINT8 *)cert->slotCert.certInfo,cert->slotCert.certLen,"Current Cert Data\n");
          AWSEC_LOG(ASM,NULL,0,"End of the Current Cert Data\n");
      }
  }
  else{
      *errorCode = CMD_ERR_INVALID_PACKET;
      return FAILURE;
  }
  
  offset = SIGNoffset;
  if(!checkFlag){
      SignedData[SIGNoffset++] = pOptData[0];
      psidLen = pOptData[1];
      memcpy((SignedData + SIGNoffset), &pOptData[2], psidLen);
      SIGNoffset += psidLen;
      encode_length(&SignedData[SIGNoffset], dataLen, &uiRetLen);
      SIGNoffset += uiRetLen;
  }
  memmove((SignedData + SIGNoffset), ToBeSignedDataBuf, dataLen); 
  SIGNoffset += dataLen;
//  if (!checkFlag && (OptLen > 1) )
  if ((OptLen > (2 + psidLen)) )
  {
    memcpy((SignedData + SIGNoffset), &pOptData[psidLen+2], OptLen -(psidLen+2));
    SIGNoffset += OptLen - (psidLen+2);
  }
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"Data sent for Digest = %d\n", SIGNoffset - offset);
  }
  if (AWSecHash(HASH_ALGO_SHA256, &SignedData[offset], SIGNoffset - offset, pDigest, &uiDigestLen, 0) == NULL)
  {

    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,SignedData,SIGNoffset,"%s:%s:%d:AWSecHash(): %s, Printing Signed Data\n",StageString[cmd],__FILE__,__LINE__, strerror(errno));
    goto err;

  }

  if (GroupType == ECDSA_NISTP_256)
  {
#ifdef NOCODE
     EC_GROUP *pGroup;
      if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL)

         {
            if(asm_log_level >= LOG_CRITICAL){
	        AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
	    }
            if(pGroup)
                EC_GROUP_clear_free(pGroup);
            goto err;
         }
     ReconstructPub(curCertInfo->privSignKey,pGroup);
                if(pGroup)
                  EC_GROUP_clear_free(pGroup);
#endif
    if (!(ECKey = ECKeyFromPrivKey(ECDSA_NISTP_256, NULL, curCertInfo->privSignKey, 32)))
    {
          if(asm_log_level >= LOG_CRITICAL){
	      AWSEC_LOG(ASM,NULL,0,"%s:%d:ECKeyFromPrivKey(): %s\n",__FILE__,__LINE__, strerror(errno));
	  }
          goto err;
    }
  }
  else if(GroupType == ECDSA_NISTP_224)
  {
    if (!(ECKey = ECKeyFromPrivKey(ECDSA_NISTP_224, NULL, curCertInfo->privSignKey, 24)))
    {
         if(asm_log_level >= LOG_CRITICAL){
             AWSEC_LOG(ASM,NULL,0,"%s:%d:ECKeyFromPrivKey(): %s\n",__FILE__,__LINE__, strerror(errno));
	 }
         goto err;
    }
  }
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,pDigest,32,"pDigest Data\n");
      AWSEC_LOG(ASM,NULL,0,"End pDigest Data\n");
  }
  if(asm_log_level >= LOG_INF)
      AWSEC_LOG(ASM,NULL,0,"SIGN-1.1\n");
  if(sign_with_fast_verification){
      if(!(pSig = Fast_ecdsa_do_sign(pDigest, uiDigestLen,NULL,NULL, ECKey,&yinfo ))){
          if(asm_log_level >= LOG_CRITICAL){
              AWSEC_LOG(ASM,NULL,0,"%s:%d:Fast_ecdsa_do_sign(): %s\n",__FILE__,__LINE__, strerror(errno));
              AWSEC_LOG(ASM,SignedData,SIGNoffset,"%s: Couldn't sign the data\n",StageString[cmd]);
          }
          goto err;
      } 
      SignedData[SIGNoffset++] = yinfo;
      if(asm_log_level >= LOG_DEBUG){
          AWSEC_LOG(ASM,NULL,0,"##### %s : %d : sign_with_fast_verification %d ##### yinfo = %x\n",__func__,__LINE__,sign_with_fast_verification, yinfo);
      }
  }
  else{
      if(!(pSig = AWSecSignData(pDigest, uiDigestLen, ECKey, pSignData, &uiSignDataLen))){
        if(asm_log_level >= LOG_CRITICAL){
            AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecSignData(): %s\n",__FILE__,__LINE__, strerror(errno));
            AWSEC_LOG(ASM,SignedData,SIGNoffset,"%s: Couldn't sign the data\n",StageString[cmd]);
        }
        goto err;
      }
      SignedData[SIGNoffset++] = 0x00; //SaveKeys.pStaticKeyPub[0]; //NAZEER; //EccPublicKeyType+R+S. calculated over[from 'start ToBeSignedAnonymousCertRequestReq' to end 'ToBeSignedAnonymousCertRequestReq'], using the private key corresponding to the public verification key in CSR certificate from bootstarp resp.
  }
if(asm_log_level >= LOG_INF)
  AWSEC_LOG(ASM,NULL,0,"SIGN-1.2\n");
  if(checkFlag){
      if (cmd == ANONCERTREQ_REQ)
      {
          memcpy(SaveKeys.DigestInfo, pDigest, uiDigestLen);
      }
      else if (cmd == ANONCERTREQ_STS_REQ)
      {
          memcpy(SaveKeys.pStatusReqDigestInfo, pDigest, uiDigestLen);
      }
      else if (cmd == DECRYPTIONKEY_REQ)
      {
          memcpy(SaveKeys.pDecryptDigestInfo, pDigest, uiDigestLen);
      }
      else if (cmd == MISBEHAVIOURREPORT_REQ)
      {
          memcpy(SaveKeys.pMisBehaveDigestInfo, pDigest, uiDigestLen);
      }
      else if (cmd == CRL_REQ)
      {
          memcpy(SaveKeys.pCrlDigestInfo, pDigest, uiDigestLen);
      }
  }
if(asm_log_level >= LOG_INF)
  AWSEC_LOG(ASM,NULL,0,"SIGN-1.3\n");
  pTmpBuf = BN_bn2hex(pSig->r);
  uiTmpBufLen = HexStr2ByteStr(pTmpBuf, &SignedData[SIGNoffset], 32);
  OPENSSL_free(pTmpBuf);
  SIGNoffset += uiTmpBufLen;
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"uiTmpBufLen = %lu\n", uiTmpBufLen);
  }
  pTmpBuf = BN_bn2hex(pSig->s);
  uiTmpBufLen = HexStr2ByteStr(pTmpBuf, &SignedData[SIGNoffset], 32);
  OPENSSL_free(pTmpBuf);
  SIGNoffset += uiTmpBufLen;
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"uiTmpBufLen = %lu\n", uiTmpBufLen);
  }
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"ToBeSigned data length = %u\n", SIGNoffset);
  }
  //end Signed Message		
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,SignedData,SIGNoffset,"**** Start of Signed Data ****\n"); 
      AWSEC_LOG(ASM,NULL,0,"****  End of Signed Data  ****\n");
  }
  if(pSig)
    ECDSA_SIG_free(pSig);
  if(ECKey)
    EC_KEY_free(ECKey);
        if(checkFlag){
            if(lcm_log_level >= LOG_DEBUG){
                flag=LCM && lcmoptions.Log_SignEncrypt_Before_Encrypt && (lcmoptions.totalLogFlag & (1<<cmd)) ? LCM : 0;
                AWSEC_LOG(flag,SignedData,SIGNoffset,":%s:**** Start of Signed Data ****\n",StageString[cmd]); 
	        AWSEC_LOG(flag,NULL,0,"****  End of Signed Data  ****\n");
            }
        }
        else{
            if(asm_log_level >= LOG_DEBUG){
                AWSEC_LOG(ASM,SignedData,SIGNoffset,"%s:**** Start of Signed Data ****\n",StageString[cmd]); 
	        AWSEC_LOG(ASM,NULL,0,"****  End of Signed Data  ****\n");
	    }
	}
  return SIGNoffset;

err:
    if(pSig)
        ECDSA_SIG_free(pSig);
    if(ECKey)
        EC_KEY_free(ECKey);
    return FAILURE;
    
}
extern certStore dayStore[24][12];
int32_t VerifySignedDataIn16092(UINT32 cmd, uint8_t *SignedData, uint8_t encContentType, uint8_t identifierType, uint8_t *pBuf, int32_t dataLen, INT32 GroupType, uint32_t *tf_flag, uint32_t *tmpOffset, uint8_t fast_verification,uint8_t *errorCode,AWSecMsg_Verify_Msg_Req2_t *req2)
{
  INT32 uiTmpBufLen,  len,ret_exp = -1, bFound = 0 ,cFound = 0;
  UINT32 uiDigestLen;
  struct issuerCertInfo *issuerntry;
  struct tempCertInfo *tempntry;
  struct SLCertInfo *SLntry;
  struct TIMCertInfo *TIMntry;
  struct SPATCertInfo *SPATntry;
  struct GIDCertInfo *GIDntry;
  uint8_t ret_val=5;
  EC_GROUP *pGroup;
  EC_POINT *pubPoint;
  uint32_t psID=0,retIDx;
  char tmp_str[256];
  int32_t offset = 0, signOffSet, CertSignDataLen, i,tmp_offset =0;
  UINT8 pDigest[SHA256_DIGEST_LENGTH], pSignData[1024], pPubKey[33], pSignerID[8];
  ECDSA_SIG *pSig = NULL;
  EC_KEY *ECKey =NULL;
  UINT8 logstring[250];
  
  Certificate msgCert, MsgCAcert;
  Time64 expiry_time = 0, generation_time = 0;
  Time32 root_cert_expiry=0,current_time=0;
  if ((pBuf[offset + 1] == 0x01) && (pBuf[offset + 2] == 0x04))
  {
    offset = 3;
    dataLen = decode_length((pBuf + offset), &len);
    offset += len;
    if(!((pBuf[offset+1] < 4) || (pBuf[offset+1] == message_ca) || (pBuf[offset+1] == message_ra) || (pBuf[offset+1] == root_ca))){
        *errorCode = CMD_ERR_INCORRECT_SIGNING_CERT_TYPE;
        return FAILURE;
    }
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"dataLen = %d, len = %d\n", dataLen, len);
    }
/** We should call Import Cert Chain function to add all the certificates in the chain **/
    msgCert.certLen = certParse((pBuf + offset), &msgCert);
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"%s : %d : msgCert.certLen = %d\n", __func__,__LINE__,msgCert.certLen);
    }
    //printf("\n##### %s : %d : psID %d #####\n",__func__,__LINE__,psID);
    memcpy(msgCert.certInfo, (pBuf + offset), msgCert.certLen);
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,(UINT8 *)msgCert.certInfo,msgCert.certLen,"Received Cert Data\n"); 
        AWSEC_LOG(ASM,NULL,0,"****  End of Received Cert Data  ****\n");
    }
    memcpy(&root_cert_expiry,&msgCert.certInfo[msgCert.expiration],4);           // Here add the certificate in issuer list if not there 
    root_cert_expiry = swap_32(root_cert_expiry);
    current_time = get_cur_time2004();
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"**********CERTIFICATE EXPIRATION TIME:%d current time:%d*******\n",root_cert_expiry,current_time);
    }
    if(root_cert_expiry <= current_time){
        *errorCode = CMD_ERR_CERT_EXPIRED;
	if(asm_log_level >= LOG_INF)
	  AWSEC_LOG(ASM,NULL,0,"certificate expired\n");
	return FAILURE;
    }
    if (AWSecHash(HASH_ALGO_SHA256, (pBuf + offset), msgCert.certLen, pDigest, &uiDigestLen, 8) == NULL)
    {
      if(asm_log_level >= LOG_CRITICAL)
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
      return FAILURE;
    }
    if((pBuf[offset+1] == root_ca)){                    //Checking if root_ca is in issuerList ,if not adding it to issuerList 
        bFound = 0;
        LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"How many times the Issuer list was executed\n");
            if (!(memcmp(issuerntry->pCertInfo.hash, pDigest, 8))){
	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"Root Certificate found in the Issuer list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
            issuerntry = (struct issuerCertInfo *)malloc(sizeof(struct issuerCertInfo));
            memcpy(issuerntry->pCertInfo.hash, pDigest, 8);
            issuerntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &issuerntry->pCertInfo.slotCert);
            memcpy(issuerntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, issuerntry->pCertInfo.slotCert.certLen);      
            issuerntry->pCertInfo.certCheckFlag = 0x00;
            issuerntry->processTime = get_cur_time2004();
            LIST_INSERT_HEAD(&issuerListHead, issuerntry, issuer_list);
	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: Root Certificate not found in the Cert Store(Issuer) List, Adding it to Issuer List\n");
        }
        else{
            issuerntry->processTime = get_cur_time2004();
	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: Root Certificate found in the Cert Store(Issuer) List\n");
        }
    }
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"Check Certificate chain root Certificate. If the root Certificate is not found, then we need to return from here.\n");
    offset += msgCert.certLen;
    dataLen -= msgCert.certLen;
    if(dataLen == 0)
    {
	*errorCode = CMD_ERR_CHAIN_ENDED_AT_UNKNOWN_ROOT;
	return FAILURE;
    }
/*implement  later
 if (locomate_board rootca != read_from_request rootca) return 0x3c
*/
    while (dataLen > 0)
    {
      msgCert.certLen = certParse((pBuf + offset), &msgCert);
      memcpy(msgCert.certInfo, (pBuf + offset), msgCert.certLen);
      psID = getPsidbyLen(&msgCert.certInfo[msgCert.psid_array_permOffset],&retIDx,NULL);
      if(memcmp(pDigest, (pBuf + offset + 3), 8))       //comparing signer id with issuer certifcate 
      {
	*errorCode =CMD_ERR_COULD_NOT_CONSTRUCT_CHAIN;
	if(asm_log_level >= LOG_INF)
	    AWSEC_LOG(ASM,NULL,0,"Mem comparision of the Certificate failed\n");
        return FAILURE;
      }
      if (AWSecHash(HASH_ALGO_SHA256, (pBuf + offset), msgCert.certLen, pDigest, &uiDigestLen, 8) == NULL)
      {
        if(asm_log_level >= LOG_CRITICAL)
	    AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
        return FAILURE;
      }
      if(asm_log_level >= LOG_DEBUG){
          AWSEC_LOG(ASM,(UINT8 *)pDigest,uiDigestLen,"***** Start of pDigest *****\n"); 
          AWSEC_LOG(ASM,NULL,0,"*****  End of pDigest  *****\n");
      }
      if((dataLen - msgCert.certLen) != 0){
          LIST_INIT(&tempListHead);
          bFound = 0;
          LIST_FOREACH(tempntry, &tempListHead, temp_list){
	  if(asm_log_level >= LOG_INF)
	      AWSEC_LOG(ASM,NULL,0,"How many times the Temp list was executed\n");
              if (!(memcmp(tempntry->pCertInfo.hash, pDigest, 8))){
		  if(asm_log_level >= LOG_INF)
		      AWSEC_LOG(ASM,NULL,0,"CA Certificate found in the Temp list\n");
                  bFound = 1;
                  break;
              }
          }
          if (!bFound){
              tempntry = (struct tempCertInfo *)malloc(sizeof(struct tempCertInfo));
              memcpy(tempntry->pCertInfo.hash, pDigest, 8);
              tempntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &tempntry->pCertInfo.slotCert);
              memcpy(tempntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, tempntry->pCertInfo.slotCert.certLen);      
              tempntry->pCertInfo.certCheckFlag = 0x00;
              tempntry->processTime = get_cur_time2004();
              LIST_INSERT_HEAD(&tempListHead, tempntry, temp_list);
    	      if(asm_log_level >= LOG_INF)
	          AWSEC_LOG(ASM,NULL,0,"NAZEER: CA Certificate not found in the Cert Store(Temp) List, Adding it to Temp List\n");
          }
          else{
              tempntry->processTime = get_cur_time2004();
    	      if(asm_log_level >= LOG_INF)
	          AWSEC_LOG(ASM,NULL,0,"NAZEER: CA Certificate found in the Cert Store(Temp) List\n");
          }
      }
#ifdef NAZEER
      if (findCertInfo(pDigest, (pBuf + offset + 3)) == NULL)
      {
        addCertInfo(pDigest, (pBuf + offset + 3));
        return FAILURE;
      }
#endif
      psID = getPsidbyLen(&msgCert.certInfo[msgCert.psid_array_permOffset],&retIDx,NULL);
      if(msgCert.certInfo[0] == 0x02)	  
      {
          CertSignDataLen = (&msgCert.certInfo[msgCert.sig_RV] - msgCert.certInfo);
          if((AWSecDigestAndVerifyData((pBuf + offset), CertSignDataLen, (pBuf + offset + CertSignDataLen), pPubKey)) != SUCCESS)
          {
	      *errorCode = CMD_ERR_CERT_VERIFICATION_FAILED;//Certificate Verification Failed
	      if(asm_log_level >= LOG_CRITICAL)
                  AWSEC_LOG(1,NULL,0,"Signature verification failed\n");
              return FAILURE;
          }
          else
          {
	      if(asm_log_level >= LOG_INFO)
                  AWSEC_LOG(1,NULL,0,"Signature verified successfully\n");
          }
      }
      offset += msgCert.certLen;
      dataLen -= msgCert.certLen;
    }
    if(tempntry != NULL){
        bFound = 0;
        LIST_FOREACH(tempntry, &tempListHead, temp_list){
            LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"How many times the Issuer list was executed\n");
                if (!(memcmp(tempntry->pCertInfo.hash, issuerntry->pCertInfo.hash, 8))){
    	            if(asm_log_level >= LOG_INF)
	                AWSEC_LOG(ASM,NULL,0,"CA Certificate found in the Issuer list\n");
                    bFound = 1;
                    break;
                }
            }
            if (!bFound){
                issuerntry = (struct issuerCertInfo *)malloc(sizeof(struct issuerCertInfo));
                memcpy(issuerntry->pCertInfo.hash, tempntry->pCertInfo.hash, 8);
                //issuerntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &issuerntry->pCertInfo.slotCert);
                issuerntry->pCertInfo.slotCert.certLen = tempntry->pCertInfo.slotCert.certLen;
                memcpy(issuerntry->pCertInfo.slotCert.certInfo, tempntry->pCertInfo.slotCert.certInfo, issuerntry->pCertInfo.slotCert.certLen);                     
                issuerntry->pCertInfo.certCheckFlag = 0x00;
                issuerntry->processTime = get_cur_time2004();
                LIST_INSERT_HEAD(&issuerListHead, issuerntry, issuer_list);
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"NAZEER: CA Certificate not found in the Cert Store(Issuer) List, Adding it to Issuer List from Temp List\n");
            }
            else{
                issuerntry->processTime = get_cur_time2004();
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"NAZEER: CA Certificate found in the Cert Store(Issuer) List\n");
            }
        }
        LIST_FOREACH(tempntry, &tempListHead, temp_list){
            LIST_REMOVE(tempntry, temp_list);
            free(tempntry);
        }
    }
    //stCertInfo();
    if(psID == 32){
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"\n##### %s : %d : psID %d #####\n",__func__,__LINE__,psID);
        }
        bFound = 0;
        LIST_FOREACH(SLntry, &SLListHead, SL_list){
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"How many times the SL list was executed\n");
            if (!(memcmp(SLntry->pCertInfo.hash, pDigest, 8))){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"ShortLived Entity Certificate found in the SL list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
            SLntry = (struct SLCertInfo *)malloc(sizeof(struct SLCertInfo));
            memcpy(SLntry->pCertInfo.hash, pDigest, 8);
            SLntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &SLntry->pCertInfo.slotCert);
            memcpy(SLntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, SLntry->pCertInfo.slotCert.certLen);      
            SLntry->pCertInfo.certCheckFlag = 0x00;
            SLntry->processTime = get_cur_time2004();
            if(msgCert.certInfo[0] == 0x03){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"##### LIST 1 #####\n");
                UINT8 pDigest[32], msgca_cert[512];
                UINT32 uiDigestLen;
                if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
#if 0
                bzero(tmp_str,256);
                sprintf(tmp_str,"%s/%s/root_ca.cert",serverPath,asmoptions.KEY_CONF_DIR);
                uiTmpBufLen = read_file(tmp_str, msgca_cert, sizeof(msgca_cert));
                MsgCAcert.certLen = certParse(msgca_cert, &MsgCAcert);
                memcpy(MsgCAcert.certInfo, msgca_cert, MsgCAcert.certLen);
#else
                cFound = 0;
                LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
                    if (!(memcmp(issuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
                         cFound = 1;
                         break;
                    }
                }
                if (!cFound){
	            *errorCode = CMD_ERR_CHAIN_ENDED_AT_UNKNOWN_ROOT;
	            goto err;
                }
                else{
                    memcpy(&MsgCAcert,&issuerntry->pCertInfo.slotCert,sizeof(MsgCAcert));
                }
#endif
                if(asm_log_level >= LOG_DEBUG){
                    AWSEC_LOG(ASM,MsgCAcert.certInfo,MsgCAcert.certLen,"Start of CACert cert of %d bytes:\n", MsgCAcert.certLen); 
          	    AWSEC_LOG(ASM,NULL,0,"*****  End of CACert *****\n");
      		}

                if (AWSecHash(HASH_ALGO_SHA256, MsgCAcert.certInfo, MsgCAcert.certLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
	    		AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;            //pushpendra
                }
	        if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)pDigest,8,"***** Start of Root Cert Digest *****\n"); 
		    AWSEC_LOG(ASM,NULL,0,"***** End of Root Cert Digest *****\n");
	        }
                bzero(msgca_cert, 512);
                memcpy(msgca_cert, pDigest, uiDigestLen);
                memcpy(&msgca_cert[uiDigestLen], msgCert.certInfo, msgCert.certLen);

                if (AWSecHash(HASH_ALGO_SHA256, msgca_cert, msgCert.certLen + uiDigestLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
	    		AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;           //pushpendra
                }
                if(asm_log_level >= LOG_DEBUG){
                    AWSEC_LOG(ASM,(UINT8 *)(&MsgCAcert.certInfo[MsgCAcert.verification_key]),33,"CACert Verification Key length of the cert = %d\n", MsgCAcert.certLen); 
          	    AWSEC_LOG(ASM,NULL,0,"End of CACert Verification Key\n");
      		}
                if ((pubPoint = PublicKeyReconstruct(pDigest, &msgCert.certInfo[msgCert.sig_RV], &MsgCAcert.certInfo[MsgCAcert.verification_key], pPubKey, pGroup)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:PublicKeyReconstruct(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;            //pushpendra
                }
	        if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,pPubKey,33,"Public Key constructed\n"); 
		    AWSEC_LOG(ASM,NULL,0,"End of public Key\n");
	        }
                memcpy(SLntry->pCertInfo.pubSignKey, pPubKey, 33);      
            }
            else
                memcpy(SLntry->pCertInfo.pubSignKey, &msgCert.certInfo[msgCert.verification_key], 33);     
            LIST_INSERT_HEAD(&SLListHead, SLntry, SL_list);
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: ShortLived Entity Certificate not found in the Cert Store(SL) List, Adding it to SL List\n");
        }
        else{
            memcpy(pPubKey, SLntry->pCertInfo.pubSignKey, 33);      
            SLntry->processTime = get_cur_time2004();
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: ShortLived Entity Certificate found in the Cert Store(SL) List\n");
        }
    }
    else if(psID == 32771){
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"\n##### %s : %d : psID %d #####\n",__func__,__LINE__,psID);
        }
        bFound = 0;
        LIST_FOREACH(TIMntry, &TIMListHead, TIM_list){
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"How many times the TIM list was executed\n");
            if (!(memcmp(TIMntry->pCertInfo.hash, pDigest, 8))){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"TIM Entity Certificate found in the TIM list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
            TIMntry = (struct TIMCertInfo *)malloc(sizeof(struct TIMCertInfo));
            memcpy(TIMntry->pCertInfo.hash, pDigest, 8);
            TIMntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &TIMntry->pCertInfo.slotCert);
            memcpy(TIMntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, TIMntry->pCertInfo.slotCert.certLen);      
            TIMntry->pCertInfo.certCheckFlag = 0x00;
            TIMntry->processTime = get_cur_time2004();
            if(msgCert.certInfo[0] == 0x03){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"##### LIST 1 #####\n");
                UINT8 pDigest[32], msgca_cert[512];
                UINT32 uiDigestLen;
                if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
#if 0
                bzero(tmp_str,256);
                sprintf(tmp_str,"%s/%s/root_ca.cert",serverPath,asmoptions.KEY_CONF_DIR);
                uiTmpBufLen = read_file(tmp_str, msgca_cert, sizeof(msgca_cert));
                MsgCAcert.certLen = certParse(msgca_cert, &MsgCAcert);
                memcpy(MsgCAcert.certInfo, msgca_cert, MsgCAcert.certLen);
#else
                cFound = 0;
                LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
                    if (!(memcmp(issuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
                         cFound = 1;
                         break;
                    }
                }
                if (!cFound){
	            *errorCode = CMD_ERR_CHAIN_ENDED_AT_UNKNOWN_ROOT;
	            goto err;
                }
                else{
                    memcpy(&MsgCAcert,&issuerntry->pCertInfo.slotCert,sizeof(MsgCAcert));
                }
#endif
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,MsgCAcert.certInfo,MsgCAcert.certLen,"Start of CACert cert of %d bytes:\n", MsgCAcert.certLen);                          
		    AWSEC_LOG(ASM,NULL,0,"*****  End of CACert *****\n");
		}

                if (AWSecHash(HASH_ALGO_SHA256, MsgCAcert.certInfo, MsgCAcert.certLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
	    		AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)pDigest,8,"***** Start of Root Cert Digest *****\n"); 
		    AWSEC_LOG(ASM,NULL,0,"***** End of Root Cert Digest *****\n");
		}
                bzero(msgca_cert, 512);
                memcpy(msgca_cert, pDigest, uiDigestLen);
                memcpy(&msgca_cert[uiDigestLen], msgCert.certInfo, msgCert.certLen);

                if (AWSecHash(HASH_ALGO_SHA256, msgca_cert, msgCert.certLen + uiDigestLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
	    		AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)&MsgCAcert.certInfo[MsgCAcert.verification_key],33,"CACert Verification Key length of the cert = %d\n", MsgCAcert.certLen);                          
		    AWSEC_LOG(ASM,NULL,0,"End of CACert Verification Key\n");
		}
                if ((pubPoint = PublicKeyReconstruct(pDigest, &msgCert.certInfo[msgCert.sig_RV], &MsgCAcert.certInfo[MsgCAcert.verification_key], pPubKey, pGroup)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:PublicKeyReconstruct(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;                   //pushpendra
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,pPubKey,33,"Public Key constructed\n"); 
		    AWSEC_LOG(ASM,NULL,0,"End of public Key\n");
		}
                memcpy(TIMntry->pCertInfo.pubSignKey, pPubKey, 33);      
            }
            else
                memcpy(TIMntry->pCertInfo.pubSignKey, &msgCert.certInfo[msgCert.verification_key], 33);     
            LIST_INSERT_HEAD(&TIMListHead, TIMntry, TIM_list);
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: TIM Entity Certificate not found in the Cert Store(TIM) List, Adding it to TIM List\n");
        }
        else{
            memcpy(pPubKey, TIMntry->pCertInfo.pubSignKey, 33);      
            TIMntry->processTime = get_cur_time2004();
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: TIM Entity Certificate found in the Cert Store(SL) List\n");
        }
    }
    else if(psID == 49120){
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"\n##### %s : %d : psID %d #####\n",__func__,__LINE__,psID);
        }
        bFound = 0;
        LIST_FOREACH(SPATntry, &SPATListHead, SPAT_list){
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"How many times the SPAT list was executed\n");
            if (!(memcmp(SPATntry->pCertInfo.hash, pDigest, 8))){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"SPAT Entity Certificate found in the SPAT list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
            SPATntry = (struct SPATCertInfo *)malloc(sizeof(struct SPATCertInfo));
            memcpy(SPATntry->pCertInfo.hash, pDigest, 8);
            SPATntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &SPATntry->pCertInfo.slotCert);
            memcpy(SPATntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, SPATntry->pCertInfo.slotCert.certLen);      
            SPATntry->pCertInfo.certCheckFlag = 0x00;
            SPATntry->processTime = get_cur_time2004();
            if(msgCert.certInfo[0] == 0x03){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"##### LIST 1 #####\n");
                UINT8 pDigest[32], msgca_cert[512];
                UINT32 uiDigestLen;
                if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
#if 0
                bzero(tmp_str,256);
                sprintf(tmp_str,"%s/%s/root_ca.cert",serverPath,asmoptions.KEY_CONF_DIR);
                uiTmpBufLen = read_file(tmp_str, msgca_cert, sizeof(msgca_cert));
                MsgCAcert.certLen = certParse(msgca_cert, &MsgCAcert);
                memcpy(MsgCAcert.certInfo, msgca_cert, MsgCAcert.certLen);
#else
                cFound = 0;
                LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
                    if (!(memcmp(issuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
                         cFound = 1;
                         break;
                    }
                }
                if (!cFound){
	            *errorCode = CMD_ERR_CHAIN_ENDED_AT_UNKNOWN_ROOT;
	            goto err;
                }
                else{
                    memcpy(&MsgCAcert,&issuerntry->pCertInfo.slotCert,sizeof(MsgCAcert));
                }
#endif
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,MsgCAcert.certInfo,MsgCAcert.certLen,"Start of CACert cert of %d bytes:\n",MsgCAcert.certLen); 
		    AWSEC_LOG(ASM,NULL,0,"*****  End of CACert *****\n");
		}

                if (AWSecHash(HASH_ALGO_SHA256, MsgCAcert.certInfo, MsgCAcert.certLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
	    		AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)pDigest,8,"***** Start of Root Cert Digest *****\n"); 
		    AWSEC_LOG(ASM,NULL,0,"***** End of Root Cert Digest *****\n");
		}
                bzero(msgca_cert, 512);
                memcpy(msgca_cert, pDigest, uiDigestLen);
                memcpy(&msgca_cert[uiDigestLen], msgCert.certInfo, msgCert.certLen);

                if (AWSecHash(HASH_ALGO_SHA256, msgca_cert, msgCert.certLen + uiDigestLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
	    		AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)&MsgCAcert.certInfo[MsgCAcert.verification_key],33,"CACert Verification Key length of the cert = %d\n", MsgCAcert.certLen); 
		    AWSEC_LOG(ASM,NULL,0,"End of CACert Verification Key\n");
		}
                if ((pubPoint = PublicKeyReconstruct(pDigest, &msgCert.certInfo[msgCert.sig_RV], &MsgCAcert.certInfo[MsgCAcert.verification_key], pPubKey, pGroup)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:PublicKeyReconstruct(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,pPubKey,33,"Public Key constructed\n");                
		    AWSEC_LOG(ASM,NULL,0,"End of public Key\n");
		}
                memcpy(SPATntry->pCertInfo.pubSignKey, pPubKey, 33);      
            }
            else
                memcpy(SPATntry->pCertInfo.pubSignKey, &msgCert.certInfo[msgCert.verification_key], 33);     
            LIST_INSERT_HEAD(&SPATListHead, SPATntry, SPAT_list);
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: SPAT Entity Certificate not found in the Cert Store(SPAT) List, Adding it to SPAT List\n");
        }
        else{
            memcpy(pPubKey, SPATntry->pCertInfo.pubSignKey, 33);      
            SPATntry->processTime = get_cur_time2004();
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: SPAT Entity Certificate found in the Cert Store(SPAT) List\n");
        }
    }
    else if(psID == 49136){
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"##### %s : %d : psID %d #####\n",__func__,__LINE__,psID);
        }
        bFound = 0;
        LIST_FOREACH(GIDntry, &GIDListHead, GID_list){
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"How many times the GID list was executed\n");
            if (!(memcmp(GIDntry->pCertInfo.hash, pDigest, 8))){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"GID Entity Certificate found in the(GID) list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
            GIDntry = (struct GIDCertInfo *)malloc(sizeof(struct GIDCertInfo));
            memcpy(GIDntry->pCertInfo.hash, pDigest, 8);
            GIDntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &GIDntry->pCertInfo.slotCert);
            memcpy(GIDntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, GIDntry->pCertInfo.slotCert.certLen);      
            GIDntry->pCertInfo.certCheckFlag = 0x00;
            GIDntry->processTime = get_cur_time2004();
            if(msgCert.certInfo[0] == 0x03){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"##### LIST 1 #####\n");
                UINT8 pDigest[32], msgca_cert[512];
                UINT32 uiDigestLen;
                if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
#if 0
                bzero(tmp_str,256);
                sprintf(tmp_str,"%s/%s/root_ca.cert",serverPath,asmoptions.KEY_CONF_DIR);
                uiTmpBufLen = read_file(tmp_str, msgca_cert, sizeof(msgca_cert));
                MsgCAcert.certLen = certParse(msgca_cert, &MsgCAcert);
                memcpy(MsgCAcert.certInfo, msgca_cert, MsgCAcert.certLen);
#else
                cFound = 0;
                LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
                    if (!(memcmp(issuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
                         cFound = 1;
                         break;
                    }
                }
                if (!cFound){
	            *errorCode = CMD_ERR_CHAIN_ENDED_AT_UNKNOWN_ROOT;
                    goto err;
                }
                else{
                    memcpy(&MsgCAcert,&issuerntry->pCertInfo.slotCert,sizeof(MsgCAcert));
                }
#endif
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,MsgCAcert.certInfo,MsgCAcert.certLen,"Start of CACert cert of %d bytes:\n", MsgCAcert.certLen);                          
		    AWSEC_LOG(ASM,NULL,0,"*****  End of CACert *****\n");
		}

                if (AWSecHash(HASH_ALGO_SHA256, MsgCAcert.certInfo, MsgCAcert.certLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
	    		AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)pDigest,8,"***** Start of Root Cert Digest *****\n"); 
		    AWSEC_LOG(ASM,NULL,0,"***** End of Root Cert Digest *****\n");
		}
                bzero(msgca_cert, 512);
                memcpy(msgca_cert, pDigest, uiDigestLen);
                memcpy(&msgca_cert[uiDigestLen], msgCert.certInfo, msgCert.certLen);

                if (AWSecHash(HASH_ALGO_SHA256, msgca_cert, msgCert.certLen + uiDigestLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
	    		AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)&MsgCAcert.certInfo[MsgCAcert.verification_key],33,"CACert Verification Key length of the cert = %d\n", MsgCAcert.certLen); 
		    AWSEC_LOG(ASM,NULL,0,"End of CACert Verification Key\n");
		}
                if ((pubPoint = PublicKeyReconstruct(pDigest, &msgCert.certInfo[msgCert.sig_RV], &MsgCAcert.certInfo[MsgCAcert.verification_key], pPubKey, pGroup)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:PublicKeyReconstruct(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,pPubKey,33,"Public Key constructed\n"); 
		    AWSEC_LOG(ASM,NULL,0,"End of public Key\n");
		}
                memcpy(GIDntry->pCertInfo.pubSignKey, pPubKey, 33);      
            }
            else
                memcpy(GIDntry->pCertInfo.pubSignKey, &msgCert.certInfo[msgCert.verification_key], 33);     
            LIST_INSERT_HEAD(&GIDListHead, GIDntry, GID_list);
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: GID Entity Certificate not found in the Cert Store(GID) List, Adding it to GID List\n");
        }
        else{
            memcpy(pPubKey, GIDntry->pCertInfo.pubSignKey, 33);      
            GIDntry->processTime = get_cur_time2004();
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: GID Entity Certificate found in the Cert Store(GID) List\n");
        }
    }
	
  }
  else if ((pBuf[offset + 1] == 0x01) && (pBuf[offset + 2] == 0x03))
  {
    offset =  3;
    msgCert.certLen = certParse((pBuf + offset), &msgCert);
    memcpy(msgCert.certInfo, (pBuf + offset), msgCert.certLen);
    psID = getPsidbyLen(&msgCert.certInfo[msgCert.psid_array_permOffset],&retIDx,NULL);
    memcpy(&root_cert_expiry,&msgCert.certInfo[msgCert.expiration],4);
    root_cert_expiry = swap_32(root_cert_expiry);
    current_time = get_cur_time2004();
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"**********CERTIFICATE EXPIRATION TIME:%d current time:%d*******\n",root_cert_expiry,current_time);
    }
    if(root_cert_expiry <= current_time){
        *errorCode = CMD_ERR_CERT_EXPIRED;
    	if(asm_log_level >= LOG_INF)
	    AWSEC_LOG(ASM,NULL,0,"certificate expired\n");
	return FAILURE;
    }
    if (AWSecHash(HASH_ALGO_SHA256, (pBuf + offset), msgCert.certLen, pDigest, &uiDigestLen, 8) == NULL)
    {
      if(asm_log_level >= LOG_CRITICAL)
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
      return FAILURE;
    }
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,(UINT8 *)rootDigest,8,"***** Start of Root Cert Digest *****\n"); 
        AWSEC_LOG(ASM,NULL,0,"***** End of Root Cert Digest *****\n");
        AWSEC_LOG(ASM,(UINT8 *)&msgCert.certInfo[msgCert.signer_id],8,"***** Start of Signer Id of Certificate *****\n");                
        AWSEC_LOG(ASM,NULL,0,"***** End of Signer Id of Certificate *****\n");
    }
    
    bFound = 0;
    LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
    	if(asm_log_level >= LOG_INF)
	    AWSEC_LOG(ASM,NULL,0,"How many times the Issuer list was executed\n");
        if (!(memcmp(issuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"Root Certificate found in the Issuer list\n");
            bFound = 1;
            break;
        }
    }
    if (!bFound){
	    *errorCode = CMD_ERR_CERT_NOT_FOUND;
	    return FAILURE;
    }
    else{
        issuerntry->processTime = get_cur_time2004();
    	if(asm_log_level >= LOG_INF)
	    AWSEC_LOG(ASM,NULL,0,"NAZEER: Root Certificate found in the Cert Store(Issuer) List\n");
    }
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,(UINT8 *)msgCert.certInfo,msgCert.certLen,"Implicit Certificate\n");
        AWSEC_LOG(ASM,NULL,0,"End of Implicit certificate\n");
    }
    if(asm_log_level >= LOG_INF)   
        AWSEC_LOG(ASM,NULL,0,"Check Certificate Manager for this Certificate, if the Certificate is not found, then store Certificate digest\n");
    offset += msgCert.certLen;
    if(psID == 32){
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"\n##### %s : %d : psID %d #####\n",__func__,__LINE__,psID);
        }
        bFound = 0;
        LIST_FOREACH(SLntry, &SLListHead, SL_list){
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"How many times the SL list was executed\n");
            if (!(memcmp(SLntry->pCertInfo.hash, pDigest, 8))){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"ShortLived Entity Certificate found in the SL list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
            SLntry = (struct SLCertInfo *)malloc(sizeof(struct SLCertInfo));
            memcpy(SLntry->pCertInfo.hash, pDigest, 8);
            SLntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &SLntry->pCertInfo.slotCert);
            memcpy(SLntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, SLntry->pCertInfo.slotCert.certLen);      
            SLntry->pCertInfo.certCheckFlag = 0x00;
            SLntry->processTime = get_cur_time2004();
            if(msgCert.certInfo[0] == 0x03){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"##### LIST 1 #####\n");
                UINT8 pDigest[32], msgca_cert[512];
                UINT32 uiDigestLen;
                if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
#if 0
                bzero(tmp_str,256);
                sprintf(tmp_str,"%s/%s/root_ca.cert",serverPath,asmoptions.KEY_CONF_DIR);
                uiTmpBufLen = read_file(tmp_str, msgca_cert, sizeof(msgca_cert));
                MsgCAcert.certLen = certParse(msgca_cert, &MsgCAcert);
                memcpy(MsgCAcert.certInfo, msgca_cert, MsgCAcert.certLen);
#else
                cFound = 0;
                LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
                    if (!(memcmp(issuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
                         cFound = 1;
                         break;
                    }
                }
                if (!cFound){
	            *errorCode = CMD_ERR_CHAIN_ENDED_AT_UNKNOWN_ROOT;
                    goto err;
                }
                else{
                    memcpy(&MsgCAcert,&issuerntry->pCertInfo.slotCert,sizeof(MsgCAcert));
                }
#endif
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,MsgCAcert.certInfo,MsgCAcert.certLen,"Start of CACert cert of %d bytes:\n", MsgCAcert.certLen); 
		    AWSEC_LOG(ASM,NULL,0,"*****  End of CACert *****\n");
		}

                if (AWSecHash(HASH_ALGO_SHA256, MsgCAcert.certInfo, MsgCAcert.certLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
			AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)pDigest,8,"***** Start of Root Cert Digest *****\n");
		    AWSEC_LOG(ASM,NULL,0,"***** End of Root Cert Digest *****\n");
		}
                bzero(msgca_cert, 512);
                memcpy(msgca_cert, pDigest, uiDigestLen);
                memcpy(&msgca_cert[uiDigestLen], msgCert.certInfo, msgCert.certLen);

                if (AWSecHash(HASH_ALGO_SHA256, msgca_cert, msgCert.certLen + uiDigestLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
			AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)&MsgCAcert.certInfo[MsgCAcert.verification_key],33,"CACert Verification Key length of the cert = %d\n", MsgCAcert.certLen);
		    AWSEC_LOG(ASM,NULL,0,"End of CACert Verification Key\n");
		}
                if ((pubPoint = PublicKeyReconstruct(pDigest, &msgCert.certInfo[msgCert.sig_RV], &MsgCAcert.certInfo[MsgCAcert.verification_key], pPubKey, pGroup)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:PublicKeyReconstruct(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,pPubKey,33,"Public Key constructed\n");
		    AWSEC_LOG(ASM,NULL,0,"End of public Key\n");
		}

                memcpy(SLntry->pCertInfo.pubSignKey, pPubKey, 33);      
            }
            else
                memcpy(SLntry->pCertInfo.pubSignKey, &msgCert.certInfo[msgCert.verification_key], 33);     
            LIST_INSERT_HEAD(&SLListHead, SLntry, SL_list);
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: ShortLived Entity Certificate not found in the Cert Store(SL) List, Adding it to SL List\n");
        }
        else{
            memcpy(pPubKey, SLntry->pCertInfo.pubSignKey, 33);      
            SLntry->processTime = get_cur_time2004();
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: ShortLived Entity Certificate found in the Cert Store(SL) List\n");
        }
    }
    else if(psID == 32771){
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"\n##### %s : %d : psID %d #####\n",__func__,__LINE__,psID);
        }
        bFound = 0;
        LIST_FOREACH(TIMntry, &TIMListHead, TIM_list){
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"How many times the TIM list was executed\n");
            if (!(memcmp(TIMntry->pCertInfo.hash, pDigest, 8))){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"TIM Entity Certificate found in the TIM list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
            TIMntry = (struct TIMCertInfo *)malloc(sizeof(struct TIMCertInfo));
            memcpy(TIMntry->pCertInfo.hash, pDigest, 8);
            TIMntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &TIMntry->pCertInfo.slotCert);
            memcpy(TIMntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, TIMntry->pCertInfo.slotCert.certLen);      
            TIMntry->pCertInfo.certCheckFlag = 0x00;
            TIMntry->processTime = get_cur_time2004();
            if(msgCert.certInfo[0] == 0x03){
                UINT8 pDigest[32], msgca_cert[512];
                UINT32 uiDigestLen;
                if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
#if 0
                bzero(tmp_str,256);
                sprintf(tmp_str,"%s/%s/root_ca.cert",serverPath,asmoptions.KEY_CONF_DIR);
                uiTmpBufLen = read_file(tmp_str, msgca_cert, sizeof(msgca_cert));
                MsgCAcert.certLen = certParse(msgca_cert, &MsgCAcert);
                memcpy(MsgCAcert.certInfo, msgca_cert, MsgCAcert.certLen);
#else
                cFound = 0;
                LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
                    if (!(memcmp(issuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
                         cFound = 1;
                         break;
                    }
                }
                if (!cFound){
	            *errorCode = CMD_ERR_CHAIN_ENDED_AT_UNKNOWN_ROOT;
                    goto err;
                }
                else{
                    memcpy(&MsgCAcert,&issuerntry->pCertInfo.slotCert,sizeof(MsgCAcert));
                }
#endif
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,MsgCAcert.certInfo,MsgCAcert.certLen,"Start of CACert cert of %d bytes:\n", MsgCAcert.certLen); 
		    AWSEC_LOG(ASM,NULL,0,"*****  End of CACert *****\n");
		}

                if (AWSecHash(HASH_ALGO_SHA256, MsgCAcert.certInfo, MsgCAcert.certLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
			AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)pDigest,8,"***** Start of Root Cert Digest *****\n"); 
		    AWSEC_LOG(ASM,NULL,0,"***** End of Root Cert Digest *****\n");
		}
                bzero(msgca_cert, 512);
                memcpy(msgca_cert, pDigest, uiDigestLen);
                memcpy(&msgca_cert[uiDigestLen], msgCert.certInfo, msgCert.certLen);

                if (AWSecHash(HASH_ALGO_SHA256, msgca_cert, msgCert.certLen + uiDigestLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
			AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)&MsgCAcert.certInfo[MsgCAcert.verification_key],33,"CACert Verification Key length of the cert = %d\n", MsgCAcert.certLen);
		    AWSEC_LOG(ASM,NULL,0,"End of CACert Verification Key\n");
		}
                if ((pubPoint = PublicKeyReconstruct(pDigest, &msgCert.certInfo[msgCert.sig_RV], &MsgCAcert.certInfo[MsgCAcert.verification_key], pPubKey, pGroup)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:PublicKeyReconstruct(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,pPubKey,33,"Public Key constructed\n"); 
		    AWSEC_LOG(ASM,NULL,0,"End of public Key\n");
		}
                memcpy(TIMntry->pCertInfo.pubSignKey, pPubKey, 33);      
            }
            else
                memcpy(TIMntry->pCertInfo.pubSignKey, &msgCert.certInfo[msgCert.verification_key], 33);     
            LIST_INSERT_HEAD(&TIMListHead, TIMntry, TIM_list);
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: TIM Entity Certificate not found in the Cert Store(TIM) List, Adding it to TIM List\n");
        }
        else{
            memcpy(pPubKey, TIMntry->pCertInfo.pubSignKey, 33);      
            TIMntry->processTime = get_cur_time2004();
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: TIM Entity Certificate found in the Cert Store(TIM) List\n");
        }
    }
    else if(psID == 49120){
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"\n##### %s : %d : psID %d #####\n",__func__,__LINE__,psID);
        }
        bFound = 0;
        LIST_FOREACH(SPATntry, &SPATListHead, SPAT_list){
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"How many times the SPAT list was executed\n");
            if (!(memcmp(SPATntry->pCertInfo.hash, pDigest, 8))){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"SPAT Entity Certificate found in the SPAT list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
            SPATntry = (struct SPATCertInfo *)malloc(sizeof(struct SPATCertInfo));
            memcpy(SPATntry->pCertInfo.hash, pDigest, 8);
            SPATntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &SPATntry->pCertInfo.slotCert);
            memcpy(SPATntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, SPATntry->pCertInfo.slotCert.certLen);      
            SPATntry->pCertInfo.certCheckFlag = 0x00;
            SPATntry->processTime = get_cur_time2004();
            if(msgCert.certInfo[0] == 0x03){
                UINT8 pDigest[32], msgca_cert[512];
                UINT32 uiDigestLen;
                if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
#if 0
                bzero(tmp_str,256);
                sprintf(tmp_str,"%s/%s/root_ca.cert",serverPath,asmoptions.KEY_CONF_DIR);
                uiTmpBufLen = read_file(tmp_str, msgca_cert, sizeof(msgca_cert));
                MsgCAcert.certLen = certParse(msgca_cert, &MsgCAcert);
                memcpy(MsgCAcert.certInfo, msgca_cert, MsgCAcert.certLen);
#else
                cFound = 0;
                LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
                    if (!(memcmp(issuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
                         cFound = 1;
                         break;
                    }
                }
                if (!cFound){
	            *errorCode = CMD_ERR_CHAIN_ENDED_AT_UNKNOWN_ROOT;
	            goto err;
                }
                else{
                    memcpy(&MsgCAcert,&issuerntry->pCertInfo.slotCert,sizeof(MsgCAcert));
                }
#endif
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,MsgCAcert.certInfo,MsgCAcert.certLen,"Start of CACert cert of %d bytes:\n", MsgCAcert.certLen);
		    AWSEC_LOG(ASM,NULL,0,"*****  End of CACert *****\n");
		}

                if (AWSecHash(HASH_ALGO_SHA256, MsgCAcert.certInfo, MsgCAcert.certLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
			AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)pDigest,8,"***** Start of Root Cert Digest *****\n"); 
		    AWSEC_LOG(ASM,NULL,0,"***** End of Root Cert Digest *****\n");
		}
                bzero(msgca_cert, 512);
                memcpy(msgca_cert, pDigest, uiDigestLen);
                memcpy(&msgca_cert[uiDigestLen], msgCert.certInfo, msgCert.certLen);

                if (AWSecHash(HASH_ALGO_SHA256, msgca_cert, msgCert.certLen + uiDigestLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
			AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)&MsgCAcert.certInfo[MsgCAcert.verification_key],33,"CACert Verification Key length of the cert = %d\n", MsgCAcert.certLen);
		    AWSEC_LOG(ASM,NULL,0,"End of CACert Verification Key\n");
		}
                if ((pubPoint = PublicKeyReconstruct(pDigest, &msgCert.certInfo[msgCert.sig_RV], &MsgCAcert.certInfo[MsgCAcert.verification_key], pPubKey, pGroup)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:PublicKeyReconstruct(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,pPubKey,33,"Public Key constructed\n"); 
		    AWSEC_LOG(ASM,NULL,0,"End of public Key\n");
		}
                memcpy(SPATntry->pCertInfo.pubSignKey, pPubKey, 33);      
            }
            else
                memcpy(SPATntry->pCertInfo.pubSignKey, &msgCert.certInfo[msgCert.verification_key], 33);     
            LIST_INSERT_HEAD(&SPATListHead, SPATntry, SPAT_list);
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: SPAT Entity Certificate not found in the Cert Store(SPAT) List, Adding it to SPAT List\n");
        }
        else{
            memcpy(pPubKey, SPATntry->pCertInfo.pubSignKey, 33);      
            SPATntry->processTime = get_cur_time2004();
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: SPAT Entity Certificate found in the Cert Store(SPAT) List\n");
        }
    }
    else if(psID == 49136){
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"\n##### %s : %d : psID %d #####\n",__func__,__LINE__,psID);
        }
        bFound = 0;
        LIST_FOREACH(GIDntry, &GIDListHead, GID_list){
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"How many times the GID list was executed\n");
            if (!(memcmp(GIDntry->pCertInfo.hash, pDigest, 8))){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"GID Entity Certificate found in the GID list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
            GIDntry = (struct GIDCertInfo *)malloc(sizeof(struct GIDCertInfo));
            memcpy(GIDntry->pCertInfo.hash, pDigest, 8);
            GIDntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &GIDntry->pCertInfo.slotCert);
            memcpy(GIDntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, GIDntry->pCertInfo.slotCert.certLen);      
            GIDntry->pCertInfo.certCheckFlag = 0x00;
            GIDntry->processTime = get_cur_time2004();
            if(msgCert.certInfo[0] == 0x03){
                UINT8 pDigest[32], msgca_cert[512];
                UINT32 uiDigestLen;
                if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
#if 0
                bzero(tmp_str,256);
                sprintf(tmp_str,"%s/%s/root_ca.cert",serverPath,asmoptions.KEY_CONF_DIR);
                uiTmpBufLen = read_file(tmp_str, msgca_cert, sizeof(msgca_cert));
                MsgCAcert.certLen = certParse(msgca_cert, &MsgCAcert);
                memcpy(MsgCAcert.certInfo, msgca_cert, MsgCAcert.certLen);
#else
                cFound = 0;
                LIST_FOREACH(issuerntry, &issuerListHead, issuer_list){
                    if (!(memcmp(issuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
                         cFound = 1;
                         break;
                    }
                }
                if (!cFound){
	            *errorCode = CMD_ERR_CHAIN_ENDED_AT_UNKNOWN_ROOT;
                    goto err;
                }
                else{
                    memcpy(&MsgCAcert,&issuerntry->pCertInfo.slotCert,sizeof(MsgCAcert));
                }
#endif
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,MsgCAcert.certInfo,MsgCAcert.certLen,"Start of CACert cert of %d bytes:\n", MsgCAcert.certLen);
		    AWSEC_LOG(ASM,NULL,0,"*****  End of CACert *****\n");
		}

                if (AWSecHash(HASH_ALGO_SHA256, MsgCAcert.certInfo, MsgCAcert.certLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
			AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)pDigest,8,"***** Start of Root Cert Digest *****\n");  
		    AWSEC_LOG(ASM,NULL,0,"***** End of Root Cert Digest *****\n");
		}
                bzero(msgca_cert, 512);
                memcpy(msgca_cert, pDigest, uiDigestLen);
                memcpy(&msgca_cert[uiDigestLen], msgCert.certInfo, msgCert.certLen);

                if (AWSecHash(HASH_ALGO_SHA256, msgca_cert, msgCert.certLen + uiDigestLen, pDigest, &uiDigestLen, 0) == NULL){
		    if(asm_log_level >= LOG_CRITICAL)
			AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,(UINT8 *)&MsgCAcert.certInfo[MsgCAcert.verification_key],33,"CACert Verification Key length of the cert = %d\n", MsgCAcert.certLen);
		    AWSEC_LOG(ASM,NULL,0,"End of CACert Verification Key\n");
		}
                if ((pubPoint = PublicKeyReconstruct(pDigest, &msgCert.certInfo[msgCert.sig_RV], &MsgCAcert.certInfo[MsgCAcert.verification_key], pPubKey, pGroup)) == NULL){
		    if(asm_log_level >= LOG_CRITICAL){
			AWSEC_LOG(ASM,NULL,0,"%s:%d:PublicKeyReconstruct(): %s\n",__FILE__,__LINE__, strerror(errno));
		    }
                    goto err;
                }
		if(asm_log_level >= LOG_DEBUG){
		    AWSEC_LOG(ASM,pPubKey,33,"Public Key constructed\n");
		    AWSEC_LOG(ASM,NULL,0,"End of public Key\n");
		}
                memcpy(GIDntry->pCertInfo.pubSignKey, pPubKey, 33);      
            }
            else
                memcpy(GIDntry->pCertInfo.pubSignKey, &msgCert.certInfo[msgCert.verification_key], 33);     
            LIST_INSERT_HEAD(&GIDListHead, GIDntry, GID_list);
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: GID Entity Certificate not found in the Cert Store(GID) List, Adding it to GID List\n");
        }
        else{
            memcpy(pPubKey, GIDntry->pCertInfo.pubSignKey, 33);      
            GIDntry->processTime = get_cur_time2004();
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: GID Entity Certificate found in the Cert Store(GID) List\n");
        }
    }
  }
  else if ((pBuf[offset + 1] == 0x01) && (pBuf[offset + 2] == 0x02))
  {
    offset =  3;
    memcpy(pDigest,&pBuf[offset],8);
    psID = getPsidbyLen(&pBuf[offset+9],&retIDx,NULL);
    if(psID == 32){
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"##### %s : %d : psID %d #####\n",__func__,__LINE__,psID);
        }
        bFound = 0;
        LIST_FOREACH(SLntry, &SLListHead, SL_list){
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"How many times the SL list was executed\n");
            if (!(memcmp(SLntry->pCertInfo.hash, pDigest, 8))){         //Comparing digest with the existing certificate hash 
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"ShortLived Entity Certificate found in the SL list\n");
                bFound = 1;
                break;
            }
        }
	
        if (!bFound){
	    *errorCode = CMD_ERR_CERT_NOT_FOUND;
	    return FAILURE;
        }
        else {
            memcpy(&root_cert_expiry,&SLntry->pCertInfo.slotCert.certInfo[SLntry->pCertInfo.slotCert.expiration],4);
            memcpy(pPubKey, SLntry->pCertInfo.pubSignKey, 33);      
            SLntry->processTime = get_cur_time2004();
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: ShortLived Entity Certificate found in the Cert Store(SL) List\n");
        }
    }
    else if(psID == 32771){
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"##### %s : %d : psID %d #####\n",__func__,__LINE__,psID);
        }
        bFound = 0;
        LIST_FOREACH(TIMntry, &TIMListHead, TIM_list){
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"How many times the TIM list was executed\n");
            if (!(memcmp(TIMntry->pCertInfo.hash, pDigest, 8))){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"TIM Entity Certificate found in the TIM list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
	    *errorCode = CMD_ERR_CERT_NOT_FOUND;
	    return FAILURE;
        }
        else{
            memcpy(&root_cert_expiry,&TIMntry->pCertInfo.slotCert.certInfo[TIMntry->pCertInfo.slotCert.expiration],4);
            memcpy(pPubKey, TIMntry->pCertInfo.pubSignKey, 33);      
            TIMntry->processTime = get_cur_time2004();
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: TIM Entity Certificate found in the Cert Store(TIM) List\n");
        }
    }
    else if(psID == 49120){
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"##### %s : %d : psID %d #####\n",__func__,__LINE__,psID);
        }
        bFound = 0;
        LIST_FOREACH(SPATntry, &SPATListHead, SPAT_list){
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"How many times the SPAT list was executed\n");
            if (!(memcmp(SPATntry->pCertInfo.hash, pDigest, 8))){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"SPAT Entity Certificate found in the SPAT list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
	    *errorCode = CMD_ERR_CERT_NOT_FOUND;
	    return FAILURE;
        }
        else{
            memcpy(&root_cert_expiry,&SPATntry->pCertInfo.slotCert.certInfo[SPATntry->pCertInfo.slotCert.expiration],4);
            memcpy(pPubKey, SPATntry->pCertInfo.pubSignKey, 33);      
            SPATntry->processTime = get_cur_time2004();
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: SPAT Entity Certificate found in the Cert Store(SPAT) List\n");
        }
    }
    else if(psID == 49136){
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"##### %s : %d : psID %d #####\n",__func__,__LINE__,psID);
        }
        bFound = 0;
        LIST_FOREACH(GIDntry, &GIDListHead, GID_list){
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"How many times the GID list was executed\n");
            if (!(memcmp(GIDntry->pCertInfo.hash, pDigest, 8))){
    	        if(asm_log_level >= LOG_INF)
	            AWSEC_LOG(ASM,NULL,0,"GID Entity Certificate found in the GID list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
	    *errorCode = CMD_ERR_CERT_NOT_FOUND;
	    return FAILURE;
        }
        else{
            memcpy(&root_cert_expiry,&GIDntry->pCertInfo.slotCert.certInfo[GIDntry->pCertInfo.slotCert.expiration],4);
            memcpy(pPubKey, GIDntry->pCertInfo.pubSignKey, 33);      
            GIDntry->processTime = get_cur_time2004();
    	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: GID Entity Certificate found in the Cert Store(GID) List\n");
        }
    }
    if(asm_log_level >= LOG_INF){
	AWSEC_LOG(ASM,NULL,0,"Verify Certifiacte digest by comparing the known Certificate digest. Certifcate Manager should be callled here\n");
	AWSEC_LOG(ASM,NULL,0,"Get the Certificate related to certificate digest and get the public key\n");
    }
    offset += 8;
    root_cert_expiry = swap_32(root_cert_expiry);
    current_time = get_cur_time2004();
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"**********CERTIFICATE EXPIRATION TIME:%d current time:%d*******\n",root_cert_expiry,current_time);
        }
    if(root_cert_expiry <= current_time){
        *errorCode = CMD_ERR_CERT_EXPIRED;
    	if(asm_log_level >= LOG_INF)
	    AWSEC_LOG(ASM,NULL,0,"certificate expired\n");
	return FAILURE;
    }
  }
  else{
      *errorCode = CMD_ERR_INVALID_PACKET;
      return FAILURE;
  }
  signOffSet = offset;
  *tf_flag = pBuf[signOffSet];
  psID = getPsidbyLen(&pBuf[offset+1],&retIDx,NULL);
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,NULL,0,"##### %s : %d : psID %d retIDx %d #####\n",__func__,__LINE__,psID,retIDx);
  }
  offset += (1+retIDx); /** Flags and PSID (check for unauthorized psid return 0x46 if true)**/
  dataLen = decode_length((pBuf + offset), &len);
  offset += len;
  //tmp_offset = offset;
  offset += dataLen;
  *tmpOffset = offset;
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,NULL,0,"##### %s : %d : signOffSet %d : offset %d : pBuf[signOffSet] %d tmpOffset %d #####\n",__func__,__LINE__,signOffSet,offset,pBuf[signOffSet],*tmpOffset);
  }
  if(pBuf[signOffSet] & 0x02) // & mf or tf byte to know use_generation_time is enabled
  {
    if(asm_log_level >= LOG_INF)
        AWSEC_LOG(ASM,NULL,0,"Use Generation time\n");
    memcpy(&generation_time,(pBuf+offset),8);
    ret_val = validate_generation_time(generation_time,req2->message_validity_period);
    if(ret_val == 1){
        if(asm_log_level >= LOG_INF){
	    AWSEC_LOG(ASM,NULL,0,"Message is valid as per generation time\n");
        }
    }
    else if(ret_val == 2){
        if(asm_log_level >= LOG_CRITICAL)
	    AWSEC_LOG(ASM,NULL,0,"#### Message expired based on generation time(early) ####\n");
	*errorCode = CMD_ERR_MESSAGE_FUTURE_MESSAGE;
	return FAILURE;
    }
    else if(ret_val == 3){
        if(asm_log_level >= LOG_CRITICAL)
	    AWSEC_LOG(ASM,NULL,0,"#### Message expired based on generation time(late) ####\n");
	*errorCode = CMD_ERR_MESSAGE_EXPIRED_BASED_ON_GENERATION_TIME;
	return FAILURE;
    }
    else{
	//here we can add our own error code, for time being it is 0x48
	//printf("Message verification failed [generation time]");
        if(asm_log_level >= LOG_CRITICAL)
	    AWSEC_LOG(ASM,NULL,0,"#### Message verification failed [generation time] ####\n");
	*errorCode = CMD_ERR_MESSAGE_VERIFICATION_FAILED;
	return FAILURE;
    }
    offset += 8;
    offset += 1; /** Confidence **/
  }
  if(pBuf[signOffSet] & 0x04)
  {
    if(asm_log_level >= LOG_INF)
        AWSEC_LOG(ASM,NULL,0,"Use expiration time\n");
    memcpy(&expiry_time,(pBuf+offset),8);
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"##### %s : %d : Expiry time %llu #####\n",__func__,__LINE__,expiry_time);
    }
    ret_exp = validate_expiry_time(expiry_time);
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"expiry_time %llu\n",expiry_time);
    }
    if(ret_exp = 1){
       if(asm_log_level >= LOG_INF)
           AWSEC_LOG(ASM,NULL,0,"Msg Is Not Expired Yet\n"); 
    }
    else{
        if(asm_log_level >= LOG_CRITICAL)
            AWSEC_LOG(ASM,NULL,0,"Msg Is Expired based on expiry time\n");
	*errorCode = CMD_ERR_MESSAGE_EXPIRED_BASED_ON_EXPIRY_TIME;
	return FAILURE;	
    }
    offset += 8;
  }
  if(req2->require_generation_location){
      if(pBuf[signOffSet] & 0x08){
        if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"Use Location\n");
	location position; 
	UINT32 rangeMeters = req2->message_validity_distance;
	position.local_latitude = req2->local_location_latitude;
	position.local_longitude = req2->local_location_longitude;
        memcpy(&position.generation,(pBuf+offset),10);
       //     printf("GENERATION Lat:%ld Lon:%ld \n",position.generation.latitude,position.generation.longitude);
	if(BIGENDIAN){
	    position.local_latitude = ntohl(position.local_latitude);
	    position.local_longitude = ntohl(position.local_longitude);
	    position.generation.latitude = ntohl(position.generation.latitude);
	    position.generation.longitude = ntohl(position.generation.longitude);
	    rangeMeters = ntohl(rangeMeters);
            if(asm_log_level >= LOG_INF)
                AWSEC_LOG(ASM,NULL,0,"ENDIANESS CORRECTED\n");
	}
	if(position.generation.latitude != 900000001 && position.generation.longitude != 1800000001){
	    if(asm_log_level >= LOG_INF){
		AWSEC_LOG(ASM,NULL,0,"GENERATION Lat:%ld Lon:%ld \n",position.generation.latitude,position.generation.longitude);
	    }
	    if(req2->local_location_latitude != 900000001 && req2->local_location_longitude != 1800000001){
		  if(asm_log_level >= LOG_INF){
		      AWSEC_LOG(ASM,NULL,0,"LOCAL Lat:%ld Lon:%ld \n",position.local_latitude,position.local_longitude);
		  }
		  distance_cal(&position);
		  if(asm_log_level >= LOG_INF){
		      AWSEC_LOG(ASM,NULL,0,"distance:%lf range:%d\n",position.distance,rangeMeters);
		  }
		  if((UINT32)position.distance > rangeMeters){
		      if(asm_log_level >= LOG_CRITICAL)
		          AWSEC_LOG(ASM,NULL,0,"distance range exceeded\n");
		      *errorCode = CMD_ERR_MESSAGE_OUT_OF_RANGE;//later use enum values
		      return FAILURE;
		  }
		  else{
		      if(asm_log_level >= LOG_INF)
		          AWSEC_LOG(ASM,NULL,0,"LOCATION VERIFICATION DONE\n");
		  }
	    }
	    else
	        if(asm_log_level >= LOG_CRITICAL)
		    AWSEC_LOG(ASM,NULL,0,"INVALID LOCATION: LOCAL\n");
	}
	else
	    if(asm_log_level >= LOG_CRITICAL)
	        AWSEC_LOG(ASM,NULL,0,"INVALID LOCATION: REMOTE\n");
        offset += 10; /** This is supposed to be 10 **/
      }
      else
	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"use_location is disabled from generation side\n");
  }
  if(pBuf[signOffSet] & 0x10)
  {
    if(asm_log_level >= LOG_INF)
	AWSEC_LOG(ASM,NULL,0,"Use Extensions\n");
    offset += 1; /** Type **/
    /** This is variable length field **/
    dataLen = decode_length((pBuf + offset), &len);
    offset += len;
    offset += dataLen;
  }
  //offset += 1;
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,pPubKey,33,"Verification Key\n");
      AWSEC_LOG(ASM,NULL,0,"End of Verification Key\n"); 
      AWSEC_LOG(ASM,(UINT8 *) pBuf+offset,33,"\npBuf Start offset = %d, dataLen = %d\n", offset - signOffSet, dataLen );
      AWSEC_LOG(ASM,NULL,0,"pBuf End\n");
  }

  if(AWSecDigestAndVerifyData((pBuf + signOffSet), offset - signOffSet, (pBuf + offset), pPubKey) != SUCCESS)
  {
    if(asm_log_level >= LOG_CRITICAL)
	AWSEC_LOG(ASM,NULL,0,"Signature verification failed\n");
    *errorCode = CMD_ERR_MESSAGE_VERIFICATION_FAILED;//failed to verfity the signature on the message with the public key from the message signer's certificate
    return FAILURE;
  }
  else
  {
    if(asm_log_level >= LOG_INF)
        AWSEC_LOG(ASM,NULL,0,"Signature verified successfully\n");
    memcpy(SignedData,(pBuf + signOffSet), (offset - signOffSet));
    //memcpy(SignedData,(pBuf + tmp_offset), (offset - signOffSet));
    //memcpy(SignedData,(pBuf + tmp_offset), (*tmpOffset - tmp_offset));
    if(asm_log_level >= LOG_DEBUG){
	AWSEC_LOG(ASM,NULL,0,"##### %s : %d : (offset - signOffSet) %d #####\n",__func__,__LINE__,(offset - signOffSet));
	AWSEC_LOG(ASM,SignedData,(offset-signOffSet),"***** Start Of Unsigned Data *****\n");
        AWSEC_LOG(ASM,NULL,0,"***** End Of Unsigned Data *****\n");
    }
  }
  return (offset - signOffSet);

err:
    if(pubPoint)
        EC_POINT_clear_free(pubPoint);
    if(pGroup)
        EC_GROUP_clear_free(pGroup);
    return FAILURE;
}


//[NAZEER] key info should be passed to this function by adding argument if need.
int32_t EncryptedDataIn16092(UINT32 cmd,uint8_t *buff16092, uint8_t *pInData, int32_t InDataLen, uint8_t *certData ,UINT32 checkFlag, uint8_t *errorCode)
{
  INT32 uiRetLen, i, outlen;
  UINT32 uiDigestLen,  uiCipherLen;
  UINT8 pDigest[32], *pCipherData= NULL;
  INT32 uiTagLen = 16, outBufLen,ENCoffset=0, uiTmpBufLen;
  UINT32 KeyLen, CipherLen, macLen;
  UINT8 TmpAESSymKey[16 + 1] = "", TmpAESSymKeyNonce[12 + 1] = "";
  UINT8 pKey[33], pCipher[sizeof(TmpAESSymKey)], pMac[SHA256_DIGEST_LENGTH];
  char logstring[250];
  uint32_t tmpPadLen=0;
  uint8_t pPadInData[InDataLen + 16];
  certStore *curCertInfo = (certStore*)certData;
  //final .2 format
  if(checkFlag)
      ENCoffset = 4; //[NAZEER]to fill length of whole .2 message
  
  buff16092[ENCoffset] = 2; //protocol_version
  ENCoffset += 1;
  buff16092[ENCoffset] = 2; //encrypted (2)
  ENCoffset += 1;
   

  if(curCertInfo->certCheckFlag & IS_CERT_REVOKED){
      *errorCode = CMD_ERR_FAIL_ON_ALL_CERTS;
      return FAILURE;
  }
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,curCertInfo->slotCert.certInfo,curCertInfo->slotCert.certLen,"***** Start Of Cert INFO *****\n");
      AWSEC_LOG(ASM,NULL,0,"*****  End Of Cert Info  *****\n");
  }


  if(!checkFlag){
      tmpPadLen = InDataLen%16;
      if(tmpPadLen != 0){
          tmpPadLen = 16 - tmpPadLen;
	  memset(pPadInData,0,(InDataLen + 16));
          memcpy(&pPadInData[tmpPadLen],pInData,InDataLen);
          InDataLen += tmpPadLen;
      }
  }	
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,NULL,0,"##### %s : %d : InDataLen %d #####\n", __func__, __LINE__, InDataLen);
      AWSEC_LOG(ASM,pPadInData,InDataLen,"***** Start Of Data Before Encryption *****\n");
      AWSEC_LOG(ASM,NULL,0,"*****  End Of Data Before Encryption  *****\n");
  }
//EncryptedData(page 73 in d9_3)
  buff16092[ENCoffset] = 0; //SymmAlgorithm->aes_128_ccm (0)
  ENCoffset += 1;
  encode_length(&buff16092[ENCoffset], 8 + 33 + 16 +20, &uiRetLen);
  ENCoffset += uiRetLen;
  //if (AWSecHash(HASH_ALGO_SHA256, RACert.certInfo, RACert.certLen, pDigest, &uiDigestLen, 8) == NULL)
  if (AWSecHash(HASH_ALGO_SHA256, curCertInfo->slotCert.certInfo, curCertInfo->slotCert.certLen, pDigest, &uiDigestLen, 8) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL)
	AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
    return FAILURE;
  }

  memcpy(&buff16092[ENCoffset], pDigest, 8);
  ENCoffset += 8;
  /** Copy ephemeral Public Key computed in the ECIES encryption **/

  RAND_bytes(TmpAESSymKey, 16);
  RAND_bytes(TmpAESSymKeyNonce, 12);
  {

  //if (AWSec_ecies_encrypt(&RACert.certInfo[RACert.encryption_key], TmpAESSymKey, 16, pKey, &KeyLen, pCipher, &CipherLen, pMac, &macLen, 0, CIPHER_ALGO_AES128_ECB) == FAILURE)
  if (AWSec_ecies_encrypt(&curCertInfo->slotCert.certInfo[curCertInfo->slotCert.encryption_key], TmpAESSymKey, 16, pKey, &KeyLen, pCipher, &CipherLen, pMac, &macLen, 0, CIPHER_ALGO_AES128_ECB, errorCode) == FAILURE)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSec_ecies_encrypt(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    return FAILURE;
  }
  else
  {
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"Encrypt using ECIES is successful\n");
        AWSEC_LOG(ASM,NULL,0,"Encrypt key length = %d, cipher data length = %d, mac len = %d\n", KeyLen, CipherLen, macLen);
    }
  }

  memcpy(&buff16092[ENCoffset], pKey, KeyLen);
  ENCoffset += KeyLen;
  memcpy(&buff16092[ENCoffset], pCipher, CipherLen);
  ENCoffset += CipherLen;
  memcpy(&buff16092[ENCoffset], pMac, 20 /* We need only first 20 bytes of the HMAC SHA256 digest no need to secure_mac_length(ciphered) */);
  ENCoffset += 20;
  }
  pCipherData = (UINT8 *)malloc(InDataLen + 1024);
  if(checkFlag){
      if(AWSecEncrypt(CIPHER_ALGO_AES128_CCM, TmpAESSymKey, TmpAESSymKeyNonce, 12, pInData, InDataLen, pCipherData, &uiCipherLen, 16) != SUCCESS)
      {
          if(asm_log_level >= LOG_CRITICAL)
	      AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecEncrypt(): %s\n",__FILE__,__LINE__, strerror(errno));
	  free(pCipherData);
          return FAILURE;
      }
      else
          if(asm_log_level >= LOG_INF)
              AWSEC_LOG(ASM,NULL,0,"Encryption successful\n");
  }
  else{
      if(AWSecEncrypt(CIPHER_ALGO_AES128_CCM, TmpAESSymKey, TmpAESSymKeyNonce, 12, pPadInData, InDataLen, pCipherData, &uiCipherLen, 16) != SUCCESS)
      {
        if(asm_log_level >= LOG_CRITICAL)
	    AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecEncrypt(): %s\n",__FILE__,__LINE__, strerror(errno));
	free(pCipherData);
        return FAILURE;
      }
      else
          if(asm_log_level >= LOG_INF)
              AWSEC_LOG(ASM,NULL,0,"Encryption successful\n");
  }
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"uiCipherLen = %ld\n", uiCipherLen);
  }
  memcpy(&buff16092[ENCoffset], TmpAESSymKeyNonce, 12);
  ENCoffset += 12;
  encode_length(&buff16092[ENCoffset], uiCipherLen, &uiRetLen); 
  ENCoffset += uiRetLen;
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"uiRetLen = %lu\n", uiRetLen);
  }
  memcpy(&buff16092[ENCoffset], pCipherData, uiCipherLen);
  ENCoffset += uiCipherLen;
  if(asm_log_level >= LOG_DEBUG){
    AWSEC_LOG(ASM,NULL,0,"Encode length bytes uiRetLen = %lu, ENCoffset = %lu, uiCipherLen = %lu\n", uiRetLen, ENCoffset, uiCipherLen);
    AWSEC_LOG(ASM,pCipherData,uiCipherLen,"Encrypted Data: \n");
    AWSEC_LOG(ASM,NULL,0,"End of Encrypted Data\n");
    
  }
#ifdef NOCODE
  {
  UINT8 TmpDecData[1024];
  INT32 DataOutLen;
  if(AWSecDecrypt(CIPHER_ALGO_AES128_CCM, TmpAESSymKey, TmpAESSymKeyNonce, 12, pCipherData, uiCipherLen, TmpDecData, &DataOutLen, 16) != SUCCESS)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecDecrypt(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    return FAILURE;
  }
  else
  {
      if(asm_log_level >= LOG_DEBUG){
          AWSEC_LOG(ASM,TmpDecData,DataOutLen,"Decryption successful -- Decrypted Data:\n");
          AWSEC_LOG(ASM,NULL,0,"End of Decrypted Data\n");
      }
  }

  }
#endif
    free(pCipherData);
  //encode total length into 1st 4bytes
    if(checkFlag)
        BUFPUT32(buff16092, ENCoffset - 4);
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,buff16092,ENCoffset,"%s: Encrypted\n",StageString[cmd]); 
    }
  return ENCoffset;
}

int32_t SignWSAMessage(UINT32 cmd, uint8_t *SignedData, uint8_t encContentType, uint8_t identifierType, uint8_t *certData, uint8_t *ToBeSignedDataBuf, int32_t dataLen, UINT8 *pPrivKeyBuf, INT32 GroupType, UINT8 *pOptData, UINT32 OptLen,uint8_t sign_with_fast_verification,uint8_t *errorCode,AWSecMsg_Sign_WSA_Req2_t *req2)
{
  INT32 uiTmpBufLen,  bFound = 0;
  UINT32 uiDigestLen,uiSignDataLen;
  int32_t SIGNoffset = 0, uiRetLen, offset, i = 0,yinfo = 0;
  UINT8 pDigest[SHA256_DIGEST_LENGTH], pSignData[256];
  ECDSA_SIG *pSig = NULL;
  EC_KEY *ECKey =NULL;
  struct WSAissuerCertInfo *WSAissuerntry;
  struct WSAissuerCertInfo *WSAcertNo[10];
  int32_t WSAnoOfCert =0;
  uint32_t WSAcertChainLen =0,j = 0;
  UINT8 *pTmpBuf = NULL;
  uint8_t permissionIndicesLen =0 ,permissionIndices[req2->number_of_permissions] ,permOffset = 0 , priority = 0, sspString = -1 , psidIsThere = 0, psidNo = 0;
  uint32_t psID =0 , retIDx = 0;
  //BIGNUM *in_kinv = NULL, *in_r = NULL; 
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"identifierType = %d\n", identifierType);
  }
  if(encContentType != 0xff)
  {
    //start Encrypted Message		
    SignedData[SIGNoffset] = encContentType; //ToBeEncrypted->ContentType
    SIGNoffset += 1;
  }
  //start Signed Message		
  SignedData[SIGNoffset] = identifierType; //SignerIdentifierType->certificate
  SIGNoffset += 1;
  certStore *curCertInfo = (certStore*)certData;

  if(identifierType == certificate_digest_with_ecdsap256)
  {
      if(asm_log_level >= LOG_INF)
          AWSEC_LOG(ASM,NULL,0,"Got the certifcate digest\n");
    if(!(curCertInfo->certCheckFlag & IS_CERT_REVOKED))
    {
       memcpy((SignedData + SIGNoffset), curCertInfo->hash, 8);
       SIGNoffset += 8;
    }
    else
    {
        if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"Invalid Slot WSA certificate received to calculate the Certificate digest\n");
        *errorCode = CMD_ERR_INVALID_INPUT;
        return FAILURE;
    }
  }
  else if (identifierType == certificate)
  {
      if(asm_log_level >= LOG_INF)
          AWSEC_LOG(ASM,NULL,0,"Got the certifcate type\n");
    if(!(curCertInfo->certCheckFlag & IS_CERT_REVOKED))
    {
       memcpy((SignedData + SIGNoffset), curCertInfo->slotCert.certInfo, curCertInfo->slotCert.certLen);
       SIGNoffset += curCertInfo->slotCert.certLen;
    }
    else
    {
        if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"Invalid Slot WSA certificate received to calculate the Certificate\n");
        *errorCode = CMD_ERR_INVALID_INPUT;
        return FAILURE;
    }
  }
  else if (identifierType == certificate_chain)
  {
    /** Certificate Chain needs to constructed and accessed to copy the certificate chain **/
    //Certificate *cert = (Certificate *)certData;
      if(asm_log_level >= LOG_INF)
          AWSEC_LOG(ASM,NULL,0,"Got the certificate chain\n");
#if 0
    certStore *cert = (certStore *)certData;
    bFound = 0;
    LIST_FOREACH(WSAissuerntry, &WSAissuerListHead, WSAissuer_list){
        printf("How many times the list was executed\n");
        //if (!(memcmp(WSAissuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
        if (!(memcmp(WSAissuerntry->pCertInfo.hash, &cert->slotCert.certInfo[cert->slotCert.signer_id], 8))){
            printf("Root Certificate found in the list\n");
            bFound = 1;
            break;
        }
    }
    if (!bFound){
        *errorCode = CMD_ERR_CERT_NOT_FOUND;
        goto err;
    }
    else{
        WSAcertNo[WSAnoOfCert] = WSAissuerntry;
        WSAcertChainLen += WSAissuerntry->pCertInfo.slotCert.certLen;
        //WSAcertChainLen += WSAcertNo[WSAnoOfCert]->pCertInfo.slotCert.certLen;
        WSAissuerntry->processTime = get_cur_time2004();
        printf("NAZEER: Certificate found in the Cert Store List\n");    
    }

    while(WSAcertNo[WSAnoOfCert]->pCertInfo.slotCert.certInfo[1] != root_ca){
        bFound = 0;
        LIST_FOREACH(WSAissuerntry, &WSAissuerListHead, WSAissuer_list){
            printf("How many times the list was executed\n");
            //if (!(memcmp(WSAissuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
            if (!(memcmp(WSAissuerntry->pCertInfo.hash, &WSAcertNo[WSAnoOfCert]->pCertInfo.slotCert.certInfo[WSAcertNo[WSAnoOfCert]->pCertInfo.slotCert.signer_id], 8))){
                printf("Root Certificate found in the list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
            *errorCode = CMD_ERR_CERT_NOT_FOUND;
            goto err;
        }
        else{
            WSAnoOfCert += 1;
            WSAcertNo[WSAnoOfCert] = WSAissuerntry;
            WSAcertChainLen += WSAissuerntry->pCertInfo.slotCert.certLen;
            WSAissuerntry->processTime = get_cur_time2004();
            printf("NAZEER: Certificate found in the Cert Store List\n");    
        }
    }
    WSAcertChainLen += cert->slotCert.certLen;
    encode_length(&SignedData[SIGNoffset], WSAcertChainLen, &uiRetLen);
    SIGNoffset += uiRetLen;
    while(WSAnoOfCert >= 0){
        if(debugPrints){
            {
                int i =0;
                printf("\n*************** Start Of WSAcertNo[WSAnoOfCert]->pCertInfo.slotCert.certInfo ***************\n");
                for(i =0 ;i< WSAcertNo[WSAnoOfCert]->pCertInfo.slotCert.certLen;i++)
                    printf("%02x ",WSAcertNo[WSAnoOfCert]->pCertInfo.slotCert.certInfo[i]);
                printf("\n*************** End Of WSAcertNo[WSAnoOfCert]->pCertInfo.slotCert.certInfo ***************\n");
            }
        }  
        memcpy((SignedData + SIGNoffset),WSAcertNo[WSAnoOfCert]->pCertInfo.slotCert.certInfo,WSAcertNo[WSAnoOfCert]->pCertInfo.slotCert.certLen );
        SIGNoffset += WSAcertNo[WSAnoOfCert]->pCertInfo.slotCert.certLen;
        WSAnoOfCert -=1; 
    } 
    //memcpy((SignedData + SIGNoffset), cert->certInfo, cert->certLen); 
    //SIGNoffset += cert->certLen;
    memcpy((SignedData + SIGNoffset), cert->slotCert.certInfo, cert->slotCert.certLen); 
    SIGNoffset += cert->slotCert.certLen;
    printf("dataLen = %d, cert->certLen = %d, SIGNoffset = %d\n", dataLen, cert->slotCert.certLen, SIGNoffset);
#else
    if(!(curCertInfo->certCheckFlag & IS_CERT_REVOKED))
    {
       encode_length(&SignedData[SIGNoffset], curCertInfo->slotCert.certLen, &uiRetLen);
       SIGNoffset += uiRetLen;
        if(asm_log_level >= LOG_INF){
      	    AWSEC_LOG(ASM,NULL,0,"dataLen = %d, curCertInfo->slotCert.certLen = %d, SIGNoffset = %d\n", dataLen, curCertInfo->slotCert.certLen, SIGNoffset);
  	}
       //SignedData[SIGNoffset] = curCertInfo->slotCert.certLen;
       memcpy((SignedData + SIGNoffset), curCertInfo->slotCert.certInfo, curCertInfo->slotCert.certLen);
       SIGNoffset += curCertInfo->slotCert.certLen;
    }
    else
    {
        if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"Invalid Slot WSA certificate received to calculate the Certificate\n");
        *errorCode = CMD_ERR_INVALID_INPUT;
        return FAILURE;
    }
    if(asm_log_level >= LOG_INF)
        AWSEC_LOG(ASM,NULL,0,"dataLen = %d, curCertInfo->slotCert.certLen = %d, SIGNoffset = %d\n", dataLen, curCertInfo->slotCert.certLen, SIGNoffset);
#endif
  }
  else{
      *errorCode = CMD_ERR_INVALID_PACKET;
      return FAILURE;
  }
  offset = SIGNoffset;
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,NULL,0,"\n##### %s : %d : req2->number_of_permissions %d #####\n",__func__,__LINE__,req2->number_of_permissions);
  }
  for(i = 0 ; i < req2->number_of_permissions ; i++){
      psID = getPsidbyLen(&req2->permissions[permOffset],&retIDx,NULL);
      permOffset += retIDx ;
      priority = req2->permissions[permOffset];
      permOffset += 1; 
      sspString = req2->permissions[permOffset];
      permOffset += 1; 
      for(j = 0 ; j < NO_OF_PSID_PERMISSION ; j++){
          if(asm_log_level >= LOG_DEBUG){
              AWSEC_LOG(ASM,NULL,0,"##### %s : %d : psID %02x : psidPermission.WSApsid[%d] %02x #####\n",__func__,__LINE__,psID,j,psidPermission.WSApsid[j]);
              AWSEC_LOG(ASM,NULL,0,"##### %s : %d : priority %02x : psidPermission.WSApriority[%d] %02x #####\n",__func__,__LINE__,priority,j,psidPermission.WSApriority[j]);
              AWSEC_LOG(ASM,NULL,0,"##### %s : %d : sspString %02x : psidPermission.WSAsspString[%d] %02x #####\n",__func__,__LINE__,sspString,j,psidPermission.WSAsspString[j]);
          }
          if(psID == psidPermission.WSApsid[j] && priority == psidPermission.WSApriority[j] && sspString == psidPermission.WSAsspString[j]){
              psidNo += 1;
              psidIsThere = 1 ; 
              break;
          }
      }
      if(psidIsThere){
         permissionIndicesLen +=1;
         permissionIndices[i] = j+1;
         //permissionIndices[i] = j; 
      }
  }
  if(psidNo != req2->number_of_permissions){
      *errorCode = CMD_ERR_INCONSISTENT_PERMISSIONS;
       goto err;
  }
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,NULL,0,"##### %s : %d : permissionIndicesLen %d #####\n",__func__,__LINE__,permissionIndicesLen);
      for(i = 0 ; i < req2->number_of_permissions ; i++){
          AWSEC_LOG(ASM,NULL,0,"##### %s : %d : permissionIndices[%d] %d #####\n",__func__,__LINE__,i,permissionIndices[i]);
      }
  }
  
  SignedData[SIGNoffset++] = permissionIndicesLen;
  for(i = 0 ; i < psidNo ; i++)
     SignedData[SIGNoffset++]= permissionIndices[i];
  SignedData[SIGNoffset++] = pOptData[0];
//  SignedData[SIGNoffset++] = 35; /** PSID **/
  encode_length(&SignedData[SIGNoffset], dataLen, &uiRetLen);
  SIGNoffset += uiRetLen;
  memcpy((SignedData + SIGNoffset), ToBeSignedDataBuf, dataLen); 
  SIGNoffset += dataLen;
  if (OptLen > 1)
  {
    memcpy((SignedData + SIGNoffset), &pOptData[1], OptLen -1);
    SIGNoffset += OptLen - 1;
  }
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"Data sent for Digest = %d\n", SIGNoffset - offset);
  }
  if (AWSecHash(HASH_ALGO_SHA256, &SignedData[offset], SIGNoffset - offset, pDigest, &uiDigestLen, 0) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL)
	AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
    goto err;
  }
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,curCertInfo->privSignKey,32,"***** Start Of curCertInfo->privSignKey *****\n");
      AWSEC_LOG(ASM,NULL,0,"*****  End Of curCertInfo->privSignKey  *****\n");
  }
  
  if (GroupType == ECDSA_NISTP_256)
  {
#ifdef NOCODE
     EC_GROUP *pGroup;
      if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL)

         {

	    if(asm_log_level >= LOG_CRITICAL){
		AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
	    }
            goto err;

         }
     ReconstructPub(curCertInfo->privSignKey,pGroup);
#endif
    if (!(ECKey = ECKeyFromPrivKey(ECDSA_NISTP_256, NULL, curCertInfo->privSignKey, 32)))
    {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%d:ECKeyFromPrivKey(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
  }
  else if(GroupType == ECDSA_NISTP_224)
  {
    if (!(ECKey = ECKeyFromPrivKey(ECDSA_NISTP_224, NULL, curCertInfo->privSignKey, 24)))
    {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%d:ECKeyFromPrivKey(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
  }
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,pDigest,32,"pDigest Data*\n");
      AWSEC_LOG(ASM,NULL,0,"End pDigest Data\n");
  }
if(asm_log_level >= LOG_INF)
    AWSEC_LOG(ASM,NULL,0,"SIGN-1.1\n");
  if(sign_with_fast_verification){
      //if(!(pSig = Fast_ecdsa_do_sign(pDigest, uiDigestLen,in_kinv,in_r, ECKey,&yinfo ))){
      if(!(pSig = Fast_ecdsa_do_sign(pDigest, uiDigestLen,NULL,NULL, ECKey,&yinfo ))){
          if(asm_log_level >= LOG_CRITICAL){
              AWSEC_LOG(ASM,NULL,0,"%s:%d:Fast_ecdsa_do_sign(): %s\n",__FILE__,__LINE__, strerror(errno));
              AWSEC_LOG(ASM,SignedData,SIGNoffset,"%s: Couldn't sign the data\n",StageString[cmd]);
 	  }
          goto err;
      } 
      SignedData[SIGNoffset++] = yinfo;
      if(asm_log_level >= LOG_DEBUG){
          AWSEC_LOG(ASM,NULL,0,"##### %s : %d : sign_with_fast_verification %d #####yinfo = %x\n",__func__,__LINE__,sign_with_fast_verification, yinfo);
      }
  }
  else{
      if(!(pSig = AWSecSignData(pDigest, uiDigestLen, ECKey, pSignData, &uiSignDataLen))){
	  if(asm_log_level >= LOG_CRITICAL){
	      AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecSignData(): %s\n",__FILE__,__LINE__, strerror(errno));
              AWSEC_LOG(ASM,SignedData,SIGNoffset,"%s: Couldn't sign the data\n",StageString[cmd]);
 	  }
          goto err;
      }
      SignedData[SIGNoffset++] = 0x00; //SaveKeys.pStaticKeyPub[0]; //NAZEER; //EccPublicKeyType+R+S. calculated over[from 'start ToBeSignedAnonymousCertRequestReq' to end 'ToBeSignedAnonymousCertRequestReq'], using the private key corresponding to the public verification key in CSR certificate from bootstarp resp.
  }
if(asm_log_level >= LOG_INF)
    AWSEC_LOG(ASM,NULL,0,"SIGN-1.2\n");
  if (cmd == ANONCERTREQ_REQ)
  {
    memcpy(SaveKeys.DigestInfo, pDigest, uiDigestLen);
  }
  else if (cmd == ANONCERTREQ_STS_REQ)
  {
    memcpy(SaveKeys.pStatusReqDigestInfo, pDigest, uiDigestLen);
  }
  else if (cmd == DECRYPTIONKEY_REQ)
  {
    memcpy(SaveKeys.pDecryptDigestInfo, pDigest, uiDigestLen);
  }
  else if (cmd == MISBEHAVIOURREPORT_REQ)
  {
    memcpy(SaveKeys.pMisBehaveDigestInfo, pDigest, uiDigestLen);
  }
  else if (cmd == CRL_REQ)
  {
    memcpy(SaveKeys.pCrlDigestInfo, pDigest, uiDigestLen);
  }
  if(asm_log_level >= LOG_INF)
      AWSEC_LOG(ASM,NULL,0,"SIGN-1.3\n");
  pTmpBuf = BN_bn2hex(pSig->r);
  uiTmpBufLen = HexStr2ByteStr(pTmpBuf, &SignedData[SIGNoffset], 32);
  OPENSSL_free(pTmpBuf);
  SIGNoffset += uiTmpBufLen;
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"uiTmpBufLen = %lu\n", uiTmpBufLen);
  }
  pTmpBuf = BN_bn2hex(pSig->s);
  uiTmpBufLen = HexStr2ByteStr(pTmpBuf, &SignedData[SIGNoffset], 32);
  OPENSSL_free(pTmpBuf);
  SIGNoffset += uiTmpBufLen;
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"uiTmpBufLen = %lu\n", uiTmpBufLen);
  }
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"ToBeSigned data length = %u\n", SIGNoffset);
  }
  //end Signed Message		
  if(pSig)
    ECDSA_SIG_free(pSig);
  if(ECKey)
    EC_KEY_free(ECKey);
  return SIGNoffset;
err:
  if(pSig)
    ECDSA_SIG_free(pSig);
  if(ECKey)
    EC_KEY_free(ECKey);
    return FAILURE;
}

#ifdef NOCODE
void ReconstructPub(UINT8 * pPrivKey, EC_GROUP *pGroup)

{

  BIGNUM *bn1 = NULL;

  EC_POINT *point = NULL, *pTmpPoint3 = NULL;

  bn1 = BN_bin2bn(pPrivKey, 32, BN_new());

  {

    if(!(pTmpPoint3 = EC_POINT_new(pGroup)))

    {

      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_new(): %s\n",__FILE__,__LINE__, strerror(errno));
      }

      goto err;

    }

    if ((point = EC_GROUP_get0_generator(pGroup)) == NULL)

    {

      if(asm_log_level >= LOG_CRITICAL){
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_GROUP_get0_generator(): %s\n",__FILE__,__LINE__, strerror(errno));
      }

      goto err;

    }

    if(!EC_POINT_mul(pGroup, pTmpPoint3, bn1, point, NULL, NULL))

    {

      if(asm_log_level >= LOG_CRITICAL){
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_mul(): %s\n",__FILE__,__LINE__, strerror(errno));
      }

      goto err;

    }

    {

    UINT32 uiPubKeyLen = 33, i;

    UINT8 pPubKey[40];

    uiPubKeyLen = EC_POINT_point2oct(pGroup, pTmpPoint3, POINT_CONVERSION_COMPRESSED, pPubKey, uiPubKeyLen, NULL);

    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"Public Key regenerated using Private Key and Generator\n");
        AWSEC_LOG(ASM,pPubKey,uiPubKeyLen,"Public Key Starts:\n");
        AWSEC_LOG(ASM,NULL,0,"End of Public Key\n");
    }
    }

  }

 err:

  if(asm_log_level >= LOG_INF)
      AWSEC_LOG(ASM,NULL,0,"NAZEER: end of the reconstruction function\n");

  return;

}
#endif

int32_t VerifyWSASignedDataIn16092(UINT32 cmd, uint8_t *SignedData, uint8_t encContentType, uint8_t identifierType, uint8_t *pBuf, int32_t dataLen, INT32 GroupType,uint8_t *errorCode,AWSecMsg_Verify_WSA_Req2_t *req2)
{
  INT32 uiTmpBufLen,  len, bFound = 0 , cFound = 0, perm_offset = 0;
  UINT32 uiSignDataLen,uiDigestLen;
  struct WSAissuerCertInfo *WSAissuerntry;
  struct tempCertInfo *tempntry;
  struct WSACertInfo *WSAntry;
  WSApsidPermission psidPermission_rx;
  int32_t offset = 0, signOffSet, CertSignDataLen, i = 0 , j = 0,ret_exp;
  uint8_t permissionIndicesLen =0 ,permissionIndices ,permOffset = 0 , priority = 0, sspString = -1 , psidIsThere = 0, psidNo = 0;
  uint32_t psID =0 , retIDx = 0;
  uint8_t ret_val=5;
  EC_GROUP *pGroup;
  EC_POINT *pubPoint;
  char tmp_str[256];
  UINT8 pDigest[SHA256_DIGEST_LENGTH], pSignData[256], pPubKey[33],certType=0;
  ECDSA_SIG *pSig = NULL;
  EC_KEY *ECKey =NULL;
  UINT8 logstring[250],wsaca_cert[512];
  Time64 generation_time = 0,expiry_time = 0;
  Certificate msgCert,WSACAcert ;
  Time32 root_cert_expiry=0,current_time=0,cert_expiry=0;
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,NULL,0,"##### %s : %d : Offset %02x : Certificate Type %d #####\n",__func__,__LINE__,pBuf[offset + 1],pBuf[offset + 2]);
  }
  certType = pBuf[offset + 2];
  if ((pBuf[offset + 1] == 0x0b) && (pBuf[offset + 2] == 0x04) )
  {
    offset = 3;
    dataLen = decode_length((pBuf + offset), &len);
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"##### %s : %d : offset %d : Data Length %d : len %d #####\n",__func__,__LINE__,offset,dataLen,len);
    }
    offset += len;
    if(!((pBuf[offset+1] == wsa) || (pBuf[offset+1] == wsa_csr) || (pBuf[offset+1] == wsa_ca) || (pBuf[offset+1] == root_ca))){
        *errorCode = CMD_ERR_INCORRECT_SIGNING_CERT_TYPE;
        return FAILURE;
    }
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"dataLen = %d, len = %d\n", dataLen, len);
    }
/** We should call Import Cert Chain function to add all the certificates in the chain **/
    msgCert.certLen = certParse((pBuf + offset), &msgCert);
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"##### %s : %d : offset %d : msgCert.certLen %d #####\n",__func__,__LINE__,offset,msgCert.certLen);
    }
    memcpy(msgCert.certInfo, (pBuf + offset), msgCert.certLen);
    memcpy(&root_cert_expiry,&msgCert.certInfo[msgCert.expiration],4);
    root_cert_expiry = swap_32(root_cert_expiry);
    current_time = get_cur_time2004();
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"**********CERTIFICATE EXPIRATION TIME:%d current time:%d*******\n",root_cert_expiry,current_time);
    }
    if(root_cert_expiry <= current_time){
        *errorCode = CMD_ERR_CERT_EXPIRED;
        if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"certificate expired\n");
	return FAILURE;
    }
    if (AWSecHash(HASH_ALGO_SHA256, (pBuf + offset), msgCert.certLen, pDigest, &uiDigestLen, 8) == NULL)
    {
      if(asm_log_level >= LOG_CRITICAL)
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
      return FAILURE;
    }
    if((pBuf[offset+1] == root_ca)){                    //Checking if root_ca is in issuerList ,if not adding it to issuerList  
        bFound = 0;
        LIST_FOREACH(WSAissuerntry, &WSAissuerListHead, WSAissuer_list){
            if(asm_log_level >= LOG_INF)
                AWSEC_LOG(ASM,NULL,0,"How many times the WSA Issuer list was executed\n");
            if (!(memcmp(WSAissuerntry->pCertInfo.hash, pDigest, 8))){
		if(asm_log_level >= LOG_INF)
		    AWSEC_LOG(ASM,NULL,0,"Root Certificate found in the WSA Issuer list\n");
                bFound = 1;
                break;
            }
        }
        if (!bFound){
            WSAissuerntry = (struct WSAissuerCertInfo *)malloc(sizeof(struct WSAissuerCertInfo));
            memcpy(WSAissuerntry->pCertInfo.hash, pDigest, 8);
            WSAissuerntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &WSAissuerntry->pCertInfo.slotCert);
            memcpy(WSAissuerntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, WSAissuerntry->pCertInfo.slotCert.certLen);
            WSAissuerntry->pCertInfo.certCheckFlag = 0x00;
            WSAissuerntry->processTime = get_cur_time2004();
            LIST_INSERT_HEAD(&WSAissuerListHead, WSAissuerntry, WSAissuer_list);
	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: Root Certificate not found in the Cert Store(WSA Issuer) List, Adding it to WSA Issuer List\n");
        }
        else{
            WSAissuerntry->processTime = get_cur_time2004();
	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,NULL,0,"NAZEER: Root Certificate found in the Cert Store(WSA Issuer) List\n");
        }
    }

    if(asm_log_level >= LOG_INF)
	AWSEC_LOG(ASM,NULL,0,"Check WSA Certificate chain root Certificate. If the root Certificate is not found, then we need to return from here.\n");
    if(msgCert.certInfo[0] != 0x03)
        memcpy(pPubKey, &msgCert.certInfo[msgCert.verification_key], 33); 
    else{
        memcpy(pPubKey, &msgCert.certInfo[msgCert.sig_RV], 33); 
    }
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,pPubKey,33,"Verification Key Root 1\n");
        AWSEC_LOG(ASM,NULL,0,"End of Verification Key Root 1\n");
    }
    offset += msgCert.certLen;
    dataLen -= msgCert.certLen;
#if 0 
    if(dataLen == 0)
    {
        *errorCode = CMD_ERR_CHAIN_ENDED_AT_UNKNOWN_ROOT;
        return FAILURE;
    }
#endif
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"##### %s : %d : offset %d : msgCert.certLen %d : Data Length %d #####\n",__func__,__LINE__,offset,msgCert.certLen,dataLen);
    }
    while (dataLen > 0)
    {
      msgCert.certLen = certParse((pBuf + offset), &msgCert);
      memcpy(msgCert.certInfo, (pBuf + offset), msgCert.certLen);
      memcpy(&cert_expiry,&msgCert.certInfo[msgCert.expiration],4);
      cert_expiry = swap_32(cert_expiry);
      current_time = get_cur_time2004();
      if(asm_log_level >= LOG_INF){
          AWSEC_LOG(ASM,NULL,0,"**********CERTIFICATE EXPIRATION TIME:%d current time:%d*******\n",root_cert_expiry,current_time);
      }
      if(cert_expiry <= current_time){
          *errorCode = CMD_ERR_CERT_IN_CHAIN_EXPIRED;
          if(asm_log_level >= LOG_INF)
              AWSEC_LOG(ASM,NULL,0,"certificate expired\n");
	  return FAILURE;
      }
      if(memcmp(pDigest, (pBuf + offset + 3), 8))
      {
	*errorCode = CMD_ERR_COULD_NOT_CONSTRUCT_CHAIN;
        if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"Could not construct chain\n");
        return FAILURE;
      }
      if (AWSecHash(HASH_ALGO_SHA256, (pBuf + offset), msgCert.certLen, pDigest, &uiDigestLen, 8) == NULL)
      {
        if(asm_log_level >= LOG_CRITICAL)
	    AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
        return FAILURE;
      }
      if(asm_log_level >= LOG_DEBUG){
          AWSEC_LOG(ASM,(UINT8 *)pDigest,uiDigestLen,"***** Start of pDigest *****\n");
          AWSEC_LOG(ASM,NULL,0,"***** End of pDigest *****\n");
      }
      if((dataLen - msgCert.certLen) != 0){
          LIST_INIT(&tempListHead);
          bFound = 0;
          LIST_FOREACH(tempntry, &tempListHead, temp_list){
              if(asm_log_level >= LOG_INF)
                  AWSEC_LOG(ASM,NULL,0,"How many times the Temp(WSA) list was executed\n");
              if (!(memcmp(tempntry->pCertInfo.hash, pDigest, 8))){
                  if(asm_log_level >= LOG_INF)
                      AWSEC_LOG(ASM,NULL,0,"CA Certificate found in the Temp(WSA) list\n");
                  bFound = 1;
                  break;
              }
          }
          if (!bFound){
              tempntry = (struct tempCertInfo *)malloc(sizeof(struct tempCertInfo));
              memcpy(tempntry->pCertInfo.hash, pDigest, 8);
              tempntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &tempntry->pCertInfo.slotCert);
              memcpy(tempntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, tempntry->pCertInfo.slotCert.certLen);
              tempntry->pCertInfo.certCheckFlag = 0x00;
              tempntry->processTime = get_cur_time2004();
              LIST_INSERT_HEAD(&tempListHead, tempntry, temp_list);
              if(asm_log_level >= LOG_INF)
                  AWSEC_LOG(ASM,NULL,0,"NAZEER: CA Certificate not found in the Cert Store(Temp) List, Adding it to Temp(WSA) List \n");
          }
          else{
              tempntry->processTime = get_cur_time2004();
              if(asm_log_level >= LOG_INF)
                  AWSEC_LOG(ASM,NULL,0,"NAZEER: CA Certificate found in the Cert Store(Temp) List\n");
          }
      }
      if(msgCert.certInfo[0] == 0x02){
          CertSignDataLen = (&msgCert.certInfo[msgCert.sig_RV] - msgCert.certInfo);
          if(asm_log_level >= LOG_DEBUG){
              AWSEC_LOG(ASM,NULL,0,"##### %s : %d : CertSignDataLen %d #####\n",__func__,__LINE__,CertSignDataLen);
          }
          if((AWSecDigestAndVerifyData((pBuf + offset), CertSignDataLen, &msgCert.certInfo[msgCert.sig_RV], pPubKey)) != SUCCESS)
          {
	      *errorCode = CMD_ERR_CERT_VERIFICATION_FAILED;//Certificate Verification Failed
              if(asm_log_level >= LOG_CRITICAL)
                  AWSEC_LOG(ASM,NULL,0,"Signature verification failed\n");
              return FAILURE;
          }
          else
          {
	      if(asm_log_level >= LOG_INF)
	          AWSEC_LOG(ASM,NULL,0,"Signature verified successfully\n"); 
          }
      }
      if(asm_log_level >= LOG_DEBUG){
          AWSEC_LOG(ASM,(UINT8 *)&msgCert.certInfo[msgCert.verification_key],33,"Verification Key\n");
          AWSEC_LOG(ASM,NULL,0,"End of Verification Key\n");
      }
      offset += msgCert.certLen;
      dataLen -= msgCert.certLen;
      if(asm_log_level >= LOG_DEBUG){
          AWSEC_LOG(ASM,NULL,0,"##### %s : %d : offset %d : Data Length %d #####\n",__func__,__LINE__,offset,dataLen);
      }
    }
    if(tempntry != NULL){
        bFound = 0;
        LIST_FOREACH(tempntry, &tempListHead, temp_list){
            LIST_FOREACH(WSAissuerntry, &WSAissuerListHead, WSAissuer_list){
                if(asm_log_level >= LOG_INF)
                    AWSEC_LOG(ASM,NULL,0,"How many times the WSA Issuer list was executed\n");
                if (!(memcmp(tempntry->pCertInfo.hash, WSAissuerntry->pCertInfo.hash, 8))){
                    if(asm_log_level >= LOG_INF)
                        AWSEC_LOG(ASM,NULL,0,"CA Certificate found in the WSA Issuer list\n");
                    bFound = 1;
                    break;
                }
            }
            if (!bFound){
                WSAissuerntry = (struct WSAissuerCertInfo *)malloc(sizeof(struct WSAissuerCertInfo));
                memcpy(WSAissuerntry->pCertInfo.hash, tempntry->pCertInfo.hash, 8);
                //WSAissuerntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &WSAissuerntry->pCertInfo.slotCert);
                WSAissuerntry->pCertInfo.slotCert.certLen = tempntry->pCertInfo.slotCert.certLen;
                memcpy(WSAissuerntry->pCertInfo.slotCert.certInfo,tempntry->pCertInfo.slotCert.certInfo,WSAissuerntry->pCertInfo.slotCert.certLen);
                WSAissuerntry->pCertInfo.certCheckFlag = 0x00;
                WSAissuerntry->processTime = get_cur_time2004();
                LIST_INSERT_HEAD(&WSAissuerListHead, WSAissuerntry, WSAissuer_list);
                if(asm_log_level >= LOG_INF)
                    AWSEC_LOG(ASM,NULL,0,"NAZEER: CA Certificate not found in the Cert Store(WSA Issuer) List, Adding it to WSA Issuer List from Temp List\n");
            }
            else{
                WSAissuerntry->processTime = get_cur_time2004();
                if(asm_log_level >= LOG_INF)
                    AWSEC_LOG(ASM,NULL,0,"NAZEER: CA Certificate found in the Cert Store(WSA Issuer) List\n");
            }
        }
        LIST_FOREACH(tempntry, &tempListHead, temp_list){
            LIST_REMOVE(tempntry, temp_list);
            free(tempntry);
        }
    }
    for(i = 0; i < NO_OF_PSID_PERMISSION; i++){
        psidPermission_rx.WSApsid[i] = getPsidbyLen(&msgCert.certInfo[msgCert.psid_array_permOffset + perm_offset],&retIDx,NULL);
        perm_offset += retIDx;
        psidPermission_rx.WSApriority[i] = msgCert.certInfo[msgCert.psid_array_permOffset + perm_offset];
        perm_offset += 1;
        psidPermission_rx.WSAsspString[i] = msgCert.certInfo[msgCert.psid_array_permOffset + perm_offset];
        perm_offset += 1;
    }


    bFound = 0;
    LIST_FOREACH(WSAntry, &WSAListHead, WSA_list)
    {
      if(asm_log_level >= LOG_INF)
          AWSEC_LOG(ASM,NULL,0,"How many times the WSA list was executed\n");
      if (!(memcmp(WSAntry->pCertInfo.hash, pDigest, 8)))
      {
        if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"WSA Entity Certificate found in the WSA list\n");
        bFound = 1;
        break;
      }
    }
    if (!bFound)
    {
      WSAntry = (struct WSACertInfo *)malloc(sizeof(struct WSACertInfo));
      memcpy(WSAntry->pCertInfo.hash, pDigest, 8);
      WSAntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &WSAntry->pCertInfo.slotCert);
      memcpy(WSAntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, WSAntry->pCertInfo.slotCert.certLen);
      WSAntry->pCertInfo.certCheckFlag = 0x00;
      WSAntry->processTime = get_cur_time2004();
      if(msgCert.certInfo[0] == 0x03){
          UINT8 pDigest[32];
          UINT32 uiDigestLen;
          if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL){
	      if(asm_log_level >= LOG_CRITICAL){
		  AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
	      }
              goto err;
          }
#if 0
          bzero(tmp_str,256);
          sprintf(tmp_str,"%s/%s/root_ca.cert",serverPath,asmoptions.KEY_CONF_DIR);
          uiTmpBufLen = read_file(tmp_str,wsaca_cert, sizeof(wsaca_cert));
          WSACAcert.certLen= certParse((wsaca_cert), &WSACAcert);
          memcpy(WSACAcert.certInfo, wsaca_cert, WSACAcert.certLen);
#else
	  cFound = 0;
	  LIST_FOREACH(WSAissuerntry, &WSAissuerListHead, WSAissuer_list){
              if (!(memcmp(WSAissuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
	          cFound = 1;
	          break;
	      }
	  }
	  if (!cFound){
	      *errorCode = CMD_ERR_CHAIN_ENDED_AT_UNKNOWN_ROOT;
               goto err;
	  }
	  else{
	      memcpy(&WSACAcert,&WSAissuerntry->pCertInfo.slotCert,sizeof(WSACAcert));
	  }
#endif
          if (AWSecHash(HASH_ALGO_SHA256, WSACAcert.certInfo, WSACAcert.certLen, pDigest, &uiDigestLen, 0) == NULL){
	      if(asm_log_level >= LOG_CRITICAL)
	    	  AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
              goto err;
          }
          bzero(wsaca_cert, 512);
          memcpy(wsaca_cert, pDigest, uiDigestLen);
          memcpy(&wsaca_cert[uiDigestLen], msgCert.certInfo, msgCert.certLen);

          if (AWSecHash(HASH_ALGO_SHA256, wsaca_cert, msgCert.certLen + uiDigestLen, pDigest, &uiDigestLen, 0) == NULL){
	      if(asm_log_level >= LOG_CRITICAL)
	    	  AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
              goto err;
          }
          if(asm_log_level >= LOG_DEBUG){
              AWSEC_LOG(ASM,&msgCert.certInfo[msgCert.sig_RV],33,"Key 1\n");
              AWSEC_LOG(ASM,NULL,0,"End of key1\n");
              AWSEC_LOG(ASM,&WSACAcert.certInfo[WSACAcert.verification_key],33,"Key 2\n");
              AWSEC_LOG(ASM,NULL,0,"End of key2\n");
          }
          if ((pubPoint = PublicKeyReconstruct(pDigest, &msgCert.certInfo[msgCert.sig_RV], &WSACAcert.certInfo[WSACAcert.verification_key], pPubKey, pGroup)) == NULL){
	      if(asm_log_level >= LOG_CRITICAL){
		  AWSEC_LOG(ASM,NULL,0,"%s:%d:PublicKeyReconstruct(): %s\n",__FILE__,__LINE__, strerror(errno));
	      }
              goto err;
          }
          memcpy(WSAntry->pCertInfo.pubSignKey, pPubKey, 33);
      }
      else
          memcpy(WSAntry->pCertInfo.pubSignKey, &msgCert.certInfo[msgCert.verification_key], 33);
      LIST_INSERT_HEAD(&WSAListHead, WSAntry, WSA_list);
      if(asm_log_level >= LOG_INF)
          AWSEC_LOG(ASM,NULL,0,"NAZEER: WSA Entity Certificate not found in the Cert Store(WSA) List, Adding it to WSA List\n");
	  
    }
    else
    {
      memcpy(pPubKey, WSAntry->pCertInfo.pubSignKey, 33);
      WSAntry->processTime = get_cur_time2004();
      if(asm_log_level >= LOG_INF)
          AWSEC_LOG(ASM,NULL,0,"NAZEER: WSA Entity Certificate found in the Cert Store(WSA) List\n");
    
    }
  }
  else if ((pBuf[offset + 1] == 0x0b) && (pBuf[offset + 2] == 0x03) )
  {
    offset =  3;
    msgCert.certLen = certParse((pBuf + offset), &msgCert);
    memcpy(msgCert.certInfo, (pBuf + offset), msgCert.certLen);
    if (AWSecHash(HASH_ALGO_SHA256, (pBuf + offset), msgCert.certLen, pDigest, &uiDigestLen, 8) == NULL)
    {
      if(asm_log_level >= LOG_CRITICAL)
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
      return FAILURE;
    }
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,(UINT8 *)rootDigest,8,"***** Start of Root Cert Digest *****\n");
        AWSEC_LOG(ASM,NULL,0,"***** End of Root Cert Digest *****\n");
        AWSEC_LOG(ASM,(UINT8 *)&msgCert.certInfo[msgCert.signer_id],8,"***** Start of Signer Id of Certificate *****\n"); 
        AWSEC_LOG(ASM,NULL,0,"***** End of Signer Id of Certificate *****\n");
    }

    bFound = 0;
    LIST_FOREACH(WSAissuerntry, &WSAissuerListHead, WSAissuer_list){
        if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"How many times the WSA Issuer list was executed\n");
        if (!(memcmp(WSAissuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
            if(asm_log_level >= LOG_INF)
                AWSEC_LOG(ASM,NULL,0,"Root Certificate found in the WSA Issuer list\n");
            bFound = 1;
            break;
        }
    }
    if (!bFound){
            *errorCode = CMD_ERR_CERT_NOT_FOUND;
            return FAILURE;
    }
    else{
        WSAissuerntry->processTime = get_cur_time2004();
        if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"NAZEER: Root Certificate found in the Cert Store(WSA Issuer) List\n");
    }

    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,(UINT8 *)msgCert.certInfo,msgCert.certLen,"Implicit Certificate\n");
        AWSEC_LOG(ASM,NULL,0,"End of Implicit Certificate\n");
    }

    if(asm_log_level >= LOG_INF)
        AWSEC_LOG(ASM,NULL,0,"Check Certificate Manager for this Certificate, if the Certificate is not found, then store Certificate digest\n");
    offset += msgCert.certLen;
    
    for(i = 0; i < NO_OF_PSID_PERMISSION; i++){
        psidPermission_rx.WSApsid[i] = getPsidbyLen(&msgCert.certInfo[msgCert.psid_array_permOffset + perm_offset],&retIDx,NULL);
        perm_offset += retIDx;
        psidPermission_rx.WSApriority[i] = msgCert.certInfo[msgCert.psid_array_permOffset + perm_offset];
        perm_offset += 1;
        psidPermission_rx.WSAsspString[i] = msgCert.certInfo[msgCert.psid_array_permOffset + perm_offset];
        perm_offset += 1;
    }
    
    bFound = 0;
    LIST_FOREACH(WSAntry, &WSAListHead, WSA_list)
    {
      if(asm_log_level >= LOG_INF)
          AWSEC_LOG(ASM,NULL,0,"How many times the WSA list was executed\n");
      if (!(memcmp(WSAntry->pCertInfo.hash, pDigest, 8)))
      {
        if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"WSA Entity Certificate found in the WSA list\n");
        bFound = 1;
        break;
      }
    }
    if (!bFound)
    {
      WSAntry = (struct WSACertInfo *)malloc(sizeof(struct WSACertInfo));
      memcpy(WSAntry->pCertInfo.hash, pDigest, 8);
      WSAntry->pCertInfo.slotCert.certLen = certParse(msgCert.certInfo, &WSAntry->pCertInfo.slotCert);
      memcpy(WSAntry->pCertInfo.slotCert.certInfo, msgCert.certInfo, WSAntry->pCertInfo.slotCert.certLen);
      WSAntry->processTime = get_cur_time2004();
      WSAntry->pCertInfo.certCheckFlag = 0x00;
      WSAntry->processTime = get_cur_time2004();
      if(msgCert.certInfo[0] == 0x03){
          UINT8 pDigest[32];
          UINT32 uiDigestLen;
          if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL){
	      if(asm_log_level >= LOG_CRITICAL){
		  AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
	      }
              goto err;
          }
#if 0
          bzero(tmp_str,256);
          sprintf(tmp_str,"%s/%s/root_ca.cert",serverPath,asmoptions.KEY_CONF_DIR);
          uiTmpBufLen = read_file(tmp_str,wsaca_cert, sizeof(wsaca_cert));
          WSACAcert.certLen= certParse((wsaca_cert), &WSACAcert);
          memcpy(WSACAcert.certInfo, wsaca_cert, WSACAcert.certLen);
#else
	  cFound = 0;
	  LIST_FOREACH(WSAissuerntry, &WSAissuerListHead, WSAissuer_list){
              if (!(memcmp(WSAissuerntry->pCertInfo.hash, &msgCert.certInfo[msgCert.signer_id], 8))){
	          cFound = 1;
	          break;
	      }
	  }
	  if (!cFound){
	      *errorCode = CMD_ERR_CHAIN_ENDED_AT_UNKNOWN_ROOT;
               goto err;
	  }
	  else{
	      memcpy(&WSACAcert,&WSAissuerntry->pCertInfo.slotCert,sizeof(WSACAcert));
	  }
#endif
          if (AWSecHash(HASH_ALGO_SHA256, WSACAcert.certInfo, WSACAcert.certLen, pDigest, &uiDigestLen, 0) == NULL){
	      if(asm_log_level >= LOG_CRITICAL)
	  	  AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
              goto err;
          }
          bzero(wsaca_cert, 512);
          memcpy(wsaca_cert, pDigest, uiDigestLen);
          memcpy(&wsaca_cert[uiDigestLen], msgCert.certInfo, msgCert.certLen);

          if (AWSecHash(HASH_ALGO_SHA256, wsaca_cert, msgCert.certLen + uiDigestLen, pDigest, &uiDigestLen, 0) == NULL){
	      if(asm_log_level >= LOG_CRITICAL)
	  	  AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
              goto err;
          }
	  if(asm_log_level >= LOG_DEBUG){
              AWSEC_LOG(ASM,&msgCert.certInfo[msgCert.sig_RV],33,"Key 1\n");
              AWSEC_LOG(ASM,NULL,0,"End of key1\n");
              AWSEC_LOG(ASM,&WSACAcert.certInfo[WSACAcert.verification_key],33,"Key 2\n");
              AWSEC_LOG(ASM,NULL,0,"End of key2\n");
          }
          if ((pubPoint = PublicKeyReconstruct(pDigest, &msgCert.certInfo[msgCert.sig_RV], &WSACAcert.certInfo[WSACAcert.verification_key], pPubKey, pGroup)) == NULL){
	      if(asm_log_level >= LOG_CRITICAL){
		  AWSEC_LOG(ASM,NULL,0,"%s:%d:PublicKeyReconstruct(): %s\n",__FILE__,__LINE__, strerror(errno));
	      }
              goto err;
          }
          memcpy(WSAntry->pCertInfo.pubSignKey, pPubKey, 33);
      }
      else
          memcpy(WSAntry->pCertInfo.pubSignKey, &msgCert.certInfo[msgCert.verification_key], 33);
      LIST_INSERT_HEAD(&WSAListHead, WSAntry, WSA_list);
      if(asm_log_level >= LOG_INF)
          AWSEC_LOG(ASM,NULL,0,"NAZEER: WSA Entity Certificate not found in the Cert Store(WSA) List, Adding it to WSA List\n");
	  
    }
    else
    {
      memcpy(pPubKey, WSAntry->pCertInfo.pubSignKey, 33);
      WSAntry->processTime = get_cur_time2004();
      if(asm_log_level >= LOG_INF)
          AWSEC_LOG(ASM,NULL,0,"NAZEER: WSA Entity Certificate found in the Cert Store(WSA) List\n");
	  
    }
  }
  else if ((pBuf[offset + 1] == 0x0b) && (pBuf[offset + 2] == 0x02))
  {
    offset =  3;
    bFound = 0;
    LIST_FOREACH(WSAntry, &WSAListHead, WSA_list)
    {
      if(asm_log_level >= LOG_INF)
          AWSEC_LOG(ASM,NULL,0,"How many times the WSA list was executed\n");
      if (!(memcmp(WSAntry->pCertInfo.hash, (pBuf + offset), 8)))
      {
        if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"WSA Entity Certificate found in the WSA list\n");
        bFound = 1;
        break;
      }
    }
    if (!bFound)
    {
        if(asm_log_level >= LOG_CRITICAL){
            AWSEC_LOG(ASM,NULL,0,"Digest received for Unknown Certificate, so return from here\n");
            AWSEC_LOG(ASM,NULL,0,"NAZEER: WSA Entity Certificate not found in the Cert Store(WSA) List\n");
  	}
	*errorCode = CMD_ERR_CERT_NOT_FOUND;   
        return FAILURE;
    }
    else
    {
      memcpy(pPubKey, WSAntry->pCertInfo.pubSignKey, 33);
      WSAntry->processTime = get_cur_time2004();
      if(asm_log_level >= LOG_INF)
          AWSEC_LOG(ASM,NULL,0,"NAZEER: WSA Entity Certificate found in the Cert Store(WSA) List\n");
    }
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"Verify WSA Certifiacte digest by comparing the known Certificate digest. Certifcate Manager should be callled here\n");
        AWSEC_LOG(ASM,NULL,0,"Get the Certificate related to certificate digest and get the public key\n");
    }
    offset += 8;
  }
  else{
      *errorCode = CMD_ERR_INVALID_PACKET;
      return FAILURE;
  }
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,&pBuf[offset],33,"WSA signed data\n");
      AWSEC_LOG(ASM,NULL,0,"End of WSA signed data\n");
  }
  signOffSet = offset;
  permissionIndicesLen = pBuf[offset];
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,NULL,0,"##### %s : %d : signOffSet %d #####\n",__func__,__LINE__,signOffSet);
      AWSEC_LOG(ASM,NULL,0,"##### %s : %d : offset %d #####\n",__func__,__LINE__,offset);
      AWSEC_LOG(ASM,NULL,0,"##### %s : %d : req2->number_of_permissions %d #####\n",__func__,__LINE__,req2->number_of_permissions);
      AWSEC_LOG(ASM,NULL,0,"##### %s : %d : permissionIndicesLen %d #####\n",__func__,__LINE__,permissionIndicesLen);
  }
  offset += 1;
  for(i = 0 ; i < req2->number_of_permissions ; i++){
      psID = getPsidbyLen(&req2->permissions[permOffset],&retIDx,NULL);
      permOffset += retIDx ;
      priority = req2->permissions[permOffset];
      permOffset += 1; 
      sspString = req2->permissions[permOffset];
      permOffset += 1; 
      for(j = 0; j < permissionIndicesLen ; j++){
          permissionIndices = pBuf[offset] -1;
          //permissionIndices = pBuf[offset];
          if(asm_log_level >= LOG_DEBUG){
              AWSEC_LOG(ASM,NULL,0,"##### %s : %d : permissionIndices %d #####\n",__func__,__LINE__,permissionIndices);
              AWSEC_LOG(ASM,NULL,0,"##### %s : %d : psID %02x : psidPermission_rx.WSApsid[%d] %02x #####\n",__func__,__LINE__,psID,permissionIndices,psidPermission_rx.WSApsid[permissionIndices]);
              AWSEC_LOG(ASM,NULL,0,"##### %s : %d : priority %02x : psidPermission_rx.WSApriority[%d] %02x #####\n",__func__,__LINE__,priority,permissionIndices,psidPermission_rx.WSApriority[permissionIndices]);
              AWSEC_LOG(ASM,NULL,0,"##### %s : %d : sspString %02x : psidPermission_rx.WSAsspString[%d] %02x #####\n",__func__,__LINE__,sspString,permissionIndices,psidPermission_rx.WSAsspString[permissionIndices]);
          }
          if(psID == psidPermission_rx.WSApsid[permissionIndices] && priority == psidPermission_rx.WSApriority[permissionIndices] && sspString == psidPermission_rx.WSAsspString[permissionIndices]){
              psidIsThere = 1 ; 
              break;
          }
      }
      offset += 1;
  }
  if(!psidIsThere){
      *errorCode = CMD_ERR_INCONSISTENT_PERMISSIONS;
       return FAILURE;
  }
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,NULL,0,"##### %s : %d : pBuf[%d] %02x #####\n",__func__,__LINE__,offset,pBuf[offset]);
  }
  offset += 1;     //flag 
  dataLen = decode_length((pBuf + offset), &len);
  offset += len;
  offset += dataLen;
      if(pBuf[signOffSet + 2 ] & 0x02){
          if(asm_log_level >= LOG_INF)
              AWSEC_LOG(ASM,NULL,0,"Use Generation time\n");
          memcpy(&generation_time,(pBuf+offset),8);
          ret_val = validate_generation_time(generation_time,req2->message_validity_period);
          if(ret_val == 1){
    	      if(asm_log_level >= LOG_INF)
                  AWSEC_LOG(ASM,NULL,0,"Message is valid as per generation time\n");
          }
          else if(ret_val == 2){
              *errorCode = CMD_ERR_MESSAGE_FUTURE_MESSAGE;    
              return FAILURE;
          }
          else if(ret_val == 3){
              *errorCode = CMD_ERR_MESSAGE_EXPIRED_BASED_ON_GENERATION_TIME;
              return FAILURE;
          }
          else{
              *errorCode = CMD_ERR_MESSAGE_VERIFICATION_FAILED;
              return FAILURE;
          }
          offset += 8;
          offset += 1; /** Confidence **/
      }
      if(pBuf[signOffSet + 2] & 0x04){
          if(asm_log_level >= LOG_INF)
              AWSEC_LOG(ASM,NULL,0,"Use expiration time\n");
          memcpy(&expiry_time,(pBuf+offset),8);
          if(asm_log_level >= LOG_INF){
              AWSEC_LOG(ASM,NULL,0,"##### %s : %d : Life Time %llu #####\n",__func__,__LINE__,expiry_time);
          }
          //expiry_time += generation_time;
          ret_exp = validate_expiry_time(expiry_time);
          if(ret_exp == 1){
    	      if(asm_log_level >= LOG_INF)
       	          AWSEC_LOG(ASM,NULL,0,"#### WSA Is Not Expired Yet ####\n");
	  }
          else{
    	      if(asm_log_level >= LOG_CRITICAL)
       	          AWSEC_LOG(ASM,NULL,0,"WSA Is Expired based on expiry time\n");
	      *errorCode = CMD_ERR_MESSAGE_VERIFICATION_FAILED;
	      return FAILURE;
     	  }
          offset += 8;
       }

      if(pBuf[signOffSet + 2 ] & 0x08){
        if(asm_log_level >= LOG_INF)
            AWSEC_LOG(ASM,NULL,0,"Use Location\n");
	location position; 
	UINT32 rangeMeters = req2->message_validity_distance;
	position.local_latitude = req2->local_location_latitude;
	position.local_longitude = req2->local_location_longitude;
        memcpy(&position.generation,(pBuf+offset),10);
	if(BIGENDIAN){
	    position.local_latitude = ntohl(position.local_latitude);
	    position.local_longitude = ntohl(position.local_longitude);
	    position.generation.latitude = ntohl(position.generation.latitude);
	    position.generation.longitude = ntohl(position.generation.longitude);
	    rangeMeters = ntohl(rangeMeters);
            if(asm_log_level >= LOG_INF)
                AWSEC_LOG(ASM,NULL,0,"ENDIANESS CORRECTED\n");
	}
	if(position.generation.latitude != 900000001 && position.generation.longitude != 1800000001){
            if(asm_log_level >= LOG_INF){
                AWSEC_LOG(ASM,NULL,0,"GENERATION Lat:%ld Lon:%ld \n",position.generation.latitude,position.generation.longitude);
            }
	    if(req2->local_location_latitude != 900000001 && req2->local_location_longitude != 1800000001){
		  if(asm_log_level >= LOG_INF){
		      AWSEC_LOG(ASM,NULL,0,"LOCAL Lat:%ld Lon:%ld \n",position.local_latitude,position.local_longitude);
		  }
		  distance_cal(&position);
		  if(asm_log_level >= LOG_INF){
		      AWSEC_LOG(ASM,NULL,0,"distance:%lf range:%d\n",position.distance,rangeMeters);
		  }
		  if((UINT32)position.distance >= rangeMeters){
		      if(asm_log_level >= LOG_INF)
		          AWSEC_LOG(ASM,NULL,0,"range exceed return from here\n");
		      *errorCode = CMD_ERR_MESSAGE_OUT_OF_RANGE;//later use enum values
		      return FAILURE;
		  }
		  else{
		      if(asm_log_level >= LOG_INF)
		          AWSEC_LOG(ASM,NULL,0,"LOCATION VERIFICATION DONE\n");
		  }
	    }
	    else
	        if(asm_log_level >= LOG_CRITICAL)
	    	    AWSEC_LOG(ASM,NULL,0,"INVALID LOCATION: LOCAL \n");
	}
	else
	    if(asm_log_level >= LOG_CRITICAL)
	        AWSEC_LOG(ASM,NULL,0,"INVALID LOCATION: REMOTE\n");
        offset += 10; /** This is supposed to be 10 **/
      }

       if(pBuf[signOffSet + 2 ] & 0x10){
	   if(asm_log_level >= LOG_INF)
	       AWSEC_LOG(ASM,NULL,0,"Use Extensions\n");
           offset += 1; /** Type **/
           /** This is variable length field **/
           dataLen = decode_length((pBuf + offset), &len);
           offset += len;
           offset += dataLen;
       }
       //offset += 1;
  
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,pPubKey,33,"Verification Key\n");
      AWSEC_LOG(ASM,NULL,0,"End of Verification Key\n");
      AWSEC_LOG(ASM,(UINT8 *)pBuf+offset,33,"pBuf Start\n");
      AWSEC_LOG(ASM,NULL,0,"pBuf End \n");
      AWSEC_LOG(ASM,NULL,0,"##### %s : %d : offset %d : offset - signOffSet %d ##### ",__func__,__LINE__,offset,(offset - signOffSet));
      AWSEC_LOG(ASM,pBuf,offset - signOffSet,"WSA signed data\n");
      AWSEC_LOG(ASM,NULL,0,"End of WSA signed data\n");
      
  }
  if((AWSecDigestAndVerifyData((pBuf + signOffSet), offset - signOffSet, (pBuf + offset), pPubKey)) != SUCCESS)
  {
    *errorCode = CMD_ERR_CERT_VERIFICATION_FAILED;//Certificate Verification Failed
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"Signature verification failed\n");
    return FAILURE;
  }
  else
  {
    if(asm_log_level >= LOG_INF)
        AWSEC_LOG(ASM,NULL,0,"Signature verified successfully\n");        
    memcpy(SignedData,(pBuf + signOffSet), (offset - signOffSet));
    //memcpy(SignedData,(pBuf + tmp_offset), (offset - signOffSet));
    //memcpy(SignedData,(pBuf + tmp_offset), (*tmpOffset - tmp_offset));
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"##### %s : %d : (offset - signOffSet) %d #####\n",__func__,__LINE__,(offset - signOffSet));
        AWSEC_LOG(ASM,SignedData,offset - signOffSet,"Start of Unsigned data\n");
        AWSEC_LOG(ASM,NULL,0,"End of Unsigned data\n");
    }
  }
  return (offset - signOffSet);
err:
    if(pubPoint)
        EC_POINT_clear_free(pubPoint);
    if(pGroup)
        EC_GROUP_clear_free(pGroup);
    return FAILURE;
}

int32_t DecryptedDataIn16092(UINT32 cmd, uint8_t *buff16092, uint8_t *pEncData, uint32_t EncDataLen, UINT8 *pDecryptionKey, uint8_t *errorCode)
{
  UINT32 outlen, offset = 0, i, j, k,  recLen, uiBufLen;
  INT32 len;
  UINT8 pKey[33], pNonce[12], pCipher[16], pMac[20], pOutBuf[16], pCertPrivKey[32], pCertPubKey[33], pCertID[8], pBuf[256];
  char logstring[250];
  struct ENC1CertInfo *ENC1ntry;
  uint32_t tmpOffset =0,tmpLen=0;
  //offset = 4; //length of whole .2 message
  offset = 0; //length of whole .2 message
  if ((pEncData[offset++] != 2) || (pEncData[offset++] != 2))
  {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"Message is not formatted correctly\n");
    *errorCode = CMD_ERR_INVALID_PACKET;
    return FAILURE;
  }

//EncryptedData(page 73 in d9_3)
  if (pEncData[offset++] != 0x00){ //SymmAlgorithm->aes_128_ccm (0)
      if(asm_log_level >= LOG_CRITICAL)
          AWSEC_LOG(ASM,NULL,0,"Invalid Symmetric Algorithm\n");
      *errorCode = CMD_ERR_INVALID_PACKET;
      return FAILURE;
  }
  recLen = decode_length(&pEncData[offset], &len);
  offset += len;
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"len = %d, recLen = %d\n", len, recLen);
  }
  memcpy(pCertID, &pEncData[offset], 8);
  offset += 8;
  memcpy(pKey, &pEncData[offset], 33);
  offset += 33;
  memcpy(pCipher, &pEncData[offset], 16);
  offset += 16;
  memcpy(pMac, &pEncData[offset], 20);
  offset += 20;
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"len = %d, recLen = %d\n", len, recLen);
  }
  if(AWSec_ecies_decrypt(pDecryptionKey, pKey, 33, pCipher, 16, pMac, 20, 0, CIPHER_ALGO_AES128_ECB, pOutBuf, &outlen, errorCode) == FAILURE)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSec_ecies_decrypt(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    return FAILURE;
  }
  else
  {
    if(asm_log_level >= LOG_INF)
        AWSEC_LOG(ASM,NULL,0,"Decrypt using ECIES is successful\n");
  }

  //AesCcmCiphertext->nonce
  memcpy(pNonce, (pEncData + offset), 12);
  offset += 12;
  //AesCcmCiphertext->ccm_ciphertext<var>
  len=0;
  recLen = decode_length((pEncData + offset), &len);
  offset += len;
  if (AWSecDecrypt(CIPHER_ALGO_AES128_CCM, pOutBuf, pNonce, 12, (pEncData + offset), recLen, pBuf, &uiBufLen, 16) != SUCCESS)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecDecrypt(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    *errorCode = CMD_ERR_COULD_NOT_DECRYPT_MESSAGE;
     return FAILURE;
  }
  else
  {
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"Decrypt using Decryption Key is successful uiBufLen = %d\n", uiBufLen);
    }
  }
  //memcpy(buff16092,(pEncData + offset),recLen);
  tmpLen = uiBufLen;
  while(pBuf[tmpOffset] == 0x00){
      uiBufLen--;
      tmpOffset++;
  }
  memcpy(buff16092,&pBuf[tmpOffset],uiBufLen);    
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,pNonce,12,"***** Start Of Nonce *****\n");
      AWSEC_LOG(ASM,NULL,0,"***** End Of Nonce *****\n");
      AWSEC_LOG(ASM,NULL,0,"##### %s : %d : recLen %d : len %d #####\n",__func__,__LINE__,recLen,len);
      AWSEC_LOG(ASM,pBuf,tmpLen,"***** Start Of Decrypted Data *****\n");
      AWSEC_LOG(ASM,NULL,0,"***** End Of Decrypted Data *****\n");
      
  }
  offset += uiBufLen;

  return recLen;
}


UINT32 AWSecGetPrivKey(EC_KEY *ECKey, UINT8 *pPrivKeyBuf, UINT32 *uiPrivSize)
{
  char * hex = NULL;
  char logstring[100];	
  BIGNUM *bn;
  if (!(bn = EC_KEY_get0_private_key(ECKey))) 
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"EC_KEY_get0_private_key\n");
	AWSEC_LOG(ASM,NULL,0,"%s\n", ERR_error_string(ERR_get_error(), NULL));
    }
    return FAILURE;
  }

  if (!(hex = BN_bn2hex(bn))) 
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"BN_bn2hex\n");
	AWSEC_LOG(ASM,NULL,0,"%s\n", ERR_error_string(ERR_get_error(), NULL));
    }
    return FAILURE;
  }

  *uiPrivSize = HexStr2ByteStr(hex, pPrivKeyBuf, 32);
  OPENSSL_free(hex);
  return SUCCESS;
}

UINT32 AWSecSetPrivKey(EC_KEY *ECKey, BIGNUM *bn)
{
  char logstring[200];
  if (!EC_KEY_set_private_key(ECKey, bn))
  {
    if(asm_log_level >= LOG_CRITICAL){
	AWSEC_LOG(ASM,NULL,0,"EC_KEY_set_private_key failed--- %s:%d:EC_KEY_set_private_key(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    return FAILURE;
  }
  return SUCCESS;
}

UINT32 AWSecSetPubKey(EC_KEY *ECKey, UINT8 *pPubKeyBuf, UINT32 uiPubSize)
{
  if (!o2i_ECPublicKey(&ECKey, (const unsigned char **)&pPubKeyBuf, uiPubSize))
    return FAILURE;
  if(uiPubSize == 33)
    EC_KEY_set_conv_form(ECKey, POINT_CONVERSION_COMPRESSED);
  return SUCCESS;
}

UINT32 AWSecGetPubKey(EC_KEY *ECKey, UINT8 *pPubKeyBuf, UINT32 *uiPubSize)
{

  UINT32 iRet;
  iRet = EC_POINT_point2oct(EC_KEY_get0_group(ECKey), EC_KEY_get0_public_key(ECKey), POINT_CONVERSION_COMPRESSED, pPubKeyBuf, 33, NULL);
  *uiPubSize = iRet;
  if (iRet == 0)
      if(asm_log_level >= LOG_CRITICAL)
          AWSEC_LOG(ASM,NULL,0,"Couldn't get the Public Key from the ECKey\n");
  return SUCCESS;
}

EC_GROUP *pGroup224 = NULL;
EC_GROUP *pGroup256 = NULL;

UINT32 ECCGroupInit()
{
  point_conversion_form_t form = POINT_CONVERSION_COMPRESSED;
  UINT32 asn1_flag = OPENSSL_EC_NAMED_CURVE;

  pGroup224 = EC_GROUP_new_by_curve_name(NID_secp224r1);
  EC_GROUP_set_asn1_flag(pGroup224, asn1_flag);
  EC_GROUP_set_point_conversion_form(pGroup224, form);

  pGroup256 = EC_GROUP_new_by_curve_name(NID_X9_62_prime256v1);
  EC_GROUP_set_asn1_flag(pGroup256, asn1_flag);
  EC_GROUP_set_point_conversion_form(pGroup256, form);

  return SUCCESS;
}

UINT32 ECCGroupDeInit()
{
  if(pGroup224)
    EC_GROUP_clear_free(pGroup224);
  if(pGroup256)
    EC_GROUP_clear_free(pGroup256);
  return SUCCESS;
}

EC_GROUP * CreateECCompGroup(UINT32 GroupType)
{
  point_conversion_form_t form = POINT_CONVERSION_COMPRESSED;
  UINT32 asn1_flag = OPENSSL_EC_NAMED_CURVE;
  UINT32 iRet = FAILURE;
  switch(GroupType) 
  {
    case ECDSA_NISTP_224:
      if (pGroup224 != NULL)
        return EC_GROUP_dup(pGroup224);
      else
      {
        pGroup224 = EC_GROUP_new_by_curve_name(NID_secp224r1);
        EC_GROUP_set_asn1_flag(pGroup224, asn1_flag);
        EC_GROUP_set_point_conversion_form(pGroup224, form);
        return EC_GROUP_dup(pGroup224);
      }  
      break;
    case ECDSA_NISTP_256:
      if (pGroup256 != NULL)
        return EC_GROUP_dup(pGroup256);
      else
      {
        pGroup256 = EC_GROUP_new_by_curve_name(NID_X9_62_prime256v1);
        EC_GROUP_set_asn1_flag(pGroup256, asn1_flag);
        EC_GROUP_set_point_conversion_form(pGroup256, form);
        return EC_GROUP_dup(pGroup256);
      }
      break;
    default:
    {
      return NULL;  
    }
  }
  return NULL;
}

EC_KEY * GenerateECCompKey(UINT32 GroupType, EC_KEY *ECKey)
{
  EC_GROUP *pGroup = NULL;
  char logstring[250];
  if (!(ECKey = EC_KEY_new()))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_KEY_new(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if((pGroup = CreateECCompGroup(GroupType)) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL){
	AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if (EC_KEY_set_group(ECKey, pGroup) != 1)
  {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_KEY_set_group(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
    goto err;
  }
  if (EC_KEY_generate_key(ECKey) != 1)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_KEY_generate_key(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }

err:
  if (ECKey == NULL)
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%s: %d: Returned error  library = %s, func = %s, reason = %s, {error = %s}\n",__FUNCTION__,__FILE__,__LINE__,ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
      }
  
  if(pGroup)
    EC_GROUP_clear_free(pGroup);
  return ECKey;
}

EC_KEY * ECKeyFromPubKey(UINT32 GroupType, EC_KEY *ECKey, UINT8 *pPubKey, UINT32 uiKeyLen)
{
  EC_GROUP *pGroup = NULL;
  EC_POINT *point = NULL;
  UINT8 hex[2 * uiKeyLen];
  UINT32 uiErr = FAILURE;
  UINT32 i;
  char logstring[250];
  if(ECKey == NULL)
  {
    if (!(ECKey = EC_KEY_new()))
    {
      if(asm_log_level >= LOG_CRITICAL){
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_KEY_new(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    if((pGroup = CreateECCompGroup(GroupType)) == NULL)
    {
      if(asm_log_level >= LOG_CRITICAL){
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    if (EC_KEY_set_group(ECKey, pGroup) != 1)
    {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_KEY_set_group(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    EC_KEY_set_conv_form(ECKey, POINT_CONVERSION_COMPRESSED);
  }
  if (!(point = EC_POINT_new(EC_KEY_get0_group(ECKey)))) 
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_new(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if(!(EC_POINT_oct2point(EC_KEY_get0_group(ECKey), point, pPubKey, 33, NULL)))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_oct2point(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if (EC_KEY_set_public_key(ECKey, point) != 1) 
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_KEY_set_public_key(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  uiErr = SUCCESS;
err:
  if (uiErr == FAILURE)
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%s: %d: Returned error  library = %s, func = %s, reason = %s, {error = %s}\n",__FUNCTION__,__FILE__,__LINE__,ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
    }
  if(pGroup)
    EC_GROUP_clear_free(pGroup);
  if(point)
    EC_POINT_clear_free(point);
  return ECKey;
}

EC_KEY * ECKeyFromPrivKey(UINT32 GroupType, EC_KEY *ECKey, UINT8 *pPrivKey, UINT32 uiKeyLen)
{
  EC_GROUP *pGroup = NULL;
  BIGNUM *bn = NULL;
  UINT8 hex[2 * uiKeyLen];
  char logstring[250];
  if (ECKey == NULL)
  {
    if((pGroup = CreateECCompGroup(GroupType)) == NULL)
    {
      if(asm_log_level >= LOG_CRITICAL){
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    if (!(ECKey = EC_KEY_new()))
    {
      if(asm_log_level >= LOG_CRITICAL){
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_KEY_new(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    if (EC_KEY_set_group(ECKey, pGroup) != 1)
    {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_KEY_set_group(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    EC_KEY_set_conv_form(ECKey, POINT_CONVERSION_COMPRESSED);
  }
  uiKeyLen = ByteStr2HexStr(pPrivKey, hex, uiKeyLen);
  if (!(BN_hex2bn(&bn, hex))) 
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_KEY_set_group(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }

  if (EC_KEY_set_private_key(ECKey, bn) != 1) 
  {
    if(asm_log_level >= LOG_CRITICAL){
	AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_KEY_set_private_key(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }


err:
  if (ECKey == NULL)
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%s: %d: Returned error  library = %s, func = %s, reason = %s, {error = %s}\n",__FUNCTION__,__FILE__,__LINE__,ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
    }
  if(pGroup)
    EC_GROUP_clear_free(pGroup);
  if(bn)
    BN_free(bn);
  return ECKey;
}

void * ECIES_KDF2_KEY_derivation(const void *input, size_t ilen, void *output, size_t *olen)
{
        int i = 0, j = 0;
        UINT32 len = *olen, outOff = 0, uiTotSize;
        int outLen = SHA256_DIGEST_LENGTH; 
        int cThreshold = (int)((len + outLen - 1) / outLen);
        UINT8 digest[SHA256_DIGEST_LENGTH];
        UINT8 pTmpBuf[ilen + 4];
        UINT32 counter = 1;
	char logstring[250];
        //EVP_MD_CTX md_ctx;
        //EVP_MD_CTX_init(&md_ctx);
        uiTotSize = sizeof(pTmpBuf);
        bzero(pTmpBuf, uiTotSize);
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"pTmpBuf = %d, Outlen = %d, inLen = %d\n", sizeof(pTmpBuf), *olen, ilen);
        }
        // this is at odds with the standard implementation, the
        // maximum value should be hBits * (2^32 - 1) where hBits
        // is the digest output size in bits. We can't have an
        // array with a long index at the moment...
        //
        if((*olen % SHA256_DIGEST_LENGTH) != 0)
          cThreshold++; 
        if (len > ((2L << 32) - 1))
        {
	    if(asm_log_level >= LOG_CRITICAL)
	        AWSEC_LOG(ASM,NULL,0,"Output length too large\n");
        }
        memcpy(pTmpBuf, input, ilen);
        for (counter = 1; counter < cThreshold; counter++)
        {
            //EVP_DigestInit(&md_ctx, EVP_sha256());
            if(asm_log_level >= LOG_INF){
                AWSEC_LOG(ASM,NULL,0,"counter = %d\n", counter);
            }
            pTmpBuf[ilen] = (UINT8)(counter >> 24);
            pTmpBuf[ilen + 1] = (UINT8)(counter >> 16);
            pTmpBuf[ilen + 2] = (UINT8)(counter >> 8);
            pTmpBuf[ilen + 3] = (UINT8)(counter);
            SHA256(pTmpBuf, uiTotSize, digest);
            outLen = SHA256_DIGEST_LENGTH;
#ifdef NAZEER
            EVP_DigestUpdate(&md_ctx, (const void*)pTmpBuf, uiTotSize);
            EVP_DigestFinal(&md_ctx, digest, &outLen);
#endif
            if (len > outLen)
            {
                memcpy((output + outOff), digest, outLen);
                outOff += outLen;
                len -= outLen;
            }
            else
            {
                memcpy((output + outOff), digest, len);
            }
	    if(asm_log_level >= LOG_INF)
	        AWSEC_LOG(ASM,digest,outLen,"Digest calculation done\n");
        }
        //EVP_MD_CTX_cleanup(&md_ctx);
        //*olen = len;
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"outLen = %d, outOff = %d\n", outLen, outOff);
        }
        return output;
}

UINT32 hashCal()
{
  UINT32 uiDigestLen, i;
  UINT8 pDigest[32], pTmpBuf[512];
  UINT8 CACert[] = {0x02, 0xFF, 0x04, 0x00, 0x83, 0xFF, 0x01, 0x00, 0x01, 0x00, 0x04, 0x13, 0x97, 0x09, 0xDD, 0x00, 0x00, 0x00, 0x01, 0x01, 0x02, 0x69, 0xDF, 0x01, 0x90, 0x66, 0x7E, 0x3C, 0xBD, 0xCA, 0xD0, 0x9D, 0x70, 0x33, 0xCC, 0xAC, 0x2F, 0xFC, 0x0F, 0xF1, 0x30, 0x24, 0x96, 0xCC, 0x92, 0xF9, 0xC6, 0xE5, 0x0B, 0x9D, 0xB1, 0x72, 0xB4, 0x02, 0x00, 0x02, 0x93, 0x53, 0x54, 0x7B, 0xEF, 0x08, 0x05, 0x45, 0x50, 0xC0, 0xA8, 0x36, 0x7F, 0x9A, 0xDC, 0xC2, 0xB2, 0x3F, 0xE2, 0xB5, 0xB0, 0x17, 0x46, 0xB3, 0xEE, 0xA4, 0xA8, 0x69, 0xB5, 0xF2, 0xEB, 0x30, 0x00, 0x3E, 0xC9, 0x34, 0x3E, 0xFE, 0xF4, 0xB9, 0xD6, 0xCE, 0x32, 0x4B, 0x93, 0x35, 0x4C, 0x3E, 0x8B, 0x38, 0x37, 0xC4, 0x93, 0x67, 0xB7, 0x01, 0xB5, 0x64, 0x15, 0x3B, 0x53, 0x59, 0xF8, 0xE2, 0x17, 0x9D, 0x1F, 0xEB, 0x3C, 0x0C, 0x34, 0xBD, 0xC2, 0x95, 0x7C, 0xAC, 0xE8, 0xC7, 0xB3, 0x3D, 0x3A, 0x9E, 0xA1, 0xDD, 0x77, 0xA7, 0x3C, 0x65, 0x2E, 0x07, 0x26, 0x4D, 0xA9, 0x02, 0xB9, 0x5A, 0x09};
   UINT8 CSRCert[] = {0x03, 0x03, 0x01, 0x7C, 0xFA, 0x71, 0xFE, 0xB7, 0x12, 0x69, 0x8B, 0x01, 0x07, 0x56, 0x41, 0x44, 0x30, 0x30, 0x30, 0x31, 0x01, 0x01, 0x01, 0x20, 0x04, 0x13, 0x97, 0x09, 0xDD, 0x0F, 0xD4, 0xA2, 0xDE, 0x00, 0x00, 0x00, 0x01, 0x02, 0x2F, 0x85, 0xD2, 0x72, 0x23, 0x6B, 0xE7, 0x99, 0xD8, 0xAE, 0x7E, 0xE8, 0x32, 0x51, 0x95, 0x39, 0x9B, 0x18, 0xBE, 0xBE, 0xAC, 0x76, 0x46, 0x57, 0xD6, 0xE5, 0x6E, 0x77, 0x09, 0x14, 0x31, 0x90};
  if (AWSecHash(HASH_ALGO_SHA256, CACert, sizeof(CACert), pDigest, &uiDigestLen, 0) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL)
	AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
  }
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,pDigest,uiDigestLen,"pDigest \n");
      AWSEC_LOG(ASM,NULL,0,"End of pDigest \n");
  }
  bzero(pTmpBuf, 512);
  memcpy(pTmpBuf, pDigest, uiDigestLen);
  memcpy(&pTmpBuf[uiDigestLen], CSRCert, sizeof(CSRCert));

  if (AWSecHash(HASH_ALGO_SHA256, pTmpBuf, sizeof(CSRCert) + uiDigestLen, pDigest, &uiDigestLen, 0) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL)
	AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
  }
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,pDigest,uiDigestLen,"pDigest \n");
      AWSEC_LOG(ASM,NULL,0,"End of pDigest \n");
  }
  return SUCCESS;
}

UINT32 ReconPrivKey()
{
  UINT8 pEph[] = {0xCE, 0x31, 0xB1, 0x55, 0xA3, 0xB0, 0x79, 0x7D, 0x82, 0x87, 0x8A, 0x2E, 0x4C, 0xF9, 0x8C, 0xD0, 0xE8, 0x71, 0xD6, 0x3A, 0xC9, 0xA9, 0x63, 0xB3, 0xA8, 0xB2, 0x03, 0x53, 0xA7, 0xB3, 0x94, 0x08};
  UINT8 Recon[] = {0xC8, 0xAF, 0x6D, 0xE7, 0x70, 0x46, 0x99, 0x68, 0x49, 0x17, 0x66, 0x12, 0xE5, 0x7B, 0xF8, 0x5E, 0x2F, 0x24, 0x35, 0x8A, 0xFF, 0x12, 0x41, 0xAD, 0x19, 0x0B, 0x37, 0xF6, 0xD7, 0x3A, 0xA8, 0x78};
  UINT8 pHash[] = {0xF6, 0x62, 0x50, 0x10, 0x4C, 0xC2, 0x1C, 0x1E, 0xAB, 0x1C, 0x13, 0xFB, 0x69, 0x8E, 0x20, 0x60, 0x2E, 0xBB, 0x96, 0xE6, 0x4B, 0x8F, 0x69, 0x9B, 0x41, 0xBD, 0x2A, 0x85, 0x78, 0x3A, 0x8E, 0x66};
  UINT8 pPrivKey[32];
  EC_GROUP *pGroup = NULL;
  UINT32 i;
  char logstring[250];
  if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    return FAILURE;
  }
  PrivateKeyReconstruct(pHash, Recon, pEph, pPrivKey, pGroup);
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,pPrivKey,32,"Private Key \n");
      AWSEC_LOG(ASM,NULL,0,"End of Private Key \n");
  }
    if (pGroup)
      EC_GROUP_clear_free(pGroup);

  return SUCCESS;
}


UINT32 ReconPubKey()
{
  UINT8 CAKey[] = {0x02, 0x69, 0xDF, 0x01, 0x90, 0x66, 0x7E, 0x3C, 0xBD, 0xCA, 0xD0, 0x9D, 0x70, 0x33, 0xCC, 0xAC, 0x2F, 0xFC, 0x0F, 0xF1, 0x30, 0x24, 0x96, 0xCC, 0x92, 0xF9, 0xC6, 0xE5, 0x0B, 0x9D, 0xB1, 0x72, 0xB4};
  UINT8 Recon[] = {0x02, 0x2F, 0x85, 0xD2, 0x72, 0x23, 0x6B, 0xE7, 0x99, 0xD8, 0xAE, 0x7E, 0xE8, 0x32, 0x51, 0x95, 0x39, 0x9B, 0x18, 0xBE, 0xBE, 0xAC, 0x76, 0x46, 0x57, 0xD6, 0xE5, 0x6E, 0x77, 0x09, 0x14, 0x31, 0x90};
  UINT8 pHash[] = {0xF6, 0x62, 0x50, 0x10, 0x4C, 0xC2, 0x1C, 0x1E, 0xAB, 0x1C, 0x13, 0xFB, 0x69, 0x8E, 0x20, 0x60, 0x2E, 0xBB, 0x96, 0xE6, 0x4B, 0x8F, 0x69, 0x9B, 0x41, 0xBD, 0x2A, 0x85, 0x78, 0x3A, 0x8E, 0x66};
  UINT8 pPubKey[32];
  EC_GROUP *pGroup = NULL;
  EC_POINT *point = NULL;
  UINT32 i;
  char logstring[250];
  if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    return FAILURE;
  }
  point = PublicKeyReconstruct(pHash, Recon, CAKey, pPubKey, pGroup);
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,pPubKey,33,"Pub Key \n");
      AWSEC_LOG(ASM,NULL,0,"End of Pub Key \n");
  }
    if(point)
      EC_POINT_clear_free(point);
    if (pGroup)
      EC_GROUP_clear_free(pGroup);

  return SUCCESS;
}

UINT8 * EC_XOR(UINT8 *pBuf, UINT32 uiBufLen, UINT8 *pKey, UINT32 uiKeyLen, UINT8 *OutBuf)
{
  UINT32 i = 0;
    for(i = 0; i < 8; i++)
    {
      OutBuf[i] = (pBuf[i]^0x00);
    }
    for(i = 8; i < 16; i++)
    {
      OutBuf[i] = (pBuf[i]^pKey[i % uiKeyLen]);
    }
    return OutBuf;
}

UINT32 VerificationKey(UINT8 *pPrivSignKey, UINT8 *pPrivAESSignKey, UINT32 i, UINT32 j, UINT8 * pVerificationKey)
{
  BIGNUM * order = BN_new(), *bn1 = NULL, *bn2 = NULL, *bn3 = BN_new();
  UINT8 pZeroBytes[] = {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
  UINT8 pOneBytes[] = {0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF};
  UINT8 pTmpBuf[32], pTmpBuf1[16], pTmpBuf2[16], pStr1[8], pStr2[4], pTmpBuf3[16], pTmpBuf4[16];
  UINT8 pCipherText[32], *pBufStr = NULL;
  UINT32 uiCipherTextLen, uiTmpBufLen, iRet = FAILURE, k = 0, BufLen, len;
  UINT8 pBuf[32];
  uint64_t idx;
  BN_CTX *ctx = NULL;
  idx = 0x100000000 * (uint64_t)i + (uint64_t)j;
  idx = htobe64(idx);
  char logstring[250];
  memcpy(pStr1, &idx, 8);

  EC_XOR(pZeroBytes, sizeof(pZeroBytes), pStr1, 8, pTmpBuf1);
  EC_XOR(pOneBytes, sizeof(pOneBytes), pStr1, 8, pTmpBuf2);

  if (AWSecEncrypt(CIPHER_ALGO_AES128_ECB, pPrivAESSignKey, NULL, 0, pTmpBuf1, 16, pTmpBuf3, &uiCipherTextLen, 0) == FAILURE)
  {
    if(asm_log_level >= LOG_CRITICAL)
	AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecEncrypt(): %s\n",__FILE__,__LINE__, strerror(errno));
    goto err;
  }
  if (AWSecEncrypt(CIPHER_ALGO_AES128_ECB, pPrivAESSignKey, NULL, 0, pTmpBuf2, 16, pTmpBuf4, &uiCipherTextLen, 0) == FAILURE)
  {
    if(asm_log_level >= LOG_CRITICAL)
	AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecEncrypt(): %s\n",__FILE__,__LINE__, strerror(errno));
    goto err;
  }
  for(i = 0; i < 16; i++)
  {
    pTmpBuf[i] = pTmpBuf3[i];
  }
  for(i = 16, j = 0; i < 32; i++, j++)
  {
    pTmpBuf[i] = pTmpBuf4[j];
  }

  bn2 = BN_bin2bn(pTmpBuf, 32, BN_new());

  if ((ctx = BN_CTX_new()) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:BN_CTX_new(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }

  if (!EC_GROUP_get_order(pGroup256, order, NULL))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_GROUP_get_order(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  bn1 = BN_bin2bn(pPrivSignKey, 32, BN_new());
  if(!BN_mod_add(bn3, bn1, bn2, order, ctx))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:BN_mod_add(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  len = BN_num_bytes(bn3);
  for(k = 0; k < (32 - len); k++)
   pVerificationKey[k] = 0x00;

  pBufStr = BN_bn2hex(bn3);
  uiTmpBufLen = HexStr2ByteStr(pBufStr, &pVerificationKey[32-len], 32 - (32 - len));
  OPENSSL_free(pBufStr);

  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,pVerificationKey,32,"pVerificationKey Start\n");
      AWSEC_LOG(ASM,NULL,0,"End of pVerificationKey\n");
  }
  iRet = SUCCESS;

err:
  if(bn1)
    BN_free(bn1);
  if(bn2)
    BN_free(bn2);
  if(bn3)
    BN_free(bn3);
  if(order)
    BN_free(order);
  if (ctx != NULL)
    BN_CTX_free(ctx);
  return iRet;
}

EC_POINT * PrivateKeyReconstruct(UINT8 *pHash, UINT8 *pPrivKeyReconBuf, UINT8 *pEphemeralPrivKey, UINT8 *pPrivateKey, EC_GROUP *pGroup)
{
  BIGNUM * order = NULL, *bn1 = NULL, *bn2 = NULL, *hash = NULL, *priv_recon = NULL, *priv = NULL;
  UINT32 uiBufLen = 32, iRet = FAILURE, uiTmpBufLen;
  UINT8 *pBufStr = NULL;
  BN_CTX *ctx = NULL;
  EC_POINT *point = NULL, *pTmpPoint3 = NULL;
  char logstring[250];

  order = BN_new();
  bn2 = BN_new();
  bn1 = BN_new();

  if ((ctx = BN_CTX_new()) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:BN_CTX_new(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if (!EC_GROUP_get_order(pGroup, order, NULL))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_GROUP_get_order(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }

  hash = BN_bin2bn(pHash, uiBufLen, BN_new());
  priv = BN_bin2bn(pEphemeralPrivKey, uiBufLen, BN_new());

  priv_recon = BN_bin2bn(pPrivKeyReconBuf, uiBufLen, BN_new());

  if (!BN_mul(bn1, hash, priv, ctx))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:BN_mul(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if(!BN_mod_add(bn2, priv_recon, bn1, order, ctx))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:BN_mod_add(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
//#ifdef NAZEER
  {
    if(!(pTmpPoint3 = EC_POINT_new(pGroup)))
    {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_new(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    if ((point = EC_GROUP_get0_generator(pGroup)) == NULL)
    {
      if(asm_log_level >= LOG_CRITICAL){
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_GROUP_get0_generator(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    if(!EC_POINT_mul(pGroup, pTmpPoint3, bn2, point, NULL, ctx))
    {
      if(asm_log_level >= LOG_CRITICAL){
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_mul(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    {
    UINT32 uiPubKeyLen = 33, i;
    UINT8 pPubKey[40];
    uiPubKeyLen = EC_POINT_point2oct(pGroup, pTmpPoint3, POINT_CONVERSION_COMPRESSED, pPubKey, uiPubKeyLen, NULL);
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,pPubKey,uiPubKeyLen,"Public Key regenerated using Private Key and Generator\n");
        AWSEC_LOG(ASM,NULL,0,"End of Public Key\n");
    }
    }
  }
//#endif
  pBufStr = BN_bn2hex(bn2);
  uiTmpBufLen = HexStr2ByteStr(pBufStr, pPrivateKey, 32);
  OPENSSL_free(pBufStr);
  iRet = SUCCESS;
err: 
  if (iRet != SUCCESS)
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%s: %d: Returned error  library = %s, func = %s, reason = %s, {error = %s}\n",__FILE__,__FUNCTION__,__LINE__,ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
    }
  if(priv)
    BN_free(priv);
  if(hash)
    BN_free(hash);
  if(order)
    BN_free(order);
  if(bn1)
    BN_free(bn1);
  if(bn2)
    BN_free(bn2);
  if(priv_recon)
    BN_free(priv_recon);
  if (ctx != NULL)
    BN_CTX_free(ctx);

  return iRet;
}

EC_POINT * PublicKeyReconstruct(UINT8 *pHash, UINT8 *pPubKeyRecon, UINT8 *pCAPubKeyRecon, UINT8 *pPubKey, EC_GROUP *pGroup)
{
  EC_POINT *pTmpPoint1 = NULL, *pTmpPoint2 = NULL, *pTmpPoint3 = NULL;
  BN_CTX *ctx = NULL;
  BIGNUM *hash = NULL, *order = BN_new();
  UINT32 uiKeyLen, uiPubKeyLen, i;

  if ((ctx = BN_CTX_new()) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:BN_CTX_new(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }

  hash = BN_bin2bn(pHash, 32, BN_new());
  
  if (!EC_GROUP_get_order(pGroup, order, NULL))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_GROUP_get_order(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if(!(pTmpPoint1 = EC_POINT_new(pGroup)))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_new(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if ((pPubKeyRecon[0] == 0x03) || (pPubKeyRecon[0] == 0x02))
    uiPubKeyLen = uiKeyLen = 33;
  else
    uiPubKeyLen = uiKeyLen = 32;
  printf("%s %d recons[0]:%02x keylen:%d\n",__func__,__LINE__,pPubKeyRecon[0],uiKeyLen);
  if (!(EC_POINT_oct2point(pGroup, pTmpPoint1, pPubKeyRecon, uiKeyLen, ctx)))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_oct2point(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  printf("%s %d\n",__func__,__LINE__);
  if ((pCAPubKeyRecon[0] == 0x03) || (pCAPubKeyRecon[0] == 0x02))
    uiKeyLen = 33;
  else
    uiKeyLen = 32;
  if(!(pTmpPoint2 = EC_POINT_new(pGroup)))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_new(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  printf("%s %d\n",__func__,__LINE__);
  if (!(EC_POINT_oct2point(pGroup, pTmpPoint2, pCAPubKeyRecon, uiKeyLen, ctx)))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_oct2point(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  printf("%s %d\n",__func__,__LINE__);

  if(!(pTmpPoint3 = EC_POINT_new(pGroup)))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_new(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  printf("%s %d\n",__func__,__LINE__);
  if (! EC_POINT_is_on_curve(pGroup, pTmpPoint2, ctx))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_is_on_curve(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  printf("%s %d\n",__func__,__LINE__);
  
  if(!EC_POINT_mul(pGroup, pTmpPoint3, NULL, pTmpPoint1, hash, ctx))
  {
    if(asm_log_level >= LOG_CRITICAL){
	AWSEC_LOG(ASM,NULL,0,"%s:%d:ec_point_mul(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if (! EC_POINT_is_on_curve(pGroup, pTmpPoint3, ctx))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:ec_point_is_on_curve(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if(!EC_POINT_add(pGroup, pTmpPoint3, pTmpPoint3, pTmpPoint2, ctx))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:ec_point_add(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  printf("%s %d\n",__func__,__LINE__);
  uiPubKeyLen = EC_POINT_point2oct(pGroup, pTmpPoint3, POINT_CONVERSION_COMPRESSED, pPubKey, uiPubKeyLen, ctx);
#if 1 
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,pPubKey,uiPubKeyLen,"2. public key regenerated\n");
        AWSEC_LOG(ASM,NULL,0,"2. end of public key\n");
    }
  printf("%s %d\n",__func__,__LINE__);
#endif

err:
  if (pTmpPoint3 == NULL)
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%s: %d: Returned error  library = %s, func = %s, reason = %s, {error = %s}\n",__FILE__,__FUNCTION__,__LINE__,ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
    }
  if (hash)
    BN_free(hash);
  if (order)
    BN_free(order);
  if (ctx != NULL)
    BN_CTX_free(ctx);
  if(pTmpPoint1)
    EC_POINT_clear_free(pTmpPoint1);
  if(pTmpPoint2)
    EC_POINT_clear_free(pTmpPoint2);

  return pTmpPoint3;
}


UINT32 EC_KEY_regenerate_key(EC_KEY *ECKey, BIGNUM *PrivKey)
{
    INT32 iRet = 0;
    BN_CTX *ctx = NULL;
    EC_POINT *pPubKey = NULL;
    const EC_GROUP *pGroup;

    if (!ECKey) return 0;

    EC_KEY_get0_group(ECKey);

    if ((ctx = BN_CTX_new()) == NULL)
        goto err;

    pPubKey = EC_POINT_new(pGroup);

    if (pPubKey == NULL)
        goto err;

    if (!EC_POINT_mul(pGroup, pPubKey, PrivKey, NULL, NULL, ctx))
        goto err;

    EC_KEY_set_private_key(ECKey, PrivKey);
    EC_KEY_set_public_key(ECKey, pPubKey);

    iRet = 1;
err:

    if (pPubKey)
        EC_POINT_clear_free(pPubKey);
    if (ctx != NULL)
        BN_CTX_free(ctx);

    return iRet;
}

// Perform ECDSA key recovery (see SEC1 4.1.6) for curves over (mod p)-fields
// recid selects which key is recovered
// if check is nonzero, additional checks are performed
int ECDSA_SIG_recover_key_GFp(EC_KEY *eckey, ECDSA_SIG *ecsig, const unsigned char *msg, int msglen, int recid, int check)
{
    if (!eckey) return 0;

    int ret = 0;
    BN_CTX *ctx = NULL;

    BIGNUM *x = NULL;
    BIGNUM *e = NULL;
    BIGNUM *order = NULL;
    BIGNUM *sor = NULL;
    BIGNUM *eor = NULL;
    BIGNUM *field = NULL;
    EC_POINT *R = NULL;
    EC_POINT *O = NULL;
    EC_POINT *Q = NULL;
    BIGNUM *rr = NULL;
    BIGNUM *zero = NULL;
    int n = 0;
    int i = recid / 2;

    const EC_GROUP *group = EC_KEY_get0_group(eckey);
    if ((ctx = BN_CTX_new()) == NULL) { ret = -1; goto err; }
    BN_CTX_start(ctx);
    order = BN_CTX_get(ctx);
    if (!EC_GROUP_get_order(group, order, ctx)) { ret = -2; goto err; }
    x = BN_CTX_get(ctx);
    if (!BN_copy(x, order)) { ret=-1; goto err; }
    if (!BN_mul_word(x, i)) { ret=-1; goto err; }
    if (!BN_add(x, x, ecsig->r)) { ret=-1; goto err; }
    field = BN_CTX_get(ctx);
    if (!EC_GROUP_get_curve_GFp(group, field, NULL, NULL, ctx)) { ret=-2; goto err; }
    if (BN_cmp(x, field) >= 0) { ret=0; goto err; }
    if ((R = EC_POINT_new(group)) == NULL) { ret = -2; goto err; }
    if (!EC_POINT_set_compressed_coordinates_GFp(group, R, x, recid % 2, ctx)) { ret=0; goto err; }
    if (check)
    {
        if ((O = EC_POINT_new(group)) == NULL) { ret = -2; goto err; }
        if (!EC_POINT_mul(group, O, NULL, R, order, ctx)) { ret=-2; goto err; }
        if (!EC_POINT_is_at_infinity(group, O)) { ret = 0; goto err; }
    }
    if ((Q = EC_POINT_new(group)) == NULL) { ret = -2; goto err; }
    n = EC_GROUP_get_degree(group);
    e = BN_CTX_get(ctx);
    if (!BN_bin2bn(msg, msglen, e)) { ret=-1; goto err; }
    if (8*msglen > n) BN_rshift(e, e, 8-(n & 7));
    zero = BN_CTX_get(ctx);
    if (!BN_zero(zero)) { ret=-1; goto err; }
    if (!BN_mod_sub(e, zero, e, order, ctx)) { ret=-1; goto err; }
    rr = BN_CTX_get(ctx);
    if (!BN_mod_inverse(rr, ecsig->r, order, ctx)) { ret=-1; goto err; }
    sor = BN_CTX_get(ctx);
    if (!BN_mod_mul(sor, ecsig->s, rr, order, ctx)) { ret=-1; goto err; }
    eor = BN_CTX_get(ctx);
    if (!BN_mod_mul(eor, e, rr, order, ctx)) { ret=-1; goto err; }
    if (!EC_POINT_mul(group, Q, eor, R, sor, ctx)) { ret=-2; goto err; }
    if (!EC_KEY_set_public_key(eckey, Q)) { ret=-2; goto err; }

    ret = 1;

err:
    if (ctx) {
        BN_CTX_end(ctx);
        BN_CTX_free(ctx);
    }
    if (R != NULL) EC_POINT_clear_free(R);
    if (O != NULL) EC_POINT_clear_free(O);
    if (Q != NULL) EC_POINT_clear_free(Q);
    return ret;
}

int Fast_ecdsa_sign_setup(EC_KEY *eckey, BN_CTX *ctx_in, BIGNUM **kinvp,
		BIGNUM **rp, int *yInfo)
{
	BN_CTX   *ctx = NULL;
	BIGNUM	 *k = NULL, *r = NULL, *order = NULL, *X = NULL, *Y = NULL, *myR = NULL;
	EC_POINT *tmp_point=NULL;
	const EC_GROUP *group;
	int 	 ret = 0, uiPubKeyLen = 33;
        char pPubKey[33];

	if (eckey == NULL || (group = EC_KEY_get0_group(eckey)) == NULL)
	{
		ECDSAerr(ECDSA_F_ECDSA_SIGN_SETUP, ERR_R_PASSED_NULL_PARAMETER);
		return FAILURE;
	}

	if (ctx_in == NULL) 
	{
		if ((ctx = BN_CTX_new()) == NULL)
		{
			ECDSAerr(ECDSA_F_ECDSA_SIGN_SETUP,ERR_R_MALLOC_FAILURE);
			return FAILURE;
		}
	}
	else
		ctx = ctx_in;

	k     = BN_new();	/* this value is later returned in *kinvp */
	r     = BN_new();	/* this value is later returned in *rp    */
	order = BN_new();
	X     = BN_new();
	Y     = BN_new();
	if (!k || !r || !order || !X)
	{
		ECDSAerr(ECDSA_F_ECDSA_SIGN_SETUP, ERR_R_MALLOC_FAILURE);
		goto err;
	}
	if ((tmp_point = EC_POINT_new(group)) == NULL)
	{
		ECDSAerr(ECDSA_F_ECDSA_SIGN_SETUP, ERR_R_EC_LIB);
		goto err;
	}
	if (!EC_GROUP_get_order(group, order, ctx))
	{
		ECDSAerr(ECDSA_F_ECDSA_SIGN_SETUP, ERR_R_EC_LIB);
		goto err;
	}
	
	do
	{
		/* get random k */	
		do
			if (!BN_rand_range(k, order))
			{
				ECDSAerr(ECDSA_F_ECDSA_SIGN_SETUP,
				 ECDSA_R_RANDOM_NUMBER_GENERATION_FAILED);	
				goto err;
			}
		while (BN_is_zero(k));

		/* We do not want timing information to leak the length of k,
		 * so we compute G*k using an equivalent scalar of fixed
		 * bit-length. */

		if (!BN_add(k, k, order)) goto err;
		if (BN_num_bits(k) <= BN_num_bits(order))
			if (!BN_add(k, k, order)) goto err;

		/* compute r the x-coordinate of generator * k */
		if (!EC_POINT_mul(group, tmp_point, k, NULL, NULL, ctx))
		{
			ECDSAerr(ECDSA_F_ECDSA_SIGN_SETUP, ERR_R_EC_LIB);
			goto err;
		}
 
#ifdef NAZEER
                uiPubKeyLen = EC_POINT_point2oct(group, tmp_point, POINT_CONVERSION_COMPRESSED, pPubKey, uiPubKeyLen, ctx);
                r = myR = BN_bin2bn(&pPubKey[1], 32, BN_new());
                *yInfo = pPubKey[0];

		if(asm_log_level >= LOG_INF){
		    AWSEC_LOG(ASM,NULL,0,"yInfo = %d\n\n", *yInfo);
		}
BN_print_fp(stdout, myR);
printf("\n");
#endif

		if (EC_METHOD_get_field_type(EC_GROUP_method_of(group)) == NID_X9_62_prime_field)
		{
#ifdef NAZEER
	                if (!EC_POINT_set_compressed_coordinates_GFp(group, tmp_point, X, 1, ctx)) 
			{
				ECDSAerr(ECDSA_F_ECDSA_SIGN_SETUP,ERR_R_EC_LIB);
				goto err;
			}
#endif
			if (!EC_POINT_get_affine_coordinates_GFp(group, tmp_point, X, Y, ctx))
			{
				ECDSAerr(ECDSA_F_ECDSA_SIGN_SETUP,ERR_R_EC_LIB);
				goto err;
			}
		}
#ifndef OPENSSL_NO_EC2M
		else /* NID_X9_62_characteristic_two_field */
		{
			if (!EC_POINT_get_affine_coordinates_GF2m(group,
				tmp_point, X, Y, ctx))
			{
				ECDSAerr(ECDSA_F_ECDSA_SIGN_SETUP,ERR_R_EC_LIB);
				goto err;
			}
		}
#endif
		if (!BN_nnmod(r, X, order, ctx))
		{
			ECDSAerr(ECDSA_F_ECDSA_SIGN_SETUP, ERR_R_BN_LIB);
			goto err;
		}
	}
	while (BN_is_zero(r));
#ifdef NAZEER
if(asm_log_level >= LOG_INF){
    AWSEC_LOG(ASM,NULL,0,"yInfo = %d\n\n", *yInfo);
}
BN_print_fp(stdout, r);
printf("\n");
BN_print_fp(stdout, k);
printf("\n");
#endif
	/* compute the inverse of k */
	if (!BN_mod_inverse(k, k, order, ctx))
	{
		ECDSAerr(ECDSA_F_ECDSA_SIGN_SETUP, ERR_R_BN_LIB);
		goto err;	
	}
	/* clear old values if necessary */
	if (*rp != NULL)
		BN_clear_free(*rp);
	if (*kinvp != NULL) 
		BN_clear_free(*kinvp);
	/* save the pre-computed values  */
	*rp    = r;
	*kinvp = k;
        *yInfo = (BN_is_odd(Y)) ? 0x03 : 0x02;

#ifdef NAZEER
if(asm_log_level >= LOG_INF){
    AWSEC_LOG(ASM,NULL,0,"yInfo = %d\n\n", *yInfo);
}
BN_print_fp(stdout, r);
printf("\n");
BN_print_fp(stdout, k);
printf("\n");
#endif
	ret = SUCCESS;
err:
	if (ret == FAILURE)
	{
		if (k != NULL) BN_clear_free(k);
		if (r != NULL) BN_clear_free(r);
	}
	if (ctx_in == NULL) 
		BN_CTX_free(ctx);
	if (order != NULL)
		BN_free(order);
	if (tmp_point != NULL) 
		EC_POINT_free(tmp_point);
	if (X)
		BN_clear_free(X);
	if (Y)
		BN_clear_free(Y);
	return(ret);
}


ECDSA_SIG *Fast_ecdsa_do_sign(const unsigned char *dgst, int dgst_len, 
		const BIGNUM *in_kinv, const BIGNUM *in_r, EC_KEY *eckey, int *yInfo)
{
	int     ok = 0, i;
	BIGNUM *kinv=NULL, *s, *m=NULL,*tmp=NULL,*order=NULL;
	const BIGNUM *ckinv;
	BN_CTX     *ctx = NULL;
	const EC_GROUP   *group;
	ECDSA_SIG  *ret;
	//ECDSA_DATA *ecdsa;
	const BIGNUM *priv_key;

	//ecdsa    = ecdsa_check(eckey);
	group    = EC_KEY_get0_group(eckey);
	priv_key = EC_KEY_get0_private_key(eckey);
	
	if (group == NULL || priv_key == NULL /* ||  ecdsa == NULL */)
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_SIGN, ERR_R_PASSED_NULL_PARAMETER);
		return NULL;
	}

	ret = ECDSA_SIG_new();
	if (!ret)
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_SIGN, ERR_R_MALLOC_FAILURE);
		return NULL;
	}
	s = ret->s;

	if ((ctx = BN_CTX_new()) == NULL || (order = BN_new()) == NULL ||
		(tmp = BN_new()) == NULL || (m = BN_new()) == NULL)
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_SIGN, ERR_R_MALLOC_FAILURE);
		goto err;
	}

	if (!EC_GROUP_get_order(group, order, ctx))
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_SIGN, ERR_R_EC_LIB);
		goto err;
	}
	i = BN_num_bits(order);
	/* Need to truncate digest if it is too long: first truncate whole
	 * bytes.
	 */
	if (8 * dgst_len > i)
		dgst_len = (i + 7)/8;
	if (!BN_bin2bn(dgst, dgst_len, m))
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_SIGN, ERR_R_BN_LIB);
		goto err;
	}
	/* If still too long truncate remaining bits with a shift */
	if ((8 * dgst_len > i) && !BN_rshift(m, m, 8 - (i & 0x7)))
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_SIGN, ERR_R_BN_LIB);
		goto err;
	}
	do
	{
		if (in_kinv == NULL || in_r == NULL)
		{
			if (Fast_ecdsa_sign_setup(eckey, ctx, &kinv, &ret->r, yInfo) != SUCCESS)
			{
				ECDSAerr(ECDSA_F_ECDSA_DO_SIGN,ERR_R_ECDSA_LIB);
				goto err;
			}
			ckinv = kinv;
		}
		else
		{
			ckinv  = in_kinv;
			if (BN_copy(ret->r, in_r) == NULL)
			{
				ECDSAerr(ECDSA_F_ECDSA_DO_SIGN, ERR_R_MALLOC_FAILURE);
				goto err;
			}
		}

		if (!BN_mod_mul(tmp, priv_key, ret->r, order, ctx))
		{
			ECDSAerr(ECDSA_F_ECDSA_DO_SIGN, ERR_R_BN_LIB);
			goto err;
		}
		if (!BN_mod_add_quick(s, tmp, m, order))
		{
			ECDSAerr(ECDSA_F_ECDSA_DO_SIGN, ERR_R_BN_LIB);
			goto err;
		}
		if (!BN_mod_mul(s, s, ckinv, order, ctx))
		{
			ECDSAerr(ECDSA_F_ECDSA_DO_SIGN, ERR_R_BN_LIB);
			goto err;
		}
		if (BN_is_zero(s))
		{
			/* if kinv and r have been supplied by the caller
			 * don't to generate new kinv and r values */
			if (in_kinv != NULL && in_r != NULL)
			{
				ECDSAerr(ECDSA_F_ECDSA_DO_SIGN, ECDSA_R_NEED_NEW_SETUP_VALUES);
				goto err;
			}
		}
		else
			/* s != 0 => we have a valid signature */
			break;
	}
	while (1);

	ok = 1;
err:
	if (!ok)
	{
		ECDSA_SIG_free(ret);
		ret = NULL;
	}
	if (ctx)
		BN_CTX_free(ctx);
	if (m)
		BN_clear_free(m);
	if (tmp)
		BN_clear_free(tmp);
	if (order)
		BN_free(order);
	if (kinv)
		BN_clear_free(kinv);
	return ret;
}

int Fast_ecdsa_do_verify(const unsigned char *dgst, int dgst_len,
		const ECDSA_SIG *sig, EC_KEY *eckey, int yInfo)
{
	int ret = FAILURE, i, uiPubKeyLen = 33, NumBytes;
	BN_CTX   *ctx;
	BIGNUM   *order, *u1, *u2, *m, *X, *myR;
	EC_POINT *point = NULL, *point1 = NULL;
	const EC_GROUP *group;
	const EC_POINT *pub_key;
        char pPubKey[33];
        char logstring[250];

	/* check input values */
	if (eckey == NULL || (group = EC_KEY_get0_group(eckey)) == NULL ||
	    (pub_key = EC_KEY_get0_public_key(eckey)) == NULL || sig == NULL)
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ECDSA_R_MISSING_PARAMETERS);
		return FAILURE;
	}

	ctx = BN_CTX_new();
	if (!ctx)
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_MALLOC_FAILURE);
		return FAILURE;
	}
	BN_CTX_start(ctx);
	order = BN_CTX_get(ctx);	
	u1    = BN_CTX_get(ctx);
	u2    = BN_CTX_get(ctx);
	m     = BN_CTX_get(ctx);
	X     = BN_CTX_get(ctx);
	if (!X)
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_BN_LIB);
		goto err;
	}
	if (!EC_GROUP_get_order(group, order, ctx))
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_EC_LIB);
		goto err;
	}

	if (BN_is_zero(sig->r)          || BN_is_negative(sig->r) || 
	    BN_ucmp(sig->r, order) >= 0 || BN_is_zero(sig->s)  ||
	    BN_is_negative(sig->s)      || BN_ucmp(sig->s, order) >= 0)
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ECDSA_R_BAD_SIGNATURE);
		ret = 0;	/* signature is invalid */
		goto err;
	}
        /* digest -> m */
        i = BN_num_bits(order);
        /* Need to truncate digest if it is too long: first truncate whole
         * bytes.
         */
        if (8 * dgst_len > i)
                dgst_len = (i + 7)/8;
        if (!BN_bin2bn(dgst, dgst_len, m))
        {
                ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_BN_LIB);
                goto err;
        }
        /* If still too long truncate remaining bits with a shift */
        if ((8 * dgst_len > i) && !BN_rshift(m, m, 8 - (i & 0x7)))
        {
                ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_BN_LIB);
                goto err;
        }
        if ((point = EC_POINT_new(group)) == NULL)
        {
                ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_MALLOC_FAILURE);
                goto err;
        }
        if ((point1 = EC_POINT_new(group)) == NULL)
        {
                ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_MALLOC_FAILURE);
                goto err;
        }

        if (!EC_POINT_mul(group, point, m, pub_key, sig->r, ctx))
        {
                ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_EC_LIB);
                goto err;
        }
        pPubKey[0] = yInfo;
        NumBytes = BN_bn2bin(sig->r, &pPubKey[1]);
        //printf("NumBytes = %d\n", NumBytes);
#ifdef NAZEER
if(asm_log_level >= LOG_INF){
    AWSEC_LOG(ASM,(UINT8 *)pPubKey,33,"Start of received data\n");
    AWSEC_LOG(ASM,NULL,0,"End of received data\n");
}
#endif
        if(!(EC_POINT_oct2point(group, point1, pPubKey, NumBytes + 1, ctx)))
        {
	    if(asm_log_level >= LOG_CRITICAL){
                AWSEC_LOG(ASM,NULL,0,"error in converting compressed point\n");
		AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_oct2point(): %s\n",__FILE__,__LINE__, strerror(errno));
	    }
                goto err;
        }
        if (!EC_POINT_mul(group, point1, NULL, point1, sig->s, ctx))
        {
                ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_EC_LIB);
                goto err;
        }
		
#ifdef NAZEER
{
  int uiPubKeyLen = 33;
  uiPubKeyLen = EC_POINT_point2oct(group, point, POINT_CONVERSION_COMPRESSED, pPubKey, uiPubKeyLen, ctx);
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,(UINT8 *)pPubKey,33,"Start of point data\n");
      AWSEC_LOG(ASM,NULL,0,"End of point data\n");
  }
  uiPubKeyLen = EC_POINT_point2oct(group, point1, POINT_CONVERSION_COMPRESSED, pPubKey, uiPubKeyLen, ctx);

  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,(UINT8 *)pPubKey,33,"Start of point1 data\n");
      AWSEC_LOG(ASM,NULL,0,"End of point data\n");
  }
}
#endif
        if(EC_POINT_cmp(group, point, point1, ctx) == 0)
	    ret = SUCCESS;

#if 0		
#ifdef NAZEER
	/* calculate tmp1 = inv(S) mod order */
	if (!BN_mod_inverse(u2, sig->s, order, ctx))
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_BN_LIB);
		goto err;
	}
	/* digest -> m */
	i = BN_num_bits(order);
	/* Need to truncate digest if it is too long: first truncate whole
	 * bytes.
	 */
	if (8 * dgst_len > i)
		dgst_len = (i + 7)/8;
	if (!BN_bin2bn(dgst, dgst_len, m))
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_BN_LIB);
		goto err;
	}
	/* If still too long truncate remaining bits with a shift */
	if ((8 * dgst_len > i) && !BN_rshift(m, m, 8 - (i & 0x7)))
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_BN_LIB);
		goto err;
	}
	/* u1 = m * tmp mod order */
	if (!BN_mod_mul(u1, m, u2, order, ctx))
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_BN_LIB);
		goto err;
	}
	/* u2 = r * w mod q */
	if (!BN_mod_mul(u2, sig->r, u2, order, ctx))
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_BN_LIB);
		goto err;
	}

	if ((point = EC_POINT_new(group)) == NULL)
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_MALLOC_FAILURE);
		goto err;
	}
	if (!EC_POINT_mul(group, point, u1, pub_key, u2, ctx))
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_EC_LIB);
		goto err;
	}
        uiPubKeyLen = EC_POINT_point2oct(group, point, POINT_CONVERSION_COMPRESSED, pPubKey, uiPubKeyLen, ctx);
                myR = BN_bin2bn(&pPubKey[1], 32, BN_new());
                //printf("pPubKey = %d, yInfo = %d\n", pPubKey[0], yInfo);
	        ret = (BN_ucmp(myR, sig->r) == 0);
                goto err;
                if(ret == 0)
{
printf("\n");
BN_print_fp(stdout, myR);
printf("\n");
BN_print_fp(stdout, sig->r);
printf("\n");
}

printf("\n");
BN_print_fp(stdout, myR);
printf("\n");
                printf("First byte = %x\n", pPubKey[0]);
	if (EC_METHOD_get_field_type(EC_GROUP_method_of(group)) == NID_X9_62_prime_field)
	{
		if (!EC_POINT_get_affine_coordinates_GFp(group,
			point, X, NULL, ctx))
		{
			ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_EC_LIB);
			goto err;
		}
	}
#ifndef OPENSSL_NO_EC2M
	else /* NID_X9_62_characteristic_two_field */
	{
		if (!EC_POINT_get_affine_coordinates_GF2m(group,

			point, X, NULL, ctx))
		{
			ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_EC_LIB);
			goto err;
		}
	}
#endif	
	if (!BN_nnmod(u1, X, order, ctx))
	{
		ECDSAerr(ECDSA_F_ECDSA_DO_VERIFY, ERR_R_BN_LIB);
		goto err;
	}
	/*  if the signature is correct u1 is equal to sig->r */
	ret = (BN_ucmp(u1, sig->r) == 0);
#endif
#endif

err:
	BN_CTX_end(ctx);
	BN_CTX_free(ctx);
	if (point)
		EC_POINT_free(point);
	if (point1)
		EC_POINT_free(point1);
	return ret;
}

/***********************************************************************
 * Function Name 	: AWSecEncrypt
 * Description   	: Encrypts the data with given Algorithm.
 *				 
 * Input        	: None
 * Output        	: None
 * Return value  	: returns SUCCESS
 ***********************************************************************/

UINT32 AWSecEncrypt(UINT32 cipherAlgo, UINT8 * pCipherKey, UINT8 * pIVNonce, UINT32 uiNonceLen, UINT8 * pBuf, UINT32 uiBufLen, UINT8 *pCipherText, UINT32 *uiCipherTextLen, UINT32 uiTagLen)
{
  UINT32 uiTmpLen;
  EVP_CIPHER_CTX cipher_ctx;
  /** If valid Key, IV passed to this function, we can set key Length, IVLen by using the sizeof those strings. 
   * Tag length need to be set as one of the values from 4, 6, 10, 12, 14 or 16. The same value should be sent to Decrypt function as well. **/
  INT32 uiCipherLen = uiBufLen, i;
  char logstring[250];
#ifdef NAZEER
if(asm_log_level >= LOG_INF){
    AWSEC_LOG(ASM,NULL,0,"pCipherKey  = %d, pIVNonce = %d, pBuf = %d, uiBufLen = %d\n", strlen(pCipherKey), strlen(pIVNonce), strlen (pBuf), uiBufLen);
}
if(asm_log_level >= LOG_INF){
    AWSEC_LOG(ASM,pPuf,uiBufLen,"pCipherText start\n");
    AWSEC_LOG(ASM,NULL,0,"End of uiBufLen start\n");
}
if(asm_log_level >= LOG_INF){
    AWSEC_LOG(ASM,pIVNonce,15,"pIVNonce start\n");
    AWSEC_LOG(ASM,NULL,0,"End of pIVNonce start\n");
}
if(asm_log_level >= LOG_INF){
    AWSEC_LOG(ASM,pCipherKey,20,"pCipherKey start\n");
    AWSEC_LOG(ASM,NULL,0,"End of pCipherKey start\n");
}
   
  /* Using openssl pseudo random generator, we are assigning the Key and Initial Vector. 
   * IV can also be treated as Nonce */
if(asm_log_level >= LOG_INF){
    AWSEC_LOG(ASM,NULL,0,"%s: %d: %s: Entry\n", __FUNCTION__, __LINE__, __FILE__);
}
#endif
  if((!pCipherKey) || (!pBuf) || (!pCipherText))
  {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"NULL pointers sent to the AWSecEncrypt function\n");
    goto err;
  }
  EVP_CIPHER_CTX_init(&cipher_ctx);
  switch(cipherAlgo)
  {
    case CIPHER_ALGO_AES128_CCM:
    {
      /** As per standards, we need to set these parameters **/
      if(EVP_EncryptInit(&cipher_ctx, EVP_aes_128_ccm(  ), NULL, NULL) != 1)
      {
        if(asm_log_level >= LOG_CRITICAL){
            AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_EncryptInit(): %s\n",__FILE__,__LINE__, strerror(errno));
        }
        goto err;
      }

      break;
    }
    case CIPHER_ALGO_AES128_ECB:
    {
      if(EVP_EncryptInit_ex(&cipher_ctx, EVP_aes_128_ecb( ), NULL, pCipherKey, NULL) != 1)
      {
        if(asm_log_level >= LOG_CRITICAL){
            AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_EncryptInit_ex(): %s\n",__FILE__,__LINE__, strerror(errno));
        }
        goto err;
      }
      break;
    }
    case CIPHER_ALGO_AES128_CBC:
    {
      if(EVP_EncryptInit_ex(&cipher_ctx, EVP_aes_128_cbc( ), NULL, pCipherKey, NULL) != 1)
      {
        if(asm_log_level >= LOG_CRITICAL){
            AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_EncryptInit_ex(): %s\n",__FILE__,__LINE__, strerror(errno));
        }
        goto err;
      }
      break;
    }
    default:
    {
      if(asm_log_level >= LOG_CRITICAL)
          AWSEC_LOG(ASM,NULL,0,"Unknown Algorithm\n"); 
      *uiCipherTextLen = 0;
      return FAILURE;
    }
  }
  if (cipherAlgo == CIPHER_ALGO_AES128_CCM)
  {
    /** Enable padding as per standard **/
    if( EVP_CIPHER_CTX_set_padding(&cipher_ctx, 1) != 1)
    {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_CIPHER_CTX_set_padding(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    if(EVP_CIPHER_CTX_ctrl(&cipher_ctx, EVP_CTRL_CCM_SET_IVLEN, uiNonceLen, NULL) != 1)
    {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_CIPHER_CTX_ctrl(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    if(EVP_CIPHER_CTX_ctrl(&cipher_ctx, EVP_CTRL_CCM_SET_TAG, uiTagLen, NULL) != 1)
    {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_CIPHER_CTX_ctrl(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    if (EVP_EncryptInit(&cipher_ctx, NULL, pCipherKey, pIVNonce) != 1)
    {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_EncryptInit(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
  }
  if( EVP_EncryptUpdate(&cipher_ctx, pCipherText, &uiCipherLen, pBuf, uiBufLen) != 1)
  {
    if(asm_log_level >= LOG_CRITICAL){
      AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_EncryptUpdate(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if (cipherAlgo == CIPHER_ALGO_AES128_ECB)
  {
#ifdef NAZEER
    if(!EVP_EncryptFinal_ex(&cipher_ctx, pCipherText + uiCipherLen, &uiTmpLen))
    {
      if(asm_log_level >= LOG_CRITICAL){
	  AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_EncryptFinal_ex(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"\nuiCipherLen = %d, uiTmpLen = %d\n", uiCipherLen, uiTmpLen);
    }
    uiCipherLen += uiTmpLen;
#endif
  }

  *uiCipherTextLen = uiCipherLen;

  if (cipherAlgo == CIPHER_ALGO_AES128_CCM)
  {
    EVP_CIPHER_CTX_ctrl(&cipher_ctx, EVP_CTRL_CCM_GET_TAG, uiTagLen, &pCipherText[*uiCipherTextLen]);
 
    *uiCipherTextLen += uiTagLen;
  }
 // AWSec_Print("pCipherTextLen = %ld\n", *uiCipherTextLen);
  EVP_CIPHER_CTX_cleanup(&cipher_ctx);
  return SUCCESS;

  err:
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%s: %d: Returned error  library = %s, func = %s, reason = %s, {error = %s}\n",__FILE__,__FUNCTION__,__LINE__,ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
    }
    return FAILURE;
}

/***********************************************************************
 * Function Name 	: AWSecDecrypt
 * Description   	: Decrypts the cipherdata with given Algorithm.
 *				 
 * Input        	: None
 * Output        	: None
 * Return value  	: returns SUCCESS
 ***********************************************************************/

UINT32 AWSecDecrypt(UINT32 cipherAlgo, UINT8 * pCipherKey, UINT8 * pIVNonce, UINT32 uiNonceLen, UINT8 *pCipherText, UINT32 uiCipherTextLen, UINT8 *pBuf, UINT32 *uiBufLen, UINT32 uiTagLen)
{
  EVP_CIPHER_CTX cipher_ctx;
  INT32 uiDataLen, i; 
  char logstring[250];
#ifdef NAZEER
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"pCipherKey  = %d, pIVNonce = %d, pCipherText = %d, uiCipherTextLen = %d, pBuf = %d\n", strlen(pCipherKey), strlen(pIVNonce), strlen (pCipherText), uiCipherTextLen, sizeof(pBuf));
  }
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,pCipherText,uiCipherTextLen,"pCipherText start\n");
      AWSEC_LOG(ASM,NULL,0,"End of pCipherText \n");
  }
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,pIVNonce,15,"pIVNonce start\n");
      AWSEC_LOG(ASM,NULL,0,"End of pIVNonce start\n");
  }
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,pCipherKey,20,"pCipherKey start\n");
      AWSEC_LOG(ASM,NULL,0,"End of pCipherKey \n");
  }
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"pBuf size = %d\n", sizeof(pBuf));
  }
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"%s: %d: %s: Entry\n", __FUNCTION__, __LINE__, __FILE__);
  }
#endif
  if((!pCipherKey) || (!pBuf) || (!pCipherText))
  {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"NULL pointers sent to the AWSecDecrypt function\n");
    goto err;
  }
  EVP_CIPHER_CTX_init(&cipher_ctx);
  switch(cipherAlgo)
  {
    case CIPHER_ALGO_AES128_CCM:
    {
      if(EVP_DecryptInit(&cipher_ctx, EVP_aes_128_ccm( ), NULL, NULL) != 1)
      {
        if(asm_log_level >= LOG_CRITICAL){
            AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_DecryptInit(): %s\n",__FILE__,__LINE__, strerror(errno));
        }
        goto err;
      }
      break;
    }
    case CIPHER_ALGO_AES128_ECB:
    {
      uiTagLen = 0;
      if(EVP_DecryptInit(&cipher_ctx, EVP_aes_128_ecb(  ), NULL, NULL) != 1)
      {
        if(asm_log_level >= LOG_CRITICAL){
            AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_DecryptInit(): %s\n",__FILE__,__LINE__, strerror(errno));
        }
        goto err;
      }
      break;
    }
    default:
    {
      if(asm_log_level >= LOG_CRITICAL)
          AWSEC_LOG(ASM,NULL,0,"Unknown Algorithm\n");        
      *uiBufLen = 0;
      return FAILURE;
    }
  }

  uiDataLen = uiCipherTextLen - uiTagLen;
  if(cipherAlgo == CIPHER_ALGO_AES128_CCM)
  {
    /** Enable padding as per standard **/
    if (EVP_CIPHER_CTX_set_padding(&cipher_ctx, 1) != 1)
    {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_CIPHER_CTX_set_padding(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
    EVP_CIPHER_CTX_ctrl(&cipher_ctx, EVP_CTRL_CCM_SET_IVLEN, uiNonceLen, 0);
    if(EVP_CIPHER_CTX_ctrl(&cipher_ctx, EVP_CTRL_CCM_SET_TAG, uiTagLen, pCipherText + (uiCipherTextLen - uiTagLen)) != 1)
    {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_CIPHER_CTX_ctrl(): %s\n",__FILE__,__LINE__, strerror(errno));
      }
      goto err;
    }
  }
  if (EVP_DecryptInit(&cipher_ctx, NULL, pCipherKey, pIVNonce) != 1)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_DecryptInit(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  
  if(EVP_DecryptUpdate(&cipher_ctx, pBuf, &uiDataLen, pCipherText, (uiCipherTextLen - uiTagLen)) != 1)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EVP_DecryptUpdate(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }

  *uiBufLen = uiDataLen;
  EVP_CIPHER_CTX_cleanup(&cipher_ctx);
  return SUCCESS;
  err:
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%s: %d: Returned error  library = %s, func = %s, reason = %s, {error = %s}\n",__FILE__,__FUNCTION__,__LINE__,ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
    }
    return FAILURE;
}

/***********************************************************************
 * Function Name 	: AWSecHash
 * Description   	: Calculate the Hash of the given Buffer.
 *				 
 * Input        	: None
 * Output        	: None
 * Return value  	: returns SUCCESS
 ***********************************************************************/

UINT8 * AWSecHash(UINT32 hashAlgo, UINT8 * pBuf, UINT32 uiBufLen, UINT8 *pDigest, UINT32 *uiDigestLen, UINT32 uiReqLen)
{
  EVP_MD_CTX md_ctx;
  EVP_MD_CTX_init(&md_ctx);
  UINT8 pTmpBuf[32];
  INT32 uiTmpBufLen = 0;

  switch(hashAlgo)
  {
    case HASH_ALGO_SHA224:
    {
      //EVP_DigestInit(&md_ctx, EVP_sha224());
      break;
    }
    case HASH_ALGO_SHA256:
    {
      EVP_DigestInit(&md_ctx, EVP_sha256());
      break;
    }
    case HASH_ALGO_ECDSA:
    {
      EVP_DigestInit(&md_ctx, EVP_ecdsa());
      break;
    }
    default:
    {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"Unknown Algorithm to compute Hash\n");
      }
      pDigest = NULL;
      break;
    }
  }
  EVP_DigestUpdate(&md_ctx, (const void*)pBuf, uiBufLen);

  if (uiReqLen > 0)
  {
    EVP_DigestFinal(&md_ctx, pTmpBuf, &uiTmpBufLen);
    memcpy(pDigest, &pTmpBuf[uiTmpBufLen - uiReqLen], uiReqLen);
    *uiDigestLen = uiReqLen;
  }
  else
  {
    EVP_DigestFinal(&md_ctx, pDigest, (INT32 *)uiDigestLen);
  }
//  AWSec_Print("uiDigestLen = %ld\n", *uiDigestLen);
  EVP_MD_CTX_cleanup(&md_ctx);
  return pDigest;
}

/***********************************************************************
 * Function Name 	: AWSecSignData
 * Description   	: Sign the data with the given Elliptic Curve info.
 *				 
 * Input        	: None
 * Output        	: None
 * Return value  	: returns SUCCESS
 ***********************************************************************/

ECDSA_SIG * AWSecSignData(UINT8 *pDigest, UINT32 uiDigestLen, EC_KEY *ECKey, UINT8 *pSignData, UINT32 *uiSignDataLen)
{
  ECDSA_SIG * pSig = NULL;   
  pSig = ECDSA_do_sign(pDigest, uiDigestLen, ECKey);
  char logstring[250];
  if (pSig == NULL)
  {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"Unable to Sign message: %s: %d: ECDSA_do_sign(): Retruned error:%s --->  library = %s, func = %s, reason = %s, {error = %s}\n",__FILE__,__LINE__, strerror(errno),ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
      }
    return NULL;
  }
  /** Following line will allocate two OpenSSL blocks and must be freed by the calling function. **/
  //AWSec_Print("(pSig->r, pSig->s): (%s,%s)\n", BN_bn2hex(pSig->r), BN_bn2hex(pSig->s));

#ifdef NAZEER
  if (ECDSA_do_verify(pDigest, uiDigestLen, pSig, ECKey) != 1)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"Unable to verify: %s: %d: ECDSA_do_verify(): Retruned error:%s --->  library = %s, func = %s, reason = %s, {error = %s}\n",__FILE__,__LINE__, strerror(errno),ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
    }
    return pSig;
  }
  else
  {
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"Signature verified successfully\n");
    }
  }
#endif

  *uiSignDataLen = i2d_ECDSA_SIG(pSig, &pSignData);
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"i2d_ECDSA_SIG returned %p, length %ld\n", pSignData, *uiSignDataLen);
  }
  return pSig;
}

ECDSA_SIG * AWSecDigestAndSignData(UINT8 *pData, UINT32 uiDataLen, UINT8 * PrivKey)
{
  ECDSA_SIG * pSig = NULL;   
  EC_KEY *ECKey = NULL;
  UINT8 pDigest[SHA256_DIGEST_LENGTH];
  UINT32 uiDigestLen;
  char logstring[250];
  AWSecHash(HASH_ALGO_SHA256, pData, uiDataLen, pDigest, &uiDigestLen, 0);
  if (!(ECKey = ECKeyFromPrivKey(ECDSA_NISTP_256, ECKey, PrivKey, 32)))
  {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"Invalid private key provided\n");
    return NULL;
  }
  pSig = ECDSA_do_sign(pDigest, uiDigestLen, ECKey);
  if (pSig == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"Unable to Sign message: %s: %d: ECDSA_do_sign(): Retruned error:%s --->  library = %s, func = %s, reason = %s, {error = %s}\n",__FILE__,__LINE__, strerror(errno),ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
    }
    EC_KEY_free(ECKey);
    return NULL;
  }
  else
  {
    EC_KEY_free(ECKey);
  }
  /** Following line will allocate two OpenSSL blocks and must be freed by the calling function. **/
  //AWSec_Print("(pSig->r, pSig->s): (%s,%s)\n", BN_bn2hex(pSig->r), BN_bn2hex(pSig->s));

  return pSig;
}

UINT32 AWSecDigestAndVerifyData(UINT8 *pData, UINT32 uiDataLen, UINT8 * pSignature, UINT8 * PubKey)
{
  ECDSA_SIG pSig;   
  EC_KEY *ECKey = NULL;
  BIGNUM *bn1, *bn2;
  UINT8 pDigest[SHA256_DIGEST_LENGTH];
  UINT32 uiDigestLen;
  char logstring[250];
  AWSecHash(HASH_ALGO_SHA256, pData, uiDataLen, pDigest, &uiDigestLen, 0);
  if (!(ECKey = ECKeyFromPubKey(ECDSA_NISTP_256, NULL, PubKey, 33)))
  {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"Invalid public key provided\n");
    return FAILURE;
  }
  bn1 = BN_bin2bn((pSignature + 1), 32, BN_new());
  bn2 = BN_bin2bn((pSignature + 1 + 32), 32, BN_new());
  pSig.r = bn1;
  pSig.s = bn2;
#ifdef NOCODE
  printf("\n");
BN_print_fp(stdout, bn1);
  printf("\n");
BN_print_fp(stdout, bn2);
  printf("\n");
  if(asm_log_level >= LOG_INF){
      AWSEC_LOG(ASM,NULL,0,"Copy Signature from the byte string pSignature[0] = %x\n", pSignature[0]);
  }
#endif
  if (pSignature[0] != 0x00)
  {
    if (Fast_ecdsa_do_verify(pDigest, uiDigestLen, &pSig, ECKey, pSignature[0]) != SUCCESS)	
    {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"Unable to fast verify signature. library = %s, func = %s, reason = %s, {error = %s}\n", ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(), NULL));
    }
      EC_KEY_free(ECKey);
      BN_free(bn1);
      BN_free(bn2);
      return FAILURE;
    }
    else
    {
      if(asm_log_level >= LOG_INF){
          AWSEC_LOG(ASM,NULL,0,"Signature verified successfully\n");
      }
      EC_KEY_free(ECKey);
      BN_free(bn1);
      BN_free(bn2);
      return SUCCESS;
    }
  
  }
  else
  {
    if (ECDSA_do_verify(pDigest, uiDigestLen, &pSig, ECKey) != 1)
    {
        if(asm_log_level >= LOG_CRITICAL){
            AWSEC_LOG(ASM,NULL,0,"Unable to verify: %s: %d: ECDSA_do_verify(): Retruned error:%s --->  library = %s, func = %s, reason = %s, {error = %s}\n",__FILE__,__LINE__, strerror(errno),ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
        }
      EC_KEY_free(ECKey);
      BN_free(bn1);
      BN_free(bn2);
      return FAILURE;
    }
    else
    {
      if(asm_log_level >= LOG_INF){
          AWSEC_LOG(ASM,NULL,0,"Signature verified successfully\n");
      }
      EC_KEY_free(ECKey);
      BN_free(bn1);
      BN_free(bn2);
      return SUCCESS;
    }
  }
}

/***********************************************************************
 * Function Name 	: AWSecVerifySign
 * Description   	: Sign the data with the given Elliptic Curve info.
 *				 
 * Input        	: None
 * Output        	: None
 * Return value  	: returns SUCCESS
 ***********************************************************************/

UINT32 GetSignedData(UINT8 *pPrivKey, UINT8 *pData, UINT32 uiDataLen, UINT8 *pDigest, UINT32 uiDigestLen, UINT8 *pSignDataBuf)
{
  EC_GROUP *pGroup = NULL;
  EC_KEY *ECKey = NULL;
  ECDSA_SIG *pSig = NULL;
  BIGNUM *bn = NULL;
  UINT32 iRet = FAILURE, uiKeyLen;  
  UINT8 pTmpBuf[66];
  char logstring[250];
  if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  ECKey = EC_KEY_new();
  EC_KEY_set_group(ECKey, pGroup);
  EC_KEY_set_conv_form(ECKey, POINT_CONVERSION_COMPRESSED);


  if (AWSecHash(HASH_ALGO_SHA256, pData, uiDataLen, pDigest, &uiDigestLen, 0) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL)
	AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecHash(): %s\n",__FILE__,__LINE__, strerror(errno));
    
    goto err;
  }
  bn = BN_bin2bn(pPrivKey,32,BN_new());
  if (EC_KEY_set_private_key(ECKey, bn) != 1)
  {
    if (bn)
      BN_free(bn);
    if(asm_log_level >= LOG_CRITICAL){
	AWSEC_LOG(ASM,NULL,0,"EC_KEY_set_private_key failed--- %s:%d:EC_KEY_set_private_key(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if(bn)
    BN_free(bn);

  if((pSig = ECDSA_do_sign(pDigest, uiDigestLen, ECKey)) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"Unable to Sign message: %s: %d: ECDSA_do_sign(): Retruned error:%s --->  library = %s, func = %s, reason = %s, {error = %s}\n",__FILE__,__LINE__, strerror(errno),ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
    }
    goto err;
  }
  else
  {
    uiKeyLen = HexStr2ByteStr(pSig->r, pSignDataBuf, 32);
    uiKeyLen = HexStr2ByteStr(pSig->s, &pSignDataBuf[32], 32);
  }
  iRet = SUCCESS;
  err:
  if(iRet != SUCCESS)
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"Unable to verify signature--> %s: %d:  library = %s, func = %s, reason = %s, {error = %s}\n",__FILE__,__LINE__,ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
      }
  if(pGroup)
    EC_GROUP_clear_free(pGroup);
  if(ECKey)
    EC_KEY_free(ECKey);
  if(pSig)
    ECDSA_SIG_free(pSig);
  return iRet;

}

UINT32 VerifySignedData(UINT8 *pPubKey, UINT8 *pSignData, UINT8 *pDigest, UINT32 uiDigestLen)
{
  EC_GROUP *pGroup = NULL;
  EC_POINT * pPoint = NULL;
  EC_KEY *ECKey = NULL;
  ECDSA_SIG pSig;
  UINT32 iRet = FAILURE, uiKeyLen = 33;  
  UINT8 pTmpBuf[66];
  char logstring[250];
  if((pGroup = CreateECCompGroup(ECDSA_NISTP_256)) == NULL)
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:CreateECCompGroup(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  ECKey = EC_KEY_new();
  EC_KEY_set_group(ECKey, pGroup);
  EC_KEY_set_conv_form(ECKey, POINT_CONVERSION_COMPRESSED);
  if(!(pPoint = EC_POINT_new(pGroup)))
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_new(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if (!(EC_POINT_oct2point(pGroup, pPoint, pPubKey, uiKeyLen, NULL)))
  {
    if(asm_log_level >= LOG_CRITICAL){
	AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_POINT_oct2point(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  if (EC_KEY_set_public_key(ECKey, pPoint) != 1)
  {
    if(asm_log_level >= LOG_CRITICAL){
	AWSEC_LOG(ASM,NULL,0,"%s:%d:EC_KEY_set_public_key(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  bzero(pTmpBuf, sizeof(pTmpBuf));
  ByteStr2HexStr(pSignData, pTmpBuf, 32);
  BN_hex2bn(&pSig.r, pTmpBuf);
  bzero(pTmpBuf, sizeof(pTmpBuf));
  ByteStr2HexStr((pSignData + 32), pTmpBuf, 32);
  BN_hex2bn(&pSig.s, pTmpBuf);
  if (ECDSA_do_verify(pDigest, uiDigestLen, &pSig, ECKey) != 1)
  {
    if(asm_log_level >= LOG_CRITICAL){
	AWSEC_LOG(ASM,NULL,0,"%s:%d:ECDSA_do_verify(): %s\n",__FILE__,__LINE__, strerror(errno));
    }
    goto err;
  }
  iRet = SUCCESS;
  err:
  if(iRet != SUCCESS)
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"Unable to verify signature--> %s: %d:  library = %s, func = %s, reason = %s, {error = %s}\n",__FILE__,__LINE__,ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
      }
  if(pGroup)
    EC_GROUP_clear_free(pGroup);
  if(ECKey)
    EC_KEY_free(ECKey);
  if(pSig.s)
      BN_free(pSig.s);
  if(pSig.r)
      BN_free(pSig.r);
  return iRet;
}  

UINT32 AWSecVerifySign(UINT8 *pDigest, UINT32 uiDigestLen, UINT8 *pSignData, UINT32 uiSignDataLen, EC_KEY *ECKey)
{
  char logstring[250];
  if (ECDSA_verify(0, pDigest, uiDigestLen, pSignData, uiSignDataLen, ECKey) != 1)
  {
      if(asm_log_level >= LOG_CRITICAL){
          AWSEC_LOG(ASM,NULL,0,"Unable to verify signature--> %s: %d:  library = %s, func = %s, reason = %s, {error = %s}\n",__FILE__,__LINE__,ERR_lib_error_string(ERR_get_error()), ERR_func_error_string(ERR_get_error()), ERR_reason_error_string(ERR_get_error()), ERR_error_string(ERR_get_error(),NULL));
      }
    return FAILURE;
  }
  else 
  {
    if(asm_log_level >= LOG_INF){
        AWSEC_LOG(ASM,NULL,0,"Signature verified successfully\n");
    }
  }
  return SUCCESS;
}

UINT32 EC_KEY_Reconstruct_Priv_Key(EC_KEY *ECKey, BIGNUM *PrivKey)
{
    UINT32 iRet = 0;
    BN_CTX *ctx = NULL;
    EC_POINT *pPubKey = NULL;

    if (!ECKey) return 0;

    const EC_GROUP *pGroup = EC_KEY_get0_group(ECKey);

    if ((ctx = BN_CTX_new()) == NULL)
        goto err;

    pPubKey = EC_POINT_new(pGroup);

    if (pPubKey == NULL)
        goto err;

    if (!EC_POINT_mul(pGroup, pPubKey, PrivKey, NULL, NULL, ctx))
        goto err;

    EC_KEY_set_private_key(ECKey, PrivKey);
    EC_KEY_set_public_key(ECKey, pPubKey);

    iRet = 1;

err:

    if (pPubKey)
        EC_POINT_clear_free(pPubKey);
    if (ctx != NULL)
        BN_CTX_free(ctx);

    return iRet;
}

/*********************************************************
 * Read an external file which is including the private key, *
 * certificate or signedCRL data.                        *
 *********************************************************/

int
read_file(
        const char* file,
        UINT8* buff,
        int buffsize)
{
    FILE* fd = 0;
    int size = 0;
    char str[4];
    char* ret = 0;
    unsigned int value = 0;
    char logstring[250];
    fd = fopen(file, "rb");
    if (NULL == fd) {
	if(asm_log_level >= LOG_CRITICAL){
            AWSEC_LOG(ASM,NULL,0,"fopen() failed. file=[%s]", file);
	}
        return 0;
    }

    while(size < buffsize) {
        ret = fgets(str, 4, fd);
        if (feof(fd)) {
            break;
        }
        if (NULL == ret) {
	    if(asm_log_level >= LOG_CRITICAL){
                AWSEC_LOG(ASM,NULL,0,"File format error. index=[%d]", size);
	    }
            exit(1);
        }

        value = 0;
        sscanf(str, "%x ", &value);
        buff[size] = (UINT8)value;
        size++;
    }
    fclose(fd);
    return size;
}

UINT8 *BUFPUT16(UINT8 *cp, UINT16 val)
{
  *cp++ = ( val >> 8 ) & 0xFF;
  *cp++ = val & 0xFF;
  return(cp); 
}

UINT8 *BUFPUT32(UINT8 *cp, UINT32 val)
{
  *cp++ = ( val >> 24 ) & 0xFF;
  *cp++ = ( val >> 16 ) & 0xFF;
  *cp++ = ( val >> 8 ) & 0xFF;
  *cp++ = ( val  ) & 0xFF;

  return(cp); 
}
UINT16 BUFGET16(register UINT8 *cp)
{
  UINT16 pp;
  UINT8 *ctmp = (UINT8*) cp;

  pp = 0;
  pp = ((*ctmp << 8 ) & 0xFF00) | 
        (*(ctmp+1)&0xFF);

  return(pp);
}

UINT32 BUFGET32(register UINT8 *cp)
{
  UINT32 pp;
  UINT8 *ctmp = (UINT8*) cp; 

  pp = 0;

  pp = ((*ctmp << 24 ) & 0xFF000000 ) | 
        ((*(ctmp+1) << 16 ) & 0x00FF0000 ) | 
        ((*(ctmp+2) << 8 ) & 0x0000FF00 ) | 
         (*(ctmp+3)&0xFF);

  return(pp);
}

uint32_t getPsidbyLen(uint8_t *addr,int *retIdx, uint8_t *buff)
{
   uint32_t recv_size = 0;

   if ((addr[0] & 0x80) == 0x00) {
       recv_size = addr[0];
       if(buff != NULL)
           buff[0] = addr[0];
       *retIdx = 1;
   }
   else if ((addr[0] & 0xc0) == 0x80) {
       recv_size = (addr[0] << 8)|(addr[1]);
       if(buff != NULL){
           buff[0] = addr[0];
           buff[1] = addr[1];
       }
       *retIdx = 2;
   }
   else if ((addr[0] & 0xe0) == 0xc0) {
       recv_size = (addr[0] << 16)|(addr[1] << 8)|(addr[2]);
       if(buff != NULL){
           buff[0] = addr[0];
           buff[1] = addr[1];
           buff[2] = addr[2];
       }
       *retIdx = 3;
   }
   else if ((addr[0] & 0xf0) == 0xe0) {
       recv_size = (addr[0] << 24)|(addr[1] << 16)|(addr[2] << 8)|(addr[3]);
       buff[0] = addr[0];
       buff[1] = addr[1];
       buff[2] = addr[2];
       buff[3] = addr[3];
       *retIdx = 4;
   }
   return recv_size;
}

UINT32 HexStr2ByteStr(UINT8 *str, UINT8 *buf, UINT32 uiBufLen)
{
#if 1
  int i = 0, j = 0;
  char pTmpBuf[uiBufLen];
  char x, y;

  for(i = 0; i < uiBufLen; i++)
  {
    x = str[2 *i];
    y = str[2 *i + 1];
    //x = (x) <= '9' ? (x) - '0' : (x) - 'a' + 10;
    //y = (y) <= '9' ? (y) - '0' : (y) - 'a' + 10;
    x = (x & 0x1f) + ((x >> 6) * 0x19) - 0x10;
    y = (y & 0x1f) + ((y >> 6) * 0x19) - 0x10;

    buf[i] = (x << 4) | (y);
  }
  return i;
#else
  int i = 0, j = 0;
  UINT8 pTmpBuf[uiBufLen];

  for(i = 0 ; i < uiBufLen; i++)
  {
    sscanf(str + 2*i,"%02x",&pTmpBuf[i]);
  }
  memcpy(buf, pTmpBuf, uiBufLen);
  return i;
#endif
}

UINT32 ByteStr2HexStr(UINT8 *str, UINT8 *buf, UINT32 inBufLen)
{
  int i = 0, j = 0;
  UINT8 pTmpBuf[inBufLen * 2];

  for (i = 0; i < inBufLen; i++)
  {
    sprintf(&pTmpBuf[i*2],"%02x", str[i]);
  }
  memcpy(buf, pTmpBuf, 2*inBufLen);
#ifdef DEBUG
if(asm_log_level >= LOG_DEBUG){
    AWSEC_LOG(ASM,buf,(i*2),"ByteStr2HexStr\n");
}
#endif
  return i * 2;
}

UINT8* itoa(UINT32 i,  UINT8 b[]){
    char const digit[] = "0123456789";
    char* p = b;
    if(i<0){
        *p++ = '-';
        i = -1;
    }
    int shifter = i;
    do{ //Move to where representation ends
        ++p;
        shifter = shifter/10;
    }while(shifter);
    //*p = '\0';
    do{ //Move back, inserting digits as u go
        *--p = digit[i%10];
        i = i/10;
    }while(i);
    return b;
}

UINT32 AWSec_ecies_encrypt(UINT8 *PubKey, UINT8 *pData, UINT32 DataLen, UINT8 *pKeyData, UINT32 *keyLen, UINT8 *pCipher, UINT32 *CipherLen, UINT8 *pMac, UINT32 *macLen, UINT32 uiTagLen, UINT32 CypherAlgo, uint8_t *errorCode) 
{
  EC_KEY *user = NULL, *ephemeral = NULL;
  UINT8 envelope_key[2 * SHA256_DIGEST_LENGTH], iv[EVP_MAX_IV_LENGTH], block[EVP_MAX_BLOCK_LENGTH];
  UINT32 block_length, key_length, i;
  char logstring[250];
  // Simple sanity check.
  if (!PubKey || !pKeyData || !pData || !pMac) 
  {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"AWSec_ecies_encrypt(): Invalid parameters passed in.\n");
    
    return FAILURE;
  }

  // Make sure we are generating enough key material for the symmetric ciphers.
  if ((key_length = EVP_CIPHER_key_length(ECIES_CIPHER)) * 2 > SHA256_DIGEST_LENGTH) 
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"The key derivation method will not produce enough envelope key material for the chosen ciphers. {envelope = %i / required = %zu}", SHA256_DIGEST_LENGTH / 8, (key_length * 2) / 8);
    }
    return FAILURE;
  }
  if (!(user = ECKeyFromPubKey(ECDSA_NISTP_256, NULL, PubKey, 33)))
  {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"AWSec_ecies_encrypt():Invalid public key provided\n");
    *errorCode = CMD_ERR_INVALID_PRIVATE_KEY_PROVIDED;
    return FAILURE;
  }

  if (!(ephemeral = GenerateECCompKey(ECDSA_NISTP_256, ephemeral)))
  {
    EC_KEY_free(user);
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"Unable to generate ephemeral key.\n");
    return FAILURE;
  }
  // Use the intersection of the provided keys to generate the envelope data used by the ciphers below. The ecies_key_derivation() function uses
  // SHA 256 to ensure we have a sufficient amount of envelope key material and that the material created is sufficiently secure.
  if (ECDH_compute_key(envelope_key, key_length + SHA256_DIGEST_LENGTH, EC_KEY_get0_public_key(user), ephemeral, ECIES_KDF2_KEY_derivation) != SHA256_DIGEST_LENGTH + key_length) 
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"An error occurred while trying to compute the envelope key. {error = %s}\n", ERR_error_string(ERR_get_error(), NULL));
    }
    EC_KEY_free(ephemeral);
    EC_KEY_free(user);
    *errorCode = CMD_ERR_KEY_MATERIAL_GENERATION_FAILED;
    return FAILURE;
  }

  // Determine the envelope and block lengths so we can allocate a buffer for the result.
  else if ((block_length = EVP_CIPHER_block_size(ECIES_CIPHER)) == 0 || block_length > EVP_MAX_BLOCK_LENGTH || (*keyLen = EC_POINT_point2oct(EC_KEY_get0_group(ephemeral), EC_KEY_get0_public_key(ephemeral), POINT_CONVERSION_COMPRESSED, NULL, 0, NULL)) == 0) 
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"Invalid block or envelope length. {block = %zu / envelope = %zu}\n", block_length, *keyLen);
    }
    EC_KEY_free(ephemeral);
    EC_KEY_free(user);
    *errorCode = CMD_ERR_INVALID_DATA_FORMAT;
    return FAILURE;
  }
        // Store the public key portion of the ephemeral key.
  else if (EC_POINT_point2oct(EC_KEY_get0_group(ephemeral), EC_KEY_get0_public_key(ephemeral), POINT_CONVERSION_COMPRESSED, pKeyData, *keyLen, NULL) != *keyLen) 
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"An error occurred while trying to record the public portion of the envelope key. {error = %s}\n", ERR_error_string(ERR_get_error(), NULL));
    }
    EC_KEY_free(ephemeral);
    EC_KEY_free(user);
    *errorCode = CMD_ERR_INVALID_DATA_FORMAT;
    return FAILURE;
  }

  // The envelope key has been stored so we no longer need to keep the keys around.
  EC_KEY_free(ephemeral);
  EC_KEY_free(user);

  // For now we use an empty initialization vector.
  memset(iv, 0, EVP_MAX_IV_LENGTH);

  for(i = 0; i < DataLen; i++)
  {
    pCipher[i] = (UINT8)(pData[i] ^ envelope_key[i]);
  }
  *CipherLen = DataLen;
  // Generate an authenticated hash which can be used to validate the data during decryption.
  if(HMAC(EVP_sha256(), envelope_key + key_length, SHA256_DIGEST_LENGTH, pCipher, *CipherLen, pMac,(unsigned int *) macLen) == NULL)
  {
    *errorCode = CMD_ERR_INVALID_DATA_FORMAT;
    return FAILURE;
  }
  return SUCCESS;
}

UINT32 AWSec_ecies_decrypt(UINT8 *key, UINT8 *pKeyData, UINT32 keyLen, char *pCipher, UINT32 length, UINT8 *mac, UINT32 macLen, UINT32 uiTagLen, UINT32 CypherAlgo, UINT8 *pOutBuf, UINT32 *outlen, uint8_t *errorCode) 
{

  size_t key_length;
  UINT32 output_length, i;
  EC_KEY *user = NULL, *ephemeral = NULL;
  UINT32 mac_length = EVP_MAX_MD_SIZE;
  unsigned char envelope_key[SHA256_DIGEST_LENGTH * 2], iv[EVP_MAX_IV_LENGTH], md[EVP_MAX_MD_SIZE], *block, *output;
  char logstring[250];
  // Simple sanity check.
  if (!key || !pCipher || !mac) 
  {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"Invalid parameters passed in.\n");
    return FAILURE;
  }

  // Make sure we are generating enough key material for the symmetric ciphers.
  else if ((key_length = EVP_CIPHER_key_length(ECIES_CIPHER)) * 2 > SHA256_DIGEST_LENGTH) 
  {
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"The key derivation method will not produce enough envelope key material for the chosen ciphers. {envelope = %i / required = %zu}", SHA256_DIGEST_LENGTH / 8, (key_length * 2) / 8);
    }
    return FAILURE;
  }
  if (!(user = ECKeyFromPrivKey(ECDSA_NISTP_256, NULL, key, 32)))
  {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"Invalid private key generated\n");
    *errorCode = CMD_ERR_INVALID_PRIVATE_KEY_PROVIDED;
    return FAILURE;
  }
  if (!(ephemeral = ECKeyFromPubKey(ECDSA_NISTP_256, NULL, pKeyData, keyLen)))
  {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"Invalid ephemeral key generated\n");
    EC_KEY_free(user);
    return FAILURE;
  }

  // Use the intersection of the provided keys to generate the envelope data used by the ciphers below. The ecies_key_derivation() function uses
  // SHA 256 to ensure we have a sufficient amount of envelope key material and that the material created is sufficiently secure.
  else if (ECDH_compute_key(envelope_key, SHA256_DIGEST_LENGTH + key_length, EC_KEY_get0_public_key(ephemeral), user, ECIES_KDF2_KEY_derivation) != SHA256_DIGEST_LENGTH + key_length) 
  { 
    if(asm_log_level >= LOG_CRITICAL){
        AWSEC_LOG(ASM,NULL,0,"An error occurred while trying to compute the envelope key. {error = %s}\n", ERR_error_string(ERR_get_error(), NULL));
    }
    EC_KEY_free(ephemeral);
    EC_KEY_free(user);
    *errorCode = CMD_ERR_KEY_MATERIAL_GENERATION_FAILED;
    return FAILURE;
  }

  // The envelope key material has been extracted, so we no longer need the user and ephemeral keys.
  EC_KEY_free(ephemeral);
  EC_KEY_free(user);

  // Use the authenticated hash of the ciphered data to ensure it was not modified after being encrypted.
  if(HMAC(EVP_sha256(), envelope_key + key_length, SHA256_DIGEST_LENGTH, pCipher, length, md, (unsigned int *)&mac_length) == NULL)
  {
    *errorCode = CMD_ERR_INVALID_DATA_FORMAT;
    return FAILURE;
  }
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,md,mac_length,"MAC generated on data\n");
      AWSEC_LOG(ASM,NULL,0,"End of MAC generated on data\n");
  }
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,mac,macLen,"MAC received from client\n");
      AWSEC_LOG(ASM,NULL,0,"End of MAC received from client\n");
  }
  // We can use the generated hash to ensure the encrypted data was not altered after being encrypted.
  //if (mac_length != macLen || memcmp(md, mac, mac_length)) 
  if (memcmp(md, mac, macLen)) 
  {
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"The authentication code was invalid! The ciphered data has been corrupted!\n");
    *errorCode = CMD_ERR_COULD_NOT_DECRYPT_KEY;
    return FAILURE;
  }

  // For now we use an empty initialization vector. We also clear out the result buffer just to be on the safe side.
  memset(iv, 0, EVP_MAX_IV_LENGTH);
  for(i = 0; i < length; i++)
  {
    pOutBuf[i] = (UINT8)(pCipher[i] ^ envelope_key[i]);
  }
  *outlen = i;
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,(UINT8 *)pCipher,length,"Encrypted Data\n");
      AWSEC_LOG(ASM,NULL,0,"End of Encrypted Data\n");
  }
  if(asm_log_level >= LOG_DEBUG){
      AWSEC_LOG(ASM,pOutBuf,*outlen,"Decrypted Data\n");
      AWSEC_LOG(ASM,NULL,0,"End of Decrypted Data\n");
  }
	return SUCCESS;
}


int getCert_from_crt(Certificate *impCert, UINT8 *pPrivKey) //input should be array of 256
{
	char crtFile[256] = "";
	char *str = NULL;
	int i;
	Certificate Cert;
	uint8_t *pCertData = NULL;
        uint8_t pCert[256];
	uint16_t idx=0,len=0;
	uint32_t offset=0;
  	char logstring[250];
        UINT32 numEntries, certLen, numCerts = 0, uiDigestLen;
        UINT8 pSymKey[] = {0x22, 0x54, 0xFF, 0x12, 0x8A, 0x9a, 0x52, 0x01, 0xA7, 0xAA, 0x3F, 0x0F, 0xDE, 0xEB, 0x34, 0x91};        
        //UINT8 pSymKey[] = {0x78, 0x5b, 0xd3, 0x85, 0x71, 0x1a, 0x9b, 0xe6, 0x05, 0x95, 0xeb, 0x5e, 0xf5, 0x85, 0x1d, 0x10};
	Time32 current_time = get_cur_time2004();
        Time32 st = current_time + REFERENCE_TIME;
       
	idx = ((Time32)((current_time % DAY_SEC)+34)/300)%288;
	offset = (idx*256) + 9;
	setenv("TZ","UTC",1);
        str = (char *)ctime(&st);
        epoch_to_time(str,crtFile,"/tmp/usb/lcm/Certificates/");
        if(asm_log_level >= LOG_CRITICAL){
            AWSEC_LOG(ASM,NULL,0,"pvk++++++++++++++++++++++Current .crt file:%s--CT:%x---idx:%d----offset:%d\n",crtFile,current_time,idx,offset);
        }
        if(mapFile(crtFile,&pCertData,0,0) == FAILURE)//added error check,spd
	    return FAILURE;

        if (pCertData[0] != 0x02)
            if(asm_log_level >= LOG_CRITICAL)
                AWSEC_LOG(ASM,NULL,0,"Invalid file format\n");
        numEntries = BUFGET32(&pCertData[1]); 
        if(asm_log_level >= LOG_INF){
            AWSEC_LOG(ASM,NULL,0,"numEntries = %x\n", numEntries);
        }

	if(pCertData[offset + 12] == 0)
       {
         if(asm_log_level >= LOG_CRITICAL)
             AWSEC_LOG(ASM,NULL,0,"NAZEER: This is not a valid cert\n");
         return -1;
	}
	if(AWSecDecrypt(CIPHER_ALGO_AES128_CCM, pSymKey, &pCertData[offset], 12, &pCertData[offset + 13], pCertData[offset + 12], pCert, &certLen, 16) != SUCCESS)
        {
	     if(asm_log_level >= LOG_CRITICAL){
                 AWSEC_LOG(ASM,NULL,0,"Error in calculating SHA256 hash of the message\n");
		 AWSEC_LOG(ASM,NULL,0,"%s:%d:AWSecDecrypt(): %s\n",__FILE__,__LINE__, strerror(errno));
	     }
             if(mapFile(crtFile,&pCertData,1,0)<0)
		if(asm_log_level >= LOG_CRITICAL){
		    AWSEC_LOG(ASM,NULL,0,"Unmap of file %s failed \n",crtFile);
		}
             return FAILURE;
        }
        else
        {
             memcpy(pPrivKey, pCert, 32);
	     impCert->certLen = certParse(&pCert[64],impCert);
	     memcpy(impCert->certInfo, &pCert[64], impCert->certLen);
	}
        if(mapFile(crtFile,&pCertData,1,0)<0)
	    if(asm_log_level >= LOG_CRITICAL){
	        AWSEC_LOG(ASM,NULL,0,"Unmap of file %s failed \n",crtFile);
	    }
	return certLen;
}

int readFileAvailable(char *filename,uint8_t **buff){
    struct flock fl = {F_WRLCK, SEEK_SET,   0,      0,     0 };
    int fd,readLen=0;
    struct stat sbuf;
    char logstring[250];
    fl.l_pid = getpid();
    if ((fd = open(filename, O_RDWR )) == -1) {
        if(asm_log_level >= LOG_CRITICAL)
            AWSEC_LOG(ASM,NULL,0,"error:open\n");
        return -1;
    }
    if (fcntl(fd, F_SETLKW, &fl) == -1) {
        if(asm_log_level >= LOG_CRITICAL)
            AWSEC_LOG(ASM,NULL,0,"error:fcntl\n");
        close(fd);
        return -1;
    }
//once acquired lock read it
    if (stat(filename, &sbuf) == -1) {
        if(asm_log_level >= LOG_CRITICAL)
            AWSEC_LOG(ASM,NULL,0,"error:stat\n");
        close(fd);
	return -1;
    }
    *buff = (uint8_t *) calloc(1,sbuf.st_size);
    readLen = read(fd,*buff,sbuf.st_size);
    if(readLen < 0){
        if(asm_log_level >= LOG_CRITICAL)
            AWSEC_LOG(ASM,NULL,0,"error:file read\n");
        close(fd);
        return -1;
    }
    if(asm_log_level >= LOG_DEBUG){
        AWSEC_LOG(ASM,NULL,0,"**readlength:%d**size:%d\n",readLen,sbuf.st_size);
    }
    fl.l_type = F_UNLCK;  /* set to unlock same region */
    if (fcntl(fd, F_SETLK, &fl) == -1) {
        if(asm_log_level >= LOG_CRITICAL)
            AWSEC_LOG(ASM,NULL,0,"fcntl\n");
        close(fd);
        return -1;
    }
    if(asm_log_level >= LOG_CRITICAL)
        AWSEC_LOG(ASM,NULL,0,"Unlocked. wr\n");
    close(fd);
    return readLen;
}

/***************************************************************
 * distance_cal() --> computes distance between two points 
 * with (latitude,longitude) known of two points.
 * Here, computation is done without elevation as per 16092 req.
*****************************************************************/

void distance_cal(location *distFrmPos) {
        double ang1,ang2,radpnt1,radpnt2,ethlat1,ethlon1,ethlat2,ethlon2,x,y;
	double lat1,lat2,lon1,lon2;
	lat1 = (double) distFrmPos->local_latitude /10000000;
	lon1 = (double) distFrmPos->local_longitude /10000000;
	lat2 = (double) distFrmPos->generation.latitude /10000000;
	lon2 = (double) distFrmPos->generation.longitude /10000000;
	//printf("lat1:%lf lon1:%lf lat2:%lf lon2:%lf\n",lat1,lon1,lat2,lon2);
        ang1 = (atan(R2divR1*tan(lat1 * pidiv180)));
        ang2 = (atan(R2divR1*tan(lat2 * pidiv180)));
        radpnt1 = (sqrt(1 /(((cos(ang1)) * (cos(ang1))) /R1 + (((sin(ang1)) * (sin(ang1))) /R2)))) + 0;
        radpnt2 = (sqrt(1 /(((cos(ang2)) * (cos(ang2))) /R1 + (((sin(ang2)) * (sin(ang2))) /R2)))) + 0;
        ethlat1 = radpnt1 * cos(ang1);
        ethlat2 = radpnt2 * cos(ang2);
        ethlon1 = radpnt1 * sin(ang1);
        ethlon2 = radpnt2 * sin(ang2);
        x = sqrt((ethlat1 - ethlat2) * (ethlat1 - ethlat2) + (ethlon1 - ethlon2) * (ethlon1 - ethlon2));
        y = 2 * pi * ((((ethlat1 + ethlat2) /2)) /360) * (lon1 - lon2);
        distFrmPos->distance=sqrt(x*x+y*y);
	//printf("distance:%lf x:%lf y:%lf\n",distFrmPos->distance,x,y);
}
