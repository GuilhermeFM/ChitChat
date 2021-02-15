using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Contexts;
using Core.Models;

namespace Core.Services
{
    public class MessageService
    {
        private CoreDBContext context;

        public IQueryable<Message> Persons
        {
            get
            {
                return this.context.Messages.AsQueryable();
            }
        }

        public MessageService(CoreDBContext context)
        {
            this.context = context;
        }
    }
}
