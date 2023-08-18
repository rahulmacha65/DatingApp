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
        private IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public MessagesController(IMapper mapper,IUnitOfWork uow)
        {
            _mapper = mapper;
            _uow = uow;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if (userName == createMessageDto.RecipientUserName.ToLower())
                return BadRequest("You can not send messages to yourself");

            var sender = await _uow.UserRepository.GetUserByUsernameAsync(userName);
            var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUserName);

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

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete())
            {
                return _mapper.Map<MessageDto>(message);
            }

            return BadRequest("Failed to send message");
        }
        [HttpGet]

        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.UserName = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var messages = await _uow.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage,
                messages.PageSize, messages.TotalCount, messages.TotalPages));

            return messages;
        }

        [HttpGet("thread/{userName}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string userName)
        {
            var currentUserName = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            return Ok(await _uow.MessageRepository.GetMessageThread(currentUserName, userName));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            string userName = User.FindFirst(ClaimTypes.NameIdentifier).Value;

            var message = await _uow.MessageRepository.GetMessage(id);

            if(message.SenderUserName!=userName && message.RecipientName !=userName) 
                return Unauthorized();

            if (message.SenderUserName == userName) 
                message.SenderDeleted = true;

            if (message.RecipientName == userName) 
                message.RecipientDeleted = true;
            
            if (message.SenderDeleted && message.RecipientDeleted)
                _uow.MessageRepository.DeleteMessage(message);

            if (await _uow.Complete())
                return Ok();

            return BadRequest("Problem deleting the message");
        }
    }
}
