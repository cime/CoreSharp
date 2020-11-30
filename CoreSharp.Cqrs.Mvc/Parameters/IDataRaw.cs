namespace CoreSharp.Cqrs.Mvc.Parameters
{
    internal interface IDataRaw<TData>
    {
        TData MapToData();
    }
}
