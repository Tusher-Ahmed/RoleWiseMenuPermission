using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using RoleWiseMenuPermissionWeb.Data;
using RoleWiseMenuPermissionWeb.Services;
using RoleWiseMenuPermissionWeb.ViewModels;
using System.Security.Claims;

namespace RoleWiseMenuPermissionWeb
{
    public class AuthorizeAccess : ActionFilterAttribute
    {
        private readonly IDataAccessService _dataAccessService;
        public AuthorizeAccess(IDataAccessService dataAccessService)
        {
            _dataAccessService = dataAccessService;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            var currentUser = filterContext.HttpContext.User;
            var context = filterContext.HttpContext.RequestServices.GetRequiredService<AppDbContext>();

            var name = currentUser.Identity.Name;
            var actionDescriptor = filterContext.ActionDescriptor;
            var authorizeAttributeName = GetAuthorizeAttributeName(actionDescriptor);
            var userRoles = filterContext.HttpContext.User.Claims
        .FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role);
            var controllerName = actionDescriptor.RouteValues["controller"];
            var actionName = actionDescriptor.RouteValues["action"];
            var controllerType = filterContext.Controller.GetType();
            var isAjaxRequest = filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var hasAuthorizeC = controllerType.GetCustomAttributes(typeof(AuthorizeAttribute), true).Any();
            string authorizeKeyword = hasAuthorizeC ? "Authorize" : "";

            if(currentUser != null && currentUser.Identity != null && currentUser.Identity.IsAuthenticated && name != null && userRoles == null)
            {
                var menuList = _dataAccessService.GetMenusByUserName(name);
                var isAllowedMenu = menuList.Where(x => x.ControllerName == controllerName && x.ActionName == actionName).Any();
                //var httpRequest = filterContext.HttpContext.Request;

                if (!isAllowedMenu && authorizeKeyword != "" && authorizeAttributeName == null && isAjaxRequest == false)
                {
                    RedirectToPermissionDenied(filterContext);
                }

                var menuDictionary = new Dictionary<string, List<MenuAccessViewModel>>();

                if (menuList != null)
                {
                    foreach (var menu in menuList)
                    {
                        if (!menuDictionary.ContainsKey(menu.ParentName))
                        {
                            menuDictionary[menu.ParentName] = new List<MenuAccessViewModel>();
                        }

                        menuDictionary[menu.ParentName].Add(menu);
                    }
                }
                filterContext.HttpContext.Items["MenuList"] = menuDictionary;
            }

            else if (currentUser != null && currentUser.Identity != null && currentUser.Identity.IsAuthenticated && name != null && userRoles.Value != "SuperAdmin")
            {
                var menuList = _dataAccessService.GetMenusByUserName(name);
                var isAllowedMenu = menuList.Where(x=>x.ControllerName == controllerName && x.ActionName == actionName).Any();

                if (!isAllowedMenu && authorizeKeyword != "" && authorizeAttributeName == null && isAjaxRequest == false )
                {
                    RedirectToPermissionDenied(filterContext);
                }

                var menuDictionary = new Dictionary<string, List<MenuAccessViewModel>>();

                if (menuList != null)
                {
                    foreach ( var menu in menuList )
                    {
                        if (!menu.ActionName.StartsWith("Edit", StringComparison.OrdinalIgnoreCase) && !menu.ActionName.StartsWith("Update", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!menuDictionary.ContainsKey(menu.ParentName))
                            {
                                menuDictionary[menu.ParentName] = new List<MenuAccessViewModel>();
                            }

                            menuDictionary[menu.ParentName].Add(menu);
                        }                      
                    }
                }
                filterContext.HttpContext.Items["MenuList"] = menuDictionary;
            }

            else if (currentUser != null && currentUser.Identity != null && currentUser.Identity.IsAuthenticated && name != null && userRoles.Value == "SuperAdmin")
            {
                var menuList = _dataAccessService.GetMenusForSuperAdmin();
                var menuDictionary = new Dictionary<string, List<MenuAccessViewModel>>();
                if (menuList != null)
                {
                    foreach (var menu in menuList)
                    {
                        if (!menu.ActionName.StartsWith("Edit", StringComparison.OrdinalIgnoreCase) && !menu.ActionName.StartsWith("Update", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!menuDictionary.ContainsKey(menu.ParentName))
                            {
                                menuDictionary[menu.ParentName] = new List<MenuAccessViewModel>();
                            }

                            menuDictionary[menu.ParentName].Add(menu);
                        }
                    }
                }
                filterContext.HttpContext.Items["MenuList"] = menuDictionary;
            }
        }

        private string GetAuthorizeAttributeName(ActionDescriptor actionDescriptor)
        {
            foreach (var metadata in actionDescriptor.EndpointMetadata)
            {
                if (metadata is IAllowAnonymous)
                {
                    return "AllowAnonymous";
                }
            }
            return null;
        }

        private void RedirectToPermissionDenied(ActionExecutingContext filterContext)
        {
            var rvd = new RouteValueDictionary();
            rvd.Add("message", "Permission Denied. User appropiate login");
            filterContext.Result = new RedirectToRouteResult("PermissionDenied", rvd);
        }
    }
}
