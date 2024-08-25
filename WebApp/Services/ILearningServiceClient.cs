namespace WebApp.Services;

public interface ILearningServiceClient
{
    Task<string> GetAuthorizedAsync();
}