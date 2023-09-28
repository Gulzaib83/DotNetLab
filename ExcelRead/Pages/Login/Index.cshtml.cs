using ExternalEntities;
using ExternalEntities.Misc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ExcelRead.Pages.Login
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public EX_Login Login { get; set; } = default!;

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            //if (ModelState.IsValid)
            {
                var httpClient = _clientFactory.CreateClient();

                var apiClient = _clientFactory.CreateClient();
                string json = JsonSerializer.Serialize(Login);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Make an API call to authenticate the user and retrieve the JWT token
                var response = await httpClient.PostAsync("http://localhost:5299/api/UserAccount/Login/", content);

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    ResponseObjectConverter<EX_TokenResult> converter = new ResponseObjectConverter<EX_TokenResult>();
                    var data = JsonSerializer.Deserialize<ResponseObject<EX_TokenResult>>(jsonContent, new JsonSerializerOptions { Converters = { converter } });

                    HttpContext.Session.SetString("jwtToken", data._data.Token.TrimEnd());

                    // Redirect to the desired page
                    return RedirectToPage(returnUrl ?? "/Index");
                }
                else
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    ResponseObjectConverter<EX_TokenResult> converter = new ResponseObjectConverter<EX_TokenResult>();
                    var data = JsonSerializer.Deserialize<ResponseObject<EX_TokenResult>>(jsonContent, new JsonSerializerOptions { Converters = { converter } });


                    StatusMessage = data._message;
                }
            }

            // If login fails, redisplay the form
            return Page();
        }
    }
}
