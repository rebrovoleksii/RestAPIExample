using System;
using System.Collections.Generic;
using System.Linq;

using TestTask.CustomExceptions;
using TestTask.Models;

namespace TestTask.WebServicesStorage
{
    public class WebServicesRepository : IDisposable
    {
        private const string USER_NOT_FOUND_MESSAGE = "User with the NickName:{0} was not found.";
        private const string USER_ALREADY_EXISTS_MESSAGE = "User with the NickName:{0} already exist.";

        private readonly WebServiceStorageContext _storage = new WebServiceStorageContext();

        public List<User> GetAllUsersFromDB()
        {
            return _storage.Users.ToList();            
        }

        public User GetUserByNickNameFromDB(string nickname)
        {
            var userToReturn = FindUser(nickname);
            return userToReturn;
        }

        public void AddUser(User user)
        {
            var existingUser = _storage.Users.Find(user.NickName);
            if (existingUser != null)
            {
                var errorMessage = String.Format(USER_ALREADY_EXISTS_MESSAGE, user.NickName);
                throw new UserAlreadyExistException(errorMessage);
            }
            _storage.Users.Add(user);
            _storage.SaveChanges();
        }

        public void UpdateUser(User user)
        {
            var userToUpdate = FindUser(user.NickName);
            userToUpdate.UserName = user.UserName;
            _storage.SaveChanges();
        }

        public User DeleteUser(string nickname)
        {
            var userToDelete = FindUser(nickname);
            _storage.Users.Remove(userToDelete);
            _storage.SaveChanges();
            return userToDelete;
        }

        public void InitializeDB()
        {
            //var user = new User() { UserName = "John", NickName = new Random().Next(100000).ToString() };
            //_storage.Users.Add(user);
            //_storage.SaveChanges();
            _storage.Database.Initialize(true);            
        }

        public void Dispose()
        {
            _storage.Dispose();
        }

        private User FindUser(string nickName)
        {
            var userToReturn = _storage.Users.Find(nickName);
            if (userToReturn == null)
            {
                var errorMessage = String.Format(USER_NOT_FOUND_MESSAGE, nickName);
                throw new UserNotFoundException(errorMessage);
            }
            return userToReturn;
        }
    }
}
