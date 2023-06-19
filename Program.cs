using CosmosDemo.Services;
using System.Configuration;

class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var cosmosDbSection = builder.Configuration.GetSection("CosmosDb");
        var cosmosDbService = InitializeCosmosClientInstanceAsync(cosmosDbSection).GetAwaiter().GetResult();
        builder.Services.AddSingleton<ICosmosDbService>(cosmosDbService);

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
    }

    private static async Task<CosmosDbService> InitializeCosmosClientInstanceAsync(IConfigurationSection configurationSection)
    {
        var databaseName = configurationSection["DatabaseName"];
        var containerName = configurationSection["ContainerName"];
        var account = configurationSection["Account"];
        var key = configurationSection["Key"];

        var client = new Microsoft.Azure.Cosmos.CosmosClient(account, key);
        var database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
        await database.Database.CreateContainerIfNotExistsAsync(containerName, "/id");

        var cosmosDbService = new CosmosDbService(client, databaseName, containerName);
        return cosmosDbService;
    }
}