using Azure.Identity;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TestWebK8s;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContextTest>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHealthChecksUI().AddInMemoryStorage();
var kvUri = "https://KV-AEU-ECP-DEV-TRUE.vault.azure.net";

builder.Services.AddHealthChecks()
    .AddSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),name:"SqlServer")
    .AddCheck<MyHealthCheck>("MyHealthCheckRandom")
    .AddDbContextCheck<DbContextTest>()
    .AddAzureServiceBusQueue("SB-AEU-ECP-DEV-TRUE.servicebus.windows.net",  "deadletter", new DefaultAzureCredential(), "Service Bus - Queue")
    .AddAzureBlobStorage(new Uri("https://saaeuecpdevtrue.blob.core.windows.net"), new DefaultAzureCredential())
    .AddAzureQueueStorage(new Uri("https://saaeuecpdevtrue.queue.core.windows.net"), new DefaultAzureCredential())
    .AddAzureKeyVault(new Uri(kvUri), 
        new DefaultAzureCredential(),
        options => 
        { 
            options.AddSecret("MsiSqlConnectionString"); 
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
