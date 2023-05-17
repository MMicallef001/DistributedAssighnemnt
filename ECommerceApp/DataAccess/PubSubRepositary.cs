using Common;
using Google.Cloud.PubSub.V1;
using Google.Protobuf;
using Newtonsoft.Json;

namespace ECommerceApp.DataAccess
{
    public class PubSubRepositary
    {
        TopicName topicName;
        Topic topic;
        public PubSubRepositary(string projectId)
        {
            topicName = TopicName.FromProjectTopic(projectId, "payment");
            if (topicName == null)
            {
                PublisherServiceApiClient publisher = PublisherServiceApiClient.Create();

                try
                {
                    topicName = new TopicName(projectId, "payment");
                    topic = publisher.CreateTopic(topicName);
                }
                catch (Exception ex)
                {
                    //log
                    throw ex;
                }
            }
        }

        public async Task<string> PushMessage(transferingModel tm)
        {

            PublisherClient publisher = await PublisherClient.CreateAsync(topicName);

            var pubsubMessage = new PubsubMessage
            {
                // The data is any arbitrary ByteString. Here, we're using text.
                Data = ByteString.CopyFromUtf8(JsonConvert.SerializeObject(tm)),
                // The attributes provide metadata in a string-to-string dictionary.
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
