namespace RoleWiseMenuPermissionWeb.Models
{
    public class Menu
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public int ParentsId { get; set; }
        public int ActionId { get; set; }
    }
}
