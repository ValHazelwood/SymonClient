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

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Symon
{
    public class DataSender : IDisposable
    {
        UdpClient udpclient;

        IPEndPoint endPoint;

        Collector collector;

        NameValueCollection countersInfo;
        Hashtable RemoteHostInfo;

        Thread symonThread;

        AutoResetEvent startedEvent = null;

        bool continueProcess = false;

        bool disposed = false;

        string secGroup = "userSettings";
        string secCounters = "Counters";
        string secRemoteHost = "RemoteHost";
        string secNameHost = "host";
        string secNamePort = "port";

        public const int DataSendInterval = 5000;

        public void Run()
        {
            udpclient = new UdpClient();

            collector = new Collector(countersInfo);

            startedEvent.Set();

            while (continueProcess)
            {
                try
                {
                    byte[] sendBytes = collector.CollectInfoForAllCounters();

                    //ShowBytes(sendBytes);
                    udpclient.Send(sendBytes, sendBytes.Length, endPoint);
                }
                catch (NullReferenceException ex)
                {
                    Trace.TraceError("SendBytes array is null, {0}", ex.ToString());
                }
                catch (SocketException ex)
                {
                    Trace.TraceError("Socket error mesage: {0}, code: {1}", ex.Message, ex.ErrorCode);
                }

                Thread.Sleep(DataSendInterval);
            }

        }

        public void Start()
        {
            symonThread = new Thread(new ThreadStart(this.Run));

            try
            {
                symonThread.Start();

                continueProcess = true;

                this.startedEvent.WaitOne();

                Trace.TraceInformation("Application started.");
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }
        }

        public void Stop()
        {
            continueProcess = false;

            if (symonThread != null && symonThread.IsAlive)
            {
                symonThread.Join();
            }

            if (udpclient != null)
            {
                udpclient.Close();
                udpclient = null;
            }

            if (collector != null)
            {
                collector.Dispose();
                collector = null;
            }

            Trace.TraceInformation("Application stoped.");

            Trace.Close();
        }

        void ShowBytes(byte[] bytes)
        {
            Console.WriteLine(string.Format("Array length: {0}", bytes.Length));

            foreach (byte b in bytes)
                Console.Write(string.Format("0x{0:X2} ", b));

            Console.WriteLine();
        }

        public DataSender()
        {
            #region Trace options
            foreach (TraceListener listener in Trace.Listeners)
            {
                if (listener.GetType().Equals(typeof(EventLogTraceListener)))
                {
                    (listener as EventLogTraceListener).EventLog.Log =
                        (listener as EventLogTraceListener).EventLog.Source;
                }
            }
            #endregion

            CultureInfo ci = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            startedEvent = new AutoResetEvent(false);

            #region Read config settings
            try
            {
                int endPort;
                IPAddress endAddr;

                countersInfo = (NameValueCollection)ConfigurationManager.GetSection(secGroup + "/" + secCounters);

                RemoteHostInfo = (Hashtable)ConfigurationManager.GetSection(secGroup + "/" + secRemoteHost);

                if (countersInfo == null || RemoteHostInfo == null)
                    throw new NullReferenceException("Invalid config options!");

                if (IPAddress.TryParse(RemoteHostInfo[secNameHost].ToString(), out endAddr) &&
                    int.TryParse(RemoteHostInfo[secNamePort].ToString(), out endPort))
                {
                    endPoint = new IPEndPoint(endAddr, endPort);
                }
                else
                {
                    throw new NullReferenceException("Invalid host or port values!");
                }
            }
            catch (NullReferenceException ex)
            {
                Trace.TraceError(ex.ToString());
                return;
            }
            #endregion
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    this.Stop();
                }

                disposed = true;
            }
        }

        ~DataSender()
        {
            Dispose(false);
        }
    }
}
