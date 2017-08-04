/*
 * Reference: "Bluetooth for Programmers". 2005. Huang, Albert & Rudolph, Larry. http://people.csail.mit.edu/rudolph/Teaching/Articles/BTBook.pdf
 */

#include "BTServer.h"
#include "infloUIMsg.h"
#include "QWarnController.h"
#include "asnwave.h"
#include <bluetooth/sdp.h>
#include <bluetooth/sdp_lib.h>
#include <pthread.h>
#include <time.h>

static net_connection_t bt_connection[MAX_CONNECTIONS];

struct sockaddr_rc loc_addr = { 0 };
struct sockaddr_rc rem_addr = { 0 };

uint8_t rfcomm_chan = (uint8_t) 1;
uint32_t service_uuid_int[] = {0x6ba50000, 0x7c6a11e3, 0x9b950002, 0xa5d5c51b};
uuid_t l2cap_uuid, root_uuid, rfcomm_uuid, service_uuid;
sdp_list_t *service_class_list = 0, *l2cap_list = 0, *rfcomm_list = 0, *root_list = 0, *protocol_list = 0, *access_protocol_list = 0;
sdp_data_t *channel = 0;
sdp_session_t *ses = 0;

pthread_t send_thread_id;

char buf[1024] = { 0 };
int 	s;
int 	client;
int 	bytes_read;

socklen_t opt = sizeof(rem_addr);

#define BT_MSG_LENGTH 3500

extern wtr btwtr;
extern veh vehData;
extern void OnRecvAlertMsg(AlertMessage *pMessage);

/**
 * This will set the adapter to a known state using linux commands
 * Initialize the BT adapter
 * 		Take the adapter down to clear all connections
 * 		Bring the adapter up
 * 		Give it a name
 * 		Make it discoverable
 */
void initializeAdapter() {
	// Reset Bluetooth and Initialize
	char cmd[50];
	sprintf(cmd, "/usr/local/bin/hciconfig hci0 up piscan\0");
	printf("Executing command:%s\n", cmd);
	system(cmd);
//	sprintf(cmd, "/usr/local/bin/hciconfig hci0 up\0");
//	printf("Executing command:%s\n", cmd);
//	system(cmd);
	//sprintf(cmd, "/usr/local/bin/hciconfig hci0 name 'OBU-Battelle'\0");
	//printf("Executing command:%s\n", cmd);
//	system(cmd);
//	sprintf(cmd, "/usr/local/bin/hciconfig hci0 piscan\0");
//	printf("Executing command:%s\n", cmd);
//	system(cmd);
}

/**
 * Creates a socket and will allow multiple clients to the connection.
 *
 */
void *bt_server( void *data )
{

	sdp_record_t *rec = sdp_record_alloc();
	
	initializeAdapter();

	// Allocate connections
	int i;
	for(i=0;i<MAX_CONNECTIONS;i++)
	{
		bt_connection[i].client_sock = (int) NULL;
		bt_connection[i].status = DISCONNECTED;
	}

	// allocate socket
	printf("Allocating Socket\n");
	s = socket(AF_BLUETOOTH, SOCK_STREAM, BTPROTO_RFCOMM);

	// bind socket to port 1 of the first available
	// local bluetooth adapter
	loc_addr.rc_family = AF_BLUETOOTH;
	loc_addr.rc_bdaddr = *BDADDR_ANY;
	loc_addr.rc_channel = rfcomm_chan;

	//Create record for service to broadcast (connect with smartphone)
	sdp_uuid128_create(&service_uuid, &service_uuid_int);
	sdp_set_service_id(rec, service_uuid);
	service_class_list = sdp_list_append(0, &service_uuid);
	sdp_set_service_classes(rec, service_class_list);

	sdp_uuid16_create(&root_uuid, PUBLIC_BROWSE_GROUP);
	root_list = sdp_list_append(0, &root_uuid);
	sdp_set_browse_groups(rec, root_list);

	sdp_uuid16_create(&l2cap_uuid, L2CAP_UUID);
	l2cap_list = sdp_list_append(0, &l2cap_uuid);

	sdp_uuid16_create(&rfcomm_uuid, RFCOMM_UUID);
	channel = sdp_data_alloc(SDP_UINT8, &rfcomm_chan);
	rfcomm_list = sdp_list_append(0, &rfcomm_uuid);
	sdp_list_append(rfcomm_list, channel);
	protocol_list = sdp_list_append(0, rfcomm_list);

	access_protocol_list = sdp_list_append(0, protocol_list);
	sdp_set_access_protos(rec, access_protocol_list);

	sdp_set_info_attr(rec, "Battelle OBU", "Battelle", "DSRC message broadcaster"); 

	//Bind to socket
	printf("Binding Socket\n");
	if(bind(s, (struct sockaddr *)&loc_addr, sizeof(loc_addr)))
		perror("BIND");


	
	ses = sdp_connect(BDADDR_ANY, BDADDR_LOCAL, SDP_RETRY_IF_BUSY);
	if (ses != NULL)
	{
		int error = sdp_record_register(ses, rec, 0);
	}
	//Free service lists
	sdp_data_free(channel);
	sdp_list_free(l2cap_list, 0);
	sdp_list_free(rfcomm_list, 0);
	sdp_list_free(root_list, 0);
	sdp_list_free(access_protocol_list, 0);

	// put socket into listening mode
	printf("Listening ...\n");

	pthread_create(&send_thread_id,NULL,sendConnectionHandler,NULL);

	while(1)
		{
			for(i=0;(i<MAX_CONNECTIONS) & (bt_connection[i].client_sock != (int) NULL);i++);	//look for first available socket fd

			if(i < MAX_CONNECTIONS)
			{
				listen(s, 1);
				client = accept(s, (struct sockaddr *)&rem_addr, &opt);
				if(client >= 0)
				{
					ba2str( &rem_addr.rc_bdaddr, buf );
					fprintf(stderr, "accepted connection from %s\n", buf);
					memset(buf, 0, sizeof(buf));

					printf("new connection %d\n",i);
					bt_connection[i].client_sock = client;
					bt_connection[i].status = CONNECTED;
					pthread_create(&bt_connection[i].thread_id,NULL,btConnectionHandler,&bt_connection[i]);
				}

			}
			else
				printf("No open sockets\n");
			sleep(1);
			pthread_testcancel();
		}

	return 0;
}


void parseMessage(char *msg)
{
	wtr wtrData;
	veh vehData;
	printf("MESSAGE: %s\n",msg);
	
	if(strstr(msg,"TIM") == msg) {
		TravelerInformation_t *tim = (TravelerInformation_t *)calloc(1, sizeof(TravelerInformation_t));
		parseTIMData(tim, msg);
		qwarnOnRecvMessage(tim, WSMMSG_TIM);

		
	} else if(strstr(msg,"WTR") == msg) {
		parseWeatherData(msg,&wtrData);
		memcpy(&btwtr, &wtrData, sizeof(btwtr));
		btveh.external_air_temperature = -1000;
		btveh.barometric_pressure = -1;

	} else if(strstr(msg,"VEH") == msg) {
		parseVEHData(msg,&vehData);
		memcpy(&btveh, &vehData,sizeof(btveh));
	}
}

/**
 * Listener thread that will listen for incoming data. At this
 * point there is no incoming traffic specified. This thread will
 * detect a closing of the socket and will set the connection to
 * DICONNECTED. When the main thread cycles through it will close
 * the socket for that connection and set the values to null so the
 * connection can be used again for any subsequent clients.
 */
void *btConnectionHandler(net_connection_t *net_connection)
{
	char ch[100];
	char msg[BT_MSG_LENGTH];

	int retvalue;
	int msgLength = 0;

	qwarnSetAlertCallback(&OnRecvAlertMsg);
	
	while(net_connection->status == CONNECTED)
	{
		retvalue = read(net_connection->client_sock,&ch,sizeof(ch));
		if(retvalue < 0)	//connection has been closed
		{
			net_connection->status = DISCONNECTED;
		} else if(retvalue > 0){
			
			if (retvalue + msgLength >= BT_MSG_LENGTH)
			{
				msgLength = 0;
			}
			memcpy(msg + msgLength, ch, retvalue);
			msgLength += retvalue;
			msg[msgLength] = '\0';

			char *p_msg_start = strstr(msg, "\x02");
			if (p_msg_start == NULL)
			{
				msgLength = 0;
			}
			else
			{
				if(p_msg_start != msg)
				{
					int shift = (p_msg_start - msg);
					memcpy(msg, msg + shift, msgLength + 1 - shift);
					msgLength -= shift;
				}

				char *p_msg_end = strstr(msg, "\x03");
				if(p_msg_end != NULL)
				{
					*p_msg_end = '\0';
					printf("BTServer::btConnectionHandler() Received Message %s\n", msg + 1);
					parseMessage(msg + 1);
					int shift = (p_msg_end - msg) + 1;
					memcpy(msg, msg + shift, msgLength + 1 - shift);
					msgLength -= shift;
					printf("BTServer::btConnectionHandler() New message length = %d\n", msgLength);
				}
			}
		} else {
			printf("BT Connection closed\n");
			net_connection->status = DISCONNECTED;
		}
		sleep(0);
	}

	//TODO: Shut socket down
	retvalue = close(net_connection->client_sock);
	if (retvalue < 0) {
		printf("btConnectionHandler() - Error Closing Socket...\n");
	}
	else {
		printf("Bluetooth Socket Closed\n");
	}
	net_connection->client_sock = NULL;
}

/**
 * Will send a message to all currently attached clients
 */
void sendMessageToAllClients(char *message)
{
	int retvalue;
	int i;
	//scroll through each connected client
	for(i=0 ;i<MAX_CONNECTIONS ;i++)
	{
		if (bt_connection[i].status == CONNECTED) {
			retvalue = write(bt_connection[i].client_sock,message,strlen(message));
			if (retvalue < 0) {
				printf("Error Sending to BT Connection %d. Setting to DISCONNECTED\n", i);
				bt_connection[i].status = DISCONNECTED;
			}
		}
	}
}

//Messages that need to be sent periodically
void *sendConnectionHandler(void * arg)
{
	const int DIA_PERIOD = 500;
	const int TRQ_PERIOD = 1500;
	const int SLEEP_TIME = 250;
	
	int diatime = 0;
	int trqtime = 0;
    
    VehicleState previousState;
	qwarnGetCurrentVehicleState(&previousState);
	
	while(1)
	{
	    VehicleState currentState;
		qwarnGetCurrentVehicleState(&currentState);
		
		if(strcmp(previousState.roadwayId, currentState.roadwayId) != 0){
			sendClrQWarnUIAlert();
			sendClrSpdHarmUIAlert();
			previousState = currentState;
		}
		diatime += SLEEP_TIME;
		trqtime += SLEEP_TIME;

		if (diatime > DIA_PERIOD)
		{
			diatime -= DIA_PERIOD;
			sendDIA();
		}

		if (trqtime > TRQ_PERIOD)
		{
			trqtime -= TRQ_PERIOD;
			
			if (!RSU_Flag)
				sendTRQ();
		}
		usleep(SLEEP_TIME * 1000);
		pthread_testcancel();
	}
}

int checkRSUConnection(time_t Curr_Timer, time_t RSU_Timer, int RSU_Flag) {
	time(&Curr_Timer);
	if(difftime(Curr_Timer, RSU_Timer) > RSUTIMEOUT) {
		RSU_Flag = 0;	
	} else {
		RSU_Flag = 1;
	}
	return RSU_Flag;
}

/**
 * Destroys the server, closes connections and cancels running threads.
 */
void btDestroy()
{
	pthread_cancel(send_thread_id);
	pthread_join(send_thread_id, NULL);

	int i;
	for(i=0;i<MAX_CONNECTIONS;i++)
	{
		if(bt_connection[i].client_sock != (int) NULL)	//If it is not null then it exist so try to cancel it
		{
			bt_connection[i].status == DISCONNECTED;
			shutdown(bt_connection[i].client_sock, SHUT_RD);
			if(!(pthread_join(bt_connection[i].thread_id,NULL)))
				printf("Bluetooth Thread (%d) Joined - shutdown\n", i);
		}
	}

	if (ses != NULL)
	{
		sdp_close(ses);
		ses = NULL;
	}
	if (s != NULL)
	{
		//close socket
		int returnVal = close(s);
		if (returnVal < 0)
		{
			printf("btDestroy() - Error Closing Server Socket ...\n");
		}
		else
		{
			printf("Bluetooth Server Socket Closed\n");
		}

		s = NULL;
	}
}
