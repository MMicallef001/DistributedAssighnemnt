using Google.Cloud.Firestore;

namespace ordersMicroservice.Models
{
    [FirestoreData]
    public class Order
    {
        [FirestoreProperty]
        public string OrderId { get; set; }

        [FirestoreProperty]
        public string ProductId { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public string Price { get; set; }

        [FirestoreProperty]
        public string Status { get; set; }

        [FirestoreProperty]
        public bool Paid { get; set; }

    }
}
