using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IredesChecksum
{
    static class Extensions
    {
        public static byte[] ReadToBytes(this Stream stream)
        {
            var initiatialPosition = stream.Position;
            stream.Position = 0;
            var bytes = new byte[stream.Length];
            int value;
            var n = 0;
            while ((value = stream.ReadByte()) != -1)
                bytes[n++] = (byte)value;

            // Restore the initial position...
            stream.Position = initiatialPosition;

            return bytes;
        }

        public static string ReadToString(
            this Stream stream, Encoding encoding = null)
        {
            var initiatialPosition = stream.Position;
            stream.Position = 0;
            if (stream.Length > int.MaxValue)
                return null;

            int value;
            var bytes = new List<byte>((int)stream.Length);
            while ((value = stream.ReadByte()) != -1)
                bytes.Add((byte)value);

            var readStr = (encoding ?? Encoding.UTF8).GetString(bytes.ToArray());

            // Restore the initial position...
            stream.Position = initiatialPosition;

            return readStr;
        }
    }
}
