using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using weatherBot.Data.Models;

namespace weatherBot.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Settings>(entity =>
            {
                entity.HasData(new Settings
                {
                    Id = -1,
                    Token = "5071818370:AAEhpKGAvgAOEJrHWxIqBcQk4AKlN0MQBcQ"
                });
            });

            builder.Entity<Region>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.HasData(new Region[]
                {
                    new Region { Id = -1, Name = "Київ"},
                    new Region { Id = -2, Name = "Вінниця"},
                    new Region{Id = -3, Name ="Луцьк"},
                    new Region { Id = -4, Name = "Дніпропетровськ"},
                    new Region { Id = -5, Name = "Житомир"},
                    new Region{Id = -6, Name ="Закарпаття"},
                    new Region { Id = -7, Name = "Запоріжжя"},
                    new Region { Id = -8, Name = "Івано-Франківськ"},
                    new Region{Id = -9, Name ="Кіровоград"},
                    new Region { Id = -10, Name = "Луганськ"},
                    new Region { Id = -11, Name = "Львів"},
                    new Region{Id = -12, Name ="Миколаїв"},
                    new Region { Id = -13, Name = "Одесса"},
                    new Region { Id = -14, Name = "Полтава"},
                    new Region{Id = -15, Name ="Рівне"},
                    new Region { Id = -16, Name = "Суми"},
                    new Region { Id = -17, Name = "Тернопіль"},
                    new Region{Id = -18, Name ="Херсон"},
                    new Region { Id = -19, Name = "Хмельницький"},
                    new Region{Id = -20, Name ="Черкаси"},
                    new Region { Id = -21, Name = "Чернівці"},
                    new Region { Id = -22, Name = "Чернігів"}
                });
            });
        }

        public DbSet<Settings> Settings { get; set; }
        public DbSet<Weather> Weathers { get; set; }
        public DbSet<Region> Regions { get; set; }
    }
}