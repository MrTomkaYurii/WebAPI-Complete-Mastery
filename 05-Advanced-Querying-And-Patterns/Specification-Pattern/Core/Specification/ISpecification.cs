using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Core.Specification
{
    public interface ISpecification<T>
    {
        // Умова (фільтр), за якою будуть відбиратись сутності.
        // Наприклад: x => x.IsActive == true
        Expression<Func<T, bool>> Criteria { get; }

        // Колекція виразів для включення пов’язаних сутностей (Include).
        // Використовується для жадного завантаження (Eager Loading).
        // Наприклад: x => x.Orders
        List<Expression<Func<T, object>>> Includes { get; }

        // Вираз для сортування за зростанням.
        // Наприклад: x => x.Name
        Expression<Func<T, object>> OrderBy { get; }

        // Вираз для сортування за спаданням.
        // Наприклад: x => x.CreatedDate
        Expression<Func<T, object>> OrderByDescending { get; }

    }
}