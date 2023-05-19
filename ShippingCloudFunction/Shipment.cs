using Google.Cloud.Firestore;

namespace ShippingCloudFunction
{
    [FirestoreData]
    public class Shipment
    {
        [FirestoreProperty]
        public string OrderId { get; set; }

        [FirestoreProperty]
        public string Status { get; set; }



    }
}