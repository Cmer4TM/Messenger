namespace MessengerModels
{
    [Serializable]
    public class Request
    {
        public string Title { get; set; } = string.Empty;
        public User User { get; set; } = new();
        public string? Login { get; set; }
        public Message? Message { get; set; }
        public override string ToString()
            => $"{User.Login} -> {Title} args: {Login}{Message?.Content}";
    }
}
