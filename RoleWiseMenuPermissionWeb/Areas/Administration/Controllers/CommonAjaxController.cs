using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RoleWiseMenuPermissionWeb.Services;

namespace RoleWiseMenuPermissionWeb.Areas.Administration.Controllers
{
    [Area("Administration")]
    [Authorize]
    public class CommonAjaxController : Controller
    {
        private readonly IDataAccessService _dataAccessService;
        public CommonAjaxController(IDataAccessService dataAccessService)
        {
            _dataAccessService = dataAccessService;
        }
        public IActionResult Index()
        {
            return View();
        }

        public JsonResult LoadMenuName(int controllerId)
        {
            var menuList = _dataAccessService.LoadMenuNamesByControllerId(controllerId);
            var menuNameSelectList = new SelectList(menuList, "Id", "DisplayName");
            return Json(new {returnMenuList = menuNameSelectList, IsSuccess = true});
        }
    }
}
