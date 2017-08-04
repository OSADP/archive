#include <stdio.h>
#include <unistd.h>
#include <sys/socket.h>
#include <bluetooth/bluetooth.h>
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
    struct sockaddr_rc loc_addr = { 0 }, rem_addr = { 0 };
    char buf[1024] = { 0 };
    int s, client, bytes_read;
    socklen_t opt = sizeof(rem_addr);

    // allocate socket
    initializeAdapter();

    printf("Allocating Socket\n");
    s = socket(AF_BLUETOOTH, SOCK_STREAM, BTPROTO_RFCOMM);

    // bind socket to port 1 of the first available
    // local bluetooth adapter
    loc_addr.rc_family = AF_BLUETOOTH;
    loc_addr.rc_bdaddr = *BDADDR_ANY;
    loc_addr.rc_channel = (uint8_t) 1;
    printf("Binding Socket\n");
    bind(s, (struct sockaddr *)&loc_addr, sizeof(loc_addr));

    // put socket into listening mode
    printf("Listening ...\n");

    listen(s, 1);

    // accept one connection
    client = accept(s, (struct sockaddr *)&rem_addr, &opt);


    printf("Responding in kind...\n");
    int status = write(client, "Thank You!", 10);

    printf("Got Connection\n");

    ba2str( &rem_addr.rc_bdaddr, buf );
    fprintf(stderr, "accepted connection from %s\n", buf);
    memset(buf, 0, sizeof(buf));

    // read data from the client
    bytes_read = read(client, buf, sizeof(buf));
    if( bytes_read > 0 ) {
        printf("received [%s]\n", buf);
    }

    sleep(1);
    printf("Responding in kind...\n");
    status = write(client, "Thank You!", 10);

    if( status < 0 ) perror("Error");

    printf("Closing Socket\n");

    // close connection
    close(client);
    close(s);
    return 0;
}
