using AutoMapper;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using Shouldly;

namespace Automapper.Extensions.ExpressionMapping.EFCore.UnitTests
{
    public class ExpressionMappingWithDbSet
    {
        [Fact]
        public async void When_Apply_Where_Clause_Over_Queryable_ToListAsync()
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

            var queryable = context.DbModel.AsQueryable<Model>();

            Expression<Func<DTO, bool>> expOverDTO = (dto) => dto.Nested.AnotherBoolean;

            // Act
            var result = await queryable
                .UseAsAsyncDataSource(mapper)
                .For<DTO>()
                .Where(expOverDTO)
                .ToListAsync();//.FirstOrDefaultAsync();

            // Assert
            result.ShouldNotBeNull();
            /*result.Count.ShouldBe(2);
            result.ShouldAllBe(expOverDTO);*/
        }
        [Fact]
        public async void When_Apply_Where_Clause_Over_Queryable_FirstOrDefaultAsync()
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

            var queryable = context.DbModel.AsQueryable<Model>();

            Expression<Func<DTO, bool>> expOverDTO = (dto) => dto.Nested.AnotherBoolean;

            // Act
            var result = await queryable
                .UseAsAsyncDataSource(mapper)
                .For<DTO>()
                .Where(expOverDTO)
                .FirstOrDefaultAsync();

            // Assert
            result.ShouldNotBeNull();
            /*result.Count.ShouldBe(2);
            result.ShouldAllBe(expOverDTO);*/
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
    }
}