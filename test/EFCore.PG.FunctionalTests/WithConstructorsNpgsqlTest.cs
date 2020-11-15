using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;

namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class WithConstructorsNpgsqlTest : WithConstructorsTestBase<WithConstructorsNpgsqlTest.WithConstructorsNpgsqlFixture>
    {
        public WithConstructorsNpgsqlTest(WithConstructorsNpgsqlFixture fixture)
            : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class WithConstructorsNpgsqlFixture : WithConstructorsFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => NpgsqlTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

#pragma warning disable CS0618 // Type or member is obsolete
                modelBuilder.Entity<BlogQuery>().HasNoKey().ToQuery(
                    () => context.Set<BlogQuery>().FromSqlRaw(@"SELECT * FROM ""Blog"""));
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }
}
