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
            string username = GetInput("Enter UserName: ");
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


            #region DesktopPool

            //List Desktop pools:
            //create desktop summary query (https://vdc-repo.vmware.com/vmwb-repository/dcr-public/e2e25628-4ed2-43fc-8bad-54fb86f3bb0f/8e4d2491-c740-4778-ac43-ba8fc0ec8175/doc/vdi.query.QueryService.html);
            VMware.Hv.QueryDefinition _qd = new VMware.Hv.QueryDefinition();
            _qd.QueryEntityType = "DesktopSummaryView";

            //perform query
            VMware.Hv.QueryResults qs = vs.QueryService.QueryService_Create(_qd);

            //Delete query when we're finished with it.
            vs.QueryService.QueryService_Delete(qs.Id);
            
            //list desktop summary:
            Console.WriteLine("Desktop Query Results: {0}", qs.Results.Count());
            foreach(VMware.Hv.DesktopSummaryView dsv in qs.Results)
            {
                Console.WriteLine("Desktop Name: {0} - Enabled: {1} - Source: {2}", dsv.DesktopSummaryData.Name, dsv.DesktopSummaryData.Enabled,dsv.DesktopSummaryData.Source);
            }
            #endregion

            #region ApplicationList

            //create application info query (https://vdc-repo.vmware.com/vmwb-repository/dcr-public/e2e25628-4ed2-43fc-8bad-54fb86f3bb0f/8e4d2491-c740-4778-ac43-ba8fc0ec8175/doc/vdi.query.QueryService.html);
            VMware.Hv.QueryDefinition _ad = new VMware.Hv.QueryDefinition();
            _ad.QueryEntityType = "ApplicationInfo";

            //perform query
            VMware.Hv.QueryResults qr = vs.QueryService.QueryService_Create(_ad);

            //Delete query when we're finished with it.
            vs.QueryService.QueryService_Delete(qr.Id);

            //list application info:
            Console.WriteLine("Application Query Results: {0}", qr.Results.Count());
            foreach (VMware.Hv.ApplicationInfo ai in qr.Results)
            {
                Console.WriteLine("Application Name: {0} - Enabled: {1} - Source: {2}", ai.Data.DisplayName, ai.Data.Enabled, ai.ExecutionData.Farm);
            }
            #endregion



            //pull a list of connected VirtualCenters:
            VMware.Hv.VirtualCenterInfo[] vciList = vs.VirtualCenter.VirtualCenter_List();
            //List Virtual Centers:
            Console.WriteLine("Connected vCenters: {0}", vciList.Length);
            foreach (VMware.Hv.VirtualCenterInfo vcInfo in vciList)
            {
                Console.WriteLine("Name: {0}, Enabled {1}, ID {2}", vcInfo.ServerSpec.ServerName,vcInfo.Enabled, vcInfo.Id.Id);
            }



            
            //Check if the system is in a CPA deployment
            try
            {
                VMware.Hv.SiteInfo[] siteList = vs.Site.Site_List();
                if (siteList.Count() > 0)
                {
                    Console.WriteLine("CPA Sites Detected: {0}", siteList.Count());
                    foreach (VMware.Hv.SiteInfo si in siteList)
                    {
                        Console.WriteLine("Site Name: {0} - Pod count {1}", si.Base.DisplayName, si.Pods.Count());
                    }
                }
                else
                {
                    Console.WriteLine("No CPA Sites Detected.");
                }


                VMware.Hv.PodInfo[] podList = vs.Pod.Pod_List();
                if (podList.Count() > 0)
                {
                    Console.WriteLine("Pods Detected: {0}", podList.Count());
                    foreach (VMware.Hv.PodInfo pi in podList)
                    {
                        Console.WriteLine("Pod Name: {0} - Connection Server Count: {1} - Site Name: {2}", pi.DisplayName, pi.Endpoints.Count(), siteList.Where(x => x.Id.Id == pi.Site.Id).First().Base.DisplayName);
                    }

                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("CPA Info not found: {0}", ex.Message);
            }


            //Get Local Sessions - does not look across pod - ping mandrew@vmware.com an email if you want more info
            //create application info query (https://vdc-repo.vmware.com/vmwb-repository/dcr-public/e2e25628-4ed2-43fc-8bad-54fb86f3bb0f/8e4d2491-c740-4778-ac43-ba8fc0ec8175/doc/vdi.query.QueryService.html);
            VMware.Hv.QueryDefinition _slsvq = new VMware.Hv.QueryDefinition();
            _slsvq.QueryEntityType = "SessionLocalSummaryView";

            //perform query
            VMware.Hv.QueryResults slsvqr = vs.QueryService.QueryService_Create(_slsvq);

            //Delete query when we're finished with it.
            vs.QueryService.QueryService_Delete(slsvqr.Id);

            //list application info:
            Console.WriteLine("Session Local Query Results: {0}", slsvqr.Results.Count());
            foreach(VMware.Hv.SessionLocalSummaryView slsv in slsvqr.Results)
            {
                Console.WriteLine("Session state: {0} - Protocol: {1} - From User: {2} - To machine: {3} - of Type: {4}", slsv.SessionData.SessionState,slsv.SessionData.SessionProtocol,slsv.NamesData.UserName,slsv.NamesData.MachineOrRDSServerDNS, slsv.SessionData.SessionType);
            }


            //close connection:
            Console.WriteLine("Closing Connection.");
            VMware.VimAutomation.HorizonView.Interop.V1.ViewServerServiceFactory.HorizonViewService.ClientManager.DisconnectViewServer(i, true);


            Console.ReadLine();
            
        }
    }
}
