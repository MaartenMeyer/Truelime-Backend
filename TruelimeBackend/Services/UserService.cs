using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TruelimeBackend.Helpers;
using TruelimeBackend.Models;

namespace TruelimeBackend.Services
{
    public interface IUserService
    {
        User Authenticate(string email, string password);
        User Create(User user, string password);
        User Get(string id);
        User GetById(string id);
        void Remove(string id);
    }
    public class UserService : IUserService
    {
        private readonly IMongoCollection<User> users;
        private readonly Settings settings;

        public UserService(IOptions<Settings> settings, DatabaseSettings.IDatabaseSettings databaseSettings)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DatabaseName);

            users = database.GetCollection<User>(databaseSettings.UsersCollectionName);
            this.settings = settings.Value;
        }
        public User Authenticate(string email, string password)
        {
            var user = users.Find<User>(u => u.Email == email).FirstOrDefault();
            if (user == null)
            {
                return null;
            }

            if (!VerifyPasswordHash(password, user.Hash, user.Salt))
            {
                return null;
            }

            return user;
        }

        public User Create(User user, string password)
        {
            byte[] hash, salt;
            CreatePasswordHash(password, out hash, out salt);

            user.Hash = hash;
            user.Salt = salt;

            users.InsertOne(user);

            return user;
        }

        public User Get(string email)
        {
            return users.Find<User>(u => u.Email == email).FirstOrDefault();
        }

        public User GetById(string id)
        {
            return users.Find<User>(u => u.Id == id).FirstOrDefault();
        }

        public void Remove(string id) =>
            users.DeleteOne(user => user.Id == id);

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt) {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512()) {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt) {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt)) {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++) {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}