using Google.Cloud.Firestore;
using PaymentMicroservice.Models;

namespace PaymentMicroservice.DataAccess
{
    public class FirebasePaymentsRepo
    {
        FirestoreDb db;
        public FirebasePaymentsRepo(string project)
        {
            db = FirestoreDb.Create(project);
        }

        public async Task<bool> CreatePayment(Payment p)
        {
            try
            {
                await db.Collection("Payments").Document().SetAsync(p);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<Payment> GetPaymentDetails(string orderId)
        {
            Query paymentsQuery = db.Collection("Payments").WhereEqualTo("OrderId", orderId);
            QuerySnapshot paymentsQuerySnapshot = await paymentsQuery.GetSnapshotAsync();

            DocumentSnapshot documentSnapshot = paymentsQuerySnapshot.Documents.FirstOrDefault();
            try
            {
                if (documentSnapshot.Exists == false) return null;
                else
                {
                    Payment result = documentSnapshot.ConvertTo<Payment>();
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
