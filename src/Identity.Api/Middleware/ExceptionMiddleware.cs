using FluentValidation;
using Identity.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }
        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var (statusCode, title, errors) = ex switch
            {
                ValidationException ve => (400, "Validation failed", ve.Errors.Select(e => e.ErrorMessage)),
                ConflictException ce => (409, ce.Message, Enumerable.Empty<string>()),
                NotFoundException nfe => (404, nfe.Message, Enumerable.Empty<string>()),
                UnauthorizedAccessException => (401, "Unauthorized", Enumerable.Empty<string>()),
                _ => (500, "An unexpected error occurred", Enumerable.Empty<string>())
            };

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var problem = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Extensions = { ["errors"] = errors }
            };

            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
