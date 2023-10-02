using Amazon;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Data.SqlClient;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.UserModel;
using NPOI.Util;
using NPOI.XSSF.UserModel;
using System.Collections.Generic;
using System.Data;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdaS3Trigger;

public class Function
{
    IAmazonS3 S3Client { get; set; }

    /// <summary>
    /// Default constructor. This constructor is used by Lambda to construct the instance. When invoked in a Lambda environment
    /// the AWS credentials will come from the IAM role associated with the function and the AWS region will be set to the
    /// region the Lambda function is executed in.
    /// </summary>
    public Function()
    {
        S3Client = new AmazonS3Client();
    }

    /// <summary>
    /// Constructs an instance with a preconfigured S3 client. This can be used for testing outside of the Lambda environment.
    /// </summary>
    /// <param name="s3Client"></param>
    public Function(IAmazonS3 s3Client)
    {
        this.S3Client = s3Client;
    }

    /// <summary>
    /// This method is called for every Lambda invocation. This method takes in an S3 event object and can be used 
    /// to respond to S3 notifications.
    /// </summary>
    /// <param name="evnt"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task FunctionHandler(S3Event evnt, ILambdaContext context)
    {
        SecretManagerUtility secretManager = new SecretManagerUtility();
        var eventRecords = evnt.Records ?? new List<S3Event.S3EventNotificationRecord>();
        foreach (var record in eventRecords)
        {
            var s3Event = record.S3;
            if (s3Event == null)
            {
                continue;
            }

            try
            {
                String SecretName = "DbSettings"; //ToDo: Implement Options Pattern to get all secret
                String RDSConnectionString = await secretManager.GetSecretValue(SecretName);
                var response = await this.S3Client.GetObjectAsync(s3Event.Bucket.Name, s3Event.Object.Key);

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

                        var toDo = new Entities_ADO.Models.ToDo()
                        {
                            Title = dataRow.GetCell(1).ToString(),
                            IsCompleted = Convert.ToBoolean(dataRow.GetCell(2).ToString()),
                            Owner = Convert.ToInt32(dataRow.GetCell(3).ToString())
                        };

                        var sqlConnection = new SqlConnection(RDSConnectionString);
                        sqlConnection.Open();

                        using (var command = sqlConnection.CreateCommand())
                        {
                            command.CommandType = CommandType.StoredProcedure;
                            command.CommandText = "INSERT_TODOS";

                            SqlParameter Id = new SqlParameter();
                            Id.ParameterName = "@Id";
                            Id.SqlDbType = SqlDbType.Int;
                            Id.Direction = ParameterDirection.Output;
                            command.Parameters.Add(Id);

                            SqlParameter Title = new SqlParameter();
                            Title.ParameterName = "@Title";
                            Title.SqlDbType = SqlDbType.NVarChar;
                            Title.Direction = ParameterDirection.Input;
                            Title.Value = toDo.Title;
                            command.Parameters.Add(Title);

                            SqlParameter IsComplete = new SqlParameter();
                            IsComplete.ParameterName = "@IsCompleted";
                            IsComplete.SqlDbType = SqlDbType.Bit;
                            IsComplete.Direction = ParameterDirection.Input;
                            IsComplete.Value = toDo.IsCompleted;
                            command.Parameters.Add(IsComplete);
                            
                            SqlParameter Owner = new SqlParameter();
                            Owner.ParameterName = "@Owner";
                            Owner.SqlDbType = SqlDbType.Int;
                            Owner.Direction = ParameterDirection.Input;
                            Owner.Value = toDo.Owner;
                            command.Parameters.Add(Owner);
                            
                            command.ExecuteNonQuery();

                            toDo.Id = Convert.ToInt32(Id.Value);
                            sqlConnection.Close();
                        }
                    }

                    await Put(s3Event.Bucket.Name, s3Event.Object.Key, workbook);
                }
                context.Logger.LogInformation(response.Headers.ContentType);
                

            }
            catch (Exception e)
            {
                context.Logger.LogError($"Error getting object {s3Event.Object.Key} from bucket {s3Event.Bucket.Name}. Make sure they exist and your bucket is in the same region as this function.");
                context.Logger.LogError(e.Message);
                context.Logger.LogError(e.StackTrace);
                throw;
            }
        }
    }

    static async Task<bool> Put(string bucket, string key, XSSFWorkbook workBook)
    {
        try
        {
            var S3client = new AmazonS3Client();

            bool Bucket = await AmazonS3Util.DoesS3BucketExistV2Async(S3client, bucket);
            if (!Bucket)
            {
                var bucketRequest = new PutBucketRequest()
                {
                    BucketName = bucket,
                    UseClientRegion = true,
                };

                await S3client.PutBucketAsync(bucketRequest);
            }

            using (var stream = new MemoryStream())
            {
                ISheet sheet = workBook.GetSheetAt(0);
                int rowCount = sheet.PhysicalNumberOfRows;

                for (int row = 1; row < rowCount; row++)
                {
                    IRow dataRow = sheet.GetRow(row);

                    dataRow.GetCell(1).SetCellValue(dataRow.GetCell(1).ToString() + "Data is being updated") ;
                }
                workBook.Write(stream, true);
                
                var ObjectRequest = new PutObjectRequest()
                {
                    BucketName = bucket,
                    Key = "Processed_" + key,
                    InputStream = stream
                };

                await S3client.PutObjectAsync(ObjectRequest);            
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception in PutS3Object:" + ex.Message);
            return false;
        }
    }
}