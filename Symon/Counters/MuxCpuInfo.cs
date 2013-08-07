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
    public struct MuxCpuInfo
    {
        // CPU = 0x01
        public byte MT_CPU;
        // CPU Num = 0
        public byte number;
        // delim = 0x0
        public byte delim;
        public ushort user;
        public ushort nice;
        public ushort system;
        public ushort interrupt;
        public ushort idle;

        public MuxCpuInfo(string resourceName)
        {
            this.MT_CPU = (byte)MT_Values.MT_CPU_VAL;
            this.number = Encoding.ASCII.GetBytes(resourceName)[0];
            this.delim = 0;
            this.user = 0;
            this.nice = 0;
            this.system = 0;
            this.interrupt = 0;
            this.idle = 0;
        }
    }

    class CPUInfo : IUpdateInfo, IDisposable
    {
        MuxCpuInfo counter;

        PerformanceCounter[] counters;

        bool disposed = false;

        public void PrepareCounters(string resourceName)
        {
            string[] counterNames = new string[] 
            { 
                "% User Time", 
                "% Privileged Time", 
                "% Interrupt Time", 
                "% Idle Time" 
            };

            counters = new PerformanceCounter[counterNames.Length];

            for (int i = 0; i < counterNames.Length; i++)
            {
                counters[i] = new PerformanceCounter("Processor", counterNames[i], "_Total", true);
                counters[i].NextValue();
            }
        }

        ushort[] GetCPUCountersValues()
        {
            ushort[] values = new ushort[counters.Length];

            for (int i = 0; i < counters.Length; i++)
            {
                values[i] = Convert.ToUInt16((Math.Round(counters[i].NextValue() * 100)).ToString());
            }

            return values;
        }

        public ValueType UpdateInfoInStruct()
        {
            ushort[] args = GetCPUCountersValues();

            this.counter.user = Collector.ReverseBytes(args[0]);
            this.counter.nice = 0;
            this.counter.system = Collector.ReverseBytes(args[1]);
            this.counter.interrupt = Collector.ReverseBytes(args[2]);
            this.counter.idle = Collector.ReverseBytes(args[3]);

            return this.counter;
        }

        public CPUInfo(string resourceName)
        {
            counter = new MuxCpuInfo(resourceName);
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

        ~CPUInfo()
        {
            Dispose(false);
        }
    }
}

