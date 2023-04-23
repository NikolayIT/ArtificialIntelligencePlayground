namespace DnesBgCommentsBinaryClassification.Data
{
    public class DnesBgComment
    {
        public int Id { get; set; }

        public int NewsId { get; set; }

        public string Content { get; set; }

        public int UpVotes { get; set; }

        public int DownVotes { get; set; }
    }
}
