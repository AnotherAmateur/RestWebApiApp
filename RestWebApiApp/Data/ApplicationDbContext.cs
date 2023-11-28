using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RestWebApiApp.Models;

namespace RestWebApiApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
    }
}
