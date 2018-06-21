namespace LogDigger.Business.Services
{
    public static class FetchResultExtensions
    {
        public static FetchResult<T> ToFetchResult<T>(this T result, string tempFolder = null, string error = "")
        {
            return new FetchResult<T>(result, error, tempFolder);
        }
    }
}