using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Core.Specification
{
    /// <summary>
    /// Базова реалізація паттерну Specification для побудови складних запитів до бази даних
    /// Паттерн Specification дозволяє інкапсулювати бізнес-логіку фільтрації в окремі класи
    /// та комбінувати їх для створення складних умов запитів
    /// </summary>
    /// <typeparam name="T">Тип сутності, для якої створюється специфікація</typeparam>
    public class BaseSpecifcation<T> : ISpecification<T>
    {
        /// <summary>
        /// Конструктор за замовчуванням - створює специфікацію без умов фільтрації
        /// Використовується коли потрібно отримати всі записи з можливістю додавання Include та OrderBy
        /// </summary>
        public BaseSpecifcation()
        {
        }

        /// <summary>
        /// Конструктор з критерієм фільтрації
        /// Використовується для створення специфікації з конкретною умовою WHERE
        /// </summary>
        /// <param name="criteria">Лямбда-вираз для фільтрації даних (наприклад: x => x.IsActive == true)</param>
        public BaseSpecifcation(Expression<Func<T, bool>> criteria)
        {
            Criteria = criteria;
        }

        /// <summary>
        /// Критерій фільтрації - основна умова WHERE для запиту
        /// Expression дозволяє передавати лямбда-вирази, які Entity Framework може перекласти в SQL
        /// Приклад: x => x.Price > 100 && x.Category == "Electronics"
        /// </summary>
        public Expression<Func<T, bool>> Criteria { get; }

        /// <summary>
        /// Список навігаційних властивостей для Eager Loading (Include в Entity Framework)
        /// Дозволяє завантажувати пов'язані сутності в одному запиті для уникнення N+1 проблеми
        /// Приклад: включення Category і Reviews для Product сутності
        /// </summary>
        public List<Expression<Func<T, object>>> Includes { get; } = new List<Expression<Func<T, object>>>();

        /// <summary>
        /// Вираз для сортування за зростанням (ORDER BY ASC)
        /// Дозволяє вказати властивість, за якою потрібно сортувати результати
        /// Приклад: x => x.Name (сортування за назвою по алфавіту)
        /// </summary>
        public Expression<Func<T, object>> OrderBy { get; private set; }

        /// <summary>
        /// Вираз для сортування за спаданням (ORDER BY DESC)
        /// Використовується для сортування у зворотному порядку
        /// Приклад: x => x.CreatedDate (найновіші записи спочатку)
        /// </summary>
        public Expression<Func<T, object>> OrderByDescending { get; private set; }

        /// <summary>
        /// Додає навігаційну властивість до списку Include для Eager Loading
        /// Protected метод - доступний тільки в похідних класах специфікацій
        /// Використовується в конструкторах конкретних специфікацій
        /// </summary>
        /// <param name="includeExpression">Вираз, що вказує на навігаційну властивість для Include</param>
        /// <example>
        /// AddInclude(x => x.Category); // Завантажить Category разом з основною сутністю
        /// AddInclude(x => x.Reviews);  // Завантажить Reviews разом з основною сутністю
        /// </example>
        protected void AddInclude(Expression<Func<T, object>> includeExpression)
        {
            Includes.Add(includeExpression);
        }

        /// <summary>
        /// Встановлює сортування за зростанням
        /// Protected метод - використовується в похідних класах для налаштування сортування
        /// Якщо викликається після AddOrderByDescending, то перезаписує попереднє налаштування
        /// </summary>
        /// <param name="orderByExpression">Вираз, що вказує властивість для сортування</param>
        /// <example>
        /// AddOrderBy(x => x.Price);     // Сортування за ціною від найдешевших
        /// AddOrderBy(x => x.Name);      // Сортування за назвою по алфавіту
        /// </example>
        protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        {
            OrderBy = orderByExpression;
        }

        /// <summary>
        /// Встановлює сортування за спаданням
        /// Protected метод - використовується в похідних класах для налаштування зворотного сортування
        /// Якщо викликається після AddOrderBy, то перезаписує попереднє налаштування
        /// </summary>
        /// <param name="orderByDescExpression">Вираз, що вказує властивість для зворотного сортування</param>
        /// <example>
        /// AddOrderByDescending(x => x.CreatedDate); // Найновіші записи спочатку
        /// AddOrderByDescending(x => x.Price);       // Від найдорожчих до найдешевших
        /// </example>
        protected void AddOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        {
            OrderByDescending = orderByDescExpression;
        }
    }
}