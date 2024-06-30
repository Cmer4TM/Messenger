namespace MessengerModels
{
    [Serializable]
    public class Message
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime Time { get; set; }
        public User Sender { get; set; } = new();
        public User Recipient { get; set; } = new();
    }
}
