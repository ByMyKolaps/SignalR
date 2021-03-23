using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRMVC.Database;
using SignalRMVC.Hubs;
using SignalRMVC.Models;

namespace SignalRMVC.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        AppDbContext _appDbContext;
        IHubContext<ChatHub> _chat;
        public ChatController(IHubContext<ChatHub> chat, AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
            _chat = chat;
        }

        [HttpPost("Chat/JoinRoom/{connectionId}/{roomId}")/*("[action]/{connectionId}/{roomName}")*/] /*?????????*/
        public async Task<IActionResult> JoinRoom(string connectionId, string roomId)
        {
            await _chat.Groups.AddToGroupAsync(connectionId, roomId);
            return Ok();
        }

        [HttpPost("Chat/LeaveRoom/{connectionId}/{roomId}")]
        public async Task<IActionResult> LeaveRoom(string connectionId, string roomId)
        {
            await _chat.Groups.RemoveFromGroupAsync(connectionId, roomId);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(string text, int roomId)
        {
            var message = new Message
            {
                ChatId = roomId,
                Text = text,
                Name = User.Identity.Name,
                Timestamp = DateTime.Now
            };
            _appDbContext.Add(message);
            await _appDbContext.SaveChangesAsync();

            await _chat.Clients.Group(roomId.ToString())
                .SendAsync("RecieveMessage", new
                {
                    Text = message.Text,
                    Name = message.Name,
                    Timestamp = message.Timestamp.ToString("dd/MM/yyyy hh:mm:ss")
                });
            
            
            return Ok();
        }
    }
}
