using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SignalRMVC.Database;
using SignalRMVC.Models;

namespace SignalRMVC.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        AppDbContext _appDbContext;
        public HomeController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            string connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("BanChatUser");
            var collection = database.GetCollection<BannedChatUser>("bannedUsers");
            var filter = new BsonDocument("UserId", userId);
            var bannedChats = await collection.Find(filter).ToListAsync();

            IEnumerable<Chat> chats = _appDbContext.Chats
                .Include(chat => chat.Users)
                .Where(chat => !chat.Users
                    .Any(user => user.UserId == userId)
                    && chat.Type == ChatType.Room)
                .ToList();

            IEnumerable<Chat> chatsResult = chats
                .Where(chat => !bannedChats.Any(banned => banned.ChatId == chat.Id));

            return View(chatsResult);
        }

        public IActionResult Find()
        {
            var users = _appDbContext.Users
                .Where(x => x.Id != User.FindFirst(ClaimTypes.NameIdentifier).Value)
                .ToList();
            return View(users);
        }

        public IActionResult Private()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var chats = _appDbContext.Chats
                .Include(x => x.Users)
                    .ThenInclude(x => x.User)
                .Where(x => x.Type == ChatType.Private
                    && x.Users
                        .Any(y => y.UserId == userId))
                .ToList();
            return View(chats);
        }

        public async Task<IActionResult> CreatePrivateRoom(string userId)
        {
            var chat = new Chat
            {
                Type = ChatType.Private
            };
            chat.Users.Add(new ChatUser
            {
                UserId = userId
            });
            chat.Users.Add(new ChatUser
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value
            });

            _appDbContext.Chats.Add(chat);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Chat", new { id = chat.Id });
        }
        [HttpGet("{id}")]
        public IActionResult Chat(int id)
        {
            var chat = _appDbContext.Chats
                .Include(chat => chat.Messages)
                .Include(chat => chat.Users)
                    .ThenInclude(user => user.User)
                .FirstOrDefault(chat => chat.Id == id);
            return View(chat);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(int chatId, string text)
        {
            var message = new Message
            {
                ChatId = chatId,
                Text = text,
                Name = User.Identity.Name,
                Timestamp = DateTime.Now
            };
            _appDbContext.Add(message);
            await _appDbContext.SaveChangesAsync();

            return RedirectToAction("Chat", new { id = chatId });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom(string name)
        {
            var chat = new Chat
            {
                Name = name,
                Type = ChatType.Room
            };
            chat.Users.Add(new ChatUser
            {
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                Role = UserRole.Admin
            });
            _appDbContext.Chats.Add(chat);
            await _appDbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> JoinRoom(int id)
        {
            var chatUser = new ChatUser
            {
                ChatId = id,
                UserId = User.FindFirst(ClaimTypes.NameIdentifier).Value,
                Role = UserRole.Member
            };
            _appDbContext.ChatUsers.Add(chatUser);
            await _appDbContext.SaveChangesAsync();
            return RedirectToAction("Chat", "Home", new { id = id });
        }

        public async Task<IActionResult> KickMember(string userId, int chatId)
        {
            var chatUser = _appDbContext.ChatUsers
                .Where(user => user.UserId == userId && user.ChatId == chatId)
                .FirstOrDefault();

            string connectionString = "mongodb://localhost:27017";
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase("BanChatUser");
            var collection = database.GetCollection<BannedChatUser>("bannedUsers");
            BannedChatUser banned = new BannedChatUser
            {
                ChatId = chatId,
                UserId = userId
            };

            await collection.InsertOneAsync(banned);

            _appDbContext.ChatUsers.Remove(chatUser);
            await _appDbContext.SaveChangesAsync();
            return RedirectToAction("Chat", new { id = chatId });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
