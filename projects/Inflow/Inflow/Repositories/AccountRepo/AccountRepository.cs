

using Inflow.Models;
using Microsoft.EntityFrameworkCore;

namespace Inflow.Repositories.AccountRepo
{
    public class AccountRepository : IAccountRepository
    {
        private readonly AppDbContext _context;
        public AccountRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Account?> GetByEmailAsync(string email)
            => await _context.Account.FirstOrDefaultAsync(a => a.Email == email);

        public async Task<Account?> GetByPhoneAsync(string phone)
            => await _context.Account.FirstOrDefaultAsync(a => a.Phone == phone);

        public async Task<Account?> GetByResetCodeAsync(string email, string code)
            => await _context.Account.FirstOrDefaultAsync(a => a.Email == email && a.ResetCode == code);

        public async Task<int> CreateAsync(Account account)
        {
            _context.Account.Add(account);
            await _context.SaveChangesAsync();
            return account.UserId;
        }

        public async Task UpdateAsync(Account account)
        {
            _context.Account.Update(account);
            await _context.SaveChangesAsync();
        }
    }
}
