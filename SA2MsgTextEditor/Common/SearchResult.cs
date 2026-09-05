namespace SA2MsgTextEditor.Common
{
    public class SearchResult
    {
        public int GroupIndex { get; set; }
        public int MessageIndex { get; set; }
        public string Text { get; set; }

        public SearchResult(int groupIndex, int messageIndex, string text)
        {
            GroupIndex = groupIndex;
            MessageIndex = messageIndex;
            Text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
