namespace RoleWiseMenuPermissionWeb.Models
{
    public class Actions
    {
        public int Id { get; set; }
        public string ActionName { get; set; }
        public int ControllerMenuGroupId { get; set; }
    }
}
