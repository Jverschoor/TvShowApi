using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Hosting;
using TvShows.Infrastructure;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration["ConnectionStrings:DefaultConnection"]);

builder.Build().Run();
