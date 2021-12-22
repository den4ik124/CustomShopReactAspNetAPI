using Microsoft.EntityFrameworkCore;
using SeaBattleDomainModel.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeaBattlePersistence
{
    public class SeaBattleDbContext : DbContext
    {
        public SeaBattleDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Ship> Ships { get; set; }
        public DbSet<BattleField> BattleFields { get; set; }
    }
}