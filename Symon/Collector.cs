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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Symon
{  
    public class Collector : IDisposable
    {
        List<IUpdateInfo> counters;

        MuxHeader header;
        MuxData sendData;
        Crc crc;

        bool disposed = false;

        #region ReverseBytes functions
        // reverse byte order (16-bit)
        public static ushort ReverseBytes(ushort value)
        {
            return (ushort)((value & 0xFFU) << 8 | (value & 0xFF00U) >> 8);
        }

        // reverse byte order (32-bit)
        public static uint ReverseBytes(uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                    (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        // reverse byte order (64-bit)
        public static ulong ReverseBytes(ulong value)
        {
            return (value & 0x00000000000000FFUL) << 56 | (value & 0x000000000000FF00UL) << 40 |
                    (value & 0x0000000000FF0000UL) << 24 | (value & 0x00000000FF000000UL) << 8 |
                    (value & 0x000000FF00000000UL) >> 8 | (value & 0x0000FF0000000000UL) >> 24 |
                    (value & 0x00FF000000000000UL) >> 40 | (value & 0xFF00000000000000UL) >> 56;
        }

        #endregion

        byte[] FinalizePacket(MuxHeader header, MuxData sendData, ushort packetLength)
        {
            uint unixTime = (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            header.packetLength = ReverseBytes(packetLength);

            header.timestamp = ReverseBytes(unixTime);

            sendData.header = header;

            // Resize byte array
            byte[] sendDataBytes = StructToByteArray(sendData);
            
            Array.Resize(ref sendDataBytes, packetLength);

            // Calculate crc
            byte[] crc32 = crc.ComputeChecksumBytes(sendDataBytes);

            Array.Reverse(crc32);

            // Add crc to header
            Buffer.BlockCopy(crc32,0,sendDataBytes,0,crc32.Length);

            return sendDataBytes;

        }

        byte[] StructToByteArray<T>(T structure)
        {
            int size = Marshal.SizeOf(structure);

            IntPtr ptr = IntPtr.Zero;

            try
            {
                ptr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(structure, ptr, true);

                byte[] buffer = new byte[size];

                Marshal.Copy(ptr, buffer, 0, size);

                return buffer;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }

        }
        
        public byte[] CollectInfoForAllCounters()
        {
            int destIndex = 0;

            ushort headerLength = (ushort)StructToByteArray(header).Length;

            foreach (IUpdateInfo counter in this.counters)
            {
                MethodInfo method = counter.GetType().GetMethod("UpdateInfoInStruct");

                ValueType objStruct = (ValueType)method.Invoke(counter, new object[] { });

                AddCounterInfoToPacket(objStruct, ref sendData.data, ref destIndex);
            }

            // Calculate packet length
            ushort packetLength = (ushort)(headerLength + destIndex);

            return FinalizePacket(header, sendData, packetLength);

        }
       
        void PrepareAllCounters(NameValueCollection countersInfo)
        {
            foreach (var counter in countersInfo.AllKeys)
            {
                string resourceName;

                Regex RE = new Regex(@"^\w+\((?<resource>\S+)\)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                Match match = RE.Match(counter.ToString());

                if (match.Success)
                {
                    resourceName = match.Groups["resource"].Value;
                }
                else
                {
                    resourceName = string.Empty;
                }

                Type type = Type.GetType(countersInfo[counter], true, true);

                IUpdateInfo objInfo = (IUpdateInfo)Activator.CreateInstance(type, new object[] { resourceName });

                MethodInfo method = objInfo.GetType().GetMethod("PrepareCounters");

                method.Invoke(objInfo, new object[] { resourceName });

                counters.Add(objInfo);

            }

        }
        
        void AddCounterInfoToPacket<T>(T structure, ref byte[] data, ref int destIndex)
        {
            byte[] sensorDataBytes = StructToByteArray(structure);

            Buffer.BlockCopy(sensorDataBytes, 0, data, destIndex, sensorDataBytes.Length);

            destIndex += sensorDataBytes.Length;
        }
        
        public Collector(NameValueCollection countersInfo)
        {
            crc = new Crc();

            header = new MuxHeader(true);

            sendData = new MuxData(true);

            counters = new List<IUpdateInfo>();

            PrepareAllCounters(countersInfo);
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

                    crc = null;
                }

                disposed = true;
            }
        }

        ~Collector()
        {
            Dispose(false);
        }
    }
}
