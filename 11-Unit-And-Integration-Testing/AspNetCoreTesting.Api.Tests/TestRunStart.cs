using AspNetCoreTesting.Api.Data;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;
using Xunit.Sdk;

[assembly: Xunit.TestFramework("AspNetCoreTesting.Api.Tests.TestRunStart", "AspNetCoreTesting.Api.Tests")]

namespace AspNetCoreTesting.Api.Tests
{
    public class TestRunStart : XunitTestFramework
    {
        public TestRunStart(IMessageSink messageSink) : base(messageSink)
        {
            var options = new DbContextOptionsBuilder<ApiContext>()
                                .UseSqlServer("Data Source=localhost,1434;Integrated Security=False;Database=TestingDemo;User ID=SA;Password=yourStrong(!)Password;TrustServerCertificate=True;");
            var dbContext = new ApiContext(options.Options);
            dbContext.Database.EnsureCreated();
            dbContext.Database.Migrate();
        }
    }
}
