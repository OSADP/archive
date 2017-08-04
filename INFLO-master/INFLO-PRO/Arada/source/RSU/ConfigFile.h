#ifndef OBU_ConfigFile_h
#define OBU_ConfigFile_h

#include "gpsc_probe.h"

typedef struct ConfigFile
{
    char tim_get_url[256];
    char bsm_post_url[256];

} ConfigFile;

extern int          cfInitialize(const char* filename);
extern ConfigFile*  cfGetConfigFile();

#endif
