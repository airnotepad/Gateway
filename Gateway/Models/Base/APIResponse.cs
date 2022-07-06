namespace Gateway.Models.Base
{
    public class APIResponse<T>
    {
        public T Result { get; set; }
        public bool Ok { get => Error == null; }
        public Dictionary<string, string>? Error { get; set; }

        public APIResponse()
        {

        }

        public APIResponse(T data)
        {
            Result = data;
        }

        public void SetException(Exception e)
        {
            var error = new Dictionary<string, string>
            {
                {"Type", e.GetType().ToString()},
                {"Message", e.Message},
                {"StackTrace", e.StackTrace},
                {"InnerException", e.InnerException.Message},
            };
            Error = error;
        }
    }
}
