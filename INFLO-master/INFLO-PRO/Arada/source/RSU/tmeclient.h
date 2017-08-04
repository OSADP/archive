/*!
    @file         RSU/tmeclient.h
    @author       Joshua Branch

    @copyright
    Copyright (c) 2013 Battelle Memorial Institute. All rights reserved.

    @par
    Unauthorized use or duplication may violate state, federal and/or
    international laws including the Copyright Laws of the United States
    and of other international jurisdictions.

    @par
    @verbatim
    Battelle Memorial Institute
    505 King Avenue
    Columbus, Ohio  43201
    @endverbatim

    @brief
    TBD

    @details
    TBD
*/


#ifndef	_TMECLIENT_H_
#define	_TMECLIENT_H_


void tme_client_init();
void tme_client_post_bsm(char *message);
void tme_client_post_tim(char *message);
void tme_client_request_tim(void(*callback)(void));
void tme_client_destroy();

#endif
