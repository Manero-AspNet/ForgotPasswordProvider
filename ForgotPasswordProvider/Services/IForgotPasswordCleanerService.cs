
namespace ForgotPasswordProvider.Services
{
    public interface IForgotPasswordCleanerService
    {
        Task RemoveExpiredRecordsAsync();
    }
}