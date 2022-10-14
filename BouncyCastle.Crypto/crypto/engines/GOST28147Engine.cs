using System;
using System.Collections.Generic;

using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Utilities;

namespace Org.BouncyCastle.Crypto.Engines
{
	/**
	* implementation of GOST 28147-89
	*/
	public class Gost28147Engine
		: IBlockCipher
	{
		private const int  BlockSize = 8;
		private int[] workingKey = null;
		private bool forEncryption;

		private byte[] S = Sbox_Default;

		// these are the S-boxes given in Applied Cryptography 2nd Ed., p. 333
		// This is default S-box!
		private static readonly byte[] Sbox_Default = {
			0x4,0xA,0x9,0x2,0xD,0x8,0x0,0xE,0x6,0xB,0x1,0xC,0x7,0xF,0x5,0x3,
			0xE,0xB,0x4,0xC,0x6,0xD,0xF,0xA,0x2,0x3,0x8,0x1,0x0,0x7,0x5,0x9,
			0x5,0x8,0x1,0xD,0xA,0x3,0x4,0x2,0xE,0xF,0xC,0x7,0x6,0x0,0x9,0xB,
			0x7,0xD,0xA,0x1,0x0,0x8,0x9,0xF,0xE,0x4,0x6,0xC,0xB,0x2,0x5,0x3,
			0x6,0xC,0x7,0x1,0x5,0xF,0xD,0x8,0x4,0xA,0x9,0xE,0x0,0x3,0xB,0x2,
			0x4,0xB,0xA,0x0,0x7,0x2,0x1,0xD,0x3,0x6,0x8,0x5,0x9,0xC,0xF,0xE,
			0xD,0xB,0x4,0x1,0x3,0xF,0x5,0x9,0x0,0xA,0xE,0x7,0x6,0x8,0x2,0xC,
			0x1,0xF,0xD,0x0,0x5,0x7,0xA,0x4,0x9,0x2,0x3,0xE,0x6,0xB,0x8,0xC
		};

		/*
		 * class content S-box parameters for encrypting
		 * getting from, see: http://tools.ietf.org/id/draft-popov-cryptopro-cpalgs-01.txt
		 *                    http://tools.ietf.org/id/draft-popov-cryptopro-cpalgs-02.txt
		 */
		private static readonly byte[] ESbox_Test = {
			0x4,0x2,0xF,0x5,0x9,0x1,0x0,0x8,0xE,0x3,0xB,0xC,0xD,0x7,0xA,0x6,
			0xC,0x9,0xF,0xE,0x8,0x1,0x3,0xA,0x2,0x7,0x4,0xD,0x6,0x0,0xB,0x5,
			0xD,0x8,0xE,0xC,0x7,0x3,0x9,0xA,0x1,0x5,0x2,0x4,0x6,0xF,0x0,0xB,
			0xE,0x9,0xB,0x2,0x5,0xF,0x7,0x1,0x0,0xD,0xC,0x6,0xA,0x4,0x3,0x8,
			0x3,0xE,0x5,0x9,0x6,0x8,0x0,0xD,0xA,0xB,0x7,0xC,0x2,0x1,0xF,0x4,
			0x8,0xF,0x6,0xB,0x1,0x9,0xC,0x5,0xD,0x3,0x7,0xA,0x0,0xE,0x2,0x4,
			0x9,0xB,0xC,0x0,0x3,0x6,0x7,0x5,0x4,0x8,0xE,0xF,0x1,0xA,0x2,0xD,
			0xC,0x6,0x5,0x2,0xB,0x0,0x9,0xD,0x3,0xE,0x7,0xA,0xF,0x4,0x1,0x8
		};

		private static readonly byte[] ESbox_A = {
			0x9,0x6,0x3,0x2,0x8,0xB,0x1,0x7,0xA,0x4,0xE,0xF,0xC,0x0,0xD,0x5,
			0x3,0x7,0xE,0x9,0x8,0xA,0xF,0x0,0x5,0x2,0x6,0xC,0xB,0x4,0xD,0x1,
			0xE,0x4,0x6,0x2,0xB,0x3,0xD,0x8,0xC,0xF,0x5,0xA,0x0,0x7,0x1,0x9,
			0xE,0x7,0xA,0xC,0xD,0x1,0x3,0x9,0x0,0x2,0xB,0x4,0xF,0x8,0x5,0x6,
			0xB,0x5,0x1,0x9,0x8,0xD,0xF,0x0,0xE,0x4,0x2,0x3,0xC,0x7,0xA,0x6,
			0x3,0xA,0xD,0xC,0x1,0x2,0x0,0xB,0x7,0x5,0x9,0x4,0x8,0xF,0xE,0x6,
			0x1,0xD,0x2,0x9,0x7,0xA,0x6,0x0,0x8,0xC,0x4,0x5,0xF,0x3,0xB,0xE,
			0xB,0xA,0xF,0x5,0x0,0xC,0xE,0x8,0x6,0x2,0x3,0x9,0x1,0x7,0xD,0x4
		};

		private static readonly byte[] ESbox_B = {
			0x8,0x4,0xB,0x1,0x3,0x5,0x0,0x9,0x2,0xE,0xA,0xC,0xD,0x6,0x7,0xF,
			0x0,0x1,0x2,0xA,0x4,0xD,0x5,0xC,0x9,0x7,0x3,0xF,0xB,0x8,0x6,0xE,
			0xE,0xC,0x0,0xA,0x9,0x2,0xD,0xB,0x7,0x5,0x8,0xF,0x3,0x6,0x1,0x4,
			0x7,0x5,0x0,0xD,0xB,0x6,0x1,0x2,0x3,0xA,0xC,0xF,0x4,0xE,0x9,0x8,
			0x2,0x7,0xC,0xF,0x9,0x5,0xA,0xB,0x1,0x4,0x0,0xD,0x6,0x8,0xE,0x3,
			0x8,0x3,0x2,0x6,0x4,0xD,0xE,0xB,0xC,0x1,0x7,0xF,0xA,0x0,0x9,0x5,
			0x5,0x2,0xA,0xB,0x9,0x1,0xC,0x3,0x7,0x4,0xD,0x0,0x6,0xF,0x8,0xE,
			0x0,0x4,0xB,0xE,0x8,0x3,0x7,0x1,0xA,0x2,0x9,0x6,0xF,0xD,0x5,0xC
		};

		private static readonly byte[] ESbox_C = {
			0x1,0xB,0xC,0x2,0x9,0xD,0x0,0xF,0x4,0x5,0x8,0xE,0xA,0x7,0x6,0x3,
			0x0,0x1,0x7,0xD,0xB,0x4,0x5,0x2,0x8,0xE,0xF,0xC,0x9,0xA,0x6,0x3,
			0x8,0x2,0x5,0x0,0x4,0x9,0xF,0xA,0x3,0x7,0xC,0xD,0x6,0xE,0x1,0xB,
			0x3,0x6,0x0,0x1,0x5,0xD,0xA,0x8,0xB,0x2,0x9,0x7,0xE,0xF,0xC,0x4,
			0x8,0xD,0xB,0x0,0x4,0x5,0x1,0x2,0x9,0x3,0xC,0xE,0x6,0xF,0xA,0x7,
			0xC,0x9,0xB,0x1,0x8,0xE,0x2,0x4,0x7,0x3,0x6,0x5,0xA,0x0,0xF,0xD,
			0xA,0x9,0x6,0x8,0xD,0xE,0x2,0x0,0xF,0x3,0x5,0xB,0x4,0x1,0xC,0x7,
			0x7,0x4,0x0,0x5,0xA,0x2,0xF,0xE,0xC,0x6,0x1,0xB,0xD,0x9,0x3,0x8
		};

		private static readonly byte[] ESbox_D = {
			0xF,0xC,0x2,0xA,0x6,0x4,0x5,0x0,0x7,0x9,0xE,0xD,0x1,0xB,0x8,0x3,
			0xB,0x6,0x3,0x4,0xC,0xF,0xE,0x2,0x7,0xD,0x8,0x0,0x5,0xA,0x9,0x1,
			0x1,0xC,0xB,0x0,0xF,0xE,0x6,0x5,0xA,0xD,0x4,0x8,0x9,0x3,0x7,0x2,
			0x1,0x5,0xE,0xC,0xA,0x7,0x0,0xD,0x6,0x2,0xB,0x4,0x9,0x3,0xF,0x8,
			0x0,0xC,0x8,0x9,0xD,0x2,0xA,0xB,0x7,0x3,0x6,0x5,0x4,0xE,0xF,0x1,
			0x8,0x0,0xF,0x3,0x2,0x5,0xE,0xB,0x1,0xA,0x4,0x7,0xC,0x9,0xD,0x6,
			0x3,0x0,0x6,0xF,0x1,0xE,0x9,0x2,0xD,0x8,0xC,0x4,0xB,0xA,0x5,0x7,
			0x1,0xA,0x6,0x8,0xF,0xB,0x0,0x4,0xC,0x3,0x5,0x9,0x7,0xD,0x2,0xE
		};

		//S-box for digest
		private static readonly byte[] DSbox_Test = {
			0x4,0xA,0x9,0x2,0xD,0x8,0x0,0xE,0x6,0xB,0x1,0xC,0x7,0xF,0x5,0x3,
			0xE,0xB,0x4,0xC,0x6,0xD,0xF,0xA,0x2,0x3,0x8,0x1,0x0,0x7,0x5,0x9,
			0x5,0x8,0x1,0xD,0xA,0x3,0x4,0x2,0xE,0xF,0xC,0x7,0x6,0x0,0x9,0xB,
			0x7,0xD,0xA,0x1,0x0,0x8,0x9,0xF,0xE,0x4,0x6,0xC,0xB,0x2,0x5,0x3,
			0x6,0xC,0x7,0x1,0x5,0xF,0xD,0x8,0x4,0xA,0x9,0xE,0x0,0x3,0xB,0x2,
			0x4,0xB,0xA,0x0,0x7,0x2,0x1,0xD,0x3,0x6,0x8,0x5,0x9,0xC,0xF,0xE,
			0xD,0xB,0x4,0x1,0x3,0xF,0x5,0x9,0x0,0xA,0xE,0x7,0x6,0x8,0x2,0xC,
			0x1,0xF,0xD,0x0,0x5,0x7,0xA,0x4,0x9,0x2,0x3,0xE,0x6,0xB,0x8,0xC
		};

		private static readonly byte[] DSbox_A = {
			0xA,0x4,0x5,0x6,0x8,0x1,0x3,0x7,0xD,0xC,0xE,0x0,0x9,0x2,0xB,0xF,
			0x5,0xF,0x4,0x0,0x2,0xD,0xB,0x9,0x1,0x7,0x6,0x3,0xC,0xE,0xA,0x8,
			0x7,0xF,0xC,0xE,0x9,0x4,0x1,0x0,0x3,0xB,0x5,0x2,0x6,0xA,0x8,0xD,
			0x4,0xA,0x7,0xC,0x0,0xF,0x2,0x8,0xE,0x1,0x6,0x5,0xD,0xB,0x9,0x3,
			0x7,0x6,0x4,0xB,0x9,0xC,0x2,0xA,0x1,0x8,0x0,0xE,0xF,0xD,0x3,0x5,
			0x7,0x6,0x2,0x4,0xD,0x9,0xF,0x0,0xA,0x1,0x5,0xB,0x8,0xE,0xC,0x3,
			0xD,0xE,0x4,0x1,0x7,0x0,0x5,0xA,0x3,0xC,0x8,0xF,0x6,0x2,0x9,0xB,
			0x1,0x3,0xA,0x9,0x5,0xB,0x4,0xF,0x8,0x6,0x7,0xE,0xD,0x0,0x2,0xC
		};

		//
		// pre-defined sbox table
		//
		private static readonly Dictionary<string, byte[]> m_sBoxes =
			new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

		static Gost28147Engine()
		{
			AddSBox("Default", Sbox_Default);
			AddSBox("E-TEST", ESbox_Test);
			AddSBox("E-A", ESbox_A);
			AddSBox("E-B", ESbox_B);
			AddSBox("E-C", ESbox_C);
			AddSBox("E-D", ESbox_D);
			AddSBox("D-TEST", DSbox_Test);
			AddSBox("D-A", DSbox_A);
		}

		private static void AddSBox(string sBoxName, byte[] sBox)
		{
			m_sBoxes.Add(sBoxName, sBox);        
		}

		/**
		* standard constructor.
		*/
		public Gost28147Engine()
		{
		}

		/**
		* initialise an Gost28147 cipher.
		*
		* @param forEncryption whether or not we are for encryption.
		* @param parameters the parameters required to set up the cipher.
		* @exception ArgumentException if the parameters argument is inappropriate.
		*/
        public virtual void Init(bool forEncryption, ICipherParameters parameters)
		{
			if (parameters is ParametersWithSBox param)
			{
				//
				// Set the S-Box
				//
				byte[] sBox = param.GetSBox();
				if (sBox.Length != Sbox_Default.Length)
					throw new ArgumentException("invalid S-box passed to GOST28147 init");

				this.S = Arrays.Clone(sBox);

				//
				// set key if there is one
				//
				if (param.Parameters != null)
				{
					workingKey = GenerateWorkingKey(forEncryption, ((KeyParameter)param.Parameters).GetKey());
				}
			}
			else if (parameters is KeyParameter keyParameter)
			{
				workingKey = GenerateWorkingKey(forEncryption, keyParameter.GetKey());
			}
			else if (parameters != null)
			{
				throw new ArgumentException("invalid parameter passed to Gost28147 init - "
                    + Platform.GetTypeName(parameters));
			}
		}

        public virtual string AlgorithmName
		{
			get { return "Gost28147"; }
		}

        public virtual int GetBlockSize()
		{
			return BlockSize;
		}

        public virtual int ProcessBlock(byte[] input, int inOff, byte[] output, int outOff)
		{
			if (workingKey == null)
				throw new InvalidOperationException("Gost28147 engine not initialised");

            Check.DataLength(input, inOff, BlockSize, "input buffer too short");
            Check.OutputLength(output, outOff, BlockSize, "output buffer too short");

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
			Gost28147Func(workingKey, input.AsSpan(inOff), output.AsSpan(outOff));
#else
			Gost28147Func(workingKey, input, inOff, output, outOff);
#endif

			return BlockSize;
		}

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		public virtual int ProcessBlock(ReadOnlySpan<byte> input, Span<byte> output)
		{
			if (workingKey == null)
				throw new InvalidOperationException("Gost28147 engine not initialised");

			Check.DataLength(input, BlockSize, "input buffer too short");
			Check.OutputLength(output, BlockSize, "output buffer too short");

			Gost28147Func(workingKey, input, output);

			return BlockSize;
		}
#endif

		private int[] GenerateWorkingKey(bool forEncryption, byte[] userKey)
		{
			this.forEncryption = forEncryption;

			if (userKey.Length != 32)
				throw new ArgumentException("Key length invalid. Key needs to be 32 byte - 256 bit!!!");

			int[] key = new int[8];
			for(int i=0; i != 8; i++)
			{
				key[i] = (int)Pack.LE_To_UInt32(userKey, i * 4);
			}

			return key;
		}

		private int Gost28147_mainStep(int n1, int key)
		{
			int cm = (key + n1); // CM1

			// S-box replacing

			int om = S[  0 + ((cm >> (0 * 4)) & 0xF)] << (0 * 4);
			om += S[ 16 + ((cm >> (1 * 4)) & 0xF)] << (1 * 4);
			om += S[ 32 + ((cm >> (2 * 4)) & 0xF)] << (2 * 4);
			om += S[ 48 + ((cm >> (3 * 4)) & 0xF)] << (3 * 4);
			om += S[ 64 + ((cm >> (4 * 4)) & 0xF)] << (4 * 4);
			om += S[ 80 + ((cm >> (5 * 4)) & 0xF)] << (5 * 4);
			om += S[ 96 + ((cm >> (6 * 4)) & 0xF)] << (6 * 4);
			om += S[112 + ((cm >> (7 * 4)) & 0xF)] << (7 * 4);

//			return om << 11 | om >>> (32-11); // 11-leftshift
			int omLeft = om << 11;
			int omRight = (int)(((uint) om) >> (32 - 11)); // Note: Casts required to get unsigned bit rotation

			return omLeft | omRight;
		}

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
		private void Gost28147Func(int[] workingKey, ReadOnlySpan<byte> input, Span<byte> output)
		{
			int N1 = (int)Pack.LE_To_UInt32(input);
			int N2 = (int)Pack.LE_To_UInt32(input[4..]);
			int tmp;  //tmp -> for saving N1

			if (this.forEncryption)
			{
			for(int k = 0; k < 3; k++)  // 1-24 steps
			{
				for(int j = 0; j < 8; j++)
				{
					tmp = N1;
					int step = Gost28147_mainStep(N1, workingKey[j]);
					N1 = N2 ^ step; // CM2
					N2 = tmp;
				}
			}
			for(int j = 7; j > 0; j--)  // 25-31 steps
			{
				tmp = N1;
				N1 = N2 ^ Gost28147_mainStep(N1, workingKey[j]); // CM2
				N2 = tmp;
			}
			}
			else //decrypt
			{
			for(int j = 0; j < 8; j++)  // 1-8 steps
			{
				tmp = N1;
				N1 = N2 ^ Gost28147_mainStep(N1, workingKey[j]); // CM2
				N2 = tmp;
			}
			for(int k = 0; k < 3; k++)  //9-31 steps
			{
				for(int j = 7; j >= 0; j--)
				{
					if ((k == 2) && (j==0))
					{
						break; // break 32 step
					}
					tmp = N1;
					N1 = N2 ^ Gost28147_mainStep(N1, workingKey[j]); // CM2
					N2 = tmp;
				}
			}
			}

			N2 = N2 ^ Gost28147_mainStep(N1, workingKey[0]);  // 32 step (N1=N1)

			Pack.UInt32_To_LE((uint)N1, output);
			Pack.UInt32_To_LE((uint)N2, output[4..]);
		}
#else
		private void Gost28147Func(int[] workingKey, byte[] inBytes, int inOff, byte[] outBytes, int outOff)
		{
			int N1 = (int)Pack.LE_To_UInt32(inBytes, inOff);
			int N2 = (int)Pack.LE_To_UInt32(inBytes, inOff + 4);
			int tmp;  //tmp -> for saving N1

			if (this.forEncryption)
			{
			for(int k = 0; k < 3; k++)  // 1-24 steps
			{
				for(int j = 0; j < 8; j++)
				{
					tmp = N1;
					int step = Gost28147_mainStep(N1, workingKey[j]);
					N1 = N2 ^ step; // CM2
					N2 = tmp;
				}
			}
			for(int j = 7; j > 0; j--)  // 25-31 steps
			{
				tmp = N1;
				N1 = N2 ^ Gost28147_mainStep(N1, workingKey[j]); // CM2
				N2 = tmp;
			}
			}
			else //decrypt
			{
			for(int j = 0; j < 8; j++)  // 1-8 steps
			{
				tmp = N1;
				N1 = N2 ^ Gost28147_mainStep(N1, workingKey[j]); // CM2
				N2 = tmp;
			}
			for(int k = 0; k < 3; k++)  //9-31 steps
			{
				for(int j = 7; j >= 0; j--)
				{
					if ((k == 2) && (j==0))
					{
						break; // break 32 step
					}
					tmp = N1;
					N1 = N2 ^ Gost28147_mainStep(N1, workingKey[j]); // CM2
					N2 = tmp;
				}
			}
			}

			N2 = N2 ^ Gost28147_mainStep(N1, workingKey[0]);  // 32 step (N1=N1)

			Pack.UInt32_To_LE((uint)N1, outBytes, outOff);
			Pack.UInt32_To_LE((uint)N2, outBytes, outOff + 4);
		}
#endif

		/**
		* Return the S-Box associated with SBoxName
		* @param sBoxName name of the S-Box
		* @return byte array representing the S-Box
		*/
		public static byte[] GetSBox(string sBoxName)
		{
			if (!m_sBoxes.TryGetValue(sBoxName, out var sBox))
			{
				throw new ArgumentException("Unknown S-Box - possible types: "
					+ "\"Default\", \"E-Test\", \"E-A\", \"E-B\", \"E-C\", \"E-D\", \"D-Test\", \"D-A\".");
			}

			return Arrays.Clone(sBox);
		}

        public static string GetSBoxName(byte[] sBox)
        {
			foreach (var entry in m_sBoxes)
            {
                if (Arrays.AreEqual(entry.Value, sBox))
                    return entry.Key;
            }

            throw new ArgumentException("SBOX provided did not map to a known one");
        }
    }
}
