namespace Games.Coresdk.Unity
{
    public class RawResponse
    {
        public int StatusCode { get; private set; }

        public string Data { get; private set; }

        public System.Exception Exception { get; private set; }

        public RawResponse(int statusCode, string data, string exception)
        {
            StatusCode = statusCode;

            Data = data;

            if (string.IsNullOrEmpty(exception))
            {
                Exception = null;
            }
            else
            {
                Exception = new System.Exception(exception);
            }
        }
    }
}
