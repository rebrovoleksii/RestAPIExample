using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;

using TestTask.Models;

namespace TestTask.WebService
{
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

        [OperationContract]
        [WebInvoke(Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "/Users",
            BodyStyle = WebMessageBodyStyle.Bare)]
        void CreateUser(User user);

        [OperationContract]
        [WebInvoke(Method = "DELETE",
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "/Users/{nickname}",
            BodyStyle = WebMessageBodyStyle.Bare)]
        User DeleteUserByNickName(string nickname);

        [OperationContract]
        [WebInvoke(Method = "PUT",
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "/Users",
            BodyStyle = WebMessageBodyStyle.Bare)]
        void UpdateUserByNickName(User user);
    }
}
