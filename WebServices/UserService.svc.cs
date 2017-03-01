using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.ComponentModel.DataAnnotations;

using TestTask.Models;
using TestTask.CustomExceptions;
using TestTask.WebServicesStorage;

using System.Data.Entity;

namespace TestTask.WebService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
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

            using (var db = new WebServicesRepository())
            {
                var result = db.GetUserByNickNameFromDB(nickname);
                if (result == null)
                {
                    var errorDetails = new ResponseMessageDetails()
                    {
                        ResponseMessage = String.Format("User with the NickName:{0} was not found.", nickname)
                    };
                    throw new WebFaultException<ResponseMessageDetails>(errorDetails, System.Net.HttpStatusCode.NotFound);
                }
                else return result;
            }
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
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                }
                catch(UserAlreadyExistException)
                {
                    
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
                catch (UserNotFoundException)
                {
                    var errorMessage = String.Format("User with the NickName:{0} was not found.", user.NickName);
                    ReturnErrorCode(errorMessage, HttpStatusCode.NotFound);
                }
                catch (Exception)
                {
                    ReturnErrorCode("Something went wrong.", HttpStatusCode.InternalServerError);
                }
            }
        }

        public void DeleteUserByNickName(string nickname)
        {
            ValidateNickNameForIllegalChars(nickname);
            ValidateNickNameLength(nickname);

            using (var db = new WebServicesRepository())
            {
                try
                {
                    db.DeleteUser(nickname);
                    WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.OK;
                }
                catch (UserNotFoundException)
                {
                    var errorMessage = String.Format("User with the NickName:{0} was not found.", nickname);
                    ReturnErrorCode(errorMessage,HttpStatusCode.NotFound);
                }
                catch (Exception)
                {
                    ReturnErrorCode("Something went wrong.", HttpStatusCode.InternalServerError);
                }
            }
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
