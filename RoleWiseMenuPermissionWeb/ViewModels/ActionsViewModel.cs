namespace RoleWiseMenuPermissionWeb.ViewModels
{
    public class ActionsViewModel
    {
        public int Id { get; set; }
        public string ActionName { get; set; }
        public int ControllerMenuGroupId { get; set; }
        public string ControllerMenuGroupName { get; set; }
        public bool IsChecked { get; set; }
        public bool IsPresent { get; set; }
    }
}
