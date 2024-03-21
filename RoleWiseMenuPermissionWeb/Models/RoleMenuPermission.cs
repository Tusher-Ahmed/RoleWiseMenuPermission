namespace RoleWiseMenuPermissionWeb.Models
{
    public class RoleMenuPermission
    {
        public int Id { get; set; }
        public string RoleId { get; set; }
        public int MenuWisePermissionId {  get; set; }
    }
}
