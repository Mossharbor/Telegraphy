using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace UnitTests
{
    public class Connections
    {
        public static string EventHubConnectionString 
        {
            get { return ConfigurationManager.AppSettings["EventHubConnectionString"]; }
        }
        public static string StorageConnectionString
        {
            get { return ConfigurationManager.AppSettings["StorageConnectionString"]; }
        }
        public static string ServiceBusConnectionString
        {
            get { return ConfigurationManager.AppSettings["ServiceBusConnectionString"]; }
        }
        public static string HybridRelayConnectionString
        {
            get { return ConfigurationManager.AppSettings["RelayConnectionString"]; }
        }
        public static string WcfRelayConnectionString
        {
            get { return ConfigurationManager.AppSettings["WcfRelayConnectionString"]; }
        }
        public static string EmailAccount
        {
            get { return ConfigurationManager.AppSettings["EmailAccount"]; }
        }
        public static string EmailAccountPassword
        {
            get { return ConfigurationManager.AppSettings["EmailAccountPassword"]; }
        }
    }
}
