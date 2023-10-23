using AWSLambdaWeb.Models;
using AWSLambdaWeb.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace AWSLambdaWeb.Pages.ShowData
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
        private readonly APISettings _settings;

        public IndexModel(IHttpClientFactory clientFactory , APISettings settings)
        {
            _clientFactory = clientFactory;
            _settings = settings;
        }

        [BindProperty]
        public List<ToDos> FilteredToDos { get; set; } = default!;

        public List<ToDos> AllToDos { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? pageIndex, string searchString, string currentFilter)
        {           
            var apiClient = _clientFactory.CreateClient();
            
            try
            {
                var response = await apiClient.GetAsync(_settings.ApiBaseUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    List<ToDos> result = new List<ToDos>();
                    var data = JsonSerializer.Deserialize<List<ToDos>>(jsonContent);
                    AllToDos = data;

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
                                    AllToDos.OrderBy(todo => todo.Owner).ToList() :
                                    AllToDos.OrderByDescending(todo => todo.Owner).ToList();
                                break;
                            default:
                                break;
                        }
                    }
                    
                    if (!String.IsNullOrEmpty(searchString))
                        AllToDos = AllToDos.Where(s => s.Id.ToString().Contains(searchString) || s.Title.ToString().Contains(searchString)
                                || s.IsCompleted.ToString().Contains(searchString) || s.Owner.ToString().Contains(searchString)
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
