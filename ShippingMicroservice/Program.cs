using ShippingMicroservice.DataAccess;
using static Google.Cloud.Firestore.V1.StructuredQuery.Types;

string projectId = "distributedprogramming-386320";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<FirebaseShipingRepo>(provider => new FirebaseShipingRepo(projectId));

var environment = builder.Services.BuildServiceProvider().GetRequiredService<IWebHostEnvironment>();

string credential_path = System.IO.Path.Combine(environment.ContentRootPath, "distributedprogramming-386320-7cd52fa89f04.json");

System.Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential_path);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
