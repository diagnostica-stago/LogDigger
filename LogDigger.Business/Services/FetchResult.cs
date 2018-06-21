namespace LogDigger.Business.Services
{
    public class FetchResult<T>
    {
        public FetchResult(string error)
            : this(default(T), error, null)
        {
        }

        public FetchResult(string error, string tempFolder)
            : this(default(T), error, tempFolder)
        {
        }

        public FetchResult(T result)
            : this(result, null, null)
        {
        }

        public FetchResult(T result, string error, string tempFolder)
        {
            Result = result;
            Error = error;
            TempFolder = tempFolder;
        }

        public T Result { get; }
        public string Error { get; }
        public string TempFolder { get; }
        public bool HasError => !string.IsNullOrEmpty(Error);
    }
}