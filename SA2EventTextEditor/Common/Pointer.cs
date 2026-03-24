namespace SA2EventTextEditor.Common
{
    public static class Pointer
    {
        public static uint BaseAddress { get; set; }

        public static void SetBaseAddress(Endianness endianness)
        {
            BaseAddress = endianness == Endianness.BigEndian ? 0x817AFE60 : 0xCBD0000;
        }
    }
}
