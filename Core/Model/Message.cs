using System;

namespace Core.Models
{
    public class Message
    {
        public int ID { get; set; }
        public Person Sender { get; set; }
        public Person Reciver { get; set; }
        public string Text { get; set; }
        public DateTime Date { get; set; }
    }
}
