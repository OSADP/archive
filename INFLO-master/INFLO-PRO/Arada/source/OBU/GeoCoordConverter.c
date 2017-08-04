#include "GeoCoordConverter.h"
#include "QWarnDefs.h"

#include <stdbool.h>
#include <stddef.h>

uint8 lat2UtmBand(double lat_deg)
{
    static const uint8 zoneChars[] = "CDEFGHIJKLMNOPQRSTUVWXX";
    return ((lat_deg >= -80.0) && (lat_deg <= 84.0)) ?
        zoneChars[(unsigned long)floor((lat_deg+80.0) / 8.0)] : (uint8)0;
}

uint8 lon2UtmZone(double lon_deg)
{
    return ((lon_deg >= -180.0) && (lon_deg < 180.0)) ?
            (uint8)(1 + (uint8)floor((lon_deg + 180.0) / 6.0)) : (uint8)0;
}

GeoCoordConverter* geoConvCreate()
{
    GeoCoordConverter* pInstance = (GeoCoordConverter*)malloc(sizeof(GeoCoordConverter));
    pInstance->pGeodeticProj = pj_init_plus("+proj=latlong +ellps=WGS84");
    
    int i;    
    for(i=0; i < GEO_COORD_CONV_NUM_UTM_ZONES; i++)
    {
        char* pPrefix = "+proj=utm +ellps=WGS84 +zone=";
    
        char definition[128];        
        char zone[16];
        
        definition[0] = 0;
        
        sprintf(zone, "%d", (i+1));        
        
        strcat(definition, pPrefix);
        strcat(definition, zone);
        
        pInstance->pUtmNorthProj[i] = pj_init_plus(definition);
        
        strcat(definition, " +south");
        pInstance->pUtmSouthProj[i] = pj_init_plus(definition);
        
    }
    
    return pInstance;    
}

int geoConvGeodetic2Utm(
    GeoCoordConverter* pInstance,
    double lat_rad, double lon_rad, 
    char* pZoneOut, char* pBandOut, 
    double* pEasting_m_out, double* pNorthing_m_out
)
{
    double x = lon_rad;
    double y = lat_rad;    
    uint8 zone = lon2UtmZone(lon_rad * RAD2DEG);
    uint8 band = lat2UtmBand(lat_rad * RAD2DEG);
    
    void* pUtmProj = (band >= 'N') ? pInstance->pUtmNorthProj[zone-1] : pInstance->pUtmSouthProj[zone-1];
    
    if(((zone >= 1) && (zone <= GEO_COORD_CONV_NUM_UTM_ZONES)) &&
       ((band >= 'C') && (band <= 'X')))
    {
        if(pj_transform(pInstance->pGeodeticProj, pUtmProj, 1, 0, &x, &y, NULL) == 0)
        {        
            *pZoneOut = zone;
            *pBandOut = band;
            *pEasting_m_out = x;
            *pNorthing_m_out = y;
            
            return true;
        }
    }
    
    return false;
}

int geoConvUtm2Geodetic(
    GeoCoordConverter* pInstance,
    unsigned char zone, unsigned char band, 
    double easting_m, double northing_m, 
    double* pLat_rad_out, double* pLon_rad_out
)
{
    double x = easting_m;
    double y = northing_m;
    
    void* pUtmProj = (band >= 'N') ? pInstance->pUtmNorthProj[zone-1] : pInstance->pUtmSouthProj[zone-1];
    
    if(pj_transform(pUtmProj, pInstance->pGeodeticProj, 1, 0, &x, &y, NULL) == 0)
    {
        *pLat_rad_out = x;
        *pLon_rad_out = y;
    
        return true;
    }
    
    return false;
}

void geoConvDestroy(GeoCoordConverter* pInstance)
{
    pj_free(pInstance->pGeodeticProj);
    
    int i;    
    for(i=0; i < GEO_COORD_CONV_NUM_UTM_ZONES; i++)
    {
        pj_free(pInstance->pUtmNorthProj[i]);
        pj_free(pInstance->pUtmSouthProj[i]);
    }
    
    free(pInstance);
    
    return;
}
