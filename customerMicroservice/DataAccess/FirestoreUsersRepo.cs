using Common.Models;
using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Data.Entity;

namespace customerMicroservice.DataAccess
{
    public class FirestoreUsersRepo
    {
        FirestoreDb db;
        public FirestoreUsersRepo(string project)
        {
            db = FirestoreDb.Create(project);
        }

        public async Task<bool> RegisterUser(User u)
        {
            try
            {
                await db.Collection("Users").Document(u.Id).SetAsync(u);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> Login(string email, string pword)
        {
            Query usersQuery = db.Collection("Users").WhereEqualTo("Email", email);
            QuerySnapshot UsersQuerySnapshot = await usersQuery.GetSnapshotAsync();

            DocumentSnapshot documentSnapshot = UsersQuerySnapshot.Documents.FirstOrDefault();
            if (documentSnapshot.Exists == false)
            {
                return false;
            }
            else
            {
                string passwordFromDb = documentSnapshot.GetValue<string>(new FieldPath("Password")); // Get the password field from 

                if (passwordFromDb.Equals(pword))
                {
                    return true;
                }
                return false;
            }
        }
        public async Task<User> GetUserDetails(string id)
        {
            Query booksQuery = db.Collection("Users").WhereEqualTo("Id", id);
            QuerySnapshot booksQuerySnapshot = await booksQuery.GetSnapshotAsync();

            DocumentSnapshot documentSnapshot = booksQuerySnapshot.Documents.FirstOrDefault();
            try
            {
                if (documentSnapshot.Exists == false) return null;
                else
                {
                    User result = documentSnapshot.ConvertTo<User>();
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
