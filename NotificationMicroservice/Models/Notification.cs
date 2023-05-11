using Google.Cloud.Firestore;

namespace NotificationMicroservice.Models
{
    [FirestoreData]
    public class Notification
    {
        [FirestoreProperty]
        public string NotificationId { get; set; }

        [FirestoreProperty]
        public string NotificanMessage { get; set; }

        [FirestoreProperty]
        public string UserId { get; set; }

        [FirestoreProperty]
        public Timestamp Timastamp { get; set; }

        public DateTime DtTimastamp
        {
            get { return Timastamp.ToDateTime(); }
            set { Timastamp = Google.Cloud.Firestore.Timestamp.FromDateTime(value.ToUniversalTime()); }
        }

    }
}
