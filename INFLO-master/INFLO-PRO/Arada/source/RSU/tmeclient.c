/*!
    @file         RSU/tmeclient.c
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

#include <curl/curl.h>
#include <pthread.h>
#include "tmeclient.h"
#include "configuration.h"
#include "tmerequest.h"
#include "ConfigFile.h"

CURL *curl;
pthread_t tme_client_thread;
volatile int enabled = 0;

void _tme_client_run(void* arg)
{
	while(enabled)
	{
		if(curl)
		{
			long active_request_count;
			curl_multi_perform(curl, &active_request_count);
			usleep(50000);
			
		}

		tmerequest_remove_completed();
	}
}

void tme_client_init()
{
	curl_global_init(CURL_GLOBAL_ALL);

	curl = curl_multi_init();
	if(!curl){
		fprintf(stderr, "TME CLIENT [FATAL]: Error creating primary cURL handle\n");
		return;
	}

	tmerequest_global_init();
	
	enabled = 1;
	int status = pthread_create(&tme_client_thread, NULL, _tme_client_run, NULL);
	
	if (status == 0)
		fprintf(stdout, "TME CLIENT: Started thread\n");
	else
	{
		enabled = 0;
		fprintf(stderr, "TME CLIENT [FATAL]: Failed to start thread.\n");
	}
}

void tme_client_post_bsm(char *message)
{
	if(!enabled)
	{
		fprintf(stderr, "TME CLIENT [ERROR]: Error sending Bsm Bundle, client not initialized\n");
		return;
	}
	if(!curl)
	{
		fprintf(stderr, "TME CLIENT [ERROR]: Error sending Bsm Bundle, primary cURL handle not initialized\n");
		return;
	}

	TmeRequest_t *request = tmerequest_init(curl);
	
	if (request == NULL)
	{
		fprintf(stderr, "TME CLIENT [ERROR]: Error sending Bsm Bundle, request handle not initialized\n");
		return;
	}
	request->type = BSM_POST;
	request->callback = NULL;

	strncpy(request->transmit_data, message, TME_MAX_TRANSMIT_SIZE);
	
	request->headers = curl_slist_append(request->headers, "Content-Type: application/json");
	
	curl_easy_setopt(request->curl,CURLOPT_URL, cfGetConfigFile()->bsm_post_url);
	curl_easy_setopt(request->curl,CURLOPT_POSTFIELDS, request->transmit_data);
	curl_easy_setopt(request->curl,CURLOPT_HTTPHEADER, request->headers);
	
	tmerequest_perform(request);
}

void tme_client_post_tim(char *message)
{
	if(!enabled)
	{
		fprintf(stderr, "TME CLIENT [ERROR]: Error sending Tim Bundle, client not initialized\n");
		return;
	}
	if(!curl)
	{
		fprintf(stderr, "TME CLIENT [ERROR]: Error sending Tim Bundle, primary cURL handle not initialized\n");
		return;
	}

	TmeRequest_t *request = tmerequest_init(curl);
	
	if (request == NULL)
	{
		fprintf(stderr, "TME CLIENT [ERROR]: Error sending Tim Bundle, request handle not initialized\n");
		return;
	}
	request->type = TIM_POST;

	strncpy(request->transmit_data, message, TME_MAX_TRANSMIT_SIZE);
	
	request->headers = curl_slist_append(request->headers, "Content-Type: application/json");
	
	curl_easy_setopt(request->curl,CURLOPT_URL, TME_TIM_POST_URL);
	curl_easy_setopt(request->curl,CURLOPT_POSTFIELDS, request->transmit_data);
	curl_easy_setopt(request->curl,CURLOPT_HTTPHEADER, request->headers);
	
	tmerequest_perform(request);
}

void tme_client_request_tim(void(*callback)(void))
{
	if(!enabled)
	{
		fprintf(stderr, "TME CLIENT [ERROR]: Error requesting TIM message, client not initialized\n");
		return;
	}
	if(!curl)
	{
		fprintf(stderr, "TME CLIENT [ERROR]: Error requesting TIM message, primary cURL handle not initialized\n");
		return;
	}

	TmeRequest_t *request = tmerequest_init(curl);
	
	if (request == NULL)
	{
		fprintf(stderr, "TME CLIENT [ERROR]: Error requesting TIM message, request handle not initialized\n");
		return;
	}
	request->type = TIM_GET;
	request->callback = callback;

	request->transmit_data[0] = 0;
	
	request->headers = curl_slist_append(request->headers, "Content-Type: application/json");
	
	curl_easy_setopt(request->curl,CURLOPT_URL, cfGetConfigFile()->tim_get_url);
	curl_easy_setopt(request->curl,CURLOPT_HTTPHEADER, request->headers);
	
	tmerequest_perform(request);
}

void tme_client_destroy()
{
	if (!enabled) return;

	fprintf(stdout, "TME CLIENT: Disabling and joining execution thread\n");
	enabled = 0;
	pthread_join(tme_client_thread, NULL);
	fprintf(stdout, "TME CLIENT: Successfully joined with execution thread\n");

	tmerequest_remove_all();

	if (!curl)
		curl_multi_cleanup(curl);

	fprintf(stdout, "TME CLIENT: Destroyed\n");
}
