using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using ExternalEntities.Misc;
using Microsoft.Extensions.Options;
using Repository.Interfaces;
using Amazon.SecretsManager;
using Amazon;
using Amazon.SecretsManager.Model;

namespace AWSLambdaAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class S3BucketController : ControllerBase
    {
        private string bucketName = "task6.s3bucket";

        private readonly IOptions<DBSettings> _dbSettings;
        private readonly Repository.Interfaces.IToDoRepository _repo;

        public S3BucketController(IOptions<DBSettings> dbSettings, IToDoRepository repository)
        {
            _dbSettings = dbSettings;
            _repo = repository;
        }

        [HttpPost]
        public async Task Post(IFormFile file)
        {
            var S3client = new AmazonS3Client();

            bool bucket = await AmazonS3Util.DoesS3BucketExistV2Async(S3client, bucketName);
            if (!bucket)
            {
                var bucketRequest = new PutBucketRequest()
                {
                    BucketName = bucketName,
                    UseClientRegion = true,
                };

                await S3client.PutBucketAsync(bucketRequest);
            }

            var objectRequest = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = file.FileName,
                //ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                InputStream = file.OpenReadStream(),

            };

            await S3client.PutObjectAsync(objectRequest);

        }

        [HttpGet("GetFile")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            var s3Client = new AmazonS3Client();

            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            var response = await s3Client.GetObjectAsync(request);

            using (var stream = new MemoryStream())
            {

                await response.ResponseStream.CopyToAsync(stream);
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
                        Owner = Convert.ToInt32(dataRow.GetCell(3).ToString())
                    };

                    var result = await _repo.AddToDo(data);
                }
                return Ok();
            }

        }

        [HttpGet("GetFileLocal")]
        public async Task<IActionResult> GetFileLocal(string fileName)
        {
            var s3Client = new AmazonS3Client();

            GetObjectRequest request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            var response = await s3Client.GetObjectAsync(request);

            using (FileStream stream = new FileStream("..//" + "ToDosFromS3-" + $"{DateTime.Now:yyyyMMddhhmm}.xlsx", FileMode.Create, FileAccess.Write))
            {
                IWorkbook workbook = new XSSFWorkbook();

                await response.ResponseStream.CopyToAsync(stream);
                stream.Position = 0;
                workbook.Write(stream);
            }

            return Ok();

        }
    }
}
