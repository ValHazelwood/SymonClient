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
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Symon
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MuxMemInfo
    {
        public byte MT_MEM;

        public byte delim;
        public ulong active;
        public ulong real;
        public ulong free;
        public ulong swapUsed;
        public ulong swapTotal;

        public MuxMemInfo(string resourceName)
        {
            this.MT_MEM = (byte)MT_Values.MT_MEM2_VAL;
            this.delim = 0;
            this.real = 0;
            this.free = 0;
            this.active = 0;
            this.swapUsed = 0;
            this.swapTotal = 0;
        }
    }

    class MEMInfo : IUpdateInfo, IDisposable
    {
        MuxMemInfo counter;

        PerformanceCounter[] counters;

        bool disposed = false;

        public void PrepareCounters(string resourceName)
        {
            string[] counterNames = new string[] 
            { 
                "Available Bytes",              // Free memory
                "Committed Bytes",              // Page file used
                "Commit Limit"                  // Page file total
            };

            counters = new PerformanceCounter[(counterNames.Length) + 1];

            for (int i = 0; i < counterNames.Length; i++)
            {
                counters[i] = new PerformanceCounter("Memory", counterNames[i], string.Empty, true);
                counters[i].NextValue();
            }

            // Active and real memory
            counters[counterNames.Length] = new PerformanceCounter("Process", "Working Set", "_Total", true);
            counters[counterNames.Length].NextValue();
        }

        ulong[] GetMEMCountersValues()
        {
            ulong[] values = new ulong[counters.Length];

            for (int i = 0; i < counters.Length; i++)
            {
                ulong tmp = 0;
                try
                {
                    tmp = Convert.ToUInt64((Math.Round(counters[i].NextValue())).ToString());
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
            ulong[] args = GetMEMCountersValues();

            this.counter.real = Collector.ReverseBytes(args[3]);
            this.counter.free = Collector.ReverseBytes(args[0]);
            this.counter.active = Collector.ReverseBytes(args[3]);
            this.counter.swapUsed = Collector.ReverseBytes(args[1]);
            this.counter.swapTotal = Collector.ReverseBytes(args[2]);

            return this.counter;
        }

        public MEMInfo(string resourceName)
        {
            counter = new MuxMemInfo(resourceName);
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

        ~MEMInfo()
        {
            Dispose(false);
        }
    }

}
