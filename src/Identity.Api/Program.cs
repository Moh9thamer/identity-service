using Identity.Api.Extensions;
using Identity.Api.Middleware;
using Identity.Application;
using Identity.Infrastructure;
using Scalar.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApiWithSecurity();
builder.Services.AddApplicationServices();
builder.Services.AddPersistenceServices(builder.Configuration);
builder.Services.AddJwtAuthentication(builder.Configuration);


builder.Services.AddControllers();

var app = builder.Build();

await app.Services.ApplyMigrationsAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}


app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();