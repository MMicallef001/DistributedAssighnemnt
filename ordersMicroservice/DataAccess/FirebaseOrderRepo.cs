using Google.Cloud.Firestore;
using ordersMicroservice.Models;
using System.Collections.Generic;

namespace ordersMicroservice.DataAccess
{
    public class FirebaseOrderRepo
    {
        FirestoreDb db;
        public FirebaseOrderRepo(string project)
        {
            db = FirestoreDb.Create(project);
        }

        public async Task<bool> CreateOrder(Order o)
        {
            try
            {
                await db.Collection("Orders").Document().SetAsync(o);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<Order>> GetOrders(string UserId)
        {
            List<Order> orders = new List<Order>();

            Query ordersQuery = db.Collection("Orders").WhereEqualTo("UserId", UserId);

            QuerySnapshot ordersQuerySnapshot = await ordersQuery.GetSnapshotAsync();

            foreach (DocumentSnapshot documentSnapshot in ordersQuerySnapshot.Documents)
            {
                Order o = documentSnapshot.ConvertTo<Order>();
                orders.Add(o);
            }

            return orders;
        }

        public async Task<Order> GetOrderDetails(string orderId)
        {
            Query ordersQuery = db.Collection("Orders").WhereEqualTo("OrderId", orderId);
            QuerySnapshot ordersQuerySnapshot = await ordersQuery.GetSnapshotAsync();

            DocumentSnapshot documentSnapshot = ordersQuerySnapshot.Documents.FirstOrDefault();
            try
            {
                if (documentSnapshot.Exists == false) return null;
                else
                {
                    Order result = documentSnapshot.ConvertTo<Order>();
                    return result;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
