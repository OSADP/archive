#define WIN32_LEAN_AND_MEAN

#include <windows.h>
#include <winsock2.h>
#include <ws2tcpip.h>
#include <stdlib.h>
#include <stdio.h>


/*Need to link with Ws2_32.lib, Mswsock.lib, and Advapi32.lib*/
#pragma comment (lib, "Ws2_32.lib")
#pragma comment (lib, "Mswsock.lib")
#pragma comment (lib, "AdvApi32.lib")

#define DEFAULT_PORT "8947"
#define GPS_INVALID_DATA -5000.0

void SwapBytes(void *pv, size_t n)
{
    char *p = pv;
    size_t lo, hi;
    for(lo=0, hi=n-1; hi>lo; lo++, hi--)
    {
        char tmp=p[lo];
        p[lo] = p[hi];
        p[hi] = tmp;
    }
}
#define SWAP(x) SwapBytes(&x, sizeof(x));


#pragma pack(1)
typedef struct{
        double          actual_time;            
        double          time;
        double          local_tod;
        unsigned long  long  local_tsf;
        double          latitude;
        char            latdir;
        double          longitude;
        char            longdir;
        double          altitude;
        char            altunit;
        double          course;
        double          speed;
        double          climb;
        double          tee;
        double          hee;
        double          vee;
        double          cee;
        double          see;
        double          clee;
        double          hdop;
        double          vdop;
        unsigned char     numsats;
        unsigned char    fix;
        double          tow;
        int             date;
        double          epx;
        double          epy;
        double          epv;
}GPSData; 

static GPSData wsmgps;


int __cdecl main(int argc, char **argv) 
{
    WSADATA wsaData;
	SOCKET ConnectSocket = INVALID_SOCKET;
    struct addrinfo *result = NULL,
                    *ptr = NULL,
                    hints;
    char *sendbuf = "1";
    int iResult=0,status=0,one=1,size=sizeof(GPSData);
     
    
    
    /*Validate the parameters*/
    if (argc != 2) {
        printf("usage: %s <Locomate Ip Address>\n", argv[0]);
        return 1;
    }

    /*Initialize Winsock*/
    iResult = WSAStartup(MAKEWORD(2,2), &wsaData);
    if (iResult != 0) {
        printf("WSAStartup failed with error: %d\n", iResult);
        return 1;
    }

    ZeroMemory( &hints, sizeof(hints) );
    hints.ai_family = AF_INET;
    hints.ai_socktype = SOCK_STREAM;
    hints.ai_protocol = IPPROTO_TCP;

    /*Resolve the server address and port*/
    iResult = getaddrinfo(argv[1], DEFAULT_PORT, &hints, &result);
    if ( iResult != 0 ) {
        printf("getaddrinfo failed with error: %d\n", iResult);
        WSACleanup();
        return 1;
    }

    /*Attempt to connect to an address until one succeeds*/
    for(ptr=result; ptr != NULL ;ptr=ptr->ai_next) {

        /*Create a SOCKET for connecting to server*/
        ConnectSocket = socket(ptr->ai_family, ptr->ai_socktype, 
            ptr->ai_protocol);
        if (ConnectSocket == INVALID_SOCKET) {
            printf("socket failed with error: %ld\n", WSAGetLastError());
            WSACleanup();
            return 1;
        }
        /*socket option*/
	if(setsockopt(ConnectSocket,SOL_SOCKET,SO_REUSEADDR,(char *)&one,sizeof(one)) == -1)
	{
		
            printf("setsockopt failed with error: %ld\n", WSAGetLastError());
            WSACleanup();
            return 1;
	}

	/*Connect to server.*/
        iResult = connect( ConnectSocket, ptr->ai_addr, (int)ptr->ai_addrlen);
	if (iResult == SOCKET_ERROR) {
            closesocket(ConnectSocket);
            ConnectSocket = INVALID_SOCKET;
            continue;
        }
        break;
    }

    freeaddrinfo(result);

    if (ConnectSocket == INVALID_SOCKET) {
        printf("\nPlease Check the Ip Address of Locomate\n");
        WSACleanup();
        return 1;
    }

    /*end an initial buffer*/
   iResult = send( ConnectSocket, sendbuf, (int)strlen(sendbuf), 0 );
    if (iResult == SOCKET_ERROR) {
        printf("send failed with error: %d\n", WSAGetLastError());
        closesocket(ConnectSocket);
        WSACleanup();
        return 1;
    }

    printf("Size of GPSData %d\n\n",size);
	//shutdown the connection since no more data will be sent
    iResult = shutdown(ConnectSocket, SD_SEND);
    if (iResult == SOCKET_ERROR) {
        printf("shutdown failed with error: %d\n", WSAGetLastError());
        closesocket(ConnectSocket);
        WSACleanup();
        return 1;
    }

    /*Receive the GPSDATA*/
    

        status = recv(ConnectSocket,(char *)&wsmgps,size, 0);
		//printf("recv status %d\n\n",status);
		if(!BIGENDIAN)
		SWAP(wsmgps.actual_time);
		if(wsmgps.actual_time == GPS_INVALID_DATA || wsmgps.actual_time == 0.0)
                {
                    printf("gpstime:wrg at=%lf fix=%d\n",wsmgps.actual_time,wsmgps.fix);
					printf("Please Check Gps Setup... Whether working or not...\n");
					exit (0);
				}
	 if ( status > 0)
		{
			if(!BIGENDIAN)
			{
				printf("LITTLE ENDIAN\n");
				//SWAP(wsmgps.actual_time);
				printf("\nActual Time = %lf\n",wsmgps.actual_time);
				SWAP(wsmgps.time);
				printf("\nTime = %lf\n",wsmgps.time);
				SWAP(wsmgps.local_tod);
				printf("\nLocal_tod = %lf\n",wsmgps.local_tod);
				SWAP(wsmgps.latitude);
				printf("\nLatitude= %lf\n",wsmgps.latitude);
				SWAP(wsmgps.longitude);
				printf("\nLongitude= %lf\n",wsmgps.longitude);
				SWAP(wsmgps.altitude);
				printf("\nAltitude = %lf\n",wsmgps.altitude);
				SWAP(wsmgps.date);
				printf("\nDate = %d\n",wsmgps.date);
				printf("\nNum of sats = %u\n",wsmgps.numsats);		
				printf("\nFix = %u\n",wsmgps.fix);
				SWAP(wsmgps.epx);
				printf("\nEpx = %lf\n",wsmgps.epx);			
				SWAP(wsmgps.epy);
				printf("\nEpy = %lf\n",wsmgps.epy);			
				SWAP(wsmgps.epv);
				printf("\nEpv = %lf\n",wsmgps.epv);
							
			}		
			else 
			{
				printf("BIG ENDIAN");
				printf("\nActual Time = %lf\n",wsmgps.actual_time);			
				printf("\nTime = %lf\n",wsmgps.time);
				printf("\nLocal_tod = %lf\n",wsmgps.local_tod);
				printf("\nLatitude= %lf\n",wsmgps.latitude);
				printf("\nLongitude= %lf\n",wsmgps.longitude);	
				printf("\nAltitude = %lf\n",wsmgps.altitude);
				printf("\nDate = %d\n",wsmgps.date);
				printf("\nNumsats = %\n",wsmgps.numsats);		
				printf("\nFix = %lf\n",wsmgps.fix);
				printf("\nEpx = %lf\n",wsmgps.epx);			
				printf("\nEpy = %lf\n",wsmgps.epy);			
				printf("\nEpv = %lf\n",wsmgps.epv);
			}


		
		
		}
        else if ( status == 0 )
            printf("Connection closed\n");
        else
            printf("recv failed with error: %d\n", WSAGetLastError());

    
	//printf("RecvBuf=%s",recvbuf);

    
	
	/*cleanup*/
    closesocket(ConnectSocket);
    WSACleanup();
#ifdef WIN32
	system("PAUSE");
#endif
    return 0;
}



