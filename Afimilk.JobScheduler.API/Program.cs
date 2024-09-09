
using Afimilk.JobScheduler.API;
using Afimilk.JobScheduler.BL;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddJobSchedulerBL();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.


AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    c.SchemaFilter<TimeSpanSchemaFilter>(); // Register the schema filter
});

var app = builder.Build();

app.Services.InitializeDatabase(); // This will ensure the database is created

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
