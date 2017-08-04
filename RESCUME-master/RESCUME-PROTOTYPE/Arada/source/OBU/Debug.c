#include "Debug.h"


const int dbgm_verbose= (DBGM_GEN | DBGM_CF | DBGM_RES | DBGM_ON | DBGM_APP);
const int dbgm_info= (DBGM_GEN | DBGM_BT | DBGM_UI | DBGM_CF | DBGM_WR | DBGM_RES | DBGM_ON | DBGM_APP | DBGM_GPS);
const int dbgm_warning= (DBGM_GEN | DBGM_GPS | DBGM_BT | DBGM_UI | DBGM_CF | DBGM_WR | DBGM_RES | DBGM_ON | DBGM_APP);
const int dbgm_error = (DBGM_GEN | DBGM_GPS | DBGM_BT | DBGM_UI | DBGM_CF | DBGM_WR | DBGM_RES | DBGM_ON | DBGM_APP);

//const int dbgm_verbose= (DBGM_GEN | DBGM_ON | DBGM_APP | DBGM_GPS);
//const int dbgm_info= (DBGM_GEN | DBGM_BT | DBGM_UI | DBGM_GPS);
//const int dbgm_warning= (DBGM_GEN | DBGM_BT | DBGM_UI | DBGM_CF | DBGM_WR | DBGM_RES | DBGM_ON | DBGM_APP | DBGM_GPS);
//const int dbgm_error = (DBGM_GEN | DBGM_BT | DBGM_UI | DBGM_CF | DBGM_WR | DBGM_RES | DBGM_ON | DBGM_APP | DBGM_GPS);

struct timespec dbg_start_time = { 0 };
static int started = 0;

double getTime()
{
	if (!started)
	{
		started = 1;
		clock_gettime(CLOCK_MONOTONIC, &dbg_start_time);
	}

	struct timespec dbg_current_time;
	clock_gettime(CLOCK_MONOTONIC, &dbg_current_time);

	return tsDiff(dbg_start_time, dbg_current_time);
}
