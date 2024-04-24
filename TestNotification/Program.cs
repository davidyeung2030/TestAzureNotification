using Microsoft.Azure.NotificationHubs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestNotification
{
    internal class Program
    {
        private static string hubConn = "";
        private static string hubPath = "";
        private static bool enableTestSend = true;
        private static string token = "";// fcmv1 token retrieved from the android device.

        static async Task Main(string[] args)
        {
            await RemoveRegistrationsByToken(token);
            Console.WriteLine("");
            await TestFCMV1TemplatedNotification();

            Console.WriteLine("");
            Console.WriteLine("");

            await RemoveRegistrationsByToken(token);
            Console.WriteLine("");
            await TestFCMV1NativeNotification();

            Console.WriteLine("Press any key to continue");
            Console.ReadKey();
        }
        private static async Task RemoveRegistrationsByToken(string token)
        {
            NotificationHubClient testHub = NotificationHubClient.CreateClientFromConnectionString(hubConn, hubPath, enableTestSend);
            var regList = await testHub.GetRegistrationsByChannelAsync(token, 0);
            Console.WriteLine($@"Found {regList.Count()} registrations for channel {token}");
            //remove all registrations for the token
            foreach (var reg in regList)
            {
                Console.WriteLine($@"Deleting registration {reg.RegistrationId}");
                await testHub.DeleteRegistrationAsync(reg.RegistrationId);
            }
        }
        private static async Task TestFCMV1TemplatedNotification()
        {
            Console.WriteLine($@"Testing FCM V1 templated notification");
            System.Net.ServicePointManager.SecurityProtocol =
           SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
            NotificationHubClient testHub = NotificationHubClient.CreateClientFromConnectionString(hubConn, hubPath, enableTestSend);


            //create a new FCM V1 template registration
            Console.WriteLine($@"Creating templated registration");
            var template = new
            {
                message = new
                {
                    android = new
                    {
                        data = new
                        {
                            message = "$(message)"
                        }
                    }
                }
            };
            var tags = new List<string>() { "testsend" };
            var templateRegistration = await testHub.CreateFcmV1TemplateRegistrationAsync(token, JsonConvert.SerializeObject(template), tags);
            //Send a templated notification
            Console.WriteLine($@"Registration created: {templateRegistration.RegistrationId}");


            //send template notification
            Console.WriteLine($@"Sending templated notification");
            Dictionary<string, string> content = new Dictionary<string, string>();
            content.Add("message", "Inbox");
            var templateNotificationOutcome = await testHub.SendTemplateNotificationAsync(content, "testsend");
            Console.WriteLine($@"Notification sent result: {templateNotificationOutcome.TrackingId},result:{templateNotificationOutcome.Results.Count}, success: {templateNotificationOutcome.Success}, failed: {templateNotificationOutcome.Failure}");
        }

        private static async Task TestFCMV1NativeNotification()
        {
            Console.WriteLine($@"Testing FCM V1 native notification");
            System.Net.ServicePointManager.SecurityProtocol =
           SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;



            NotificationHubClient testHub = NotificationHubClient.CreateClientFromConnectionString(hubConn, hubPath, enableTestSend);



            //create a new FCM V1 native registration
            Console.WriteLine($@"Creating native registration");
            var tags = new List<string>() { "testsend" };

            var nativeRegistration = await testHub.CreateFcmV1NativeRegistrationAsync(token, tags);
            Console.WriteLine($@"Registration created: {nativeRegistration.RegistrationId}");



            //Send a native notification
            Console.WriteLine($@"Sending native notification");
            var payload = new
            {
                message = new
                {
                    android = new
                    {
                        data = new
                        {
                            message = "inbox"
                        }
                    }
                }
            };
            var nativeNotificationOutcome = await testHub.SendFcmV1NativeNotificationAsync(JsonConvert.SerializeObject(payload), "testsend");
            Console.WriteLine($@"Notification sent result: {nativeNotificationOutcome.TrackingId},result:{nativeNotificationOutcome.Results.Count}, success: {nativeNotificationOutcome.Success}, failed: {nativeNotificationOutcome.Failure}");

        }

    
    }
}
