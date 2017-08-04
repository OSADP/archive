using System;
using System.Globalization;
using INCZONE;
namespace INCZONE.NMEA
{
    /// <summary>
    /// Global Positioning System Fix Data
    /// </summary>
    public class GPGGA
    {
        /// <summary>
        /// Initializes the NMEA Global Positioning System Fix Data
        /// </summary>
        public GPGGA()
        {
            _position = new Coordinate();
        }

        /// <summary>
        /// Initializes the NMEA Global Positioning System Fix Data and parses an NMEA sentence
        /// </summary>
        /// <param name="nmeaSentence"></param>
        public GPGGA(string nmeaSentence)
        {
            try
            {
                if (nmeaSentence.IndexOf('*') > 0)
                    nmeaSentence = nmeaSentence.Substring(0, nmeaSentence.IndexOf('*'));
                //Split into an array of strings.
                string[] split = nmeaSentence.Split(new[] { ',' });
                if (split[1].Length >= 6)
                {
                    var hrs = 0;
                    var min = 0;
                    var sec= 0;

                    int.TryParse(split[1].Substring(0, 2), out hrs);
                    int.TryParse(split[1].Substring(2, 2), out min);
                    int.TryParse(split[1].Substring(4, 2), out sec);


                    TimeSpan t = new TimeSpan(hrs,min, sec);
                    DateTime nowutc = DateTime.UtcNow;
                    nowutc = nowutc.Add(-nowutc.TimeOfDay);
                    _timeOfFix = nowutc.Add(t);

                }

                _position = new Coordinate(GpsHandler.GPSToDecimalDegrees(split[4], split[5]),
                                           GpsHandler.GPSToDecimalDegrees(split[2], split[3]));
                if (split[6] == "1")
                    FixQuality = FixQualityEnum.GPS;
                else if (split[6] == "2")
                    FixQuality = FixQualityEnum.DGPS;
                else
                    FixQuality = FixQualityEnum.Invalid;
                _noOfSats = Convert.ToByte(split[7]);
                double.TryParse(split[8], out _dilution);
                double.TryParse(split[9], out _altitude);
                _altitudeUnits = split[10][0];
                double.TryParse(split[11], out _heightOfGeoid);
                int.TryParse(split[13], out _dGPSUpdate);
                _dGPSStationID = split[14];
            }
            catch { }
        }

        /// <summary>
        /// Enum for the GGA Fix Quality.
        /// </summary>
        public enum FixQualityEnum
        {
            /// <summary>
            /// Invalid fix
            /// </summary>
            Invalid = 0,
            /// <summary>
            /// GPS fix
            /// </summary>
            GPS = 1,
            /// <summary>
            /// DGPS fix
            /// </summary>
            DGPS = 2
        }


        private readonly DateTime _timeOfFix;
        private readonly Coordinate _position;
        private readonly byte _noOfSats;
        private readonly double _altitude;
        private readonly char _altitudeUnits;
        private readonly double _dilution;
        private readonly double _heightOfGeoid;
        private readonly int _dGPSUpdate;
        private readonly string _dGPSStationID;

        /// <summary>
        /// time of fix (hhmmss).
        /// </summary>
        public DateTime TimeOfFix
        {
            get { return _timeOfFix; }
        }

        /// <summary>
        /// Coordinate of recieved position
        /// </summary>
        public Coordinate Position
        {
            get { return _position; }
        }

        /// <summary>
        /// Fix quality (0=invalid, 1=GPS fix, 2=DGPS fix)
        /// </summary>
        public FixQualityEnum FixQuality { get; internal set; }

        /// <summary>
        /// number of satellites being tracked.
        /// </summary>
        public byte NoOfSats
        {
            get { return _noOfSats; }
        }

        /// <summary>
        /// Altitude above sea level.
        /// </summary>
        public double Altitude
        {
            get { return _altitude; }
        }

        /// <summary>
        /// Altitude Units - M (meters).
        /// </summary>
        public char AltitudeUnits
        {
            get { return _altitudeUnits; }
        }

        /// <summary>
        /// Horizontal dilution of position (HDOP).
        /// </summary>
        public double Dilution
        {
            get { return _dilution; }
        }

        /// <summary>
        /// Height of geoid (mean sea level) above WGS84 ellipsoid.
        /// </summary>
        public double HeightOfGeoid
        {
            get { return _heightOfGeoid; }
        }

        /// <summary>
        /// Time in seconds since last DGPS update.
        /// </summary>
        public int DGPSUpdate
        {
            get { return _dGPSUpdate; }
        }

        /// <summary>
        /// DGPS station ID number.
        /// </summary>
        public string DGPSStationID
        {
            get { return _dGPSStationID; }
        }

        public static string GenerateGPGGAcode(Coordinate coord)
        {
            double posnum = 0;
            double minutes = 0;

            DateTime UTCTime = DateTime.UtcNow;


            string mycode = "GPGGA,";

            if (UTCTime.Hour < 10)
            {
                mycode = mycode + "0";
            }
            mycode = mycode + UTCTime.Hour;
            if (UTCTime.Minute < 10)
            {
                mycode = mycode + "0";
            }
            mycode = mycode + UTCTime.Minute;

            if (UTCTime.Second < 10)
            {
                mycode = mycode + "0";
            }
            mycode = mycode + UTCTime.Second;
            mycode = mycode + ",";

            posnum = Math.Abs(coord.Latitude);
            minutes = posnum % 1;

            posnum = posnum - minutes;
            minutes = minutes * 60;
            posnum = (posnum * 100) + minutes;
            if (posnum < 1000)
            {
                mycode = mycode + "0";
                if (posnum < 100)
                {
                    mycode = mycode + "0";
                }
            }
            mycode = mycode + posnum.ToString();

            if (coord.Latitude > 0)
            {
                mycode = mycode + ",N,";
            }
            else
            {
                mycode = mycode + ",S,";
            }

            posnum = Math.Abs(coord.Longitude);
            minutes = posnum % 1;
            posnum = posnum - minutes;
            minutes = minutes * 60;
            posnum = (posnum * 100) + minutes;
            if (posnum < 10000)
            {
                mycode = mycode + "0";
                if (posnum < 1000)
                {
                    mycode = mycode + "0";
                    if (posnum < 100)
                    {
                        mycode = mycode + "0";
                    }
                }
            }
            mycode = mycode + posnum.ToString();

            if (coord.Longitude > 0)
            {
                mycode = mycode + ",E,";
            }
            else
            {
                mycode = mycode + ",W,";
            }

            mycode = mycode + "4,10,1,200,M,1,M,";

            mycode = mycode + ((DateTime.Now.Second) % 6) + 3 + ",0";


            mycode = "$" + mycode + "*" + CalculateChecksum(mycode);

            return mycode;
        }

        public static string CalculateChecksum(string sentence)
        {
            int Checksum = 0;

            foreach(char var in sentence)
            {
                switch(var)
                {
                    case '$':
                    case '*':
                        break;
                    default:
                        {
                            if (Checksum == 0)
                                Checksum = Convert.ToByte(var);
                            else
                                Checksum = Checksum ^ Convert.ToByte(var);
                        }
                        break;
                }
            }

            return Checksum.ToString("X2");
        }
    }
}