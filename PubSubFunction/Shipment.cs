using Google.Cloud.Firestore;


namespace PubSubFunction{
    [FirestoreData]
    public class Shipment
    {
        [FirestoreProperty]
        public string OrderId { get; set; }

        [FirestoreProperty]
        public string Status { get; set; }
        
        [FirestoreProperty]
        public string ShippingAddress { get; set; }


    }
}
