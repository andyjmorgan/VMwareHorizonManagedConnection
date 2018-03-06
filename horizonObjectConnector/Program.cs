using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace horizonObjectConnector
{

    class Program
    {
        public static void IgnoreBadCertificates()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
        }
        private static bool AcceptAllCertifications(object sender, System.Security.Cryptography.X509Certificates.X509Certificate certification, System.Security.Cryptography.X509Certificates.X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            return true;
            
        }

        static void Main(string[] args)
        {

            //Set your credentials:
            string connectionServer = "hcon1.lab.local";
            string pass = "F0rgetmenot10l!?";
            string username = "andy";
            string domain = "lab";
            Console.ReadLine();
           
            //Funny certificate / ssl issues:
            IgnoreBadCertificates();

            //create a network credential to retrieve the SecureString needed.
            SecureString ss = new NetworkCredential(username, pass).SecurePassword;


            //Connect to Horizon Connection server:
            VMware.VimAutomation.HorizonView.Interop.V1.ViewServerInterop i = VMware.VimAutomation.HorizonView.Interop.V1.ViewServerServiceFactory.HorizonViewService.ClientManager.ConnectViewServer(connectionServer, username, ss, domain);

            //retrieve the needed extension data to do stuff:
            VMware.Hv.Services vs =(VMware.Hv.Services) i.ExtensionData;

            //pull a list of connected VirtualCenters:
            VMware.Hv.VirtualCenterInfo[] vciList = vs.VirtualCenter.VirtualCenter_List();

            //List Virtual Centers:
            foreach (VMware.Hv.VirtualCenterInfo vcInfo in vciList)
            {
                Console.WriteLine("Name: {0}, Enabled {1}, ID {2}", vcInfo.ServerSpec.ServerName,vcInfo.Enabled, vcInfo.Id.Id);
            }

            Console.ReadLine();
            
        }
    }
}
