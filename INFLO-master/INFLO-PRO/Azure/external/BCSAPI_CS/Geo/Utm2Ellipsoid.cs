using System;
using System.Runtime.InteropServices;
using BCS;

namespace BCS
{
    namespace Geo
    {
        using Bool = System.UInt32;
        using Handle = System.IntPtr;
        using String = System.String;

        public interface IUtm2Ellipsoid
        {
            void
                destroy();

            bool
                LatLon2Utm(
                    double lat_rad, 
                    double lon_rad, 
                    out byte zone, 
                    out byte band, 
                    out double easting_m, 
                    out double northing_m
                );

            bool
                Utm2LatLon(
                    byte zone, 
                    byte band, 
                    double easting_m, 
                    double northing_m, 
                    out double lat_rad, 
                    out double lon_rad
                );
        }

        public class Utm2Ellipsoid : IUtm2Ellipsoid
        {
            [DllImport(BCS.BCSAPI.BCSAPI_C_DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
            private static extern 
                Handle BCSAPI_C_Geo_Utm2Ellipsoid_create(
                    String ellipsoid
                );

            [DllImport(BCS.BCSAPI.BCSAPI_C_DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
            private static extern 
                void BCSAPI_C_Geo_Utm2Ellipsoid_destroy(
                    Handle hInstance
                );

            [DllImport(BCS.BCSAPI.BCSAPI_C_DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
            private static extern 
                Bool BCSAPI_C_Geo_Utm2Ellipsoid_transformLatLon2Utm(
                    Handle hInstance,
                    double latitude_rad,
                    double longitude_rad,
                    out byte zone,
                    out byte band,
                    out double easting_m,
                    out double northing_m
                );

            [DllImport(BCS.BCSAPI.BCSAPI_C_DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
            private static extern 
                Bool BCSAPI_C_Geo_Utm2Ellipsoid_transformUtm2LatLon(
                    Handle hInstance,
                    byte zone,
                    byte band,
                    double easting_m,
                    double northing_m,
                    out double lattitude_rad,
                    out double longitude_rad
                );

            [DllImport(BCS.BCSAPI.BCSAPI_C_DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
            private static extern 
                byte BCSAPI_C_Geo_Utm2Ellipsoid_computeUtmBandFromLat(
                    double latitude_deg
                );

            [DllImport(BCS.BCSAPI.BCSAPI_C_DLL_FILENAME, CallingConvention = CallingConvention.Cdecl)]
            private static extern 
                byte BCSAPI_C_Geo_Utm2Ellipsoid_computeUtmZoneFromLon(
                    double longitude_deg
                );

            public 
                Utm2Ellipsoid(string ellipsoid)
                {
                    mhInstance = BCSAPI_C_Geo_Utm2Ellipsoid_create(ellipsoid);
                    return;
                }
            
                ~Utm2Ellipsoid()
                {
                    this.destroy();
                    return;
                }

            public 
                void
                    destroy()
                {
                    if (mhInstance != Handle.Zero)
                    {
                        BCSAPI_C_Geo_Utm2Ellipsoid_destroy(mhInstance);
                        mhInstance = Handle.Zero;
                    }
                    return;
                }

            public 
                bool
                    LatLon2Utm(
                        double lat_rad, 
                        double lon_rad, 
                        out byte zone, 
                        out byte band, 
                        out double easting_m, 
                        out double northing_m
                    )
                {
                    return
                        BCSAPI_C_Geo_Utm2Ellipsoid_transformLatLon2Utm(
                            mhInstance,
                            lat_rad,
                            lon_rad,
                            out zone,
                            out band,
                            out easting_m,
                            out northing_m
                        ) != 0;
                }

            public 
                bool
                    Utm2LatLon(
                        byte zone, 
                        byte band, 
                        double easting_m, 
                        double northing_m, 
                        out double lat_rad, 
                        out double lon_rad
                    )
                {
                    return
                        BCSAPI_C_Geo_Utm2Ellipsoid_transformUtm2LatLon(
                            mhInstance,
                            zone,
                            band,
                            easting_m,
                            northing_m,
                            out lat_rad,
                            out lon_rad
                        ) != 0;
                }

            public 
                static byte
                    ComputeUtmBandFromLat(
                        double latitude_deg
                    )
                {
                    return
                        BCSAPI_C_Geo_Utm2Ellipsoid_computeUtmBandFromLat(
                            latitude_deg
                        );
                }

            public 
                static byte
                    ComputeUtmZoneFromLon(
                        double longitude_deg
                    )
                {
                    return
                        BCSAPI_C_Geo_Utm2Ellipsoid_computeUtmZoneFromLon(
                            longitude_deg
                        );
                }

            protected Handle mhInstance;
        }
    }
}
