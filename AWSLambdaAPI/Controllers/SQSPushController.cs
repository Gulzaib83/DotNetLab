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

namespace AWSLambdaAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SQSPushController : ControllerBase
    {
        public SQSPushController()
        {
            
        }

        [HttpPost]
        public async Task Post(IFormFile file)
        {
            //var SQSClient = new AmazonSQSClient();
            var credentials = new BasicAWSCredentials("AKIAZWGBV43T4YI2SKTY", "F33Brq0R9zLGsP/vAs71Upx6vJMoCPe/tYA9PJbn");

            var SNSClient = new AmazonSimpleNotificationServiceClient(credentials, Amazon.RegionEndpoint.USEast1);

            var workbook = new XSSFWorkbook(file.OpenReadStream()); // For .xlsx files

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

                var request = new PublishRequest()
                {
                    TopicArn = "arn:aws:sns:us-east-1:666126116583:FileUpdateSNS",
                    Subject = $"Message for {data.Owner}",
                    Message = JsonSerializer.Serialize(data)
                };

                var response = await SNSClient.PublishAsync(request);
                //var request = new SendMessageRequest()
                //{
                //    QueueUrl = "https://sqs.us-east-1.amazonaws.com/666126116583/ToDosQueue",
                //    MessageBody = $"Record pushed to SQS for processing for Owner {data.Owner}"
                //};

                //SQSClient.SendMessageAsync(request); //await removed purposfully
            }
        }
    }
}
