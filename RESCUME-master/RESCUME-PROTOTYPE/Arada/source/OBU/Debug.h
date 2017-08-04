#ifndef _DEBUG_H_
#define _DEBUG_H_

#include "TimeStamp.h"

extern const int dbgm_verbose;
extern const int dbgm_info;
extern const int dbgm_warning;
extern const int dbgm_error;

#define DBGM_GEN 0x01 //GENERAL
#define DBGM_GPS 0x02 //GPS
#define DBGM_BT 0x04  //Bluetooth
#define DBGM_UI 0x08  //UI
#define DBGM_CF 0x10  //Config File
#define DBGM_WR 0x20  //WAVE Radio
#define DBGM_RES 0x40 //Responder
#define DBGM_ON 0x80 //Oncoming
#define DBGM_APP 0x100 //Oncoming

extern struct timespec dbg_start_time;
double getTime();

#if defined(NDEBUG)
	#define DBG(_m, _x) ((void)0)
	#define DBG_INFO(_m, _x) ((void)0)
	#define DBG_WARN(_m, _x) ((void)0)
	#define DBG_ERR(_m, _x) ((void)0)
	#define DBG_FATAL(_m) ((void)0)
	#define DBGN(_m, _x) ((void)0)
#else
	#define DBG(_m, _x) (((_m) & dbgm_verbose) ? (printf("%.4f - (%s, %d) | ", getTime(), __FILE__, __LINE__), (void)(_x)) : ((void)0))
	#define DBG_INFO(_m, _x) (((_m) & dbgm_info) ? (printf("%.4f - (%s, %d) [INFO] | ", getTime(), __FILE__, __LINE__), (void)(_x)) : ((void)0))
	#define DBG_WARN(_m, _x) (((_m) & dbgm_warning) ? (printf("%.4f - (%s, %d) [WARNING] | ", getTime(), __FILE__, __LINE__), (void)(_x)) : ((void)0))
	#define DBG_ERR(_m, _x) (((_m) & dbgm_error) ? (printf("%.4f - (%s, %d) [ERROR] | ", getTime(), __FILE__, __LINE__), (void)(_x)) : ((void)0))
	#define DBG_FATAL(_x) (printf("%.4f - (%s, %d) [FATAL] | ", getTime(), __FILE__, __LINE__), (void)(_x), abort())
	#define DBGN(_m, _x) (((_m) & dbgm_verbose) ? ((void)(_x)) : ((void)0))
#endif

#endif

