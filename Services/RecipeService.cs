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

        public async Task<List<Recipe>> GetAllRecipesAsync()
        {
            return await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Recipe?> GetRecipeByIdAsync(int id)
        {
            return await _context.Recipes
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);
        }


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

        public async Task AddRecipeAsync(Recipe recipe)
        {
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
        }

        //  UPDATE 

        public async Task UpdateRecipeAsync(Recipe updated)
        {
            var existing = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .FirstOrDefaultAsync(r => r.Id == updated.Id);

            if (existing is null) return;

            existing.Name      = updated.Name;
            existing.Servings  = updated.Servings;
            existing.Category  = updated.Category;
            existing.Cuisine   = updated.Cuisine;
            existing.ImagePath = updated.ImagePath;
            existing.VideoLink = updated.VideoLink;

            _context.RecipeIngredients.RemoveRange(existing.RecipeIngredients);
            foreach (var ri in updated.RecipeIngredients)
            {
                ri.RecipeId = existing.Id;   
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

       
        public double CalculateTotalCalories(Recipe recipe)
        {
            if (recipe.RecipeIngredients is null || !recipe.RecipeIngredients.Any())
                return 0;

            return recipe.RecipeIngredients
                .Sum(ri => ri.Quantity * (ri.Ingredient?.Calories ?? 0));
        }

        public double CalculateCaloriesPerServing(Recipe recipe)
        {
            if (recipe.Servings <= 0) return 0;
            return Math.Round(CalculateTotalCalories(recipe) / recipe.Servings, 1);
        }

        public (string Text, string BootstrapColor) GetHealthyScore(Recipe recipe)
        {
            var perServing = CalculateCaloriesPerServing(recipe);

            if (perServing < 400)
                return ("Healthy", "success");     

            if (perServing < 700)
                return ("Moderate", "warning");    

            return ("High Calorie", "danger");     
        }


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

        public async Task<List<(string Name, double TotalCalories, double PerServing)>> GetTopCalorieRecipesAsync(int top = 5)
        {
            var recipes = await GetAllRecipesAsync();
            return recipes
                .Select(r => (r.Name, TotalCalories: CalculateTotalCalories(r), PerServing: CalculateCaloriesPerServing(r)))
                .OrderByDescending(x => x.TotalCalories)
                .Take(top)
                .ToList();
        }

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

        public async Task<List<CategoryStat>> GetCategoryStatsAsync()
        {
            return await _context.Recipes
                .GroupBy(r => r.Category)
                .Select(g => new CategoryStat
                {
                    CategoryName = g.Key ?? "Unknown",
                    Count        = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
        }
    }

    public class CuisineStats
    {
        public string  Cuisine         { get; set; } = string.Empty;
        public int     RecipeCount     { get; set; }
        public double  AverageCalories { get; set; }
    }

    public class CategoryStat
    {
        public string CategoryName { get; set; } = string.Empty;
        public int    Count        { get; set; }
    }
}
