using System;

namespace Core.Models
{
    public class Person
    {
        public int ID { get; set; }
        public string ExternalId { get; set; }
        public string Username { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
