using GraphQL.Types;

namespace MyDemo.Models.Request
{
    public class HelloWorldQuery : ObjectGraphType
    {
        public HelloWorldQuery()
        {
            Field<StringGraphType>(
                name: "hello",
                resolve: context => "world"
            );
        }
    }
}
