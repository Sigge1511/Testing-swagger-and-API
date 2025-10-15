using apiv4.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace apiv4.Data
{
    public class ApiContext :IdentityDbContext<ApiUser>
    {
        //public string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=apitester;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";

        public ApiContext(DbContextOptions<ApiContext> options):base(options)
        {
        }
        public DbSet<Book> BookSet { get; set; } = default!;
        public DbSet<ApiUser> ApiUserSet { get; set; } = default!;

    }
}
