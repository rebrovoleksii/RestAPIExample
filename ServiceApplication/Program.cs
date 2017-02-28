using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Description;

using TestTask.WebService;

namespace TestTask.ServiceHostApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            WebServiceHost host = new WebServiceHost(typeof(UserService));
            host.Opening += UserService.ServiceInitizializationEventHander;
            host.Open();
            Console.ReadLine();            
        }
    }
}
