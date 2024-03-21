namespace RoleWiseMenuPermissionWeb.ViewModels
{
    public class MenuPermissionViewModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public string AreaName { get; set; }
        public int ParentsId { get; set; }
        public int ActionId { get; set; }
    }
}
