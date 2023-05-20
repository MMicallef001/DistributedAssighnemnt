using Common.Models;
using Google.Cloud.Firestore;

namespace NotificationMicroservice.DataAccess
{
    public class FirebaseNotificationRepo
    {

        FirestoreDb db;
        public FirebaseNotificationRepo(string project)
        {
            db = FirestoreDb.Create(project);
        }

        public async Task<bool> CreateNotification(Notification n)
        {
            try
            {
                DateTime dateTime = DateTime.UtcNow;
                Timestamp timestamp = Timestamp.FromDateTime(dateTime);

                n.Timastamp = timestamp;

                await db.Collection("Notifications").Document().SetAsync(n);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Notification>> GetNotifications(string UserId)
        {
            List<Notification> notifications = new List<Notification>();

            Query notificationsQuery = db.Collection("Notifications").WhereEqualTo("UserId", UserId);

            QuerySnapshot notificationsQuerySnapshot = await notificationsQuery.GetSnapshotAsync();

            foreach (DocumentSnapshot documentSnapshot in notificationsQuerySnapshot.Documents)
            {
                Notification n = documentSnapshot.ConvertTo<Notification>();
                notifications.Add(n);
            }

            return notifications;
        }
        
    }
}