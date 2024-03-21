using System.ComponentModel.DataAnnotations;

namespace RoleWiseMenuPermissionWeb.ViewModels
{
    public class MenuGroupViewModel
    {
        public int Id { get; set; }
        [Display(Name = "Area Name")]
        public string AreaName { get; set; }
        public bool IsPresent { get; set; }
    }
}
