using FluentValidation;
using Identity.Application.DTOs.Auth;

namespace Identity.Application.Validators.Auth
{
    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
    {
        public ForgotPasswordValidator()
        {
            RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();
        }
    }
}
