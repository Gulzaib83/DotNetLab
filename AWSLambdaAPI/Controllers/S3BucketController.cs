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
        private const string _BucketName = "task6.s3bucket";

        private readonly IOptions<DBSettings> _DbSettings;
        private readonly Repository.Interfaces.IToDoRepository _Repo;

        public S3BucketController(IOptions<DBSettings> dbSettings, IToDoRepository repository)
        {
            _DbSettings = dbSettings;
            _Repo = repository;
        }

        [HttpPost]
        public async Task Post(IFormFile file)
        {
                var S3client = new AmazonS3Client();

                bool Bucket = await AmazonS3Util.DoesS3BucketExistV2Async(S3client, _BucketName);
                if (!Bucket)
                {
                    var bucketRequest = new PutBucketRequest()
                    {
                        BucketName = _BucketName,
                        UseClientRegion = true,
                    };

                    await S3client.PutBucketAsync(bucketRequest);
                }

                var ObjectRequest = new PutObjectRequest()
                {
                    BucketName = _BucketName,
                    Key = file.FileName,
                    InputStream = file.OpenReadStream(),

                };

                await S3client.PutObjectAsync(ObjectRequest);

        }

        [HttpGet("GetFile")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            var S3client = new AmazonS3Client();

            GetObjectRequest Request = new GetObjectRequest
            {
                BucketName = _BucketName,
                Key = fileName
            };

            var Response = await S3client.GetObjectAsync(Request);

            using (var stream = new MemoryStream())
            {

                await Response.ResponseStream.CopyToAsync(stream);
                stream.Position = 0;
                var Workbook = new XSSFWorkbook(stream); // For .xlsx files

                ISheet Sheet = Workbook.GetSheetAt(0);
                int rowCount = Sheet.PhysicalNumberOfRows;

                for (int row = 1; row < rowCount; row++)
                {
                    IRow dataRow = Sheet.GetRow(row);

                    var data = new Entities_ADO.Models.ToDo()
                    {
                        Title = dataRow.GetCell(1).ToString(),
                        IsCompleted = Convert.ToBoolean(dataRow.GetCell(2).ToString()),
                        Owner = Convert.ToInt32(dataRow.GetCell(3).ToString())
                    };

                    var Result = await _Repo.AddToDo(data);
                }
                return Ok();
            }

        }

        [HttpGet("GetFileLocal")]
        public async Task<IActionResult> GetFileLocal(string fileName)
        {
            var S3Client = new AmazonS3Client();

            GetObjectRequest Request = new GetObjectRequest
            {
                BucketName = _BucketName,
                Key = fileName
            };

            var Response = await S3Client.GetObjectAsync(Request);

            using (FileStream stream = new FileStream("..//" + "ToDosFromS3-" + $"{DateTime.Now:yyyyMMddhhmm}.xlsx", FileMode.Create, FileAccess.Write))
            {
                IWorkbook Workbook = new XSSFWorkbook();

                await Response.ResponseStream.CopyToAsync(stream);
                stream.Position = 0;
                Workbook.Write(stream);
            }

            return Ok();

        }
    }
}
