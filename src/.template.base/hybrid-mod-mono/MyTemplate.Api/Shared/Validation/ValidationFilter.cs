using FluentValidation;

namespace MyTemplate.Api.Shared.Validation;

public class ValidationFilter<TRequest> : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
        
        if (validator is not null)
        {
            var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
            if (request is not null)
            {
                var validationResult = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
                if (!validationResult.IsValid)
                {
                    return Microsoft.AspNetCore.Http.TypedResults.ValidationProblem(validationResult.ToDictionary());
                }
            }
        }

        return await next(context);
    }
}
