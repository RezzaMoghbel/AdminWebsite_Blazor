using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mars.Admin.Data;
// Identity-only DbContext. I keep my app tables out of EF because I'll use Dapper for those.
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    // No extra DbSets here — this DbContext is dedicated to ASP.NET Core Identity.
}
