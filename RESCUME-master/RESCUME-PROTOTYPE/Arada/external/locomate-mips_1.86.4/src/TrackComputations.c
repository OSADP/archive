
/*
RM: The method here use the equations on pg 4 of
"Formula and constants for the calculation of the Swiss cnformal cylindrical
projection and for the transformation between coordinate systems"
*/

#include<stdio.h>
#include<math.h>
#include<stdlib.h>
#include<syslog.h>

#include "TrackComputations.h"
//
#ifndef PI
#define PI (4 * atan(1.0))
#endif

extern struct ECEFCoords gECEFCoords;
extern struct EllipsoidalCoords gEllipsoidalCoords;

struct ECEFCoords *fnRotateAboutZAxis(struct ECEFCoords *PointA, double AngleOfRotation)
{
	//struct ECEFCoords *RotatedPoint = (struct ECEFCoords *)malloc(sizeof (struct ECEFCoords));
	struct ECEFCoords *RotatedPoint = (struct ECEFCoords *)&gECEFCoords;

	/*
	Since rotation is about the Z axis the Z coordinate is invariant under the
	transformation
	|X| =    |cos(angle -sin(angle)| |X|
	|Y|      |sin(angle) cos(angle)| |Y|
	*/
	if(!(-PI <= AngleOfRotation && PI >= AngleOfRotation))
	syslog(LOG_INFO,"AngleOfRotation: %lf  <= %lf && %lf >= %lf",-PI,AngleOfRotation,PI,AngleOfRotation);

	RotatedPoint->Z = PointA->Z;
	RotatedPoint->X = PointA->X * cos(AngleOfRotation) - PointA->Y * sin(AngleOfRotation);
	RotatedPoint->Y = PointA->X * sin(AngleOfRotation) + PointA->Y * cos(AngleOfRotation);

	return RotatedPoint;

}

/*
int fnGetTokensFromCSVString(char *CSVString, char **ArrayForTokens)
{
	int i = 0;
	int LengthOfToken;
	char *ptrToStartOfSearch = CSVString;
	char *ptrToNextComma;

	if(0 == strlen(CSVString))
		return 0;

	ptrToNextComma = strchr(ptrToStartOfSearch, ',');
	if (NULL == ptrToNextComma)
	{
		LengthOfToken = strlen(CSVString);;
		ArrayForTokens[0] = (char *)calloc((LengthOfToken + 1), sizeof(char));
		strncpy(ArrayForTokens[0], CSVString, LengthOfToken);
		return 1;
	}

	while(NULL != ptrToNextComma)
	{
		LengthOfToken = (ptrToNextComma - ptrToStartOfSearch);
		ArrayForTokens[i] = (char *)calloc((LengthOfToken + 1), sizeof(char));
		strncpy(ArrayForTokens[i], ptrToStartOfSearch, LengthOfToken);
		ptrToStartOfSearch = ptrToNextComma + 1;
		ptrToNextComma = strchr(ptrToStartOfSearch, ',');

		//printf("\nArrayForTokens[%d]: %s\n", i, ArrayForTokens[i]);

		i += 1;

	}
	LengthOfToken = strlen(ptrToStartOfSearch);
	ArrayForTokens[i] = (char *)calloc((LengthOfToken + 1), sizeof(char));
	strncpy(ArrayForTokens[i], ptrToStartOfSearch, LengthOfToken);
	//printf("\nArrayForTokens[%d]: %s\n", i, ArrayForTokens[i]);

	return (i+ 1);
}
*/

struct EllipsoidalCoords *fnDel_X_Del_YToLatLong(struct EllipsoidalCoords *ptrToRefPoint, \
													CARTESIANCOORDS *ptrToDel_X_Del_Y)
{
	long double del_Lat, del_Long;
	struct EllipsoidalCoords *OffsetPointEllipsoidalCoords = \
						(struct EllipsoidalCoords *)&gEllipsoidalCoords;
	long double DistanceAlongTheNormalFromPointToZAxis = ELLIPSOID_MAJOR_AXIS \
													/
													(sqrt(1 - SqrOfTheEccentricity * \
													pow(sin(ptrToRefPoint->lat), 2)));
	
	del_Long = ptrToDel_X_Del_Y->X/(DistanceAlongTheNormalFromPointToZAxis * \
								cos(ptrToRefPoint->lat));
	del_Lat = ptrToDel_X_Del_Y->Y/DistanceAlongTheNormalFromPointToZAxis;

	OffsetPointEllipsoidalCoords->lat = ptrToRefPoint->lat + del_Lat;
	if(!(-PI/2 < OffsetPointEllipsoidalCoords->lat && \
			PI/2 > OffsetPointEllipsoidalCoords->lat))
	{
	//	free(OffsetPointEllipsoidalCoords);
		return NULL;
	}
	OffsetPointEllipsoidalCoords->lon = ptrToRefPoint->lon + del_Long;

	if(-PI > OffsetPointEllipsoidalCoords->lon) 
		OffsetPointEllipsoidalCoords->lon += 2*PI;	

	if(PI <= OffsetPointEllipsoidalCoords->lon)
		OffsetPointEllipsoidalCoords->lon -= 2*PI;	

	return OffsetPointEllipsoidalCoords;
}

struct EllipsoidalCoords *fnComputeOffsetPointUsingHeadingLatLong( \
											struct EllipsoidalCoords *ptrToPredecessorPoint, \
											struct EllipsoidalCoords *ptrToPoint,\
											CARTESIANCOORDS *ptrToOffsetCoords,unsigned char TrackIsStationary, 
											unsigned char OffsetWRTPredecessor,long double *ptrToPrevHeading)
{
	long double HeadingInPlusMinusPIwrtEast;
	struct EllipsoidalCoords *OffsetPointEllipsoidalCoords; 
						
						//(struct EllipsoidalCoords *)calloc(1, sizeof(struct EllipsoidalCoords));
	//CARTESIANCOORDS *ptrToOffsetInWE_SN = (CARTESIANCOORDS *)calloc(1, sizeof(CARTESIANCOORDS));
	CARTESIANCOORDS *ptrToOffsetInWE_SN ;
	/*
	In the above we do not take the average of the two latitudes as this required fpr
	a point that is perpendicular to he track.
	This is a source of error.
	*/
	/*
	First we compute the heading. The heading is the angle that the track from
	PredecessorPoint to Point makes with the S-N axis. A track going East has a heading of PI/2.
	A Track going South has a heading of PI.
	A track going West has a heading of 3*PI/2
	*/
    if(TrackIsStationary && 2 * PI <= *ptrToPrevHeading)
		/*
		Then they are both co-located. Hence there is no track 
		*/
        /*
        The track is stationary and no past heading exists
        The heading passed in, if valid, should be in the range [-PI, +PI)
        */
		{
           //free(OffsetPointEllipsoidalCoords);
	        if (TRUE == OffsetWRTPredecessor)
            {
               return ptrToPredecessorPoint;
            }
            else
            {
                 return ptrToPoint;
          	}
 		}
	if(TrackIsStationary )
	{
       if(!(-PI <= *ptrToPrevHeading && PI > *ptrToPrevHeading));
			syslog(LOG_INFO,"PrevHeading: %lf <= %Lf && %lf > %Lf",-PI,*ptrToPrevHeading,PI,*ptrToPrevHeading);
       /*
       The track is stationary but we have the last heading.
       We can use it
       */
       HeadingInPlusMinusPIwrtEast = *ptrToPrevHeading;
	}
    if (!TrackIsStationary)
    {
	    HeadingInPlusMinusPIwrtEast = fnComputeAngleWRT_WE(ptrToPredecessorPoint, \
														    ptrToPoint);

		if(!(-PI <= HeadingInPlusMinusPIwrtEast && PI > HeadingInPlusMinusPIwrtEast))
			syslog(LOG_INFO,"Heading: %lf <= %Lf && %lf > %Lf",-PI,HeadingInPlusMinusPIwrtEast,PI,HeadingInPlusMinusPIwrtEast);
        /*
        We need to retain the headin. In case the vehicle comes to a halt. The offset will still be required.
        */
        *ptrToPrevHeading = HeadingInPlusMinusPIwrtEast;
    }

	/*
	The offset is specified 	by displacements along track and cross track
	The +ve X-Axis is assumed to be towards the front of the vehicle.
	The +ve Y-Axis is to the left of the vehicle.
	The +ve Z-axis is the vertical axis
	If the X-Y plane is rotated about the Z-Axis by -HeadingInPlusMinusPIwrtEast the X-Axis will then
	coincide with the WE Axis and the Y-Axis with the SN-Axis.
	We have routines for rotating a vector by an angle in a fixed reference system.
	The same routines can be used if we recognize that rotating the vector by Theta is the same as 
	rotating the reference axes by -Theta.
	So since the rference axes have to be rotated by -HeadingInPlusMinusPIwrtEast. We will
	get the correct result if we use HeadingInPlusMinusPIwrtEast as the angle of rotation
	*/
	ptrToOffsetInWE_SN = fnRotateAboutZAxis(ptrToOffsetCoords, HeadingInPlusMinusPIwrtEast);
	if (TRUE == OffsetWRTPredecessor)
	{
		OffsetPointEllipsoidalCoords = fnDel_X_Del_YToLatLong(ptrToPredecessorPoint, ptrToOffsetInWE_SN);
	}
	else
	{
		OffsetPointEllipsoidalCoords = fnDel_X_Del_YToLatLong(ptrToPoint, ptrToOffsetInWE_SN);
	}
	if (NULL == OffsetPointEllipsoidalCoords)
	{
		//free(ptrToOffsetInWE_SN);
		return NULL;
	}

	//free(ptrToOffsetInWE_SN);
	return OffsetPointEllipsoidalCoords;
}



//
double fnComputeAngleWRT_WE(struct EllipsoidalCoords *ptrToPoint_A, \
											struct EllipsoidalCoords *ptrToPoint_B)
{
	double Angle;
	long double DistanceAlongTheNormalFromPointToZAxis = ELLIPSOID_MAJOR_AXIS \
													/
													(sqrt(1 - SqrOfTheEccentricity * \
													pow(sin((ptrToPoint_A->lat + \
													ptrToPoint_B->lat)/2), 2)));
	long double DistanceAlongS_N, DistanceAlongW_E;

	DistanceAlongW_E = DistanceAlongTheNormalFromPointToZAxis * \
						cos((ptrToPoint_A->lat + ptrToPoint_B->lat)/2) * \
						(ptrToPoint_B->lon - ptrToPoint_A->lon);

	DistanceAlongS_N =  DistanceAlongTheNormalFromPointToZAxis * \
						(ptrToPoint_B->lat - ptrToPoint_A->lat);

	Angle = atan2(DistanceAlongS_N, DistanceAlongW_E);
	if(PI == Angle)
		Angle = -Angle;
	return Angle;
}
//
