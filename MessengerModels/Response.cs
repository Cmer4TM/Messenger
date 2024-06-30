using System.Collections.ObjectModel;

namespace MessengerModels
{
    [Serializable]
    public class Response
    {
        public string? Message { get; set; }
        public User? User { get; set; }
        public Chat? Chat { get; set; }
        public ObservableCollection<Chat> Chats { get; set; } = [];
    }
}
