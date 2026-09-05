namespace SA2EventTextEditor.Common
{
    public static class Pointer
    {
        public static uint Base { get; set; }

        public static void SetBaseAddress(Endianness endianness)
        {
            Base = endianness == Endianness.BigEndian ? 0x817AFE60 : 0xCBD0000;
        }
    }
}
