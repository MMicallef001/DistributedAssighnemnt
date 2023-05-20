using CloudNative.CloudEvents;
using Google.Cloud.Firestore;
using Google.Cloud.Functions.Framework;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ShippingCloudFunction
{

public class Function : ICloudEventFunction<MessagePublishedData>
{
    private readonly ILogger<Function> _logger;

        public Function(ILogger<Function> logger) => _logger = logger;

        public async Task HandleAsync(CloudEvent cloudEvent, MessagePublishedData data, CancellationToken cancellationToken)
        {
            _logger.LogInformation("PubSub Function started");
            var jsonFromMessage = data.Message?.TextData;

            //dynamic receivedData = JsonConvert.DeserializeObject(jsonFromMessage);
            
            DataObject receivedData = JsonConvert.DeserializeObject<DataObject>(jsonFromMessage);

             _logger.LogInformation($"Data Received is {jsonFromMessage}");

            
            Shipment s = receivedData.Shipment;

            //Shipment s = JsonConvert.DeserializeObject<Shipment>(jsonFromMessage);
            string userId = receivedData.userId;

            _logger.LogInformation($"Data Received is {userId}");
            _logger.LogInformation($"s.orderId{s.OrderId}");
            _logger.LogInformation($"s.status {s.Status}");

            using (var client = new HttpClient())
            {
                _logger.LogInformation($"in using");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                HttpResponseMessage UpdatedOrderedResponse = await client.PostAsJsonAsync("https://shippingmicroservice-pqkchsrqxa-uc.a.run.app/api/ShippingMicroservice/update/", s);
                //HttpResponseMessage UpdatedOrderedResponse = await client.PostAsJsonAsync("https://localhost:7293/api/ShippingMicroservice/update/", s);

                if (UpdatedOrderedResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successful shipment update");
                    SendNotification(s.Status,DateTime.Now, userId);
                }
                _logger.LogInformation($"Not successful");
            }
        }
    
    private async void SendNotification(string notification, System.DateTime timeStamp,string userId)
        {

            _logger.LogInformation($"In send Notification");
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
                
                _logger.LogInformation($"n.Tim" + n.Timastamp );
                _logger.LogInformation($"n.NotificanMessage" + n.NotificanMessage );

                _logger.LogInformation($"n.NotificationId" + n.NotificationId );
                _logger.LogInformation($"n.UserId" + n.UserId );


                //HttpResponseMessage response = await client.PostAsJsonAsync("https://shippingmicroservice-pqkchsrqxa-uc.a.run.app/api/NotificationMicroservice/AddShipment/", notification);

                HttpResponseMessage response = await client.PostAsJsonAsync("https://notificationmicroservice-pqkchsrqxa-uc.a.run.app/api/NotificationMicroservice/AddNotification/", n);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Successful send notif");
                }

                _logger.LogInformation($"response" + response);
                _logger.LogInformation($"response requestmessage" + response.RequestMessage);


            }
            _logger.LogInformation($"not successful send notif");

        }
    }
}
