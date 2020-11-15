﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.TestUtilities;
using Xunit;

// ReSharper disable StringLiteralTypo
namespace Npgsql.EntityFrameworkCore.PostgreSQL
{
    public class ConnectionSpecificationTest
    {
        [Fact]
        public void Can_specify_connection_string_in_OnConfiguring()
        {
            var serviceProvider = new ServiceCollection()
                .AddDbContext<StringInOnConfiguringContext>()
                .BuildServiceProvider();

            using var _ = NpgsqlTestStore.GetNorthwindStore();
            using var context = serviceProvider.GetRequiredService<StringInOnConfiguringContext>();

            Assert.True(context.Customers.Any());
        }

        [Fact]
        public void Can_specify_connection_string_in_OnConfiguring_with_default_service_provider()
        {
            using var _ = NpgsqlTestStore.GetNorthwindStore();
            using var context = new StringInOnConfiguringContext();

            Assert.True(context.Customers.Any());
        }

        class StringInOnConfiguringContext : NorthwindContextBase
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseNpgsql(NpgsqlTestStore.NorthwindConnectionString, b => b.ApplyConfiguration());
        }

        [Fact]
        public void Can_specify_connection_in_OnConfiguring()
        {
            var serviceProvider = new ServiceCollection()
                .AddScoped(p => new NpgsqlConnection(NpgsqlTestStore.NorthwindConnectionString))
                .AddDbContext<ConnectionInOnConfiguringContext>().BuildServiceProvider();

            using var _ = NpgsqlTestStore.GetNorthwindStore();
            using var context = serviceProvider.GetRequiredService<ConnectionInOnConfiguringContext>();

            Assert.True(context.Customers.Any());
        }

        [Fact]
        public void Can_specify_connection_in_OnConfiguring_with_default_service_provider()
        {
            using var _ = NpgsqlTestStore.GetNorthwindStore();
            using var context = new ConnectionInOnConfiguringContext(new NpgsqlConnection(NpgsqlTestStore.NorthwindConnectionString));

            Assert.True(context.Customers.Any());
        }

        class ConnectionInOnConfiguringContext : NorthwindContextBase
        {
            readonly NpgsqlConnection _connection;

            public ConnectionInOnConfiguringContext(NpgsqlConnection connection)
                => _connection = connection;

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseNpgsql(_connection, b => b.ApplyConfiguration());

            public override void Dispose()
            {
                _connection.Dispose();
                base.Dispose();
            }
        }

        // ReSharper disable once UnusedMember.Local
        class StringInConfigContext : NorthwindContextBase
        {
            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseNpgsql("Database=Crunchie", b => b.ApplyConfiguration());
        }

        [Fact]
        public void Throws_if_no_connection_found_in_config_without_UseNpgsql()
        {
            var serviceProvider = new ServiceCollection()
                .AddDbContext<NoUseNpgsqlContext>().BuildServiceProvider();

            using var context = serviceProvider.GetRequiredService<NoUseNpgsqlContext>();

            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Customers.Any()).Message);
        }

        [Fact]
        public void Throws_if_no_config_without_UseNpgsql()
        {
            var serviceProvider = new ServiceCollection()
                .AddDbContext<NoUseNpgsqlContext>().BuildServiceProvider();
            using var context = serviceProvider.GetRequiredService<NoUseNpgsqlContext>();

            Assert.Equal(
                CoreStrings.NoProviderConfigured,
                Assert.Throws<InvalidOperationException>(() => context.Customers.Any()).Message);
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        class NoUseNpgsqlContext : NorthwindContextBase
        {
        }

        [Fact]
        public void Can_depend_on_DbContextOptions()
        {
            var serviceProvider = new ServiceCollection()
                .AddScoped(p => new NpgsqlConnection(NpgsqlTestStore.NorthwindConnectionString))
                .AddDbContext<OptionsContext>()
                .BuildServiceProvider();

            using var _ = NpgsqlTestStore.GetNorthwindStore();
            using var context = serviceProvider.GetRequiredService<OptionsContext>();

            Assert.True(context.Customers.Any());
        }

        [Fact]
        public void Can_depend_on_DbContextOptions_with_default_service_provider()
        {
            using var _ = NpgsqlTestStore.GetNorthwindStore();
            using var context = new OptionsContext(
                new DbContextOptions<OptionsContext>(),
                new NpgsqlConnection(NpgsqlTestStore.NorthwindConnectionString));

            Assert.True(context.Customers.Any());
        }

        class OptionsContext : NorthwindContextBase
        {
            readonly NpgsqlConnection _connection;
            readonly DbContextOptions<OptionsContext> _options;

            public OptionsContext(DbContextOptions<OptionsContext> options, NpgsqlConnection connection)
                : base(options)
            {
                _options = options;
                _connection = connection;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                Assert.Same(_options, optionsBuilder.Options);

                optionsBuilder.UseNpgsql(_connection, b => b.ApplyConfiguration());

                Assert.NotSame(_options, optionsBuilder.Options);
            }

            public override void Dispose()
            {
                _connection.Dispose();
                base.Dispose();
            }
        }

        [Fact]
        public void Can_depend_on_non_generic_options_when_only_one_context()
        {
            var serviceProvider = new ServiceCollection()
                .AddDbContext<NonGenericOptionsContext>()
                .BuildServiceProvider();

            using var _ = NpgsqlTestStore.GetNorthwindStore();
            using var context = serviceProvider.GetRequiredService<NonGenericOptionsContext>();

            Assert.True(context.Customers.Any());
        }

        [Fact]
        public void Can_depend_on_non_generic_options_when_only_one_context_with_default_service_provider()
        {
            using var _ = NpgsqlTestStore.GetNorthwindStore();
            using var context = new NonGenericOptionsContext(new DbContextOptions<DbContext>());

            Assert.True(context.Customers.Any());
        }

        class NonGenericOptionsContext : NorthwindContextBase
        {
            readonly DbContextOptions _options;

            public NonGenericOptionsContext(DbContextOptions options)
                : base(options)
                => _options = options;

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                Assert.Same(_options, optionsBuilder.Options);

                optionsBuilder.UseNpgsql(NpgsqlTestStore.NorthwindConnectionString, b => b.ApplyConfiguration());

                Assert.NotSame(_options, optionsBuilder.Options);
            }
        }

        class NorthwindContextBase : DbContext
        {
            protected NorthwindContextBase()
            {
            }

            protected NorthwindContextBase(DbContextOptions options)
                : base(options)
            {
            }

            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public DbSet<Customer> Customers { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
                => modelBuilder.Entity<Customer>(b =>
                {
                    b.HasKey(c => c.CustomerId);
                    b.ToTable("Customers");
                });
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        class Customer
        {
            // ReSharper disable once UnusedAutoPropertyAccessor.Local
            public string CustomerId { get; set; }
            // ReSharper disable once UnusedMember.Local
            public string CompanyName { get; set; }
            // ReSharper disable once UnusedMember.Local
            public string Fax { get; set; }
        }

        #region Added for Npgsql

        [Fact]
        public void Can_specify_connection_in_OnConfiguring_and_create_master_connection()
        {
            using var conn = new NpgsqlConnection(NpgsqlTestStore.NorthwindConnectionString);

            conn.Open();

            using var _ = NpgsqlTestStore.GetNorthwindStore();
            using var context = new ConnectionInOnConfiguringContext(conn);
            var relationalConn = context.GetService<INpgsqlRelationalConnection>();
            using var masterConn = relationalConn.CreateMasterConnection();

            masterConn.Open();
        }

        #endregion
    }
}