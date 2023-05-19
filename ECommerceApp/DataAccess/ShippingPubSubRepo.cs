using Common;
using Common.Models;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Newtonsoft.Json;

namespace ECommerceApp.DataAccess
{
    public class ShippingPubSubRepo
    {
        TopicName topicName;
        Topic topic;
        public ShippingPubSubRepo(string projectId)
        {
            topicName = TopicName.FromProjectTopic(projectId, "shipment");
            if (topicName == null)
            {
                PublisherServiceApiClient publisher = PublisherServiceApiClient.Create();

                try
                {
                    topicName = new TopicName(projectId, "shipment");
                    topic = publisher.CreateTopic(topicName);
                }
                catch (Exception ex)
                {
                    //log
                    throw ex;
                }
            }
        }

        public async Task<string> PushMessage(Shipment s, string userId)
        {

            PublisherClient publisher = await PublisherClient.CreateAsync(topicName);

            var dataObject = new
            {
                Shipment = s,
                userId = userId
            };

            var pubsubMessage = new PubsubMessage
            {

                Data = ByteString.CopyFromUtf8(JsonConvert.SerializeObject(dataObject)),

                Attributes =
                {
                    { "priority", "normal" }
                }
            };
            string message = await publisher.PublishAsync(pubsubMessage);
            return message;
        }
    }
}
