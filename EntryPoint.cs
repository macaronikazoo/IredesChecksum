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
                var (stored, computed) = GetChecksum(args[0]);
                Console.WriteLine($"Checking \"{filepath}\"");
                Console.WriteLine($"Stored checkSum is:    {stored}");
                Console.WriteLine($"Computed checkSum is:  {computed}");
                if (computed != stored)
                {
                    Console.WriteLine($"***ERROR - checksum mismatch***");
                    retVal = 1;
                }
                else
                    Console.WriteLine($"SUCCESS - checksum is correct");
            }

            return retVal;
        }

        static (uint Stored, uint Computed) GetChecksum(string filepath)
        {
            const string checkSumTag = "<IR:ChkSum>";
            const string endTag = "</";
            string contentsStr;
            using (var fileStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                contentsStr = fileStream.ReadToString();

            var indexStart = contentsStr.IndexOf(checkSumTag);
            if (indexStart < 0)
                indexStart = 0;

            indexStart += checkSumTag.Length;
            var indexEnd = contentsStr.IndexOf(endTag, indexStart);
            if (indexEnd < 0)
                indexEnd = indexStart;

            var checkSumStr = indexStart == indexEnd ?
                "0" :
                contentsStr.Substring(indexStart, indexEnd - indexStart);
            var storedCheckSum = uint.Parse(checkSumStr);

            var encoding = Encoding.UTF8;
            var newContentsBytes = new byte[contentsStr.Length - (indexEnd - indexStart) + 1];
            encoding.GetBytes(contentsStr.Substring(0, indexStart)).CopyTo(newContentsBytes, 0);
            encoding.GetBytes("0").CopyTo(newContentsBytes, indexStart);
            encoding.GetBytes(contentsStr.Substring(indexEnd)).CopyTo(newContentsBytes, indexStart + 1);

            return (storedCheckSum, Crc32.Compute(newContentsBytes));
        }
    }
}
