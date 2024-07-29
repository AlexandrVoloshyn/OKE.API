using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using OKE.API.Extensions;
using OKE.API.Filters;
using OKE.API.Middlewares;
using OKE.Database;
using OKE.Database.Repositories;
using OKE.Domain.Repositories;
using Serilog;
using Serilog.Extensions.Logging;
using StackExchange.Redis;
using static OKE.Application.Handlers.Movies.List;
using static System.Net.Mime.MediaTypeNames;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Host.UseSerilog((ctx, lc) => lc
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}")
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(ctx.Configuration));

    builder.Services.AddValidatorsFromAssembly(typeof(Query).Assembly);
    builder.Services.AddHealthChecks();

    builder.Services.AddMediatR(config =>
    {
        config.AddOpenBehavior(typeof(ValidationBehavior<,>));
        config.RegisterServicesFromAssembly(typeof(Query).Assembly);
    });

    builder.Services.AddDbContext<Context>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

    builder.Services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(
            ConfigurationOptions.Parse(builder.Configuration.GetConnectionString("Redis"))));

    builder.Services.AddScoped<IMovieRepository, MovieRepository>();
    builder.Services.AddScoped<IActorRepository, ActorRepository>();

    builder.Services.AddControllers(opt =>
    {
        opt.Filters.Add(typeof(ResultFilter));
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Version = "v1",
            Title = "OKE Movies Api"
        });
    });

    var app = builder.Build();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            
            exceptionHandlerApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                context.Response.ContentType = Text.Plain;

                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerFeature>();

                if (exceptionHandlerPathFeature?.Error != null)
                {
                    Log.Error(exceptionHandlerPathFeature.Error, "An unhandled exception has occurred.");
                }

                await context.Response.WriteAsync("Whoops. Something went wrong. Contact an administrator of resource.");
            });
        });
    }

    app.UseHttpsRedirection();

    app.UseAuthorization();
    app.MapHealthChecks("/health");

    app.MapControllers();

    await app.MigrateAsync();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}