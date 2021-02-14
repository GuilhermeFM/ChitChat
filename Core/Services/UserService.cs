using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Core.Contexts;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class UserService
    {
        private CoreDBContext context;

        public IQueryable<User> Users
        {
            get
            {
                return this.context.Users.AsQueryable();
            }
        }

        public UserService(CoreDBContext context)
        {
            this.context = context;
        }

        public async Task CreaterUser(User user)
        {
            var usernameExists = await context.Users.AnyAsync(u => u.Username == user.Username);
            if (usernameExists)
                throw new Exception("Username already taken");

            var emailExists = await context.Users.AnyAsync(u => u.Email == user.Email);
            if (emailExists)
                throw new Exception("Email already taken");

            context.Users.Add(user);
            context.SaveChanges();
        }
    }
}
