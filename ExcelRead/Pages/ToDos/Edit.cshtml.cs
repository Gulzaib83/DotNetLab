using ExternalEntities;
using ExternalEntities.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ExcelRead.Pages.ToDos
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public EditModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public Ex_ToDo ToDo { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var apiClient = _clientFactory.CreateClient();
            var apiUrl = "http://localhost:5299/api/ToDo/" + id;

            try
            {
                var response = await apiClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    ResponseObjectConverter<Ex_ToDo> converter = new ResponseObjectConverter<Ex_ToDo>();
                    var data = JsonSerializer.Deserialize<ResponseObject<Ex_ToDo>>(jsonContent, new JsonSerializerOptions { Converters = { converter } });
                    ToDo = data._data;

                    // Process the data as needed
                }
                else
                {
                    // Handle an error response here, if needed
                }
                return Page();
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., network errors)
                // Logging and error handling should be added here
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var apiClient = _clientFactory.CreateClient();
            string json = JsonSerializer.Serialize(ToDo);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await apiClient.PutAsync("http://localhost:5299/api/ToDo/", content);
                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    ResponseObjectConverter<Ex_ToDo> converter = new ResponseObjectConverter<Ex_ToDo>();
                    var data = JsonSerializer.Deserialize<ResponseObject<Ex_ToDo>>(jsonContent, new JsonSerializerOptions { Converters = { converter } });
                    ToDo = data._data;
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

        private bool ToDosExists(int id)
        {
            return true;// (_context.Trades?.Any(e => e.TradeId == id)).GetValueOrDefault();
        }
    }
}
