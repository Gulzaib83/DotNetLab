using DbOperations;
using Entities.Models;
using Entities_ADO.Models;
using ExternalEntities;
using ExternalEntities.Misc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Repository.Interfaces;
using System.Buffers;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;

namespace LabAPI.Controllers
{
    [Authorize(Policy = "ResourceOwnershipPolicy")]
    [ApiController]
    [Route("api")]
    //[Produces("application/json")]
    public class LabContoller : ControllerBase
    {
        private readonly IOptions<DBSettings> _dbSettings;
        private readonly ConnectionManagement.ConnectionManager _connection;
        private readonly Repository.Interfaces.IToDoRepository _repo;

        public LabContoller(IOptions<DBSettings> dbSettings, IToDoRepository repository)
        {
            _dbSettings = dbSettings;
            //_connection = connection;
            _repo = repository;
        }

        [HttpGet("ToDo")]
        public async Task<ActionResult<ResponseObject<List<Ex_ToDo>>>> GetToDo()
        {
            ResponseObject<List<Ex_ToDo>> result = new ResponseObject<List<Ex_ToDo>>();

            try
            {               
                {   
                    List<Entities_ADO.Models.ToDo> list = await _repo.GetAllToDos();
                    if (list != null && list.Count > 0)
                    {
                        List<Ex_ToDo> ex_ToDos = new List<Ex_ToDo>();
                        list.ForEach(x => ex_ToDos.Add(new Ex_ToDo { Id = x.Id, IsCompleted = x.IsCompleted, Title = x.Title, UserId = x.UserId }));
                        result.SetResponeData(ex_ToDos, ResultCode.Success, "Todos found");
                    }
                    else
                        result.SetResponeData(null, ResultCode.Success, "No record for ToDo was found");
                }
            }
            catch (Exception ex)
            {
                result.SetResponeData(null, ResultCode.Failure, ex.InnerException.ToString() ?? ex.Message.ToString() ?? ex.Message);
                return result;
            }
            finally
            {

            }
            

            return result;
        }


        [HttpGet("ToDo/{id}")]
        public async Task<ActionResult<ResponseObject<Ex_ToDo>>>GetToDo(int id)
        {
            ResponseObject<Ex_ToDo> result = new ResponseObject<Ex_ToDo>();
            try
            {
                {
                    Entities_ADO.Models.ToDo toDo = await _repo.GetToDoByID(id);
                    if (toDo != null)
                    {
                        Ex_ToDo ex_ToDos = new Ex_ToDo()
                        { Id = toDo.Id, IsCompleted = toDo.IsCompleted, Title = toDo.Title, UserId = toDo.UserId };
                        result.SetResponeData(ex_ToDos, ResultCode.Success, "Todos found");
                    }
                    else
                        result.SetResponeData(null, ResultCode.Success, "No record for ToDo was found");
                }

                return result;
            }
            catch (Exception ex)
            {
                result.SetResponeData(null, ResultCode.Failure, ex.InnerException.ToString() ?? ex.Message);
                return result;
            }
        }

        [HttpPost("ToDo")]
        public async Task<ActionResult<ResponseObject<Ex_ToDo>>>AddToDo(Ex_ToDo ex_ToDo)
        {
            ResponseObject<Ex_ToDo> result = new ResponseObject<Ex_ToDo>();
            try
            {
                Entities_ADO.Models.ToDo toDo = new Entities_ADO.Models.ToDo() { IsCompleted = ex_ToDo.IsCompleted, Title = ex_ToDo.Title , UserId = ex_ToDo.UserId };

                //var claim = this.HttpContext.User.Claims.Where(x => x.Type == "Role").FirstOrDefault();
                //if (claim != null && (claim.Value.ToLower() == "Admin".ToLower()))
                //{
                //    if (!ex_ToDo.UserId.Equals(string.Empty))
                //        toDo.UserId = ex_ToDo.UserId;
                //    else
                //        toDo.UserId = this.HttpContext.User.Claims.First().Value;
                //}
                //else
                //{
                //    toDo.UserId = this.HttpContext.User.Claims.First().Value;
                //}


                {
                    toDo = await _repo.AddToDo(toDo);
                    if (toDo.Id > 0)
                    {
                        Ex_ToDo ex_ToDos = new Ex_ToDo()
                        { Id = toDo.Id, IsCompleted = toDo.IsCompleted, Title = toDo.Title, UserId = toDo.UserId };
                        result.SetResponeData(ex_ToDos, ResultCode.Success, "ToDo inserted successfully");
                    }
                    else
                        result.SetResponeData(null, ResultCode.Success, "ToDo could not be added");
                }
                return result;
            }
            catch (Exception ex)
            {
                result.SetResponeData(null, ResultCode.Failure, ex.InnerException.ToString() ?? ex.Message);
                return result;
            }

        }

        [HttpPut("ToDo")]
        public async Task<ActionResult<ResponseObject<Ex_ToDo>>> UpdateToDo(Ex_ToDo ex_ToDo)
        {
            ResponseObject<Ex_ToDo> result = new ResponseObject<Ex_ToDo>();
            try
            {
                Entities_ADO.Models.ToDo toDo = new Entities_ADO.Models.ToDo() { Id = ex_ToDo.Id, IsCompleted = ex_ToDo.IsCompleted, Title = ex_ToDo.Title, UserId = ex_ToDo.UserId };

                //var claim = this.HttpContext.User.Claims.Where(x => x.Type == "Role").FirstOrDefault();
                //if (claim != null && (claim.Value.ToLower() == "Admin".ToLower()))
                //{
                //    if (!ex_ToDo.UserId.Equals(string.Empty))
                //        toDo.UserId = ex_ToDo.UserId;
                //    else
                //        toDo.UserId = this.HttpContext.User.Claims.First().Value;
                //}
                //else
                //{
                //    toDo.UserId = this.HttpContext.User.Claims.First().Value;
                //}

                {
                    toDo = await _repo.UpdateToDo(toDo);
                    if (toDo.Id > 0)
                    {
                        Ex_ToDo ex_ToDos = new Ex_ToDo()
                        { Id = toDo.Id, IsCompleted = toDo.IsCompleted, Title = toDo.Title, UserId = toDo.UserId };
                        result.SetResponeData(ex_ToDos, ResultCode.Success, "ToDo Update successfully");
                    }
                    else
                        result.SetResponeData(null, ResultCode.Success, "ToDo could not be updated");
                }
                return result;
            }
            catch (Exception ex)
            {
                result.SetResponeData(null, ResultCode.Failure, ex.InnerException.ToString() ?? ex.Message);
                return result;
            }
        }

        [HttpDelete("ToDo/{Id}")]
        public async Task<ActionResult<ResponseObject<bool>>> DeleteToDo(int Id)
        {
            ResponseObject<bool> result = new ResponseObject<bool>();
            try
            {
                {
                    var data = await _repo.RemoveToDo(Id);
                    if (data)
                    {
                        
                        result.SetResponeData(true, ResultCode.Success, "ToDo deleted successfully");
                    }
                    else
                        result.SetResponeData(false, ResultCode.Success, "ToDo could not be deletd");
                }
                return result;
            }
            catch (Exception ex)
            {
                result.SetResponeData(false, ResultCode.Failure, ex.InnerException.ToString() ?? ex.Message);
                return result;
            }

        }

        [HttpPost("Upload")]
        [DisableRequestSizeLimit]
        public async Task<ActionResult<ResponseObject<bool>>> UploadFile()
        {
            ResponseObject<bool> result = new ResponseObject<bool>();
            try
            {
                var file = Request.Form.Files[0];
                if (file != null && file.Length > 0)
                {
                    using (var stream = new MemoryStream())
                    {

                        await file.CopyToAsync(stream);
                        stream.Position = 0;
                        var workbook = new XSSFWorkbook(stream); // For .xlsx files

                        ISheet sheet = workbook.GetSheetAt(0);
                        int rowCount = sheet.PhysicalNumberOfRows;

                        for (int row = 1; row < rowCount; row++)
                        {
                            IRow dataRow = sheet.GetRow(row);

                            var data = new Entities_ADO.Models.ToDo()
                            {
                                Title = dataRow.GetCell(1).ToString(),
                                IsCompleted = Convert.ToBoolean(dataRow.GetCell(2).ToString()),
                                UserId = dataRow.GetCell(3).ToString()
                            };

                            _repo.AddToDo(data);
                        }

                        result.SetResponeData(true, ResultCode.Success, "Data uploaded successfully");

                    }
                }
            }
            catch(Exception ex)
            {
                result.SetResponeData(false, ResultCode.Failure, ex.InnerException.ToString());
            }
            return result;
        }

    }
}
