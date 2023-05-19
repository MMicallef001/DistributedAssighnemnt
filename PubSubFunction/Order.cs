using Google.Cloud.Firestore;

namespace PubSubFunction
{
    [FirestoreData]
    public class Order
    {
        [FirestoreProperty]
        public string OrderId { get; set; }

        [FirestoreProperty]
        public string ProductAsin { get; set; }

        [FirestoreProperty]
        public string ProductId { get; set; }
        [FirestoreProperty]
        public string PaymentId { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string ProductName { get; set; }

        [FirestoreProperty]
        public string image { get; set; }

        [FirestoreProperty]
        public double Price { get; set; }

        [FirestoreProperty]
        public string Status { get; set; }

        [FirestoreProperty]
        public bool Paid { get; set; }
    }
}
