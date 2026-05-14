using Microsoft.EntityFrameworkCore;
using Recipe_Nutrition_App.Data;
using Recipe_Nutrition_App.Models;

namespace Recipe_Nutrition_App.Services
{
    public class RecipeService
    {
        private readonly AppDbContext _context;

        public RecipeService(AppDbContext context)
        {
            _context = context;
        }

        //  READ 

        /// <summary>Returns all recipes with their ingredients eagerly loaded.</summary>
        public async Task<List<Recipe>> GetAllRecipesAsync()
        {
            return await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .AsNoTracking()
                .ToListAsync();
        }

        /// Returns a single recipe by id, or null if not found.
        public async Task<Recipe?> GetRecipeByIdAsync(int id)
        {
            return await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        //  SEARCH / FILTER 

        /// Filters recipes by name/ingredient keyword, category, and/or cuisine.
        public async Task<List<Recipe>> SearchRecipesAsync(
            string? searchTerm,
            string? category,
            string? cuisine)
        {
            var query = _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(r =>
                    r.Name.ToLower().Contains(term) ||
                    r.RecipeIngredients.Any(ri =>
                        ri.Ingredient.Name.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(r => r.Category == category);

            if (!string.IsNullOrWhiteSpace(cuisine))
                query = query.Where(r => r.Cuisine == cuisine);

            return await query.AsNoTracking().ToListAsync();
        }

        //  CREATE 

        /// <summary>Adds a new recipe (with its RecipeIngredient rows) to the DB.</summary>
        public async Task AddRecipeAsync(Recipe recipe)
        {
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
        }

        //  UPDATE 

        /// Updates scalar fields and replaces the ingredient list atomically.
        public async Task UpdateRecipeAsync(Recipe updated)
        {
            var existing = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .FirstOrDefaultAsync(r => r.Id == updated.Id);

            if (existing is null) return;

            // Scalar fields
            existing.Name      = updated.Name;
            existing.Servings  = updated.Servings;
            existing.Category  = updated.Category;
            existing.Cuisine   = updated.Cuisine;
            existing.ImagePath = updated.ImagePath;
            existing.VideoLink = updated.VideoLink;

            // Replace join-table rows
            _context.RecipeIngredients.RemoveRange(existing.RecipeIngredients);
            foreach (var ri in updated.RecipeIngredients)
            {
                ri.RecipeId = existing.Id;   // make sure FK is set
                existing.RecipeIngredients.Add(ri);
            }

            await _context.SaveChangesAsync();
        }

        //  DELETE 

        public async Task DeleteRecipeAsync(int id)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe is null) return;

            _context.RecipeIngredients.RemoveRange(recipe.RecipeIngredients);
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
        }

        //  CALORIE CALCULATIONS 

        /// <summary>Sum of (Quantity × Ingredient.Calories) for every ingredient.</summary>
        public double CalculateTotalCalories(Recipe recipe)
        {
            if (recipe.RecipeIngredients is null || !recipe.RecipeIngredients.Any())
                return 0;

            return recipe.RecipeIngredients
                .Sum(ri => ri.Quantity * (ri.Ingredient?.Calories ?? 0));
        }

        /// Total calories divided by the number of servings.
        public double CalculateCaloriesPerServing(Recipe recipe)
        {
            if (recipe.Servings <= 0) return 0;
            return Math.Round(CalculateTotalCalories(recipe) / recipe.Servings, 1);
        }

        //  ANALYSIS HELPERS (used by Analytics page) 

        /// Returns a dictionary: Category → recipe count.
        public async Task<Dictionary<string, int>> GetRecipeCountByCategoryAsync()
        {
            return await _context.Recipes
                .GroupBy(r => r.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category ?? "Unknown", x => x.Count);
        }

        public async Task<List<CuisineStats>> GetStatsByCuisineAsync()
        {
            var recipes = await GetAllRecipesAsync();

            return recipes
                .GroupBy(r => r.Cuisine ?? "Unknown")
                .Select(g => new CuisineStats
                {
                    Cuisine        = g.Key,
                    RecipeCount    = g.Count(),
                    AverageCalories = Math.Round(
                        g.Average(r => CalculateTotalCalories(r)), 1)
                })
                .OrderByDescending(x => x.RecipeCount)
                .ToList();
        }

        /// Returns top N recipes sorted by total calories descending.
        public async Task<List<(string Name, double TotalCalories, double PerServing)>> GetTopCalorieRecipesAsync(int top = 5)
        {
            var recipes = await GetAllRecipesAsync();
            return recipes
                .Select(r => (r.Name, TotalCalories: CalculateTotalCalories(r), PerServing: CalculateCaloriesPerServing(r)))
                .OrderByDescending(x => x.TotalCalories)
                .Take(top)
                .ToList();
        }

        /// Returns calorie-range buckets for all recipes.
        public async Task<Dictionary<string, int>> GetCalorieRangeDistributionAsync()
        {
            var recipes = await GetAllRecipesAsync();
            var buckets = new Dictionary<string, int>
            {
                { "< 200 kcal",   0 },
                { "200–500 kcal", 0 },
                { "500–800 kcal", 0 },
                { "800–1200 kcal",0 },
                { "> 1200 kcal",  0 }
            };

            foreach (var r in recipes)
            {
                var cal = CalculateTotalCalories(r);
                if      (cal < 200)  buckets["< 200 kcal"]++;
                else if (cal < 500)  buckets["200–500 kcal"]++;
                else if (cal < 800)  buckets["500–800 kcal"]++;
                else if (cal < 1200) buckets["800–1200 kcal"]++;
                else                 buckets["> 1200 kcal"]++;
            }

            return buckets;
        }
    }

    //  DTO returned by GetStatsByCuisineAsync 
    public class CuisineStats
    {
        public string  Cuisine         { get; set; } = string.Empty;
        public int     RecipeCount     { get; set; }
        public double  AverageCalories { get; set; }
    }
}