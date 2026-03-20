using Microsoft.EntityFrameworkCore;
using Recipe_Nutrition_App.Models;

namespace Recipe_Nutrition_App.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        //tab ingredient
        public DbSet<Ingredient> Ingredients { get; set; }

        //tab recipe
        public DbSet<Recipe> Recipes { get; set; }

        //tab ingredientss of recipe
        public DbSet<RecipeIngredients> RecipeIngredients { get; set; }

        
         protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // COMPOSITE PRIMARY KEY for (many to many tabl)
            modelBuilder.Entity<RecipeIngredients>()
                .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

            // many recipes
            modelBuilder.Entity<RecipeIngredients>()
                .HasOne(ri => ri.Recipe)
                .WithMany(r => r.RecipeIngredients)
                .HasForeignKey(ri => ri.RecipeId);

            //many ingredients
            modelBuilder.Entity<RecipeIngredients>()
                .HasOne(ri => ri.Ingredient)
                .WithMany(i => i.RecipeIngredients)
                .HasForeignKey(ri => ri.IngredientId);
        }



    }
}