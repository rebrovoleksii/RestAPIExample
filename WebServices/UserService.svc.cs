using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ComponentModel.DataAnnotations;

using TestTask.Models;
using TestTask.CustomExceptions;
using TestTask.WebServicesStorage;

using System.Data.Entity;

namespace TestTask.WebService
{
    public class UserService : IRestUserService
    {
        public List<User> GetUsers()
        {
            using (var db = new WebServicesRepository())
            {
                return db.GetAllUsersFromDB();
            }
        }

        public User GetUserByNickName(string nickname)
        {
            ValidateNickNameForIllegalChars(nickname);
            ValidateNickNameLength(nickname);

            User retrievedUser = null;

            using (var db = new WebServicesRepository())
            {
                try
                {
                    retrievedUser = db.GetUserByNickNameFromDB(nickname);
                }
                catch (UserNotFoundException exception)
                {
                    ReturnErrorCode(exception.Message, HttpStatusCode.NotFound);
                }
                catch (Exception)
                {
                    ReturnErrorCode("Something went wrong.", HttpStatusCode.InternalServerError);
                }
            }

            return retrievedUser;
        }

        public void CreateUser(User user)
        {
            ValidateNickNameForIllegalChars(user.NickName);
            ValidateNickNameLength(user.NickName);

            using (var db = new WebServicesRepository())
            {
                try
                {
                    db.AddUser(user);
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Created;
                }
                catch(UserAlreadyExistException exception)
                {
                    ReturnErrorCode(exception.Message, HttpStatusCode.Conflict);
                }
                catch (Exception)
                {
                    ReturnErrorCode("Something went wrong.", HttpStatusCode.InternalServerError);
                }
            }
        }

        public void UpdateUserByNickName(User user)
        {
            ValidateNickNameForIllegalChars(user.NickName);
            ValidateNickNameLength(user.NickName);

            using (var db = new WebServicesRepository())
            {
                try
                {
                    db.UpdateUser(user);
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                }
                catch (UserNotFoundException exception)
                {
                    ReturnErrorCode(exception.Message, HttpStatusCode.NotFound);
                }
                catch (Exception)
                {
                    ReturnErrorCode("Something went wrong.", HttpStatusCode.InternalServerError);
                }
            }
        }

        public User DeleteUserByNickName(string nickname)
        {
            ValidateNickNameForIllegalChars(nickname);
            ValidateNickNameLength(nickname);

            User deletedUser = null;

            using (var db = new WebServicesRepository())
            {
                try
                {
                    deletedUser = db.DeleteUser(nickname);
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                }
                catch (UserNotFoundException exception)
                {
                    ReturnErrorCode(exception.Message, HttpStatusCode.NotFound);
                }
                catch (Exception)
                {
                    ReturnErrorCode("Something went wrong.", HttpStatusCode.InternalServerError);
                }
            }

            return deletedUser;
        }

        public static void ServiceInitizializationEventHander(object sender, EventArgs args)
        {
            using (var db = new WebServicesRepository())
            {
                db.InitializeDB();
            }
        }

        private void ValidateNickNameLength(string nickname)
        {
            StringLengthAttribute userNickNameAttr = typeof(User).GetProperties()
                            .Where(p => p.Name == "NickName")
                            .Single()
                            .GetCustomAttributes(typeof(StringLengthAttribute), true)
                            .Single() as StringLengthAttribute;

            if (nickname.Length > userNickNameAttr.MaximumLength)
            {
                var responseMessage = String.Format("User NickName {0} is too long.",nickname);
                ReturnErrorCode(responseMessage, HttpStatusCode.BadRequest);
            }
   
        }

        private void ValidateNickNameForIllegalChars(string nickname)
        {
            if (!nickname.All(char.IsLetterOrDigit))
            {
                var responseMessage = String.Format("User NickName {0} contains invalid character.", nickname);
                ReturnErrorCode(responseMessage, HttpStatusCode.BadRequest);
            }
        }

        private void ReturnErrorCode(string message,HttpStatusCode code)
        {
            var errorDetails = new ResponseMessageDetails() {ResponseMessage = message};
            throw new WebFaultException<ResponseMessageDetails>(errorDetails, code);
        }
    }
}
