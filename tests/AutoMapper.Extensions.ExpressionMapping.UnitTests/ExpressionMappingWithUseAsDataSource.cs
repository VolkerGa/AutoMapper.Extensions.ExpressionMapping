namespace AutoMapper.Extensions.ExpressionMapping.UnitTests
{
    using AutoMapper;
    using AutoMapper.QueryableExtensions;
    using Shouldly;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

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
        public void When_Apply_Where_Clause_Over_Queryable_With_Enum_As_Data_Source()
        {
            // Arrange
            var mapper = CreateMapper();

            var models = new List<EnumModel>()
            {
                new EnumModel { Enum = ModelEnum.One },
                new EnumModel { Enum = ModelEnum.Two },
                new EnumModel { Enum = ModelEnum.One },
                new EnumModel { Enum = ModelEnum.Two }
            };

            var queryable = models.AsQueryable();

            // Act
            var query = queryable
                .UseAsDataSource(mapper)
                .For<DTOwithEnum>()
                .Where(d => d.Enum == DTOEnum.One);
            var expression = query.Expression;
            var result = query
                .ToList();

            // Assert
            result.ShouldNotBeNull();
            result.Count.ShouldBe(2);
            result.ShouldAllBe(d => d.Enum == DTOEnum.One);
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

                cfg.CreateMap<DTOwithEnum, EnumModel>();
                cfg.CreateMap<EnumModel, DTOwithEnum>();
                    /*.ForMember(d => d.Enum, opt => opt.MapFrom(s => s.Enum == ModelEnum.One ? DTOEnum.One :
                    (s.Enum == ModelEnum.Two ? DTOEnum.Two : DTOEnum.Two)));*/
                cfg.CreateMap<ModelEnum, DTOEnum>()
                .ConvertUsing(dst => dst == ModelEnum.One ? DTOEnum.One :
                    (dst == ModelEnum.Two ? DTOEnum.Two : DTOEnum.Two));
            });

            var mapper = mapperConfig.CreateMapper();
            return mapper;
        }

        private class DTO
        {
            public class DTONested
            {
                public bool AnotherBoolean { get; set; }
            }

            public DTONested Nested { get; set; }
        }

        private class Model
        {
            public bool ABoolean { get; set; }
        }


        private class GenericModel<T>
        {
            public T ABoolean { get; set; }
        }

        private enum DTOEnum
        {
            One = 1,
            Two = 2
        }

        private class DTOwithEnum
        {
            public DTOEnum Enum { get; set; }
        }

        private enum ModelEnum
        {
            One = 11,
            Two = 12
        }

        private class EnumModel
        {
            public ModelEnum Enum { get; set; }
        }


    }
}
