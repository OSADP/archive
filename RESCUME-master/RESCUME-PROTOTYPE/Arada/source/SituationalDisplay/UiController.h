/**
 * @file         UiController.h
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

#ifndef	_UICONTROLLER_H_
#define	_UICONTROLLER_H_

#include "cJSONxtra.h"

void ui_startServer();
void ui_stopServer();

void ui_addRxMessageHandler(int (*handler)(cJSON *root));
void ui_sendMessage(cJSON *root);

void ui_printf(const char* format, ... );

#endif
