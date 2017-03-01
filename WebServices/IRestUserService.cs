using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using TestTask.Models;

namespace TestTask.WebService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IRestUserService
    {

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "/Users")
        ]
        List<User> GetUsers();

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "/Users/{nickname}")]
        User GetUserByNickName(string nickname);

        //[OperationContract]
        //[WebInvoke(Method = "POST",
        //    ResponseFormat = WebMessageFormat.Json,
        //    UriTemplate = "/Users",
        //      BodyStyle = WebMessageBodyStyle.Wrapped   )]
        //string CreateUser(User user);

        [OperationContract]
        [WebInvoke(Method = "DELETE",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "/Users/{nickname}",
            BodyStyle = WebMessageBodyStyle.Wrapped)]
        void DeleteUserByNickName(string nickname);

        //[OperationContract]
        //[WebInvoke(Method="PUT",
        //    ResponseFormat = WebMessageFormat.Json,
        //    UriTemplate = "/Users{nickname}")]
        //string GetData(string nickname);
    }
}
