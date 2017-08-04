#include <stdio.h>
#include <unistd.h>
#include <sys/socket.h>
#include <signal.h>
#include <bluetooth/bluetooth.h>
#include <bluetooth/rfcomm.h>
#include <bluetooth/sdp.h>
#include <bluetooth/sco.h>
#include <bluetooth/sdp_lib.h>
#include <bluetooth/l2cap.h>
#include <pthread.h>
#include <errno.h>

#include "wave.h"

#define SERVER_DEVICE 1
#define CLIENT_DEVICE 2

#define CLIENT_STRING ((char *)("client"))

static WMEApplicationRequest entry;
static WMETARequest tareq;
static WSMRequest wsmreq;
static int pid;
static int client_server;

sdp_session_t* register_service(uint8_t rfcomm_channel)
{
    uint32_t svc_uuid_int[] =   {   0xfa87c0d0, 0xafac11de, 0x8a390800, 0x200c9a66 };
    const char *service_name = "Roto-Rooter Data Router";
    const char *svc_dsc = "An experimental plumbing router";
    const char *service_prov = "Roto-Rooter";

    uuid_t root_uuid, l2cap_uuid, rfcomm_uuid, svc_uuid,
           svc_class_uuid;
    sdp_list_t *l2cap_list = 0,
               *rfcomm_list = 0,
               *root_list = 0,
               *proto_list = 0,
               *access_proto_list = 0,
               *svc_class_list = 0,
               *profile_list = 0;
    sdp_data_t *channel = 0;
    sdp_profile_desc_t profile;
    sdp_record_t record = { 0 };
    sdp_session_t *session = 0;

printf("\n %s...%d...\n", __func__, __LINE__);
    sdp_uuid128_create( &svc_uuid, &svc_uuid_int );
    sdp_set_service_id( &record, svc_uuid );
    svc_class_list = sdp_list_append(0, &svc_uuid);
    sdp_set_service_classes(&record, svc_class_list);
    char str[256] = "";
    sdp_uuid2strn(&svc_uuid, str, 256);
    printf(".... Registering UUID \n #### %s####\n", str);

/*
    sdp_uuid16_create(&svc_class_uuid, SERIAL_PORT_SVCLASS_ID);
    svc_class_list = sdp_list_append(0, &svc_class_uuid);
    sdp_set_service_classes(&record, svc_class_list);

printf("\n %s...%d\n", __func__, __LINE__);

    sdp_uuid16_create(&profile.uuid, SERIAL_PORT_PROFILE_ID);
    profile.version = 0x0100;
    profile_list = sdp_list_append(0, &profile);
    sdp_set_profile_descs(&record, profile_list);
*/

printf("\n %s...%d\n", __func__, __LINE__);
    sdp_uuid16_create(&root_uuid, PUBLIC_BROWSE_GROUP);
    root_list = sdp_list_append(0, &root_uuid);
    sdp_set_browse_groups( &record, root_list );

    sdp_uuid16_create(&l2cap_uuid, L2CAP_UUID);
    l2cap_list = sdp_list_append( 0, &l2cap_uuid );
    proto_list = sdp_list_append( 0, l2cap_list );

    sdp_uuid16_create(&rfcomm_uuid, RFCOMM_UUID);
    channel = sdp_data_alloc(SDP_UINT8, &rfcomm_channel);
    rfcomm_list = sdp_list_append( 0, &rfcomm_uuid );
    sdp_list_append( rfcomm_list, channel );
    sdp_list_append( proto_list, rfcomm_list );

printf("\n %s...%d\n", __func__, __LINE__);
    access_proto_list = sdp_list_append( 0, proto_list );
printf("\n %s...%d\n", __func__, __LINE__);
    sdp_set_access_protos( &record, access_proto_list );

printf("\n %s...%d\n", __func__, __LINE__);
    sdp_set_info_attr(&record, service_name, service_prov, svc_dsc);
printf("\n %s...%d\n", __func__, __LINE__);

    session = sdp_connect(BDADDR_ANY, BDADDR_LOCAL, SDP_RETRY_IF_BUSY);

    if (session == NULL) {
printf("\n %s...%d\n", __func__, __LINE__);
    } else {
printf("\n ##### %s...%d\n", __func__, __LINE__);
    }
printf("\n %s...%d\n", __func__, __LINE__);
    sdp_record_register(session, &record, 0);

printf("\n %s...%d\n", __func__, __LINE__);
    sdp_data_free( channel );
    sdp_list_free( l2cap_list, 0 );
    sdp_list_free( rfcomm_list, 0 );
    sdp_list_free( root_list, 0 );
    sdp_list_free( access_proto_list, 0 );
    sdp_list_free( svc_class_list, 0 );
    sdp_list_free( profile_list, 0 );

    return session;
}


int buildPSTEntry(char **argv)
{
    memset(&entry, 0 , sizeof(WMEApplicationRequest));
    entry.psid = 25;
    entry.priority = atoi(argv[5]);
    entry.channel = atoi(argv[4]);

    if (client_server == SERVER_DEVICE) {
        entry.repeatrate = 50; // repeatrate =50 per 5seconds = 1Hz
        if (atoi(argv[1]) > 1) {
            printf("channel access set default to alternating access\n");
            entry.channelaccess = CHACCESS_ALTERNATIVE;
        } else {
            entry.channelaccess = atoi(argv[1]);
        }
    } else {
        if ((atoi(argv[6]) > USER_REQ_SCH_ACCESS_NONE) || (atoi(argv[6]) < USER_REQ_SCH_ACCESS_AUTO)) {
            printf("User request type invalid: setting default to auto\n");
            entry.userreqtype = USER_REQ_SCH_ACCESS_AUTO;

        } else {
            entry.userreqtype = atoi(argv[6]);
        }

        entry.schaccess  = atoi(argv[7]);
        entry.schextaccess = atoi(argv[8]);
    }

    return 1;
}

int buildWMETARequest(char **argv)
{
    if (client_server == CLIENT_DEVICE) {
        tareq.action = TA_ADD;
        tareq.repeatrate = 100; 
        tareq.channel = atoi(argv[2]);
        tareq.channelinterval = atoi(argv[3]);
        tareq.servicepriority = 1;
     } else {
     }

    return 0;
}

int init_wave (char **argv)
{
    int ret = -1;

    pid = getpid();
    buildPSTEntry(argv);
    if (client_server == SERVER_DEVICE) {
        buildWMETARequest(argv);
    }

printf("\n %s...%d\n", __func__, __LINE__);
    if (invokeWAVEDriver(0)< 0) {
printf("\n %s...%d\n", __func__, __LINE__);
        printf(stderr, "initWAVE: Failure in invoking WAVEDriver\n");
        return ret;
    }

    if (client_server == SERVER_DEVICE) {
        if (registerProvider(pid, &entry) < 0) {
printf("\n %s...%d\n", __func__, __LINE__);
            printf(stderr, "initWAVE: Failure in registering provider\n");
            return ret;
        }
    } else {
        if (registerUser(pid, &entry) < 0) {
printf("\n %s...%d\n", __func__, __LINE__);
            printf(stderr, "initWAVE: Failure in registering user\n");
            return ret;
        }
    }
    ret = 0;
    return ret;
}

int buildWSMRequestPacket(void *Data,int len)
{
    wsmreq.chaninfo.channel = entry.channel;
    wsmreq.chaninfo.rate = 3;
    wsmreq.chaninfo.txpower = 15;
    wsmreq.version = 1;
    wsmreq.security = 1;
    wsmreq.psid = 25;
    wsmreq.txpriority = 1;
    memset ( &wsmreq.data, 0, sizeof( WSMData));
    memcpy ( &wsmreq.data.contents, Data,len);
    wsmreq.data.length=len;
    return 1;

}

void sig_int_bluetooth(void)
{
}

void sig_int(void)
{
printf("\n %s...%d\n", __func__, __LINE__);
    if (client_server == CLIENT_DEVICE) {
        removeUser(pid, &entry);
    } else {
        removeProvider(pid, &entry);
    }
    signal(SIGINT, SIG_DFL);
    sig_int_bluetooth();    
    exit(0);

}

void sig_term(void)
{
printf("\n %s...%d\n", __func__, __LINE__);
    if (client_server == CLIENT_DEVICE) {
        removeUser(pid, &entry);
    } else {
        removeProvider(pid, &entry);
    }
    signal(SIGINT,SIG_DFL);
    sig_int_bluetooth();    
    exit(0);
}

int send_data_over_wsmp(int fd)
{
    int status=0,j=0,k=0,ret,count;
    static size_t i;
    unsigned char buffer[1200];

    printf("Will read the data now\n");
    memset(buffer, 0, sizeof(buffer));
fprintf(stderr, "\n venkata...%s...%d\n", __func__, __LINE__);
printf("\n venkata...%s...%d\n", __func__, __LINE__);
    while((status = read(fd, buffer, sizeof(buffer))) >= 0){
printf("\n venkata...%s...%d\n", __func__, __LINE__);
fprintf(stderr, "\nvenkata... %s...%d\n", __func__, __LINE__);
        buildWSMRequestPacket(buffer,status);
        ret = txWSMPacket(pid, &wsmreq);
        if( ret < 0) {
            printf("\n Error while sending data over WAVE interface\n");
        } else {
          //  printf("\n WAVE packet sent successfully...%d\n", status);
        }
    }
    return 1;
}

int bt_r_thread_main (int *pfd)
{
    int fd;

    fd = *pfd;

fprintf(stderr, "\n %s...%d\n", __func__, __LINE__);
    send_data_over_wsmp(fd);

    return 0;
}

int start_bluetooth_read_thread (int fd)
{
    int ret = -1;
    pthread_t bt_thread;

    ret = pthread_create(&bt_thread, NULL, bt_r_thread_main, &fd);
printf("\n %s...%d\n", __func__, __LINE__);
    ret = 0;
    return ret;
}

int send_data_over_bluetooth (int fd)
{
    int ret = -1;
    WSMIndication rxpkt;
    int count = 0;

    while (1) {
        memset(&rxpkt, 0, sizeof(WSMIndication));
        ret = rxWSMPacket(pid, &rxpkt); // rx wsmp pkt
        count++;
        if(ret > 0){
            printf("\nVenkata..ding... WAVE packet ret=%d rxlen=%d\n", ret,rxpkt.data.length);
            //printf("Received WSMP Packet txpower= %d, rateindex=%d Packet No =#%llu#\n", rxpkt.chaninfo.txpower, rxpkt.chaninfo.rate, count++);
            ret = write(fd, rxpkt.data.contents, rxpkt.data.length);
            if (ret < 0) {
                printf("\nVenkata..error = %s\n", strerror(errno));
                return ret;
            }
            count = 0;
        }
        if (count == 30000) {
            return ret;
        }
        sched_yield();
        usleep(1000);
    }

    ret = 0;
    return ret;
}

int bt_w_thread_main (int *pfd)
{
    int fd;

    fd = *pfd;

    send_data_over_bluetooth(fd);
    return 0;
}

int start_bluetooth_write_thread (int fd)
{
    int ret = -1;
    pthread_t bt_wthread;

    ret = pthread_create(&bt_wthread, NULL, bt_w_thread_main, &fd);
    ret = 0;
    return ret;
}

int main(int argc, char **argv)
{
#define PORT_NUMBER 1
    struct sockaddr_rc loc_addr = { 0 }, rem_addr = { 0 };
    char buf[1024] = { 0 };
    int s, client, bytes_read;
    socklen_t opt = sizeof(rem_addr);
    sdp_session_t *session;
    int ret;

    client_server = CLIENT_DEVICE;
    ret = signal(SIGINT, (void *)sig_int);
printf("\n %s...%d...ret = %d\n", __func__, __LINE__, ret);
    ret = signal(SIGTERM, (void *)sig_term);
printf("\n %s...%d...ret = %d\n", __func__, __LINE__, ret);
    ret = signal(SIGHUP, (void *)sig_term);
printf("\n %s...%d...ret = %d\n", __func__, __LINE__, ret);

    session = register_service(PORT_NUMBER);

    // allocate socket
    s = socket(AF_BLUETOOTH, SOCK_STREAM, BTPROTO_RFCOMM);

    // bind socket to port 1 of the first available 
    // local bluetooth adapter
    loc_addr.rc_family = AF_BLUETOOTH;
    loc_addr.rc_bdaddr = *BDADDR_ANY;
    loc_addr.rc_channel = (uint8_t) (PORT_NUMBER);
    bind(s, (struct sockaddr *)&loc_addr, sizeof(loc_addr));

    ret = init_wave(argv);

    if (ret < 0) {
printf("\n %s...%d\n", __func__, __LINE__);
        /* Venkatch: TODO: Send a meaningful message to the
        * bluetooth client
        */
        goto main_end;
    }

    listen(s, 1);
    while (1) {
    // put socket into listening mode

    // accept one connection
 
printf("\n Before accepting the conenction\n");
        client = accept(s, (struct sockaddr *)&rem_addr, &opt);
printf("\n After accepting the conenction\n");

        ba2str( &rem_addr.rc_bdaddr, buf );
fprintf(stderr, "accepted connection from %s\n", buf);
        memset(buf, 0, sizeof(buf));

printf("\n Venkata...%s...%d\n", __func__, __LINE__);
    // read data from the client
        bytes_read = read(client, buf, sizeof(buf));
        if( bytes_read > 0 ) {
            printf("received [%s]\n", buf);
        }

        if (strcmp(buf, CLIENT_STRING) == 0) {
            client_server = CLIENT_DEVICE;
        } else {
            client_server = SERVER_DEVICE;
        }
printf("\n Venkata...%s...%d...%d\n", __func__, __LINE__, client_server);

        if (client_server == CLIENT_DEVICE) {
        //start_bluetooth_write_thread(client);
            bt_w_thread_main(&client);
        } else {
        //start_bluetooth_read_thread(client);
            bt_r_thread_main(&client);
        }
        close(client);
    }
    if (client_server == CLIENT_DEVICE) {
        removeUser(pid, &entry);
    } else {
        removeProvider(pid, &entry);
    }
main_end:
    // close connection
    close(s);
    sdp_close(session);
    return 0;
}

