namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection.Metadata;
    using System.Threading.Tasks;
    using Xunit;

    public class ModelContext : DbContext
    {
        public ModelContext() { }
        public ModelContext(DbContextOptions<ModelContext> options) : base(options) { }
        public DbSet<ExpressionMappingWithUseAsDataSource.Model> Model => Set<ExpressionMappingWithUseAsDataSource.Model>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ExpressionMappingWithUseAsDataSource.Model>()
                .HasKey(m => m.Id);
        }
    }

    public class ExpressionMappingWithUseAsDataSource
    {
        [Fact]
        public void When_Apply_Where_Clause_Over_Queryable_As_Data_Source()
        {
            // Arrange
            var mapper = CreateMapper();

            var models = new List<Model>()
            {
                new Model { ABoolean = true },
                new Model { ABoolean = false },
                new Model { ABoolean = true },
                new Model { ABoolean = false }
            };

            var queryable = models.AsQueryable();

            Expression<Func<DTO, bool>> expOverDTO = (dto) => dto.Nested.AnotherBoolean;

            // Act
            var result = queryable
                .UseAsDataSource(mapper)
                .For<DTO>()
                .Where(expOverDTO)
                .ToList();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result.ShouldAllBe(expOverDTO);
        }

        [Fact]
        public async void When_Apply_Where_Clause_Over_Queryable_As_Data_Source_Async()
        {
            // Arrange
            var mapper = CreateMapper();

            // Create and open a connection. This creates the SQLite in-memory database, which will persist until the connection is closed
            // at the end of the test (see Dispose below).
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            // These options will be used by the context instances in this test suite, including the connection opened above.
            var contextOptions = new DbContextOptionsBuilder<ModelContext>()
                .UseSqlite(connection)
                .Options;

            // Create the schema and seed some data
            using var context = new ModelContext(contextOptions);

            context.Database.EnsureCreated();
            context.AddRange(
                new Model { ABoolean = true },
                new Model { ABoolean = false },
                new Model { ABoolean = true },
                new Model { ABoolean = false });
            context.SaveChanges();

            var queryable = context.Model.AsQueryable<Model>();

            Expression<Func<DTO, bool>> expOverDTO = (dto) => dto.Nested.AnotherBoolean;

            // Act
            var result = await queryable
                .UseAsDataSource(mapper)
                .For<DTO>()
                .Where(expOverDTO)
                .ToListAsync();//.FirstOrDefaultAsync();

            // Assert
            result.ShouldNotBeNull();
            /*result.Count.ShouldBe(2);
            result.ShouldAllBe(expOverDTO);*/
        }

        [Fact]
        public void Should_Map_From_Generic_Type()
        {
            // Arrange
            var mapper = CreateMapper();

            var models = new List<GenericModel<bool>>()
            {
                new GenericModel<bool> {ABoolean = true},
                new GenericModel<bool> {ABoolean = false},
                new GenericModel<bool> {ABoolean = true},
                new GenericModel<bool> {ABoolean = false}
            };

            var queryable = models.AsQueryable();

            Expression<Func<DTO, bool>> expOverDTO = (dto) => dto.Nested.AnotherBoolean;

            // Act
            var q = queryable.UseAsDataSource(mapper).For<DTO>().Where(expOverDTO);

            var result = q.ToList();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result.ShouldAllBe(expOverDTO);
        }

        private static IMapper CreateMapper()
        {
            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Model, DTO>()
                    .ForMember(d => d.Nested, opt => opt.MapFrom(s => s));
                cfg.CreateMap<Model, DTO.DTONested>()
                    .ForMember(d => d.AnotherBoolean, opt => opt.MapFrom(s => s.ABoolean));
                cfg.CreateMap<DTO, Model>()
                    .ForMember(d => d.ABoolean, opt => opt.MapFrom(s => s.Nested.AnotherBoolean));
                cfg.CreateMap<GenericModel<bool>, DTO>()
                    .ForMember(d => d.Nested, opt => opt.MapFrom(s => s));
                cfg.CreateMap<GenericModel<bool>, DTO.DTONested>()
                    .ForMember(d => d.AnotherBoolean, opt => opt.MapFrom(s => s.ABoolean));
                cfg.CreateMap(typeof(Task<>), typeof(Task<>));
            });

            var mapper = mapperConfig.CreateMapper();
            return mapper;
        }

        public class DTO
        {
            public class DTONested
            {
                public bool AnotherBoolean { get; set; }
            }
            public int Id { get; set; }

            public DTONested Nested { get; set; }
        }

        public class Model
        {
            public int Id { get; set; }
            public bool ABoolean { get; set; }
        }


        private class GenericModel<T>
        {
            public T ABoolean { get; set; }
        }
    }
}
