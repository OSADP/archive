/**
 * @file         Responder.c
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
#include "Responder.h"
#include "RescumeConfig.h"
#include "UiController.h"
#include "GpsAdapter.h"
#include "GpsUBloxDevice.h"
#include "Debug.h"
#include "TimeStamp.h"
#include "DsrcUtils.h"

#include <stdio.h>
#include <string.h>
#include <pthread.h>
#include <endian.h>

#include <EmergencyVehicleAlert.h>
#include <TravelerInformation.h>
#include <AlaCarte.h>

#include <wave.h>

/********************* TYPEDEFS AND STRUCTURES **********************/


/********************* INTERNAL PROTOTYPES **********************/
static Responder *responder_create();
static void responder_destroy(Responder *);
static void *responder_run(void *arg);
static void responder_loadTimFromHexBerFile(Responder *responder, char *filename);
static int responder_buildEvaPacket(Responder *responder, EmergencyVehicleAlert_t *eva);

static cJSON *responder_buildUiMsgFromAcmLists(Responder *responder);
static int responder_insertOrUpdateThreatAcm(Responder *responder, RescumeAlaCartePayload *newAcmPayload, struct timespec *rxTime);
static int responder_insertOrUpdateCollisionAcm(Responder *responder, RescumeAlaCartePayload *newAcmPayload, struct timespec *rxTime);
static void responder_purgeStaleAcms(Responder *responder, double staleTimeout);

extern void buildBSMRequestData();


/********************* FILE GLOBAL VARIABLES **********************/
static pthread_mutex_t singletonLock = PTHREAD_MUTEX_INITIALIZER;
static Responder *sInstance = NULL;


/********************* EXPOSED METHODS **********************/
Responder *responder_getInstance()
{
	pthread_mutex_lock(&singletonLock);
	if (!sInstance)
		sInstance = responder_create();
	pthread_mutex_unlock(&singletonLock);
	return sInstance;
}

void responder_destroyInstance()
{
	pthread_mutex_lock(&singletonLock);
	if (sInstance)
		responder_destroy(sInstance);
	sInstance = NULL;
	pthread_mutex_unlock(&singletonLock);
}

void responder_start(Responder *responder)
{
	if (!responder)
		return;
	
	DBG_INFO(DBGM_RES, printf("Responder: Starting Responder Module\n"));
	thread_start(responder->thread, responder_run, responder);
}

void responder_stop(Responder *responder)
{
	if (!responder)
		return;
	
	DBG_INFO(DBGM_RES, printf("Responder: Stopping Responder Module\n"));
	thread_join(responder->thread);
}

void responder_processAndDestroyWaveMessage(Responder *responder, WaveRxPacket *packet)
{
	if (!responder)
	{
		waveRadio_destroyRxPacket(packet);
		return;
	}

	RescumeConfig config;
	rescumeConfig_get(&config);

	lock_lock(responder->dataLock);

	if (packet->type == &asn_DEF_AlaCarte)
	{
		AlaCarte_t *acm = packet->structure;

		if (acm->data.item7_87)
		{
			if (acm->data.item7_87->size == sizeof(RescumeAlaCartePayload))
			{
				RescumeAlaCartePayload newAcmPayload;
				memcpy(&newAcmPayload, acm->data.item7_87->buf, acm->data.item7_87->size);

				uint32_t newAcmResponderId = be32toh(*((uint32_t*)newAcmPayload.responderId));

				int changed = 0;
				if (newAcmResponderId == config.appVehicleId)
				{
					changed |= responder_insertOrUpdateCollisionAcm(responder, &newAcmPayload, &packet->rxTime);
				}
				else if (newAcmResponderId == 0)
				{
					changed |= responder_insertOrUpdateThreatAcm(responder, &newAcmPayload, &packet->rxTime);
				}

				if (changed)
				{
					cJSON *root = responder_buildUiMsgFromAcmLists(responder);
					assert(root);
					ui_sendMessage(root);
					cJSON_Delete(root);
				}

			}
			else
			{
				DBG_ERR(DBGM_RES, printf("Responder: Received Ala Carte with incorrect payload length (expected %d, got %d).\n", sizeof(RescumeAlaCartePayload), acm->data.item7_87->size));
			}
		}
		else
		{
			DBG_ERR(DBGM_RES, printf("Responder: Ala Carte message doesn't have any payload data (item 7-87).\n"));
		}
		
		waveRadio_destroyRxPacket(packet);
	}
	else if(packet->type == &asn_DEF_TravelerInformation)
	{
		lock_lock(responder->dataLock);
		if (responder->timInfo.msgStruct)
		{
			if (dsrc_hasSameTimPacketId(responder->timInfo.msgStruct, packet->structure))
			{
				clock_gettime(CLOCK_MONOTONIC, &responder->timInfo.differentTimRxTime);
			}
		}
		lock_unlock(responder->dataLock);

		waveRadio_destroyRxPacket(packet);
	}
	else
	{
		waveRadio_destroyRxPacket(packet);
	}


	lock_unlock(responder->dataLock);
}

int responder_processUiMsg(Responder *responder, cJSON *root)
{
	if (!responder)
		return 0;
	
	cJSON *typeId = cJSON_GetObjectItem(root,"typeid");

	if (typeId == NULL || typeId->type != cJSON_String)
		return 0;

	if (!strcmp(typeId->valuestring, "EVA"))
	{

		lock_lock(responder->dataLock);
		cJSONxtra_tryGetBool(root, "enabled", &responder->evaInfo.enabled);
		cJSONxtra_tryGetInt(root, "itis", &responder->evaInfo.itisCode);

		if (responder->evaInfo.enabled && !responder->evaInfo.itisCode)
		{
			responder->evaInfo.enabled = 0;
			DBG_ERR(DBGM_RES, printf("Responder: Can not enable EVA's without an ITIS code.\n"));
		}

		RescumeConfig config;
		rescumeConfig_lockForEdit(&config);
		if (responder->evaInfo.enabled)
		{
			if (config.appVehicleIdLock == VehicleIdLock_noLock)
				config.appVehicleIdLock = VehicleIdLock_temporary;
		}
		else if (config.appVehicleIdLock == VehicleIdLock_temporary)
		{
			config.appVehicleIdLock = VehicleIdLock_noLock;
		}
		rescumeConfig_updateAndUnlock(&config);

		lock_unlock(responder->dataLock);
		return 1;
	}
	else if (!strcmp(typeId->valuestring, "TIM"))
	{
		lock_lock(responder->dataLock);
		cJSONxtra_tryGetBool(root, "enabled", &responder->timInfo.enabled);

		cJSON *payload = cJSON_GetObjectItem(root, "payload");
		if (payload && payload->type == cJSON_String)
		{
			int payloadLength = strlen(payload->valuestring)/2;
			if (payloadLength <= sizeof(responder->timInfo.encodedMsg))
			{
				int i;
				for(i = 0; i < payloadLength; i++)
					sscanf(payload->valuestring + (2 * i), "%2hhx", &responder->timInfo.encodedMsg[i]);
				responder->timInfo.encodedMsgLength = payloadLength;

				DBG(DBGM_RES, printf("Responder: Saved new encoded TIM message (length %d bytes).\n", payloadLength));

				if (responder->timInfo.msgStruct)
				{
					ASN_STRUCT_FREE(asn_DEF_TravelerInformation, responder->timInfo.msgStruct);
					responder->timInfo.msgStruct = NULL;
				}
				asn_dec_rval_t rval = ber_decode(NULL, &asn_DEF_TravelerInformation, (void **)&responder->timInfo.msgStruct, responder->timInfo.encodedMsg, responder->timInfo.encodedMsgLength);
				if (rval.code == RC_OK && responder->timInfo.msgStruct)
				{
					//asn_fprint(stdout, &asn_DEF_TravelerInformation, responder->timInfo.msgStruct);
					
					if (responder->timInfo.msgStruct->packetID)
					{
						char buf[100];
						char len = 0;
						len += snprintf(buf + len, sizeof(buf) - len, "Responder: New TIM PackedID: 0x");
						int i;
						for(i = 0; i < responder->timInfo.msgStruct->packetID->size; i++)
							len += snprintf(buf + len, sizeof(buf) - len, "%02x", responder->timInfo.msgStruct->packetID->buf[i]);

						DBG_INFO(DBGM_RES, printf("%s\n", buf));

					}
					else
					{
						DBG_WARN(DBGM_RES, printf("Responder: New TIM doesn't contain a PacketID.\n"));
					}
				}
				else
				{
					DBG_ERR(DBGM_RES, printf("Responder: Failed to decode new TIM message.\n"));
					responder->timInfo.msgStruct = NULL;
					responder->timInfo.enabled = 0;
				}
			}
			else
			{
				DBG_ERR(DBGM_RES, printf("Responder: Unable to save new TIM message. Length is too large (%d bytes).\n", payloadLength));
				responder->timInfo.enabled = 0;
			}
		}

		if (!responder->timInfo.enabled)
			responder->timInfo.active = 0;

		lock_unlock(responder->dataLock);
		return 1;
	}

	return 0;
}

int responder_isEvasEnabled(Responder *responder)
{
	return responder->evaInfo.enabled != 0;
}

int responder_isTimsEnabled(Responder *responder)
{
	return responder->timInfo.active;
}

/********************* INTERNAL METHODS **********************/
Responder *responder_create()
{
	Responder *results = (Responder *)calloc(1, sizeof(Responder));
	if (!results)
		return NULL;

	if (!(results->thread = thread_create()))
	{
		responder_destroy(results);
		return NULL;
	}

	if (!(results->dataLock = lock_create()))
	{
		responder_destroy(results);
		return NULL;
	}

	RescumeConfig rescumeConfig;
	rescumeConfig_get(&rescumeConfig);
	DBG_INFO(DBGM_RES, printf("Responder: Creating new Responder Module. Using RESPONDER ID: 0x%08x\n", rescumeConfig.appVehicleId));

	
	clock_gettime(CLOCK_MONOTONIC, &results->timInfo.differentTimRxTime);
	//results->timInfo.startDelay = (rand() / (double)RAND_MAX) * 3.0;

	if (rescumeConfig.responderPreloadedTimFile[0] != '\0')
	{
		responder_loadTimFromHexBerFile(results, rescumeConfig.responderPreloadedTimFile);
	}
	return results;
}

void responder_destroy(Responder *responder)
{
	if (!responder)
		return;

	responder_stop(responder);
	if (responder->thread)
		thread_destroy(responder->thread);

	if (responder->dataLock)
		lock_destroy(responder->dataLock);

	if (responder->timInfo.msgStruct)
		ASN_STRUCT_FREE(asn_DEF_TravelerInformation, responder->timInfo.msgStruct);
	responder->timInfo.msgStruct = NULL;

	singleList_destroy(&responder->activeThreatAcms);

	free(responder);
}

void *responder_run(void *arg)
{
	Responder *responder = (Responder *)arg;

	if (!responder)
		return NULL;

	RescumeConfig config;

	EmergencyVehicleAlert_t eva;

	struct timespec lastBsmBroadcastTime, lastEvaBroadcastTime, lastTimBroadcastTime, lastAcmPurgeTime, lastAlertCountTime, lastGpsUpdateTime;
	clock_gettime(CLOCK_MONOTONIC, &lastBsmBroadcastTime);
	clock_gettime(CLOCK_MONOTONIC, &lastEvaBroadcastTime);
	clock_gettime(CLOCK_MONOTONIC, &lastTimBroadcastTime);
	clock_gettime(CLOCK_MONOTONIC, &lastAcmPurgeTime);
	clock_gettime(CLOCK_MONOTONIC, &lastAlertCountTime);
	clock_gettime(CLOCK_MONOTONIC, &lastGpsUpdateTime);

	while(thread_isRunning(responder->thread))
	{
		rescumeConfig_get(&config);

		struct timespec currentTime;
		clock_gettime(CLOCK_MONOTONIC, &currentTime);

		lock_lock(responder->dataLock);

		if (responder->evaInfo.enabled && tsDiff(lastEvaBroadcastTime, currentTime) >= 1.0/config.dsrcEvaMaxRate && config.dsrcEvaMaxRate > 0)
		{	
			clock_gettime(CLOCK_MONOTONIC, &lastEvaBroadcastTime);
			if (responder_buildEvaPacket(responder, &eva))
				waveRadio_txPacket(&asn_DEF_EmergencyVehicleAlert, &eva);
			else
				DBG_ERR(DBGM_RES, printf("Responder: Error building EVA Packet.\n"));
		}

		if (!responder->evaInfo.enabled && tsDiff(lastBsmBroadcastTime, currentTime) >= 1.0/config.dsrcBsmMaxRate && config.dsrcBsmMaxRate > 0)
		{
			clock_gettime(CLOCK_MONOTONIC, &lastBsmBroadcastTime);
			buildBSMRequestData();
		}

		if (tsDiff(lastGpsUpdateTime, currentTime) >= 20.0)
		{
			clock_gettime(CLOCK_MONOTONIC, &lastGpsUpdateTime);

			GpsDevice *gps = gps_getDefaultDevice();
			// Do not set in stationary mode
			/*
			if (responder->timInfo.enabled)
			{
				gpsUBloxDevice_setCfgRateInfo(gps, 1000);
				usleep(1000);
				gpsUBloxDevice_setDynamicMode(gps, UBloxDynamicMode_Stationary);
				usleep(1000);
				gpsUBloxDevice_setStaticHoldThreshold(gps, 20);
			}
			else
			{
			*/
				gpsUBloxDevice_setCfgRateInfo(gps, 200);
				usleep(1000);
				gpsUBloxDevice_setDynamicMode(gps, UBloxDynamicMode_Automotive);
				usleep(1000);
				gpsUBloxDevice_setStaticHoldThreshold(gps, 0);
			//}
			
			usleep(1000);
			gpsUBloxDevice_pollForNav5Info(gps);
		}

		/*if (responder->timInfo.enabled && tsDiff(lastTimBroadcastTime, currentTime) >= 1.0/config.dsrcTimMaxRate && config.dsrcTimMaxRate > 0 
			&& tsDiff(responder->timInfo.differentTimRxTime, currentTime) >= ((responder->timInfo.active ? 0 : 10.0) + responder->timInfo.startDelay))*/
		if (responder->timInfo.enabled && tsDiff(lastTimBroadcastTime, currentTime) >= 1.0/config.dsrcTimMaxRate && config.dsrcTimMaxRate > 0 
			&& tsDiff(responder->timInfo.differentTimRxTime, currentTime) >= 3.0)
		{
			clock_gettime(CLOCK_MONOTONIC, &lastTimBroadcastTime);
			if (responder->timInfo.msgStruct) //TODO this should probably just be an assert..
			{
				responder->timInfo.active = 1;
				waveRadio_txRawPacket(responder->timInfo.encodedMsg, responder->timInfo.encodedMsgLength);
			}
			else
			{
				responder->timInfo.enabled = 0;
				DBG_ERR(DBGM_RES, printf("Responder: Unable to transmit TIM. No valid message loaded.\n"));
			}
		}
		else if (tsDiff(lastTimBroadcastTime, currentTime) > 10.0)
		{
			responder->timInfo.active = 0;
		}

		if (tsDiff(lastAlertCountTime, currentTime) >= 1.0/config.responderMaxThreatMessageRate && config.responderMaxThreatMessageRate > 0)
		{
			clock_gettime(CLOCK_MONOTONIC, &lastAlertCountTime);
			cJSON *root = responder_buildUiMsgFromAcmLists(responder);
			assert(root);
			ui_sendMessage(root);
			cJSON_Delete(root);
		}

		if (tsDiff(lastAcmPurgeTime, currentTime) > 1.0)
		{
			clock_gettime(CLOCK_MONOTONIC, &lastAcmPurgeTime);
			responder_purgeStaleAcms(responder, config.responderStaleAcmTimeout);
		}

		lock_unlock(responder->dataLock);
		
		usleep(10000);
	}

	ASN_STRUCT_FREE_CONTENTS_ONLY(asn_DEF_EmergencyVehicleAlert, &eva);
	return NULL;
}

void responder_loadTimFromHexBerFile(Responder *responder, char *filename)
{
	FILE* fd = fopen(filename, "r");
	
	if(!fd)
	{
		DBG_FATAL(printf("Responder: Unable to open tim file '%s'\n", filename));
	}
	else
	{
		char buffer[5000];
		while(fgets(buffer, sizeof(buffer), fd))
		{
			cJSON *results = cJSON_CreateObject();
			assert(results != NULL);

			cJSON_AddStringToObject(results, "typeid", "TIM");
			cJSON_AddStringToObject(results, "payload", buffer);
			cJSON_AddTrueToObject(results, "enabled");

			responder_processUiMsg(responder, results);
			cJSON_Delete(results);


			results = cJSON_CreateObject();
			assert(results != NULL);

			cJSON_AddStringToObject(results, "typeid", "EVA");
			cJSON_AddNumberToObject(results, "itis", 1);
			cJSON_AddTrueToObject(results, "enabled");

			responder_processUiMsg(responder, results);
			cJSON_Delete(results);
		}
		
		fclose(fd);
	}
}

int responder_buildEvaPacket(Responder *responder, EmergencyVehicleAlert_t *eva)
{
	if (!responder)
		return 0;

	if (!eva)
		return 0;
	static int pktnum = 0;

	GPSData gpsData;
	gpsDevice_getData(gps_getDefaultDevice(), &gpsData);

	RescumeConfig config;
	rescumeConfig_get(&config);

	/**** MSG ID = EVA ****/
	if (!eva->msgID.buf)
		if (!(eva->msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t))))
			return 0;
	eva->msgID.size = sizeof(uint8_t);
	eva->msgID.buf[0] = DSRCmsgID_emergencyVehicleAlert;
	
	/**** RSA MSG ID = RSA ****/
	if (!eva->rsaMsg.msgID.buf)
		if (!(eva->rsaMsg.msgID.buf = (uint8_t *) calloc(1, sizeof(uint8_t))))
			return 0;
	eva->rsaMsg.msgID.size = sizeof(uint8_t);
	eva->rsaMsg.msgID.buf[0] = DSRCmsgID_roadSideAlert;

	/**** RSA MSG Count ****/
	eva->rsaMsg.msgCnt = pktnum % 127;

	/**** RSA ITIS Code ****/
	eva->rsaMsg.typeEvent = responder->evaInfo.itisCode;

	/**** RSA Position ****/
	if (!eva->rsaMsg.positon)
		if (!(eva->rsaMsg.positon = (FullPositionVector_t *) calloc(1, sizeof(FullPositionVector_t))))
			return 0;
	eva->rsaMsg.positon->lat = dsrc_convToStructLatitude(gpsData.latitude);
	eva->rsaMsg.positon->Long = dsrc_convToStructLongitude(gpsData.longitude);

	/**** RSA Position.Elevation ****/
	if (!eva->rsaMsg.positon->elevation)
		if (!(eva->rsaMsg.positon->elevation = (Elevation_t *) calloc(1, sizeof(Elevation_t))))
			return 0;
	if (!eva->rsaMsg.positon->elevation->buf)
		if (!(eva->rsaMsg.positon->elevation->buf = (uint8_t *) calloc(2, sizeof(uint8_t))))
			return 0;
	eva->rsaMsg.positon->elevation->size = 2 * sizeof(uint8_t);
	*((uint16_t *)eva->rsaMsg.positon->elevation->buf) = htobe16(dsrc_convToStructElevation(gpsData.altitude));

	/**** EVA ID ****/
	if (!eva->id)
		if (!(eva->id = (TemporaryID_t*) calloc(1, sizeof(TemporaryID_t))))
			return 0;
	if (!eva->id->buf)
		if (!(eva->id->buf = (uint8_t *) calloc(4, sizeof(uint8_t))))
			return 0;
	eva->id->size = 4 * sizeof(uint8_t);
	*((uint32_t*)(eva->id->buf)) = htobe32(config.appVehicleId);

	pktnum++;
	return 1;
}

cJSON *responder_buildUiMsgFromAcmLists(Responder *responder)
{
	cJSON *results = cJSON_CreateObject();
	if (!results)
		return NULL;

	lock_lock(responder->dataLock);

	uint8_t threatLevels[RescumeThreatLevel__length - RescumeThreatLevel_reset] = {0};

	SingleList *iterator = responder->activeThreatAcms;
	while (iterator)
	{
		AcmMetaData *acm = iterator->value;
		assert(acm != NULL);
		assert(acm->payload.tLevel >= RescumeThreatLevel_reset && acm->payload.tLevel < RescumeThreatLevel__length);

		threatLevels[acm->payload.tLevel - RescumeThreatLevel_reset]++;

		iterator = iterator->nextNode;
	}

	iterator = responder->activeCollisionAcms;
	while (iterator)
	{
		AcmMetaData *acm = iterator->value;
		assert(acm != NULL);
		assert(acm->payload.tLevel == RescumeThreatLevel_reset || acm->payload.tLevel == RescumeThreatLevel_collision);

		if (acm->payload.tLevel == RescumeThreatLevel_collision)
			threatLevels[acm->payload.tLevel - RescumeThreatLevel_reset]++;

		iterator = iterator->nextNode;
	}

	cJSON_AddStringToObject(results, "typeid", "THREAT");
	int i;
	for(i = RescumeThreatLevel_noThreat; i < RescumeThreatLevel__length; i++)
	{
		char buf[100];
		snprintf(buf, sizeof(buf), "tlevel%dcount", i);
		cJSON_AddNumberToObject(results, buf, threatLevels[i - RescumeThreatLevel_reset]);
	}

	lock_unlock(responder->dataLock);
	
	return results;
}

int responder_insertOrUpdateThreatAcm(Responder *responder, RescumeAlaCartePayload *newAcmPayload, struct timespec *rxTime)
{
	lock_lock(responder->dataLock);
	int listChanged = 0;

	uint32_t newAcmOncomingId = be32toh(*((uint32_t*)newAcmPayload->oncomingId));
	uint32_t newAcmResponderId = be32toh(*((uint32_t*)newAcmPayload->responderId));

	assert(newAcmOncomingId != 0);
	assert(newAcmResponderId == 0);
	assert(newAcmPayload->tLevel >= RescumeThreatLevel_reset && newAcmPayload->tLevel < RescumeThreatLevel_collision);

	AcmMetaData *newAcm = (AcmMetaData *)calloc(1, sizeof(AcmMetaData));
	assert(newAcm != NULL);
	newAcm->payload = *newAcmPayload;
	newAcm->rxTime = *rxTime;

	SingleList *newNode = singleList_nodeCreate(newAcm, free);
	assert(newNode != NULL);

	SingleList **iterator = &responder->activeThreatAcms;

	while(*iterator)
	{
		AcmMetaData *acm = (*iterator)->value;
		assert(acm != NULL);

		uint32_t oldAcmOncomingId = be32toh(*((uint32_t*)acm->payload.oncomingId));
		if (oldAcmOncomingId == newAcmOncomingId)
		{
			if (acm->payload.tLevel != newAcm->payload.tLevel)
				listChanged = 1;

			singleList_replaceAndDestroy(iterator, 0, newNode);
			break;
		}
		else
		{
			iterator = &(*iterator)->nextNode;
		}

	}
	if (!*iterator)
	{
		singleList_insert(iterator, 0, newNode);
		listChanged = 1;
	}

	lock_unlock(responder->dataLock);
	return listChanged;
}

int responder_insertOrUpdateCollisionAcm(Responder *responder, RescumeAlaCartePayload *newAcmPayload, struct timespec *rxTime)
{
	lock_lock(responder->dataLock);
	int listChanged = 0;

	uint32_t newAcmOncomingId = be32toh(*((uint32_t*)newAcmPayload->oncomingId));
	uint32_t newAcmResponderId = be32toh(*((uint32_t*)newAcmPayload->responderId));

	assert(newAcmOncomingId != 0);
	assert(newAcmResponderId != 0);
	assert(newAcmPayload->tLevel == RescumeThreatLevel_reset || newAcmPayload->tLevel == RescumeThreatLevel_collision);

	if (newAcmPayload->tLevel == RescumeThreatLevel_reset)
	{
		SingleList **iterator = &responder->activeCollisionAcms;
		while(*iterator)
		{
			AcmMetaData *acm = (*iterator)->value;
			assert(acm != NULL);

			uint32_t oldAcmOncomingId = be32toh(*((uint32_t*)acm->payload.oncomingId));
			if (oldAcmOncomingId == newAcmOncomingId)
			{
				listChanged = 1;
				singleList_removeAndDestroy(iterator, 0);
				break;
			}
			else
			{
				iterator = &(*iterator)->nextNode;
			}
		}
	} 
	else
	{
		AcmMetaData *newAcm = (AcmMetaData *)calloc(1, sizeof(AcmMetaData));
		assert(newAcm != NULL);
		newAcm->payload = *newAcmPayload;
		newAcm->rxTime = *rxTime;

		SingleList *newNode = singleList_nodeCreate(newAcm, free);
		assert(newNode != NULL);

		SingleList **iterator = &responder->activeCollisionAcms;

		while(*iterator)
		{
			AcmMetaData *acm = (*iterator)->value;
			assert(acm != NULL);

			uint32_t oldAcmOncomingId = be32toh(*((uint32_t*)acm->payload.oncomingId));
			if (oldAcmOncomingId == newAcmOncomingId)
			{
				if (acm->payload.tLevel != newAcm->payload.tLevel)
					listChanged = 1;

				singleList_replaceAndDestroy(iterator, 0, newNode);
				break;
			}
			else
			{
				iterator = &(*iterator)->nextNode;
			}

		}
		if (!*iterator)
		{
			singleList_insert(iterator, 0, newNode);
			listChanged = 1;
		}
	}

	lock_unlock(responder->dataLock);
	return listChanged;
}

void responder_purgeStaleAcms(Responder *responder, double staleTimeout)
{
	assert(responder != NULL);
	
	lock_lock(responder->dataLock);

	struct timespec currentTime;
	clock_gettime(CLOCK_MONOTONIC, &currentTime);

	int changed = 0;
	SingleList **iterator = &responder->activeThreatAcms;
	while(*iterator)
	{
		AcmMetaData *acm = (*iterator)->value;

		if (tsDiff(acm->rxTime, currentTime) >= staleTimeout)
		{
			if (acm->payload.tLevel > RescumeThreatLevel_reset)
				changed = 1;
			singleList_removeAndDestroy(iterator, 0);
		}
		else
		{
			iterator = &(*iterator)->nextNode;
		}
	}

	iterator = &responder->activeCollisionAcms;
	while(*iterator)
	{
		AcmMetaData *acm = (*iterator)->value;

		if (tsDiff(acm->rxTime, currentTime) >= staleTimeout)
		{
			if (acm->payload.tLevel > RescumeThreatLevel_reset)
				changed = 1;
			singleList_removeAndDestroy(iterator, 0);
		}
		else
		{
			iterator = &(*iterator)->nextNode;
		}
	}

	if (changed)
	{
		cJSON *root = responder_buildUiMsgFromAcmLists(responder);
		assert(root);
		ui_sendMessage(root);
		cJSON_Delete(root);
	}

	lock_unlock(responder->dataLock);
}
