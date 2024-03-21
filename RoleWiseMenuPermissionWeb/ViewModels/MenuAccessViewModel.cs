namespace RoleWiseMenuPermissionWeb.ViewModels
{
    public class MenuAccessViewModel
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public string AreaName { get; set; }
        public string ParentName { get; set; }
        public int ParentsId {  get; set; }
        public List<MenuAccessViewModel> Children { get; set; } = new List<MenuAccessViewModel>();
    }
}
