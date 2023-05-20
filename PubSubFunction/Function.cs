using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.Web;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Google.Cloud.Firestore;

namespace PubSubFunction
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

             _logger.LogInformation($"Data Received is {jsonFromMessage}");

            model model = JsonConvert.DeserializeObject<model>(jsonFromMessage);

            ////////////////////////////////////////////////////////////////////

            
             _logger.LogInformation($"Before using");
            _logger.LogInformation($"Data Received is {model.CardNumber}");
            _logger.LogInformation($"Data Received is {model.Asin}");

            _logger.LogInformation($"Data Received is {model.UserID}");
            _logger.LogInformation($"Data Received is {model.Addess}");



            using (var client = new HttpClient()){


                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                HttpResponseMessage response = await client.GetAsync("https://productcatalougemicroservice-pqkchsrqxa-uc.a.run.app/api/ProductsMicroservice/" + model.Asin);
                //HttpResponseMessage response = await client.GetAsync("https://localhost:7074/api/ProductsMicroservice/" + url);

                 _logger.LogInformation($"Data Received is {response.IsSuccessStatusCode}");

                if (response.IsSuccessStatusCode)
                {

                    ProductDetail productDetail = await response.Content.ReadAsAsync<ProductDetail>();

                    string userId = model.UserID;

                    Order o = new Order();

                    
                    o.OrderId = Guid.NewGuid().ToString();
                    o.ProductId = productDetail.Id;
                    o.UserId = userId;
                    o.ProductAsin = productDetail.Asin;
                    o.ProductName = productDetail.ProductName;
                    o.image = productDetail.Image;
                    o.PaymentId = "";


                    string stringPrice = productDetail.Pricing;
                    
                    o.Price = double.Parse(stringPrice);

                    o.Status = "Order Is Waiting Payment";
                    o.Paid = true;

                    string PaymentId = Guid.NewGuid().ToString();
                    o.PaymentId = PaymentId;


                    using (var orderClient = new HttpClient())
                    {
                        _logger.LogInformation($"adding order");

                    
                        orderClient.BaseAddress = new Uri("https://ordersmicroservice-pqkchsrqxa-uc.a.run.app/api/OrdersMicroservice/");
                        //orderClient.BaseAddress = new Uri("https://localhost:7202/api/OrdersMicroservice/");


                        orderClient.DefaultRequestHeaders.Clear();
                        orderClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage OrderedResponse = await orderClient.PostAsJsonAsync("AddOrder", o);

                        if (OrderedResponse.IsSuccessStatusCode)
                        {

                            Payment newPayment = new Payment();


                            newPayment.PaymentId = PaymentId;

                            newPayment.OrderId = o.OrderId;
                            newPayment.UserId = userId;
                            newPayment.Amount = o.Price;
                            newPayment.CardNumber = model.CardNumber;
                            newPayment.Address = model.Addess;


                            using (var paymentclient = new HttpClient())
                            {
                                paymentclient.BaseAddress = new Uri("https://paymentmicroservice-pqkchsrqxa-uc.a.run.app/api/PaymentsMicroservice/");
                                //client.BaseAddress = new Uri("https://localhost:7153/api/PaymentsMicroservice/");


                                paymentclient.DefaultRequestHeaders.Clear();
                                paymentclient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                HttpResponseMessage Response = await paymentclient.PostAsJsonAsync("AddPayment", newPayment);

                                if (Response.IsSuccessStatusCode)
                                {
  
                                    Shipment s = new Shipment();
                                    s.Status = "ordered received not yet dispatched";
                                    s.OrderId = o.OrderId;
                                    
                                    using (var ShipmentClient = new HttpClient())
                                    {
                                        ShipmentClient.DefaultRequestHeaders.Clear();
                                        ShipmentClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                

                                        HttpResponseMessage shipmentResponse = await ShipmentClient.PostAsJsonAsync("https://shippingmicroservice-pqkchsrqxa-uc.a.run.app/api/ShippingMicroservice/AddShipment/", s);
                                        //HttpResponseMessage response = await ShipmentClient.PostAsJsonAsync("https://localhost:7293/api/ShippingMicroservice/AddShipment/", s);


                                        if (shipmentResponse.IsSuccessStatusCode)
                                        {
                                            DateTime dateTime = DateTime.UtcNow;
                                            

                                            SendNotification(s.Status,dateTime,userId);
                                        }

                                    }

                                }
                            }
                        }
                    }
                }
            }

        }

        private async void SendNotification(string notification, System.DateTime timeStamp,string userId)
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

                HttpResponseMessage response = await client.PostAsJsonAsync("https://notificationmicroservice-pqkchsrqxa-uc.a.run.app/api/NotificationMicroservice/AddNotification/", n);

                //HttpResponseMessage response = await client.PostAsJsonAsync("https://localhost:7149/api/NotificationMicroservice/AddNotification/", n);

                if (response.IsSuccessStatusCode)
                {

                }

            }

        }
    }
}