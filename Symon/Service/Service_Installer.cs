using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Symon
{
    /// <summary>
    /// Mail server service installer.
    /// </summary>
    [RunInstaller(true)]
    public class Symon_Service_Installer : Installer
    {
        /// Required designer variable.
        private System.ComponentModel.Container components = null;

        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller processInstaller;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Symon_Service_Installer()
        {
            // This call is required by the Designer.
            InitializeComponent();

            // TODO: Add any initialization after the InitComponent call
            processInstaller = new ServiceProcessInstaller();
            serviceInstaller = new ServiceInstaller();

            // The services will run under the system account.
            processInstaller.Account = ServiceAccount.LocalSystem;

            // The services will be started automatic
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            // ServiceName must equal those on ServiceBase derived classes.            
            serviceInstaller.ServiceName = SymonMain.serviceName;

            // Add installers to collection. Order is not important.
            Installers.Add(serviceInstaller);
            Installers.Add(processInstaller);
        }

        #region Component Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }
        #endregion

    }
}
