using System.Data;
using ExternalEntities.Misc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace ConnectionManagement
{
    public class ConnectionManager //: IDisposable
    {
        private readonly DBSettings _dbSettings;
        private IDbConnection connection;
        private bool disposed = false;

        public ConnectionManager(IOptions<DBSettings> dbSettings)
        {
            _dbSettings = (DBSettings)dbSettings.Value;
            
            if (_dbSettings == null || string.IsNullOrWhiteSpace(_dbSettings.ConnectionString))
            {
                throw new ArgumentNullException(nameof(dbSettings), "DBSettings ConnectionString cannot be null or empty.");
            }

            this.connection = new SqlConnection(_dbSettings.ConnectionString);

        }

        public IDbConnection GetConnection()
        {
            if (connection.State != ConnectionState.Open)
            {
                connection.Open();
            }
            return connection;
        }

        public void CloseConnection()
        {
            if (connection.State != ConnectionState.Closed)
            {
                connection.Close();
            }
        }

        public void Dispose()
        {
            //Dispose(true);
            //GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    if (connection != null)
                    {
                        connection.Dispose();
                        connection = null;
                    }
                }

                // Dispose unmanaged resources

                disposed = true;
            }
        }
    }
}