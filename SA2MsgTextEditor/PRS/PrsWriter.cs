using csharp_prs;
using System.IO;
using System.Text;
using SA2MsgTextEditor.Common;
using SA2MsgTextEditor.Extensions;

namespace SA2MsgTextEditor.PRS
{
    public class PrsWriter : IDisposable
    {
        private readonly MemoryStream _memory;
        private readonly BinaryWriter _writer;

        public PrsWriter()
        {
            _memory = new MemoryStream();
            _writer = new BinaryWriter(_memory);
        }

        private void WriteOffsets(List<string> rawStrings, Encoding encoding, Endianness endianness)
        {
            int separatorLength = 4;
            int offset = rawStrings.Count * sizeof(int) + separatorLength;

            foreach (var str in rawStrings)
            {
                _writer.WriteInt32(offset, endianness);
                offset += encoding.GetByteCount(str) + 1;
            }
        }

        private void WriteSeparator()
        {
            _writer.Write(BitConverter.GetBytes(-1));
        }

        private void WriteStrings(List<string> rawStrings, Encoding encoding)
        {
            foreach (var str in rawStrings)
            {
                _writer.WriteCString(str, encoding);
            }
        }

        public void WriteToBuffer(List<string> rawStrings, Encoding encoding, Endianness endianness)
        {
            WriteOffsets(rawStrings, encoding, endianness);
            WriteSeparator();
            WriteStrings(rawStrings, encoding);
        }

        public void WriteBufferToFile(string fileName)
        {
            File.WriteAllBytes(fileName, Prs.Compress(_memory.ToArray(), 0x1FFF));
        }

        public void Dispose()
        {
            _writer.Dispose();
            _memory.Dispose();
        }
    }
}
