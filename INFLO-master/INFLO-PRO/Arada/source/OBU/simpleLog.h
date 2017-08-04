#ifndef SIMPLE_LOG_H
#define SIMPLE_LOG_H

#include <sys/time.h>
#include <stdio.h>
#include <string.h>
#include <time.h>

#include <stdlib.h>

typedef signed long int Time;
Time TimerNow();



void simple_log_init();
void simple_log_gps(double time, double	latitude, double longitude, unsigned long heading);
void simple_log_event(double time, double latitude, double longitude, unsigned long heading , const char* eventText);
void simple_log_text(double time, const char* eventText);
void simple_log_close();

#endif
