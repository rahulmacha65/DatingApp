using AutoMapper;
using DatingApp.Data;
using DatingApp.DTOs;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace DatingApp.SignalR
{
    public class MessageHub:Hub
    {
        private readonly IMessageRepository _messageRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        public MessageHub(IMessageRepository messageRepository, IUserRepository userRepository,IMapper mapper)
        {
            _messageRepository = messageRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public override async Task OnConnectedAsync()
        {
            string userName = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var httpContext  = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"];
            var groupName = GetGroupName(userName, otherUser);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var messages = await _messageRepository.GetMessageThread(userName, otherUser);

            await Clients.Group(groupName).SendAsync("ReceiveMessageThread", messages);
        }
        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            string userName = Context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (userName == createMessageDto.RecipientUserName.ToLower())
                throw new HubException("You can not send messages to yourself");

            var sender = await _userRepository.GetUserByUsernameAsync(userName);
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUserName);

            if (recipient == null) throw new HubException($"{createMessageDto.RecipientUserName} not found");

            var message = new Entities.Message
            {
                SenderId = sender.Id,
                Sender = sender,
                RecipientId = recipient.Id,
                Recipient = recipient,
                SenderUserName = sender.UserName,
                RecipientName = recipient.UserName,
                Content = createMessageDto.Content
            };

            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAllAsync())
            {
                var group = GetGroupName(sender.UserName,recipient.UserName);
                await Clients.Group(group).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }
        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
        private string GetGroupName(string caller,string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other)<0;

            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
    }
}
