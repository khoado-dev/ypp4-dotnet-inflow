using Inflow.Models;
namespace Inflow.Repositories.AccountRepo
{
    public interface IAccountRepository
    {
        Task<Account?> GetByEmailAsync(string email);
        Task<Account?> GetByPhoneAsync(string phone);
        Task<Account?> GetByResetCodeAsync(string email, string code);
        Task CreateAsync(Account account);
        Task UpdateAsync(Account account);
    }
}
