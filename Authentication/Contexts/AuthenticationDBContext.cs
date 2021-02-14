using Authentication.Models;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Contexts
{
    public class AuthenticationDBContext : IdentityDbContext<User>
    {
        public AuthenticationDBContext(DbContextOptions<AuthenticationDBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}