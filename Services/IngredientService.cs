using Microsoft.EntityFrameworkCore;
using Recipe_Nutrition_App.Data;
using Recipe_Nutrition_App.Models;

namespace Recipe_Nutrition_App.Services
{
    public class IngredientService
    {
        private readonly AppDbContext _context ;

        public IngredientService(AppDbContext context)
        {
            _context = context;
        }

        //all ing
        public async Task<List<Ingredient>> GetIngredientsAsync()
        {
            return await _context.Ingredients.ToListAsync();
        }

        //add an ing
        public async Task addIngredientAsync (Ingredient ingredient)
        {
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();
        }


        //update ing
        public async Task UpdateIngredientAsync(Ingredient ingredient)
        {
            _context.Ingredients.Update(ingredient);
            await _context.SaveChangesAsync();
        }


        //get ing by id
        public async Task<Ingredient?> GetIngByIdAsync(int id)
        {
            return await _context.Ingredients.FindAsync(id);
        }


        //delete ing

        public async Task DeleteIngredientAsync(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);

            if(ingredient != null)
            {
                _context.Ingredients.Remove(ingredient);
                await _context.SaveChangesAsync();
            }
        }





    }
}
