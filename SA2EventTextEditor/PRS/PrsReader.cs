using csharp_prs;
using System.IO;
using SA2EventTextEditor.Common;
using SA2EventTextEditor.Extensions;
using System.Collections.ObjectModel;
using System.Text;

namespace SA2EventTextEditor.PRS
{
    public class PrsReader : IDisposable
    {
        private readonly byte[] _decompressedFile;
        private readonly BinaryReader _reader;

        public PrsReader(string path)
        {
            _decompressedFile = Prs.Decompress(File.ReadAllBytes(path));
            _reader = new BinaryReader(new MemoryStream(_decompressedFile));
        }

        public Endianness DetectEndianness()
        {
            uint le = _reader.ReadAt(0xC, x => x.ReadUInt32(Endianness.LittleEndian));
            uint be = _reader.ReadAt(0xC, x => x.ReadUInt32(Endianness.BigEndian));

            return le < be ? Endianness.LittleEndian : Endianness.BigEndian;
        }

        public ObservableCollection<SA2Scene> ReadEventData(Encoding encoding, Endianness endianness)
        {
            var eventInfoList = new List<SA2EventInfo>();

            while (true)
            {
                var eventInfo = new SA2EventInfo();
                eventInfo.Read(_reader, endianness);

                if (eventInfo.IsValid())
                {
                    eventInfoList.Add(eventInfo);
                }
                else break;
            }

            var eventData = new ObservableCollection<SA2Scene>();

            foreach (var scene in eventInfoList)
            {
                var messages = new ObservableCollection<SA2EventMessage>();
                _reader.SetPosition(scene.MessagePointer - Pointer.Base);

                if (scene.TotalMessages == 0)
                {
                    int character = _reader.ReadInt32(endianness);
                    messages.Add(new SA2EventMessage(character, TextCentering.None, ""));
                }

                for (int i = 0; i < scene.TotalMessages; i++)
                {
                    var message = new SA2EventMessage();
                    message.Read(_reader, encoding, endianness);
                    messages.Add(message);
                }

                eventData.Add(new SA2Scene(scene.EventID, messages));
            }
            
            return eventData;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
