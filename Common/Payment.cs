using Google.Cloud.Firestore;

namespace Common.Models
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
    }
}
