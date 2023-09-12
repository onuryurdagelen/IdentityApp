using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace Api.Attributes
{
    public class ValidationErrorResponseAttribute:ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if(!context.ModelState.IsValid)
            {
                var errors = context.ModelState.Values
                    .SelectMany(p => p.Errors)
                    .Select(p => p.ErrorMessage)
                    .ToList();

                context.Result = new BadRequestObjectResult(new
                {
                    Errors = errors
                });
            }
        }
    }
}
