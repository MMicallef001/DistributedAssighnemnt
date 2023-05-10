using customerMicroservice.Models;
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
        public virtual DbSet<User> UserMicroContext { get; set; }

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
            if (documentSnapshot.Exists == false) throw new Exception("Book does not exist");
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
        public async Task<User> LoginUser(string id)
        {
            Query booksQuery = db.Collection("Users").WhereEqualTo("Id", id);
            QuerySnapshot booksQuerySnapshot = await booksQuery.GetSnapshotAsync();

            DocumentSnapshot documentSnapshot = booksQuerySnapshot.Documents.FirstOrDefault();
            if (documentSnapshot.Exists == false) return null;
            else
            {
                User result = documentSnapshot.ConvertTo<User>();
                return result;
            }
        }
    }
}
