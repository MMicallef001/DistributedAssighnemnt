using Google.Cloud.Firestore;
using ShippingMicroservice.Models;

namespace ShippingMicroservice.DataAccess
{
    public class FirebaseShipingRepo
    {
        FirestoreDb db;
        public FirebaseShipingRepo(string project)
        {
            db = FirestoreDb.Create(project);
        }

        public async Task<bool> CreateShipment(Shipment s)
        {
            try
            {
                await db.Collection("Shipments").Document().SetAsync(s);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Shipment> GetShipmentDetails(string orderId)
        {
            Query shipmentsQuery = db.Collection("Shipments").WhereEqualTo("OrderId", orderId);
            QuerySnapshot shipmentsQuerySnapshot = await shipmentsQuery.GetSnapshotAsync();

            DocumentSnapshot documentSnapshot = shipmentsQuerySnapshot.Documents.FirstOrDefault();
            try
            {
                if (documentSnapshot.Exists == false) return null;
                else
                {
                    Shipment result = documentSnapshot.ConvertTo<Shipment>();
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
