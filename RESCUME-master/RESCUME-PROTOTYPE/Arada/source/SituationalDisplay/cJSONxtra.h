/**
 * @file         cJSONxtra.h
 * @author       Joshua Branch, Veronica Hohe
 * 
 * @copyright Copyright (c) 2014 Battelle Memorial Institute. All rights reserved.
 * 
 * @par
 * Unauthorized use or duplication may violate state, federal and/or
 * international laws including the Copyright Laws of the United States
 * and of other international jurisdictions.
 * 
 * @par
 * @verbatim
 * Battelle Memorial Institute
 * 505 King Avenue
 * Columbus, Ohio  43201
 * @endverbatim
 * 
 * @brief
 * TBD
 * 
 * @details
 * TBD
*/

#ifndef _CJSONXTRA_H_
#define _CJSONXTRA_H_

#include <stdlib.h>
#include <cJSON.h>

int cJSONxtra_tryGetDouble(cJSON *root, const char *key, double *out);
int cJSONxtra_tryGetInt(cJSON *root, const char *key, int *out);
int cJSONxtra_tryGetBool(cJSON *root, const char *key, int *out);

//int cJSONxtra_hasKey(cJSON *root, const char *key);

#endif

