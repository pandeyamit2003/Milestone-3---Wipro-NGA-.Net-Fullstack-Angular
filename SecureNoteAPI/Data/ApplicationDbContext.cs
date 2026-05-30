using Microsoft.EntityFrameworkCore;
using SecureNoteAPI.Models;

namespace SecureNoteAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Note> Notes { get; set; }
    }
}