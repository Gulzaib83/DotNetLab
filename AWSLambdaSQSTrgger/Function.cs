using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using Amazon.Runtime;
using AWSLambdaSQSTrgger.SecretManager;
using Entities_ADO.Models;
using Newtonsoft.Json.Linq;
using System.Text.Json;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdaSQSTrgger;

public class Function
{
    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {

    }


    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an SQS event object and can be used 
    /// to respond to SQS messages.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(SQSEvent evnt, ILambdaContext context)
    {
        foreach(var message in evnt.Records)
        {
            await ProcessMessageAsync(message, context);
        }
    }

    private async Task ProcessMessageAsync(SQSEvent.SQSMessage message, ILambdaContext context)
    {
        var mssagePart = JObject.Parse(message.Body)["Message"]!.ToString();
        
        var data = JsonSerializer.Deserialize<Entities_ADO.Models.ToDo>(mssagePart);

        string _accessKey = await AWSSecretManager.GetSecret("AccessKey");

        string _clientSecret = await AWSSecretManager.GetSecret("ClientSecret");

        var credentials = new BasicAWSCredentials(_accessKey, _clientSecret);
        
        AmazonDynamoDBClient client = new AmazonDynamoDBClient(credentials);

        string tableName = "ToDos";
        
        var request = new PutItemRequest
        {
            TableName = tableName,
            Item = new Dictionary<string, AttributeValue>()
            {
                { "Id", new AttributeValue { N = data.Id.ToString() } },
                { "Title", new AttributeValue { S = data.Title } },
                { "IsCompleted", new AttributeValue { BOOL = data.IsCompleted ?? false } },
                { "Owner", new AttributeValue { N = data.Owner?.ToString() ?? "0" } }
            }

        };
        context.Logger.LogInformation($"Deserialized data from Topic is {data.Title}");


        var response =  await client.PutItemAsync(request);
        context.Logger.LogInformation($"Response from Dynamo is {response.HttpStatusCode}");
        await Task.CompletedTask;
    }
}