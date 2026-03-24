using System.IO;
using SA2EventTextEditor.Extensions;

namespace SA2EventTextEditor.Common
{
    public class SA2EventInfo
    {
        public int EventID { get; set; }
        public uint MessagePointer { get; set; }
        public int TotalMessages { get; set; }
        public static uint Size => 12;
        public static SA2EventInfo Null => new SA2EventInfo(-1, 0, 0);

        public SA2EventInfo() { }

        public SA2EventInfo(int id, uint offset, int total)
        {
            EventID = id;
            MessagePointer = offset;
            TotalMessages = total;
        }


        public void Read(BinaryReader reader, Endianness endianness)
        {
            EventID = reader.ReadInt32(endianness);
            if (EventID == -1) return;

            MessagePointer = reader.ReadUInt32(endianness);
            TotalMessages = reader.ReadInt32(endianness);
        }

        public void Write(BinaryWriter writer, Endianness endianness)
        {
            writer.WriteUInt32((uint)EventID, endianness);
            writer.WriteUInt32(MessagePointer, endianness);
            writer.WriteUInt32((uint)TotalMessages, endianness);
        }

        public bool IsValid()
        {
            return EventID >= 0;
        }
    }
}
