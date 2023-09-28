using ConnectionManagement;
using Entities_ADO.Models;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;


namespace DbOperations_ADO
{
    public class DbOperations : IToDoRepository
    {
        private readonly ConnectionManager _connectionManager;

        public DbOperations(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
        }

        public async Task<ToDo> AddToDo(ToDo toDo)
        {
            //using (var connection = _connectionManager.GetConnection())
            {
                var connection = _connectionManager.GetConnection();
                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "INSERT_TODOS";

                    SqlParameter Id = new SqlParameter();
                    Id.ParameterName = "@Id";
                    Id.SqlDbType = SqlDbType.Int;
                    Id.Direction = ParameterDirection.Output;
                    command.Parameters.Add(Id);

                    SqlParameter Title = new SqlParameter();
                    Title.ParameterName = "@Title";
                    Title.SqlDbType = SqlDbType.NVarChar;
                    Title.Direction = ParameterDirection.Input;
                    Title.Value = toDo.Title;
                    command.Parameters.Add(Title);

                    SqlParameter IsComplete = new SqlParameter();
                    IsComplete.ParameterName = "@IsCompleted";
                    IsComplete.SqlDbType = SqlDbType.Bit;
                    IsComplete.Direction = ParameterDirection.Input;
                    IsComplete.Value = toDo.IsCompleted;
                    command.Parameters.Add(IsComplete);

                    if(!toDo.UserId.IsNullOrEmpty())
                    {
                        SqlParameter UserId = new SqlParameter();
                        UserId.ParameterName = "@UserId";
                        UserId.SqlDbType = SqlDbType.NVarChar;
                        UserId.Direction = ParameterDirection.Input;
                        UserId.Value = toDo.UserId;
                        command.Parameters.Add(UserId);
                    }
                    else
                    {
                        SqlParameter Owner = new SqlParameter();
                        Owner.ParameterName = "@Owner";
                        Owner.SqlDbType = SqlDbType.Int;
                        Owner.Direction = ParameterDirection.Input;
                        Owner.Value = toDo.Owner;
                        command.Parameters.Add(Owner);
                    }
                    

                    command.ExecuteNonQuery();
                   
                    toDo.Id =Convert.ToInt32( Id.Value);
                    connection.Close();
                }
                return toDo;
            }
        }

        public async Task<List<ToDo>> GetAllToDos()
        {
            //using (var connection = _connectionManager.GetConnection())
            {
                var connection = _connectionManager.GetConnection();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM ToDos";
                    using (var reader = command.ExecuteReader())
                    {
                        var todos = new List<ToDo>();
                        while (reader.Read())
                        {
                            ToDo toDo = new ToDo() { 
                            Id = reader[0]!= null ? Convert.ToInt32( reader[0]): 0,
                            Title = reader[1] != null ? reader[1].ToString() : string.Empty,
                            IsCompleted = reader[2] != null ? Convert.ToBoolean( reader[2]) : false,
                            UserId = reader[3] != null ? reader[3].ToString() : string.Empty
                            };

                            todos.Add(toDo);
                        }
                        connection.Close();

                        return todos;
                    }
                }
            }
        }

        public async Task<ToDo> GetToDoByID(int Id)
        {
            //using (var connection = _connectionManager.GetConnection())
            {
                var connection = _connectionManager.GetConnection();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM ToDos Where Id = @Id";

                    SqlParameter parameter = new SqlParameter();
                    parameter.ParameterName = "@Id";
                    parameter.SqlDbType = SqlDbType.Int;
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = Id;

                    command.Parameters.Add(parameter);
                    using (var reader = command.ExecuteReader())
                    {
                        ToDo toDo = new ToDo();

                        while (reader.Read())
                        {
                                toDo.Id = reader[0] != null ? Convert.ToInt32(reader[0]) : 0;
                                toDo.Title = reader[1] != null ? reader[1].ToString() : string.Empty;
                                toDo.IsCompleted = reader[2] != null ? Convert.ToBoolean(reader[2]) : false;
                                toDo.UserId = reader[3] != null ? reader[3].ToString() : string.Empty;                           
                        }
                        connection.Close();

                        return toDo;
                    }
                }
            }
        }

        public ToDo GetToDoByIDForValidation(int Id)
        {
            //using (var connection = _connectionManager.GetConnection())
            {
                var connection = _connectionManager.GetConnection();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM ToDos Where Id = @Id";

                    SqlParameter parameter = new SqlParameter();
                    parameter.ParameterName = "@Id";
                    parameter.SqlDbType = SqlDbType.Int;
                    parameter.Direction = ParameterDirection.Input;
                    parameter.Value = Id;

                    command.Parameters.Add(parameter);
                    using (var reader = command.ExecuteReader())
                    {
                        ToDo toDo = null;

                        while (reader.Read())
                        {
                            toDo = new ToDo();

                            toDo.Id = reader[0] != null ? Convert.ToInt32(reader[0]) : 0;
                            toDo.Title = reader[1] != null ? reader[1].ToString() : string.Empty;
                            toDo.IsCompleted = reader[2] != null ? Convert.ToBoolean(reader[2]) : false;
                            toDo.UserId = reader[3] != null ? reader[3].ToString() : string.Empty;

                            break;
                        }
                        connection.Close();

                        return toDo;
                    }
                }
            }
        }

        public async Task<bool> RemoveToDo(int Id)
        {
            bool result = false;
            //using (var connection = _connectionManager.GetConnection())
            {
                var connection = _connectionManager.GetConnection();

                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "DELETE_TODOS";

                    SqlParameter toDO_Id = new SqlParameter();
                    toDO_Id.ParameterName = "@Id";
                    toDO_Id.SqlDbType = SqlDbType.Int;
                    toDO_Id.Direction = ParameterDirection.Input;
                    toDO_Id.Value = Id;
                    command.Parameters.Add(toDO_Id);

                    var rowsAffected = command.ExecuteNonQuery();

                    connection.Close();

                    if(rowsAffected > 0)
                        result = true;
                }

                return result;
            }
        }

        public async Task<ToDo> UpdateToDo(ToDo toDo)
        {
            //using (var connection = _connectionManager.GetConnection())
            {
                var connection = _connectionManager.GetConnection();

                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "UPDATE_TODOS";

                    SqlParameter Id = new SqlParameter();
                    Id.ParameterName = "@Id";
                    Id.SqlDbType = SqlDbType.Int;
                    Id.Direction = ParameterDirection.Input;
                    Id.Value = toDo.Id;
                    command.Parameters.Add(Id);

                    SqlParameter Title = new SqlParameter();
                    Title.ParameterName = "@Title";
                    Title.SqlDbType = SqlDbType.NVarChar;
                    Title.Direction = ParameterDirection.Input;
                    Title.Value = toDo.Title;
                    command.Parameters.Add(Title);

                    SqlParameter IsComplete = new SqlParameter();
                    IsComplete.ParameterName = "@IsCompleted";
                    IsComplete.SqlDbType = SqlDbType.Bit;
                    IsComplete.Direction = ParameterDirection.Input;
                    IsComplete.Value = toDo.IsCompleted;
                    command.Parameters.Add(IsComplete);

                    SqlParameter UserId = new SqlParameter();
                    UserId.ParameterName = "@UserId";
                    UserId.SqlDbType = SqlDbType.NVarChar;
                    UserId.Direction = ParameterDirection.Input;
                    UserId.Value = toDo.UserId;
                    command.Parameters.Add(UserId);

                    command.ExecuteNonQuery();
                }
                connection.Close();
                return toDo;
            }
        }
    }
}
