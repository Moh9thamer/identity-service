namespace Identity.Application.Interfaces
{
    public interface IValidationService
    {
        Task ValidateAndThrowAsync<T>(T request);
    }
}
