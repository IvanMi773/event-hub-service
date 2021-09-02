namespace EventHubService.Services.Validators
{
    public interface IValidate<in T>
    {
        bool Validate(T t);
    }
}