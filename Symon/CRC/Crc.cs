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

namespace Symon
{
    public class Crc
    {
        const uint polynomial = 0x04c11db7;
        uint[] table = new uint[256];

        public uint ComputeChecksum(byte[] bytes)
        {
            int index;
            uint crc;
            int len = bytes.Length;

            crc = 0xffffffff;

            for (index = 0; len > 0; ++index, --len)
            {
                crc = (crc << 8) ^ table[(crc >> 24) ^ bytes[index]];
            }

            return ~crc;
        }

        public byte[] ComputeChecksumBytes(byte[] bytes)
        {
            uint crc = ComputeChecksum(bytes);
            return BitConverter.GetBytes(crc);
        }

        public Crc()
        {
            uint value;

            for (uint i = 0; i < table.Length; ++i)
            {
                value = i << 24;

                for (uint j = 8; j > 0; --j)
                {
                    if ((value & 0x80000000) != 0)
                    {
                        value = (value << 1) ^ polynomial;
                    }
                    else
                    {
                        value = (value << 1);
                    }
                }

                table[i] = value;
            }
        }
    }

}

