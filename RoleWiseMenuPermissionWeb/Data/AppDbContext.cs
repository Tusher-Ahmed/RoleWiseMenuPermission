using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoleWiseMenuPermissionWeb.Models;

namespace RoleWiseMenuPermissionWeb.Data
{
    public class AppDbContext:IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
        {
            
        }

        public DbSet<MenuGroup> MenuGroups { get; set; }
        public DbSet<ControllerMenuGroup> ControllerMenuGroups { get; set; }
        public DbSet<Actions> Actions { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<MenuWisePermission> MenuWisePermission { get; set; }
        public DbSet<RoleMenuPermission> RoleMenuPermissions { get; set; }
    }
}
