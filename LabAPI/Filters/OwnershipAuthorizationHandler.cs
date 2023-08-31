﻿using Azure.Core;
using DbOperations;
using Entities.Models;
using ExternalEntities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Claims;
using System.Text;

namespace LabAPI.Filters
{
    public class OwnershipAuthorizationHandler : AuthorizationHandler<ResourceOwnershipRequirement>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OwnershipAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ResourceOwnershipRequirement requirement)
        {

            if (context.User.Claims.Count() < 1)
            {
                return Task.CompletedTask;
            }

            if(context.User.HasClaim("Role" ,"Admin"))
            {
                context.Succeed(requirement); 
                return Task.CompletedTask;
            }
            else
            {
                int id =0;
                //BAD Idea, but need to be resolved later
                string ConnectionString = @"Data Source=.;Initial Catalog=Lab;Integrated Security=True;Encrypt=False";

                if (_httpContextAccessor.HttpContext.Request.Method == HttpMethod.Get.Method ||
                    _httpContextAccessor.HttpContext.Request.Method == HttpMethod.Delete.Method)
                {
                   Int32.TryParse(_httpContextAccessor.HttpContext.Request.RouteValues.LastOrDefault().Value.ToString(), out id);    
                   
                    if(id==0) //This logic needs some working
                    {
                        context.Fail(); 
                        return Task.CompletedTask;
                    }
                        
                }
                else if(_httpContextAccessor.HttpContext.Request.Method == HttpMethod.Post.Method ||
                    _httpContextAccessor.HttpContext.Request.Method == HttpMethod.Put.Method)
                {
                    _httpContextAccessor.HttpContext.Request.EnableBuffering();

                    using (var streamReader = new StreamReader(_httpContextAccessor.HttpContext.Request.Body, Encoding.UTF8, true, -1, true))
                    {
                        {
                            var requestBody = streamReader.ReadToEnd();

                            var requestBodyObject = JsonConvert.DeserializeObject<Ex_ToDo>(requestBody);

                            id = requestBodyObject.Id;
                        }
                    }
                    _httpContextAccessor.HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
                }

                using (Operations op = new Operations(ConnectionString))
                {
                    ToDo toDo = op.GetToDoByIDForValidation(id);
                    var userId = context.User.Claims.Where(x => x.Type == "Id").First();

                    if (toDo == null)
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                        
                    
                    if (toDo.UserId.ToLower().Equals(userId.Value.ToString().ToLower()))
                    {
                        context.Succeed(requirement);
                        return Task.CompletedTask;
                    }
                    else
                        context.Fail();
                }
            }
            return Task.CompletedTask;
        }
    }
}
