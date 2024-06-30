using MessengerClient.Infrastructure;
using MessengerModels;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MessengerClient.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    internal class MainWindowViewModel
    {
        public string NewChatTextBox { get; set; } = string.Empty;
        public string LoginTextBox { get; set; } = string.Empty;
        public string PasswordTextBox { get; set; } = string.Empty;
        public string MessageTextBox { get; set; } = string.Empty;
        public ObservableCollection<Chat> Chats { get; set; } = [];
        public Chat? SelectedChat { get; set; }
        public ICommand SignInOrRegisterCommand { get; }
        public ICommand NewChatCommand { get; }
        public ICommand SendMessageCommand { get; }
        public ICommand BlockCommand { get; }
        public ICommand UnblockCommand { get; }
        public MainWindowViewModel()
        {
            SignInOrRegisterCommand = new RelayCommand<string>(SignInOrRegister, CanSignInOrRegister);
            NewChatCommand = new RelayCommand(NewChat, CanNewChat);
            SendMessageCommand = new RelayCommand(SendMessage, CanSend);
            BlockCommand = new RelayCommand(Block, CanBlock);
            UnblockCommand = new RelayCommand<User>(Unblock);

            new Task(TimerThread).Start();
        }
        private async void TimerThread()
        {
            while (true)
            {
                if (CurrentUser.User is not null)
                {
                    await Refresh();
                }

                await Task.Delay(100);
            }
        }
        private async Task Refresh()
        {
            try
            {
                Request request = new()
                {
                    Title = "GET",
                    User = CurrentUser.User!
                };
                Response response = await CurrentUser.SendMessageAsync(request);
                
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    foreach (Chat responseChat in response.Chats)
                    {
                        if (SelectedChat is null || responseChat.User.Login != SelectedChat.User.Login)
                        {
                            if (Chats.Any(item => item.User.Login == responseChat.User.Login))
                            {
                                Chat chat = Chats.First(item => item.User.Login == responseChat.User.Login);
                                int index = Chats.IndexOf(chat);

                                Chats[index] = responseChat;
                            }
                            else
                            {
                                Chats.Add(responseChat);
                            }
                        }
                        else
                        {
                            if (responseChat.User.Status.Name != SelectedChat.User.Status.Name
                                || responseChat.Messages.Count != SelectedChat.Messages.Count)
                            {
                                Chat chat = Chats.First(item => item.User.Login == responseChat.User.Login);
                                int index = Chats.IndexOf(chat);

                                SelectedChat = Chats[index] = responseChat;
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void SignInOrRegister(string title)
        {
            try
            {
                Request request = new()
                {
                    Title = title,
                    User = new()
                    {
                        Login = LoginTextBox,
                        Password = PasswordTextBox
                    }
                };

                LoginTextBox = PasswordTextBox = string.Empty;

                Response response = await CurrentUser.SendMessageAsync(request);
                await Exit();

                CurrentUser.User = response.User;
                
                Chats = [];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void NewChat()
        {
            try
            {
                if (Chats.Any(item => item.User.Login == NewChatTextBox))
                {
                    return;
                }

                Request request = new()
                {
                    Title = "NEW",
                    User = CurrentUser.User!,
                    Login = NewChatTextBox
                };

                NewChatTextBox = string.Empty;

                Response response = await CurrentUser.SendMessageAsync(request);

                Chats.Add(response.Chat!);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void SendMessage()
        {
            try
            {
                Request request = new()
                {
                    Title = "SEND",
                    User = CurrentUser.User!,
                    Message = new()
                    {
                        Content = MessageTextBox,
                        Recipient = SelectedChat!.User,
                        Time = DateTime.Now
                    }
                };

                MessageTextBox = string.Empty;

                await CurrentUser.SendMessageAsync(request);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void Block()
        {
            try
            {
                Request request = new()
                {
                    Title = "BLOCK",
                    User = CurrentUser.User!,
                    Login = SelectedChat!.User.Login
                };
                await CurrentUser.SendMessageAsync(request);

                CurrentUser.User!.BlockedUsers.Add(SelectedChat.User);
                Chats.Remove(SelectedChat);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private async void Unblock(User user)
        {
            try
            {
                Request request = new()
                {
                    Title = "UNBLOCK",
                    User = CurrentUser.User!,
                    Login = user.Login
                };
                await CurrentUser.SendMessageAsync(request);

                CurrentUser.User!.BlockedUsers.Remove(user);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public static async Task Exit()
        {
            if (CurrentUser.User is null)
            {
                return;
            }

            await CurrentUser.SendMessageAsync(new()
            {
                Title = "EXIT",
                User = CurrentUser.User
            });
        }
        private bool CanSignInOrRegister()
            => string.IsNullOrEmpty(LoginTextBox) == false
            && string.IsNullOrEmpty(PasswordTextBox) == false;
        private bool CanNewChat()
            => string.IsNullOrEmpty(NewChatTextBox) == false
            && CurrentUser.User is not null;
        private bool CanSend()
            => string.IsNullOrEmpty(MessageTextBox) == false
            && SelectedChat is not null;
        private bool CanBlock()
            => CurrentUser.User is not null
            && SelectedChat is not null;
    }
}
