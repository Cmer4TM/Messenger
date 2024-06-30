using System.Collections.ObjectModel;

namespace MessengerModels
{
    [Serializable]
    public class Chat
    {
        public User User { get; set; } = new();
        public ObservableCollection<Message> Messages { get; set; } = [];
    }
}
