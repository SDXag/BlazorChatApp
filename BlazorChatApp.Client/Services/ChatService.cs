using Microsoft.AspNetCore.SignalR.Client;
using BlazorChatApp.Shared.Models;
using System.Threading.Tasks;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using BlazorChatApp.Models;
using System.Collections.Generic;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http.Connections;
using Microsoft.Extensions.Configuration;

namespace BlazorChatApp.Services
{
    public class ChatService
    {
        private readonly HttpClient Http;
        private readonly IConfiguration Configuration;

        public User User { get; private set; }

        private IDictionary<string, List<Message>> _messages;
        public IEnumerable<string> TargetUsers => _messages.Keys;

        public delegate void UserChangeEventHandler();
        public event UserChangeEventHandler UsersChanged;

        public delegate void NewMessageEventHandler(string user);
        public event NewMessageEventHandler NewMessage;

        private readonly HubConnection _hubConnection;

        public ChatService(HttpClient client, IConfiguration configuration)
        {
            Configuration = configuration;
            Http = client;
            Http.BaseAddress = new System.Uri(Configuration["API_Base_URL"]);
            Http.DefaultRequestHeaders.Add("x-functions-key", Configuration["API_Host_Key"]);

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(Configuration["API_URL_SignalR_Negotiation"], o => o.Transports = HttpTransportType.LongPolling)
                .Build();
            
            _messages = new Dictionary<string, List<Message>>();
        }

        public async Task SignInUser(User newUser)
        {
            User = newUser;
            await RetrieveMessages();
            await ConnectToSignalR();
        }

        public async Task SignOutUser()
        {
            if (User != null)
            {
                _hubConnection.Remove($"newMessage-{User.UserName}");
                await _hubConnection.StopAsync();
            }

            User = null;
            _messages = new Dictionary<string, List<Message>>();
            UsersChanged?.Invoke();
        }

        private async Task RetrieveMessages()
        {
            if (User != null)
            {
                var messages = await Http.GetJsonAsync<IEnumerable<Message>>($"{Configuration["API_URL_PART_GetMessages"]}/{User.UserName}");
                if (messages != null)
                {
                    Console.WriteLine($"Received {messages.Count()} messages");
                    _messages = messages.GroupBy(m => m.TargetUserName != User.UserName ? m.TargetUserName : m.SourceUserName).ToDictionary(g => g.Key, g => g.OrderBy(e => e.DateTime).ToList());

                    UsersChanged?.Invoke();
                }


            }
        }

        private async Task ConnectToSignalR()
        {
            if (User != null)
            {                
                _hubConnection.On<Message>($"newMessage-{User.UserName}", MessageReceived);
                await _hubConnection.StartAsync();
            }
        }

        private void MessageReceived(Message message)
        {
            var sourceUser = message.SourceUserName;
            if (!_messages.ContainsKey(sourceUser))
            {
                _messages.Add(sourceUser, new List<Message>());
                UsersChanged?.Invoke();
            }

            _messages[sourceUser].Add(message);
            NewMessage?.Invoke(sourceUser);
        }

        public void AddTargetUser(string newTargetUser)
        {
            if (!_messages.ContainsKey(newTargetUser))
            {
                _messages.Add(newTargetUser, new List<Message>());
                UsersChanged?.Invoke();
            }
        }

        public async Task SendMessage(string targetUser, string content)
        {
            if (User != null)
            {
                var message = new Message
                {
                    SourceUserName = User.UserName,
                    TargetUserName = targetUser,
                    Content = content,
                    DateTime = DateTime.Now
                };

                await Http.PostJsonAsync(Configuration["API_URL_PART_SendMessages"], message);

                _messages[targetUser].Add(message);
                NewMessage?.Invoke(targetUser);
            }
        }

        public IEnumerable<Message> GetMessages(string user) => _messages[user];
    }
}