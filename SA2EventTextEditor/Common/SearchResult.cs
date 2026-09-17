namespace SA2EventTextEditor.Common
{
    public class SearchResult
    {
        public int EventID { get; set; }
        public int MessageIndex { get; set; }
        public string Text { get; set; }

        public SearchResult(int eventID, int messageIndex, string text)
        {
            EventID = eventID;
            MessageIndex = messageIndex;
            Text = text;
        }
    }
}
