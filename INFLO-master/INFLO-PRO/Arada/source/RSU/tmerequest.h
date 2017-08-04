/*!
    @file         RSU/tmerequest.h
    @author       Joshua Branch

    @copyright
    Copyright (c) 2013 Battelle Memorial Institute. All rights reserved.

    @par
    Unauthorized use or duplication may violate state, federal and/or
    international laws including the Copyright Laws of the United States
    and of other international jurisdictions.

    @par
    @verbatim
    Battelle Memorial Institute
    505 King Avenue
    Columbus, Ohio  43201
    @endverbatim

    @brief
    TBD

    @details
    TBD
*/


#ifndef	_TMEREQUEST_H_
#define	_TMEREQUEST_H_

#include "configuration.h"

typedef enum { BSM_POST, TIM_GET, TIM_POST } TmeRequestType;

typedef struct {
	TmeRequestType type;
	unsigned long request_id;

	CURL *curl;
	struct curl_slist *headers;
	char transmit_data[TME_MAX_TRANSMIT_SIZE + 1];
    char receive_data[TME_MAX_RECEIVE_SIZE + 1];

    void (*callback)(char *);

	CURL *curl_multi;
} TmeRequest_t;

int tmerequest_global_init();
int tmerequest_find_request_slot();
TmeRequest_t *tmerequest_init(CURL* curl_multi);
void tmerequest_free(TmeRequest_t *request);
void tmerequest_strcat(TmeRequest_t *request, char *buf, size_t max);
void tmerequest_remove(TmeRequest_t *request);
int tmerequest_get_status_code(TmeRequest_t *request);
double tmerequest_get_elapsed_time(TmeRequest_t *request);
char *tmerequest_get_url(TmeRequest_t *request);
void tmerequest_remove_completed();
void tmerequest_perform(TmeRequest_t *request);
void tmerequest_remove_all();
size_t tmerequest_receive_data(void *buffer, size_t size, size_t nmemb, void* userptr);
void tmerequest_log(TmeRequest_t *request);


#endif
