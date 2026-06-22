using FluentValidation;
using Identity.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Identity.Infrastructure.Services
{
    public class ValidationService: IValidationService
    {
        private readonly IServiceProvider _serviceProvider;
        public ValidationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task ValidateAndThrowAsync<T>(T request)
        {
            var validator = _serviceProvider.GetService<IValidator<T>>();
            if (validator != null)
                await validator.ValidateAndThrowAsync(request);
        }
    }
}
