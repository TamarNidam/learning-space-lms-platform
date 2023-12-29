namespace Learning_Space.DTO
{
    public class ChatMessageDTO
    {
        public int ISender {  get; set; } = 0;
        public string? SenderFirstName { get; set; }
        public string? SenderLastName { get; set; }
        public string Body { get; set; }
        public DateTime? SentDateTime { get; set; }
    }
}
