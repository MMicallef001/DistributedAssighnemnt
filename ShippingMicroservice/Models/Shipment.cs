using Google.Api;
using Google.Cloud.Firestore;

namespace ShippingMicroservice.Models
{
    [FirestoreData]
    public class Shipment
    {
        [FirestoreProperty]
        public string shipmentId { get; set; }
        [FirestoreProperty]
        public string shipmentStatus { get; set;}
        [FirestoreProperty]
        public string orderId { get; set;}
    }
}
