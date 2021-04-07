using System;
using System.Collections.Generic;
using System.Security.Cryptography;

// Taken from https://github.com/damieng/DamienGKit/blob/master/CSharp/DamienG.Library/Security/Cryptography/Crc32.cs
namespace DamienG.Security.Cryptography
{
    /// <summary>
    /// Implements a 32-bit CRC hash algorithm compatible with Zip etc.
    /// </summary>
    /// <remarks>
    /// Crc32 should only be used for backward compatibility with older file formats
    /// and algorithms. It is not secure enough for new applications.
    /// If you need to call multiple times for the same data either use the HashAlgorithm
    /// interface or remember that the result of one Compute call needs to be ~ (XOR) before
    /// being passed in as the seed for the next Compute call.
    /// </remarks>
    public sealed class Crc32 : HashAlgorithm
    {
        public const uint DefaultPolynomial = 0xedb88320u;
        public const uint DefaultSeed = 0xffffffffu;

        static uint[] defaultTable;

        readonly uint seed;
        readonly uint[] table;
        uint hash;

        public Crc32()
            : this(DefaultPolynomial, DefaultSeed) { }

        public Crc32(uint polynomial, uint seed)
        {
            if (!BitConverter.IsLittleEndian)
                throw new PlatformNotSupportedException("Not supported on Big Endian processors");

            table = _initializeTable(polynomial);
            this.seed = hash = seed;
        }

        public override void Initialize() => hash = seed;

        protected override void HashCore(byte[] array, int ibStart, int cbSize) =>
            hash = _calculateHash(table, hash, array, ibStart, cbSize);

        protected override byte[] HashFinal() =>
            HashValue = _uInt32ToBigEndianBytes(~hash);

        public override int HashSize => 32;

        public static uint Compute(byte[] buffer) => Compute(DefaultSeed, buffer);

        public static uint Compute(uint seed, byte[] buffer) =>
            Compute(DefaultPolynomial, seed, buffer);

        public static uint Compute(uint polynomial, uint seed, byte[] buffer) =>
            ~_calculateHash(_initializeTable(polynomial), seed, buffer, 0, buffer.Length);

        static uint[] _initializeTable(uint polynomial)
        {
            if (polynomial == DefaultPolynomial && defaultTable != null)
                return defaultTable;

            var createTable = new uint[256];
            for (var i = 0; i < 256; i++)
            {
                var entry = (uint)i;
                for (var j = 0; j < 8; j++)
                    if ((entry & 1) == 1)
                        entry = (entry >> 1) ^ polynomial;
                    else
                        entry >>= 1;
                createTable[i] = entry;
            }

            if (polynomial == DefaultPolynomial)
                defaultTable = createTable;

            return createTable;
        }

        static uint _calculateHash(
            uint[] table, uint seed, IList<byte> buffer, int start, int size)
        {
            var hash = seed;
            for (var i = start; i < start + size; i++)
                hash = (hash >> 8) ^ table[buffer[i] ^ hash & 0xff];
            return hash;
        }

        static byte[] _uInt32ToBigEndianBytes(uint uint32)
        {
            var result = BitConverter.GetBytes(uint32);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(result);

            return result;
        }
    }
}