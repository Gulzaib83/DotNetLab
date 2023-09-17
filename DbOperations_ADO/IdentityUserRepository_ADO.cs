using ConnectionManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbOperations_ADO
{
    public class IdentityUserRepository_ADO : IUserStore<IdentityUser>
    {
        private readonly ConnectionManager _connectionManager;

        public IdentityUserRepository_ADO(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        }
        public async Task<IdentityResult> CreateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            using (var connection = _connectionManager.GetConnection())
            {
                //await connection.Open(cancellationToken);
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Users (Id, UserName, NormalizedUserName, Email, NormalizedEmail, " +
                    "PasswordHash) VALUES (@Id, @UserName, @NormalizedUserName, @Email, " +
                    "@NormalizedEmail, @PasswordHash)";
                 
                    command.Parameters.Add(new SqlParameter("@Id", user.Id));
                    command.Parameters.Add(new SqlParameter("@UserName", user.UserName));
                    command.Parameters.Add(new SqlParameter("@NormalizedUserName", user.NormalizedUserName));
                    command.Parameters.Add(new SqlParameter("@Email", user.Email));
                    command.Parameters.Add(new SqlParameter("@NormalizedEmail", user.NormalizedEmail));
                    command.Parameters.Add(new SqlParameter("@PasswordHash", user.PasswordHash));

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return IdentityResult.Success;
                    }
                }
            }

            return IdentityResult.Failed(null);
        }

        public Task<IdentityResult> DeleteAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task<IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IdentityUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            using (var connection = _connectionManager.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT NORMALIZEDUSERNAME FROM ASPNETUSERS WHERE NormalizedUserName = @NormalizedUserName";
                    command.Parameters.Add(new SqlParameter("@NormalizedUserName", normalizedUserName.ToUpper()));

                    using (var reader = command.ExecuteReader())
                    {
                        var user = new IdentityUser();

                        while (reader.Read())
                        {
                            user.Id = reader[0] != null ? reader[0].ToString() : string.Empty;
                        }

                        return user;
                    }
                }
            }
            return null;
        }

        public Task<string> GetNormalizedUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserIdAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetUserNameAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(IdentityUser user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetUserNameAsync(IdentityUser user, string userName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(IdentityUser user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
