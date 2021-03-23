using Microsoft.AspNetCore.Mvc;
using SignalRMVC.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SignalRMVC.ViewComponents
{
    public class CurrentUserViewComponent : ViewComponent
    {
        AppDbContext _appDbContext;
        public CurrentUserViewComponent(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public IViewComponentResult Invoke()
        {
            var userId = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            var user = _appDbContext.Users
                .Where(user => user.Id == userId)
                .FirstOrDefault();

            return View(user);
        }
    }
}
