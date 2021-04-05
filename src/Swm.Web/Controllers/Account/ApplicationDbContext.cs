using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Swm.Web
{
    // TODO 文件位置
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
    }

    public class ApplicationUser : IdentityUser
    {
        public bool IsBuiltIn { get; set; }
    }

    public class ApplicationRole : IdentityRole
    {
        public bool IsBuiltIn { get; set; }
    }
}
