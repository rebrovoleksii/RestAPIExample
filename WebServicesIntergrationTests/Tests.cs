using System;
using System.Linq;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Web.Configuration;  
using System.ServiceModel.Configuration;
using System.Collections.Specialized;

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

        private const string INVALID_NICKNAME_CHARACTER_MESSAGE = "User NickName {0} contains invalid character.";
        private const string USER_ALREADY_EXIST_MESSAGE = "User with the NickName:{0} already exist.";

        private static string [] _invalidNicknameTestCases = new string [] {"&*NickName","Nick()Name","NickName}|"};

        [OneTimeSetUp]
        public void TestSuiteSetup()
        {
            _hostingApplication= LaunchHostingApplication();

            var testSettings = ConfigurationManager.GetSection("testSettings") as NameValueCollection;
            _restClient = new TestRestClient(testSettings["UserServiceURL"]);
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
        public void CreateUser_ReturnsCreatedCodeAndSavesUserInDB_WhenAddingNewUniqueUser()
        {
            var userToCreate = new User() { NickName = "userToCreate", UserName = "Jane J." };
            var response = _restClient.ExecuteRequestWithBody<User>("/Users", userToCreate, Method.POST);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var createdUserInDb = GetUser("userToCreate");

            Assert.AreEqual(userToCreate, createdUserInDb);
        }

        [Test]
        public void CreateUser_ReturnsCreatedCodeAndSavesUserInDB_WhenAddingNewUniqueUserWithoutName()
        {
            var userToCreate = new User() { NickName = "userToCreate"};
            var response = _restClient.ExecuteRequestWithBody<User>("/Users", userToCreate, Method.POST);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var createdUserInDb = GetUser("userToCreate");

            Assert.AreEqual(userToCreate, createdUserInDb);
        }

        [Test]
        public void CreateUser_ReturnsConflictCode_WhenSuchUserAlreadyExistInDB()
        {
            var user = new User() { NickName = "userAlreadyExistInDB", UserName = "Jane" };
            AddUser(user);
            var url = String.Format("/Users");

            var response = _restClient.ExecuteRequestWithBody<User>(url, user, Method.POST);
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual(String.Format(USER_ALREADY_EXIST_MESSAGE, user.NickName), error.ResponseMessage);
        }

        [Test, TestCaseSource("_invalidNicknameTestCases")]
        public void CreateUser_ReturnsBadRequestCode_WhenRequestContainsIllegalCharacter(string invalidNickName)
        {
            var user = new User() { NickName = invalidNickName, UserName = "Jane" };
            var url = "/Users";

            var response = _restClient.ExecuteRequestWithBody<User>(url, user, Method.POST);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual(String.Format(INVALID_NICKNAME_CHARACTER_MESSAGE, invalidNickName), error.ResponseMessage);
        }
                
        [Test]
        public void GetUsers_ReturnsEmptyResponse_WhenNoUserInDB()
        {
            var response = _restClient.ExecuteRequest("/Users", Method.GET);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var users = NewtonsoftJsonSerializer.Default.Deserialize<List<User>>(response);
            Assert.IsEmpty(users);
        }

        [Test]
        public void GetUsers_ReturnsOneUser_WhenOnlyOneUserInDB()
        {
            var expectedUser = new User() { NickName = "SingleUser", UserName = "John" };
            AddUser(expectedUser);

            var response = _restClient.ExecuteRequest("/Users", Method.GET);


            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var users = NewtonsoftJsonSerializer.Default.Deserialize<List<User>>(response);
            Assert.AreEqual(1, users.Count);
            Assert.AreEqual(expectedUser, users[0]);
        }

        [Test]
        public void GetUsers_ReturnsAllUsers_WhenMoreThenOneUserInDB()
        {
            var expectedUser1 = new User() { NickName = "FirstUser1", UserName = "John" };
            var expectedUser2 = new User() { NickName = "SecondUser2", UserName = "Jane" };
            AddUser(expectedUser1);
            AddUser(expectedUser2);

            var response = _restClient.ExecuteRequest("/Users", Method.GET);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var users = NewtonsoftJsonSerializer.Default.Deserialize<List<User>>(response);

            Assert.AreEqual(2, users.Count);
            CollectionAssert.Contains(users, expectedUser1);
            CollectionAssert.Contains(users, expectedUser2);
        }

        [Test, TestCaseSource("_invalidNicknameTestCases")]
        public void GetUserByNickName_ReturnsBadRequestCode_WhenNickNametContainsIllegalCharacter(string invalidNickName)
        {
            var response = _restClient.ExecuteRequest("/Users/" + invalidNickName, Method.GET);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual(String.Format(INVALID_NICKNAME_CHARACTER_MESSAGE, invalidNickName), error.ResponseMessage);
        }

        [Test]
        public void GetUserByNickName_ReturnsBadRequestCode_WhenNickNameIsTooLong()
        {
            // 21 chars long
            var invalidNickName = "MaxNickNameLenIs20aaa";
            var response = _restClient.ExecuteRequest("/Users/" + invalidNickName, Method.GET);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual(String.Format("User NickName {0} is too long.", invalidNickName), error.ResponseMessage);
        }

        [Test]
        public void GetUserByNickName_ReturnsNotFoundCode_WhenNoUserFoundInDB()
        {
            var nickName = "SomeRandomUser321";
            var response = _restClient.ExecuteRequest("/Users/" + nickName, Method.GET);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual(String.Format("User with the NickName:{0} was not found.", nickName), error.ResponseMessage);
        }

        [Test]
        public void GetUserByNickName_ReturnsCorrectUser_WithMinNickNameLength()
        {
            var minLenghtNickname = "1";
            var expectedUser = new User() { NickName = minLenghtNickname, UserName = "Jane" };
            AddUser(expectedUser);

            var response = _restClient.ExecuteRequest("/Users/" + minLenghtNickname, Method.GET);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var userFromResponse = NewtonsoftJsonSerializer.Default.Deserialize<User>(response);
            Assert.AreEqual(expectedUser, userFromResponse);
        }

        [Test]
        public void GetUserByNickName_ReturnsCorrectUser_WithMaxNickNameLength()
        {
            var nickNameMaxvalue = "MaxNickNameLenIs20aa";
            var expectedUser = new User() { NickName = nickNameMaxvalue, UserName = "Jane" };
            AddUser(expectedUser);

            var response = _restClient.ExecuteRequest(String.Format("/Users/{0}",nickNameMaxvalue), Method.GET);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var userFromResponse = NewtonsoftJsonSerializer.Default.Deserialize<User>(response);
            Assert.AreEqual(expectedUser, userFromResponse);
        }

        [Test]
        public void UpdateUserByNickName_ReturnsOkCode_WhenUserUpdateInDB()
        {
            var nickname = "UserToUpdate";
            var initialUser = new User() { NickName = nickname, UserName = "Jane" };
            AddUser(initialUser);

            var updatedUserInRequest = new User() { NickName = nickname, UserName = "Jane J." };
            var response = _restClient.ExecuteRequestWithBody<User>("/Users", updatedUserInRequest, Method.PUT);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var updatedUserInDb = GetUser(nickname);

            Assert.AreEqual(updatedUserInRequest, updatedUserInDb);
        }

        [Test]
        public void UpdateUserByNickName_ReturnsNotFoundCode_WhenNoSuchFoundInDB()
        {
            var nickname = "UserDoesNotExistN0w";
            var user = new User() { NickName = nickname, UserName = "Jane" };
            
            var response = _restClient.ExecuteRequestWithBody<User>("/Users", user, Method.PUT);
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual(String.Format("User with the NickName:{0} was not found.", nickname), error.ResponseMessage);
        }

        [Test, TestCaseSource("_invalidNicknameTestCases")]
        public void UpdateUser_ReturnsBadRequestCode_WhenRequestContainsIllegalCharacter(string invalidNickName)
        {
            var user = new User() { NickName = invalidNickName, UserName = "Jane" };
           
            var response = _restClient.ExecuteRequestWithBody<User>("/Users", user, Method.PUT);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual(String.Format(INVALID_NICKNAME_CHARACTER_MESSAGE, invalidNickName), error.ResponseMessage);
        }

        [Test]
        public void DeleteUserByNickName_ReturnsOkCodeAndUser_WhenUserDeletedInDB()
        {
            var nickname ="UserToDelete";
            var expectedUser = new User() { NickName = nickname, UserName = "Jane" };
            AddUser(expectedUser);

            var response = _restClient.ExecuteRequest("/Users/"+nickname, Method.DELETE);
            var deletedUser = NewtonsoftJsonSerializer.Default.Deserialize<User>(response);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual(expectedUser, deletedUser);

            var checkUserIsNotInDB = _restClient.ExecuteRequest("/Users/" + nickname, Method.GET);
            Assert.AreEqual(HttpStatusCode.NotFound,checkUserIsNotInDB.StatusCode);

        }

        [Test]
        public void DeleteUserByNickName_ReturnsNotFoundCode_WhenNoSuchFoundInDB()
        {
            var response = _restClient.ExecuteRequest("/Users/MyTestUser111", Method.DELETE);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual("User with the NickName:MyTestUser111 was not found.", error.ResponseMessage);
        }

        [Test, TestCaseSource("_invalidNicknameTestCases")]
        public void DeleteUserByNickName_ReturnsBadRequestCode_WhenRequestContainsIllegalCharacter(string invalidNickName)
        {
            var response = _restClient.ExecuteRequest("/Users/" + invalidNickName, Method.DELETE);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);

            var error = NewtonsoftJsonSerializer.Default.Deserialize<ResponseMessageDetails>(response);
            Assert.AreEqual(String.Format(INVALID_NICKNAME_CHARACTER_MESSAGE, invalidNickName), error.ResponseMessage);
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

        private User GetUser(string nickName)
        {
            using (var db = new WebServicesRepository())
            {
                return db.GetUserByNickNameFromDB(nickName);
            }
        }

        #endregion
     }
}
