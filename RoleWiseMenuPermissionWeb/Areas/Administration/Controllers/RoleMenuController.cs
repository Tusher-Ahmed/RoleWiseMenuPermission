using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleWiseMenuPermissionWeb.Services;
using RoleWiseMenuPermissionWeb.ViewModels;

namespace RoleWiseMenuPermissionWeb.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize]
    public class RoleMenuController : Controller
    {
        private readonly IDataAccessService _dataAccessService;
        public RoleMenuController(IDataAccessService dataAccessService)
        {
            _dataAccessService = dataAccessService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAreas()
        {
            var areas = _dataAccessService.LoadAreas();
            return View(areas);
        }

        [HttpGet]
        public IActionResult CreateArea()
        {
            return View();
        }

        [HttpPost]
        public IActionResult CreateArea(MenuGroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                _dataAccessService.AddMenuGroup(model);
                return RedirectToAction("GetAreas");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult GetController()
        {
            var controller = _dataAccessService.GetControllerThrowDbAndReflection();

            return View(controller);
        }
        [HttpPost]
        public IActionResult AddController(List<ControllerMenuGroupViewModel> list)
        {
            if (list == null)
            {
                return RedirectToAction("GetController");
            }

            _dataAccessService.AddControllerMenuGroupToDb(list);

            return RedirectToAction("GetController");
        }
        [HttpGet]
        public IActionResult GetActions()
        {
            var Actions = _dataAccessService.GetActionsThrowReflectionAndDb();
            return View(Actions);
        }

        [HttpPost]
        public IActionResult AddActions(List<ActionsViewModel> list)
        {
            if (list == null)
            {
                return RedirectToAction("GetActions");
            }

            _dataAccessService.AddActionsToDb(list);
            return RedirectToAction("GetActions");
        }

        [HttpGet]
        public IActionResult GetMenu()
        {
            var Actiondata = _dataAccessService.GetActionsFromDb();
            var parent = _dataAccessService.GetMenusFromDb();
            ViewData["Actions"] = new SelectList(Actiondata, "Id", "ActionName");
            ViewData["Parent"] = new SelectList(parent, "Id", "DisplayName");
            return View();
        }

        [HttpPost]
        public IActionResult AddMenu(MenuViewModel model)
        {
            if (model == null)
            {
                return RedirectToAction("Getmenu");
            }

            _dataAccessService.AddMenuToDb(model);
            return RedirectToAction("Getmenu");
        }

        [HttpGet]
        public IActionResult ShowMenus()
        {
            var data = _dataAccessService.LoadMenus();
            return View(data);
        }
    }
}
