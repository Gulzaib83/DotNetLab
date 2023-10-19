using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.S3;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Amazon.SQS;
using Amazon.SQS.Model;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.Text.Json;
using Amazon.SimpleNotificationService;
using Amazon.Runtime;
using Amazon.SimpleNotificationService.Model;
using Microsoft.Extensions.Options;
using AWSLambdaAPI.AWS;
using Entities_ADO.Models;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using System.Reflection.Metadata.Ecma335;
using AWSLambdaAPI.Models;

namespace AWSLambdaAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SQSPushController : ControllerBase
    {
        private readonly AWSSecretsOptions _options;

        public SQSPushController(IOptions<AWSSecretsOptions> options)
        {
            _options = options.Value;
        }

        [HttpPost]
        public async Task Post(IFormFile file)
        {
            var credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
            var SNSClient = new AmazonSimpleNotificationServiceClient(credentials, Amazon.RegionEndpoint.USEast1);

            var workbook = new XSSFWorkbook(file.OpenReadStream()); // For .xlsx files

            ISheet sheet = workbook.GetSheetAt(0);
            int rowCount = sheet.PhysicalNumberOfRows;

            for (int row = 1; row < rowCount; row++)
            {
                IRow dataRow = sheet.GetRow(row);

                var data = new Entities_ADO.Models.ToDo()
                {
                    Id = Convert.ToInt32(dataRow.GetCell(0).ToString()),
                    Title = dataRow.GetCell(1).ToString(),
                    IsCompleted = Convert.ToBoolean(dataRow.GetCell(2).ToString()),
                    Owner = Convert.ToInt32(dataRow.GetCell(3).ToString())
                };

                var request = new PublishRequest()
                {
                    TopicArn = _options.SNSARN,
                    Subject = $"Message for {data.Owner}",
                    Message = JsonSerializer.Serialize(data)
                };

               await SNSClient.PublishAsync(request);
                
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetToDos()
        {
            var credentials = new BasicAWSCredentials(_options.AccessKey, _options.SecretKey);
            AmazonDynamoDBClient client = new AmazonDynamoDBClient(credentials);
            DynamoDBContext db = new DynamoDBContext(client);

            var condition = new List<ScanCondition>();
            var allTodos = db.ScanAsync<ToDos>(condition).GetRemainingAsync().Result;

            return Ok(allTodos);

        }
    }
}
