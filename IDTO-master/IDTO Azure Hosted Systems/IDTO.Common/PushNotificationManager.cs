using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Notifications;

namespace IDTO.Common
{
    public class PushNotificationManager
    {
        private NotificationHubClient myClient;

        public PushNotificationManager(string endpoint, string HubName)
        {
            myClient = NotificationHubClient.CreateClientFromConnectionString(
                    endpoint,
                    HubName);
        }

        public async Task<bool>SendRejectNotificationsAsync(string tag)
        {
            
            String msg = "Current bus is running late.  Connection will not be held.";
            bool result = await SendGcmNotificationAsync(tag, msg);
            result = await SendIOSNotificationAsync(tag, msg);

            return result;
        }

         public async Task<bool>SendAcceptNotificationsAsync(string tag)
        {
            String msg = "Current bus is running late.  Connection will be held for 1 minute.";
            bool result = await SendGcmNotificationAsync(tag, msg);
            result = await SendIOSNotificationAsync(tag, msg);

            return result;
        }

        
        /// <summary>
        /// Send notification to Android devices
        /// </summary>
        /// <param name="tag"> If tag is present the notification will go only to the device(s) that have this tag in the Notification hub registration</param>
        /// <param name="message">Notification Message</param>
        public async Task<bool> SendGcmNotificationAsync(string tag, string message)
        {
            try
            {
                NotificationOutcome result = await myClient.SendGcmNativeNotificationAsync("{ \"data\" : {\"message\":\"" + message + "\"}}", tag);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
                // TODO log error
            }


        }

        /// <summary>
        /// Send notification to Android devices
        /// </summary>
        /// <param name="tag"> If tag is present the notification will go only to the device(s) that have this tag in the Notification hub registration</param>
        /// <param name="message">Notification Message</param>
        public async Task<bool> SendGcmSilentNotificationAsync(string tag, int val)
        {
            try
            {
                NotificationOutcome result = await myClient.SendGcmNativeNotificationAsync("{ \"data\" : {\"content-available\":" + val.ToString() + "}}", tag);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
                // TODO log error
            }


        }

        /// <summary>
        /// Send Trip Start notification to Android devices
        /// </summary>
        /// <param name="tag"> If tag is present the notification will go only to the device(s) that have this tag in the Notification hub registration</param>
        /// <param name="message">Notification Message</param>
        /// <param name="tripId">Trip ID</param>
        public async Task<bool> SendGcmTripStartNotificationAsync(string tag, string message, string tripId)
        {
            try
            {
                NotificationOutcome result = await myClient.SendGcmNativeNotificationAsync("{ \"data\" : {\"message\":\"" + message + "\",\"tripid\":" + tripId + "}}", tag);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
                // TODO log error
            }


        }

        /// <summary>
        /// Send notification to IOS devices
        /// </summary>
        /// <param name="tag"> If tag is present the notification will go only to the device(s) that have this tag in the Notification hub registration</param>
        /// <param name="message">Notification Message</param>
        public async Task<bool> SendIOSNotificationAsync(string tag, string message)
        {
            try
            {
                String apsMsg = "{ \"aps\" : {\"alert\":\"" + message + "\"}}";
                NotificationOutcome result = await myClient.SendAppleNativeNotificationAsync(apsMsg, tag);

                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
                // TODO log error
            }


        }

        /// <summary>
        /// Send notification to IOS devices
        /// </summary>
        /// <param name="tag"> If tag is present the notification will go only to the device(s) that have this tag in the Notification hub registration</param>
        /// <param name="message">Notification Message</param>
        public async Task<bool> SendIOSSilentNotificationAsync(string tag, int val)
        {
            try
            {
                String message = "Start tracking user location for " + val.ToString() + " seconds";

                NotificationOutcome result = await myClient.SendAppleNativeNotificationAsync("{ \"aps\" : {\"content-available\":" + val.ToString() + "}}", tag);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
                // TODO log error
            }
        }

        /// <summary>
        /// Send Trip Start notification to IOS devices
        /// </summary>
        /// <param name="tag"> If tag is present the notification will go only to the device(s) that have this tag in the Notification hub registration</param>
        /// <param name="message">Notification Message</param>
        /// <param name="tripId">Trip ID</param>
        public async Task<bool> SendIOSTripStartNotificationAsync(string tag, string message, string tripId)
        {
            try
            {
                NotificationOutcome result = await myClient.SendAppleNativeNotificationAsync("{ \"aps\" : {\"alert\":\"" + message + "\",\"tripid\":" + tripId + "}}", tag);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return false;
                // TODO log error
            }


        }
    }
}
