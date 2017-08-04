#include "timreceiver.h"
#include "stdio.h"
#include "configuration.h"


uint8_t _tim_data[TIM_MAX_DATA_LENGTH];
volatile int _tim_length = 0;
pthread_mutex_t tim_lock = PTHREAD_MUTEX_INITIALIZER;

void tim_setdata(char *data)
{
	pthread_mutex_lock(&tim_lock);

	char *start = strstr(data, "\"payload\":\"");
	if (start == NULL)
	{
		return;
	}

	start += 11;

	char *end = strstr(start, "\"");
	if (end == NULL)
	{
		pthread_mutex_unlock(&tim_lock);
		return;
	}

	_tim_length = (end - start)/2;

	int i;
	for(i = 0; i < _tim_length; i++)
	{
		sscanf(&start[i * 2], "%2hhX", &_tim_data[i]);
	}


	fprintf(stdout, "TME RECEIVER: Received new TIM data: ");
	for(i = 0; i < _tim_length; i++)
	{
		printf("%02X",_tim_data[i]);
	}
	printf("\n");

	pthread_mutex_unlock(&tim_lock);
}

int tim_copymessage(uint8_t *dest, uint16_t max_length)
{
	pthread_mutex_lock(&tim_lock);

	int length = _tim_length;

	if (length > max_length)
	{
		memset(dest, 0, max_length);
		length = 0;
	}
	else
	{
		memcpy(dest, _tim_data, length);
	}

	pthread_mutex_unlock(&tim_lock);
	return length;
}
