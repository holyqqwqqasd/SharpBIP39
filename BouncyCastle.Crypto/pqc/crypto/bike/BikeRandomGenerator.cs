﻿using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Org.BouncyCastle.Pqc.Crypto.Bike
{
    class BikeRandomGenerator
    {
        private static int BitScanReverse(int t)
        {
            int res = 0;
            while (t != 0)
            {
                t >>= 1;
                res++;
            }

            return res;
        }

        private static int GetRandomInMod(int mod, IXof digest)
        {
            int mask = MaskNumber(BitScanReverse(mod));
            int res = -1;
            while (true)
            {
                res = GetRandomNumber(digest);
                res &= mask;

                if (res < mod)
                {
                    break;
                }
            }
            return res;
        }

        private static void GenerateRandomArray(byte[] res, int mod, int weight, IXof digest)
        {
            int index = 0;
            while (index < weight)
            {
                int tmp = GetRandomInMod(mod, digest);

                if (CheckBit(res, tmp) == 0)
                { // check for new index
                    SetBit(res, tmp);
                    index++;
                }
            }
        }

        private static int CheckBit(byte[] a, int position)
        {
            int index = position / 8;
            int pos = position % 8;
            return ((a[index] >> (pos)) & 0x01);
        }



        private static void SetBit(byte[] a, int position)
        {
            int index = position / 8;
            int pos = position % 8;
            a[index] |= (byte) (1 << (pos));
        }

        public static byte[] GenerateRandomByteArray(int mod, int size, int weight, IXof digest)
        {
            byte[] res = new byte[size];
            GenerateRandomArray(res, mod, weight, digest);
            return res;
        }

        private static int MaskNumber(int n)
        {
            return ((1 << n) - 1);
        }

        private static int GetRandomNumber(IXof digest)
        {
            byte[] output = new byte[4];
            digest.Output(output, 0, output.Length);
            int tmp = Pack.LE_To_UInt16(output, 0);
            return tmp;
        }
    }
}
