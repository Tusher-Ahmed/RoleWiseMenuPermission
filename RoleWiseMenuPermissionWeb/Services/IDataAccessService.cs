using RoleWiseMenuPermissionWeb.Models;
using RoleWiseMenuPermissionWeb.ViewModels;

namespace RoleWiseMenuPermissionWeb.Services
{
    public interface IDataAccessService
    {
        List<MenuGroupViewModel> LoadAreas();
        void AddMenuGroup(MenuGroupViewModel model);
        List<ControllerMenuGroupViewModel> GetControllerThrowDbAndReflection();
        void AddControllerMenuGroupToDb(List<ControllerMenuGroupViewModel> model);
        List<ActionsViewModel> GetActionsThrowReflectionAndDb();
        void AddActionsToDb(List<ActionsViewModel> model);
        List<ActionsViewModel> GetActionsFromDb();
        List<MenuViewModel> GetMenusFromDb();
        void AddMenuToDb(MenuViewModel model);
        List<MenuViewModel> LoadMenus();
        List<MenuPermissionViewModel> GetMenus();
        bool IsAlreadyPermitted(int menuId, string roleId);
        void AddRoleMenuPermission(List<MenuPermissionByRoleViewModel> model);
        List<MenuAccessViewModel> GetMenusByUserName(string name);
        List<MenuAccessViewModel> GetMenusForSuperAdmin();
        List<MenuGroupViewModel> GetAreaFromDb();
        List<ControllerMenuGroupViewModel> GetControllerFromDb();
        List<ActionsViewModel> GetAreasFromDb();
        List<MenuViewModel> LoadMenuNamesByControllerId(int controllerId);
    }
}
