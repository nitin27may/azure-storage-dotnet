using AzureBlobApi.Middleware;
using AzureBlobApi.Services;
using AzureStorageApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    // Bind both HTTP and HTTPS ports as defined in applicationUrl
    serverOptions.ListenAnyIP(5149); // HTTP
    serverOptions.ListenAnyIP(7149, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
    });
    // serverOptions.Limits.MaxRequestBodySize = 10L * 1024 * 1024 * 1024; // 10 GB
    serverOptions.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(10);
    serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(10);
});
//builder.Services.Configure<FormOptions>(options =>
//{
//    options.MultipartBodyLengthLimit = 10L * 1024 * 1024 * 1024; // 10 GB
//});


// Add services to the container.
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<ITableStorageService, TableStorageService>();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IQueueStorageService, QueueStorageService>();

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new RequestSizeLimitAttribute(long.MaxValue)); // Unlimited request size
});
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200") // Angular dev server
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
