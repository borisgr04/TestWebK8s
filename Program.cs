using Azure.Identity;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;
using TestWebK8s;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var storageAccount = builder.Configuration["storageaccount"];
var servicebus = builder.Configuration["servicebus"];
var keyvaultName = builder.Configuration["keyvault"];
var secret = builder.Configuration["secret"];
var connectionStringSql = builder.Configuration.GetConnectionString("DefaultConnection");


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContextTest>(options =>
    options.UseSqlServer(connectionStringSql));

builder.Services.AddHealthChecksUI().AddInMemoryStorage();


builder.Services.AddHealthChecks()
    .AddSqlServer(connectionStringSql, name:"SqlServer", healthQuery: "SELECT count(*) from [Admin].[Category]")
    .AddCheck<MyHealthCheck>("MyHealthCheckRandom")
    .AddDbContextCheck<DbContextTest>(customTestQuery:
            (db, cancel) => Task.FromResult(db.Category.Any()))
    .AddAzureServiceBusQueue($"{servicebus}.servicebus.windows.net",  "deadletter", new DefaultAzureCredential(), "Service Bus - Queue")
    .AddAzureBlobStorage(new Uri($"https://{storageAccount}.blob.core.windows.net"), new DefaultAzureCredential())
    .AddAzureQueueStorage(new Uri($"https://{storageAccount}.queue.core.windows.net"), new DefaultAzureCredential())
    .AddAzureKeyVault(new Uri($"https://{keyvaultName}.vault.azure.net"), 
        new DefaultAzureCredential(),
        options => 
        { 
            options.AddSecret(secret); 
        }, 
        name:"Key Vault")
    .AddApplicationInsightsPublisher();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecksUI();
app.MapControllers();

app.Run();
