using Google.Cloud.Firestore;

namespace PaymentMicroservice.Models
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
        public string Amount { get; set; }

        [FirestoreProperty]
        public string CardNumber { get; set; }
    }
}
