using AutoMapper;
using DatingApp.DTOs;
using DatingApp.Extensions;
using DatingApp.HelperClasses;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Xml.Linq;

namespace DatingApp.Controllers
{
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private IMapper _mapper;
        public MessagesController(IUserRepository userRepository, IMessageRepository messageRepository,
            IMapper mapper)
        {
            _mapper = mapper;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (userName == createMessageDto.RecipientUserName.ToLower())
                return BadRequest("You can not send messages to yourself");

            var sender = await _userRepository.GetUserByUsernameAsync(userName);
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUserName);

            if (recipient == null) return NotFound($"{createMessageDto.RecipientUserName} not found");

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
                return _mapper.Map<MessageDto>(message);
            }

            return BadRequest("Failed to send message");
        }
        [HttpGet]

        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var messages = await _messageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage,
                messages.PageSize, messages.TotalCount, messages.TotalPages));

            return messages;
        }

        [HttpGet("thread/{userName}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string userName)
        {
            var currentUserName = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            return Ok(await _messageRepository.GetMessageThread(currentUserName, userName));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var message = await _messageRepository.GetMessage(id);

            if(message.SenderUserName!=userName && message.RecipientName !=userName) 
                return Unauthorized();

            if (message.SenderUserName == userName) 
                message.SenderDeleted = true;

            if (message.RecipientName == userName) 
                message.RecipientDeleted = true;
            
            if (message.SenderDeleted && message.RecipientDeleted)
                _messageRepository.DeleteMessage(message);

            if (await _messageRepository.SaveAllAsync())
                return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}
