/**
 * @file         cJSONxtra.c
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

#include "cJSONxtra.h"

int cJSONxtra_tryGetDouble(cJSON *root, const char *key, double *out)
{
	cJSON *element = cJSON_GetObjectItem(root, key);

	if (element == NULL || element->type != cJSON_Number)
		return 0;

	*out = element->valuedouble;
	return 1;
}

int cJSONxtra_tryGetInt(cJSON *root, const char *key, int *out)
{
	cJSON *element = cJSON_GetObjectItem(root, key);

	if (element == NULL || element->type != cJSON_Number)
		return 0;

	*out = element->valueint;
	return 1;
}

int cJSONxtra_tryGetBool(cJSON *root, const char *key, int *out)
{
	cJSON *element = cJSON_GetObjectItem(root, key);

	if (element != NULL && element->type == cJSON_True)
	{
		*out = 1;
		return 1;
	}
	else if (element != NULL && element->type == cJSON_False)
	{
		*out = 0;
		return 1;
	}
	
	return 0;
}
