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

﻿using System;
using System.Configuration.Install;
using System.Diagnostics;
using System.Reflection;
using System.ServiceProcess;

namespace Symon
{
    class SymonMain
    {
        public const string serviceName = "SymonService";

        #region static method Main

        /// <summary>
        /// Application main entry point.
        /// </summary>
        /// <param name="args">Command line argumnets.</param>
        public static void Main(string[] args)
        {
            try
            {
                if (args.Length > 0 && args[0].ToLower() == "/i")
                {
                    ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });

                    ServiceController c = new ServiceController(serviceName);
                    c.Start();
                }
                else if (args.Length > 0 && args[0].ToLower() == "/u")
                {
                    ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                }
                else if (args.Length > 0 && args[0].ToLower() == "/test")
                {
                    using (DataSender sender = new DataSender())
                    {
                        sender.Start();

                        Console.WriteLine("The {0} is ready.", serviceName);
                        Console.WriteLine("Press <ENTER> to terminate service.");
                        Console.WriteLine();
                        Console.ReadLine();

                        sender.Stop();
                    }
                }
                else if (args.Length > 0 && args[0].ToLower() == "/list")
                {
                    Console.WriteLine(IFInfo.getNetIF());

                    return;
                }
                else
                {
                    ServiceBase[] servicesToRun = new ServiceBase[] { new SymonService() };
                    ServiceBase.Run(servicesToRun);
                }
            }
            catch (Exception x)
            {
                Trace.TraceError("Error: {0}", x.ToString());
            }
        }

        #endregion

    }
}
