using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Google.Cloud.Firestore;

namespace PubSubFunction{
    public class HelperClass{
      public async void SendNotification(string notification, System.DateTime timeStamp,string userId)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                Notification n = new Notification();

                n.Timastamp = Google.Cloud.Firestore.Timestamp.FromDateTime(timeStamp.ToUniversalTime());

                DateTime dateTime = DateTime.UtcNow;
                Timestamp timestamp = Timestamp.FromDateTime(dateTime);

                n.Timastamp = timestamp;
                n.NotificanMessage = notification;
                n.NotificationId = Guid.NewGuid().ToString();
                n.UserId = userId;

                HttpResponseMessage response = await client.PostAsJsonAsync("https://notificationmicroservice-pqkchsrqxa-uc.a.run.app/AddNotification/", n);

                //HttpResponseMessage response = await client.PostAsJsonAsync("https://localhost:7149/api/NotificationMicroservice/AddNotification/", n);

                if (response.IsSuccessStatusCode)
                {

                }

            }

        }
      }
}