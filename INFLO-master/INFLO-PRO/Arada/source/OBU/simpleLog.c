#include "simpleLog.h"

#define GPS_FILENAME	"/var/bin/log/gpsData"
#define EVENT_FILENAME	"/var/bin/log/eventData"

const char *gpsfilename;
const char *eventfilename;

static FILE *g_logfile = NULL;
static FILE *e_logfile = NULL;

#define DATE_SIZE 30
struct timeval timevalue;
struct tm local_time;
char date_string[DATE_SIZE];

long intpart;
long decpart;

void simple_log_init()
{
		// Rotate logs
		int num_backups = 30;
		const int MAX_COMMAND_SIZE = 256;
		char os_command[MAX_COMMAND_SIZE];
		int i;
		// Backup GPS Data
		for (i = num_backups-1; i > 0; i--) {
	   		snprintf(os_command, MAX_COMMAND_SIZE, "mv -f -- '%s.%03d.gz' '%s.%03d.gz' & > /dev/null", GPS_FILENAME, i-1, GPS_FILENAME, i);
	   		system(os_command);
	   		usleep(50000);
		}

		// Zip the most recent one.
	   	snprintf(os_command, MAX_COMMAND_SIZE, "mv -f -- '%s' '%s.%03d' &> /dev/null", GPS_FILENAME, GPS_FILENAME, 0);
		system(os_command);
		usleep(50000);
	   	snprintf(os_command, MAX_COMMAND_SIZE, "gzip -f -- '%s.%03d' &> /dev/null", GPS_FILENAME, 0);
	   	system(os_command);
	   	usleep(50000);
	   	// Backup Event Data
		for (i = num_backups-1; i > 0; i--) {
	   		snprintf(os_command, MAX_COMMAND_SIZE, "mv -f -- '%s.%03d.gz' '%s.%03d.gz' & > /dev/null", EVENT_FILENAME, i-1, EVENT_FILENAME, i);
	   		system(os_command);
	   		usleep(50000);
		}

		// Zip the most recent one.
	   	snprintf(os_command, MAX_COMMAND_SIZE, "mv -f -- '%s' '%s.%03d' &> /dev/null", EVENT_FILENAME, EVENT_FILENAME, 0);
		system(os_command);
		usleep(50000);
	   	snprintf(os_command, MAX_COMMAND_SIZE, "gzip -f -- '%s.%03d' &> /dev/null", EVENT_FILENAME, 0);
	   	system(os_command);
	   	usleep(50000);


	   	g_logfile = fopen(GPS_FILENAME, "w");

		if (g_logfile == NULL) {
			printf(
					"Log file failed to open.  You can still see the "
					"log entries if you have redirected stdout to file.\n");
			return;
		} else {
			printf("Opened log file: %s\n", GPS_FILENAME);
			fprintf(g_logfile, "Time\tLatitude\tLongitude\tHeading\n");
			fclose(g_logfile);
		}

	   	e_logfile = fopen(EVENT_FILENAME, "w");

		if (e_logfile == NULL) {
			printf(
					"Log file failed to open.  You can still see the "
					"log entries if you have redirected stdout to file.\n");
			return;
		} else {
			printf("Opened log file: %s\n", EVENT_FILENAME);
			fprintf(e_logfile, "Time\tLatitude\tLongitude\tHeading\tMessage\n");
			fclose(e_logfile);
		}


}
void simple_log_gps(double time, double	latitude, double longitude, unsigned long heading)
{

	g_logfile = fopen(GPS_FILENAME, "a");

	intpart = (long)time;
	decpart = (long) ((time - intpart) * 1000000);


	timevalue.tv_sec = intpart;
	timevalue.tv_usec = decpart;

	localtime_r(&timevalue.tv_sec, &local_time);
	snprintf (date_string, DATE_SIZE, "%4d-%02d-%02d %02d:%02d:%02d.%03ld",
		local_time.tm_year + 1900,
		local_time.tm_mon + 1,
		local_time.tm_mday,
		local_time.tm_hour,
		local_time.tm_min,
		local_time.tm_sec,
		timevalue.tv_usec / 1000);

//	printf("%s\t%f\t%f\n", date_string, latitude, longitude);

	if (g_logfile) {
		if (fprintf(g_logfile, "%s\t%f\t%f\t%d\n", date_string, latitude, longitude, heading) < 0) {
			;
		}
		fclose(g_logfile);
	}

}
void simple_log_event(double time, double latitude, double longitude, unsigned long heading, const char* eventText)
{

	e_logfile = fopen(EVENT_FILENAME, "a");

	intpart = (long)time;
	decpart = (long) ((time - intpart) * 1000000);

	timevalue.tv_sec = intpart;
	timevalue.tv_usec = decpart;

	localtime_r(&timevalue.tv_sec, &local_time);
	snprintf (date_string, DATE_SIZE, "%4d-%02d-%02d %02d:%02d:%02d.%03ld",
		local_time.tm_year + 1900,
		local_time.tm_mon + 1,
		local_time.tm_mday,
		local_time.tm_hour,
		local_time.tm_min,
		local_time.tm_sec,
		timevalue.tv_usec / 1000);

	printf("%s\t%f\t%f\t%d\t%s\n", date_string, latitude, longitude, heading, eventText);

	if (e_logfile) {
		if (fprintf(e_logfile, "%s\t%f\t%f\t%d\t%s\n", date_string, latitude, longitude, heading, eventText) < 0) {
			;
		}
		fclose(e_logfile);
	}
}
void simple_log_text(double time, const char* eventText)
{

	e_logfile = fopen(EVENT_FILENAME, "a");

	intpart = (long)time;
	decpart = (long) ((time - intpart) * 1000000);

	timevalue.tv_sec = intpart;
	timevalue.tv_usec = decpart;

	localtime_r(&timevalue.tv_sec, &local_time);
	snprintf (date_string, DATE_SIZE, "%4d-%02d-%02d %02d:%02d:%02d.%03ld",
		local_time.tm_year + 1900,
		local_time.tm_mon + 1,
		local_time.tm_mday,
		local_time.tm_hour,
		local_time.tm_min,
		local_time.tm_sec,
		timevalue.tv_usec / 1000);

	printf("%s\t%s\n", date_string, eventText);

	if (e_logfile) {
		if (fprintf(e_logfile, "%s\t%s\n", date_string, eventText) < 0) {
			;
		}
		fclose(e_logfile);
	}
}
void simple_log_close()
{
	if (g_logfile)
		fclose(g_logfile);
	if (e_logfile)
		fclose(e_logfile);
}

/**
 * Get current time in ms
 */
Time TimerNow() {
	Time Now;
	struct timeval t;

	if (gettimeofday(&t, NULL) != 0) {
		printf("Get time of day failed\n");
		return 0;
	}

	Now = t.tv_sec *1000L + (t.tv_usec / 1000L);

	return Now;
}


