using Google.Cloud.Firestore;
using Common.Models;

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

        public async Task<bool> Update(Shipment s)
        {
            Query shipmentQuery = db.Collection("Shipments").WhereEqualTo("OrderId", s.OrderId);
            QuerySnapshot shipmentsQuerySnapshot = await shipmentQuery.GetSnapshotAsync();

            DocumentSnapshot documentSnapshot = shipmentsQuerySnapshot.Documents.FirstOrDefault();
            if (documentSnapshot.Exists == false)
            {
                return false;
            }
            else
            {
                DocumentReference shipmentsRef = db.Collection("Shipments").Document(documentSnapshot.Id);
                await shipmentsRef.SetAsync(s);
                return true;
            }
        }

        public async Task<List<Shipment>> GetShipments()
        {
            List<Shipment> shipments = new List<Shipment>();
            Query allShipmentsQuery = db.Collection("Shipments");
            QuerySnapshot allShipmentsQuerySnapshot = await allShipmentsQuery.GetSnapshotAsync();
            foreach (DocumentSnapshot documentSnapshot in allShipmentsQuerySnapshot.Documents)
            {
                Shipment s = documentSnapshot.ConvertTo<Shipment>();
                shipments.Add(s);
            }

            return shipments;
        }

    }
}
