using DoAnLapTrinhWeb.Areas.Identity.Data;
using DoAnLapTrinhWeb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAnLapTrinhWeb.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<AppliactionUser> _userManager;

        public UserController(UserManager<AppliactionUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IActionResult> index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> index(AppliactionUser user, IFormFile ProfileImage)
        {
            var existingUser = await _userManager.GetUserAsync(User);
            ModelState.Remove("ProfileImage");
            if (ModelState.IsValid)
            {
                if (user == null) { return NotFound(); }
                if (ProfileImage == null)
                {
                    user.ProfileImage = existingUser.ProfileImage;
                }
                else
                {
                    user.ProfileImage = await SaveImage(ProfileImage);
                }
                existingUser.LastName = user.LastName;
                existingUser.FirstName = user.FirstName;
                existingUser.ProfileImage = user.ProfileImage;
                var result = await _userManager.UpdateAsync(existingUser);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    // Handle update failure
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(existingUser);
        }
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/ProfileImage", image.FileName); // Thay đổi đường dẫn theo cấu hình của bạn
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/ProfileImage/" + image.FileName; // Trả về đường dẫn tương đối
        }
    }
}
