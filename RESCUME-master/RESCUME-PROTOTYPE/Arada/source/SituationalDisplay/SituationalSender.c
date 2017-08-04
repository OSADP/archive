/**
 * @file         SituationalSender.c
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
#include "Debug.h"

#include "SituationalSender.h"
#include "UiController.h"
#include "GpsAdapter.h"
#include "Debug.h"
#include "TimeStamp.h"
#include "DsrcUtils.h"
 
#include <stdio.h>
#include <string.h>
#include <pthread.h>
#include <endian.h>

#include <EmergencyVehicleAlert.h>
#include <TravelerInformation.h>
#include <BasicSafetyMessage.h>
#include <wave.h>

/********************* FILE GLOBAL VARIABLES **********************/
static pthread_mutex_t singletonLock = PTHREAD_MUTEX_INITIALIZER;
static SituationalSender *sInstance = NULL;

SituationalSender *situationalSender_create();
void situationalSender_destroy(SituationalSender *situationalSender);
void situationalSender_insertUpdateOrDestroyEva(SituationalSharedData *situationalData, WaveRxPacket *packet);
void situationalSender_insertUpdateOrDestroyBsm(SituationalSharedData *situationalData, WaveRxPacket *packet);
void situationalSender_insertUpdateOrDestroyTim(SituationalSharedData *situationalData, WaveRxPacket *packet);
void *situationalSender_run(void *arg);
void situationalSender_purgeOldWaveMessages(SituationalSharedData *situationalData, double staleThreshold);

/********************* EXPOSED METHODS **********************/
SituationalSender *situationalSender_getInstance()
{
	pthread_mutex_lock(&singletonLock);
	if (!sInstance)
		sInstance = situationalSender_create();
	pthread_mutex_unlock(&singletonLock);

	return sInstance;
}

void situationalSender_destroyInstance()
{
	pthread_mutex_lock(&singletonLock);
	if (sInstance)
		situationalSender_destroy(sInstance);
	sInstance = NULL;
	pthread_mutex_unlock(&singletonLock);
}

void situationalSender_start(SituationalSender *situationalSender)
{
	if (!situationalSender)
		return;
	
	DBG_INFO(DBGM_ON, printf("SituationalSender: Starting SituationalSender Module\n"));
	thread_start(situationalSender->thread, situationalSender_run, situationalSender);
}

void situationalSender_stop(SituationalSender *situationalSender)
{
	if (!situationalSender)
		return;
	
	DBG_INFO(DBGM_ON, printf("SituationalSender: Stopping SituationalSender Module\n"));
	thread_join(situationalSender->thread);
}

void situationalSender_processAndDestroyWaveMessage(SituationalSender *situationalSender, WaveRxPacket *packet)
{
	if (!situationalSender)
	{
		waveRadio_destroyRxPacket(packet);
		return;
	}

	if (packet->type == &asn_DEF_EmergencyVehicleAlert)
	{
		situationalSender_insertUpdateOrDestroyEva(&situationalSender->data, packet);
	}
	else if (packet->type == &asn_DEF_BasicSafetyMessage)
	{
		situationalSender_insertUpdateOrDestroyBsm(&situationalSender->data, packet);
	}
	else if (packet->type == &asn_DEF_TravelerInformation)
	{
		situationalSender_insertUpdateOrDestroyTim(&situationalSender->data, packet);	
	}
	else
		waveRadio_destroyRxPacket(packet);
}



/********************* INTERNAL METHODS **********************/
SituationalSender *situationalSender_create()
{
	SituationalSender *results = (SituationalSender *)calloc(1, sizeof(SituationalSender));
	if (!results)
		return NULL;

	if (!(results->thread = thread_create()))
	{
		situationalSender_destroy(results);
		return NULL;
	}

	if (!(results->data.lock = lock_create()))
	{
		situationalSender_destroy(results);
		return NULL;
	}

	DBG_INFO(DBGM_ON, printf("SituationalSender: Creating new SituationalSender Module\n"));

	return results;
}

void situationalSender_destroy(SituationalSender *situationalSender)
{
	if (!situationalSender)
		return;

	situationalSender_stop(situationalSender);

	if (situationalSender->thread)
		thread_destroy(situationalSender->thread);

	singleList_destroy(&situationalSender->data.evaList);
	singleList_destroy(&situationalSender->data.bsmList);
	singleList_destroy(&situationalSender->data.timList);
	if (situationalSender->data.lock)
		lock_destroy(situationalSender->data.lock);

	free(situationalSender);
}


/**
 * Inserts, updates, or deletes a new EVA from the EVA list.	
 */
void situationalSender_insertUpdateOrDestroyEva(SituationalSharedData *situationalData, WaveRxPacket *packet)
{
	//Safety.  Should make this an assert...
	if (!packet || !situationalData || packet->type != &asn_DEF_EmergencyVehicleAlert)
		return;

	//Get it's vehicle id and destroy it if it's not valid. 
	uint32_t evaVehicleId = dsrc_getEvaVehicleId32(packet->structure);
	if (!evaVehicleId)
	{
		DBG_WARN(DBGM_ON, printf("SituationalSharedData: Can't insert an EVA without a Vehicle Id.\n"));
		waveRadio_destroyRxPacket(packet);
		return;
	}

	//Lock up the list and insert or update the EVA.
	lock_lock(situationalData->lock);
	SingleList **iterator = &situationalData->evaList;
	while(*iterator != NULL)
	{	
		WaveRxPacket *evaPacket = (*iterator)->value;
		if (evaVehicleId == dsrc_getEvaVehicleId32(evaPacket->structure))
		{		
			//EVA's match Id's, so destroy the old packet and save the current packet.
			//DBG(DBGM_ON, printf("SituationalSharedData: Updating EVA...\n"));
			waveRadio_destroyRxPacket(evaPacket);
			(*iterator)->value = packet;
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
		//EVA wasn't found, so create List node and insert.
		DBG_INFO(DBGM_ON, printf("SituationalSharedData: New EVA Found.\n"));
		WaveRxPacket *newEva = packet;
		if (newEva)
		{
			SingleList *newNode = singleList_nodeCreate(newEva, waveRadio_destroyRxPacket);
			if (newNode)
			{
				singleList_insert(iterator, 0, newNode);
			}
			else
			{
				DBG_ERR(DBGM_ON, printf("SituationalSharedData: Unable to malloc new node to store EVA.\n"));
			}
		}
		else
		{
			DBG_ERR(DBGM_ON, printf("SituationalSharedData: Unable to malloc new space for EVA.\n"));
		}
	
	}

	lock_unlock(situationalData->lock);
}


/**
 * Inserts, updates, or deletes a new BSM from the BSM list.	
 */
void situationalSender_insertUpdateOrDestroyBsm(SituationalSharedData *situationalData, WaveRxPacket *packet)
{
	//Safety.  Should make this an assert...
	if (!packet || !situationalData || packet->type != &asn_DEF_BasicSafetyMessage)
		return;

	//Get it's vehicle id and destroy it if it's not valid. 
	uint32_t bsmVehicleId = dsrc_getBsmVehicleId32(packet->structure);
	if (!bsmVehicleId)
	{
		DBG_WARN(DBGM_ON, printf("SituationalSharedData: Can't insert a BSM without a Vehicle Id.\n"));
		waveRadio_destroyRxPacket(packet);
		return;
	}

	// Check if vehicle's ID is already in EVA list.
	lock_lock(situationalData->lock);
	int haveEvaWithSameVehicleId = 0;
	uint32_t evaVehicleId;
	SingleList **iterator = &situationalData->evaList;
	char bsmVehId[9];
	char evaVehId[9];
	sprintf(bsmVehId, "%X", bsmVehicleId);
	while(*iterator != NULL)
	{	
		WaveRxPacket *evaPacket = (*iterator)->value;
		evaVehicleId = dsrc_getEvaVehicleId32(evaPacket->structure);
		sprintf(evaVehId, "%X", evaVehicleId);
		if (strncmp(bsmVehId, evaVehId, 6) == 0)
		{
			haveEvaWithSameVehicleId = 1;	
			break;
		}
		iterator = &(*iterator)->nextNode;
	}

	lock_unlock(situationalData->lock);
	if(haveEvaWithSameVehicleId)
	{
		waveRadio_destroyRxPacket(packet);
		return;
	}
	//Lock up the list and insert or update the BSM.
	lock_lock(situationalData->lock);
	iterator = &situationalData->bsmList;
	while(*iterator != NULL)
	{
		WaveRxPacket *bsmPacket = (*iterator)->value;
		if (memcmp(((BasicSafetyMessage_t *)packet->structure)->blob1.buf+1, ((BasicSafetyMessage_t *)bsmPacket->structure)->blob1.buf+1, 4) == 0)
		{			
			//BSM's match Id's, so destroy the old packet and save the current packet.
			//DBG(DBGM_ON, printf("SituationalSharedData: Updating BSM...\n"));
			waveRadio_destroyRxPacket(bsmPacket);
			(*iterator)->value = packet;
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
		//BSM wasn't found, so create List node and insert.
		DBG_INFO(DBGM_ON, printf("SituationalSharedData: New BSM Found.\n"));

		WaveRxPacket*newBSM = packet;

		if (newBSM)
		{
			SingleList *newNode = singleList_nodeCreate(newBSM, waveRadio_destroyRxPacket);
			if (newNode)
			{
				singleList_insert(iterator, 0, newNode);
			}
			else
			{
				DBG_ERR(DBGM_ON, printf("SituationalSharedData: Unable to malloc new node to store BSM.\n"));
			}
		}
		else
		{
			DBG_ERR(DBGM_ON, printf("SituationalSharedData: Unable to malloc new space for BSM.\n"));
		}
	}
	lock_unlock(situationalData->lock);
}

/**
 * Inserts, updates, or deletes a new TIM from the TIM list.	
 */
void situationalSender_insertUpdateOrDestroyTim(SituationalSharedData *situationalData, WaveRxPacket *packet)
{
	//Safety.  Should make this an assert...
	if (!packet || !situationalData || packet->type != &asn_DEF_TravelerInformation)
		return;

	//Get it's vehicle id and destroy it if it's not valid. 
	uint32_t timVehicleId = dsrc_getTimVehicleId32(packet->structure);
	if (!timVehicleId)
	{
		DBG_WARN(DBGM_ON, printf("SituationalSharedData: Can't insert a TIM without a Vehicle Id.\n"));
		waveRadio_destroyRxPacket(packet);
		return;
	}

	//Lock up the list and insert or update the TIM.
	lock_lock(situationalData->lock);
	SingleList **iterator = &situationalData->timList;
	while(*iterator != NULL)
	{
		WaveRxPacket *timPacket = (*iterator)->value;
		if (timVehicleId == dsrc_getTimVehicleId32(timPacket->structure))
		{		
			//TIM's match Id's, so destroy the old packet and save the current packet.
			//DBG(DBGM_ON, printf("SituationalSharedData: Updating TIM...\n"));
			waveRadio_destroyRxPacket(timPacket);
			(*iterator)->value = packet;
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
		//TIM wasn't found, so create List node and insert.
		DBG_INFO(DBGM_ON, printf("SituationalSharedData: New TIM Found.\n"));

		WaveRxPacket *newTIM = packet;
		if (newTIM)
		{
			SingleList *newNode = singleList_nodeCreate(newTIM, waveRadio_destroyRxPacket);
			if (newNode)
			{
				singleList_insert(iterator, 0, newNode);
			}
			else
			{
				DBG_ERR(DBGM_ON, printf("SituationalSharedData: Unable to malloc new node to store TIM.\n"));
			}
		}
		else
		{
			DBG_ERR(DBGM_ON, printf("SituationalSharedData: Unable to malloc new space for TIM.\n"));
		}
	}
	lock_unlock(situationalData->lock);
}


void *situationalSender_run(void *arg)
{
	struct timespec lastWaveMessagePurgeTime;
	clock_gettime(CLOCK_MONOTONIC, &lastWaveMessagePurgeTime);
	struct timespec lastSituationalTime;
	clock_gettime(CLOCK_MONOTONIC, &lastSituationalTime);
	struct timespec lastSituationalTIMTime;
	clock_gettime(CLOCK_MONOTONIC, &lastSituationalTIMTime);
	SituationalSender *situationalSender = (SituationalSender *)arg;
	char vehicleId[50];
	if (!situationalSender)
		return NULL;


	while(thread_isRunning(situationalSender->thread))
	{
		struct timespec currentTime;
		clock_gettime(CLOCK_MONOTONIC, &currentTime);

		//Purge old EVA's that haven't updated recently. Only bother to check once a second.
		if (tsDiff(lastWaveMessagePurgeTime, currentTime) > 1.0)
		{
			clock_gettime(CLOCK_MONOTONIC, &lastWaveMessagePurgeTime);
			situationalSender_purgeOldWaveMessages(&situationalSender->data, 5.0);
		}

		if(tsDiff(lastSituationalTime, currentTime) > 1.0/2.0)
		{
			//Create and send UI message containing extracts of EVA messages.
			cJSON *root = cJSON_CreateObject();
			cJSON_AddStringToObject(root, "typeid", "EVAS");
			cJSON *listOfEvas = cJSON_CreateArray();
			lock_lock(situationalSender->data.lock);
			SingleList **iterator = &situationalSender->data.evaList;
			while(*iterator != NULL)
			{
				WaveRxPacket *evaPacket = (*iterator)->value;
				EmergencyVehicleAlert_t *eva = evaPacket->structure;
				cJSON * evaVehicle = cJSON_CreateObject();
				uint32_t vehicleID = dsrc_getEvaVehicleId32(eva);
				sprintf(vehicleId, "%08X", vehicleID);
				cJSON_AddStringToObject(evaVehicle, "typeid", "eva");
				cJSON_AddStringToObject(evaVehicle, "vehID", vehicleId);
				cJSON_AddNumberToObject(evaVehicle, "lat", eva->rsaMsg.positon->lat / 10000000.00);
				cJSON_AddNumberToObject(evaVehicle, "lon", eva->rsaMsg.positon->Long / 10000000.00);
				cJSON_AddNumberToObject(evaVehicle, "typeEvent", eva->rsaMsg.typeEvent);
				cJSON_AddItemToArray(listOfEvas, evaVehicle);
				iterator = &(*iterator)->nextNode;
			}
			lock_unlock(situationalSender->data.lock);
			cJSON_AddItemToObject(root, "eva", listOfEvas);
			ui_sendMessage(root);
			cJSON_Delete(root);

			//Create and send UI message containing extracts of BSM messages.
			root = cJSON_CreateObject();
			cJSON_AddStringToObject(root, "typeid", "BSMS");
			lock_lock(situationalSender->data.lock);
			cJSON *listOfBsms = cJSON_CreateArray();
			iterator = &situationalSender->data.bsmList;
			while(*iterator != NULL)
			{
				WaveRxPacket *bsmPacket = (*iterator)->value;
				BasicSafetyMessage_t *bsm = bsmPacket->structure;
				cJSON * bsmVehicle = cJSON_CreateObject();
				uint32_t vehicleID = dsrc_getBsmVehicleId32(bsm);
				sprintf(vehicleId, "%08X", vehicleID);
				cJSON_AddStringToObject(bsmVehicle, "typeid", "bsm");
				cJSON_AddStringToObject(bsmVehicle, "vehID", vehicleId);
				Latitude_t latitude;
				memcpy(&latitude, bsm->blob1.buf + 7, 4);
				cJSON_AddNumberToObject(bsmVehicle, "lat", latitude / 10000000.00);
				Longitude_t longitude;
				memcpy(&longitude, bsm->blob1.buf + 11, 4);
				cJSON_AddNumberToObject(bsmVehicle, "lon", longitude / 10000000.00);
				cJSON_AddItemToArray(listOfBsms, bsmVehicle);
				iterator = &(*iterator)->nextNode;
			}
			lock_unlock(situationalSender->data.lock);
			cJSON_AddItemToObject(root, "bsm", listOfBsms);
			ui_sendMessage(root);
			cJSON_Delete(root);

			clock_gettime(CLOCK_MONOTONIC, &lastSituationalTime);
		}

		if(tsDiff(lastSituationalTIMTime, currentTime) > 5.0)
		{
			//Create and send UI message containing extracts of TIM messages.
			cJSON *root = cJSON_CreateObject();
			cJSON_AddStringToObject(root, "typeid", "TIMS");
			cJSON *listOfTims = cJSON_CreateArray();
			lock_lock(situationalSender->data.lock);
			SingleList **iterator = &situationalSender->data.timList;
			while(*iterator != NULL)
			{
				cJSON *timJSON = cJSON_CreateObject();
				WaveRxPacket *timPacket = (*iterator)->value;
				TravelerInformation_t *tim = timPacket->structure;
				cJSON *listOfDataFrames = cJSON_CreateArray();
				int i;
				for(i = 0; i < tim->dataFrames.list.count; i++)
				{
					cJSON *dataFrameJSON = cJSON_CreateObject();
					struct TravelerInformation__dataFrames__Member *dataFrame = tim->dataFrames.list.array[i];
					if (dataFrame->content.present == content_PR_advisory)
					{
						int laneType;
						if (dsrc_containsItisCode(&dataFrame->content.choice.advisory, 769))
							laneType = TimLaneType_Closed;
						else if (dsrc_containsItisCode(&dataFrame->content.choice.advisory, 6933))
							laneType = TimLaneType_SpeedRestriction;
						else if (dsrc_containsItisCode(&dataFrame->content.choice.advisory, 531))
							laneType = TimLaneType_Incident;
						else
							laneType = TimLaneType_Other;

						cJSON_AddNumberToObject(dataFrameJSON, "lnT", laneType);
					}
					int j = 0;
					cJSON *listOfRegions = cJSON_CreateArray();
					for(j = 0; j < dataFrame->regions.list.count; j++)
					{
						ValidRegion_t *region = dataFrame->regions.list.array[j];
						if(region->area.present == ValidRegion__area_PR_shapePointSet)
						{
							cJSON *regionJSON = cJSON_CreateObject();
							ShapePointSet_t set = region->area.choice.shapePointSet;
							cJSON_AddNumberToObject(regionJSON, "aLat", ((double ) set.anchor->lat) / 10000000.00);	
							cJSON_AddNumberToObject(regionJSON, "aLon", ((double) set.anchor->Long) / 10000000.00);
							cJSON_AddNumberToObject(regionJSON, "lnW", *(set.laneWidth));
							cJSON_AddNumberToObject(regionJSON, "dir", *((uint8_t *) set.directionality->buf));
							int k = 0;
							cJSON *listOfNodes = cJSON_CreateArray();
							for(k = 0; k < set.nodeList.list.count; k++)
							{
								cJSON *nodeJSON = cJSON_CreateObject();
								double pLatitude, pLongitude;
								dsrc_getShapePointSet(&set, k, &pLatitude, &pLongitude);
								cJSON_AddNumberToObject(nodeJSON, "nLat", pLatitude);
								cJSON_AddNumberToObject(nodeJSON, "nLon", pLongitude);
								cJSON_AddItemToArray(listOfNodes, nodeJSON);
							}
							cJSON_AddItemToObject(regionJSON, "regionNodes", listOfNodes);
							cJSON_AddItemToArray(listOfRegions, regionJSON);
						}
					}
					cJSON_AddItemToObject(dataFrameJSON, "region", listOfRegions);
					cJSON_AddItemToArray(listOfDataFrames, dataFrameJSON);
				}
				cJSON_AddItemToObject(timJSON, "dataFrame", listOfDataFrames);
				cJSON_AddItemToArray(listOfTims, timJSON);
				iterator = &(*iterator)->nextNode;
			}
			lock_unlock(situationalSender->data.lock);
			cJSON_AddItemToObject(root, "tim", listOfTims);
			ui_sendMessage(root);
			cJSON_Delete(root);


			clock_gettime(CLOCK_MONOTONIC, &lastSituationalTIMTime);
		}
		usleep(100000);
	}
	return NULL;	
}


/**
 * Purges old EVA's, BSM's, and TIM's from the EVA, BSM, and TIM lists that are older than the stale threshold (seconds).
 */
void situationalSender_purgeOldWaveMessages(SituationalSharedData *situationalData, double staleThreshold)
{
	int result = 0;
	int length = 0;

	struct timespec currentTime;
	clock_gettime(CLOCK_MONOTONIC, &currentTime);

	lock_lock(situationalData->lock);
	SingleList **iterator = &situationalData->evaList;
	while(*iterator != NULL)
	{
		WaveRxPacket *evaPacket = (*iterator)->value;
		if (!evaPacket || tsDiff(evaPacket->rxTime, currentTime) >= staleThreshold)
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

	lock_unlock(situationalData->lock);
	if (result)
		DBG_INFO(DBGM_ON, printf("situationalSharedData: Purged %d of %d EVA(s)\n", result, length + result));

	result = 0;
	length = 0;

	lock_lock(situationalData->lock);
	iterator = &situationalData->bsmList;
	while(*iterator != NULL)
	{
		WaveRxPacket *bsmPacket = (*iterator)->value;
		if (!bsmPacket || tsDiff(bsmPacket->rxTime, currentTime) >= staleThreshold)
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

	lock_unlock(situationalData->lock);

	if (result)
		DBG_INFO(DBGM_ON, printf("situationalSharedData: Purged %d of %d BSM(s)\n", result, length + result));

	result = 0;
	length = 0;

	lock_lock(situationalData->lock);
	iterator = &situationalData->timList;
	while(*iterator != NULL)
	{
		WaveRxPacket *timPacket = (*iterator)->value;
		if (!timPacket || tsDiff(timPacket->rxTime, currentTime) >= staleThreshold)
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

	lock_unlock(situationalData->lock);

	if (result)
		DBG_INFO(DBGM_ON, printf("situationalSharedData: Purged %d of %d TIM(s)\n", result, length + result));
}
