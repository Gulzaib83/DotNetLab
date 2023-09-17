using ConnectionManagement;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbOperations_ADO
{
    public class IdentityRoleRepository_ADO : IRoleStore<IdentityRole>
    {
        private readonly ConnectionManager _connectionManager;

        public IdentityRoleRepository_ADO(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        }

        public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            using (var connection = _connectionManager.GetConnection())
            {
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO Roles (Id, Name, NormalizedName) VALUES (@Id, @Name, @NormalizedName)";

                    command.Parameters.Add(new SqlParameter("@Id", role.Id));
                    command.Parameters.Add(new SqlParameter("@Name", role.Name));
                    command.Parameters.Add(new SqlParameter("@NormalizedName", role.NormalizedName));

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return IdentityResult.Success;
                    }
                }

                
            }

            return IdentityResult.Failed(null);
        }

        public Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
