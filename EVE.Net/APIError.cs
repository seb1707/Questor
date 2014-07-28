
namespace EVE.Net
{
    public class APIError
    {
        public int errorCode = 0;
        public string errorMsg = "";

        public APIError(int code, string msg)
        {
            errorCode = code;
            errorMsg = msg;
        }
    }
}
