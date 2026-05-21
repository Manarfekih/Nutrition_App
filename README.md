#  NutriRecipe

A web app for managing recipes and tracking calories  built with Blazor and Entity Framework Core.

---

## What is this?

NutriRecipe lets you build a library of recipes using an ingredient database where each ingredient has a caloric value. You can organize recipes by meal type and cuisine, and get a clear picture of the nutritional content of what you're cooking.

---

## What can you do with it?

**Manage ingredients**
Add any ingredient with its caloric value per unit (grams, ml, pieces, etc.). You can search, edit, or delete them anytime( By the Admin ).

**Manage recipes**
Create recipes by picking ingredients and setting quantities. The app automatically calculates the total calories and calories per serving. Each recipe has a category (Breakfast, Lunch, Dinner, Dessert) and a cuisine type (Tunisian, French, Italian, Mediterranean...).

**Explore the dashboard**
A visual dashboard shows you things like which categories have the most calories, how recipes are spread across cuisines, and which recipes are the heaviest calorie-wise.

---

## Tech stack

- **Blazor** — for the UI
- **Entity Framework Core** — Code-First with migrations
- **SQL Server** — relational database
- **Bootstrap 5** — for styling

The project is structured in three layers: UI, Services, and Data — each with a clear responsibility.

---

## Getting started

```bash
# Clone the project
git clone https://github.com/your-team/nutrirecipe.git
cd nutrirecipe

# Restore packages
dotnet restore

# Run migrations
dotnet ef database update --project NutriRecipe.Data --startup-project NutriRecipe.Web

# Start the app
dotnet run --project NutriRecipe.Web
```


---

## Data model (quick overview)

There are three main entities:

- **Ingredient** — name, unit, calories per unit
- **Recipe** — name, servings, category, cuisine, description
- **RecipeIngredient** — links a recipe to its ingredients with quantities

Calories are computed on the fly: `quantity × calories per unit`, summed across all ingredients, then divided by servings.

---

## Dashboard indicators

- Total calories per recipe
- Calories per serving
- Recipe count by category (bar chart)
- Recipe count by cuisine type (pie chart)
- Top 5 highest-calorie recipes
- Average calories per serving per category

---

## Our extension idea — Weekly Meal Planner 

Beyond the base project, we added a weekly meal planning feature. You assign recipes to specific days and meal slots, and the app tracks your total calorie intake for the week, comparing it against a daily target (default: 2000 kcal).

It made sense to add this because the data was already there — recipes with categories and calorie values. The planner just puts it to practical use and adds real decision-making value: *am I eating balanced this week?*

---

## Creativity features : 
Classify recipes based on their calorie range (low, medium, or high).

## Team

Name  : 
|Manar El Fakih Romdahne
Chayma Akkachi  
Nada Azzouz 


---
