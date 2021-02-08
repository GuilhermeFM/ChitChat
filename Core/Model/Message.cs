using System;

namespace Core.Models
{
    public class Message
    {
        public int ID { get; set; }
        public User Sender { get; set; }
        public User Reciver { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }
}
