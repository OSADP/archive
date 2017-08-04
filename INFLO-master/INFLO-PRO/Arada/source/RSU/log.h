#ifndef LOG_H
#define LOG_H

#include <stdarg.h>
#include <stdio.h>
#include <stdbool.h>

/*
Log levels.  LOG_ERROR means that an unrecoverable error
has occurred.  LOG_WARNING means that an unexpected
event has occurred.  LOG_INFO is for general information
to let the user see what is going on.  LOG_DETAIL
is for implementation details.
*/
enum {LOG_ERROR=1, LOG_WARNING, LOG_INFO, LOG_DETAIL};
/*
Log classes.  This enables the user to enable or disable
logging for specific functions.  Note identifiers are
for non-app types (will always be linked in the binary).
App types are assumed to be initially disabled and stored
in another list.
To add a group (that is not an app in the factory):<ol>
<li>Add entry here</li>
<li>Adjust initialization of g_log_groups in log.c</li>
<li>Add a string for the group in the log_entry function</li>
<li>Update parser in debug_server.c to accept the string</li>
<li>Update help text in debug_server.c to include the group</li></ol>
*/
enum {LOG_SENSOR_SERVER, LOG_LOG, LOG_DEBUG_SERVER,
	LOG_SERVER, LOG_SETTING, LOG_APP_MAN, LOG_UDP_SERVER,
	LOG_SERIAL, LOG_ARCHIVE, LOG_META, LOG_COUNT};
		
void log_entry (unsigned short level, int group,
		const char* file, unsigned int line, 
		const char* format_string, ...);
void log_entry_no_network(unsigned short level, int group,
		const char* file, unsigned int line, 
		const char* format_string, ...);
// Please be careful if you use any of the function below.  
// They are not thread-safe, but as currently used (in constructor
// and then only by the debug_server) there are no issues.
// To make thread-safe, you will need to create a mutex,
// but it will have to be init'ed with a constructors that is
// higher priority than the other constructors
void log_enable_group(int group);
void log_disable_group (int group);
void log_set_level(unsigned short level);
void log_close();
void log_init(const char *filename);

void checkDiskUsage(void);

#define True 1
#define False 0
#define MOUNT_PATH			"/tmp/uda1"
#define ARCHIVE_LOG_PATH	"/tmp/usbmnt/logs_archive/"
#define LOGFILE_PATH		"/tmp/usbmnt/logs/"
#define CURRENT_LOG_FILE 	"/tmp/usbmnt/logs/vsm.log"
#define DEFAULT_BACKUP_NAME "/tmp/usbmnt/logs/vsm_back.log"


#endif
