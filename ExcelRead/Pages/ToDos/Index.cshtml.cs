using ExcelRead.Misc;
using ExternalEntities;
using ExternalEntities.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Sockets;
using System;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Mail;

namespace ExcelRead.Pages.ToDos
{
    [IgnoreAntiforgeryToken]
    public class IndexModel : PageModel
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public string SortField { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortDirection { get; set; }

        public string CurrentFilter { get; set; }

        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public List<Ex_ToDo> FilteredToDos { get; set; } = default!;

        public List<Ex_ToDo> AllToDos { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? pageIndex, string searchString, string currentFilter)
        {           
            

            var JwtToken = HttpContext.Session.GetString("jwtToken");

            var apiClient = _clientFactory.CreateClient();



            if (!string.IsNullOrEmpty(JwtToken))
            {
                //request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
                //apiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", JwtToken);
                apiClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + JwtToken);

            }
            try
            {
                var response = await apiClient.GetAsync("https://localhost:7299/api/ToDo/");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    ResponseObjectConverter<List<Ex_ToDo>> converter = new ResponseObjectConverter<List<Ex_ToDo>>();
                    var data = JsonSerializer.Deserialize<ResponseObject<List<Ex_ToDo>>>(jsonContent, new JsonSerializerOptions { Converters = { converter } });
                    AllToDos = data._data;

                    PageIndex = pageIndex ?? 1;

                    if (searchString != null)
                        pageIndex = 1;
                    else
                        searchString = currentFilter;
                    CurrentFilter = searchString;

                    if (!string.IsNullOrEmpty(SortField))
                    {
                        switch (SortField.ToLower())
                        {
                            case "id":
                                AllToDos = SortDirection == "asc" ?
                                    AllToDos.OrderBy(todo => todo.Id).ToList() :
                                    AllToDos.OrderByDescending(todo => todo.Id).ToList();
                                break;
                            case "title":
                                AllToDos = SortDirection == "asc" ?
                                    AllToDos.OrderBy(todo => todo.Title).ToList() :
                                    AllToDos.OrderByDescending(todo => todo.Title).ToList();
                                break;
                            case "iscompleted":
                                AllToDos = SortDirection == "asc" ?
                                    AllToDos.OrderBy(todo => todo.IsCompleted).ToList() :
                                    AllToDos.OrderByDescending(todo => todo.IsCompleted).ToList();
                                break;
                            case "userid":
                                AllToDos = SortDirection == "asc" ?
                                    AllToDos.OrderBy(todo => todo.UserId).ToList() :
                                    AllToDos.OrderByDescending(todo => todo.UserId).ToList();
                                break;
                            default:
                                break;
                        }
                    }
                    //List<Ex_ToDo> shortList = new List<Ex_ToDo>();
                    if (!String.IsNullOrEmpty(searchString))
                        AllToDos = AllToDos.Where(s => s.Id.ToString().Contains(searchString) || s.Title.ToString().Contains(searchString)
                                || s.IsCompleted.ToString().Contains(searchString) || s.UserId.Contains(searchString)
                                ).ToList();

                    // Paging logic
                    var pagedRecords = AllToDos
                        .Skip((PageIndex - 1) * PageSize)
                        .Take(PageSize)
                        .ToList();

                    FilteredToDos = pagedRecords;


                }
                else
                {

                }
                return Page();
            }
            catch (Exception ex)
            {

                return Page();
            }
        }

        public string GetNewSortDirection(string currentSortField)
        {
            if (SortField == currentSortField)
            {
                if (SortDirection == "asc")
                {
                    SortDirection = "desc";
                }
                else
                    SortDirection = "asc";
            }
            else
            {
                // Default to ascending when sorting a new column
                SortField = currentSortField;
                SortDirection = "asc";
            }

            return SortDirection;
        }
    }
}
