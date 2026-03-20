using System.ComponentModel.DataAnnotations;

namespace Recipe_Nutrition_App.Models
{
    public class RecipeIngredients
    {
        // fk Recipe
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        // fk Ingredient
        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; } 

        [Required, Range(0, double.MaxValue)]
        public double Quantity { get; set; } 
    }
}