var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

var app = builder.Build();

// Add middleware which will be used in ridge tests
if (app.WasApplicationCreatedFromTest())
{
    app.RethrowExceptionInsteadOfReturningHttpResponse();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
