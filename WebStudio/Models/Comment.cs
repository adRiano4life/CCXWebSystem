using System;

namespace WebStudio.Models
{
    public class Comment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Message { get; set; }
        public DateTime DateOfSend { get; set; }
        public DateTime DateOfChange { get; set; }

        public string UserId { get; set; }
        public virtual User User { get; set; }

        public string CardId { get; set; }
        public virtual Card Card { get; set; }
    }
}