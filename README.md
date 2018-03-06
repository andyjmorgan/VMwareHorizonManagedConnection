# VMwareHorizonManagedConnection
This project is an example of how to connect to a VMware Horizon environment natively in c#. While it's well documented on how to do this with powershell, c# and .Net are less well documented.

In order to use this code sample, first install the VMware Powercli libraries on your machine (https://code.vmware.com/web/dp/tool/vmware-powercli/10.0.0)

add the following references to the project:

c:\Program Files (x86)\VMware\Infrastructure\PowerCLI\Modules\VMware.VimAutomation.HorizonView:

VMware.VimAutomation.HorizonView.Commands.dll

VMware.VimAutomation.HorizonView.Impl.dll

VMware.VimAutomation.HorizonView.Interop.dll

VMware.VimAutomation.HorizonView.Types.dll

c:\Program Files (x86)\VMware\Infrastructure\PowerCLI\Modules\VMware.VimAutomation.Sdk:

VMware.VimAutomation.Sdk.Impl.dll

VMware.VimAutomation.Sdk.Interop.dll

VMware.VimAutomation.Sdk.Types.dll

VMware.VimAutomation.Sdk.Util10.dll
