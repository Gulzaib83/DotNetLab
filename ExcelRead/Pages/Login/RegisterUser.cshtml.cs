using ExternalEntities.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text;

namespace ExcelRead.Pages.Login
{
    public class RegisterUserModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public RegisterUserModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [BindProperty]
        public EX_UserRegister UserRegister { get; set; } = default!;
        
        [TempData]
        public string StatusMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {

            var httpClient = _clientFactory.CreateClient();

            var apiClient = _clientFactory.CreateClient();
            string json = JsonSerializer.Serialize(UserRegister);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Make an API call to authenticate the user and retrieve the JWT token
            var response = await httpClient.PostAsync("http://localhost:5299/api/UserAccount/RegisterUser/", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                ResponseObjectConverter<EX_TokenResult> converter = new ResponseObjectConverter<EX_TokenResult>();
                var data = JsonSerializer.Deserialize<ResponseObject<bool>>(jsonContent, new JsonSerializerOptions { Converters = { converter } });

               // HttpContext.Session.SetString("jwtToken", data._data.Token.TrimEnd());

                // Redirect to the desired page
                return RedirectToPage(returnUrl ?? "/Index");
            }
            else
            {

                var jsonContent = await response.Content.ReadAsStringAsync();
                ResponseObjectConverter<bool> converter = new ResponseObjectConverter<bool>();
                var data = JsonSerializer.Deserialize<ResponseObject<bool>>(jsonContent, new JsonSerializerOptions { Converters = { converter } });

                StatusMessage = data._message;
            }


            // If login fails, redisplay the form
            return Page();
        }
    }
}
