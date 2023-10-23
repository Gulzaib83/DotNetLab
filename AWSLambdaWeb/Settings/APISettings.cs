namespace AWSLambdaWeb.Settings
{
    public class APISettings
    {
        public string ApiBaseUrl { get; }

        public APISettings(IConfiguration configuration)
        {
            ApiBaseUrl = configuration.GetSection("ApiSettings:ApiBaseUrl").Value;
        }
    }
}
