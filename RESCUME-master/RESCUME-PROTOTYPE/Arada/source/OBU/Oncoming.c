/**
 * @file         Oncoming.c
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


/********************* INCLUDES **********************/
#include "Oncoming.h"
#include "RescumeConfig.h"
#include "UiController.h"
#include "GpsAdapter.h"
#include "GpsUBloxDevice.h"
#include "Debug.h"
#include "TimeStamp.h"
#include "RescumeAlaCartePayload.h"
#include "DsrcUtils.h"
#include "Queue.h"
 
#include <stdio.h>
#include <string.h>
#include <pthread.h>
#include <endian.h>

#include <EmergencyVehicleAlert.h>
#include <TravelerInformation.h>
#include <AlaCarte.h>
#include <wave.h>

/********************* TYPEDEFS AND STRUCTURES **********************/
#define MAX_LANE_COUNT 20

typedef enum {
	SpeedSignType_noSign = -1,
	SpeedSignType_reducedSpeedAhead,
	SpeedSignType_reducedSpeed,
	SpeedSignType__length
} SpeedSignType;

typedef enum {
	LaneChangeSignType_noSign = -1,
	LaneChangeSignType_closedAhead,
	LaneChangeSignType_closed,
	LaneChangeSignType_merge,
	LaneChangeSignType_laneClosedMerge,
	LaneChangeSignType__length
} LaneChangeSignType;

typedef struct {
	struct timespec timestamp;
	double dt;
	double speed;

	double laneLocations[MAX_LANE_COUNT];

} OncomingAlgorithmSnapshot;


/********************* INTERNAL PROTOTYPES **********************/
static Oncoming *oncoming_create();
static void oncoming_destroy(Oncoming *responder);
static void *oncoming_run(void *arg);

static int oncoming_tryGetRawLanePosition(Oncoming *oncoming, double latitude, double longitude, double heading, double *out);

static OncomingAlgorithmSnapshot *oncoming_createOncomingAlgorithmSnapshot(Oncoming *oncoming, OncomingAlgorithmSnapshot *reuse);
static void oncoming_updateOncomingSnapshots(Oncoming *oncoming, Queue *snapshots);
static double oncoming_getEstimatedDistanceAlongLaneFromSnapshots(Queue *snapshots, int laneNumber);

static int oncoming_buildAcmPacket(AlaCarte_t *acm, RescumeAlaCartePayload *payload);

static EvaMetadata *oncomingData_createEvaMetaData(WaveRxPacket *packet);
static void oncomingData_destroyEvaMetaData(EvaMetadata *eva);
static void oncomingData_insertUpdateOrDestroyEva(OncomingSharedData *oncomingData, WaveRxPacket *packet);
static int oncomingData_purgeOldEvas(OncomingSharedData *oncomingData, double staleThreshold);

static void oncomingData_destroyCurrentTim(OncomingSharedData *oncomingData);
static void oncomingData_updateOrDestroyTim(OncomingSharedData *oncomingData, WaveRxPacket *packet);
static void oncomingData_updateOrDestroyTimNewUntested(OncomingSharedData *oncomingData, WaveRxPacket *packet);
static SingleList *oncomingData_createLanesFromTim(WaveRxPacket *packet);
static int oncomingData_getPostedSpeedLimit(TravelerInformation_t *tim);
static int oncomingData_hasTimInformation(OncomingSharedData *oncomingData);
static int oncomingData_isNewTim(OncomingSharedData *oncomingData, WaveRxPacket *packet);

extern void buildBSMRequestData();


/********************* FILE GLOBAL VARIABLES **********************/
static pthread_mutex_t singletonLock = PTHREAD_MUTEX_INITIALIZER;
static Oncoming *sInstance = NULL;


/********************* EXPOSED METHODS **********************/
Oncoming *oncoming_getInstance()
{
	pthread_mutex_lock(&singletonLock);
	if (!sInstance)
		sInstance = oncoming_create();
	pthread_mutex_unlock(&singletonLock);

	return sInstance;
}

void oncoming_destroyInstance()
{
	pthread_mutex_lock(&singletonLock);
	if (sInstance)
		oncoming_destroy(sInstance);
	sInstance = NULL;
	pthread_mutex_unlock(&singletonLock);
}

void oncoming_start(Oncoming *oncoming)
{
	if (!oncoming)
		return;
	
	DBG_INFO(DBGM_ON, printf("Oncoming: Starting Oncoming Module\n"));
	thread_start(oncoming->thread, oncoming_run, oncoming);
}

void oncoming_stop(Oncoming *oncoming)
{
	if (!oncoming)
		return;
	
	DBG_INFO(DBGM_ON, printf("Oncoming: Stopping Oncoming Module\n"));
	thread_join(oncoming->thread);
}

void oncoming_processAndDestroyWaveMessage(Oncoming *oncoming, WaveRxPacket *packet)
{
	if (!oncoming)
	{
		waveRadio_destroyRxPacket(packet);
		return;
	}

	if (packet->type == &asn_DEF_EmergencyVehicleAlert)
	{
		oncomingData_insertUpdateOrDestroyEva(&oncoming->data, packet);

	}
	else if (packet->type == &asn_DEF_TravelerInformation)
	{
		clock_gettime(CLOCK_MONOTONIC, &oncoming->lastTimReceiveTime);
		oncomingData_updateOrDestroyTim(&oncoming->data, packet);
		//oncomingData_updateOrDestroyTimNewUntested(&oncoming->data, packet);	
	}
	else
		waveRadio_destroyRxPacket(packet);
}

int oncoming_processUiMsg(Oncoming *oncoming, cJSON *root)
{
	if (!oncoming)
		return 0;
	
	cJSON *typeId = cJSON_GetObjectItem(root,"typeid");

	if (typeId == NULL || typeId->type != cJSON_String)
		return 0;

	return 0;
}

int oncoming_getEvaCount(Oncoming *oncoming)
{
	if (!oncoming)
		return 0;

	lock_lock(oncoming->data.lock);

	int results = singleList_size(oncoming->data.evaList);

	lock_unlock(oncoming->data.lock);

	return results;
}

void oncoming_getTimPacketId(Oncoming *oncoming, char *buf, int length)
{
	if (!oncoming)
	{
		strncpy(buf, "", length);
		return;
	}

	lock_lock(oncoming->data.lock);
	
	if (oncoming->data.tim.packet && ((TravelerInformation_t *) oncoming->data.tim.packet->structure)->packetID)
	{
		char len = 0;
		len += snprintf(buf + len, length - len, "0x");
		int i;
		for(i = ((TravelerInformation_t *) oncoming->data.tim.packet->structure)->packetID->size - 1; i >= 0; i--)
			len += snprintf(buf + len, length - len, "%02x", ((TravelerInformation_t *) oncoming->data.tim.packet->structure)->packetID->buf[i]);
	}
	else
	{
		strncpy(buf, "", length);
	}

	lock_unlock(oncoming->data.lock);
}

int oncoming_tryGetCurrentRawLanePosition(Oncoming *oncoming, double *out)//, double latitude, double longitude, double heading, double *out)
{
	assert(oncoming != NULL);
	assert(out != NULL);

	GPSData gpsData;
	gpsDevice_getData(gps_getDefaultDevice(), &gpsData);
	return oncoming_tryGetRawLanePosition(oncoming, gpsData.latitude, gpsData.longitude, gpsData.course, out);
}


/********************* INTERNAL METHODS **********************/
Oncoming *oncoming_create()
{
	Oncoming *results = (Oncoming *)calloc(1, sizeof(Oncoming));
	if (!results)
		return NULL;

	if (!(results->thread = thread_create()))
	{
		oncoming_destroy(results);
		return NULL;
	}

	if (!(results->data.lock = lock_create()))
	{
		oncoming_destroy(results);
		return NULL;
	}

	DBG_INFO(DBGM_ON, printf("Oncoming: Creating new Oncoming Module\n"));

	return results;
}

void oncoming_destroy(Oncoming *oncoming)
{
	if (!oncoming)
		return;

	oncoming_stop(oncoming);

	if (oncoming->thread)
		thread_destroy(oncoming->thread);

	singleList_destroy(&oncoming->data.evaList);
	if (oncoming->data.tim.packet)
		waveRadio_destroyRxPacket(oncoming->data.tim.packet);
	if (oncoming->data.lock)
		lock_destroy(oncoming->data.lock);

	free(oncoming);
}

void *oncoming_run(void *arg)
{
	Oncoming *oncoming = (Oncoming *)arg;
	assert(oncoming != NULL);

	RescumeConfig config;

	//State
	RescumeThreatLevel lastSpeedThreatLevel = RescumeThreatLevel_reset;
	RescumeThreatLevel lastLaneChangeThreatLevel = RescumeThreatLevel_reset;
	RescumeThreatLevel lastCollisionThreatLevel = RescumeThreatLevel_reset;
	SpeedSignType lastSpeedSignType = SpeedSignType_noSign;
	int lastLaneChangeSignType = -1;

	clock_gettime(CLOCK_MONOTONIC, &oncoming->lastTimReceiveTime);

	//Timing
	struct timespec lastBsmBroadcastTime;
	struct timespec lastCalculationTime;
	struct timespec lastAcmBroadcastTime;
	struct timespec lastLaneRateChangeCalcTime;
	struct timespec lastGpsUpdateTime;
	clock_gettime(CLOCK_MONOTONIC, &lastBsmBroadcastTime);
	clock_gettime(CLOCK_MONOTONIC, &lastCalculationTime);
	clock_gettime(CLOCK_MONOTONIC, &lastAcmBroadcastTime);
	clock_gettime(CLOCK_MONOTONIC, &lastLaneRateChangeCalcTime);
	clock_gettime(CLOCK_MONOTONIC, &lastGpsUpdateTime);

	Queue *snapshotQueue = queue_create(free);

	while(thread_isRunning(oncoming->thread))
	{
		struct timespec currentTime;
		clock_gettime(CLOCK_MONOTONIC, &currentTime);

		rescumeConfig_get(&config);

		if (tsDiff(lastBsmBroadcastTime, currentTime) >= 1.0/config.dsrcBsmMaxRate && config.dsrcBsmMaxRate > 0)
		{
			clock_gettime(CLOCK_MONOTONIC, &lastBsmBroadcastTime);
			buildBSMRequestData();
		}

		if (tsDiff(lastGpsUpdateTime, currentTime) >= 20.0)
		{
			clock_gettime(CLOCK_MONOTONIC, &lastGpsUpdateTime);

			GpsDevice *gps = gps_getDefaultDevice();
			gpsUBloxDevice_setCfgRateInfo(gps, 200);
			usleep(1000);
			gpsUBloxDevice_setDynamicMode(gps, UBloxDynamicMode_Automotive);
			usleep(1000);
			gpsUBloxDevice_setStaticHoldThreshold(gps, 0);
		}

		oncomingData_purgeOldEvas(&oncoming->data, config.oncomingStaleEvaTimeout);

		lock_lock(oncoming->data.lock);
		if (tsDiff(oncoming->lastTimReceiveTime, currentTime) >= config.oncomingStaleEvaTimeout)
		{
			oncomingData_destroyCurrentTim(&oncoming->data);
		}
		lock_unlock(oncoming->data.lock);

		if (oncomingData_hasTimInformation(&oncoming->data) && tsDiff(lastCalculationTime, currentTime) >= 1.0/config.oncomingMaxCalculationRate)
		{

			oncoming_updateOncomingSnapshots(oncoming, snapshotQueue);

			RescumeThreatLevel speedThreatLevel = RescumeThreatLevel_reset;
			RescumeThreatLevel laneChangeThreatLevel = RescumeThreatLevel_reset;
			SpeedSignType speedSignType = SpeedSignType_noSign;
			int laneChangeSignType = -1;

			clock_gettime(CLOCK_MONOTONIC, &lastCalculationTime);

			int allowAcmBroadcast = tsDiff(lastAcmBroadcastTime, currentTime) >= 1.0/config.dsrcAcmMaxRate && config.dsrcAcmMaxRate > 0.0;
			if (allowAcmBroadcast)
				clock_gettime(CLOCK_MONOTONIC, &lastAcmBroadcastTime);

			lock_lock(oncoming->data.lock);

			/*
			 * Match vehicle to lanes
			 */
			GPSData gpsData;
			gpsDevice_getData(gps_getDefaultDevice(), &gpsData);

			TimLaneMetaData *bestClosedLane = NULL;
			TimLaneMetaData *bestSpeedLane = NULL;
			struct DsrcPointToPathMatchResults bestClosedLaneMatchResults;
			struct DsrcPointToPathMatchResults bestSpeedLaneMatchResults;

			SingleList **iterator = &oncoming->data.tim.laneInformation;
			while(*iterator)
			{
				TimLaneMetaData *laneInformation = (*iterator)->value;

				struct DsrcPointToPathMatchResults matchResults = dsrc_matchPointToShapePointSet(laneInformation->shapePointSet, gpsData.latitude, gpsData.longitude, gpsData.course, config.dsrcMatchOptions);

				if (matchResults.percentOffCenter < 10.0)
				{
					if ((bestClosedLane == NULL || matchResults.percentOffCenter < bestClosedLaneMatchResults.percentOffCenter) && (laneInformation->type == TimLaneType_Closed))
					{
						bestClosedLane = laneInformation;
						bestClosedLaneMatchResults = matchResults;
					}
					else if ((bestSpeedLane == NULL || matchResults.percentOffCenter < bestSpeedLaneMatchResults.percentOffCenter)
						&& (laneInformation->type == TimLaneType_SpeedRestriction))
					{
						bestSpeedLane = laneInformation;
						bestSpeedLaneMatchResults = matchResults;
					}
				}

				iterator = &(*iterator)->nextNode;
			}

			rescumeConfig_lockForEdit(&config);
			if (bestClosedLane == NULL && bestSpeedLane == NULL)
			{
				if (config.appVehicleIdLock == VehicleIdLock_temporary)
					config.appVehicleIdLock = VehicleIdLock_noLock;
			}
			else if (config.appVehicleIdLock == VehicleIdLock_noLock)
			{
				config.appVehicleIdLock = VehicleIdLock_temporary;
			}
			rescumeConfig_updateAndUnlock(&config);
			
			/*
			 * Determine Threat level.
			 */
			int isClearOfSpeedRestrictions = bestSpeedLane && (bestSpeedLane->laneNumber == (oncoming->data.tim.numberOfLanes - 1)) && ((bestSpeedLaneMatchResults.percentOffCenter * bestSpeedLaneMatchResults.sideOfPath * oncoming->data.tim.sideOfRoad) < -0.99);
			int isClearOfClosedLanes = bestClosedLane && (bestClosedLane->laneNumber == (oncoming->data.tim.numberOfClosedLanes - 1)) && ((bestClosedLaneMatchResults.percentOffCenter * bestClosedLaneMatchResults.sideOfPath * oncoming->data.tim.sideOfRoad) < -0.99);

			if (bestClosedLane == NULL && bestSpeedLane == NULL)
			{
				//printf("Not in a lane...\n");
			}
			else
			{
				if (bestClosedLane)
				{
					if (!isClearOfClosedLanes && bestClosedLaneMatchResults.distanceAlongPath >= bestClosedLane->distanceToAlert2)
					{
						laneChangeThreatLevel = RescumeThreatLevel_inViolation;
						laneChangeSignType = 3;
					}
					else if (!isClearOfClosedLanes && bestClosedLaneMatchResults.distanceAlongPath >= bestClosedLane->distanceToAlert1)
					{
						laneChangeThreatLevel = RescumeThreatLevel_approachingViolation; 
						laneChangeSignType = 2;
					}
					else if (!isClearOfClosedLanes)
					{
						laneChangeThreatLevel = RescumeThreatLevel_noThreat; 
						laneChangeSignType = 0;
					}
					else
					{
						if (bestClosedLaneMatchResults.distanceAlongPath >= bestClosedLane->distanceToAlert1)
							laneChangeSignType = 1;
						else
							laneChangeSignType = 0;
					}
				}

				if (bestSpeedLane && !isClearOfSpeedRestrictions)
				{
					if (bestSpeedLaneMatchResults.distanceAlongPath >= bestSpeedLane->distanceToAlert2)
					{
						speedSignType = SpeedSignType_reducedSpeed;
						speedThreatLevel = (gpsData.speed * 2.236936292054402 > (oncoming->data.tim.reducedSpeedMph + 3)) ? RescumeThreatLevel_inViolation : RescumeThreatLevel_noThreat;
					}
					else if (bestSpeedLaneMatchResults.distanceAlongPath >= bestSpeedLane->distanceToAlert1)
					{
						speedSignType = SpeedSignType_reducedSpeed;
						speedThreatLevel = (gpsData.speed * 2.236936292054402 > (oncoming->data.tim.reducedSpeedMph + 3)) ? RescumeThreatLevel_approachingViolation : RescumeThreatLevel_noThreat;
					}
					else
					{
						speedThreatLevel = RescumeThreatLevel_noThreat;
						speedSignType = SpeedSignType_reducedSpeedAhead;
					}
				}
			}

			RescumeThreatLevel threatLevel = laneChangeThreatLevel > speedThreatLevel ? laneChangeThreatLevel : speedThreatLevel;
			RescumeThreatLevel lastThreatLevel = lastLaneChangeThreatLevel > lastSpeedThreatLevel ? lastLaneChangeThreatLevel : lastSpeedThreatLevel;

			/*
			 * Build Alerts
			 */
			if ((threatLevel >= RescumeThreatLevel_noThreat && allowAcmBroadcast) || (threatLevel != lastThreatLevel))
			{
				AlaCarte_t acm;
				memset(&acm, 0, sizeof(AlaCarte_t));

				RescumeAlaCartePayload payload;
				*((uint32_t *)payload.oncomingId) = htobe32(config.appVehicleId);
				payload.tLevel = threatLevel;

				DBG(DBGM_ON, printf("Oncoming: Building AlaCarte Message.\n"));
				if (oncoming_buildAcmPacket(&acm, &payload))
				{
					waveRadio_txPacket(&asn_DEF_AlaCarte, &acm);
				}
				else
					DBG_ERR(DBGM_ON, printf("Oncoming: Error building AlaCarte Packet.\n"));


				ASN_STRUCT_FREE_CONTENTS_ONLY(asn_DEF_AlaCarte, &acm);
			}

			int collisionThreatLevel = RescumeThreatLevel_reset;

			if (gpsData.speed > config.oncomingCollisionMinSpeed)
			{
				SingleList **evaIterator = &oncoming->data.evaList;
				while(*evaIterator != NULL)
				{
					EvaMetadata *eva = (*evaIterator)->value;
					int collisionDetected = 0;
					
					SingleList **laneIterator = &oncoming->data.tim.laneInformation;
					while(*laneIterator != NULL)
					{
						TimLaneMetaData *laneInformation = (*laneIterator)->value;

						struct DsrcPointToPathMatchResults vehMatchResults = dsrc_matchPointToShapePointSet(laneInformation->shapePointSet, gpsData.latitude, gpsData.longitude, gpsData.course, config.dsrcMatchOptions);
						struct DsrcPointToPathMatchResults evaMatchResults = dsrc_matchPointToShapePointSet(laneInformation->shapePointSet, eva->latitude, eva->longitude, 0, DSRC_MATCH_ALL);

						if (evaMatchResults.percentOffCenter < 1.2 && abs(vehMatchResults.percentOffCenter - evaMatchResults.percentOffCenter) < (config.oncomingCollisionLaneWidth / 2.0)) //vehMatchResults.percentOffCenter < 1.0)
						{
							double requiredStoppingDistance = gpsData.speed * gpsData.speed / (2 * config.oncomingEmergencyDecelRate);
							if (eva->collisionDetected)
								requiredStoppingDistance *= 2;

							double requiredStoppingDistanceUi = requiredStoppingDistance + gpsData.speed * config.oncomingCollisionDriverPrtTime;
							double requiredStoppingDistanceAcm = requiredStoppingDistance / gpsData.speed < 2.5 ? gpsData.speed * config.oncomingCollisionResponderPrtTime : requiredStoppingDistance;

							double distanceAlongPath = oncoming_getEstimatedDistanceAlongLaneFromSnapshots(snapshotQueue, laneInformation->laneNumber);

							double distanceToEva = evaMatchResults.distanceAlongPath - (distanceAlongPath > 0 ? distanceAlongPath : vehMatchResults.distanceAlongPath);
							if (distanceToEva < 100.0 && distanceToEva > 0.0)
								printf("Distance to EVA: %f meters. requiredStoppingDistance: %f meters. requiredStoppingDistanceUi: %f meters.  requiredStoppingDistanceAcm: %f meters\n", distanceToEva, requiredStoppingDistance, requiredStoppingDistanceUi, requiredStoppingDistanceAcm);
							if (requiredStoppingDistanceUi >= distanceToEva && distanceToEva > 0)
							{
								collisionThreatLevel = RescumeThreatLevel_collision;
								laneChangeSignType = 4;
							}
							
							if (requiredStoppingDistanceAcm >= distanceToEva && distanceToEva > 0)
							{
								collisionDetected = 1;
								if (eva->collisionDetected == 0 || allowAcmBroadcast)
								{
									AlaCarte_t acm;
									memset(&acm, 0, sizeof(AlaCarte_t));

									RescumeAlaCartePayload payload;
									*((uint32_t *)payload.oncomingId) = htobe32(config.appVehicleId);
									*((uint32_t *)payload.responderId) = htobe32(eva->vehicleId);
									payload.tLevel = RescumeThreatLevel_collision;

									DBG(DBGM_ON, printf("Oncoming: Building AlaCarte Message.\n"));
									if (oncoming_buildAcmPacket(&acm, &payload))
									{
										waveRadio_txPacket(&asn_DEF_AlaCarte, &acm);
									}
									else
										DBG_ERR(DBGM_ON, printf("Oncoming: Error building AlaCarte Packet.\n"));

									ASN_STRUCT_FREE_CONTENTS_ONLY(asn_DEF_AlaCarte, &acm);
								}

								eva->collisionDetected = 1;
								clock_gettime(CLOCK_MONOTONIC, &eva->collisionDetectionTime);
								printf("COLLISION DETECTED AGAINST RESPONDER 0x%8x (Distance: %f meters, Required Stopping Distance: %f)\n", eva->vehicleId, distanceToEva, requiredStoppingDistance);
							}

							break;
						}
						else if (evaMatchResults.percentOffCenter < 1.0 || vehMatchResults.percentOffCenter < 1.0)
						{
							//printf("Not in same lane as EVA\n");
						}

						laneIterator = &(*laneIterator)->nextNode;
					}

					//This keeps collision threat levels on after you pass the collision..
					if (eva->collisionDetected)
					{
						collisionThreatLevel = RescumeThreatLevel_collision;
						laneChangeSignType = 4;
					}

					if (eva->collisionDetected && !collisionDetected && tsDiff(eva->collisionDetectionTime, currentTime) >= config.oncomingCollisionResetTime)
					{
						eva->collisionDetected = 0;
						printf("CLEARING COLLISION AGAINST RESPONDER 0x%8x \n", eva->vehicleId);

						AlaCarte_t acm;
						memset(&acm, 0, sizeof(AlaCarte_t));

						RescumeAlaCartePayload payload;
						*((uint32_t *)payload.oncomingId) = htobe32(config.appVehicleId);
						*((uint32_t *)payload.responderId) = htobe32(eva->vehicleId);
						payload.tLevel = RescumeThreatLevel_reset;

						DBG(DBGM_ON, printf("Oncoming: Building AlaCarte Message.\n"));
						if (oncoming_buildAcmPacket(&acm, &payload))
						{
							waveRadio_txPacket(&asn_DEF_AlaCarte, &acm);
						}
						else
							DBG_ERR(DBGM_ON, printf("Oncoming: Error building AlaCarte Packet.\n"));

						ASN_STRUCT_FREE_CONTENTS_ONLY(asn_DEF_AlaCarte, &acm);
					}

					evaIterator = &(*evaIterator)->nextNode;
				}
			}

			//TODO do we need to debounce this?
			if (lastLaneChangeThreatLevel != laneChangeThreatLevel || lastLaneChangeSignType != laneChangeSignType
				|| lastSpeedThreatLevel != speedThreatLevel || lastSpeedSignType != speedSignType 
				|| lastCollisionThreatLevel != collisionThreatLevel)
			{
				lastLaneChangeThreatLevel = laneChangeThreatLevel;
				lastLaneChangeSignType = laneChangeSignType;
				lastSpeedThreatLevel = speedThreatLevel;
				lastSpeedSignType = speedSignType;
				lastCollisionThreatLevel = collisionThreatLevel;
				//TODO ui message

				int laneCount = oncoming->data.tim.numberOfClosedLanes - 1;
				int isOnLeft = oncoming->data.tim.sideOfRoad < 0;
				int speed = oncoming->data.tim.reducedSpeedMph;

				cJSON *root = cJSON_CreateObject();
				assert(root);

				cJSON_AddStringToObject(root, "typeid", "ALRT");
				cJSON_AddNumberToObject(root, "speedthreatlevel", speedThreatLevel);
				cJSON_AddNumberToObject(root, "speedsigntype", speedSignType);
				cJSON_AddNumberToObject(root, "speed", speed);
				cJSON_AddNumberToObject(root, "lanechangethreatlevel", laneChangeThreatLevel > collisionThreatLevel ? laneChangeThreatLevel : collisionThreatLevel);
				cJSON_AddNumberToObject(root, "lanechangesigntype", laneChangeSignType);
				cJSON_AddNumberToObject(root, "lanecount", laneCount);
				isOnLeft ? cJSON_AddTrueToObject(root, "isonleft") : cJSON_AddFalseToObject(root, "isonleft");

				ui_sendMessage(root);
				cJSON_Delete(root);
			}


			lock_unlock(oncoming->data.lock);
		}


		//struct DsrcPathMatchOptions options;
		//struct DsrcPathMatchResults results = dsrc_matchToShapePointSet(&responder->timInfo.msgStruct->dataFrames.list.array[1]->regions.list.array[0]->area.choice.shapePointSet, 0, 0, 0, options);
		//printf("MSG: '%s', PATH LENGTH: %f\n\n", results.errorTxt, results.pathLength);

		//TODO... Run collision detection at rate 'X'
			//This should run very quickly to find the threshold, 
			//but how do we limit the rate of ACM transmission once the collision detection is activated.
			//
			//!!!!!!!!!
			//I've thought about expanding the OncomingData.evaList to not be Linked List of WaveRxPacket's
			//but a Linked List of a structure to hold state about the EVA and our algorithms regarding each EVA.
			//!!!!!!!!!

		//TODO... Run thread determiniation at rate 'Y'
			//This really has very little to do with the EVA's, so it'll just be calculated at a certain rate
			//and [0..1] ACM sent per round.

		//SO I'm thinking I need another file and structure to handle all of this... Now what to call this one?

		usleep(10000);
	}

	queue_destroy(snapshotQueue);

	return NULL;
}

int oncoming_tryGetRawLanePosition(Oncoming *oncoming, double latitude, double longitude, double heading, double *out)
{
	assert(oncoming != NULL);
	assert(out != NULL);

	const int MAX_PERCENT_OFF_CENTER = 50.0;

	int results = 0;
	lock_lock(oncoming->data.lock);

	if (oncoming->data.tim.laneInformation != NULL)
	{
		RescumeConfig config;
		rescumeConfig_get(&config);

		//Match the first lane...
		SingleList *iterator = oncoming->data.tim.laneInformation;
		TimLaneMetaData *lastLaneInformation = iterator->value;
		struct DsrcPointToPathMatchResults lastMatchResults = dsrc_matchPointToShapePointSet(lastLaneInformation->shapePointSet, latitude, longitude, heading, config.dsrcMatchOptions);

		if (lastMatchResults.percentOffCenter < MAX_PERCENT_OFF_CENTER)
		{
			*out = (lastMatchResults.percentOffCenter / 2.0) * lastMatchResults.sideOfPath * oncoming->data.tim.sideOfRoad * -1.0 + lastLaneInformation->laneNumber;
			results = 1;
		}

		iterator = iterator->nextNode;

		//Try to find a better match combination..
		while(iterator)
		{
			TimLaneMetaData *laneInformation = iterator->value;
			struct DsrcPointToPathMatchResults matchResults = dsrc_matchPointToShapePointSet(laneInformation->shapePointSet, latitude, longitude, heading, config.dsrcMatchOptions);

			if(matchResults.sideOfPath != lastMatchResults.sideOfPath)
			{
				*out = ((lastMatchResults.percentOffCenter / 2.0) * lastMatchResults.sideOfPath * oncoming->data.tim.sideOfRoad * -1.0 + lastLaneInformation->laneNumber);
				*out += ((matchResults.percentOffCenter / 2.0) * matchResults.sideOfPath * oncoming->data.tim.sideOfRoad * -1.0 + laneInformation->laneNumber);
				*out /= 2.0;

				results = 2;
				break;
			}
			else if (matchResults.percentOffCenter < lastMatchResults.percentOffCenter && matchResults.percentOffCenter < MAX_PERCENT_OFF_CENTER)
			{
				*out = (matchResults.percentOffCenter / 2.0) * matchResults.sideOfPath * oncoming->data.tim.sideOfRoad * -1.0 + laneInformation->laneNumber;
			}

			lastLaneInformation = laneInformation;
			lastMatchResults = matchResults;

			iterator = iterator->nextNode;
		}

	}

	lock_unlock(oncoming->data.lock);
	return results;
}

OncomingAlgorithmSnapshot *oncoming_createOncomingAlgorithmSnapshot(Oncoming *oncoming, OncomingAlgorithmSnapshot *reuse)
{
	assert(oncoming != NULL);

	if (!reuse)
	{
		reuse = (OncomingAlgorithmSnapshot *)calloc(1, sizeof(OncomingAlgorithmSnapshot));
	}

	if (!reuse)
		return NULL;

	clock_gettime(CLOCK_MONOTONIC, &reuse->timestamp);

	GPSData gpsData;
	gpsDevice_getData(gps_getDefaultDevice(), &gpsData);

	RescumeConfig config;
	rescumeConfig_get(&config);

	reuse->speed = gpsData.speed;

	int i;
	for(i = 0; i < MAX_LANE_COUNT; i++)
	{
		reuse->laneLocations[i] = -1.0;
	}

	lock_lock(oncoming->data.lock);

	SingleList *laneIterator = oncoming->data.tim.laneInformation;

	while(laneIterator != NULL)
	{
		TimLaneMetaData *laneInformation = laneIterator->value;
		assert(laneInformation != NULL);
		assert(laneInformation->laneNumber < MAX_LANE_COUNT);

		struct DsrcPointToPathMatchResults matchResults = dsrc_matchPointToShapePointSet(laneInformation->shapePointSet, gpsData.latitude, gpsData.longitude, gpsData.course, config.dsrcMatchOptions);

		if (matchResults.percentOffCenter < 1000.0)
		{
			reuse->laneLocations[laneInformation->laneNumber] = matchResults.distanceAlongPath;
		}

		laneIterator = laneIterator->nextNode;
	}

	lock_unlock(oncoming->data.lock);

	return reuse;
}

void oncoming_updateOncomingSnapshots(Oncoming *oncoming, Queue *snapshots)
{
	OncomingAlgorithmSnapshot *removedSnapshot = NULL;

	struct timespec currentTime;
	clock_gettime(CLOCK_MONOTONIC, &currentTime);

	while(queue_front(snapshots) != NULL && tsDiff(((OncomingAlgorithmSnapshot *)queue_front(snapshots))->timestamp, currentTime) > 4.0)
	{
		if (removedSnapshot)
			free(removedSnapshot);
		removedSnapshot = queue_pop(snapshots);
	}

	if (!queue_back(snapshots) || tsDiff(((OncomingAlgorithmSnapshot *)queue_back(snapshots))->timestamp, currentTime) > 0.1)
	{
		OncomingAlgorithmSnapshot * newSnapshot = oncoming_createOncomingAlgorithmSnapshot(oncoming, removedSnapshot);
		if (newSnapshot)
			queue_push(snapshots, newSnapshot);
	}
}

double oncoming_getEstimatedDistanceAlongLaneFromSnapshots(Queue *snapshots, int laneNumber)
{
	assert(snapshots != NULL);
	assert(laneNumber >= 0);
	assert(laneNumber < MAX_LANE_COUNT);

	SingleList *iterator = queue_getUnderlyingList(snapshots);

	if (iterator == NULL)
		return -1;

	OncomingAlgorithmSnapshot *lastSnapshot = (OncomingAlgorithmSnapshot *)iterator->value;
	assert(lastSnapshot != NULL);
	iterator = iterator->nextNode;

	double results = lastSnapshot->laneLocations[laneNumber];

	while(iterator != NULL)
	{
		OncomingAlgorithmSnapshot *snapshot = iterator->value;
		assert(snapshot != NULL);
		
		double dt = tsDiff(lastSnapshot->timestamp, snapshot->timestamp);
		double speedAdvance = dt * snapshot->speed;
		double thisSnapshotLocation = snapshot->laneLocations[laneNumber];

		if (results < 0)
		{
			results = thisSnapshotLocation;
		}
		else if (thisSnapshotLocation < 0)
		{
			results += speedAdvance;
		}
		else
		{
			results = results + speedAdvance > thisSnapshotLocation ? results + speedAdvance : thisSnapshotLocation;
		}

		lastSnapshot = snapshot;
		iterator = iterator->nextNode;
	}

	return results;
}

int oncoming_buildAcmPacket(AlaCarte_t *acm, RescumeAlaCartePayload *payload)
{
	if (!acm || !payload)
		return 0;

	GPSData gpsData;
	gpsDevice_getData(gps_getDefaultDevice(), &gpsData);

	/**** MSG ID = ACM ****/
	if (!acm->msgID.buf)
		if (!(acm->msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t))))
			return 0;
	acm->msgID.size = sizeof(uint8_t);
	acm->msgID.buf[0] = DSRCmsgID_alaCarteMessage;
	
	/**** PayloadData ****/
	if (!acm->data.item7_87)
		if (!(acm->data.item7_87 = (PayloadData_t *) calloc(1, sizeof(PayloadData_t))))
			return 0;
	if (!acm->data.item7_87->buf)
		if (!(acm->data.item7_87->buf = (uint8_t *) calloc(1, sizeof(RescumeAlaCartePayload))))
			return 0;
	acm->data.item7_87->size = sizeof(RescumeAlaCartePayload);
	memcpy(acm->data.item7_87->buf, payload, sizeof(RescumeAlaCartePayload));

	return 1;
}

EvaMetadata *oncomingData_createEvaMetaData(WaveRxPacket *packet)
{
	EvaMetadata *results = (EvaMetadata *)calloc(1, sizeof(EvaMetadata));
	if (!results)
		return NULL;

	results->packet = packet;

	EmergencyVehicleAlert_t *eva = packet->structure;
	if (eva->rsaMsg.positon)
	{
		results->latitude = dsrc_convFromStructLatitude(eva->rsaMsg.positon->lat);
		results->longitude = dsrc_convFromStructLongitude(eva->rsaMsg.positon->Long);
	}
	else
	{
		results->latitude = -9000;
		results->longitude = -9000;
	}

	results->vehicleId = be32toh(*((uint32_t*)(eva->id->buf)));

	return results;
}

void oncomingData_destroyEvaMetaData(EvaMetadata *eva)
{
	if (eva->packet)
		waveRadio_destroyRxPacket(eva->packet);

	free(eva);
}

/**
 * Inserts, updates, or deletes a new EVA from the EVA list.	
 */
void oncomingData_insertUpdateOrDestroyEva(OncomingSharedData *oncomingData, WaveRxPacket *packet)
{
	//Safety.  Should make this an assert...
	if (!packet || !oncomingData || packet->type != &asn_DEF_EmergencyVehicleAlert)
		return;

	//Get it's vehicle id and destroy it if it's not valid. 
	uint32_t evaVehicleId = dsrc_getEvaVehicleId32(packet->structure);
	if (!evaVehicleId)
	{
		DBG_WARN(DBGM_ON, printf("OncomingData: Can't insert an EVA without a Vehicle Id.\n"));
		waveRadio_destroyRxPacket(packet);
		return;
	}

	//Lock up the list and insert or update the EVA.
	lock_lock(oncomingData->lock);
	SingleList **iterator = &oncomingData->evaList;
	while(*iterator != NULL)
	{
		EvaMetadata *eva = (*iterator)->value;
		if (evaVehicleId == dsrc_getEvaVehicleId32(eva->packet->structure))
		{			
			//EVA's match Id's, so destroy the old packet and save the current packet.
			//DBG(DBGM_ON, printf("OncomingData: Updating EVA...\n"));
			waveRadio_destroyRxPacket(eva->packet);
			eva->packet = packet;
			break;
		}
		else
		{
			//Advance to next node.
			iterator = &(*iterator)->nextNode;
		}
	}
	if (*iterator == NULL)
	{
		//EVA wasn't found, so create new MetaData and List node and insert.
		DBG_INFO(DBGM_ON, printf("OncomingData: New EVA Found.\n"));

		EvaMetadata *newEva = oncomingData_createEvaMetaData(packet);
		if (newEva)
		{
			SingleList *newNode = singleList_nodeCreate(newEva, (void (*)(void *))oncomingData_destroyEvaMetaData);
			if (newNode)
			{
				singleList_insert(iterator, 0, newNode);
			}
			else
			{
				DBG_ERR(DBGM_ON, printf("OncomingData: Unable to malloc new node to store EVA Meta Data.\n"));
			}
		}
		else
		{
			DBG_ERR(DBGM_ON, printf("OncomingData: Unable to malloc new space for EVA Meta Data.\n"));
		}
	}

	lock_unlock(oncomingData->lock);
}

/**
 * Purges old EVA's from the EVA list that are older than the stale threshold (seconds).
 */
int oncomingData_purgeOldEvas(OncomingSharedData *oncomingData, double staleThreshold)
{
	int result = 0;
	int length = 0;

	struct timespec currentTime;
	clock_gettime(CLOCK_MONOTONIC, &currentTime);

	lock_lock(oncomingData->lock);
	SingleList **iterator = &oncomingData->evaList;
	while(*iterator != NULL)
	{
		EvaMetadata* eva = (*iterator)->value;
		if (!eva || tsDiff(eva->packet->rxTime, currentTime) >= staleThreshold)
		{
			result++;
			singleList_removeAndDestroy(iterator, 0);
		}
		else
		{
			iterator = &(*iterator)->nextNode;
			length++;
		}
	}

	lock_unlock(oncomingData->lock);

	if (result)
		DBG_INFO(DBGM_ON, printf("OncomingData: Purged %d of %d EVA(s)\n", result, length + result));

	return result;
}

void oncomingData_destroyCurrentTim(OncomingSharedData *oncomingData)
{
	lock_lock(oncomingData->lock);

	singleList_destroy(&oncomingData->tim.laneInformation);

	if(oncomingData->tim.packet)
		waveRadio_destroyRxPacket(oncomingData->tim.packet);
	oncomingData->tim.packet = NULL;

	lock_unlock(oncomingData->lock);

}

void oncomingData_updateOrDestroyTim(OncomingSharedData *oncomingData, WaveRxPacket *packet)
{
	RescumeConfig config;
	rescumeConfig_get(&config);

	lock_lock(oncomingData->lock);
	
	if (oncomingData_isNewTim(oncomingData, packet))
	{
		//asn_fprint(stdout, packet->type, packet->structure);

		printf("NEW TIM, here we go!!!\n");
		oncomingData_destroyCurrentTim(oncomingData);

		oncomingData->tim.packet = packet;

		TravelerInformation_t *tim = packet->structure;
		
		//Load lanes..... Loop through each Data Frame
		int i;
		for(i = 0; i < tim->dataFrames.list.count; i++)
		{
			struct TravelerInformation__dataFrames__Member *dataFrame = tim->dataFrames.list.array[i];
			if (dataFrame->content.present != content_PR_advisory)
				continue;
			if (dataFrame->regions.list.count < 1)
				continue;

			TimLaneType laneType;
			//Only going to load these itis codes
			if (dsrc_containsItisCode(&dataFrame->content.choice.advisory, 769))
				laneType = TimLaneType_Closed;
			else if (dsrc_containsItisCode(&dataFrame->content.choice.advisory, 6933))
				laneType = TimLaneType_SpeedRestriction;
			else
				continue;

			//Loop through each region
			int j;
			for(j = 0; j < dataFrame->regions.list.count; j++)
			{
				ValidRegion_t *region = dataFrame->regions.list.array[j];
				//Only going to use ShapePointSets which can discribe lanes.
				if (region->area.present != ValidRegion__area_PR_shapePointSet)
					continue;

				TimLaneMetaData *newLaneInformation = (TimLaneMetaData *)calloc(1, sizeof(TimLaneMetaData));
				assert(newLaneInformation != NULL);
				SingleList *newNode = singleList_nodeCreate(newLaneInformation, free);
				assert(newNode != NULL);

				newLaneInformation->shapePointSet = &region->area.choice.shapePointSet;
				newLaneInformation->type = laneType;

				SingleList **iterator = &oncomingData->tim.laneInformation;
				while (*iterator != NULL)
				{
					TimLaneMetaData *laneInformation = (*iterator)->value;
					//TODO: Some asserts?
					int compare = dsrc_compareParallelShapePointSets(laneInformation->shapePointSet, newLaneInformation->shapePointSet);
					if (compare > 0)
					{
						singleList_insert(iterator, 0, newNode);
						break;
					}
					else if (compare == 0)
					{
						//Err bad lane
						break;
					}
					else
					{
						iterator = &(*iterator)->nextNode;
					}
				}
				//Needs to be inserted at the end.
				if (*iterator == NULL)
				{
					singleList_insert(iterator, 0, newNode);
				}

				//TODO....??
			}

		}
		oncomingData->tim.numberOfLanes = singleList_size(oncomingData->tim.laneInformation);
		oncomingData->tim.numberOfClosedLanes = 0;

		printf("Found %d lanes\n", oncomingData->tim.numberOfLanes);

		if (singleList_size(oncomingData->tim.laneInformation) == 0)
		{
			oncomingData_destroyCurrentTim(oncomingData);
		}
		else
		{
			//Load Posted Speed.....
			oncomingData->tim.postedSpeedMph = oncomingData_getPostedSpeedLimit(tim);
			if (oncomingData->tim.postedSpeedMph == 0)
			{
				DBG_ERR(DBGM_ON, printf("Oncoming::oncomingData_updateOrDestroyTim(): No posted speed in TIM.  Using 65 MPH\n"));
				oncomingData->tim.postedSpeedMph = 65;
			}
			if (oncomingData->tim.postedSpeedMph >= 65)
				oncomingData->tim.reducedSpeedMph = 45;
			else
				oncomingData->tim.reducedSpeedMph = 25;
			printf("Posted speed is: %d mph.  Reduced speed is: %d mph\n", oncomingData->tim.postedSpeedMph, oncomingData->tim.reducedSpeedMph);

			//Load left or right.
			oncomingData->tim.sideOfRoad = ((TimLaneMetaData *)oncomingData->tim.laneInformation->value)->type == TimLaneType_SpeedRestriction ? -1 : 1;
			printf("Incident is on: %s\n", oncomingData->tim.sideOfRoad > 0 ? "Right" : "Left");

			//Load incident zone distances....
			//Flip the list so that we are working from the traffic side towards the side that is closed.
			//This is because we have to build back taper zones.
			if (oncomingData->tim.sideOfRoad > 0)
			{
				singleList_reverse(&oncomingData->tim.laneInformation);
			}

			//TODO need to handle lane ShapePointSets not ending at the same point on the road.
			double previousTaperLengths = 0;

			int currentLaneNumber = oncomingData->tim.numberOfLanes - 1;

			SingleList **iterator = &oncomingData->tim.laneInformation;
			while(*iterator)
			{
				TimLaneMetaData *laneInformation = (*iterator)->value;
				struct DsrcMatchOptions matchOptions = { .headingAcceptance = -2.0, .defaultLaneWidth = 3.0 };
				struct DsrcPointToPathMatchResults bestResults = { .percentOffCenter = 2000.0 };

				int i;
				for(i = 0; i < tim->dataFrames.list.count; i++)
				{
					struct TravelerInformation__dataFrames__Member *dataFrame = tim->dataFrames.list.array[i];
					if (dataFrame->content.present != content_PR_advisory)
						continue;
					if (dataFrame->regions.list.count < 1)
						continue;

					//Only going to load these itis codes
					if (!dsrc_containsItisCode(&dataFrame->content.choice.advisory, 531))
						continue;

					//Loop through each region
					int j;
					for(j = 0; j < dataFrame->regions.list.count; j++)
					{
						ValidRegion_t *region = dataFrame->regions.list.array[j];
						//Only going to use ShapePointSets which can discribe lanes.
						if (region->area.present != ValidRegion__area_PR_shapePointSet)
							continue;

						double incidentLatitude, incidentLongitude;
						dsrc_getBackOfShapePointSet(&region->area.choice.shapePointSet, &incidentLatitude, &incidentLongitude);

						struct DsrcPointToPathMatchResults matchResults = dsrc_matchPointToShapePointSet(laneInformation->shapePointSet, incidentLatitude, incidentLongitude, 0, matchOptions);
						
						if (matchResults.percentOffCenter < bestResults.percentOffCenter)
						{
							bestResults = matchResults;
						}

					}
				}

				if (bestResults.percentOffCenter < 6)
				{
					printf("Found back of incident at %.1f meters (%.2f%% complete) and %.2f%% off center\n", bestResults.distanceAlongPath, bestResults.percentComplete * 100.0, bestResults.sideOfPath * bestResults.percentOffCenter * 100.0);
					laneInformation->distanceToBackOfIncident = bestResults.distanceAlongPath;
				}
				else
				{
					laneInformation->distanceToBackOfIncident = bestResults.pathLength;
					//PROBLEM.. didn't find an incident..
				}

				laneInformation->laneNumber = currentLaneNumber;
				currentLaneNumber--;

				//TODO legibility distance
				double bufferSpace = dsrc_getBufferZoneImperial(oncomingData->tim.postedSpeedMph) * 0.3048;
				double decelDistance = ((oncomingData->tim.postedSpeedMph * 0.44704)*(oncomingData->tim.postedSpeedMph * 0.44704) - (oncomingData->tim.reducedSpeedMph * 0.44704)*(oncomingData->tim.reducedSpeedMph * 0.44704)) / (config.oncomingAggressiveDecelRate);
				double taperLength = (oncomingData->tim.postedSpeedMph * 12) * 0.3048;


				if (laneInformation->type == TimLaneType_SpeedRestriction)
				{
					laneInformation->distanceToAlert2 = laneInformation->distanceToBackOfIncident - bufferSpace;
					laneInformation->distanceToAlert1 = laneInformation->distanceToAlert2 - decelDistance;
				}
				if (laneInformation->type == TimLaneType_Closed)
				{
					laneInformation->distanceToAlert2 = laneInformation->distanceToBackOfIncident - (bufferSpace + previousTaperLengths);
					laneInformation->distanceToAlert1 = laneInformation->distanceToAlert2 - taperLength;
					previousTaperLengths += taperLength;
					oncomingData->tim.numberOfClosedLanes++;
				}



				printf("Finished with lane...\n");
				printf("     -> Lane Type = %s\n", laneInformation->type == TimLaneType_SpeedRestriction ? "Speed Restricted" : "Closed");
				printf("     -> Alert 1 Distance = %.1f meters\n", laneInformation->distanceToAlert1);
				printf("     -> Alert 2 Distance = %.1f meters\n", laneInformation->distanceToAlert2);
				printf("     -> Incident Distance = %.1f meters\n", laneInformation->distanceToBackOfIncident);



				iterator = &(*iterator)->nextNode;
			}
		}


	}
	else
	{
		//Repeated tim...
		waveRadio_destroyRxPacket(packet);
	}

	lock_unlock(oncomingData->lock);
}

void oncomingData_updateOrDestroyTimNewUntested(OncomingSharedData *oncomingData, WaveRxPacket *packet)
{
	assert(oncomingData != NULL);
	assert(packet != NULL);

	if (!oncomingData_isNewTim(oncomingData, packet))
	{
		waveRadio_destroyRxPacket(packet);
		return;
	}
	
	DBG_INFO(DBGM_ON, printf("Received new TIM...\n"));
	TravelerInformation_t *tim = packet->structure;
	//asn_fprint(stdout, packet->type, packet->structure);

	RescumeConfig config;
	rescumeConfig_get(&config);

	TimMetaData newTimData;

	newTimData.packet = packet;
	newTimData.laneInformation = oncomingData_createLanesFromTim(packet);
	newTimData.numberOfLanes = singleList_size(newTimData.laneInformation);
	newTimData.numberOfClosedLanes = 0;

	DBG_INFO(DBGM_ON, printf("Found %d lanes.\n", newTimData.numberOfLanes));
	
	newTimData.postedSpeedMph = oncomingData_getPostedSpeedLimit(tim);
	if (newTimData.postedSpeedMph == 0)
	{
		DBG_ERR(DBGM_ON, printf("Oncoming::oncomingData_updateOrDestroyTim(): No posted speed in TIM.  Using 65 MPH\n"));
		newTimData.postedSpeedMph = 65;
	}
	if (newTimData.postedSpeedMph >= 65)
		newTimData.reducedSpeedMph = 45;
	else
		newTimData.reducedSpeedMph = 25;
	DBG_INFO(DBGM_ON, printf("Posted speed is: %d mph.  Reduced speed is: %d mph\n", newTimData.postedSpeedMph, newTimData.reducedSpeedMph));

	newTimData.sideOfRoad = ((TimLaneMetaData *)newTimData.laneInformation->value)->type == TimLaneType_SpeedRestriction ? -1 : 1;
	printf("Incident is on: %s\n", newTimData.sideOfRoad > 0 ? "Right" : "Left");

	if (newTimData.sideOfRoad > 0)
	{
		singleList_reverse(&newTimData.laneInformation);
	}


	double previousTaperLengths = 0;
	int currentLaneNumber = newTimData.numberOfLanes - 1;

	SingleList **iterator = &newTimData.laneInformation;
	while(*iterator)
	{
		TimLaneMetaData *laneInformation = (*iterator)->value;
		struct DsrcMatchOptions matchOptions = { .headingAcceptance = -2.0, .defaultLaneWidth = 3.0 };
		struct DsrcPointToPathMatchResults bestResults = { .percentOffCenter = 2000.0 };

		int i;
		for(i = 0; i < tim->dataFrames.list.count; i++)
		{
			struct TravelerInformation__dataFrames__Member *dataFrame = tim->dataFrames.list.array[i];
			if (dataFrame->content.present != content_PR_advisory)
				continue;
			if (dataFrame->regions.list.count < 1)
				continue;

			//Only going to load these itis codes
			if (!dsrc_containsItisCode(&dataFrame->content.choice.advisory, 531))
				continue;

			//Loop through each region
			int j;
			for(j = 0; j < dataFrame->regions.list.count; j++)
			{
				ValidRegion_t *region = dataFrame->regions.list.array[j];
				//Only going to use ShapePointSets which can discribe lanes.
				if (region->area.present != ValidRegion__area_PR_shapePointSet)
					continue;

				double incidentLatitude, incidentLongitude;
				dsrc_getBackOfShapePointSet(&region->area.choice.shapePointSet, &incidentLatitude, &incidentLongitude);

				struct DsrcPointToPathMatchResults matchResults = dsrc_matchPointToShapePointSet(laneInformation->shapePointSet, incidentLatitude, incidentLongitude, 0, matchOptions);
				
				if (matchResults.percentOffCenter < bestResults.percentOffCenter)
				{
					bestResults = matchResults;
				}

			}
		}

		if (bestResults.percentOffCenter < 6)
		{
			printf("Found back of incident at %.1f meters (%.2f%% complete) and %.2f%% off center\n", bestResults.distanceAlongPath, bestResults.percentComplete * 100.0, bestResults.sideOfPath * bestResults.percentOffCenter * 100.0);
			laneInformation->distanceToBackOfIncident = bestResults.distanceAlongPath;
		}
		else
		{
			laneInformation->distanceToBackOfIncident = bestResults.pathLength;
			//PROBLEM.. didn't find an incident..
		}

		laneInformation->laneNumber = currentLaneNumber;
		currentLaneNumber--;

		//TODO legibility distance
		double bufferSpace = dsrc_getBufferZoneImperial(newTimData.postedSpeedMph) * 0.3048;
		double decelDistance = ((newTimData.postedSpeedMph * 0.44704)*(newTimData.postedSpeedMph * 0.44704) - (newTimData.reducedSpeedMph * 0.44704)*(newTimData.reducedSpeedMph * 0.44704)) / (config.oncomingAggressiveDecelRate);
		double taperLength = (newTimData.postedSpeedMph * 12) * 0.3048;


		if (laneInformation->type == TimLaneType_SpeedRestriction)
		{
			laneInformation->distanceToAlert2 = laneInformation->distanceToBackOfIncident - bufferSpace;
			laneInformation->distanceToAlert1 = laneInformation->distanceToAlert2 - decelDistance;
		}
		if (laneInformation->type == TimLaneType_Closed)
		{
			laneInformation->distanceToAlert2 = laneInformation->distanceToBackOfIncident - (bufferSpace + previousTaperLengths);
			laneInformation->distanceToAlert1 = laneInformation->distanceToAlert2 - taperLength;
			previousTaperLengths += taperLength;
			newTimData.numberOfClosedLanes++;
		}



		printf("Finished with lane...\n");
		printf("     -> Lane Type = %s\n", laneInformation->type == TimLaneType_SpeedRestriction ? "Speed Restricted" : "Closed");
		printf("     -> Alert 1 Distance = %.1f meters\n", laneInformation->distanceToAlert1);
		printf("     -> Alert 2 Distance = %.1f meters\n", laneInformation->distanceToAlert2);
		printf("     -> Incident Distance = %.1f meters\n", laneInformation->distanceToBackOfIncident);



		iterator = &(*iterator)->nextNode;
	}

	lock_lock(oncomingData->lock);
	oncomingData->tim = newTimData;
	lock_unlock(oncomingData->lock);
}

SingleList *oncomingData_createLanesFromTim(WaveRxPacket *packet)
{
	assert(packet != NULL);

	SingleList* results = NULL;

	TravelerInformation_t *tim = packet->structure;

	int i;
	for(i = 0; i < tim->dataFrames.list.count; i++)
	{
		struct TravelerInformation__dataFrames__Member *dataFrame = tim->dataFrames.list.array[i];
		if (dataFrame->content.present != content_PR_advisory)
			continue;
		if (dataFrame->regions.list.count < 1)
			continue;

		TimLaneType laneType;
		//Only going to load these itis codes
		if (dsrc_containsItisCode(&dataFrame->content.choice.advisory, 769))
			laneType = TimLaneType_Closed;
		else if (dsrc_containsItisCode(&dataFrame->content.choice.advisory, 6933))
			laneType = TimLaneType_SpeedRestriction;
		else
			continue;

		//Loop through each region
		int j;
		for(j = 0; j < dataFrame->regions.list.count; j++)
		{
			ValidRegion_t *region = dataFrame->regions.list.array[j];
			//Only going to use ShapePointSets which can discribe lanes.
			if (region->area.present != ValidRegion__area_PR_shapePointSet)
				continue;

			TimLaneMetaData *newLaneInformation = (TimLaneMetaData *)calloc(1, sizeof(TimLaneMetaData));
			assert(newLaneInformation != NULL);
			SingleList *newNode = singleList_nodeCreate(newLaneInformation, free);
			assert(newNode != NULL);

			newLaneInformation->shapePointSet = &region->area.choice.shapePointSet;
			newLaneInformation->type = laneType;

			//Insert from right(0) to left(n)
			SingleList **iterator = &results;
			while (*iterator != NULL)
			{
				TimLaneMetaData *laneInformation = (*iterator)->value;
				int compare = dsrc_compareParallelShapePointSets(laneInformation->shapePointSet, newLaneInformation->shapePointSet);
				if (compare > 0)
				{
					singleList_insert(iterator, 0, newNode);
					break;
				}
				/*else if (compare == 0)
				{
					DBG_WARN(DBGM_ON, printf("Oncoming::oncomingData_loadLanesFromTim(): Unable to insert lane. (FRAME: %d, REGION: %d)\n", i, j));
					break;
				}*/
				else
				{
					iterator = &(*iterator)->nextNode;
				}
			}
			if (*iterator == NULL)
			{
				singleList_insert(iterator, 0, newNode);
			}
		}
	}

	return results;
}

int oncomingData_getPostedSpeedLimit(TravelerInformation_t *tim)
{
	int results = 0;

	int i;
	for(i = 0; i < tim->dataFrames.list.count; i++)
	{
		struct TravelerInformation__dataFrames__Member *dataFrame = tim->dataFrames.list.array[i];
		if (dataFrame->content.present != content_PR_advisory)
			continue;

		//Only going to load these itis codes
		if (!dsrc_containsItisCode(&dataFrame->content.choice.advisory, 6933))
			continue;
		
		ITIScodesAndText_t *itisCodesAndText = &dataFrame->content.choice.advisory;

		int j;
		for(j = 0; j < itisCodesAndText->list.count; j++)
		{
			if (itisCodesAndText->list.array[j]->item.present != item_PR_text_it)
				continue;

			ITIStext_t *text = &itisCodesAndText->list.array[j]->item.choice.text;

			if (text->size < 7)
				continue;

			if (strstr((char *)text->buf, "mph:") == (char *)text->buf)
			{
				results = atoi((char *)text->buf + 4);
				break;
			}
		}
		
	}

	return results;
}

int oncomingData_hasTimInformation(OncomingSharedData *oncomingData)
{
	int results = oncomingData->tim.packet != NULL;

	return results;
}

int oncomingData_isNewTim(OncomingSharedData *oncomingData, WaveRxPacket *packet)
{
	assert(packet != NULL);
	assert(oncomingData != NULL);
	assert(packet != NULL);
	assert(packet->structure != NULL);

	int results = 0;

	lock_lock(oncomingData->lock);

	if (oncomingData_hasTimInformation(oncomingData))
		results = dsrc_hasSameTimPacketId((TravelerInformation_t *)oncomingData->tim.packet->structure, (TravelerInformation_t *)packet->structure);
	else
		results = 1;

	lock_unlock(oncomingData->lock);
	return results;
}
