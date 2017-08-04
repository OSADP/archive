#ifndef OBU_Vector_h
#define OBU_Vector_h

#include <math.h>

typedef struct Vector
{
    double x;
    double y;

} Vector;

static inline void vecAdd(Vector* pA, Vector* pB, Vector* pR)
{
    pR->x = pA->x + pB->x;
    pR->y = pA->y + pB->y;
    
    return;
}

static inline void vecSub(Vector* pA, Vector* pB, Vector* pR)
{
    pR->x = pA->x - pB->x;
    pR->y = pA->y - pB->y;
    
    return;
}

static inline void vecMult(double s, Vector* pV, Vector* pR)
{
    pR->x = pV->x * s;
    pR->y = pV->y * s;
    
    return;
}

static inline double vecDot(Vector* pA, Vector* pB)
{
    return (pA->x * pB->x + pA->y * pB->y);
}

static inline double vecCross(Vector* pA, Vector *pB, Vector *pC)
{
    return ((pB->x - pA->x)*(pC->y - pA->y) - (pB->y - pA->y)*(pC->x - pA->x));
}

static inline double vecMag(Vector* pA)
{
    return sqrt(vecDot(pA, pA));
}

static inline void vecProjAontoB(Vector* pA, Vector* pB, Vector* pR)
{
    double bMag = vecMag(pB);
    double s = vecDot(pB, pA) / (bMag * bMag);
    
    Vector proj;
    vecMult(s, pB, &proj);
    
    *pR = proj;
    
    return;
}

static inline void vecNormalize(Vector* pV, Vector* pR)
{
    double invMag = 1.0 / vecMag(pV);
    
    pR->x = pV->x * invMag;
    pR->y = pV->y * invMag;
    
    return;
}

/*
typedef struct Vector
{
    double x;
    double y;
    double z;

} Vector;

static inline void vecAdd(Vector* pA, Vector* pB, Vector* pR)
{
    pR->x = pA->x + pB->x;
    pR->y = pA->y + pB->y;
    pR->z = pA->z + pB->z;
    
    return;
}

static inline void vecSub(Vector* pA, Vector* pB, Vector* pR)
{
    pR->x = pA->x - pB->x;
    pR->y = pA->y - pB->y;
    pR->z = pA->z - pB->z;
    
    return;
}

static inline void vecMult(double s, Vector* pV, Vector* pR)
{
    pR->x = pV->x * s;
    pR->y = pV->y * s;
    pR->z = pV->z * s;
    
    return;
}

static inline double vecDot(Vector* pA, Vector* pB)
{
    return (pA->x * pB->x + pA->y * pB->y + pA->z * pB->z);
}

static inline double vecMag(Vector* pA)
{
    return sqrt(vecDot(pA, pA));
}

static inline void vecProjAontoB(Vector* pA, Vector* pB, Vector* pR)
{
    double bMag = vecMag(pB);
    double s = vecDot(pB, pA) / (bMag * bMag);
    
    Vector proj;
    vecMult(s, pB, &proj);
    
    *pR = proj;
    
    return;
}

static inline void vecNormalize(Vector* pV, Vector* pR)
{
    double invMag = 1.0 / vecMag(pV);
    
    pR->x = pV->x * invMag;
    pR->y = pV->y * invMag;
    pR->z = pV->z * invMag;
    
    return;
}
*/

#endif
