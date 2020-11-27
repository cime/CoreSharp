namespace CoreSharp.DataAccess
{
    public interface ICodeList : IEntity<string>
    {
        bool Active { get; set; }
    }
}
