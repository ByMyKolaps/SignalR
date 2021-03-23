using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SignalRMVC.Models
{
    public class User : IdentityUser
    {
        public ICollection<ChatUser> Chats { get; set; }
    }
}
