﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using DatingApp.DTOs;
using DatingApp.Entities;
using DatingApp.HelperClasses;
using DatingApp.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DatingApp.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context,IMapper mapper) 
        {
            _context = context;
            _mapper = mapper;
        }
        public void AddMessage(Message message)
        {
           _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(x => x.MessageSent).AsQueryable();
            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientName == messageParams.UserName && u.RecipientDeleted==false),
                "Outbox" => query.Where(u => u.SenderUserName == messageParams.UserName && u.SenderDeleted==false),
                _ => query.Where(u=>u.RecipientName == messageParams.UserName && u.DateRead==null  && u.RecipientDeleted==false)
            };

            var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string senderUserName, string recipientUserName)
        {
            var messages = await _context.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(r => r.Recipient).ThenInclude(pr => pr.Photos)
                .Where(
                    m => m.RecipientName == senderUserName
                        && m.RecipientDeleted == false
                        && m.SenderUserName == recipientUserName
                        || m.RecipientName == recipientUserName
                        && m.SenderDeleted == false
                        && m.SenderUserName == senderUserName
                ).OrderBy(m => m.MessageSent).ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null
                                && m.RecipientName == senderUserName).ToList();

            if(unreadMessages.Any())
            {
                foreach(var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
                await _context.SaveChangesAsync();
            }

            return _mapper.Map<IEnumerable<MessageDto>>(messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync()>0;
        }
    }
}