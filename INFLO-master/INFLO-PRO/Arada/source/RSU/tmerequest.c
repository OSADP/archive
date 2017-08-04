/*!
    @file         RSU/tmerequest.c
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
#include <unistd.h>
#include "tmerequest.h"

#ifndef min
#define min(a,b) (((a)<(b))?(a):(b))
#define max(a,b) (((a)>(b))?(a):(b))
#endif

pthread_mutex_t active_requests_lock = PTHREAD_MUTEX_INITIALIZER;
TmeRequest_t *active_requests[TME_MAX_CONCURRENT_COUNT];
unsigned long next_request_id = 1;

#define LOG_FILENAME_MAX_LENGTH 250
char log_filename[LOG_FILENAME_MAX_LENGTH];

/*!
 *
 *
 */
int tmerequest_global_init()
{
	int i;
	for(i = 0; i < TME_MAX_CONCURRENT_COUNT; i++)
		active_requests[i] = NULL;

	// Rotate logs
	const int MAX_COMMAND_SIZE = 256;
	char os_command[MAX_COMMAND_SIZE];
   	snprintf(os_command, MAX_COMMAND_SIZE, "mkdir -p %s", TME_LOG_LOCATION);
	system(os_command);
	usleep(100*1000);

	time_t rawtime;
	time(&rawtime);
	struct tm *utctime = gmtime(&rawtime);
	strftime(os_command, MAX_COMMAND_SIZE, TME_LOG_FILENAME, utctime);

	strncpy(log_filename, TME_LOG_LOCATION, LOG_FILENAME_MAX_LENGTH);
	strncat(log_filename, "/", LOG_FILENAME_MAX_LENGTH);
	strncat(log_filename, os_command, LOG_FILENAME_MAX_LENGTH);

   	int logfile = fopen(log_filename, "w");

	if (logfile == NULL) {
		fprintf(stderr, "TME REQUEST [ERROR]: Failed to open log file %s\n", log_filename);
		return;
	} else {
		fprintf(stdout, "TME REQUEST: Started log file %s\n", log_filename);
		fprintf(logfile, "Timestamp\tType\tRequestID\tURL\tHttp Status\tElapsed Time(seconds)\tTransmit Data\tReceive Data\n");
		fclose(logfile);
	}
}

/*!
 *
 *
 */
TmeRequest_t *tmerequest_init(CURL* curl_multi)
{
	pthread_mutex_lock(&active_requests_lock);
	int request_slot = tmerequest_find_request_slot();

	TmeRequest_t *request = NULL;
	if (request_slot != -1)
	{
		request = (TmeRequest_t *)malloc(sizeof(TmeRequest_t));
		if (request != NULL)
		{
			request->transmit_data[0] = '\0';
			request->receive_data[0] = '\0';
			request->request_id = next_request_id++;
			request->curl_multi = curl_multi;
			request->headers = NULL;
			request->curl = curl_easy_init();
			if (request->curl == NULL)
			{
#ifdef TME_DEBUG
				fprintf(stderr, "TME REQUEST-DEBUG [ERROR]: tmerequest_init() -> Failed to initialize curl easy handle\n");
#endif
				free(request);
				request = NULL;
			}
			else
			{
				curl_easy_setopt(request->curl, CURLOPT_WRITEFUNCTION, tmerequest_receive_data);
				curl_easy_setopt(request->curl, CURLOPT_WRITEDATA, request);
#ifdef TME_DEBUG
				//curl_easy_setopt(request->curl, CURLOPT_VERBOSE, 1);
#endif
			}
		}
#ifdef TME_DEBUG
		else
			fprintf(stderr, "TME REQUEST-DEBUG [ERROR]: tmerequest_init() -> Failed to malloc new TmeRequest_t\n");
#endif

	}
	else
	{
#ifdef TME_DEBUG 
		fprintf(stderr, "TME REQUEST-DEBUG [ERROR]: tmerequest_init() -> Unable to locate an available Request Slot. Currently at max number of concurrent requests.\n");
#endif
	}


#ifdef TME_DEBUG 
	if (request != NULL)
		fprintf(stdout, "TME REQUEST-DEBUG: tmerequest_init() -> Initialized request %d at slot #: %d\n", request->request_id, request_slot);
	else
		fprintf(stderr, "TME REQUEST-DEBUG [ERROR]: tmerequest_init() -> Error initializing new request\n");
#endif

	if (request_slot != -1)
		active_requests[request_slot] = request;
	
	pthread_mutex_unlock(&active_requests_lock);
	return request;
}

/*!
 *
 *
 */
void tmerequest_free(TmeRequest_t *request)
{
	if (request == NULL) return;

	curl_slist_free_all(request->headers);

	if(request->curl_multi != NULL)
		curl_multi_remove_handle(request->curl_multi, request->curl);
	curl_easy_cleanup(request->curl);
	
	unsigned long id = request->request_id;
	free(request);

#ifdef TME_DEBUG 
	fprintf(stdout, "TME REQUEST-DEBUG: tmerequest_free() -> Successfully free'd request %d.\n", id);
#endif
}

size_t tmerequest_receive_data(void *buffer, size_t size, size_t nmemb, void* userptr)
{
	if (userptr == NULL)
		return;

	TmeRequest_t *request = (TmeRequest_t*)userptr;

	size_t length = size * nmemb;
	size_t currentLength = strlen(request->receive_data);
	size_t effectiveLength = min(TME_MAX_RECEIVE_SIZE - currentLength, length);
	memcpy(request->receive_data + currentLength, buffer, effectiveLength); 
	request->receive_data[effectiveLength + currentLength] = '\0';
	
	if (request->callback != NULL)
		request->callback(request->receive_data);
	//TODO: Data Received Callback? -> or doo that when destroying after knowning finishing code (latter)

	return length;
}

/*!
 *
 *
 */
void tmerequest_strcat(TmeRequest_t *request, char *buf, size_t max)
{
	int status = tmerequest_get_status_code(request);
	double elapsed_time = tmerequest_get_elapsed_time(request);
	char *url = tmerequest_get_url(request);

	switch(request->type)
	{
		case BSM_POST:
			snprintf(buf, max, "%s%s\n",   buf, "***********  TME REQUEST  ***********");
			snprintf(buf, max, "%s%s%d\n", buf, "    Request ID: ", request->request_id);
			snprintf(buf, max, "%s%s\n",   buf, "          Type: BSM POST");
			if (url != NULL)
			snprintf(buf, max, "%s%s%s\n", buf, "           URL: ", url);
			snprintf(buf, max, "%s%s%d\n", buf, "   Http Status: ", status);
			snprintf(buf, max, "%s%s%f\n", buf, "  Elapsed Time: ", elapsed_time);
			snprintf(buf, max, "%s%s%s\n", buf, " Received Data: ", request->receive_data);
			snprintf(buf, max, "%s%s\n\n", buf, "*************************************");
			break;
		case TIM_POST:
			snprintf(buf, max, "%s%s\n",   buf, "***********  TME REQUEST  ***********");
			snprintf(buf, max, "%s%s%d\n", buf, "    Request ID: ", request->request_id);
			snprintf(buf, max, "%s%s\n",   buf, "          Type: TIM POST");
			if (url != NULL)
			snprintf(buf, max, "%s%s%s\n", buf, "           URL: ", url);
			snprintf(buf, max, "%s%s%d\n", buf, "   Http Status: ", status);
			snprintf(buf, max, "%s%s%f\n", buf, "  Elapsed Time: ", elapsed_time);
			snprintf(buf, max, "%s%s%s\n", buf, " Received Data: ", request->receive_data);
			snprintf(buf, max, "%s%s\n\n", buf, "*************************************");
			break;
		case TIM_GET:
			snprintf(buf, max, "%s%s\n",   buf, "***********  TME REQUEST  ***********");
			snprintf(buf, max, "%s%s%d\n", buf, "    Request ID: ", request->request_id);
			snprintf(buf, max, "%s%s\n",   buf, "          Type: TIM GET");
			if (url != NULL)
			snprintf(buf, max, "%s%s%s\n", buf, "           URL: ", url);
			snprintf(buf, max, "%s%s%d\n", buf, "   Http Status: ", status);
			snprintf(buf, max, "%s%s%f\n", buf, "  Elapsed Time: ", elapsed_time);
			snprintf(buf, max, "%s%s%s\n", buf, " Received Data: ", request->receive_data);
			snprintf(buf, max, "%s%s\n\n", buf, "*************************************");
			break;
	}

}

/*!
 *
 *
 */
void tmerequest_remove(TmeRequest_t *request)
{
	if (request == NULL) return;

	pthread_mutex_lock(&active_requests_lock);
	int i;
	for(i = 0; i < TME_MAX_CONCURRENT_COUNT; i++)
	{
		if (active_requests[i] == request)
		{
#ifdef TME_DEBUG 
			const int MAX_MSG_SIZE = 2000;
			char buf[MAX_MSG_SIZE];
			snprintf(buf, MAX_MSG_SIZE, "TME REQUEST-DEBUG: tmerequest_remove() -> Removed request %d from slot #: %d\n\n", request->request_id, i);
			tmerequest_strcat(active_requests[i], buf, MAX_MSG_SIZE);
			fprintf(stdout, "%s", buf);
#endif
			active_requests[i] = NULL;
		}
	}
	tmerequest_log(request);
	tmerequest_free(request);
	pthread_mutex_unlock(&active_requests_lock);
}

/*!
 *
 *
 */
int tmerequest_get_status_code(TmeRequest_t *request)
{
	if (request == NULL || request->curl == NULL) return -1;

	long response;
	curl_easy_getinfo(request->curl, CURLINFO_RESPONSE_CODE, &response);
	return (int)response;
}

/*!
 *
 *
 */
double tmerequest_get_elapsed_time(TmeRequest_t *request)
{
	if (request == NULL || request->curl == NULL) return -1;

	double response;
	if (CURLE_OK == curl_easy_getinfo(request->curl, CURLINFO_TOTAL_TIME, &response))
		return response;
	else
		return -1;
}

/*!
 *
 *
 */
char *tmerequest_get_url(TmeRequest_t *request)
{
	if (request == NULL || request->curl == NULL) return NULL;

	char* response;
	if (CURLE_OK == curl_easy_getinfo(request->curl, CURLINFO_EFFECTIVE_URL, &response))
		return response;
	else
		return NULL;
}

/*!
 *
 *
 */
int tmerequest_find_request_slot()
{
	int i, request_slot = -1;
	for(i = 0; i < TME_MAX_CONCURRENT_COUNT; i++)
	{
		if (active_requests[i] == NULL)
		{
			request_slot = i;
			break;
		}
	}

	return request_slot;
}

/*!
 *
 *
 */
void tmerequest_remove_completed()
{
	int i;
	for(i = 0; i < TME_MAX_CONCURRENT_COUNT; i++)
	{
		if (active_requests[i] == NULL) continue;
		
		int statuscode = tmerequest_get_status_code(active_requests[i]);
		if (statuscode >= 200 || tmerequest_get_elapsed_time(active_requests[i]) > 10.0)
		{
			fprintf(stdout, "TME REQUEST: Removing request %d with response code: %d\n", active_requests[i]->request_id, statuscode);
			tmerequest_remove(active_requests[i]);
		}
	}
}

/*!
 *
 *
 */
void tmerequest_remove_all()
{
	int i;
	for(i = 0; i < TME_MAX_CONCURRENT_COUNT; i++)
	{
		tmerequest_remove(active_requests[i]);
	}
}

/*!
 *
 *
 */
void tmerequest_perform(TmeRequest_t *request)
{
	if (request->curl_multi != NULL)
		curl_multi_add_handle(request->curl_multi, request->curl);
	else
		curl_easy_perform(request->curl);
}


char log_buf[TME_LOG_LINE_LENGTH_MAX + 1];
/*!
 *
 *
 */
void tmerequest_log(TmeRequest_t *request)
{
	int length = 0;

	int status = tmerequest_get_status_code(request);
	double elapsed_time = tmerequest_get_elapsed_time(request);
	char *url = tmerequest_get_url(request);

	time_t rawtime;
	time(&rawtime);
	struct tm *utctime = gmtime(&rawtime);

	length += strftime(log_buf + length, TME_LOG_LINE_LENGTH_MAX-length, "%FT%Xz", utctime);

	switch(request->type)
	{
		case BSM_POST:
			length += snprintf(log_buf + length, TME_LOG_LINE_LENGTH_MAX-length, "\tBSM POST");
			break;
		case TIM_POST:
			length += snprintf(log_buf + length, TME_LOG_LINE_LENGTH_MAX-length, ",\"TIM POST\"");
			break;
		default:
			length += snprintf(log_buf + length, TME_LOG_LINE_LENGTH_MAX-length, "\tunknown");

	}
	length += snprintf(log_buf + length, TME_LOG_LINE_LENGTH_MAX-length, "\t%d", request->request_id);
	length += snprintf(log_buf + length, TME_LOG_LINE_LENGTH_MAX-length, "\t%s", url);
	length += snprintf(log_buf + length, TME_LOG_LINE_LENGTH_MAX-length, "\t%d", status);
	length += snprintf(log_buf + length, TME_LOG_LINE_LENGTH_MAX-length, "\t%f", elapsed_time);
	length += snprintf(log_buf + length, TME_LOG_LINE_LENGTH_MAX-length, "\t%s", request->transmit_data);
	length += snprintf(log_buf + length, TME_LOG_LINE_LENGTH_MAX-length, "\t%s", request->receive_data);


	char *nextReplace = strstr(log_buf, "\n");
	while(nextReplace != NULL)
	{
		*nextReplace = ' ';
		nextReplace = strstr(nextReplace, "\n");
	}

	strncat(log_buf, "\n", TME_LOG_LINE_LENGTH_MAX);

	int logfile = fopen(log_filename, "a");
	if (logfile == NULL) {
		fprintf(stderr, "TME REQUEST [ERROR]: Failed to open log file %s\n", log_filename);
		fprintf(stdout, log_buf);
		return;
	} else {
		fprintf(logfile, log_buf);
		fclose(logfile);
	}
}


