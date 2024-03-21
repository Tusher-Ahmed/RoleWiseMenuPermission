using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RoleWiseMenuPermissionWeb.Services;
using RoleWiseMenuPermissionWeb.ViewModels;

namespace RoleWiseMenuPermissionWeb.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize]
    public class UsersHomeController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IDataAccessService _dataAccessService;
        public UsersHomeController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IDataAccessService dataAccessService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dataAccessService = dataAccessService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Roles()
        {
            var roleViewModel = new List<RoleViewModel>();

            var roles =  _roleManager.Roles.ToList();
            foreach (var item in roles)
            {
                if (item.Name != "SuperAdmin")
                {
                    roleViewModel.Add(new RoleViewModel()
                    {
                        Id = item.Id,
                        Name = item.Name,
                    });
                }
            }
            return View(roleViewModel);
        }

        [HttpGet]
        public IActionResult CreateRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(RoleViewModel viewModel)
        {
            if (viewModel.Name != null)
            {
                var result = await _roleManager.CreateAsync(new IdentityRole() { Name = viewModel.Name });
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Roles));
                }
                else
                {
                    ModelState.AddModelError("Name", string.Join(",", result.Errors));
                }
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Users()
        {
            var userViewModel = new List<UserViewModel>();
            var users = _userManager.Users.ToList();
            foreach (var item in users)
            {
                if (item.UserName != "superadmin@gmail.com")
                {
                    userViewModel.Add(new UserViewModel()
                    {
                        Id = item.Id,
                        Email = item.Email,
                        UserName = item.UserName,
                    });
                }
            }
            return View(userViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> EditUserPermission(string id)
        {
            var userViewModel = new UserViewModel();
            if(!string.IsNullOrWhiteSpace(id))
            {
                var user = await _userManager.FindByIdAsync(id);

                if(user == null)
                {
                    return RedirectToAction("Users");
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                userViewModel.Email = user?.Email;
                
                var roles= await _roleManager.Roles.ToListAsync();
                userViewModel.Roles = roles.Select(x => new RoleViewModel()
                {
                    Id=x.Id,
                    Name = x.Name,
                    Selected = userRoles.Contains(x.Name)  
                }).ToArray();
            }

            return View(userViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> EditUserPermission(UserViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(viewModel.Id);
                var userRoles = await _userManager.GetRolesAsync(user);

                await _userManager.RemoveFromRolesAsync(user, userRoles);
                await _userManager.AddToRolesAsync(user, viewModel.Roles.Where(x => x.Selected).Select(x => x.Name));

                return RedirectToAction("Users");
            }
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult EditRolePermission(string id)
        {
            var permissions = new List<MenuPermissionByRoleViewModel>();
            var menus = _dataAccessService.GetMenus();
            foreach(var menu in menus)
            {
                var isPresent = _dataAccessService.IsAlreadyPermitted(menu.Id, id);
                var item = new MenuPermissionByRoleViewModel
                {
                    Id = menu.Id,
                    DisplayName = menu.DisplayName,
                    ParentsId = menu.ParentsId,
                    ActionId = menu.ActionId,
                    Permission = isPresent,
                    IsChecked = isPresent,
                    RoleId = id,
                    ActionName = menu.ActionName,
                    ControllerName = menu.ControllerName,
                    AreaName = menu.AreaName
                };
                permissions.Add(item);
            }

            ViewData["Role"] = _roleManager.FindByIdAsync(id).Result.Name;
            ViewData["Menus"] = _dataAccessService.GetMenusFromDb();
            return View(permissions);
        }

        [HttpPost]
        public IActionResult EditRolePermission(List<MenuPermissionByRoleViewModel> model)
        {
            if(model == null)
            {
                return RedirectToAction("Roles");
            }
            _dataAccessService.AddRoleMenuPermission(model);
            return RedirectToAction("Roles");
        }

        [HttpGet]
        public IActionResult CheckControllerAccess()
        {
            var controllerNames = _dataAccessService.GetControllerFromDb();
            ViewBag.ControllerNameList = new SelectList(controllerNames, "Id", "ControllerName");
            List<SelectListItem> emptyList = new List<SelectListItem>();
            ViewBag.MenuList = new SelectList(emptyList, "Value", "Text");
            return View();
        }
    }
}
