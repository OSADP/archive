#include "TimProcessor.h"
#include "TimFilter.h"
#include "QWarnController.h"
#include "Map.h"
#include "TimeStamp.h"
#include "ConfigFile.h"

#include "asnwave.h"
#include "wave.h"
#include "tool_def.h"
#include "ValidRegion.h"
#include <stdbool.h>


extern asn_TYPE_descriptor_t *asn_pdu_collection[];

const char* MESSAGE_ORIGIN_FIELD = "MessageOrigin";
const long BROADCAST_RANGE_MI = 2;

static unsigned long _timprocRelayCount = 0;

void timprocProcessTim(TimProcessor* pInstance, TravelerInformation_t* pMessage);
void timprocRelayTim(TimProcessor* pInstance, TravelerInformation_t* pMessage);
bool isQAlert(const char* alertMessage);
bool isSpeedHarm(const char* alertMessage);
bool isV2VQAlert(const char* alertMessage);
bool isValidTimFrame(TravelerInformation_t* pMessage, int frameIndex);
double convertHeadingSliceToRad(uint16 headingSlice);

TimProcessor* timprocCreate(int incomingQueueMaxLength, int outgoingQueueMaxLength, int filterSize)
{
    TimProcessor* pInstance = (TimProcessor*)malloc(sizeof(TimProcessor));
    
    pInstance->pIncomingQueue = msgQueueCreate(incomingQueueMaxLength);
    pInstance->pFilter = timfCreate(filterSize);
    pInstance->pAlertCallback = NULL;
    
    return pInstance;
}

void timprocDestroy(TimProcessor* pInstance)
{
    msgQueueDestroy(pInstance->pIncomingQueue);
    timfDestroy(pInstance->pFilter);
    
    free(pInstance);
    
    return;
}

void timprocSetAlertCallback(TimProcessor* pInstance, OnRecvAlert_t pCallback)
{
    pInstance->pAlertCallback = pCallback;
    
    return;
}

int timprocPushMessage(TimProcessor* pInstance, TravelerInformation_t* pMessage)
{
    msgQueuePushFront(pInstance->pIncomingQueue, pMessage, WSMMSG_TIM);
    
    return;
}

void timprocProcessMessages(TimProcessor* pInstance, double maxTimeToProcess_s)
{
    void* pMessage = NULL;
    unsigned short messageType = 0;
    struct timespec tStart={0,0}, tEnd={0,0};
    double timediff_ms = 0.0;
    
 //   printf("%i\tTimProcessor: Incoming = %i\n", time(NULL), pInstance->pIncomingQueue->count);
    
    clock_gettime(CLOCK_MONOTONIC, &tStart);
    while(msgQueuePopBack(pInstance->pIncomingQueue, &pMessage, &messageType))
    {
        if(timfIsNewMessage(pInstance->pFilter, pMessage))
        {
            timprocProcessTim(pInstance, (TravelerInformation_t*)pMessage);
            timprocRelayTim(pInstance, (TravelerInformation_t*)pMessage);
        }
        else
        {            
            printf("TimProcessor: Throwing out TIM already received.\n");
        }
        
        msgQueueFreeMessage(pMessage, messageType); 
        
        clock_gettime(CLOCK_MONOTONIC, &tEnd);
        
        double timediff_s = tsToSeconds(tsSubtract(tEnd, tStart));
        
        if(timediff_s > maxTimeToProcess_s)
        {
            break;
        }
    }
    
    return;
}

void timprocProcessTim(TimProcessor* pInstance, TravelerInformation_t* pMessage)
{
    time_t curr = time(NULL);
    printf("%i\tTimProcessor: Processing TIM dataframes; count: %d\n", curr, pMessage->dataFrames.list.count);    
    
    AlertMessage minimumQAlert;
    minimumQAlert.completion_pct = 100.0;
    minimumQAlert.length_m = 0.0;
    minimumQAlert.message[0] = 0;
    
    AlertMessage minimumSpeedHarmAlert;
    minimumSpeedHarmAlert.completion_pct = 100.0;
    minimumSpeedHarmAlert.length_m = 0.0;
    minimumSpeedHarmAlert.message[0] = 0;
    
    bool gotQAlert = false;
    bool gotSpeedHarmAlert = false;
    
    int i, j;
    for(i=0; i<pMessage->dataFrames.list.count; i++)
    {
        if(!isValidTimFrame(pMessage, i))
        {
            continue;
        }
        
        char* alertMsg = pMessage->dataFrames.list.array[i]->content.choice.advisory.list.array[0]->item.choice.text.buf;
        
        printf("TimProcessor: Alert = %s\n", alertMsg);
        
        if(isV2VQAlert(alertMsg))
        {
            ValidRegion_t* pValidRegion = pMessage->dataFrames.list.array[i]->regions.list.array[0];
            
            if(pValidRegion->area.present != ValidRegion__area_PR_circle)
            {
                printf("TimProcessor: Invalid area in V2V alert. Skipping Frame\n");
            }
            
            uint16 headingSlice = *((uint16*)pValidRegion->direction.buf);
            double alertHeading_rad = convertHeadingSliceToRad(headingSlice) * DEG2RAD;
            double alertLat_deg = pValidRegion->area.choice.circle.center.lat / BSM_BLOB1_GEO_POS_UNIT_CONV_DEG;
            double alertLon_deg = pValidRegion->area.choice.circle.center.Long / BSM_BLOB1_GEO_POS_UNIT_CONV_DEG;
            double alertDistance_m = pValidRegion->area.choice.circle.raduis.choice.miles * MILES2METERS;
            double alertEasting_m = 0.0;
            double alertNorthing_m = 0.0;
            uint8 zone, band;
            
            if(alertHeading_rad == -1.0)
            {
                printf("TimProcessor: Invalid heading slice in V2V alert. Skipping frame.\n");
                continue;
            }
            
            GeoCoordConverter* pGeoConv = geoConvCreate();
            
            if(!geoConvGeodetic2Utm(
                pGeoConv,
                alertLat_deg * DEG2RAD, alertLon_deg * DEG2RAD, 
                &zone, &band, 
                &alertEasting_m, &alertNorthing_m))
            {
                printf("TimProcessor: Could not convert coords in origin dataframe to UTM!\n");
            }
            else
            {
            
                VehicleState currentVehicleState;
                qwarnGetCurrentVehicleState(&currentVehicleState);
                
                Vector currentPos = {currentVehicleState.easting_m, currentVehicleState.northing_m};
                Vector alertPos = {alertEasting_m, alertNorthing_m};
                
                Vector alertToCurrent;
                vecSub(&currentPos, &alertPos, &alertToCurrent);
                
                Vector alertHeading;
                mapHeadingToVector(alertHeading_rad, &alertHeading);
                
                Vector alertToCurrentDir;
                vecNormalize(&alertToCurrent, &alertToCurrentDir);
                
                //printf("TimProcessor: CurrentPos = {%f, %f}\n", currentVehicleState.easting_m, currentVehicleState.northing_m);
                //printf("TimProcessor: AlertPos =   {%f, %f}\n", alertEasting_m, alertNorthing_m);
                //printf("TimProcessor: Alert Heading = %f\n", alertHeading_rad);
                //printf("TimProcessor: Distance = %f\n", vecMag(&alertToCurrent));
                //printf("TimProcessor: CurrentHeading = %f\n", currentVehicleState.heading_rad);
                
                Vector currHeading;
                mapHeadingToVector(currentVehicleState.heading_rad, &currHeading);
                
                if(
                    vecMag(&alertToCurrent) < alertDistance_m &&    // Within radius
                    vecDot(&alertHeading, &currHeading) > 0.1 &&    // Heading same direction as alert
                    vecDot(&currHeading, &alertToCurrentDir) < 0.0 && // Alert is ahead of us
                    (currentVehicleState.speed_m_s > cfGetConfigFile()->queuedSpeedThreshold_m_s) && // moving
                    !qwarnIsQueued()									// Not queued

                )
                {
                    printf("TimProcessor: V2V Alert applies.\n");
                
                    AlertMessage v2vAlert;
                    v2vAlert.completion_pct = 1.0 - (vecMag(&alertToCurrent) / alertDistance_m);
                    v2vAlert.length_m = alertDistance_m;
                    strBufferCopy(v2vAlert.message, alertMsg, ALERT_MSG_MAX_LEN);
                    printf("TimProcessor: v2vAlert.message = %s\n", v2vAlert.message);
                    if (pInstance->pAlertCallback)
                        (*pInstance->pAlertCallback)(&v2vAlert);
                }
                else
                {
                    printf("TimProcessor: V2V Alert DOES NOT apply.\n");
                }
            }
            
            geoConvDestroy(pGeoConv);
        }
        else // TME Q-Alert
        {            
            RouteApproach approach = {0};
            
            if(mapRegionListToRouteApproach(
                pMessage->dataFrames.list.array[i]->regions.list.array, 
                pMessage->dataFrames.list.array[i]->regions.list.count, 
                &approach
            ))
            {
                printf("TimProcessor: Processing region...\n");
                //mapPrintApproach(&approach);
                  
                VehicleState currentVehicleState;
                qwarnGetCurrentVehicleState(&currentVehicleState);
                
                PositionHeading currPosHead = 
                { 
                    {currentVehicleState.easting_m, currentVehicleState.northing_m}, 
                    currentVehicleState.heading_rad
                };
                
                double complete_pct = 0.0;
            
                if(mapFindPositionAlongApproach(
                    &currPosHead,
                    &approach,
                    &complete_pct
                ))
                {
                    printf("TimProcessor: Region processed.\n");
                    
                    if(isQAlert(alertMsg))
                    {
                        if(complete_pct <= minimumQAlert.completion_pct)
                        {
                            minimumQAlert.completion_pct = complete_pct;
                            minimumQAlert.length_m = approach.length_m;
                            strBufferCopy(minimumQAlert.message, alertMsg, ALERT_MSG_MAX_LEN);
                            
                            gotQAlert = true;
                        } 
                    }
                    else if(isSpeedHarm(alertMsg))
                    {
                        if(complete_pct <= minimumSpeedHarmAlert.completion_pct)
                        {
                            minimumSpeedHarmAlert.completion_pct = complete_pct;
                            minimumSpeedHarmAlert.length_m = approach.length_m;
                            strBufferCopy(minimumSpeedHarmAlert.message, alertMsg, ALERT_MSG_MAX_LEN);
                            
                            gotSpeedHarmAlert = true;
                        } 
                    }                                          
                }
            }
        
            // Clean up approach
            if(approach.segmentCount > 0)
            {
                free(approach.pSegments);
            }
        }
    }
    
    if(gotQAlert && pInstance->pAlertCallback)
    {                    
        printf(
            "%i\tTimProcessor: Alert \"%s\" applies. Length=%fm. Completion=%f%%, Traveled=%f\n", 
            time(NULL), 
            minimumQAlert.message, 
            minimumQAlert.length_m, 
            minimumQAlert.completion_pct * 100.0, 
            minimumQAlert.length_m * minimumQAlert.completion_pct
        ); 
    
        (*pInstance->pAlertCallback)(&minimumQAlert);
        printf("%i\tTimProcessor: Alert Callback finished for Q-Alert!\n", time(NULL));
    }
    
    if(gotSpeedHarmAlert && pInstance->pAlertCallback)
    {
        printf(
            "%i\tTimProcessor: Alert \"%s\" applies. Length=%fm. Completion=%f%%, Traveled=%f\n", 
            time(NULL), 
            minimumSpeedHarmAlert.message, 
            minimumSpeedHarmAlert.length_m, 
            minimumSpeedHarmAlert.completion_pct * 100.0, 
            minimumSpeedHarmAlert.length_m * minimumSpeedHarmAlert.completion_pct
        ); 
    
        (*pInstance->pAlertCallback)(&minimumSpeedHarmAlert);
        printf("%i\tTimProcessor: Alert Callback finished for SpeedHarm!\n", time(NULL));
    }
    
    return;
}
void timprocRelayTim(TimProcessor* pInstance, TravelerInformation_t* pMessage)
{
    _timprocRelayCount++;

    const int txChan            = 172; // Default TxPkt Channel 255
    const int data_rateidx      = 3;
    const int app_txpower       = 14;
    const uint8_t qpriority     = 2;
    const uint32_t app_psid     = 32;
    const uint8_t securityType  = AsmOpen; // Default No Security
    const uint8_t app_wsmps     = 0;

    WSMRequest request;
    
    // Fill request header information
    {
        request.chaninfo.channel    = txChan;
	    request.chaninfo.rate       = data_rateidx;
	    request.chaninfo.txpower    = app_txpower;
	    request.version             = 2;
	    request.psid                = app_psid;
	    request.wsmps               = app_wsmps;
	    request.txpriority          = qpriority;
	    request.security            = securityType;
        request.macaddr[0] = 255;
        request.macaddr[1] = 255;
        request.macaddr[2] = 255;
        request.macaddr[3] = 255;
        request.macaddr[4] = 255;
        request.macaddr[5] = 255;	

	    //request.macaddr = ? ;
	    //request.expirytime = ? ;
	    
	    getMACAddr(request.srcmacaddr, txChan);
	    
	    memset(&request.data, 0, sizeof(WSMData));
	}
    
    asn_enc_rval_t rvalenc = der_encode_to_buffer(&asn_DEF_TravelerInformation, pMessage, &request.data.contents, HALFK);
 
    if(rvalenc.encoded != -1)
    {
        printf("TimProcessor: Rebroadcasting TIM\n");
        request.data.length = rvalenc.encoded;
        int ret = txWSMPacket(getpid(), &request);
        if(ret < 0)
        {
            printf("TimProcessor: Could not transmit packet!\n");
	}
    }
    
    return;
    
    /*
    bool shouldBroadcast = false;
    struct TravelerInformation__dataFrames__Member* pDataFrame = NULL;
    
    // Find origin of broadcasted message
    {    
        
        int i;
        for(i=0; i<pMessage->dataFrames.list.count; i++)
        {
            if(pMessage->dataFrames.list.array[i]->content.present == content_PR_advisory)
            {
                if(pMessage->dataFrames.list.array[i]->content.choice.advisory.list.count > 0)
                {            
                    char* pText = pMessage->dataFrames.list.array[i]->content.choice.advisory.list.array[0]->item.choice.text.buf;
                    
                    if(strcmp(pText, MESSAGE_ORIGIN_FIELD) == 0)
                    {
                        pDataFrame = pMessage->dataFrames.list.array[i];
                        break;
                    }
                }
            }
        }
    }
    
    VehicleState currentVehicleState;
    qwarnGetCurrentVehicleState(&currentVehicleState);
        
    if(!pDataFrame) // No origin dataframe found. Create one.
    {
        Position3D_t position;
        position.lat = currentVehicleState.lat_rad * RAD2DEG;
        position.Long = currentVehicleState.lon_rad * RAD2DEG;
        position.elevation = NULL;
        
        double heading_deg = currentVehicleState.heading_rad * RAD2DEG;
        
        pDataFrame = (struct TravelerInformation__dataFrames__Member*)calloc(1, sizeof(struct TravelerInformation__dataFrames__Member));
        
        ValidRegion_t* pValidRegion = (ValidRegion_t*)calloc(1, sizeof(ValidRegion_t));
        
        pValidRegion->direction.buf = malloc(sizeof(double));
        pValidRegion->direction.size = sizeof(double);
        memcpy(pValidRegion->direction.buf, &heading_deg, sizeof(double));
        
        pValidRegion->area.present = ValidRegion__area_PR_circle;
        pValidRegion->area.choice.circle.center = position;
        pValidRegion->area.choice.circle.raduis.present = Circle__raduis_PR_miles;
        pValidRegion->area.choice.circle.raduis.choice.miles = BROADCAST_RANGE_MI;
        
        int textLength = strlen(MESSAGE_ORIGIN_FIELD)+1;
        struct ITIScodesAndText__Member* pText = (struct ITIScodesAndText__Member*)calloc(1, sizeof(struct ITIScodesAndText__Member));
        pText->item.present = item_PR_text_it;
        pText->item.choice.text.buf = calloc(1, textLength);
        pText->item.choice.text.size = textLength;
        strcpy(pText->item.choice.text.buf, MESSAGE_ORIGIN_FIELD);
        
        asn_set_add(&pDataFrame->regions.list, pValidRegion);
        asn_set_add(&pMessage->dataFrames.list, pDataFrame);
        asn_set_add(&pDataFrame->content.choice.advisory.list, pText);
        
        shouldBroadcast = true;
    }
    else // Origin dataframe found.
    {
        GeoCoordConverter* pGeoConv = geoConvCreate();            
        
        ValidRegion_t* pRegion = pDataFrame->regions.list.array[0];
        
        double broadcastRange_m = pRegion->area.choice.circle.raduis.choice.miles * MILES2METERS;
        double heading_rad = (*((double*)pRegion->direction.buf)) * DEG2RAD;
        double latitude_rad = pRegion->area.choice.circle.center.lat * DEG2RAD;
        double longitude_rad = pRegion->area.choice.circle.center.Long * DEG2RAD;
        
        double easting_m, northing_m;
        char zone, band;
        
        if(!geoConvGeodetic2Utm(
            pGeoConv,
            latitude_rad, longitude_rad, 
            &zone, &band, 
            &easting_m, &northing_m))
        {
            printf("TimProcessor: Could not convert coords in origin dataframe to UTM!\n");
        }
        else // Success
        {
            Vector currentPos = {currentVehicleState.easting_m, currentVehicleState.northing_m};
            Vector originPos = {easting_m, northing_m};
            
            Vector originToCurrent;
            vecSub(&currentPos, &originPos, &originToCurrent);
            
            Vector dirFromOriginToCurrent;
            vecNormalize(&originToCurrent, &dirFromOriginToCurrent);
            
            Vector originHeading;
            mapHeadingToVector(heading_rad, &originHeading);
            
            if(vecMag(&originToCurrent) < broadcastRange_m && vecDot(&dirFromOriginToCurrent, &originHeading) < 0.0)
            {
                shouldBroadcast = true;
            }            
        }
        
        geoConvDestroy(pGeoConv);
    }
    
    if(shouldBroadcast)
    {
        if(!msgQueuePushFront(pInstance->pOutgoingQueue, pMessage, WSMMSG_TIM))
        {
            msgQueueFreeMessage(pMessage, WSMMSG_TIM);
        }
    }
    */
    
    return;
}

bool isQAlert(const char* alertMessage)
{
    return (alertMessage[0] == 'A' || alertMessage[0] == 'a' || alertMessage[0] == 'Q' || alertMessage[0] == 'q');
}

bool isSpeedHarm(const char* alertMessage)
{
    return (alertMessage[0] == 'S' || alertMessage[0] == 's');
}

bool isV2VQAlert(const char* alertMessage)
{
	return (alertMessage[0] == 'V' || alertMessage[0] == 'v');
//    return (alertMessage[0]) >= 'A' && (alertMessage[0] <='Z');
}

unsigned long timprocGetRelayCount()
{
    return _timprocRelayCount;
}

bool isValidTimFrame(TravelerInformation_t* pMessage, int frameIndex)
{
    if(pMessage->dataFrames.list.array[frameIndex]->content.present != content_PR_advisory)
    {
        printf("TimProcessor: Content not advisory. Skipping frame.\n");
        return false;
    }
    
    if(pMessage->dataFrames.list.array[frameIndex]->content.choice.advisory.list.count == 0)
    {
        printf("TimProcessor: No advisory present. Skipping frame.\n");
        return false;
    }
    
    if(pMessage->dataFrames.list.array[frameIndex]->content.choice.advisory.list.array[0]->item.present != item_PR_text_it)
    {
        printf("TimProcessor: Alert text not present. Skipping frame.\n");
        return false;
    }
      
    if(pMessage->dataFrames.list.array[frameIndex]->regions.list.count <= 0)
    {
        printf("TimProcessor: No regions in alert. Skipping frame.\n");
        return false;
    }
    
    return true;
}

double convertHeadingSliceToRad(uint16 headingSlice)
{
    if(headingSlice == 0x8001)
    {
        return 0.0;
    }
    else if(headingSlice == 0x0003)
    {
        return 22.5;
    }
    else if(headingSlice == 0x0006)
    {
        return 45.0;
    }
    else if(headingSlice == 0x000C)
    {
        return 67.5;
    }
    else if(headingSlice == 0x0018)
    {
        return 90.0;
    }
    else if(headingSlice == 0x0030)
    {
        return 112.5;
    }
    else if(headingSlice == 0x0060)
    {
        return 135.0;
    }
    else if(headingSlice == 0x00C0)
    {
        return 157.5;
    }
    else if(headingSlice == 0x0180)
    {
        return 180.0;
    }
    else if(headingSlice == 0x0300)
    {
        return 202.5;
    }
    else if(headingSlice == 0x0600)
    {
        return 225.0;
    }
    else if(headingSlice == 0x0C00)
    {
        return 247.5;
    }
    else if(headingSlice == 0x1800)
    {
        return 270.0;
    }
    else if(headingSlice == 0x3000)
    {
        return 292.5;
    }
    else if(headingSlice == 0x6000)
    {
        return 315.0;
    }
    else if(headingSlice == 0xC000)
    {
        return 337.5;
    }
}
