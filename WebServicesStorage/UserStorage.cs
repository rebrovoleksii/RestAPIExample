using System;
using System.Collections.Generic;
using System.Data.Entity;

using TestTask.Models;

namespace TestTask.WebServicesStorage
{
    internal class WebServiceStorageContext : DbContext
    {
        public WebServiceStorageContext()
            : base("name = WebServicesDBConnectionString")
            //: base("WebServicesDB")
        {
            Database.SetInitializer<WebServiceStorageContext>(new CreateDatabaseIfNotExists<WebServiceStorageContext>());
        }

        public DbSet<User> Users { get; set; }
    }
}
