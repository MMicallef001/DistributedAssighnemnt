using Google.Cloud.Firestore;

namespace PubSubFunction
{
    [FirestoreData]
    public class Payment
    {
        [FirestoreProperty]
        public string OrderId { get; set; }

        [FirestoreProperty]
        public string PaymentId { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public double Amount { get; set; }

        [FirestoreProperty]
        public string CardNumber { get; set; }

        [FirestoreProperty]
        public string Address { get; set; }
    }
}
