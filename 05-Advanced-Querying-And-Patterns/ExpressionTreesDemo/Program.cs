using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace ExpressionTreesDemo
{
    // ====================================================================
    // БАЗОВИЙ ПРИКЛАД EXPRESSION TREES - МАТЕМАТИЧНІ ОПЕРАЦІЇ
    // ====================================================================

    /// <summary>
    /// Клас для демонстрації базових концепцій Expression Trees на простих прикладах
    /// Показує різницю між звичайними делегатами та Expression Trees
    /// </summary>
    public class BasicExpressionTreesDemo
    {
        /// <summary>
        /// Демонстрація різниці між Func та Expression
        /// Показує чому Expression Trees можуть бути проаналізовані програмно
        /// </summary>
        public static void DemonstrateBasicConcepts()
        {
            Console.WriteLine("БАЗОВІ КОНЦЕПЦІЇ EXPRESSION TREES");
            Console.WriteLine("=====================================\n");

            // 1. ЗВИЧАЙНИЙ ДЕЛЕГАТ (Func)
            Console.WriteLine("1. Звичайний делегат Func<int, int, int>:");
            Func<int, int, int> addFunc = (a, b) => a + b;
            int result1 = addFunc(5, 3);
            Console.WriteLine($"   addFunc(5, 3) = {result1}");
            Console.WriteLine($"   Можемо тільки ВИКОНАТИ: {addFunc}");
            Console.WriteLine("    НЕ можемо аналізувати структуру коду\n");

            // 2. EXPRESSION TREE
            Console.WriteLine("2. Expression Tree:");
            Expression<Func<int, int, int>> addExpression = (a, b) => a + b;

            // Компілюємо Expression в виконуваний код
            var compiledAdd = addExpression.Compile();
            int result2 = compiledAdd(5, 3);
            Console.WriteLine($"   compiledAdd(5, 3) = {result2}");
            Console.WriteLine($"   Можемо АНАЛІЗУВАТИ: {addExpression}");
            Console.WriteLine($"   Можемо ВИКОНАТИ: {result2}");
            Console.WriteLine("    Можемо програмно розібрати структуру!\n");

            // 3. АНАЛІЗ СТРУКТУРИ EXPRESSION TREE
            Console.WriteLine("3. Аналіз структури Expression Tree:");
            AnalyzeExpressionStructure(addExpression);
            Console.WriteLine();

            // 4. СКЛАДНІШИЙ ПРИКЛАД
            Console.WriteLine("4. Складніший приклад з Expression Tree:");
            Expression<Func<int, int, bool>> complexExpression = (x, y) => (x * 2 + y) > 10;
            Console.WriteLine($"   Expression: {complexExpression}");
            Console.WriteLine("   Структура:");
            AnalyzeComplexExpression(complexExpression);

            var compiledComplex = complexExpression.Compile();
            Console.WriteLine($"   Результат для (3, 5): {compiledComplex(3, 5)}");
            Console.WriteLine($"   Пояснення: (3 * 2 + 5) = 11 > 10 = True\n");

            // 5. РУЧНА ПОБУДОВА EXPRESSION TREE
            Console.WriteLine("5. Ручна побудова Expression Tree:");
            DemonstrateManualExpressionBuilding();
        }

        /// <summary>
        /// Аналізує структуру простого Expression Tree
        /// Показує що Expression Tree - це дерево об'єктів, яке можна обходити
        /// </summary>
        private static void AnalyzeExpressionStructure(Expression<Func<int, int, int>> expression)
        {
            Console.WriteLine($"   Тип Expression: {expression.NodeType}");
            Console.WriteLine($"   Тип повернення: {expression.ReturnType}");
            Console.WriteLine($"   Параметри: {string.Join(", ", expression.Parameters.Select(p => p.Name))}");

            if (expression.Body is BinaryExpression binary)
            {
                Console.WriteLine($"   Операція: {binary.NodeType}");
                Console.WriteLine($"   Лівий операнд: {binary.Left} (тип: {binary.Left.NodeType})");
                Console.WriteLine($"   Правий операнд: {binary.Right} (тип: {binary.Right.NodeType})");
            }
        }

        /// <summary>
        /// Аналізує складніший Expression Tree
        /// Демонструє рекурсивну структуру дерева виразів
        /// </summary>
        private static void AnalyzeComplexExpression(Expression<Func<int, int, bool>> expression)
        {
            // Корінь: (x * 2 + y) > 10
            if (expression.Body is BinaryExpression rootBinary)
            {
                Console.WriteLine($"     Кореневя операція: {rootBinary.NodeType} (>)");
                Console.WriteLine($"     Ліва частина: {rootBinary.Left}");
                Console.WriteLine($"     Права частина: {rootBinary.Right} (константа)");

                // Ліва частина: (x * 2 + y)
                if (rootBinary.Left is BinaryExpression leftBinary)
                {
                    Console.WriteLine($"       Ліва операція: {leftBinary.NodeType} (+)");
                    Console.WriteLine($"       Ліва частина від +: {leftBinary.Left}");
                    Console.WriteLine($"       Права частина від +: {leftBinary.Right}");

                    // x * 2
                    if (leftBinary.Left is BinaryExpression multiplyBinary)
                    {
                        Console.WriteLine($"         Множення: {multiplyBinary.Left} * {multiplyBinary.Right}");
                    }
                }
            }
        }

        /// <summary>
        /// Демонструє ручну побудову Expression Tree за допомогою Expression API
        /// Показує як програмно створювати математичні вирази
        /// </summary>
        private static void DemonstrateManualExpressionBuilding()
        {
            // Будуємо вираз: (x + y) * 2

            // Крок 1: Створюємо параметри
            var xParam = Expression.Parameter(typeof(int), "x");
            var yParam = Expression.Parameter(typeof(int), "y");

            // Крок 2: Створюємо операцію додавання x + y
            var addition = Expression.Add(xParam, yParam);

            // Крок 3: Створюємо константу 2
            var two = Expression.Constant(2);

            // Крок 4: Створюємо операцію множення (x + y) * 2
            var multiplication = Expression.Multiply(addition, two);

            // Крок 5: Створюємо лямбда-вираз
            var lambda = Expression.Lambda<Func<int, int, int>>(multiplication, xParam, yParam);

            Console.WriteLine($"   Створений вираз: {lambda}");
            Console.WriteLine("   Це еквівалентно: (x, y) => (x + y) * 2");

            // Тестуємо
            var compiled = lambda.Compile();
            int testResult = compiled(3, 4);
            Console.WriteLine($"   Результат для (3, 4): {testResult}");
            Console.WriteLine($"   Пояснення: (3 + 4) * 2 = 14\n");

            Console.WriteLine("ЧОМУ ЦЕ ВАЖЛИВО ДЛЯ ENTITY FRAMEWORK:");
            Console.WriteLine("=====================================");
            Console.WriteLine("• Expression Tree - це ДАНІ (дерево об'єктів), а не код");
            Console.WriteLine("• EF може АНАЛІЗУВАТИ це дерево і перекладати в SQL");
            Console.WriteLine("• Звичайний Func не можна аналізувати - це скомпільований код");
            Console.WriteLine("• Expression дозволяє 'заглянути всередину' логіки фільтрації");
            Console.WriteLine("• Тому EF може оптимізувати запити на рівні бази даних\n");
        }
    }
    // ====================================================================
    // МОДЕЛІ ДАНИХ
    // ====================================================================

    /// <summary>
    /// Модель продукту для демонстрації Expression Trees
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int StockQuantity { get; set; }
    }

    // ====================================================================
    // КОНТЕКСТ БАЗИ ДАНИХ
    // ====================================================================

    /// <summary>
    /// Контекст Entity Framework для роботи з продуктами
    /// </summary>
    public class ProductContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Використовуємо SQLite для простоти демонстрації
            optionsBuilder.UseSqlite("Data Source=products.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Налаштування моделі
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
                entity.Property(p => p.Category).HasMaxLength(50);
            });
        }
    }

    // ====================================================================
    // КЛАС ДЛЯ ДЕМОНСТРАЦІЇ EXPRESSION TREES
    // ====================================================================

    /// <summary>
    /// Клас для демонстрації різних способів використання Expression Trees з EF
    /// </summary>
    public class ExpressionTreesDemo
    {
        private readonly ProductContext _context;

        public ExpressionTreesDemo(ProductContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Демонстрація простого використання Expression як предиката
        /// Expression<Func<T, bool>> - це дерево виразів, яке EF може перекласти в SQL
        /// </summary>
        public async Task<List<Product>> GetProductsBySimpleExpression()
        {
            Console.WriteLine("================================");
            Console.WriteLine("=== 1. Простий Expression Tree ===");

            // Створюємо Expression Tree для фільтрації активних продуктів
            Expression<Func<Product, bool>> activeProducts = p => p.IsActive == true;
            Console.WriteLine($"Expression: {activeProducts}");
            // Створюємо IQueryable для того, щоб отримати SQL
            var query = _context.Products.Where(activeProducts);
            // Виводимо SQL, який згенерує EF Core
            Console.WriteLine("Згенерований SQL:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(query.ToQueryString());
            Console.ResetColor();
            // Виконуємо запит і повертаємо результат
            var result = await query.ToListAsync();
            return result;
        }


        /// <summary>
        /// Демонстрація складного Expression з комбінацією умов
        /// </summary>
        public async Task<List<Product>> GetProductsByComplexExpression()
        {
            Console.WriteLine("=====================================");
            Console.WriteLine("=== 2. Складний Expression Tree ===");
            // Комбінуємо декілька умов в одному Expression
            Expression<Func<Product, bool>> complexFilter = p =>
                p.IsActive == true &&
                p.Price >= 10 &&
                p.Price <= 1000 &&
                p.StockQuantity > 0 &&
                p.Category == "Electronics";
            Console.WriteLine($"Expression: {complexFilter}");
            // Створюємо IQueryable з потрібним фільтром та сортуванням
            var query = _context.Products
                .Where(complexFilter)
                .OrderBy(p => (double)p.Price);
            // Виводимо SQL
            Console.WriteLine("Згенерований SQL:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(query.ToQueryString());
            Console.ResetColor();
            // Виконуємо запит
            var result = await query.ToListAsync();
            return result;
        }

        /// <summary>
        /// Демонстрація динамічної побудови Expression Trees
        /// Корисно коли умови фільтрації залежать від користувацького вводу
        /// </summary>
        public async Task<List<Product>> GetProductsByDynamicExpression(string category, 
            decimal? minPrice, 
            decimal? maxPrice)
        {
            Console.WriteLine("=====================================");
            Console.WriteLine("=== 3. Динамічний Expression Tree ===");

            var query = _context.Products.AsQueryable();
            Console.WriteLine("Початковий SQL:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(query.ToQueryString());
            Console.ResetColor();

            if (!string.IsNullOrEmpty(category))
            {
                Expression<Func<Product, bool>> categoryFilter = p => p.Category == category;
                query = query.Where(categoryFilter);
                Console.WriteLine($"Додано фільтр категорії: {categoryFilter}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(query.ToQueryString());
                Console.ResetColor();
            }

            if (minPrice.HasValue)
            {
                Expression<Func<Product, bool>> minPriceFilter = p => p.Price >= minPrice.Value;
                query = query.Where(minPriceFilter);
                Console.WriteLine($"Додано мінімальну ціну: {minPriceFilter}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(query.ToQueryString());
                Console.ResetColor();
            }

            if (maxPrice.HasValue)
            {
                Expression<Func<Product, bool>> maxPriceFilter = p => p.Price <= maxPrice.Value;
                query = query.Where(maxPriceFilter);
                Console.WriteLine($"Додано максимальну ціну: {maxPriceFilter}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(query.ToQueryString());
                Console.ResetColor();
            }

            query = query.OrderBy(p => (double)p.Price);
            Console.WriteLine("Після OrderBy:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(query.ToQueryString());
            Console.ResetColor();

            return await query.ToListAsync();
        }


        /// <summary>
        /// Демонстрація Expression для проекції (Select)
        /// Показує як Expression Trees використовуються не тільки для фільтрації
        /// </summary>
        public async Task<List<object>> GetProductProjections()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("=== 5. Expression для проекції (Select) ===");

            // Expression для вибору тільки потрібних полів
            Expression<Func<Product, object>> projection = p => new
            {
                p.Name,
                p.Price,
                PriceCategory = p.Price > 500 ? "Expensive" : "Affordable",
                InStock = p.StockQuantity > 0
            };

            Console.WriteLine($"Projection Expression: {projection}");

            var result = await _context.Products
                .Where(p => p.IsActive)
                .Select(projection)
                .ToListAsync();

            return result;
        }

        /// <summary>
        /// Демонстрація ручної побудови Expression Tree за допомогою Expression API
        /// Показує як Expression Trees працюють "під капотом"
        /// </summary>
        public async Task<List<Product>> GetProductsByManualExpression()
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("=== 6. Ручна побудова Expression Tree ===");
            // Створюємо параметр 'p' для Product
            var parameter = Expression.Parameter(typeof(Product), "p");
            // Створюємо доступ до властивості IsActive: p.IsActive
            var isActiveProperty = Expression.Property(parameter, nameof(Product.IsActive));
            // Створюємо константу true
            var trueConstant = Expression.Constant(true);
            // Створюємо порівняння: p.IsActive == true
            var isActiveComparison = Expression.Equal(isActiveProperty, trueConstant);
            // Створюємо доступ до властивості Price: p.Price
            var priceProperty = Expression.Property(parameter, nameof(Product.Price));
            // Створюємо константу 100m
            var priceConstant = Expression.Constant(100m);
            // Створюємо порівняння: p.Price > 100
            var priceComparison = Expression.GreaterThan(priceProperty, priceConstant);
            // Комбінуємо умови: p.IsActive == true && p.Price > 100
            var combinedExpression = Expression.AndAlso(isActiveComparison, priceComparison);
            // Створюємо лямбда-вираз
            var lambda = Expression.Lambda<Func<Product, bool>>(combinedExpression, parameter);
            Console.WriteLine($"Manually built Expression: {lambda}");
            Console.WriteLine("Це еквівалентно: p => p.IsActive == true && p.Price > 100");
            var result = await _context.Products.Where(lambda).ToListAsync();
            return result;
        }
    }

    // ====================================================================
    // ГОЛОВНИЙ КЛАС ПРОГРАМИ
    // ====================================================================

    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("Expression Trees з Entity Framework - Демонстрація");
            Console.WriteLine("======================================================\n");

            // СПОЧАТКУ - базові концепції Expression Trees
            BasicExpressionTreesDemo.DemonstrateBasicConcepts();

            Console.WriteLine("Натисніть будь-яку клавішу для продовження до EF прикладів...");
            Console.ReadKey();
            Console.Clear();

            Console.WriteLine("EXPRESSION TREES З ENTITY FRAMEWORK");
            Console.WriteLine("===================================\n");

            // Ініціалізація контексту та створення бази даних
            await using var context = new ProductContext();
            await context.Database.EnsureCreatedAsync();

            // Заповнення тестовими даними якщо база пуста
            if (!await context.Products.AnyAsync())
            {
                await SeedDatabase(context);
            }

            var demo = new ExpressionTreesDemo(context);

            try
            {
                // Демонстрація різних типів Expression Trees

                // 1. Простий Expression
                var simpleResults = await demo.GetProductsBySimpleExpression();
                Console.WriteLine($"Знайдено {simpleResults.Count} активних продуктів\n");

                // 2. Складний Expression
                var complexResults = await demo.GetProductsByComplexExpression();
                Console.WriteLine($"Знайдено {complexResults.Count} продуктів за складним фільтром\n");

                // 3. Динамічний Expression
                var dynamicResults = await demo.GetProductsByDynamicExpression("Electronics", 50m, 500m);
                Console.WriteLine($"Знайдено {dynamicResults.Count} продуктів за динамічним фільтром\n");

                // 4. Expression для проекції
                var projectionResults = await demo.GetProductProjections();
                Console.WriteLine($"Отримано {projectionResults.Count} проекцій продуктів");

                // Виводимо кілька прикладів
                foreach (var item in projectionResults.Take(3))
                {
                    Console.WriteLine($"  - {item}");
                }

                // 5. Ручна побудова Expression
                var manualResults = await demo.GetProductsByManualExpression();
                Console.WriteLine($"\nЗнайдено {manualResults.Count} продуктів за ручно побудованим Expression\n");

                Console.WriteLine("======================================================");
                Console.WriteLine("ПОЯСНЕННЯ:");
                Console.WriteLine("Expression Trees дозволяють:");
                Console.WriteLine("• Передавати код як дані");
                Console.WriteLine("• Entity Framework може аналізувати вираз і перекласти в SQL");
                Console.WriteLine("• Динамічно будувати запити в runtime");
                Console.WriteLine("• Створювати повторно використовувані фільтри");
                Console.WriteLine("• Оптимізувати запити на рівні бази даних");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка: {ex.Message}");
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для завершення...");
            Console.ReadKey();
        }

        /// <summary>
        /// Заповнення бази даних тестовими продуктами
        /// </summary>
        private static async Task SeedDatabase(ProductContext context)
        {
            var products = new List<Product>
            {
                new() { Name = "iPhone 14", Price = 999m, Category = "Electronics", IsActive = true, CreatedDate = DateTime.Now.AddDays(-10), StockQuantity = 25 },
                new() { Name = "Samsung TV", Price = 750m, Category = "Electronics", IsActive = true, CreatedDate = DateTime.Now.AddDays(-5), StockQuantity = 15 },
                new() { Name = "Office Chair", Price = 150m, Category = "Furniture", IsActive = true, CreatedDate = DateTime.Now.AddDays(-3), StockQuantity = 50 },
                new() { Name = "Laptop Dell", Price = 1200m, Category = "Electronics", IsActive = false, CreatedDate = DateTime.Now.AddDays(-15), StockQuantity = 0 },
                new() { Name = "Coffee Table", Price = 80m, Category = "Furniture", IsActive = true, CreatedDate = DateTime.Now.AddDays(-7), StockQuantity = 30 },
                new() { Name = "Headphones", Price = 45m, Category = "Electronics", IsActive = true, CreatedDate = DateTime.Now.AddDays(-2), StockQuantity = 100 },
                new() { Name = "Bookshelf", Price = 200m, Category = "Furniture", IsActive = true, CreatedDate = DateTime.Now.AddDays(-12), StockQuantity = 20 }
            };

            context.Products.AddRange(products);
            await context.SaveChangesAsync();

            Console.WriteLine("База даних заповнена тестовими даними.\n");
        }
    }
}