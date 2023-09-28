using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace ExcelRead.Pages.UploadExcel
{
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _clientFactory;

        public IndexModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }
        public void OnGet()
        {
        }

        [BindProperty]
        public IFormFile File { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var apiClient = _clientFactory.CreateClient();

            if (File == null || File.Length == 0)
            {
                return BadRequest("File not selected or empty.");
            }

            if (!Path.GetExtension(File.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Invalid file format. Only Excel files (.xlsx) are allowed.");
            }

            // Create a MultipartFormDataContent instance to build your multipart request.
            var multipartContent = new MultipartFormDataContent();

            // Add the file content to the multipart request.
            var fileStreamContent = new StreamContent(File.OpenReadStream());
            fileStreamContent.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data")
            {
                Name = "file", // The name of the form field for the file upload
                FileName = File.FileName, // The original filename
            };

            multipartContent.Add(fileStreamContent);

            // Make the POST request with the multipart content.
            var response = await apiClient.PostAsync("http://localhost:5299/api/Upload/", multipartContent);


            return RedirectToPage("/ToDos/index");
        }
    }
}
