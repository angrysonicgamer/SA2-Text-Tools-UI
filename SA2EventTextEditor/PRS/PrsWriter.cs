using csharp_prs;
using SA2EventTextEditor.Common;
using SA2EventTextEditor.Extensions;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SA2EventTextEditor.PRS
{
    public class PrsWriter : IDisposable
    {
        private readonly MemoryStream _memory;
        private readonly BinaryWriter _writer;
        private readonly uint _prsMessageSize = 8;

        public PrsWriter()
        {
            _memory = new MemoryStream();
            _writer = new BinaryWriter(_memory);
        }


        private void WriteEventInfo(ObservableCollection<SA2Scene> scenes, Endianness endianness)
        {
            uint messagePointer = SA2EventInfo.Size * ((uint)scenes.Count + 1) + Pointer.Base;

            foreach (var scene in scenes)
            {
                int messagesCount = scene.Messages.Count;

                if (messagesCount == 1 && scene.Messages[0].Text == "")
                    messagesCount = 0;

                var eventInfo = new SA2EventInfo(scene.EventID, messagePointer, messagesCount);
                eventInfo.Write(_writer, endianness);
                messagePointer += (uint)scene.Messages.Count * _prsMessageSize;
            }
        }

        private void WriteSeparator(Endianness endianness)
        {
            SA2EventInfo.Null.Write(_writer, endianness);
        }

        private void WriteMessageData(ObservableCollection<SA2Scene> scenes, Encoding encoding, Endianness endianness)
        {
            int totalMessagesCount = 0;

            foreach (var scene in scenes)
            {
                totalMessagesCount += scene.Messages.Count;
            }

            uint textPointer = SA2EventInfo.Size * ((uint)scenes.Count + 1) + _prsMessageSize * (uint)totalMessagesCount + Pointer.Base;

            foreach (var scene in scenes)
            {
                foreach (var message in scene.Messages)
                {
                    _writer.WriteUInt32((uint)message.Character, endianness);
                    _writer.WriteUInt32(textPointer, endianness);
                    textPointer += (uint)encoding.GetByteCount(message.GetRawString()) + 1;
                }
            }

            foreach (var scene in scenes)
            {
                foreach (var message in scene.Messages)
                {
                    _writer.WriteCString(message.GetRawString(), encoding);
                }
            }
        }

        public void WriteToBuffer(ObservableCollection<SA2Scene> scenes, Encoding encoding, Endianness endianness)
        {
            WriteEventInfo(scenes, endianness);
            WriteSeparator(endianness);
            WriteMessageData(scenes, encoding, endianness);
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
