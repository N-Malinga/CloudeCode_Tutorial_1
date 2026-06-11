namespace ProductManagement.Application.Common.Exceptions;

public sealed class NotFoundException : AppException
{
    public string EntityName { get; }
    public object Key { get; }

    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.")
    {
        EntityName = entityName;
        Key = key;
    }

    public override int StatusCode => 404;
    public override string Title => "Resource not found";
}
