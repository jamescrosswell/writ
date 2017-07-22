using Xunit;

namespace Writ.Messaging.Tests
{
    public class SchemaTypeMapTests
    {
        private class FooOne { }

        private class FooTwo { }

        private sealed class Fixture
        {
            public SpecificSchema FooOneSchemaV1 => new SpecificSchema("SchemaTests.Foo1", 1);
            public SpecificSchema FooOneSchemaV2 => new SpecificSchema("SchemaTests.Foo1", 2);
            public SpecificSchema FooTwoSchemaV1 => new SpecificSchema("SchemaTests.Foo2", 1);

            public SchemaTypeMap GetSut()
            {
                var sut = new SchemaTypeMap();
                sut.RegisterTypeSchema<FooOne>(FooOneSchemaV1);
                sut.RegisterTypeSchema<FooOne>(FooOneSchemaV2);
                sut.RegisterTypeSchema<FooTwo>(FooTwoSchemaV1);
                return sut;
            }
        }

        private readonly Fixture _fixture = new Fixture();

        [Fact]
        public void TypeIndex_ReturnsCorrectSchema()
        {
            var sut = _fixture.GetSut();

            var fooOneSchema = sut.GetSchema(typeof(FooOne));
            Assert.Equal(_fixture.FooOneSchemaV2, fooOneSchema);

            var fooTwoSchema = sut.GetSchema(typeof(FooTwo));
            Assert.Equal(_fixture.FooTwoSchemaV1, fooTwoSchema);
        }

        [Fact]
        public void SchemaIndex_ReturnsCorrectType()
        {
            var sut = _fixture.GetSut();

            var result = sut.GetType(_fixture.FooOneSchemaV1);
            Assert.Equal(typeof(FooOne), result);

            result = sut.GetType(_fixture.FooOneSchemaV2);
            Assert.Equal(typeof(FooOne), result);

            result = sut.GetType(_fixture.FooTwoSchemaV1);
            Assert.Equal(typeof(FooTwo), result);
        }
    }
}
