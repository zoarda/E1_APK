namespace Games.Coresdk.Unity
{
    public class LogoutResult
    {
        public System.Exception Exception { get; private set; }

        public LogoutResult()
        {
        }

        public LogoutResult(System.Exception exception)
        {
            Exception = exception;
        }
    }
}
