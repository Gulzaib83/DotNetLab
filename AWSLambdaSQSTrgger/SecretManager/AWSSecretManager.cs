using Amazon;
using Amazon.Internal;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdaSQSTrgger.SecretManager
{
    public class AWSSecretManager
    {
        public static async Task<string> GetSecret(string secretName)
        {
            var request = new GetSecretValueRequest
            {
                SecretId = secretName,
                VersionStage = "AWSCURRENT" 
            };

            string secretString;

            using (var client =
            new AmazonSecretsManagerClient(Amazon.RegionEndpoint.USEast1))
            {
                var response = await client.GetSecretValueAsync(request);

                if(response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    if (response.SecretString != null)
                    {
                        secretString = response.SecretString;
                    }
                    else
                    {
                        var memoryStream = response.SecretBinary;
                        var reader = new StreamReader(memoryStream);
                        secretString =
                System.Text.Encoding.UTF8
                    .GetString(Convert.FromBase64String(reader.ReadToEnd()));
                    }

                    return secretString;
                }
                return string.Empty; 
                
            }

        }
    }
}
