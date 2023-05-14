using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    [FirestoreData]
    public class Shipment
    {
        [FirestoreProperty]
        public string OrderId { get; set; }

        [FirestoreProperty]
        public string Status { get; set; }


    }
}
