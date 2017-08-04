#ifndef TIMRECEIVER_H_
#define TIMRECEIVER_H_

#include "stdint.h"

void tim_setdata(char *data);
int tim_copymessage(uint8_t *dest, uint16_t max_length);

#endif