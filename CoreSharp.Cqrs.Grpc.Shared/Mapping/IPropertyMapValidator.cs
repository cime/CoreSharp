namespace CoreSharp.Cqrs.Grpc.Mapping
{
    public interface IPropertyMapValidator
    {
        bool MapProperty(object obj, string propName);
    }
}
