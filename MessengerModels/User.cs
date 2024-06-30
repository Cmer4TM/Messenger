using System.Collections.ObjectModel;

namespace MessengerModels
{
    [Serializable]
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Status Status { get; set; } = new();
        public ObservableCollection<User> BlockedUsers { get; set; } = [];
    }
}
