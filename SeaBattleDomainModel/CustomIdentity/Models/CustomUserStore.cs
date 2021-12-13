using Microsoft.AspNetCore.Identity;
using OrmRepositoryUnitOfWork.Interfaces;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CustomIdentity.Models
{
    public class CustomUserStore : IUserStore<CustomIdentityUser>, IUserPasswordStore<CustomIdentityUser>, IUserEmailStore<CustomIdentityUser>
    {
        private IUnitOfWork unitOfWork;

        public CustomUserStore(IUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        public async Task<IdentityResult> CreateAsync(CustomIdentityUser user, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() =>
            {
                this.unitOfWork.Create(user);
                return IdentityResult.Success;
            });
        }

        public async Task<IdentityResult> DeleteAsync(CustomIdentityUser user, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() =>
            {
                this.unitOfWork.Delete(user);
                return IdentityResult.Success;
            });
        }

        public void Dispose()
        {
            this.unitOfWork.Dispose();
        }

        public async Task<CustomIdentityUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() =>
            {
                return this.unitOfWork.ReadItems<CustomIdentityUser>("Email", normalizedEmail).FirstOrDefault();
            });
        }

        public async Task<CustomIdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() =>
            {
                int id;
                int.TryParse(userId, out id);
                return this.unitOfWork.ReadItem<CustomIdentityUser>(id);
            });
        }

        public async Task<CustomIdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() =>
            {
                return this.unitOfWork.ReadItems<CustomIdentityUser>("UserName", normalizedUserName).FirstOrDefault();
            });
        }

        public async Task<string> GetEmailAsync(CustomIdentityUser user, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() => user.Email);
        }

        public async Task<bool> GetEmailConfirmedAsync(CustomIdentityUser user, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() => user.EmailConfirmed);
        }

        public async Task<string> GetNormalizedEmailAsync(CustomIdentityUser user, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() => user.NormalizedEmail);
        }

        public async Task<string> GetNormalizedUserNameAsync(CustomIdentityUser user, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() => user.NormalizedUserName);
        }

        public async Task<string> GetPasswordHashAsync(CustomIdentityUser user, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() => user.PasswordHash);
        }

        public async Task<string> GetUserIdAsync(CustomIdentityUser user, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() => user.Id.ToString());
        }

        public async Task<string> GetUserNameAsync(CustomIdentityUser user, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() => user.UserName);
        }

        public async Task<bool> HasPasswordAsync(CustomIdentityUser user, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() => user.PasswordHash != null);
        }

        public async Task SetEmailAsync(CustomIdentityUser user, string email, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(() =>
            {
                user.Email = email;
            });
        }

        public async Task SetEmailConfirmedAsync(CustomIdentityUser user, bool confirmed, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(() =>
            {
                user.EmailConfirmed = confirmed;
            });
        }

        public async Task SetNormalizedEmailAsync(CustomIdentityUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(() =>
            {
                user.NormalizedEmail = normalizedEmail;
            });
        }

        public async Task SetNormalizedUserNameAsync(CustomIdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(() =>
            {
                user.NormalizedUserName = normalizedName;
            });
        }

        public async Task SetPasswordHashAsync(CustomIdentityUser user, string passwordHash, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(() => user.PasswordHash = passwordHash);
        }

        public async Task SetUserNameAsync(CustomIdentityUser user, string userName, CancellationToken cancellationToken)
        {
            await Task.Factory.StartNew(() =>
            {
                user.UserName = userName;
            });
        }

        public async Task<IdentityResult> UpdateAsync(CustomIdentityUser user, CancellationToken cancellationToken)
        {
            return await Task.Factory.StartNew(() =>
            {
                this.unitOfWork.Update(user);
                return IdentityResult.Success;
            });
        }
    }
}