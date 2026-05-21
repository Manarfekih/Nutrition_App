using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Recipe_Nutrition_App.Models
{
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required, Range(0, double.MaxValue)]
        public double Calories { get; set; }

        [Required, StringLength(20)]
        public string Unit { get; set; } = "";

        public string? ImagePath { get; set; }

        public ICollection<RecipeIngredients> RecipeIngredients { get; set; } = new List<RecipeIngredients>();
    }
}
