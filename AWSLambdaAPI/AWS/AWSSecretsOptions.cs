namespace AWSLambdaAPI.AWS
{
    public class AWSSecretsOptions
    {
        public string AccessKey { set; get; }

        public string SecretKey { set; get; }

        public string SQSARN { set; get; }

        public string SNSARN { set; get; }
    }
}
