
#include "AsmEncryptor.h"

#include <stdlib.h>

#include "genericAPI.h"
#include "Debug.h"

int asmEncryptor_init(AsmEncryptor *encryptor)
{
	if (encryptor->socket)
		return 0;

	int results = AsmConnect(TX_SOCKET, DEFAULT_DEV_ADDR);

	if (results > 0)
		encryptor->socket = results;
	else
		encryptor->socket = 0;

	return results != 0;
}

void asmEncryptor_destroy(AsmEncryptor *encryptor)
{
	if (!encryptor->socket)
		return;

	AsmDisconnect(encryptor->socket, 0);
}

int asmEncryptor_encrypt(AsmEncryptor *encryptor, WSMRequest *wsmreq)
{
	if (!encryptor->socket)
		return -1;

	uint8_t sendBuff[1024];
	uint8_t recvBuff[1024];

	int sendSize = 0, recvSize = 0;

	//Build AsmMsg_Enc Request
	bzero(sendBuff, sizeof(sendBuff));
	msg_create_enc_msg(sendBuff, (uint8_t *)wsmreq->data.contents, &sendSize, wsmreq->data.length);

	do
	{
		INFO("Send AsmMsg_Enc request. [0x%02x]", sendBuff[0]);
		if (AsmSend((char *) sendBuff, sendSize, encryptor->socket))
		{
			return -1;
		}

		//Receive AsmMsg_Enc response
		bzero(recvBuff, sizeof(recvBuff));
		recvSize = AsmRecv((char*) recvBuff, sizeof(recvBuff), encryptor->socket);
		if (recvSize <= 0)
		{
			return -1;
		}

		if (recvBuff[0] != CMD_OK_ENC_POST && (recvBuff[0] != CMD_LCM_STATUS_RDY && recvBuff[0] != CMD_LCM_STATUS_CERT_CHANGED))
		{
			ERROR("Receive error. [0x%02x]", recvBuff[0]);
			return -1;
		}
		else
		{
			INFO("Receive AsmMsg_Enc response. [0x%02x]", recvBuff[0]);
		}
	} while (recvBuff[0] == CMD_LCM_STATUS_RDY || recvBuff[0] == CMD_LCM_STATUS_CERT_CHANGED);


	bzero(sendBuff, sizeof(sendBuff));
	msg_extract_Enc_OTA(sendBuff, recvBuff, &recvSize);

	memcpy(&wsmreq->data.contents, &sendBuff, recvSize);
	wsmreq->data.length = recvSize;

	wsmreq->security = AsmEncrypt;
	printf("asmEncryptor_encrypt return: %02x\n\n", recvBuff[0]);

	return recvBuff[0];
}

