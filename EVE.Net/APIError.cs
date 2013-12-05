using System;
using System.Collections.Generic;
using System.Text;

namespace EVE.Net
{
    public class APIError
    {
        public int errorCode = 0;
        public string errorMsg = "";

        public APIError(int code, string msg)
        {
            this.errorCode = code;
            this.errorMsg = msg;
        }
    }
}
