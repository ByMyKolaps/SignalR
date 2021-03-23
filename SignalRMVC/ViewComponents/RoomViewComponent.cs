using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignalRMVC.Database;
using SignalRMVC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SignalRMVC.ViewComponents
{
    public class RoomViewComponent : ViewComponent
    {
        AppDbContext _appDbContext;
        public RoomViewComponent(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var chats = _appDbContext.ChatUsers
                .Include(x => x.Chat)
                .Where(x => x.UserId == userId 
                    && x.Chat.Type == ChatType.Room)
                .Select(x => x.Chat)
                .ToList();
            return View(chats);
        } 
    }
}
