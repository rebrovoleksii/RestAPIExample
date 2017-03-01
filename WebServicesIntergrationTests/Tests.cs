using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;

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
        private Process _hostingApplication;

        [OneTimeSetUp]
        public void TestSuiteSetup()
        {
            _hostingApplication= LaunchHostingApplication();
            _restClient = new TestRestClient("http://localhost:8000");
        }

        [OneTimeTearDown]
        public void CleanUpTestSuite()
        {
            _hostingApplication.Kill();
        }

        [SetUp]
        public void TestSetup()
        {
            CleanDB();
        }

        #region Tests
        
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
            Assert.AreEqual(1, users.Count);
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
            CollectionAssert.Contains(users, expectedUser1);
            CollectionAssert.Contains(users, expectedUser2);
        }

        [TestCase("&*NickName")]
        [TestCase("Nick()Name")]
        [TestCase("NickName}|")]
        [Test]
        public void GetUserByNickName_ReturnsBadRequestCode_WhenNickNametContainsIllegalCharacter(string invalidNickName)
        {
            var response = _restClient.ExecuteRequest("Services/TestService/Users/" + invalidNickName, Method.GET);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual(String.Format("User NickName {0} contains invalid character.", invalidNickName), error.ResponseMessage);
        }

        [Test]
        public void GetUserByNickName_ReturnsBadRequestCode_WhenNickNameIsTooLong()
        {
            // 21 chars long
            var invalidNickName = "MaxNickNameLenIs20aaa";
            var response = _restClient.ExecuteRequest("Services/TestService/Users/" + invalidNickName, Method.GET);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual(String.Format("User NickName {0} is too long.", invalidNickName), error.ResponseMessage);
        }

        [Test]
        public void GetUserByNickName_ReturnsNotFoundCode_WhenNoUserFoundInDB()
        {
            var response = _restClient.ExecuteRequest("Services/TestService/Users/1", Method.GET);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual("User with the NickName:1 was not found.", error.ResponseMessage);
        }

        [Test]
        public void GetUserByNickName_ReturnsCorrectUser_WithMinNickNameLength()
        {
            var expectedUser = new User() { NickName = "1", UserName = "Jane" };
            AddUser(expectedUser);

            var response = _restClient.ExecuteRequest("Services/TestService/Users/1", Method.GET);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var user = NewtonsoftJsonSerializer.Default.Deserialize<User>(response);
            Assert.AreEqual(expectedUser, user);
        }

        [Test]
        public void GetUserByNickName_ReturnsCorrectUser_WithMaxNickNameLength()
        {
            var nickNameMaxvalue = "MaxNickNameLenIs20aa";
            var expectedUser = new User() { NickName = nickNameMaxvalue, UserName = "Jane" };
            AddUser(expectedUser);

            var response = _restClient.ExecuteRequest(String.Format("Services/TestService/Users/{0}",nickNameMaxvalue), Method.GET);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var user = NewtonsoftJsonSerializer.Default.Deserialize<User>(response);
            Assert.AreEqual(expectedUser, user);
        }



        [Test]
        public void DeleteUserByNickName_ReturnsNotFoundCode_WhenNoSuchFoundInDB()
        {
            var response = _restClient.ExecuteRequest("Services/TestService/Users/MyTestUser111", Method.DELETE);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual("User with the NickName:MyTestUser111 was not found.", error.ResponseMessage);
        }

        [TestCase("&*NickName")]
        [TestCase("Nick()Name")]
        [TestCase("NickName}|")]
        [Test]
        public void DeleteUserByNickName_ReturnsBadRequestCode_WhenRequestContainsIllegalCharacter(string invalidNickName)
        {
            var response = _restClient.ExecuteRequest("Services/TestService/Users/" + invalidNickName, Method.DELETE);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual(String.Format("User NickName {0} contains invalid character.", invalidNickName), error.ResponseMessage);
        }

        #endregion

        #region Private helper methods

        private Process LaunchHostingApplication()
        {
            var path = AppDomain.CurrentDomain.BaseDirectory;
            var appFullPath = Path.Combine(path, "ServiceApplication.exe");
            return Process.Start(appFullPath);
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

        #endregion
     }
}
