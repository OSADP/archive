

#include <time.h>

struct timespec tsSubtract(struct timespec time1, struct timespec time2);
double tsDiff(struct timespec start, struct timespec end);
void tsAddSeconds(struct timespec *time, double secondsToAdd);
double tsToSeconds(struct timespec time);
void tsFromSeconds(double time, struct timespec *out);
