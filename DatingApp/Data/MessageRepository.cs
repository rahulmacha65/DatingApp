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

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
           _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FindAsync(id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups.Include(x=>x.Connections).FirstOrDefaultAsync(x=>x.Name == groupName);
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
            var query = _context.Messages
                .Where(
                    m => m.RecipientName == senderUserName
                        && m.RecipientDeleted == false
                        && m.SenderUserName == recipientUserName
                        || m.RecipientName == recipientUserName
                        && m.SenderDeleted == false
                        && m.SenderUserName == senderUserName
                ).OrderBy(m => m.MessageSent).AsQueryable();

            var unreadMessages = query.Where(m => m.DateRead == null
                                && m.RecipientName == senderUserName).ToList();

            if(unreadMessages.Any())
            {
                foreach(var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }
                //await _context.SaveChangesAsync();
            }

            return await query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public  void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        //public async Task<bool> SaveAllAsync()
        //{
        //    return await _context.SaveChangesAsync()>0;
        //}
    }
}
