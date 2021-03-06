/*
 * Copyright (c) 2011-2013 Val Hazelwood
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Symon
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MuxIFInfo
    {
        public byte MT_IF;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] deviceName;
        public byte delim;

        public ulong param1;
        public ulong param2;
        public ulong param3;
        public ulong param4;
        public ulong param5;
        public ulong param6;
        public ulong param7;
        public ulong param8;
        public ulong param9;
        public ulong param10;

        public MuxIFInfo(string ifaceIdx)
        {
            this.MT_IF = (byte)MT_Values.MT_IF2_VAL;
            this.deviceName = Encoding.ASCII.GetBytes(string.Format("eth{0}", ifaceIdx));
            this.delim = 0;
            this.param1 = 0;
            this.param2 = 0;
            this.param3 = 0;
            this.param4 = 0;
            this.param5 = 0;
            this.param6 = 0;
            this.param7 = 0;
            this.param8 = 0;
            this.param9 = 0;
            this.param10 = 0;
        }
    }

    class IFInfo : IUpdateInfo, IDisposable
    {
        MuxIFInfo counter;

        PerformanceCounter[] counters;

        bool disposed = false;

        public void PrepareCounters(string resourceName)
        {
            string[] counterNames = new string[] 
            { 
                "Bytes Received/sec", 
                "Bytes Sent/sec"
            };

            counters = new PerformanceCounter[counterNames.Length];

            for (int i = 0; i < counterNames.Length; i++)
            {
                counters[i] = new PerformanceCounter("Network Interface", counterNames[i],
                    getNetIF(int.Parse(resourceName)), true);
                counters[i].NextValue();
            }
        }

        ulong[] GetIFCountersValues()
        {
            ulong[] values = new ulong[counters.Length];

            for (int i = 0; i < counters.Length; i++)
            {
                ulong tmp = 0;
                try
                {
                    tmp = Convert.ToUInt64(
                        (Math.Round(counters[i].NextValue() * (DataSender.DataSendInterval / 1000))).ToString()
                        );
                }
                catch (OverflowException)
                {
                    tmp = ulong.MaxValue;
                }

                values[i] = tmp;
            }

            return values;
        }

        public ValueType UpdateInfoInStruct()
        {
            ulong[] args = GetIFCountersValues();

            ulong recvBytes = Collector.ReverseBytes(this.counter.param3);
            ulong sentBytes = Collector.ReverseBytes(this.counter.param4);

            recvBytes += args[0];
            sentBytes += args[1];

            this.counter.param3 = Collector.ReverseBytes(recvBytes);
            this.counter.param4 = Collector.ReverseBytes(sentBytes);

            return this.counter;
        }

        string getNetIF(int ifaceIdx)
        {
            int count = 0;
            string networkIFName = string.Empty;

            PerformanceCounterCategory cat = new PerformanceCounterCategory("Network Interface");

            foreach (var instance in cat.GetInstanceNames())
            {
                if (count == ifaceIdx) networkIFName = instance;
                count++;
            }

            return networkIFName;
        }

        public static string getNetIF()
        {
            int count = 0;
            StringBuilder networkIfaces = new StringBuilder();

            PerformanceCounterCategory cat = new PerformanceCounterCategory("Network Interface");

            foreach (var instance in cat.GetInstanceNames())
            {
                networkIfaces.AppendFormat("Num: {0} name {1}\n", count, instance);
                count++;
            }

            return networkIfaces.ToString();
        }

        public IFInfo(string resourceName)
        {
            counter = new MuxIFInfo(resourceName);
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
                    foreach (var counter in counters)
                    {
                        counter.Dispose();
                    }

                    counters = null;
                }

                disposed = true;
            }
        }

        ~IFInfo()
        {
            Dispose(false);
        }
    }
}

