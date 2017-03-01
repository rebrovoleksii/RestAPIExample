using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TestTask.CustomExceptions;
using TestTask.Models;

namespace TestTask.WebServicesStorage
{
    public class WebServicesRepository : IDisposable
    {
        private readonly WebServiceStorageContext _storage = new WebServiceStorageContext();

        public List<User> GetAllUsersFromDB()
        {
            return _storage.Users.ToList();            
        }

        public User GetUserByNickNameFromDB(string nickname)
        {
            return _storage.Users.Find(nickname);
        }

        public void AddUser(User user)
        {
            _storage.Users.Add(user);
            _storage.SaveChanges();
        }

        public void UpdateUser(User user)
        {
            var userToUpdate = _storage.Users.Find(user.NickName);
            if (userToUpdate == null)
                throw new UserNotFoundException();
            userToUpdate.UserName = user.NickName;
            _storage.SaveChanges();
        }

        public void DeleteUser(string nickname)
        {
            var userToDelete = _storage.Users.Find(nickname);
            if (userToDelete == null)
                throw new UserNotFoundException();
            _storage.Users.Remove(userToDelete);
            _storage.SaveChanges();
        }

        public void InitializeDB()
        {
            var user = new User() { UserName = "John", NickName = new Random().Next(100000).ToString() };
            _storage.Users.Add(user);
            _storage.SaveChanges();
            _storage.Database.Initialize(true);            
        }

        public void Dispose()
        {
          _storage.Dispose();
        }
    }
}
