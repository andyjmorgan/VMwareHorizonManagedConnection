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

        static string GetInput(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        static void Main(string[] args)
        {

            //Funny certificate / ssl issues:
            IgnoreBadCertificates();

            //Set access details:
            string connectionServer = GetInput("Enter Connection Server: ");
            string username = GetInput("Enter UserName : ");
            string pass = GetInput("Enter Password: ");
            string domain = GetInput("Enter domain: ");


            //clear console to remove username from prompt:
            Console.Clear();
            Console.WriteLine("Connecting to: {0}", connectionServer);
            //create a network credential to retrieve the SecureString needed.
            SecureString ss = new NetworkCredential(username, pass).SecurePassword;

            //Connect to Horizon Connection server:
            VMware.VimAutomation.HorizonView.Interop.V1.ViewServerInterop i = VMware.VimAutomation.HorizonView.Interop.V1.ViewServerServiceFactory.HorizonViewService.ClientManager.ConnectViewServer(connectionServer, username, ss, domain);

            //retrieve the needed extension data to do stuff:
            VMware.Hv.Services vs =(VMware.Hv.Services) i.ExtensionData;




            //List Desktop pools:
            //create desktop summary query (https://vdc-repo.vmware.com/vmwb-repository/dcr-public/e2e25628-4ed2-43fc-8bad-54fb86f3bb0f/8e4d2491-c740-4778-ac43-ba8fc0ec8175/doc/vdi.query.QueryService.html);
            VMware.Hv.QueryDefinition _qd = new VMware.Hv.QueryDefinition();
            _qd.QueryEntityType = "DesktopSummaryView";

            //perform query
            VMware.Hv.QueryResults qs = vs.QueryService.QueryService_Create(_qd);

            //Delete query when we're finished with it.
            vs.QueryService.QueryService_Delete(qs.Id);
            
            //list desktop summary:
            Console.WriteLine("Desktop Query Results: {0}", qs.Results);
            foreach(VMware.Hv.DesktopSummaryView dsv in qs.Results)
            {
                Console.WriteLine("Desktop Name: {0} - Enabled: {1} - Source: {2}", dsv.DesktopSummaryData.Name, dsv.DesktopSummaryData.Enabled,dsv.DesktopSummaryData.Source);
            }
            
            //pull a list of connected VirtualCenters:
            VMware.Hv.VirtualCenterInfo[] vciList = vs.VirtualCenter.VirtualCenter_List();
            //List Virtual Centers:
            Console.WriteLine("Connected vCenters: {0}", vciList.Length);
            foreach (VMware.Hv.VirtualCenterInfo vcInfo in vciList)
            {
                Console.WriteLine("Name: {0}, Enabled {1}, ID {2}", vcInfo.ServerSpec.ServerName,vcInfo.Enabled, vcInfo.Id.Id);
            }

            //close connection:
            Console.WriteLine("Closing Connection.");
            VMware.VimAutomation.HorizonView.Interop.V1.ViewServerServiceFactory.HorizonViewService.ClientManager.DisconnectViewServer(i, true);


            Console.ReadLine();
            
        }
    }
}
