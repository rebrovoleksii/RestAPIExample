using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Configuration;

using NUnit.Framework;
using RestSharp;

using TestTask.Models;
using TestTask.WebServicesStorage;

using TestTask.WebServices.TestRestClient;


namespace WebServicesIntergrationTests
{
    [TestFixture]
    public class Tests
    {
        private TestRestClient _restClient;
 
        [OneTimeSetUp]
        public void TestSuiteSetup()
        { 
          _restClient = new TestRestClient("http://localhost:8000"); 
        }

        [SetUp]
        public void TestSetup()
        {
            CleanDB();
        }

        [Test]
        public void GetUsers_ReturnsEmptyResponse_WhenNoUserInDB()
        {
            var response = _restClient.ExecuteRequest("Services/TestService/Users", Method.GET);    
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var users = NewtonsoftJsonSerializer.Default.Deserialize<List<User>>(response);
            Assert.IsEmpty(users);
        }

        [Test]
        public void GetUsers_ReturnsOneUser_WhenOnlyOneUserInDB()
        {
            var expectedUser = new User() { NickName = "49940", UserName = "John" };
            AddUser(expectedUser);

            var response = _restClient.ExecuteRequest("Services/TestService/Users", Method.GET); 
           

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var users = NewtonsoftJsonSerializer.Default.Deserialize<List<User>>(response);
            Assert.AreEqual(1,users.Count);
            Assert.AreEqual(expectedUser, users[0]);
        }

        [Test]
        public void GetUsers_ReturnsAllUsers_WhenMoreThenOneUserInDB()
        {
            var expectedUser1 = new User() { NickName = "49940", UserName = "John" };
            var expectedUser2 = new User() { NickName = "1111", UserName = "Jane" };
            AddUser(expectedUser1);
            AddUser(expectedUser2);

            var response = _restClient.ExecuteRequest("Services/TestService/Users", Method.GET);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
          
            var users = NewtonsoftJsonSerializer.Default.Deserialize<List<User>>(response);
            CollectionAssert.Contains(users,expectedUser1);
            CollectionAssert.Contains(users,expectedUser2);
        }

        [Test]
        public void GetUserByNickName_ReturnsNotFoundCode_WhenNoUserFoundInDB()
        {
            var response = _restClient.ExecuteRequest("Services/TestService/Users/1", Method.GET);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual("User with the NickName:1 was not found.",error.ResponseMessage);
        }

        [Test]
        public void GetUserByNickName_ReturnsCorrectUser_WhenOneFoundInDB()
        {
            var expectedUser = new User() { NickName = "1111", UserName = "Jane" };
            AddUser(expectedUser);

            var response = _restClient.ExecuteRequest("Services/TestService/Users/1111", Method.GET);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var user = NewtonsoftJsonSerializer.Default.Deserialize<User>(response);
            Assert.AreEqual(expectedUser,user);
        }

        private void CleanDB()
        { 
            using (var db = new WebServicesRepository())
            {
                var allUsers = db.GetAllUsersFromDB();
                allUsers.ForEach(user => db.DeleteUser(user.NickName));
            }
        }

        private void AddUser(User user)
        {
            using (var db = new WebServicesRepository())
            {
                db.AddUser(user);
            }
        }

    }
}
