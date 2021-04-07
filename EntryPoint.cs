using DamienG.Security.Cryptography;
using System;
using System.IO;
using System.Text;

namespace IredesChecksum
{
    static class EntryPoint
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter a filepath to check");
                return 0;
            }

            var retVal = 0;
            foreach (var filepath in args)
            {
                var (stored, computed) = _getChecksum(args[0]);
                Console.WriteLine($"Checking \"{filepath}\"");
                Console.WriteLine($"Stored checkSum is:    {stored}");
                Console.WriteLine($"Computed checkSum is:  {computed}");
                if (computed != stored)
                {
                    Console.WriteLine("***ERROR - checksum mismatch***");
                    retVal = 1;
                }
                else
                    Console.WriteLine("SUCCESS - checksum is correct");
            }

            return retVal;
        }

        static (uint Stored, uint Computed) _getChecksum(string filepath)
        {
            const string checkSumTag = "<IR:ChkSum>";
            const string endTag = "</";
            string contentsStr;
            using (var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                contentsStr = fileStream.ReadToString();

            // Look for the start of the checksum node
            var indexStart = contentsStr.IndexOf(checkSumTag);
            if (indexStart < 0)
                indexStart = 0;

            // Add the length of the string to the start idx
            indexStart += checkSumTag.Length;

            // Find the start of the end of the checksum tag so we know where to
            // slice the string
            var indexEnd = contentsStr.IndexOf(endTag, indexStart);
            if (indexEnd < 0)
                indexEnd = indexStart;

            // If there is no checksum found, consider it to be zero...
            var checkSumStr = indexStart == indexEnd ?
                "0" :
                contentsStr[indexStart..indexEnd];
            var storedCheckSum = uint.Parse(checkSumStr);

            // Now copy the contents of the file to a byte array but replace the stored
            // checksum value with 0
            var encoding = Encoding.UTF8;
            var newContentsBytes = new byte[contentsStr.Length - (indexEnd - indexStart) + 1];
            encoding.GetBytes(contentsStr[..indexStart]).CopyTo(newContentsBytes, 0);
            encoding.GetBytes("0").CopyTo(newContentsBytes, indexStart);
            encoding.GetBytes(contentsStr[indexEnd..]).CopyTo(newContentsBytes, indexStart + 1);

            // Return the stored and computed checksum values
            return (storedCheckSum, Crc32.Compute(newContentsBytes));
        }
    }
}
