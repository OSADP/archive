#ifndef OBU_GeoCoordConverter_h
#define OBU_GeoCoordConverter_h

#define GEO_COORD_CONV_NUM_UTM_ZONES 60

typedef struct GeoCoordConverter
{
    void* pGeodeticProj;
    void* pUtmNorthProj[GEO_COORD_CONV_NUM_UTM_ZONES];
    void* pUtmSouthProj[GEO_COORD_CONV_NUM_UTM_ZONES];   

} GeoCoordConverter;

extern GeoCoordConverter* geoConvCreate();

extern int geoConvGeodetic2Utm(
    GeoCoordConverter* pInstance,
    double lat_rad, double lon_rad, 
    char* pZoneOut, char* pBandOut, 
    double* pEasting_m_out, double* pNorthing_m_out
);

extern int geoConvUtm2Geodetic(
    GeoCoordConverter* pInstance,
    unsigned char zone, unsigned char band, 
    double easting_m, double northing_m, 
    double* pLat_rad_out, double* pLon_rad_out
);

extern void geoConvDestroy(GeoCoordConverter* pInstance);

#endif
