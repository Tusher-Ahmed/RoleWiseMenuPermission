namespace RoleWiseMenuPermissionWeb.ViewModels
{
    public class MenuPermissionByRoleViewModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public int ParentsId { get; set; }
        public int ActionId { get; set; }
        public bool Permission { get; set; }
        public bool IsChecked { get; set; }
        public string ActionName { get; set; }
        public string RoleId { get; set; }
        public string ControllerName { get; set; }
        public string AreaName { get; set; }

    }
}
