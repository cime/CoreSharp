namespace System.Threading
{
    public static class CancellationTokenExtensions
    {

        public static CancellationToken JoinToken(this CancellationToken token, CancellationToken joinToken)
        {
            var tokens = new CancellationToken[] { token, joinToken };
            var store = CancellationTokenSource.CreateLinkedTokenSource(tokens);
            return store.Token;

        }

        public static CancellationToken AddTimeout(this CancellationToken token, int timeoutMs)
        {
            if(timeoutMs <= 0)
            {
                return token;
            }

            var timeoutStore = new CancellationTokenSource(timeoutMs);
            return timeoutStore.Token.JoinToken(token);
        }

    }
}
