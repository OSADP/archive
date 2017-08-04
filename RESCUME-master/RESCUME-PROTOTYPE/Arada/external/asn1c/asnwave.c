#include <asnwave.h>
#include <wave.h>

struct asn_TYPE_descriptor_s;	/* Forward declaration */

extern struct asn_TYPE_descriptor_s asn_DEF_AlaCarte;
extern struct asn_TYPE_descriptor_s asn_DEF_BasicSafetyMessage;
extern struct asn_TYPE_descriptor_s asn_DEF_BasicSafetyMessageVerbose;
extern struct asn_TYPE_descriptor_s asn_DEF_CommonSafetyRequest;
extern struct asn_TYPE_descriptor_s asn_DEF_EmergencyVehicleAlert;
extern struct asn_TYPE_descriptor_s asn_DEF_IntersectionCollision;
extern struct asn_TYPE_descriptor_s asn_DEF_MapData;
extern struct asn_TYPE_descriptor_s asn_DEF_NMEA_Corrections;
extern struct asn_TYPE_descriptor_s asn_DEF_ProbeDataManagement;
extern struct asn_TYPE_descriptor_s asn_DEF_ProbeVehicleData;
extern struct asn_TYPE_descriptor_s asn_DEF_RoadSideAlert;
extern struct asn_TYPE_descriptor_s asn_DEF_RTCM_Corrections;
extern struct asn_TYPE_descriptor_s asn_DEF_SignalRequestMsg;
extern struct asn_TYPE_descriptor_s asn_DEF_SignalStatusMessage;
extern struct asn_TYPE_descriptor_s asn_DEF_SPAT;
extern struct asn_TYPE_descriptor_s asn_DEF_TravelerInformation;


struct asn_TYPE_descriptor_s *asn_pdu_collection[] = {
	0,
	&asn_DEF_AlaCarte,	
	&asn_DEF_BasicSafetyMessage,	
	&asn_DEF_BasicSafetyMessageVerbose,	
	&asn_DEF_CommonSafetyRequest,	
	&asn_DEF_EmergencyVehicleAlert,	
	&asn_DEF_IntersectionCollision,	
	&asn_DEF_MapData,	
	&asn_DEF_NMEA_Corrections,	
	&asn_DEF_ProbeDataManagement,	
	&asn_DEF_ProbeVehicleData,	
	&asn_DEF_RoadSideAlert,
	&asn_DEF_RTCM_Corrections,	
	&asn_DEF_SPAT,
	&asn_DEF_SignalRequestMsg,	
	&asn_DEF_SignalStatusMessage,		
	&asn_DEF_TravelerInformation,	
};


u_int16_t identifyMessage(uint8_t *msgData) {

	u_int16_t type = WSMMSG_INVALID;
	u_int16_t i=2;
        while(1){
        if(msgData[i] == 128 && msgData[i+1] == 1)
           break;
        else
           i=i+1;
        }    
	     if(i!=0){	
             switch(msgData[i+2]) {
			case WSMMSG_ALACARTE:
				type = WSMMSG_ALACARTE;
				break;
			case WSMMSG_BSM:
				type = WSMMSG_BSM;
				break;
			case WSMMSG_BSMVERBOSE:
				type = WSMMSG_BSMVERBOSE;
				break;
			case WSMMSG_CSR:
				type = WSMMSG_CSR;
				break;
			case WSMMSG_EVA:
				type = WSMMSG_EVA;
				break;
			case WSMMSG_ICA:
				type = WSMMSG_ICA;
				break;
			case WSMMSG_MAPDATA:
				type = WSMMSG_MAPDATA;
				break;
			case WSMMSG_NMEA:
				type = WSMMSG_NMEA;
				break;
			case WSMMSG_PDM:
				type = WSMMSG_PDM;
				break;
			case WSMMSG_PVD:
				type = WSMMSG_PVD;
				break;
			case WSMMSG_RSA:
				type = WSMMSG_RSA;
				break;
			case WSMMSG_RTCM:
				type = WSMMSG_RTCM;
				break;
			case WSMMSG_SPAT:
				type = WSMMSG_SPAT;
				break;
			case WSMMSG_SRM:
				type = WSMMSG_SRM;
				break;
			case WSMMSG_SSM:
				type = WSMMSG_SSM;
				break;
			case WSMMSG_TIM:
				type = WSMMSG_TIM;
				break;
			default:
				break;
		}
	}
	return type;
}

void *decodeMessage(u_int16_t msgType, WSMData data, u_int16_t *status) {
	asn_TYPE_descriptor_t **pdu = asn_pdu_collection;
	asn_TYPE_descriptor_t *pduType = pdu[msgType];
	asn_codec_ctx_t *opt_codec_ctx = 0;
	void *structure = 0;
	asn_dec_rval_t rval;
	char *i_bptr;
	size_t i_size;
	i_bptr = data.contents;
	i_size = (size_t) data.length;
	rval = ber_decode(opt_codec_ctx, pduType, (void **)&structure, 
											i_bptr, i_size);
	switch(rval.code) {
		case RC_OK:
			*status = 0;
			return structure;
		case RC_FAIL:
			*status = 1;
			break;
	}
	ASN_STRUCT_FREE(*pduType, structure);
	return 0;
}

int rxWSMMessage(int pid, WSMMessage *msg) {
	int ret = 0;
	WSMIndication *rxpkt;
	WSMData data;
	uint8_t *buf;
	int i;
	u_int16_t status;
	rxpkt = msg->wsmIndication;
	ret = rxWSMPacket(pid, rxpkt);
	return ret;
}

int rxWSMIdentity(WSMMessage *msg, int type)
{
	WSMIndication *rxpkt;
	WSMData data;
	u_int16_t status;
	rxpkt = msg->wsmIndication;
	data = rxpkt->data;
	msg->type = identifyMessage(data.contents);
	if (msg->type != 0) {
	    msg->structure = decodeMessage(msg->type, data, &status);
	    msg->decode_status = status;
	}
	
} 

int xml_print(WSMMessage msg) {
	asn_TYPE_descriptor_t **pdu = asn_pdu_collection;
	asn_TYPE_descriptor_t *pduType = pdu[msg.type];
	if (msg.type != 0) {
		pduType = pdu[msg.type];
		if(xer_fprint(stdout, pduType, msg.structure)) {
			fprintf(stderr, "Cannot convert %s into XML\n", pduType->name);
			return 1;
		}
	}
	return 0;
}

