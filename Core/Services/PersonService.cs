using System;
using System.Linq;
using System.Threading.Tasks;
using Core.Contexts;
using Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Core.Services
{
    public class PersonService
    {
        private CoreDBContext context;

        public IQueryable<Person> Persons
        {
            get
            {
                return this.context.Persons.AsQueryable();
            }
        }

        public PersonService(CoreDBContext context)
        {
            this.context = context;
        }

        public async Task CreaterPerson(Person person)
        {
            var usernameExists = await context.Persons.AnyAsync(u => u.Username == person.Username);
            if (usernameExists)
                throw new Exception("Username already taken");

            context.Persons.Add(person);
            context.SaveChanges();
        }
    }
}
