using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web3App.Models;

namespace Web3App.Controllers
{
	[Authorize(Roles = "admin")]
	[Route("/Admin/[controller]/{action=Index}/{id?}")]
	public class UsersController : Controller
	{
		private readonly UserManager<ApplicationUser> userManager;
		private readonly RoleManager<IdentityRole> roleManager;

		public UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			this.userManager = userManager;
			this.roleManager = roleManager;
		}
		public IActionResult Index(string? search)
		{


            IQueryable<ApplicationUser> users = userManager.Users.OrderByDescending(u => u.CreatedDate);
            /*
			if(search != null)
			{
				users = users.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));
			}
			*/
			
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                users = users.Where(u => u.UserName.ToLower().Contains(search) || u.Email.ToLower().Contains(search));
            }

            var filteredUsers = users.ToList();

            if (!filteredUsers.Any())
            {
                ViewBag.ErrorMessage = "User not found";
                //return RedirectToAction("Index", "Users");
            }
            else
            {
                return View(filteredUsers);
            }
            return RedirectToAction("Index", "Users");
            //return View(users.ToList());
        }

		public async Task<IActionResult> Details(string? id)
		{
			if (id == null)
			{
				return RedirectToAction("Index", "Users");
			}
			var appUser = await userManager.FindByIdAsync(id);

			if (appUser == null)
			{
				return RedirectToAction("Index", "Users");
			}

			ViewBag.Roles = await userManager.GetRolesAsync(appUser);

			return View(appUser);
		}
		/*
		public async Task<IActionResult> Profile()
		{
			var appUser = await userManager.GetUserAsync(User);
			if (appUser == null)
			{
				return RedirectToAction("Index", "Users");
			}

			var profileDto = new ProfileDto()
			{
				FirstName = appUser.FirstName,
				LastName = appUser.LastName,
				PhoneNumber = appUser.PhoneNumber,
				Address = appUser.Address,
				Email = appUser.Email ?? ""
			};

			return View(profileDto);
		}

		*/
		[HttpPost]
		public async Task<IActionResult> Details(ApplicationUser appUser)
		{
			if (!ModelState.IsValid)
			{
				ViewBag.ErrorMessage = "Please fill all the required fields with valid values";
				return View(appUser);
			}

			// Get the current user
			var user = await userManager.FindByIdAsync(appUser.Id);
			if (user == null)
			{
				return RedirectToAction("Index", "Users");
			}

			// Update the user profile
			user.FirstName = appUser.FirstName;
			user.LastName = appUser.LastName;
			user.UserName = appUser.Email;
			user.PhoneNumber = appUser.PhoneNumber;
			user.Address = appUser.Address;
			user.Email = appUser.Email;

			var result = await userManager.UpdateAsync(user);

			if (result.Succeeded)
			{
				ViewBag.SuccessMessage = "Profile updated successfully";
				return RedirectToAction("Details", new { id = appUser.Id });
			}
			else
			{
				ViewBag.ErrorMessage = "Unable to update the profile: " + result.Errors.First().Description;
			}


			return View(appUser);
		}
		
		public async Task<IActionResult> Delete(string? id)
		{
			if (id == null)
			{
				return RedirectToAction("Index", "Users");
			}
			var appUser = await userManager.FindByIdAsync(id);

			if (appUser == null)
			{
				return RedirectToAction("Index", "Users");
			}

			var currentUser = await userManager.GetUserAsync(User);
			if (currentUser.Id == appUser.Id)
			{
				TempData["ErrorMessage"] = "You cannot delete your own account!";
				return RedirectToAction("Details", "Users", new { id });
			}

			//delete account
			var result = await userManager.DeleteAsync(appUser);
			if (result.Succeeded) {
				return RedirectToAction("Index", "Users");
			}

			TempData["ErrorMessage"] = "Unable to delete this account: " + result.Errors.First().Description;
			return RedirectToAction("Details", "Users", new { id });

		}

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }

            var user = new ApplicationUser()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                UserName = registerDto.Email,
                Email = registerDto.Email,
                CreatedDate = DateTime.Now,
            };
            var result = await userManager.CreateAsync(user, registerDto.Password);

            if (result.Succeeded)
            {

                await userManager.AddToRoleAsync(user, "client");
                return RedirectToAction("Index", "Users");
            }

            return View(registerDto);
        }
    }
}

