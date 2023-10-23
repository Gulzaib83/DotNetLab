using Amazon;
using AWSLambdaAPI.AWS;
using ExternalEntities.Misc;
using Repository.Interfaces;
using System.Drawing;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration(((_, configurationBuilder) =>
{
    configurationBuilder.AddAmazonSecretsManager("us-east-1", "MyAWSSecrets");
}
));
builder.Services.Configure<AWSSecretsOptions>(builder.Configuration);
// Add services to the container.
builder.Services.Configure<DBSettings>(builder.Configuration.GetSection("ConnectionStrings"));

//builder.Services.AddOptions<AWSSecretsOptions>().BindConfiguration("AWSSecrets");
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

builder.Services.AddTransient<ConnectionManagement.ConnectionManager>();
builder.Services.AddTransient<IToDoRepository, DbOperations_ADO.DbOperations>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
