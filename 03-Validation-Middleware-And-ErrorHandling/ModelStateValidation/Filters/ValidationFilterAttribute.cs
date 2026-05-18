using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace AspNet_ModelStateValidation.Filters
{
    public class ValidationFilterAttribute : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new UnprocessableEntityObjectResult(context.ModelState);
            }
        }

        public void OnActionExecuted(ActionExecutedContext context) { }
    }
}
