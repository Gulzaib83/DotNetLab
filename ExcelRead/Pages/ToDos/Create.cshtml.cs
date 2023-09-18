using ExternalEntities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ExcelRead.Pages.ToDos
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;
        
        public CreateModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public Ex_ToDo ToDo { get; set;} = default!;

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {

            var apiClient = _clientFactory.CreateClient();
            string json = JsonSerializer.Serialize(ToDo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await apiClient.PostAsync("http://localhost:5299/api/ToDo/", content);
                if (response.IsSuccessStatusCode)
                {
                    
                }
                else
                {
                    // Handle an error response here, if needed
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., network errors)
                // Logging and error handling should be added here
            }

            return RedirectToPage("./Index");
        }
    }
}
