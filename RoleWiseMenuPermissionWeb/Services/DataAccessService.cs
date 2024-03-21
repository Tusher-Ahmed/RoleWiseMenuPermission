using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RoleWiseMenuPermissionWeb.Data;
using RoleWiseMenuPermissionWeb.Models;
using RoleWiseMenuPermissionWeb.ViewModels;
using System.Data;
using System.Reflection;

namespace RoleWiseMenuPermissionWeb.Services
{
    public class DataAccessService : IDataAccessService
    {
        private readonly AppDbContext _context;
        public DataAccessService(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public void AddControllerMenuGroupToDb(List<ControllerMenuGroupViewModel> model)
        {
            foreach (var controller in model)
            {
                var isPresent = _context.ControllerMenuGroups.Where(x => x.ControllerName == controller.ControllerName).Any();
                var menuGroup = _context.MenuGroups.Where(x => x.AreaName == controller.AreaName).FirstOrDefault();
                if (!isPresent && controller.IsChecked && menuGroup != null)
                {
                    var AreaId = menuGroup.Id;
                    var item = new ControllerMenuGroup
                    {
                        ControllerName = controller.ControllerName,
                        MenuGroupId = AreaId,
                    };
                    _context.Add(item);
                    _context.SaveChanges();
                }
            }
        }

        public void AddMenuGroup(MenuGroupViewModel model)
        {
            var area = new MenuGroup
            {
                AreaName = model.AreaName,
            };
            _context.MenuGroups.Add(area);
            _context.SaveChanges();
        }

        public List<ControllerMenuGroupViewModel> GetControllerThrowDbAndReflection()
        {
            var controllerList = GetControllerThrowReflection();
            return controllerList;
        }

        public List<MenuGroupViewModel> LoadAreas()
        {
            var area = _context.MenuGroups.ToList();
            List<MenuGroupViewModel> menuGroupViewModels = new List<MenuGroupViewModel>();
            foreach (var item in area)
            {
                var group = new MenuGroupViewModel
                {
                    Id = item.Id,
                    AreaName = item.AreaName,
                };
                menuGroupViewModels.Add(group);
            }
            return menuGroupViewModels;
        }


        private List<ControllerMenuGroupViewModel> GetControllerThrowReflection()
        {
            List<ControllerMenuGroupViewModel> controllerList = new List<ControllerMenuGroupViewModel>();
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                           .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract);

            if (controllers.Any())
            {
                foreach (var controller in controllers)
                {
                    var controllerName = controller.Name.Substring(0, controller.Name.Length - 10);
                    var controllerAttributes = controller.GetCustomAttributes(true);
                    var authorizeAttribute = controllerAttributes.FirstOrDefault(attr => attr.GetType() == typeof(AuthorizeAttribute)) as AuthorizeAttribute;
                    if (authorizeAttribute != null)
                    {
                        string controllerNamespace = ParseAreaName(controller.Namespace);
                        var isPresent = _context.ControllerMenuGroups.Where(x => x.ControllerName == controllerName).Any();

                        var item = new ControllerMenuGroupViewModel
                        {
                            ControllerName = controllerName,
                            AreaName = controllerNamespace,
                            Permitted = isPresent
                        };
                        controllerList.Add(item);
                    }
                }
            }
            return controllerList;
        }

        string ParseAreaName(string namespaceString)
        {
            var parts = namespaceString.Split('.');
            int areasIndex = Array.IndexOf(parts, "Areas");

            if (areasIndex >= 0 && areasIndex < parts.Length - 1)
            {
                return parts[areasIndex + 1];
            }
            else
            {
                return "Unknown";
            }
        }

        public void AddActionsToDb(List<ActionsViewModel> model)
        {
            foreach (var action in model)
            {
                if (!action.IsPresent && action.IsChecked)
                {
                    var item = new Actions
                    {
                        ActionName = action.ActionName,
                        ControllerMenuGroupId = action.ControllerMenuGroupId,
                    };
                    _context.Actions.Add(item);
                    _context.SaveChanges();
                }
            }
        }

        public List<ActionsViewModel> GetActionsFromDb()
        {
            var actions = _context.Actions.ToList();
            List<ActionsViewModel> list = new List<ActionsViewModel>();

            foreach (var action in actions)
            {
                var IsPresent = _context.Menus.Where(x => x.ActionId == action.Id).Any();
                if (!IsPresent)
                {
                    var item = new ActionsViewModel
                    {
                        ActionName = action.ActionName,
                        Id = action.Id,
                        ControllerMenuGroupId = action.ControllerMenuGroupId,
                    };
                    list.Add(item);
                }

            }
            return list;
        }

        public List<MenuViewModel> GetMenusFromDb()
        {
            var list = new List<MenuViewModel>();
            var MenuList = _context.Menus.Where(x=>x.ParentsId == 0).ToList();
            foreach (var menu in MenuList)
            {
                var item = new MenuViewModel
                {
                    DisplayName = menu.DisplayName,
                    Id = menu.Id,
                };
                list.Add(item);
            }
            return list;
        }

        public List<MenuGroupViewModel> GetAreaFromDb()
        {
            var menuGroup= _context.MenuGroups.ToList();
            var list = new List<MenuGroupViewModel>();
            foreach(var group in menuGroup)
            {
                var item = new MenuGroupViewModel
                {
                    Id = group.Id,
                    AreaName = group.AreaName,
                };
                list.Add(item);
            }
            return list.ToList();
        }

        public List<ActionsViewModel> GetAreasFromDb()
        {
            var action = _context.Actions.ToList();
            var list = new List<ActionsViewModel>();
            foreach(var act in action)
            {
                var item = new ActionsViewModel
                {
                    Id = act.Id,
                    ControllerMenuGroupId = act.ControllerMenuGroupId,
                };
                list.Add(item);
            }
            return list.ToList();
        }

        public List<ControllerMenuGroupViewModel> GetControllerFromDb()
        {
            var controlers= _context.ControllerMenuGroups.ToList();
            var list = new List<ControllerMenuGroupViewModel>();
            foreach(var control in controlers)
            {
                var item = new ControllerMenuGroupViewModel()
                {
                    Id= control.Id,
                    ControllerName = control.ControllerName,
                };
                list.Add(item);
            }
            return list.ToList();
        }

        public void AddRoleMenuPermission(List<MenuPermissionByRoleViewModel> model)
        {
            foreach (var item in model)
            {

                var menu = _context.Menus.FirstOrDefault(x => x.Id == item.Id);
                if (menu != null)
                {
                    var IsPresent = _context.RoleMenuPermissions.Where(x => x.RoleId == item.RoleId && x.MenuWisePermissionId == menu.Id).Any();

                    if (!IsPresent && item.IsChecked)
                    {

                        var menuPermission = new RoleMenuPermission
                        {
                            RoleId = item.RoleId,
                            MenuWisePermissionId = menu.Id
                        };
                        _context.RoleMenuPermissions.Add(menuPermission);

                    }
                    if (IsPresent && !item.IsChecked)
                    {
                        var menuPermission = _context.RoleMenuPermissions
                   .FirstOrDefault(x => x.RoleId == item.RoleId && x.MenuWisePermissionId == menu.Id);
                        _context.RoleMenuPermissions.Remove(menuPermission);
                    }
                }
            }
            _context.SaveChanges();
        }

        public void AddMenuToDb(MenuViewModel model)
        {
            var isPresent = _context.Menus.Where(x => x.DisplayName.ToLower() == model.DisplayName.ToLower() && x.ActionId == model.ActionId && x.ParentsId == model.ParentsId).Any();
            if (!isPresent)
            {
                // var ActionName = _context.Actions.Find(model.ActionId);
                var item = new Menu
                {
                    DisplayName = model.DisplayName,
                    ActionId = model.ActionId,
                    ParentsId = model.ParentsId
                };
                _context.Menus.Add(item);
                _context.SaveChanges();
            }
        }

        public List<MenuViewModel> LoadMenus()
        {
            var menus = _context.Menus.ToList();
            List<MenuViewModel> menuViewModels = new List<MenuViewModel>();

            foreach (var menu in menus)
            {
                string actionName = string.Empty;
                string parentName = string.Empty;

                if (menu.ActionId != 0)
                {
                    actionName = _context.Actions.Find(menu.ActionId).ActionName;
                }
                if (menu.ParentsId != 0)
                {
                    parentName = _context.Menus.Find(menu.ParentsId).DisplayName;
                }

                var item = new MenuViewModel
                {
                    DisplayName = menu.DisplayName,
                    ActionName = actionName,
                    ParentName = parentName
                };
                menuViewModels.Add(item);
            }
            return menuViewModels;
        }

        public List<ActionsViewModel> GetActionsThrowReflectionAndDb()
        {
            List<ActionsViewModel> actions = new List<ActionsViewModel>();
            var controllers = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(ControllerBase).IsAssignableFrom(type) && !type.IsAbstract);
            if (controllers.Any())
            {
                foreach (var controller in controllers)
                {
                    var controllerName = controller.Name.Substring(0, controller.Name.Length - 10);
                    var controllerAttributes = controller.GetCustomAttributes(true);
                    var authorizeAttribute = controllerAttributes.FirstOrDefault(attr => attr.GetType() == typeof(AuthorizeAttribute)) as AuthorizeAttribute;
                    if (authorizeAttribute != null)
                    {
                        var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                        var AreaController = _context.ControllerMenuGroups.Where(x => x.ControllerName == controllerName).FirstOrDefault();
                        foreach (var method in methods)
                        {
                            var attributes = method.GetCustomAttributes(true);
                            var allowAnonymousAttribute = attributes.FirstOrDefault(attr => attr.GetType() == typeof(AllowAnonymousAttribute)) as AllowAnonymousAttribute;

                            if (allowAnonymousAttribute == null)
                            {

                                if (AreaController != null)
                                {
                                    var isPresent = _context.Actions.Where(item => item.ControllerMenuGroupId == AreaController.Id && item.ActionName == method.Name).Any();

                                    var returnType = method.ReturnType;

                                    if (typeof(IActionResult).IsAssignableFrom(returnType) || typeof(Task<IActionResult>).IsAssignableFrom(returnType))
                                    {
                                        var menuItem = new ActionsViewModel
                                        {
                                            ActionName = method.Name,
                                            ControllerMenuGroupName = controllerName,
                                            ControllerMenuGroupId = AreaController.Id,
                                            IsPresent = isPresent
                                        };

                                        actions.Add(menuItem);
                                    }

                                }
                            }


                        }
                    }
                }
            }
            return actions;
        }

        public List<MenuPermissionByRoleViewModel> GetPermissionsByRoleId(string id)
        {
            var sql = @"
SELECT
m.Id,
m.DisplayName,
m.ParentsId,
m.ActionId,
rmp.RoleId,
mp.Permission,
mp.Permission as IsChecked,
a.ActionName,
cmg.ControllerName,
mg.AreaName
FROM Menus AS m
LEFT JOIN MenuWisePermission AS mp ON mp.MenuId = m.Id
LEFT JOIN Actions AS a ON a.Id = m.ActionId
LEFT JOIN ControllerMenuGroups AS cmg ON cmg.Id = a.ControllerMenuGroupId
LEFT JOIN MenuGroups AS mg ON mg.Id = cmg.MenuGroupId
LEFT JOIN RoleMenuPermissions AS rmp ON rmp.MenuWisePermissionId = mp.Id AND rmp.RoleId = @RolesId
ORDER BY m.ParentsId,m.Id
";
            var result = LoadRoleMenuByDapper(sql, new {RolesId = id});
            return result;
        }

        public List<MenuPermissionByRoleViewModel> LoadRoleMenuByDapper( string query,object parameter)
        {
            using (IDbConnection db = new SqlConnection(_context.Database.GetConnectionString()))
            {
                return db.Query<MenuPermissionByRoleViewModel>(query,parameter).ToList();
            }
        }

        public List<MenuPermissionViewModel> GetMenus()
        {
            var sql = @"
SELECT
m.Id,
m.DisplayName,
m.ParentsId,
m.ActionId,
a.ActionName,
cmg.ControllerName
FROM Menus AS m
LEFT JOIN MenuWisePermission AS mp ON mp.MenuId = m.Id
LEFT JOIN Actions AS a ON a.Id = m.ActionId
LEFT JOIN ControllerMenuGroups AS cmg ON cmg.Id = a.ControllerMenuGroupId
LEFT JOIN MenuGroups AS mg ON mg.Id = cmg.MenuGroupId
";
            var result = LoadMenuByDapper(sql);
            return result;
        }
        public List<MenuPermissionViewModel> LoadMenuByDapper(string query)
        {
            using (IDbConnection db = new SqlConnection(_context.Database.GetConnectionString()))
            {
                return db.Query<MenuPermissionViewModel>(query).ToList();
            }
        }

        public bool IsAlreadyPermitted(int menuId, string roleId)
        {
            return _context.RoleMenuPermissions.Where(x=>x.RoleId == roleId && x.MenuWisePermissionId == menuId).Any();
        }

        public List<MenuAccessViewModel> GetMenusByUserName(string name)
        {
            var sql = @"
SELECT DISTINCT
m.Id,
m.DisplayName,
a.ActionName,
cmg.ControllerName,
mg.AreaName,
m.ParentsId,
ap.DisplayName as ParentName
FROM RoleMenuPermissions AS rmp
LEFT JOIN Menus AS m ON rmp.MenuWisePermissionId = m.Id 
LEFT JOIN Actions AS a ON m.ActionId = a.Id
LEFT JOIN Menus AS ap ON ap.Id = m.ParentsId
LEFT JOIN ControllerMenuGroups AS cmg ON cmg.Id = a.ControllerMenuGroupId
LEFT JOIN MenuGroups AS mg ON mg.Id = cmg.MenuGroupId
LEFT JOIN AspNetUsers AS u ON u.UserName = 'tusher@gmail.com'
LEFT JOIN AspNetUserRoles AS ur ON ur.UserId = u.Id
";
            var result = LoadPermissionedMenusByDapper(sql, new { UserName = name });
            return result;
        }

        public List<MenuAccessViewModel> LoadPermissionedMenusByDapper(string query, object parameter)
        {
            using (IDbConnection db = new SqlConnection(_context.Database.GetConnectionString()))
            {
                return db.Query<MenuAccessViewModel>(query, parameter).ToList();
            }
        }
        
        public List<MenuAccessViewModel> GetMenusForSuperAdmin()
        {
            var sql = @"
SELECT DISTINCT
m.Id,
m.DisplayName,
a.ActionName,
cmg.ControllerName,
mg.AreaName,
m.ParentsId,
ap.DisplayName as ParentName
FROM Menus AS m 
LEFT JOIN Actions AS a ON m.ActionId = a.Id
LEFT JOIN Menus AS ap ON ap.Id = m.ParentsId
LEFT JOIN ControllerMenuGroups AS cmg ON cmg.Id = a.ControllerMenuGroupId
LEFT JOIN MenuGroups AS mg ON mg.Id = cmg.MenuGroupId
WHERE m.ParentsId != 0
";
            var result = LoadSuperAdminMenusByDapper(sql);
            return result;
        }
        public List<MenuAccessViewModel> LoadSuperAdminMenusByDapper(string query)
        {
            using (IDbConnection db = new SqlConnection(_context.Database.GetConnectionString()))
            {
                return db.Query<MenuAccessViewModel>(query).ToList();
            }
        }

        public List<MenuViewModel> LoadMenuNamesByControllerId(int controllerId)
        {
            var sql = @"
SELECT
m.Id,
m.DisplayName
FROM Menus AS m
LEFT JOIN ControllerMenuGroups AS cmg ON cmg.Id = @ControllerId
LEFT JOIN Actions AS a ON cmg.Id = a.ControllerMenuGroupId
WHERE m.ActionId = a.Id AND m.ParentsId != 0
";
            var result = LoadMenuNamesByDapper(sql, new { ControllerId = controllerId });
            return result;
        }

        public List<MenuViewModel> LoadMenuNamesByDapper(string query, object parameter)
        {
            using (IDbConnection db = new SqlConnection(_context.Database.GetConnectionString()))
            {
                return db.Query<MenuViewModel>(query, parameter).ToList();
            }
        }
    }
}
