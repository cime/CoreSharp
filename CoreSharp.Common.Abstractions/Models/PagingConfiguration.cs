namespace CoreSharp.Common.Models
{
    public class PagingConfiguration
    {
        public int MaxTake { get; set; } = 10000;

        public int MaxSkip { get; set; } = int.MaxValue - 10000;
    }
}
