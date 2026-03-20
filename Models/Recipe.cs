using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Recipe_Nutrition_App.Models
{
    public class Recipe
    {
        [Key] 
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } 

        [Required]
        public int Servings { get; set; } 

        [Required]
        public string Category { get; set; }

        [Required]
        public string Cuisine { get; set; } 

        public string? ImagePath { get; set; } 

        public string? VideoLink { get; set; } 

        public ICollection<RecipeIngredients> RecipeIngredients { get; set; } = new List<RecipeIngredients>();
    }
}