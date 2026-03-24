using csharp_prs;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using SA2MsgTextEditor.Common;
using SA2MsgTextEditor.Extensions;

namespace SA2MsgTextEditor.PRS
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
            uint le = _reader.ReadAt(0, x => x.ReadUInt32(Endianness.LittleEndian));
            uint be = _reader.ReadAt(0, x => x.ReadUInt32(Endianness.BigEndian));

            return le < be ? Endianness.LittleEndian : Endianness.BigEndian;
        }

        public List<int> ReadOffsets(Endianness endianness)
        {
            var offsets = new List<int>();

            while (true)
            {
                int offset = _reader.ReadInt32(endianness);
                if (offset == -1 || offset > _reader.BaseStream.Length) break;

                offsets.Add(offset);
            }

            return offsets;
        }

        public ObservableCollection<ObservableCollection<SA2Message>> ReadEmeraldHints(List<int> offsets, Encoding encoding)
        {
            var hintsPerPiece = new ObservableCollection<SA2Message>();
            var messagesList = new ObservableCollection<ObservableCollection<SA2Message>>();

            foreach (var offset in offsets)
            {
                var hint = new SA2Message();
                string rawText = _reader.ReadAt(offset, x => x.ReadCString(encoding));
                hint.Parse(rawText, encoding);
                hintsPerPiece.Add(hint);

                if (hintsPerPiece.Count == 3)
                {
                    messagesList.Add(hintsPerPiece);
                    hintsPerPiece = new ObservableCollection<SA2Message>();
                }
            }

            return messagesList;
        }

        public ObservableCollection<ObservableCollection<SA2Message>> ReadGameplayMessages(List<int> offsets, Encoding encoding)
        {
            var groupedMessages = new ObservableCollection<ObservableCollection<SA2Message>>();

            foreach (var offset in offsets)
            {
                string rawText = _reader.ReadAt(offset, x => x.ReadCString(encoding));
                string[] lines = rawText.Split(new char[] { '\x0C' }, StringSplitOptions.RemoveEmptyEntries);
                var linesList = new ObservableCollection<SA2Message>();

                foreach (var line in lines)
                {
                    var message = new SA2Message();
                    message.Parse($"\x0C{line}", encoding);
                    linesList.Add(message);
                }

                groupedMessages.Add(linesList);
            }

            return groupedMessages;
        }

        public ObservableCollection<ObservableCollection<SA2Message>> ReadSimpleText(List<int> offsets, Encoding encoding)
        {
            var messagesList = new ObservableCollection<ObservableCollection<SA2Message>>();
            var stringsList = new ObservableCollection<SA2Message>();

            foreach (var offset in offsets)
            {
                var hint = new SA2Message();
                string rawText = _reader.ReadAt(offset, x => x.ReadCString(encoding));
                hint.Parse(rawText, encoding);
                stringsList.Add(hint);
            }

            messagesList.Add(stringsList);
            return messagesList;
        }

        public ObservableCollection<ObservableCollection<SA2Message>> ReadChaoNames(List<int> offsets, Encoding encoding)
        {
            var messagesList = new ObservableCollection<ObservableCollection<SA2Message>>();
            var namesList = new ObservableCollection<SA2Message>();
            bool useCyrillic = encoding == Encoding.GetEncoding((int)Codepage.Windows1251);
            var converter = new ChaoTextConverter(useCyrillic);

            foreach (var offset in offsets)
            {
                var chaoName = new SA2Message();
                chaoName.Text = converter.ToReadable(_reader.ReadAt(offset, x => x.ReadBytesUntilNullTerminator()));
                namesList.Add(chaoName);
            }

            messagesList.Add(namesList);
            return messagesList;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
