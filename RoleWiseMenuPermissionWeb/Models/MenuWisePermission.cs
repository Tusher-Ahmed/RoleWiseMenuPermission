namespace RoleWiseMenuPermissionWeb.Models
{
    public class MenuWisePermission
    {
        public int Id { get; set; }
        public int MenuId {  get; set; }
        public bool Permission {  get; set; }
    }
}
