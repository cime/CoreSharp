namespace CoreSharp.Cqrs.Query
{
    /// <summary>
    /// Query handler for <typeparam name="TQuery" />
    /// </summary>
    /// <typeparam name="TQuery"> query</typeparam>
    /// <typeparam name="TResult">Return type of query</typeparam>
    public interface IQueryHandler<in TQuery, out TResult> where TQuery : IQuery<TResult>
    {
        /// <summary>
        /// Handle <typeparam name="TQuery" /> query
        /// </summary>
        /// <param name="query"><typeparamref name="TQuery"/> command</param>
        /// <returns><typeparamref name="TResult"/></returns>
        TResult Handle(TQuery query);
    }
}
