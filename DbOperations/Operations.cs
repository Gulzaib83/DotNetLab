using Entities.Models;
using ExternalEntities;
using Microsoft.EntityFrameworkCore;

namespace DbOperations
{
    public class Operations : IDisposable
    {
        private readonly string _connectionString;

        //private string _connString;
        //public string ConnectionString
        //{
        //    get { return _connString; }
        //    set { _connString = value; }
        //}

        public Operations(string connString)
        {
            Context = new LabContext(connString);
        }

        private LabContext Context = new LabContext();

        public async Task<List<ToDo>> GetAllToDos()
        {
            
            List<ToDo> list = new List<ToDo>();
            try
            {
                var data = await Context.ToDos.ToListAsync();

                if (data != null)
                    list = data;

                return list;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {

            }
        }

        public async Task<ToDo> GetToDoByID(int Id)
        {
            
            try
            {
                var data = await Context.ToDos.Where(x => x.Id == Id).FirstOrDefaultAsync();                 
                    
                return data;
                
                
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {

            }
        }

        public async Task<ToDo> UpdateToDo(ToDo td)
        {
            ToDo? obj = null;
            try
            {
                Context.ToDos.Update(td);

                int affectedRows = await Context.SaveChangesAsync();

                if (affectedRows > 0)
                {
                    obj = await GetToDoByID(td.Id);
                }
                
                return obj;

            }
            catch (Exception ex)
            {
                throw;
            }
            finally { }
        }

        public async Task<ToDo> AddToDo(ToDo toDo)
        {
            try
            {
                Context.ToDos.Add(toDo);

                int affectedRows = await Context.SaveChangesAsync();

                if (affectedRows > 0)
                {
                    return toDo;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally { }
        }

        public async Task<bool> RemoveToDo(int Id)
        {
            try
            {
                var data = await this.Context.ToDos.SingleAsync(x => x.Id == Id);

                this.Context.ToDos.Remove(data);

                int affectedRows = await Context.SaveChangesAsync();

                if (affectedRows > 0)
                {
                    return true;
                }
                else
                    return false;

            }
            catch (Exception ex)
            {
                throw;
            }
            finally { }
        }

        public ToDo GetToDoByIDForValidation(int Id)
        {

            try
            {
                var data =  Context.ToDos.Where(x => x.Id == Id).FirstOrDefault();

                return data;


            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {

            }
        }

        public void Dispose()
        {
            ///TODO: Memory Managment
            //throw new NotImplementedException();
        }
    }
}
