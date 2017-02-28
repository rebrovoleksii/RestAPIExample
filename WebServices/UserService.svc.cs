using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using TestTask.Models;
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
            VadlidateNickName(nickname);
            using (var db = new WebServicesRepository())
            {
                var result = db.GetUserByNickNameFromDB(nickname);
                if (result == null)
                {
                    var errorDetails = new ResponseMessageDetails() 
                    {
                        ResponseMessage = String.Format("User with the NickName:{0} was not found.",nickname)
                    };
                    throw new WebFaultException<ResponseMessageDetails>(errorDetails, System.Net.HttpStatusCode.NotFound);
                }
                else return result;
            }
        }

        public void UpdateUserByNickName(User user)
        {
            using (var db = new WebServicesRepository())
            {
                try
                {
                    db.UpdateUser(user);
                    WebOperationContext ctx = WebOperationContext.Current;
                    ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
                }
                catch (NullReferenceException)
                {
                    var errorDetails = new ResponseMessageDetails()
                    {
                        ResponseMessage = String.Format("User with the NickName:{0} was not found.", user.NickName)
                    };
                    throw new WebFaultException<ResponseMessageDetails>(errorDetails, System.Net.HttpStatusCode.NotFound);
                }
                catch (Exception)
                {
                    var errorDetails = new ResponseMessageDetails()
                    {
                        ResponseMessage = String.Format("Something went wrong.")
                    };
                    throw new WebFaultException<ResponseMessageDetails>(errorDetails, System.Net.HttpStatusCode.InternalServerError);
                
                }
            }
        }

        public void DeleteUserByNickName(string nickname)
        {
            VadlidateNickName(nickname);
            using (var db = new WebServicesRepository())
            {
                try
                {
                    db.DeleteUser(nickname);
                    WebOperationContext ctx = WebOperationContext.Current;
                    ctx.OutgoingResponse.StatusCode = System.Net.HttpStatusCode.OK;
                }
                catch (ApplicationException)
                {
                    var errorDetails = new ResponseMessageDetails()
                    {
                        ResponseMessage = String.Format("User with the NickName:{0} was not found.", nickname)
                    };
                    throw new WebFaultException<ResponseMessageDetails>(errorDetails, System.Net.HttpStatusCode.NotFound);
                }
            }
        }

        private void VadlidateNickName(string nickname)
        {
            if (!nickname.All(char.IsLetterOrDigit))
            {
                var errorDetails = new ResponseMessageDetails()
                {
                    ResponseMessage = String.Format("User NickName contains invalid character.")
                };
                throw new WebFaultException<ResponseMessageDetails>(errorDetails, System.Net.HttpStatusCode.BadRequest);
            }
        }

        public static void ServiceInitizializationEventHander(object sender, EventArgs args)
        {
            using (var db = new WebServicesRepository())
            {
                db.InitializeDB();
            }
        }
    }
}
