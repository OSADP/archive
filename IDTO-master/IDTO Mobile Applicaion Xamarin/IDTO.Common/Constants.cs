using System;

namespace IDTO.Common
{
    public class Constants
    {

#if DEBUG
        public const string ConnectionString = "Endpoint=sb://idto-devhub-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=2UVOWqON1LFo8AWVdVPBwtP904dGdNK0ZlktWC0QIP8=";
        public const string NotificationHubPath = "idto-devhub";

        public const string SenderID = "806243004701"; // Google API Project Number
#else
        public const string ConnectionString = "Endpoint=sb://c-ride.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=gVKuoUW8R3stNA/5GkrQuXXswgzIHn5MM5aU2n+oqA8=";
        public const string NotificationHubPath = "cride-notificationhub";

        public const string SenderID = "806243004701"; // Google API Project Number
#endif

    }
}

