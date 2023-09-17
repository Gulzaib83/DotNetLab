using Entities_ADO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Interfaces
{
    public interface IToDoRepository
    {
        public Task<List<ToDo>> GetAllToDos();

        public Task<ToDo> GetToDoByID(int Id);

        public Task<ToDo> UpdateToDo(ToDo td);

        public Task<ToDo> AddToDo(ToDo toDo);

        public Task<bool> RemoveToDo(int Id);

        public ToDo GetToDoByIDForValidation(int Id);
    }
}
