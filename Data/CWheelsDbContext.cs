using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Restfull_API_Project.Models;

namespace Restfull_API_Project.Data
{
    public class CWheelsDbContext : DbContext
    {
        public CWheelsDbContext(DbContextOptions<CWheelsDbContext> options) : base(options)
        {

        }

        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Image> Images { get; set; }
    }
}
