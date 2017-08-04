/***************************************************************************
 * 1. INCLUDES                                                             *
 ***************************************************************************/
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include "genericAPI.h"

int txsocket_id=-1;
UINT8 send_buff[10240];
UINT8 recv_buff[10240];

/**********************************************************************
 * The function certchange sends a certificate change request and
 * receives the response.
 *
 * The following request is sent:
 *  - EscMsg_CertChg
 **********************************************************************/
static int
certchange(void)
{
    int send_size = 0;
    int recv_size = 0;

    // send EscMsg_CertChg request
    bzero(send_buff, sizeof(send_buff));
    msg_create_cert_change(send_buff, &send_size);
    INFO("Send EscMsg_CertChg request. [0x%02x]", send_buff[0]);
    if (0 != AsmSend(send_buff, send_size,txsocket_id)) {
        return -1;
    }

    // receive EscMsg_CertChg response
    bzero(recv_buff, sizeof(recv_buff));
    recv_size = AsmRecv(recv_buff, sizeof(recv_buff),txsocket_id);
    if (recv_size <= 0) {
        return -1;
    }
    if (recv_buff[0] != CMD_OK_CERT_CHG_POST) {
        ERROR("Receive error. [0x%02x]", recv_buff[0]);
        return -1;
    }
    else {
        INFO("Receive EscMsg_CertChg response. [0x%02x]", recv_buff[0]);
    }
    return 0;
}

/**********************************************************************
 * The function restart sends a restart request and
 * receives the response.
 *
 * The following request is sent:
 *  - EscMsg_Restart
 **********************************************************************/
static int 
restart(void)
{
    int send_size = 0;
    int recv_size = 0;

    // send EscMsg_CertChg request
    bzero(send_buff, sizeof(send_buff));
    msg_create_restart_msg(send_buff, &send_size);
    INFO("Send EscMsg_Restart request. [0x%02x]", send_buff[0]);
    if (0 != AsmSend(send_buff, send_size,txsocket_id)) {
        return -1;
    }

    // receive EscMsg_CertChg response
    bzero(recv_buff, sizeof(recv_buff));
    recv_size = AsmRecv(recv_buff, sizeof(recv_buff),txsocket_id);
    if (recv_size <= 0) {
        return -1;
    }
    if (recv_buff[0] != CMD_OK_Restart_POST) {
        ERROR("Receive error. [0x%02x]", recv_buff[0]);
        return -1;
    }
    else {
        INFO("Receive EscMsg_Restart response. [0x%02x]", recv_buff[0]);
    }
    return 0;
}
/************************************************
 * Main function.                               *
 ************************************************/
int main(
        int argc,
        char *argv[])
{
    int action = 0;
    if (argc < 2) {
            printf("usage: crtchange [Action <1 - certificate change> <2 - restart>]\n");
            return 0;
    }
    action = atoi(argv[1]);
    txsocket_id = AsmConnect(TX_SOCKET, DEFAULT_DEV_ADDR);

    switch(action){
    case 1:
        // Do a test to change certificate
        if (0 != certchange()) {
            AsmDisconnect(txsocket_id, 0);
            return -1;
        }
        break;
    case 2:
        if(0 != restart()) {
            AsmDisconnect(txsocket_id, 0);
	        return -1;
        }
        break;
    default:
        printf("Check the usage of the command\n");
        break;
    }
   
    AsmDisconnect(txsocket_id, 0);
    return 0;
}

