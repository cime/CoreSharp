namespace CoreSharp.Breeze
{
    public interface IClientModel
    {
        long Id { get; set; }

        bool IsNew();
    }
}
