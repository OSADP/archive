#include "TimeStamp.h"

struct timespec tsSubtract(struct timespec time1, struct timespec time2)
{
    struct  timespec  result ;

    if ((time1.tv_sec < time2.tv_sec) ||
        ((time1.tv_sec == time2.tv_sec) &&
         (time1.tv_nsec <= time2.tv_nsec))) {		/* TIME1 <= TIME2? */
        result.tv_sec = result.tv_nsec = 0 ;
    } else {						/* TIME1 > TIME2 */
        result.tv_sec = time1.tv_sec - time2.tv_sec ;
        if (time1.tv_nsec < time2.tv_nsec) {
            result.tv_nsec = time1.tv_nsec + 1000000000L - time2.tv_nsec ;
            result.tv_sec-- ;				/* Borrow a second. */
        } else {
            result.tv_nsec = time1.tv_nsec - time2.tv_nsec ;
        }
    }

    return (result) ;
}

double tsDiff(struct timespec start, struct timespec end)
{
    return tsToSeconds(end) - tsToSeconds(start);
}

void tsAddSeconds(struct timespec *time, double secondsToAdd)
{
    time->tv_nsec += (int)(secondsToAdd * 1000000000.0);
    time->tv_sec += ((int)secondsToAdd);
    if (time->tv_nsec > 1000000000)
    {
        time->tv_nsec -= 1000000000;
        time->tv_sec += 1;
    } else if (time->tv_nsec < 0)
    {
        time->tv_nsec += 1000000000;
        time->tv_sec -= 1;
    }
}

double tsToSeconds(struct timespec time)
{
    return ((double) time.tv_sec + (time.tv_nsec / 1000000000.0)) ;
}

void tsFromSeconds(double time, struct timespec *out)
{
    out->tv_sec = (int)time;
    out->tv_nsec = (int)(time * 1000000000.0);
}
