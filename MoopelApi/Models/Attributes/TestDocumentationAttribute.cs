using System.Net;

namespace MoopelApi.Models.Attributes;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class TestDocumentationAttribute : Attribute
{
    public TestDocumentationAttribute(
        string description,
        HttpStatusCode expectedResult)
    {
        Description = description;
        ExpectedResult = expectedResult;
    }

    public string Description { get; }

    public HttpStatusCode ExpectedResult { get; }
}
