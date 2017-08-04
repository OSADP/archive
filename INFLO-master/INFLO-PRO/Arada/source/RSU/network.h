#ifndef	_NETWORK_H_
#define	_NETWORK_H_

//Message types
#define CONNECTION			"CONNECTION"
#define TELEMETRY			"TELEMETRY"
#define VERSION_REQUEST		"VERSION_REQUEST"
#define VERSION_REPLY		"VERSION_REPLY"
#define TIME				"TIME"
#define CROP				"CROP"
#define DISPLAY				"DISPLAY"

//Events
#define EVENTS_ERROR	"ERROR"
#define EVENTS_WARNING	"WARNING"
#define EVENTS_ADVISORY	"ADVISORY"
#define EVENTS_DANGER	"DANGER"

#define VERSION			1.0
// Definitions
#define DISCONNECTED	0
#define CONNECTED		1
#define MAX_CONNECTIONS	10

char 	Name[10];
char 	SINK_IP[20];
int 	Port;
char 	SINK_PORT[5];

char NodeName[20];

typedef struct
{
	long int message_type;
	char serialmessage[100];
}message_t;

void initializeTelemetry(void);
void netServer(void);
void sendMessageToAllClients(char *message);
void setPortNumber(int port);

#endif
