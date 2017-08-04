#include <getopt.h>
#include <stdio.h>
#include <stdlib.h>
#include <errno.h>
#include <signal.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <syslog.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/file.h>
#include <sys/time.h>
#include <string.h>
#include <unistd.h>
#include <time.h>
#include "convert_pcap.h"


struct file_records *file1=NULL;
struct file_records *file2=NULL;
struct file_records *file3=NULL;
struct file_records *file4=NULL;
WSMData data;
FILE *fd_kml;
//Global variable Declarations
char *buf;
uint32_t flags = 3; //default captures tx and rx files
int set_number;
int set_direction=1; 
int set_interface=1,sock=0;
float set_size = 10*MB;
uint32_t set_time = 0; // in minutes
char logsyscmd[150],ipv4=1;
char model_dep_id[10]="1234";
char ap_name[5]="ffff",filename[50];
uint16_t port=0,kml=0,files=0;

//Functions forward declarations
void open_logfiles(void);
void close_logfiles(void);
void filestr_gen(void );
void dump_pcap_t(struct file_records *, struct pcap_pkthdr *, char *);
void catch_signal(int sgnl);
void SwapData(struct widi_loghdr *);
void write_to_file(void *,char *,void *);
int sf_write_header_t(int , int , int , int );
void sig_int();
void sig_term();
void sig_segv();

void catch_signal(int sgnl)
{
	 if(!kml)
	 { 
		close_logfiles();
		printf("\n\t@Total number of files Created in ModelDeploymentPktCapture--%u @\n",files);
	 }
	 if(file1 != NULL)
	     free(file1);
	 if(file2 != NULL)
	     free(file2);
	 if(file3 != NULL)
	     free(file3);
	 if(file4 != NULL)
	     free(file4);
        //close(fd);
	
	 system("nohup mv -f wlan_capture/* ModelDeploymentPktCaptures/  2> /dev/null");	
	 close(sock);

	if(buf)
		free(buf);
	if(kml)
		fclose(fd_kml);

	if(sgnl == 11)
	(void)syslog(LOG_INFO,"Oops.. Segfault!!!  covert_pcap got killed\n");
    else if (sgnl == 99)
	(void)syslog(LOG_INFO, "File handling Error. Closing Convert_pcap Application  \n");
	else
	(void)syslog(LOG_INFO, "Closing convert_pcap Application  \n");
        exit(0);

}

void sig_term()
{
    sig_int();
}
void sig_int(void)
{
    catch_signal(0); 
}
void sig_segv()
{
	catch_signal(11);	
}

void genhrlogfilename(char *hrlogfile, char *str_t, unsigned int seq_num)
{
	char *token = NULL;
    char *str = NULL;;
    int i;
    int month_num=0,date, hour, min,year;int file_psid = 0xff;
    char *month;
//	struct log_file_names *gen_file = (struct log_file_names *)file_names;
	char ctimestr[50];
	struct timeval tv;
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
    gettimeofday(&tv, NULL);
    str = ctime(&tv.tv_sec);
    strcpy(ctimestr, str);
    str = ctimestr;
    token = strtok(str," ");                //week_day

    token = strtok(NULL," ");               //month
    month=token;
    for(i=1; i <= 12; i++)
    {
 	   if(!strcmp(mon[i-1],month))
    	   month_num = i;
    }

    token = strtok(NULL," ");               //date
    date=atoi(token);

    token = strtok(NULL,":");               //hour
    hour=atoi(token);

    token = strtok(NULL,":");               //min
    min=atoi(token);

    token = strtok(NULL," ");               //sec

    token = strtok(NULL," ");               //year
    year = atoi(token);

	if(strcmp(str_t,"NULL") != 0)
	{
           sprintf(hrlogfile, "wlan_capture/%04d%02d%02d_%02d%02d_%x_%s_%s_%s_%04d.pcap", year, month_num, date, hour, min , file_psid, model_dep_id, ap_name,str_t,seq_num);
	}
	else
	    sprintf(hrlogfile, "wlan_capture/%04d%02d%02d_%02d%02d_%x_%s_%s_%04d.pcap", year, month_num, date, hour, min , file_psid, model_dep_id, ap_name, seq_num);

}

void usage() 
{
    printf("\nConvert_pcap  Application\n");
    printf("\nINFO: Convert_pcap application recieves data from Locomate board and it writes to pcap file(as per interface and direction)\n");
    printf(" \n******** Options ******\n");
    printf("\n\t -h:\tThis help \n");
    printf("\t -p:\tCaptrue app listen on this port\n");
    printf("\t -i:\tIndividual pcap files for each interface [1 - Enabled, 0 - Disabled]\n");
    printf("\t -d:\tIndividual pcap files for TX and RX [1 - Enabled, 0 - Disabled]\n");
    printf("\t -s:\tSize of pcap file [Units are in MB]\n");
    printf("\t -m:\tModel deployment device id\n");
    printf("\t -v:\tVersion of ip layer [4- ipv4, 6- ipv6 address]\n");
    printf("\t -k:\tTo Create kml file [Mention file name with .kml extension]\n");
    printf("\t    \tThis feature disables pcap logging on machine\n\n");
    printf("\t DEFAULT Values for options [ -s 10 ] [ -d 1 ] [ -i 1 ] [ -p 8888 ] [ -m 0xffff] [ -v 4 ] [ -k 0]\n\n");

    exit(0);
}



void Options(int argc, char *argv[])
{
    int index = 0;
    int t;
    
	static struct option opts[] =
    {
        {"help", no_argument, 0, 'h'},
        {"interface ", required_argument, 0, 'i'},
        {"Port", required_argument, 0, 'p'},
        {"direction", required_argument, 0, 'd'},
        {"pcap file size", required_argument, 0, 's'},
        {"model deployment device id", required_argument, 0, 'm'},
        {"kml file name", required_argument, 0, 'k'},
        {"ip address", required_argument, 0, 'v'},
        {0, 0, 0, 0}
    };
    while(1) {
        t = getopt_long(argc, argv, "hi:p:d:s:m:k:v:", opts, &index);
        if(t < 0) {
            break;
        }
        switch(t) {
        case 'h':
            usage();
            break;
        case 'p':
            port = atoi(optarg);
            break;

        case 'i':
            set_interface = atoi(optarg);
            break;

        case 'd':
            set_direction = atoi(optarg);
            break;

        case 's':
            set_size = atoi(optarg);
            set_size = set_size*MB;
			break;
        case 'm':
			strcpy(model_dep_id,optarg);
			break;
        case 'v':
			t=atoi(optarg);
			if(t==6)
				ipv4=0;
			break;
		case  'k':
			memset(filename,0,50);
			strcpy(filename,optarg);
			kml = 1;
			break;
        default:
            usage();
            break;
        }
    }
}

int main (int argc, char **argv)
{
	struct sockaddr_in addr4,from_addr4;
	struct sockaddr_in6 addr6,from_addr6;
	struct widi_loghdr *wHeader;
	struct pcap_pkthdr preparedHeader;
	socklen_t addrlen;
	int ret=0,size=1500,i=0;
	int position,tempid,first_data=0;
	uint16_t tmpfiles=0;
	char *ptr=NULL, bufferstart[1000],bufferlonlat[50], bufferend[1000];

	/*Code for Signal Handling */
	signal(SIGINT, (void*)sig_int);
	signal(SIGTERM, (void*)sig_term);
	signal(SIGSEGV, (void*)sig_segv);
	
	if(argc < 2)
		usage();
	Options(argc,argv);
	
	if(ipv4 ==1 )
	{	sock = socket(AF_INET,SOCK_DGRAM,0);		
		if(sock<0)
			return -1;
   	
		addr4.sin_family = AF_INET;
		addr4.sin_addr.s_addr = htons(INADDR_ANY);
		addr4.sin_port = htons(port);
	
		addrlen = sizeof(struct sockaddr_in);        

   		ret=bind(sock, (struct sockaddr *) &addr4, addrlen);
		if(ret<0)
		{	
			perror("bind");
			exit(-1);
		}
	} 
	else	
	{	
		sock = socket(AF_INET6,SOCK_DGRAM,0);		
		if(sock<0)
			return -1;
   	
		addr6.sin6_family = AF_INET6;
		//addr6.sin6_addr = htons(IN6ADDR_ANY_INIT);
		addr6.sin6_addr = in6addr_any;
		addr6.sin6_port = htons(port);
	
		addrlen = sizeof(struct sockaddr_in6);        

   		ret=bind(sock, (struct sockaddr *) &addr6, addrlen);
		if(ret<0)
		{	
			perror("bind");
			exit(-1);
		}
	} 

	if(kml == 0)    
	{
		system("mkdir -p ModelDeploymentPktCaptures");
    	system("mkdir -p wlan_capture");
	}
	else
	{
    	//open kml file with fd_kml
        fd_kml = fopen(filename,"w+");
        if(fd_kml == NULL)
		{
        	printf("\ncould not open kml write file\n");
            exit(0);
        }
		fill_buffer(bufferend,1,0);
    	//fputs(bufferend,fd_kml);
        fflush(fd_kml);
		printf("\n\tGps data writing to %s file\n",filename);
	}
	
	buf = malloc(1500);
	if(!buf)
	{
		printf("malloc failed\n");
		exit(-1);
	}	
	if(kml == 0)
	{
		filestr_gen();
		printf("\n\tLoging packets under \"ModelDepolymentCapture\" directory with pcap format.. \n");
	}
   
	while (1) 
    { 
		
		if(ipv4 == 1)
			ret = recvfrom(sock,buf,size,0,(struct sockaddr *) &from_addr4, &addrlen);
		else 
			ret = recvfrom(sock,buf,size,0,(struct sockaddr *) &from_addr6, &addrlen);
		if(ret<0)
		{	
			perror("recvfrom");
			continue;
		}
        ptr=buf;
		if(0==i && kml == 0)
			open_logfiles();        
		
			wHeader = (struct widi_loghdr *)ptr;
			SwapData(wHeader);	
		    
			ptr = ptr + sizeof(struct widi_loghdr);
			if (wHeader->size == 0) 
			{
				//printf("Total no pkts %u\n",i);
				if(kml == 0)
					close_logfiles();
				memset(buf,0,1500);
				i=0;
				if(tmpfiles != files)
				{
					printf("\t####Total number of files moved to ModelDeploymentPktCapture -- %u####\n",files);
					tmpfiles = files;
				}	
				continue;
			}

			if(!kml)
			{	
				preparedHeader.ts.tv_sec  = wHeader->time.tv_sec;//(wHeader->timestamp)/1000;
				preparedHeader.ts.tv_usec = wHeader->time.tv_usec;//((wHeader->timestamp)%1000)*1000;
				preparedHeader.caplen   = wHeader->size;
				preparedHeader.len  	= wHeader->size;
				write_to_file((void*)wHeader,ptr,(void*)&preparedHeader);
		    }   
			else
			{
				ptr = ptr +  PRISM_HEADER_LENGTH ;   // Added prism header length, ptr moved to 802.11 header
				if((uint8_t)ptr[0] == 0x88)	         // Here we identify whether WSMP or WSA packet
				{
			   		ptr = ptr + WLAN_HEADER_LENGTH + LLC_HEADER_LENGTH;  // Moving the pointer to 1609.3 header
			    	
					if(ptr[1] == 32)  // Processing only BSM packets, Filters using psid 32
					{	
						ptr = ptr + 12;   //Getting the data contents of wsmp packets 
			 			data.length = htons(*(uint16_t *)ptr);
						ptr = ptr + 2;
						memcpy(data.contents,ptr,data.length);
						printf("WSMP PACKET LENGTH %u\n",data.length);	
						if(kml == 1)
						{
							//extract lat-lon from recieved data
							tempid = extract_data(&data,bufferlonlat);
									
							if(first_data == 0)
							{
							    //create start buffer for kml
							    fill_buffer(bufferstart,0,tempid);
							    fputs(bufferstart,fd_kml);
							    position = ftell(fd_kml); //retain fd_kml position to write data where bufferstart ends
							    first_data++;
							}	

							//get fd_kml to position where next coordinates is to be added
							fseek(fd_kml,position,SEEK_SET);
							fputs(bufferlonlat,fd_kml);
		
							position = ftell(fd_kml); //retain fd_kml position to write next coordinates
							fputs(bufferend,fd_kml); // put bufferend data to complete kml tags.
        
							fflush(fd_kml);
							fclose(fd_kml);
							fd_kml = fopen(filename,"r+");
							if(fd_kml == NULL)
							{
				        	    printf("\ncould not open kml write file\n");
        	    				exit(0);
    						}		
						}
					}
				}
				else if((uint8_t)ptr[0] == 0xd0)	
				{
					//printf("WSA PACKET\n");	
				}
				else
				{
					//printf("INVALID PACKET\n");	
				}	
			}
			++i;
			memset(buf,0,1500);
	}
return 0;
}

void write_to_file(void *WHeader,char *ptr,void *pkt_header)
{
	struct widi_loghdr *wHeader_t = (struct widi_loghdr*)WHeader;
	struct pcap_pkthdr *preparedHeader_t = (struct pcap_pkthdr*)pkt_header;
	u_int8_t ic_index = wHeader_t->ic_index;
	u_int16_t log_type = wHeader_t->log_type;
	//unsigned int Pktsize = wHeader_t->size;

	if(set_number == TX_RX_DIRECTION_INTERFACE)
	{
	  if(log_type == 1)
	  {	
	      if(ic_index == 0)
	      {
	          if(file1->file_size == 0)
		  {
                      file1->seq_num++;
		      if(file1->seq_num == 10000)
		          file1->seq_num = 1;	
                      genhrlogfilename(file1->hrlogfilename, file1->logstring, file1->seq_num);
                      file1->fd = open(file1->hrlogfilename,O_WRONLY | O_CREAT | O_APPEND, S_IRWXU | S_IRWXO | S_IRWXG); 
		      if(file1->fd < 0)
		          catch_signal(99);		 
                      sf_write_header_t(file1->fd,LINKTYPE_PRISM_HEADER,0,SNAPLEN);  
		      file1->file_size = file1->file_size + sizeof(struct pcap_file_header);	
		  }
	          dump_pcap_t(file1,preparedHeader_t, ptr);
		  //file1->file_size = file1->file_size + Pktsize + sizeof(struct pcap_pkthdr);
		  if((file1->file_size >= (int)set_size))
		  {
			  //syslog(LOG_INFO,"file1->file_size_1 = %d \n",file1->file_size_1);
		          if(close(file1->fd) < 0)
			      		catch_signal(99);
			  sprintf(logsyscmd, "mv -f %s %s",file1->hrlogfilename,"ModelDeploymentPktCaptures/");
			  system(logsyscmd);
			  files++;

			  memset(file1->hrlogfilename,0,sizeof(file1->hrlogfilename));
			  file1->file_size = 0;		
		  }		
	      }
	      else if(ic_index == 1)
              {	
	          if(file3->file_size == 0)
		  {
                      file3->seq_num++;
		      if(file3->seq_num == 10000)
		          file3->seq_num = 1;	
                      genhrlogfilename(file3->hrlogfilename, file3->logstring, file3->seq_num);
                      file3->fd = open(file3->hrlogfilename,O_WRONLY | O_CREAT | O_APPEND, S_IRWXU | S_IRWXO | S_IRWXG);   
		      if(file3->fd < 0)
		          catch_signal(99);		 
                      sf_write_header_t(file3->fd,LINKTYPE_PRISM_HEADER,0,SNAPLEN);  
		      file3->file_size = file3->file_size + sizeof(struct pcap_file_header);	
		  }
	          dump_pcap_t(file3,preparedHeader_t, ptr);
		  //file3->file_size = file3->file_size + Pktsize+ sizeof(struct pcap_pkthdr);	
		  if((file3->file_size >= (int)set_size))
		  {
		      if(close(file3->fd) < 0)
		          catch_signal(99);	
		      sprintf(logsyscmd, "mv -f %s %s",file3->hrlogfilename,"ModelDeploymentPktCaptures/");
		      system(logsyscmd);
			  files++;
		      memset(file3->hrlogfilename,0,sizeof(file3->hrlogfilename));
		      file3->file_size = 0;		
		  }		
		      	
              }
	  }
	  else
	  {
	      if(ic_index == 0)
              {
	          if(file2->file_size == 0)
		  {
                      file2->seq_num++;
		      if(file2->seq_num == 10000)
		          file2->seq_num = 1;	
                      genhrlogfilename(file2->hrlogfilename, file2->logstring, file2->seq_num);
                      file2->fd = open(file2->hrlogfilename,O_WRONLY | O_CREAT | O_APPEND, S_IRWXU | S_IRWXO | S_IRWXG);   
		      if(file2->fd < 0)
		          catch_signal(99);		 
                      sf_write_header_t(file2->fd,LINKTYPE_PRISM_HEADER,0,SNAPLEN);  
		      file2->file_size = file2->file_size + sizeof(struct pcap_file_header);	
		  }
	          dump_pcap_t(file2,preparedHeader_t, ptr);
		  //file2->file_size = file2->file_size + Pktsize+ sizeof(struct pcap_pkthdr);	
		  if((file2->file_size >= (int)set_size))
		  {
		      if(close(file2->fd) < 0)
		          catch_signal(99);	
		      sprintf(logsyscmd, "mv -f %s %s",file2->hrlogfilename,"ModelDeploymentPktCaptures/");
		      system(logsyscmd);
			  files++;
		      memset(file2->hrlogfilename,0,sizeof(file2->hrlogfilename));
		      file2->file_size = 0;		
		  }		
              }
	      else if(ic_index == 1)	
              {
	          if(file4->file_size == 0)
		  {
                      file4->seq_num++;
		      if(file4->seq_num == 10000)
		          file4->seq_num = 1;	
                      genhrlogfilename(file4->hrlogfilename, file4->logstring, file4->seq_num);
                      file4->fd = open(file4->hrlogfilename,O_WRONLY | O_CREAT | O_APPEND, S_IRWXU | S_IRWXO | S_IRWXG);   
		      if(file4->fd < 0)
		          catch_signal(99);		 
                      sf_write_header_t(file4->fd,LINKTYPE_PRISM_HEADER,0,SNAPLEN);  
		      file4->file_size = file4->file_size + sizeof(struct pcap_file_header);	
		  }
	          dump_pcap_t(file4,preparedHeader_t, ptr);
		  //file4->file_size = file4->file_size + Pktsize+ sizeof(struct pcap_pkthdr);	
		  if((file4->file_size >= (int)set_size))
		  {
		      if(close(file4->fd) < 0)
		          catch_signal(99);	
		      sprintf(logsyscmd, "mv -f %s %s",file4->hrlogfilename,"ModelDeploymentPktCaptures/");
		      system(logsyscmd);
			  files++;
		      memset(file4->hrlogfilename,0,sizeof(file4->hrlogfilename));
		      file4->file_size = 0;		
                  } 
	      }
	  }
        }		        			    		
	else if((set_number == TX_RX_INTERFACE) ||(set_number == TX_INTERFACE) || (set_number == RX_INTERFACE) || (set_number == TX_RX_DIRECTION))
	{
	      if(ic_index == 0)
              {
	          if(file1->file_size == 0)
		  {
                      file1->seq_num++;
		      if(file1->seq_num == 10000)
		          file1->seq_num = 1;	
                      genhrlogfilename(file1->hrlogfilename, file1->logstring, file1->seq_num);
                      file1->fd = open(file1->hrlogfilename,O_WRONLY | O_CREAT | O_APPEND, S_IRWXU | S_IRWXO | S_IRWXG);   
		      if(file1->fd < 0)
		          catch_signal(99);		 
                      sf_write_header_t(file1->fd,LINKTYPE_PRISM_HEADER,0,SNAPLEN);  
		      file1->file_size = file1->file_size + sizeof(struct pcap_file_header);;	
		  }
	          dump_pcap_t(file1,preparedHeader_t, ptr);
		  //file1->file_size = file1->file_size + Pktsize + sizeof(struct pcap_pkthdr);	
		  if((file1->file_size >= (int)set_size))
		  {
		      if(close(file1->fd) < 0)
		          catch_signal(99);	
		      sprintf(logsyscmd, "mv -f %s %s",file1->hrlogfilename,"ModelDeploymentPktCaptures/");
		      system(logsyscmd);
			  files++;
		      memset(file1->hrlogfilename,0,sizeof(file1->hrlogfilename));
		      file1->file_size = 0;		
                  } 
              }
	      else if(ic_index == 1)
              {
	          if(file2->file_size == 0)
		  {
                      file2->seq_num++;
		      if(file2->seq_num == 10000)
		          file2->seq_num = 1;	
                      genhrlogfilename(file2->hrlogfilename, file2->logstring, file2->seq_num);
                      file2->fd = open(file2->hrlogfilename,O_WRONLY | O_CREAT | O_APPEND, S_IRWXU | S_IRWXO | S_IRWXG);   
		      if(file2->fd < 0)
		          catch_signal(99);		 
                      sf_write_header_t(file2->fd,LINKTYPE_PRISM_HEADER,0,SNAPLEN);  
		      file2->file_size = file2->file_size + sizeof(struct pcap_file_header);;	
		  }
	          dump_pcap_t(file2,preparedHeader_t, ptr);
		  //file2->file_size = file2->file_size + Pktsize + sizeof(struct pcap_pkthdr);	
		  if((file2->file_size >= (int)set_size))
		  {
		      if(close(file2->fd) < 0)
	                  catch_signal(99);
		      sprintf(logsyscmd, "mv -f %s %s",file2->hrlogfilename,"ModelDeploymentPktCaptures/");
		      system(logsyscmd);
			  files++;
		      memset(file2->hrlogfilename,0,sizeof(file2->hrlogfilename));
		      file2->file_size = 0;		
		  }		
              }
	}
	else if((set_number == TX_DIRECTION) || (set_number == RX_DIRECTION) || (set_number == NO_DIRECTION))
	{
	          if(file1->file_size == 0)
		  {
                      file1->seq_num++;
		      if(file1->seq_num == 10000)
		          file1->seq_num = 1;	
                      genhrlogfilename(file1->hrlogfilename, file1->logstring, file1->seq_num);
                      file1->fd = open(file1->hrlogfilename,O_WRONLY | O_CREAT | O_APPEND, S_IRWXU | S_IRWXO | S_IRWXG);   
		      if(file1->fd < 0)
		          catch_signal(99);		 
                      sf_write_header_t(file1->fd,LINKTYPE_PRISM_HEADER,0,SNAPLEN);  
		      file1->file_size = file1->file_size + sizeof(struct pcap_file_header);	
		  }
	          dump_pcap_t(file1,preparedHeader_t, ptr);
		  //file1->file_size = file1->file_size + Pktsize + sizeof(struct pcap_pkthdr);	
		  if((file1->file_size >= (int)set_size))
		  {
		          if(close(file1->fd) < 0)
			      catch_signal(99);
			  sprintf(logsyscmd, "mv -f %s %s",file1->hrlogfilename,"ModelDeploymentPktCaptures/");
			  system(logsyscmd);
			  files++;
			  memset(file1->hrlogfilename,0,sizeof(file1->hrlogfilename));
			  file1->file_size = 0;		
		  }		
	}

}

int
sf_write_header_t(int fp, int linktype, int thiszone, int snaplen)
{
        struct pcap_file_header hdr;

        hdr.magic = TCPDUMP_MAGIC;
        hdr.version_major = PCAP_VERSION_MAJOR;
        hdr.version_minor = PCAP_VERSION_MINOR;

        hdr.thiszone = thiszone;
        hdr.snaplen = snaplen;
        hdr.sigfigs = 0;
        hdr.linktype = linktype;

        if (write(fp, (char *)&hdr,sizeof(hdr)) < sizeof(hdr))
                return (-1);
        return (0);
}

void
dump_pcap_t(struct file_records *fr,  struct pcap_pkthdr *h, char *sp)
{
	int len;
        struct pcap_pkthdr sf_hdr;

        sf_hdr.ts.tv_sec  = h->ts.tv_sec;
        sf_hdr.ts.tv_usec = h->ts.tv_usec;
        sf_hdr.caplen     = h->caplen;
        sf_hdr.len        = h->len;
	
	len = sizeof(sf_hdr) + h->caplen; 
	if(((fr->file_size + len) >= (int)set_size) || ((fr->sizeLeft + len) >= 8192)){
	//if((fr->sizeLeft + len) >= 8192){
	        write(fr->fd,fr->tmp_buff,fr->sizeLeft);
		fr->sizeLeft = 0;
	}
	
	memcpy(fr->tmp_buff+fr->sizeLeft,(char*)&sf_hdr,sizeof(sf_hdr));
	memcpy(fr->tmp_buff+fr->sizeLeft+sizeof(sf_hdr), (char*)sp, h->caplen);
	fr->sizeLeft += len;
	fr->file_size += len;	
	
}

void open_logfiles(void)
{
   
    if((file1 != NULL) && (file1->file_size > 0))
    {	
        file1->fd= open(file1->hrlogfilename,O_WRONLY | O_CREAT | O_APPEND, S_IRWXU | S_IRWXO | S_IRWXG);   
	if(file1->fd < 0)
	    catch_signal(99);
    } 	
    if((file2 != NULL) && (file2->file_size > 0))
    {	
        file2->fd = open(file2->hrlogfilename,O_WRONLY | O_CREAT | O_APPEND, S_IRWXU | S_IRWXO | S_IRWXG);   
	if(file2->fd < 0)
	    catch_signal(99);
    }	
    if((file3 != NULL) && (file3->file_size > 0))
    {	
        file3->fd = open(file3->hrlogfilename,O_WRONLY | O_CREAT | O_APPEND, S_IRWXU | S_IRWXO | S_IRWXG);   
	if(file3->fd < 0)
	    catch_signal(99);
    }
    if((file4 != NULL) && (file4->file_size > 0))
    {	
        file4->fd = open(file4->hrlogfilename,O_WRONLY | O_CREAT | O_APPEND, S_IRWXU | S_IRWXO | S_IRWXG);   
	if(file4->fd < 0)
	    catch_signal(99);
    }	
}


void close_logfiles(void)
{
   
    if((file1 != NULL) && (file1->file_size > 0))
    { 
	fsync(file1->fd);	
        if(close(file1->fd) < 0)
	    catch_signal(99); 	 
    	files++;
	} 	
    if((file2 != NULL) && (file2->file_size > 0))
    { 
	fsync(file2->fd);	
        if(close(file2->fd) < 0) 
	    catch_signal(99); 	 
    	files++;
    } 	
    if((file3 != NULL) && (file3->file_size > 0))
    { 
	fsync(file3->fd);	
        if(close(file3->fd) < 0)
	    catch_signal(99); 	 
    	files++;
    } 	
    if((file4 != NULL) && (file4->file_size > 0))
    { 
	fsync(file4->fd);	
        if(close(file4->fd) < 0) 
	    catch_signal(99); 	 
    	files++;
    } 	
}


void filestr_gen(void )
{

	   if((flags == TX_RX_ON) && (set_direction == DIRECTION_ENABLED) && (set_interface == INTERFACE_SET))
           {
	       file1 = (struct file_records *)calloc(1,sizeof(struct file_records));
	       file2 = (struct file_records *)calloc(1,sizeof(struct file_records));
	       file3 = (struct file_records *)calloc(1,sizeof(struct file_records));
	       file4 = (struct file_records *)calloc(1,sizeof(struct file_records));

               set_number = TX_RX_DIRECTION_INTERFACE ;
               sprintf(file1->logstring, "%s_%s","wifi0","tx");
               sprintf(file2->logstring, "%s_%s","wifi0","rx");
               sprintf(file3->logstring, "%s_%s","wifi1","tx");
               sprintf(file4->logstring, "%s_%s","wifi1","rx");
			}
           else if((flags == TX_RX_ON) && (set_interface == INTERFACE_SET))
           {
	       file1 = (struct file_records *)calloc(1,sizeof(struct file_records));
	       file2 = (struct file_records *)calloc(1,sizeof(struct file_records));
               set_number = TX_RX_INTERFACE ;
               sprintf(file1->logstring, "%s_%s","wifi0","tx_rx");
               sprintf(file2->logstring, "%s_%s","wifi1","tx_rx");
           }
           else if((flags == TX_RX_ON) && (set_direction == DIRECTION_ENABLED))
           {
	       file1 = (struct file_records *)calloc(1,sizeof(struct file_records));
	       file2 = (struct file_records *)calloc(1,sizeof(struct file_records));
               set_number = TX_RX_DIRECTION ;
               sprintf(file1->logstring, "%s","tx");
               sprintf(file2->logstring, "%s","rx");

           }
           else if((flags == TX_ON) && (set_interface == INTERFACE_SET))
           {
	       file1 = (struct file_records *)calloc(1,sizeof(struct file_records));
	       file2 = (struct file_records *)calloc(1,sizeof(struct file_records));
               set_number = TX_INTERFACE ;
               sprintf(file1->logstring, "%s_%s","wifi0","tx");
               sprintf(file2->logstring, "%s_%s","wifi1","tx");
           }
           else if((flags == RX_ON) && (set_interface == INTERFACE_SET))
           {
	       file1 = (struct file_records *)calloc(1,sizeof(struct file_records));
	       file2 = (struct file_records *)calloc(1,sizeof(struct file_records));
               set_number = RX_INTERFACE ;
               sprintf(file1->logstring, "%s_%s","wifi0","rx");
               sprintf(file2->logstring, "%s_%s","wifi1","rx");
           }
           else if((flags == TX_ON) && (set_direction == DIRECTION_ENABLED))
           {
	       file1 = (struct file_records *)calloc(1,sizeof(struct file_records));
               set_number =TX_DIRECTION ;
               sprintf(file1->logstring, "%s","tx");
           }
           else if((flags == RX_ON) && (set_direction == DIRECTION_ENABLED))
           {
	       file1 = (struct file_records *)calloc(1,sizeof(struct file_records));
               set_number = RX_DIRECTION ;
               sprintf(file1->logstring, "%s","rx");
           }
	   else if(set_direction == 0)
	   {
	       file1 = (struct file_records *)calloc(1,sizeof(struct file_records));
	       set_number = NO_DIRECTION;	
               sprintf(file1->logstring, "%s","NULL");
	   }    
	
}

void SwapData(struct widi_loghdr *wHeader)
{
	wHeader->size = htons(wHeader->size);	
	wHeader->log_type = htons(wHeader->log_type);	
	wHeader->flags = htonl(wHeader->flags);	
	wHeader->time.tv_sec = htonl(wHeader->time.tv_sec);	
	wHeader->time.tv_usec = htonl(wHeader->time.tv_usec);	
}

int extract_data(WSMData *data,char *kml_lonlat)
{
	int msg_cnt = 0;
	int i;
	unsigned char * buffer;
	int lat ,lon;
	int lat1 ,lon1;
	int tmpid=0,tmpid1=0;
	double latrx,lonrx;

	if(*(unsigned char *)&data->contents[SECURITY] == 0x00)
	{
	msg_cnt = MSG_CNT_PLAIN;
	if(*(unsigned char *)&data->contents[SECURITY + 2] > 0x80)
        msg_cnt = msg_cnt+1;        
	}
	else if (*(unsigned char *)&data->contents[SECURITY] != 0x00)
	{
		if(*(unsigned char *)&data->contents[DIGEST] == 0x03)
		{
		msg_cnt = MSG_CNT_DIGEST;
		if(*(unsigned char *)&data->contents[84] > 0x7f)
	        msg_cnt = msg_cnt+2;
		}
		else if (*(unsigned char *)&data->contents[DIGEST] == 0x02)
		{
		msg_cnt = MSG_CNT_CERT;
		if(*(unsigned char *)&data->contents[13] > 0x7f)
	        msg_cnt = msg_cnt+2;        
		}
	}
	
	buffer = (unsigned char *)(data->contents + msg_cnt) ;

	//copy lat lon from buffer: msg_cnt+7 is lat, msg_cnt+11 is lon
	memcpy(&lat,buffer+7,4);
   	memcpy(&lon,buffer+11,4);

	//convert big endian to host
	lat1 = ntohl(lat);
	lon1 = ntohl(lon);

	latrx = (double)lat1/10000000;
	lonrx = (double)lon1/10000000;	

	printf("\nRecieved lat,lon  = %lf,%lf\n", latrx,lonrx);

	memcpy(&tmpid,buffer+1,2);//taken first two bytes for dev_id else tempid is 4 bytes
	tmpid1 = ntohl(tmpid);
	tmpid1 =((0x0000FFFF)&(tmpid1 >> 16)); //first 2 bytes are needed

	//data for kml file: bufferlonlat
	sprintf(kml_lonlat,"%lf,%lf,0 ",lonrx,latrx);
	return tmpid1;
}
void fill_buffer(char *buffer,int sig,int tmpid)
{
	char string1[] = "<?xml version=\"1.0\" standalone=\"yes\"?>";
	char string2[] = "<kml xmlns=\"http://earth.google.com/kml/2.2\">";
	char string3[] = "<Document>";
	char string4[] = "<name><![CDATA[LOCOMATE-ARADA]]></name>";
	char string5[] = "<visibility>1</visibility>";
	char string6[] = "<open>1</open>";
	char string7[] = "<Snippet></Snippet>";
	char string8[] = "<Folder id=\"Tracks\">";
	char string9[] = "<name>Tracks</name>";
	char string10[] = "<visibility>1</visibility>";
	char string11[] = "<open>0</open>";
	char string12[] = "<Placemark>";
	char string13[60] ;//= "<name><![CDATA[wsmpforwardserver]]></name>";
	char string14[] = "<Snippet></Snippet>";
	char string15[] = "<description><![CDATA[&nbsp;]]></description>";
	char string16[] = "<Style>";
	char string17[] = "<LineStyle>";
	char string18[] = "<color>FF0000E6</color>";
	char string19[] = "<width>2</width>";
	char string20[] = "</LineStyle>";
	char string21[] = "</Style>";
	char string22[] = "<MultiGeometry>";
	char string23[] = "<LineString>";
	char string24[] = "<tessellate>1</tessellate>";
	char string25[] = "<altitudeMode>clampToGround</altitudeMode>";
	char string26[] = "<coordinates>";
	char string27[] = "</coordinates>";
	char string28[] = "</LineString>";
	char string29[] = "</MultiGeometry>";
	char string30[] = "</Placemark>";
	char string31[] = "</Folder>";
	char string32[] = "</Document>";
	char string33[] = "</kml>";
	
	//sig = 0 start, sig = 1 end
	if(sig == 0){
	sprintf(string13,"<name><![CDATA[devid : %x]]></name>",tmpid);
	//create start buffer
	sprintf(buffer,"%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n%s\n",string1,string2,string3,string4,string5,string6,string7,string8,string9,string10,string11,string12,string13,string14,string15,string16,string17,string18,string19,string20,string21,string22,string23,string24,string25,string26);
	}
	else if(sig == 1){
	//create end buffer
	sprintf(buffer,"%s\n%s\n%s\n%s\n%s\n%s\n%s",string27,string28,string29,string30,string31,string32,string33);
	}
	else{
	printf("\nInvalid token\n");
	}
}
