namespace EmailService.Models
{
    public class GdExcuseHistory
    {
        public int? UserId { get; set; }
        public long HistoryId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string ToEmail    { get; set; } = string.Empty;
        public string? Subject   { get; set; }
        public string ExcuseText { get; set; } = string.Empty;
        public string? Motive    { get; set; }
        public string? Tone      { get; set; }
        public DateTime SentAt   { get; set; }
    }
}