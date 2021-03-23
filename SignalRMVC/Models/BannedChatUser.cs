using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SignalRMVC.Models
{
    public class BannedChatUser
    {
        public ObjectId Id { get; set; }
        public string UserId { get; set; }
        public int ChatId { get; set; }
    }
}
