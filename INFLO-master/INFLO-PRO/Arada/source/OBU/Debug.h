#ifndef _DEBUG_H_
#define _DEBUG_H_

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

#if defined(NDEBUG)
	#define DBG(_m, _x) ((void)0)
	#define DBG_INFO(_m, _x) ((void)0)
	#define DBG_WARN(_m, _x) ((void)0)
	#define DBG_ERR(_m, _x) ((void)0)
	#define DBG_FATAL(_m) ((void)0)
	#define DBGN(_m, _x) ((void)0)
#else
	#define DBG(_m, _x) (((_m) & dbgm_verbose) ? (printf("(%s, %d) | ", __FILE__, __LINE__), (void)(_x)) : ((void)0))
	#define DBG_INFO(_m, _x) (((_m) & dbgm_info) ? (printf("(%s, %d) [INFO] | ", __FILE__, __LINE__), (void)(_x)) : ((void)0))
	#define DBG_WARN(_m, _x) (((_m) & dbgm_warning) ? (printf("(%s, %d) [WARNING] | ", __FILE__, __LINE__), (void)(_x)) : ((void)0))
	#define DBG_ERR(_m, _x) (((_m) & dbgm_error) ? (printf("(%s, %d) [ERROR] | ", __FILE__, __LINE__), (void)(_x)) : ((void)0))
	#define DBG_FATAL(_x) (printf("(%s, %d) [FATAL] | ", __FILE__, __LINE__), (void)(_x), abort())
	#define DBGN(_m, _x) (((_m) & dbgm_verbose) ? ((void)(_x)) : ((void)0))
#endif

#endif

