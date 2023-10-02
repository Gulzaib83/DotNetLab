using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdaS3Trigger
{
    internal class SecretManagerUtility
    {
        private readonly IAmazonSecretsManager SecretsManager;

        public SecretManagerUtility()
        {
            SecretsManager = new AmazonSecretsManagerClient();
        }

        public async Task<string> GetSecretValue(string secretName)
        {
            var request = new GetSecretValueRequest
            {
                SecretId = secretName
            };

            var response = await SecretsManager.GetSecretValueAsync(request);
            return response.SecretString;
        }
    }
}
