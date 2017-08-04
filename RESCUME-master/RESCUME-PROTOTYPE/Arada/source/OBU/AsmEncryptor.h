

#ifndef _ASMENCRYPTOR_H_
#define _ASMENCRYPTOR_H_

#include "wave.h"

typedef struct AsmEncryptor {
	int socket;
} AsmEncryptor;

int asmEncryptor_init(AsmEncryptor *encryptor);
void asmEncryptor_destroy(AsmEncryptor *encryptor);
int asmEncryptor_encrypt(AsmEncryptor *encryptor, WSMRequest *wsmreq);

#endif
