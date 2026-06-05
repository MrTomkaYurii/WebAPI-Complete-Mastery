using InMemoryCaching.Models;
using InMemoryCaching.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace InMemoryCaching.Services;

public class ProductService(AppDbContext context, 
    IMemoryCache cache, 
    ILogger<ProductService> logger) : IProductService
{
    public async Task Add(ProductCreationDto request)
    {
        var product = new Product(request.Name, request.Description, request.Price);
        await context.Products.AddAsync(product);
        await context.SaveChangesAsync();

        // invalidate cache for products, as new product is added
        var cacheKey = "products";
        logger.LogInformation("invalidating cache for key: {CacheKey} from cache.", cacheKey);
        cache.Remove(cacheKey);
    }

    public async Task<Product> Get(Guid id)
    {
        var cacheKey = $"product:{id}";
        logger.LogInformation("fetching data for key: {CacheKey} from cache.", cacheKey);
        if (!cache.TryGetValue(cacheKey, out Product? product))
        {
            logger.LogInformation("cache miss. fetching data for key: {CacheKey} from database.", cacheKey);
            product = await context.Products.FindAsync(id);
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(50))
                .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                .SetPriority(CacheItemPriority.Normal);
            logger.LogInformation("setting data for key: {CacheKey} to cache.", cacheKey);
            cache.Set(cacheKey, product, cacheOptions);
        }
        else
        {
            logger.LogInformation("cache hit for key: {CacheKey}.", cacheKey);
        }
        return product;
    }

    public async Task<List<Product>> GetAll()
    {
        var cacheKey = "products";
        logger.LogInformation("fetching data for key: {CacheKey} from cache.", cacheKey);
        if (!cache.TryGetValue(cacheKey, out List<Product>? products))
        {
            logger.LogInformation("cache miss. fetching data for key: {CacheKey} from database.", cacheKey);
            products = await context.Products.ToListAsync();
            // Створення налаштувань для запису в кеші
            var cacheOptions = new MemoryCacheEntryOptions()
                // Абсолютний час життя - 20 хвилин від моменту додавання
                // Після цього запис видалиться, навіть якщо до нього звертаються
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(20))

                // Ковзаючий час життя - 2 хвилини
                // Якщо протягом 2 хвилин ніхто не звертався до запису - він видалиться
                // При кожному зверненні таймер скидається на 2 хвилини
                .SetSlidingExpiration(TimeSpan.FromMinutes(2))

                // Пріоритет - ніколи не видаляти автоматично
                // Навіть при нестачі пам'яті цей запис залишиться в кеші
                // Використовувати обережно! Може призвести до переповнення пам'яті
                .SetPriority(CacheItemPriority.NeverRemove)

                // Розмір запису - 2048 умовних одиниць
                // Використовується для контролю загального розміру кешу
                // Якщо не встановлений SizeLimit, цей параметр ігнорується
                .SetSize(2048);
            logger.LogInformation("setting data for key: {CacheKey} to cache.", cacheKey);
            cache.Set(cacheKey, products, cacheOptions);
        }
        else
        {
            logger.LogInformation("cache hit for key: {CacheKey}.", cacheKey);
        }
        return products;
    }
}