using customerMicroservice.DataAccess;
using Google.Cloud.Firestore.V1;
using static Google.Cloud.Firestore.V1.StructuredQuery.Types;


string projectId = "distributedprogramming-386320";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();

builder.Services.AddScoped<FirestoreUsersRepo>(provider => new FirestoreUsersRepo(projectId));

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
