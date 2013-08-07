/*
 * Copyright (c) 20011-2013 Val Hazelwood
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 *
 *    - Redistributions of source code must retain the above copyright
 *      notice, this list of conditions and the following disclaimer.
 *    - Redistributions in binary form must reproduce the above
 *      copyright notice, this list of conditions and the following
 *      disclaimer in the documentation and/or other materials provided
 *      with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
 * FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE
 * COPYRIGHT HOLDERS OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT
 * LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
 * ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 *
 */

using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Symon
{
    [RunInstaller(true)]
    public class Symon_Service_Installer : Installer
    {
        private System.ComponentModel.Container components = null;

        private ServiceInstaller serviceInstaller;
        private ServiceProcessInstaller processInstaller;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Symon_Service_Installer()
        {
            InitializeComponent();
            
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
        
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
        }

    }
}
