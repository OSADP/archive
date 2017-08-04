#include <stdio.h>
#include <string.h> 
#include <time.h>

#include <stdlib.h>

#include "log.h"
#include "network.h"
//#include "debug_server.h"
//#include "app_man.h"

static unsigned short log_level = LOG_DETAIL; /* Default log level */
/* Array of initial states for the logging groups. */
static bool log_groups[LOG_COUNT] = {
 true, /* LOG_SENSOR_SERVER */ 
 true, /* LOG_LOG */
 true, /* LOG_DEBUG_SERVER */
 true, /* LOG_SERVER */
 true, /* LOG_SETTING */
 true, /* LOG_APP_MAN */
 true, /* LOG_UDP_SERVER */
 true, /* LOG_SERIAL */
 true, /* LOG_ARCHIVE */
 true, /* LOG_META */
};

/* List of app types that have logging enabled */
static int log_app_type_enabled[MAX_NUM_APPS] = {0};
//static FILE *g_logfile = NULL;

/*
Set below to true for the log to only log the group and the line number
(this will make the log quicker in serial poll mode, because less to
output)
*/
static bool quick_log = false;
static bool no_file_or_line = false;

/* size of date string in log. */
#define DATE_SIZE 30 

/**
 * Enables a logging group.  Enabling a logging group that is
 * already enabled is NOT an error.
 */
void log_enable_group(int group) 
{
	if (group <= LOG_COUNT) {
		/* Then in non-app type. */
		log_groups[group] = true;
		return;
	} else {
		/* First verify it is not already in the list*/
		int i;
		// While looking at the list, might as well look
		// for a place to put it if it is not already there
		int candidate_index = -1;
		for (i = 0; i < MAX_NUM_APPS; i++) {
			if (log_app_type_enabled[i] == group)
				return;
			if (log_app_type_enabled[i] == 0 &&
				candidate_index == -1)
				candidate_index = i;
		}
		// If still here, then not already in the list
		// Check that a spot was found
		if (candidate_index == -1) {
			log_entry(LOG_WARNING, LOG_LOG, __FILE__, __LINE__, 
					"log_app_type_enabled list is full in enable_log. "
					"Group could not be enabled.");
			return;
		}

		// else save the group
		log_app_type_enabled[candidate_index] = group;
	}  
}

/**
 * Sets the logging level.  This can be changed at any time
 */
void log_set_level(unsigned short level)
{
	if (level > LOG_DETAIL || level < LOG_ERROR) {
		log_entry(LOG_WARNING, LOG_LOG, __FILE__, __LINE__, 
				"logSetLevel received invalid level %d - not used", level);
		return;
	}		
	log_level = level;
}

/**
 * Call to disable logging of a specify group.  
 */
void log_disable_group (int group)
{
	if (group <= LOG_COUNT) {
		/* Then in non-app type. */
		log_groups[group] = false;
		return;
	} else {
		/* Remove app from enabled list. */
		int i;
		for (i = 0; i < MAX_NUM_APPS; i++) {
			if (log_app_type_enabled[i] == group) {
				log_app_type_enabled[i] = 0;
				return;
			}
		}
		/* It's not an error if the group is already disabled. */
	}
}

/**
 * Returns if the group is enabled for logging
 */
bool group_enabled (int group)
{
	if (group <= LOG_COUNT) {
		/* Then in non-app type. */
		return log_groups[group];
	} else {
		/* Check app list. */
		int i;
		for (i = 0; i < MAX_NUM_APPS; i++) {
			if (log_app_type_enabled[i] == group) {
				return true;
			}
		}

		/* If get here, then not in list, so not enabled. */
		return false;
	}
	return false;
}

static const int GROUP_NAME_SIZE = 24;
static const int HEADING_SIZE = 24 + 32; /* The first number should match GROUP_NAME_SIZE.  Extra tries to make
					       space for optional filename, line number, log level title, and 
					       spaces between them.  If things start to get truncated (possibly from too
					       long of a file name, increase the extra). */
/**
* The class implements logging capability
*  All messages for ERROR and WARN level are printed.
*  INFO and DETAIL messages are only printed if the group
*  is enabled
*/
void log_entry_valist(unsigned short level, int group,
		const char* file, unsigned int line, 
		bool broadcast,
		const char* format_string, va_list pvar)
{
#ifndef NDEBUG
	if (level <= log_level) {
		//va_list pvar;
		bool enabled_group = false;
		char group_name[GROUP_NAME_SIZE];
		char log_heading[HEADING_SIZE];
	 
		enabled_group = group_enabled(group);
		
		/* Put the group name in a string. */
		switch(group) {
			case LOG_LOG:
				snprintf(group_name, GROUP_NAME_SIZE, "Log");
				break;
			case LOG_SENSOR_SERVER:
				snprintf(group_name, GROUP_NAME_SIZE, "SensorServer");
				break;
			case LOG_DEBUG_SERVER:
				snprintf(group_name, GROUP_NAME_SIZE, "DebugServer");
				break;
			case LOG_SERVER:
				snprintf(group_name, GROUP_NAME_SIZE, "Server");
				break;
			case LOG_SETTING:
				snprintf(group_name, GROUP_NAME_SIZE, "Setting");
				break;
			case LOG_APP_MAN:
				snprintf(group_name, GROUP_NAME_SIZE, "AppMan");
				break;
			case LOG_UDP_SERVER:
				snprintf(group_name, GROUP_NAME_SIZE, "UdpServer");
				break;
			case LOG_SERIAL:
				snprintf(group_name, GROUP_NAME_SIZE, "Serial");
				break;
			case LOG_ARCHIVE:
				snprintf(group_name, GROUP_NAME_SIZE, "Archive");
				break;
			case LOG_META:
				snprintf(group_name, GROUP_NAME_SIZE, "Meta");
				break;
			default:
				/* Then try to find app name */
				//get_app_type_from_log_group(group, group_name, GROUP_NAME_SIZE);
				break;
		}
		
		/* Print the heading. */
		log_heading[0] = 0;
		switch(level) {
			case LOG_ERROR:
				if (quick_log)
					snprintf (log_heading, HEADING_SIZE, "E: %s %d\r\n", group_name, line);
				else if (no_file_or_line)
					snprintf (log_heading, HEADING_SIZE, "E: %s - ", group_name);
				else
					snprintf (log_heading, HEADING_SIZE, "Error: %s (%d) %s - ", file, line, group_name);					 
				break;
			case LOG_WARNING:
				if (quick_log)
					snprintf (log_heading, HEADING_SIZE, "W: %s %d\r\n", group_name, line);
				else if (no_file_or_line)
					snprintf (log_heading, HEADING_SIZE, "W: %s - ", group_name);
				else
					snprintf (log_heading, HEADING_SIZE, "Warning: %s (%d) %s - ", file, line, group_name);					 
				break;
			case LOG_INFO:
				if (enabled_group) {
					if (quick_log)
						snprintf (log_heading, HEADING_SIZE, "I: %s %d\r\n", group_name, line);
					else if (no_file_or_line)
						snprintf (log_heading, HEADING_SIZE, "I: %s - ", group_name);
					else
						snprintf (log_heading, HEADING_SIZE, "Info: %s (%d) %s - ", file, line, group_name);						
				}
				break;
			case LOG_DETAIL:
				if (enabled_group) {
					if (quick_log)
						snprintf (log_heading, HEADING_SIZE, "D: %s %d\r\n", group_name, line);
					else if (no_file_or_line)
						snprintf (log_heading, HEADING_SIZE, "D: %s - ", group_name);
					else
						snprintf (log_heading, HEADING_SIZE, "Detail: %s (%d) %s - ", file, line, group_name);						
				}								 
				break;
			default:
				/* Unexpected state. */
				log_entry(LOG_WARNING, LOG_LOG, __FILE__, __LINE__, 
						"logEnableGroup received invalid group %d - not used", group);
				break;
		} 
		
		if ((level == LOG_ERROR || level == LOG_WARNING || enabled_group)) {		 
			/* Send to any connected debug sockets. */
			const unsigned int DATA_SIZE = 2048;
			char* log_data = malloc(sizeof(char)*DATA_SIZE);
			unsigned int num_chars_vsnprintf_wants_in_string;
			char date_string[DATE_SIZE];
			struct timeval timevalue;
			struct tm local_time;
			const int LOG_STRING_SIZE = HEADING_SIZE + DATE_SIZE + DATA_SIZE + 6; /* extra
				to take into spacing, etc. */
			char *log_string = malloc(sizeof(char) * LOG_STRING_SIZE);

			num_chars_vsnprintf_wants_in_string = vsnprintf(log_data, DATA_SIZE, format_string, pvar);
			if (num_chars_vsnprintf_wants_in_string >= DATA_SIZE) {
				/* This string must fit in DATA_SIZE, or we will get stuck in a recursive loop! */
				log_entry(LOG_WARNING, LOG_LOG, __FILE__, __LINE__, 
				 "The log dat string has been truncated to fit into the allocated buffer.");
			}

			gettimeofday(&timevalue,NULL);
			localtime_r(&timevalue.tv_sec, &local_time);		
			snprintf (date_string, DATE_SIZE, "%4d-%02d-%02d %02d:%02d:%02d.%03d", 
				local_time.tm_year + 1900, 
				local_time.tm_mon + 1, 
				local_time.tm_mday, 
				local_time.tm_hour, 
				local_time.tm_min, 
				(int) local_time.tm_sec,
				(int) timevalue.tv_usec / 1000);

			snprintf(log_string, LOG_STRING_SIZE, "%s - %s%s\n", date_string, log_heading, log_data);

			printf("%s", log_string);
			free(log_string);
			free(log_data);
		}
	}
#endif
}

void log_entry(unsigned short level, int group,
		const char* file, unsigned int line, 
		const char* format_string, ...)
{
	va_list pvar;
	va_start(pvar, format_string);
	log_entry_valist(level, group, file, line, true, format_string, pvar);
	va_end(pvar); 
}

void log_entry_no_network(unsigned short level, int group,
		const char* file, unsigned int line, 
		const char* format_string, ...)
{
	va_list pvar;
	va_start(pvar, format_string);
	log_entry_valist(level, group, file, line, false, format_string, pvar);
	va_end(pvar); 
}


void log_init(const char *filename)
{
	// Not implemented
}


void backupCurrentLogfile(char *filename)
{
	// Not implemented
}


/**
 * Call to close open logfile.  (Not an error to call with no open log file).
 * The purpose is just make sure the file is flushed.
 */
void log_close()
{
	// Not implemented
}

void checkDiskUsage(void)
{
	#define MAX_COMMAND_SIZE 255
	char os_command[MAX_COMMAND_SIZE];
	FILE *read_fp;
	char data[10];
	int chars_read;
	int percentage = 0;
	snprintf(os_command, MAX_COMMAND_SIZE,"df | awk '{if($1 == \"%s\")print $5}'",MOUNT_PATH);
	printf("%s\n",os_command);
   	read_fp = popen(os_command,"r");
	if(read_fp != NULL)
	{
		chars_read = fread(data,sizeof(char),10,read_fp);
		if(chars_read > 0)
		{
			printf("space used = %s\n",data);
			percentage = strtol(data,NULL,10);
		}
		else
			printf("no chard read\n");
	}
	else
		printf("failed to open pipe\n");
	if(percentage > 90)
	{
		sprintf(os_command,"%s;0;%s;%s;Disk space low %d %% used\n","HPU0","EVENT","ADVISORY",percentage);	
		sendMessageToAllClients(os_command);
	}
}
