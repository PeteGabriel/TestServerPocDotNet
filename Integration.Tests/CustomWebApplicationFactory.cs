using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Repository;
using Repository.model;

namespace Integrate.Tests
{
    public class CustomWebApplicationFactory
        : WebApplicationFactory<Startup>
    {

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Create a new service provider.
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                // Add a database context (ApplicationDbContext) using an in-memory 
                // database for testing.
                services.AddDbContext<AppDbContext>(options => 
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                    options.UseInternalServiceProvider(serviceProvider);
                });

                // Build the service provider.
                var sp = services.BuildServiceProvider();

                // Create a scope to obtain a reference to the database
                // context (ApplicationDbContext).
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<AppDbContext>();
                   
                    // Ensure the database is created.
                    db.Database.EnsureCreated();

                    try
                    {
                        // Seed the database with test data.
                        db.ShortenedUrls.Add(new ShortenedUrl
                        {
                            Url = "www.youtube.com",
                            ShortCode = "Example"
                        });
                        
                        db.ShortenedUrls.Add(new ShortenedUrl
                        {
                            Url = "www.example.com",
                            ShortCode = "example"
                        });
                        db.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        // TODO log
                    }
                }
            });
        }
    }
}