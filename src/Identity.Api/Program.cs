using Identity.Api.Extensions;
using Identity.Application;
using Identity.Infrastructure;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();


builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);


builder.Services.AddControllers();

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();