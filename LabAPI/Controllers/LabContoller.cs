using DbOperations;
using Entities.Models;
using ExternalEntities;
using ExternalEntities.Misc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Buffers;
using System.Runtime;

namespace LabAPI.Controllers
{
    [Authorize(Policy = "ResourceOwnershipPolicy")]
    [ApiController]
    [Route("api")]
    [Produces("application/json")]
    public class LabContoller : ControllerBase
    {
        private readonly IOptions<DBSettings> _dbSettings;

        public LabContoller(IOptions<DBSettings> dbSettings)
        {
            _dbSettings = dbSettings;
        }

        [HttpGet("ToDo")]
        public async Task<ActionResult<ResponseObject<List<Ex_ToDo>>>> GetToDo()
        {
            ResponseObject<List<Ex_ToDo>> result = new ResponseObject<List<Ex_ToDo>>();

            try
            {
                using (Operations op = new Operations(_dbSettings.Value.ConnectionString))
                {
                    List<ToDo> list = await op.GetAllToDos();
                    if(list != null && list.Count> 0)
                    {
                        List<Ex_ToDo> ex_ToDos = new List<Ex_ToDo>();
                        list.ForEach(x=> ex_ToDos.Add (new Ex_ToDo { Id = x.Id, IsCompleted = x.IsCompleted, Title = x.Title }));
                        result.SetResponeData(ex_ToDos, ResultCode.Success, "Todos found");
                    }
                    else
                        result.SetResponeData(null, ResultCode.Success, "No record for ToDo was found");

                    return result;
                }
            }
            catch (Exception ex)
            {
                result.SetResponeData(null, ResultCode.Failure, ex.InnerException.ToString()??ex.Message.ToString()??ex.Message);
                return result;
            }
        }


        [HttpGet("ToDo/{id}")]
        public async Task<ActionResult<ResponseObject<Ex_ToDo>>>GetToDo(int id)
        {
            ResponseObject<Ex_ToDo> result = new ResponseObject<Ex_ToDo>();

            try
            {
                using (Operations op = new Operations(_dbSettings.Value.ConnectionString))
                {
                    ToDo toDo = await op.GetToDoByID(id);
                    if (toDo != null)
                    {
                        Ex_ToDo ex_ToDo = new Ex_ToDo() { Id = toDo.Id, IsCompleted = toDo.IsCompleted, Title = toDo.Title };

                        result.SetResponeData(ex_ToDo, ResultCode.Success, "Todos found");

                    }
                    else
                    {
                        result.SetResponeData(null, ResultCode.Success, String.Format("No record for ToDo was found with provide id {0}", id));
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.SetResponeData(null, ResultCode.Failure, ex.InnerException.ToString()??ex.Message);
                return result;
            }
        }

        [HttpPost("ToDo")]
        public async Task<ActionResult<ResponseObject<Ex_ToDo>>>AddToDo(Ex_ToDo ex_ToDo)
        {
            ResponseObject<Ex_ToDo> result = new ResponseObject<Ex_ToDo>();

            try
            {
                ToDo toDo = new ToDo() { IsCompleted = ex_ToDo.IsCompleted, Title = ex_ToDo.Title };
                
                var claim = this.HttpContext.User.Claims.Where(x => x.Type == "Role").FirstOrDefault();
                if (claim != null && (claim.Value.ToLower() == "Admin".ToLower() || claim.Value.ToLower() == "User".ToLower()))
                {
                    if (!ex_ToDo.UserId.Equals(string.Empty))
                        toDo.UserId = ex_ToDo.UserId;
                    else
                        toDo.UserId = this.HttpContext.User.Claims.First().Value;
                }
                

                using (Operations op = new Operations(_dbSettings.Value.ConnectionString))
                {
                    var data = await op.AddToDo(toDo);
                    if (data != null)
                    {
                        Ex_ToDo ex_Td = new Ex_ToDo() { Id = data.Id, IsCompleted = data.IsCompleted, Title = data.Title }; 
                        result.SetResponeData(ex_Td, ResultCode.Success, "ToDo inserted successfully");
                    }
                    else
                        result.SetResponeData(null, ResultCode.Failure, "ToDo could not be added");

                    return result;
                }
            }
            catch (Exception ex)
            {
                result.SetResponeData(null, ResultCode.Failure, ex.InnerException.ToString()??ex.Message);
                return result;
            }
        }

        [HttpPut("ToDo")]
        public async Task<ActionResult<ResponseObject<Ex_ToDo>>> UpdateToDo(Ex_ToDo ex_ToDo)
        {
            ResponseObject<Ex_ToDo> result = new ResponseObject<Ex_ToDo>();

            try
            {
                ToDo toDo = new ToDo() {Id= ex_ToDo.Id, IsCompleted = ex_ToDo.IsCompleted, Title = ex_ToDo.Title };

                var claim = this.HttpContext.User.Claims.Where(x => x.Type == "Role").FirstOrDefault();
                if (claim != null && (claim.Value.ToLower() == "Admin".ToLower() || claim.Value.ToLower() == "User".ToLower()))
                {
                    if (!ex_ToDo.UserId.Equals(string.Empty))
                        toDo.UserId = ex_ToDo.UserId;
                    else
                        toDo.UserId = this.HttpContext.User.Claims.First().Value;
                }

                using (Operations op = new Operations(_dbSettings.Value.ConnectionString))
                {
                    var data = await op.UpdateToDo(toDo);
                    if (data != null)
                    {
                        Ex_ToDo ex_Td = new Ex_ToDo() { Id = data.Id, IsCompleted = data.IsCompleted, Title = data.Title, UserId = data.UserId };
                        result.SetResponeData(ex_Td, ResultCode.Success, "ToDo Updated successfully");
                    }
                    else
                        result.SetResponeData(null, ResultCode.Failure, "ToDo could not be Updated");

                    return result;
                }
            }
            catch (Exception ex)
            {
                result.SetResponeData(null, ResultCode.Failure, ex.InnerException.ToString()??ex.Message);
                return result;
            }
        }

        [HttpDelete("ToDo/{Id}")]
        public async Task<ActionResult<ResponseObject<bool>>> DeleteToDo(int Id)
        {
            ResponseObject<bool> result = new ResponseObject<bool>();

            try
            {
                using (Operations op = new Operations(_dbSettings.Value.ConnectionString))
                {
                    var data = await op.RemoveToDo(Id);
                    if (data)
                    {                       
                        result.SetResponeData(data, ResultCode.Success, "ToDo deleted successfully");
                    }
                    else
                        result.SetResponeData(data, ResultCode.Failure, "ToDo could not be deletd");

                    return result;
                }
            }
            catch (Exception ex)
            {
                result.SetResponeData(false, ResultCode.Failure, ex.InnerException.ToString()??ex.Message);
                return result;
            }
        }
    }
}
