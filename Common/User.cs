﻿using Google.Cloud.Firestore;

namespace Common.Models
{

    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public string Id { get; set; }

        [FirestoreProperty]
        public string Name { get; set; }

        [FirestoreProperty]
        public string SurName { get; set; }
        [FirestoreProperty]
        public string Email { get; set; }
        [FirestoreProperty]
        public string Password { get; set; }
        [FirestoreProperty]
        public string PhoneNumber { get; set; }
        [FirestoreProperty]
        public string address { get; set; }


    }
}
