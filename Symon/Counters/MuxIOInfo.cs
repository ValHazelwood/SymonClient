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
    public struct MuxIOInfo
    {
        // IO = 0x09
        public byte MT_IO;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] deviceName;
        // delim = 0x0
        public byte delim;
        public ulong diskReadAccess;
        public ulong diskWriteAccess;
        // Not used
        public ulong diskSeek;
        public ulong diskReadSpeed;
        public ulong diskWriteSpeed;

        public MuxIOInfo(string resourceName)
        {
            string diskName = string.Empty;

            switch (resourceName.ToUpper())
            {
                case "C:":
                    diskName = "ad0";
                    break;
                case "D:":
                    diskName = "ad1";
                    break;
                case "E:":
                    diskName = "ad2";
                    break;
                case "F:":
                    diskName = "ad3";
                    break;
            }

            this.MT_IO = (byte)MT_Values.MT_IO_VAL;
            this.deviceName = Encoding.ASCII.GetBytes(diskName);
            this.delim = 0;
            this.diskReadAccess = 0;
            this.diskWriteAccess = 0;
            this.diskSeek = 0;
            this.diskReadSpeed = 0;
            this.diskWriteSpeed = 0;
        }
    }

    class IOInfo : IUpdateInfo, IDisposable
    {
        MuxIOInfo counter;

        PerformanceCounter[] counters;

        bool disposed = false;

        public void PrepareCounters(string resourceName)
        {
            string[] counterNames = new string[] 
            { 
                "Disk Reads/sec", 
                "Disk Writes/sec", 
                "Disk Read Bytes/sec", 
                "Disk Write Bytes/sec" 
            };

            counters = new PerformanceCounter[counterNames.Length];

            for (int i = 0; i < counterNames.Length; i++)
            {
                counters[i] = new PerformanceCounter("LogicalDisk", counterNames[i], resourceName, true);
                counters[i].NextValue();
            }
        }

        ulong[] GetIOCountersValues()
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
            ulong[] args = GetIOCountersValues();

            ulong diskReadAccess = Collector.ReverseBytes(this.counter.diskReadAccess);
            ulong diskWriteAccess = Collector.ReverseBytes(this.counter.diskWriteAccess);
            ulong diskReadSpeed = Collector.ReverseBytes(this.counter.diskReadSpeed);
            ulong diskWriteSpeed = Collector.ReverseBytes(this.counter.diskWriteSpeed);

            diskReadAccess += args[0];
            diskWriteAccess += args[1];
            diskReadSpeed += args[2];
            diskWriteSpeed += args[3];

            this.counter.diskReadAccess = Collector.ReverseBytes(diskReadAccess);
            this.counter.diskWriteAccess = Collector.ReverseBytes(diskWriteAccess);
            this.counter.diskSeek = 0;
            this.counter.diskReadSpeed = Collector.ReverseBytes(diskReadSpeed);
            this.counter.diskWriteSpeed = Collector.ReverseBytes(diskWriteSpeed);

            return this.counter;
        }

        public IOInfo(string resourceName)
        {
            counter = new MuxIOInfo(resourceName);
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

        ~IOInfo()
        {
            Dispose(false);
        }
    }


}

