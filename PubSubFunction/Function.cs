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

namespace PubSubFunction
{
    public class Function : ICloudEventFunction<MessagePublishedData>
    {

        public async Task HandleAsync(CloudEvent cloudEvent, MessagePublishedData data, CancellationToken cancellationToken)
        {
            var jsonFromMessage = data.Message?.TextData;

            dynamic receivedData = JsonConvert.DeserializeObject(jsonFromMessage);

            model model = JsonConvert.DeserializeObject<model>(receivedData.model.ToString());

            ////////////////////////////////////////////////////////////////////



            using (var client = new HttpClient())
                            {
                string url = HttpUtility.UrlEncode(model.ProductUrl);


                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));


                HttpResponseMessage response = await client.GetAsync("https://productcatalougemicroservice-pqkchsrqxa-uc.a.run.app/api/ProductsMicroservice/" + url);
                //HttpResponseMessage response = await client.GetAsync("https://localhost:7074/api/ProductsMicroservice/" + url);



                if (response.IsSuccessStatusCode)
                {

                    ProductDetail productDetail = await response.Content.ReadAsAsync<ProductDetail>();

                    string userId = model.UserID;

                    Order o = new Order();

                    
                    o.OrderId = Guid.NewGuid().ToString();
                    o.ProductId = productDetail.Id;
                    o.UserId = userId;
                    o.ProductUrl = url;
                    o.ProductName = productDetail.ProductName;
                    o.image = productDetail.Image;
                    o.PaymentId = "";

                    
                   
                    if(!(productDetail.ShippingPrice.ToLower().Equals("free")))
                    {
                        string stringPrice = Regex.Replace(productDetail.ShippingPrice, "\\$", "");
                        double shipping = double.Parse(stringPrice);

                        stringPrice = Regex.Replace(productDetail.Pricing, "\\$", "");
                        double price = double.Parse(stringPrice);

                        double totalPrice = shipping + price;
                        o.Price = totalPrice;


                    }
                    else
                    {
                        string stringPrice = Regex.Replace(productDetail.Pricing, "\\$", "");
                        o.Price = double.Parse(stringPrice);
                    }

                    o.Status = "Order Is Waiting Payment";
                    o.Paid = true;

                    string PaymentId = Guid.NewGuid().ToString();
                    o.PaymentId = PaymentId;


                    using (var orderClient = new HttpClient())
                    {
                    
                        orderClient.BaseAddress = new Uri("https://ordersmicroservice-pqkchsrqxa-uc.a.run.app/api/OrdersMicroservice/");
                        //orderClient.BaseAddress = new Uri("https://localhost:7202/api/OrdersMicroservice/");


                        orderClient.DefaultRequestHeaders.Clear();
                        orderClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        HttpResponseMessage OrderedResponse = await orderClient.PostAsJsonAsync("AddOrder", o);

                        if (OrderedResponse.IsSuccessStatusCode)
                        {
                           // return RedirectToAction("Payment", new { price = o.Price, orderId = o.OrderId });


                            Payment newPayment = new Payment();


                            newPayment.PaymentId = PaymentId;

                            newPayment.OrderId = o.OrderId;
                            newPayment.UserId = userId;
                            newPayment.Amount = o.Price;
                            newPayment.CardNumber = model.CardNumber;
                            newPayment.Address = model.ShippingAddress;


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
                                            return Task.CompletedTask;
                                        }

                                    }

                                }
                            }
                        }
                    }
                }
            }

        }   
    }
}