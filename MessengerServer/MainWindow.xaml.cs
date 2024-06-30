using MessengerModels;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media;

#pragma warning disable SYSLIB0011

namespace MessengerServer
{
    public partial class MainWindow : Window
    {
        private bool serverStarted;
        private TcpListener? server;
        private Thread? serverThread;
        public MainWindow()
        {
            InitializeComponent();
        }
        private async void ServerThread()
        {
            while (serverStarted)
            {
                try
                {
                    if (server is null)
                    {
                        throw new Exception("server is null");
                    }

                    using TcpClient acceptor = await server.AcceptTcpClientAsync();
                    await using NetworkStream ns = acceptor.GetStream();
                    await using MessengerDbContext context = new();

                    BinaryFormatter bf = new();

                    Request request = (Request)bf.Deserialize(ns);
                    Response response = new();
                    User? user;
                    User? opponent;

                    if (request.Title != "GET")
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            logTextBox.AppendText($"{DateTime.Now} -- {request}\r");
                        });
                    }

                    try
                    {
                        switch (request.Title)
                        {
                            case "GET":
                                user = context.Users
                                    .Include(item => item.BlockedUsers)
                                    .First(item => item.Login == request.User.Login
                                        && item.Password == request.User.Password);

                                response.Chats = new(await context.Messages
                                    .Where(item => item.Sender.Id == user.Id
                                        || item.Recipient.Id == user.Id)
                                    .GroupBy(item => item.Sender.Id == user.Id
                                        ? item.Recipient.Id
                                        : item.Sender.Id)
                                    .Select(item => new Chat()
                                    {
                                        User = context.Users
                                            .Include(item => item.Status)
                                            .First(item2 => item2.Id == item.Key),
                                        Messages = new(item.ToList())
                                    })
                                    .ToListAsync());

                                for (int x = 0; x < response.Chats.Count; x++)
                                {
                                    if (user.BlockedUsers.Any(item => item.Login == response.Chats[x].User.Login))
                                    {
                                        response.Chats.RemoveAt(x);
                                    }
                                }
                                break;

                            case "SIGN IN":
                                user = await context.Users
                                    .Include(item => item.BlockedUsers)
                                    .FirstOrDefaultAsync(item => item.Login == request.User.Login
                                        && item.Password == request.User.Password);

                                if (user is null)
                                {
                                    response.Message = "Entered login or password is incorrect";
                                    break;
                                }

                                user.Status = await context.Statuses.FirstAsync(item => item.Name == "Online");

                                context.Users.Update(user);

                                response.User = user;
                                break;

                            case "REGISTER":
                                if (await context.Users.AnyAsync(item => item.Login == request.User.Login))
                                {
                                    response.Message = "User with this login is alredy exists. Please sign in";
                                }
                                else
                                {
                                    request.User.Status = await context.Statuses.FirstAsync(item => item.Name == "Online");
                                    
                                    response.User = (await context.Users.AddAsync(request.User)).Entity;
                                }
                                break;

                            case "NEW":
                                opponent = await context.Users
                                    .Include(item => item.Status)
                                    .FirstOrDefaultAsync(item => item.Login == request.Login);

                                if (opponent is null)
                                {
                                    response.Message = $"User {request.Login} not found";
                                    break;
                                }

                                user = await context.Users
                                    .Include(item => item.BlockedUsers)
                                    .FirstAsync(item => item.Login == request.User.Login
                                        && item.Password == request.User.Password);

                                if (user.BlockedUsers.Any(item => item.Login == request.Login) == false)
                                {
                                    response.Chat = new()
                                    {
                                        User = opponent
                                    };
                                }
                                else
                                {
                                    response.Message = $"User {request.Login} is blocked";
                                }
                                break;

                            case "SEND":
                                request.Message!.Sender = await context.Users
                                    .FirstAsync(item => item.Login == request.User.Login
                                        && item.Password == request.User.Password);
                                request.Message.Recipient = await context.Users
                                    .FirstAsync(item => item.Id == request.Message.Recipient.Id);

                                await context.Messages.AddAsync(request.Message);
                                break;

                            case "BLOCK":
                                user = await context.Users
                                    .FirstAsync(item => item.Login == request.User.Login
                                        && item.Password == request.User.Password);

                                opponent = await context.Users.FirstAsync(item => item.Login == request.Login);

                                user.BlockedUsers.Add(opponent);
                                break;

                            case "UNBLOCK":
                                user = await context.Users
                                    .Include(item => item.BlockedUsers)
                                    .FirstAsync(item => item.Login == request.User.Login
                                        && item.Password == request.User.Password);

                                opponent = await context.Users.FirstAsync(item => item.Login == request.Login);

                                user.BlockedUsers.Remove(opponent);
                                break;

                            case "EXIT":
                                user = await context.Users
                                    .FirstAsync(item => item.Login == request.User.Login
                                        && item.Password == request.User.Password);

                                user.Status = await context.Statuses.FirstAsync(item => item.Name == "Offline");

                                context.Users.Update(user);
                                break;
                        }

                        await context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        response.Message = ex.Message;
                    }

                    bf.Serialize(ns, response);
                }
                catch (SocketException) { }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                server = new(IPAddress.Parse("127.0.0.1"), 9001);
                server.Start();

                serverStarted = true;
                serverThread = new(ServerThread);
                serverThread.Start();

                serverStatus.Text = "on";
                serverStatus.Foreground = Brushes.Green;
                startButton.IsEnabled = false;
                stopButton.IsEnabled = true;
                logTextBox.AppendText(DateTime.Now + " -- Server started successfully\r");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                serverStarted = false;
                server?.Stop();

                serverStatus.Text = "off";
                serverStatus.Foreground = Brushes.Red;
                startButton.IsEnabled = true;
                stopButton.IsEnabled = false;
                logTextBox.AppendText(DateTime.Now + " -- Server stopped successfully\r");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Window_Closed(object sender, EventArgs e)
            => stopButton_Click(sender, null!);
    }
}