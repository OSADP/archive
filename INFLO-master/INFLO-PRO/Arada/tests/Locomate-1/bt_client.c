#include <stdio.h>
#include <unistd.h>
#include <sys/socket.h>
#include <bluetooth/bluetooth.h>
#include <bluetooth/hci.h>
#include <bluetooth/rfcomm.h>

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
	sprintf(cmd, "/usr/local/bin/hciconfig hci0 down\0");
	printf("Executing command:%s\n", cmd);
	system(cmd);
	sprintf(cmd, "/usr/local/bin/hciconfig hci0 up\0");
	printf("Executing command:%s\n", cmd);
	system(cmd);
	sprintf(cmd, "/usr/local/bin/hciconfig hci0 name 'Arada1'\0");
	printf("Executing command:%s\n", cmd);
	system(cmd);
	sprintf(cmd, "/usr/local/bin/hciconfig hci0 piscan\0");
	printf("Executing command:%s\n", cmd);
	system(cmd);
}

int main(int argc, char **argv)
{
    struct sockaddr_rc addr = { 0 };
    int status;
    char buf[1024] = { 0 };
    int bytes_read;
    //int sk
    //uint16_t ptype = 0x0000;
    //uint16_t clkoffset = 0x0000;
    //uint16_t handle;
    //uint8_t rswitch = 0x01;


   //  char dest[18] = "00:1F:81:00:08:30";
   //  char dest[18] = "00:10:60:57:12:F7";
   //  char dest[18] = "00:10:60:57:16:12";
   //  char dest[18] = "00:06:66:47:40:A7";
       char dest[18] = "00:26:AD:01:F0:8A";
    initializeAdapter();
    // allocate a socket
    int sock = socket(AF_BLUETOOTH, SOCK_STREAM, BTPROTO_RFCOMM);

    // set the connection parameters (who to connect to)
    addr.rc_family = AF_BLUETOOTH;
    addr.rc_channel = (uint8_t) 1;
    str2ba( dest, &addr.rc_bdaddr );


    //int dev_id = hci_get_route(NULL);
    //sk = hci_open_dev(dev_id);
    //int hciconnect = hci_create_connection(sk, &addr.rc_bdaddr, htobs(ptype), htobs(clkoffset), rswitch, &handle, (uint16_t) 25000);

    // connect to server
    printf("Connecting to BT\n");
    status = connect(sock,(struct sockaddr*)&addr, sizeof(addr));
    printf("Status : %d\n", status);

    //char name[248];
    //if (hci_read_remote_name(sk, &addr.rc_bdaddr, sizeof(name), name, 25000) == 0) {
    //	  printf("Remote Name: %s\n", name);
    // }
    //int8_t rssi;
  	// int local_name = hci_local_name(sk, 100, name, 25000);
  	// printf("Local Name: %s\n",name);
  	//  if (hci_read_rssi(sk, htobs(handle), &rssi, 25000) < 0) {
  	//	  	  perror("Read RSSI failed");
  	// }

    if(status < 0) {
      	perror("Connect error");
      }
    if(status == 0) {
    	printf("Sending get time\n");
    	status = write(sock, "%GET TIME;\r\n", 11);
    }
    if(status >= 0) {
    	printf("Reading...\n");
    	// read data from the client
    	bytes_read = read(sock, buf, sizeof(buf));
    	if( bytes_read > 0 ) {
    		printf("received [%s]\n", buf);
    	} else{
    		perror("Read error");
    	}
    } else {
    	perror("Write error");
    }

    close(sock);
    return 0;
}
