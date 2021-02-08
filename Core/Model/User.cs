using System;

namespace Core.Models
{
    public class User
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public DateTime RegisterDate { get; set; }
    }
}
