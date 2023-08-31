using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalEntities.Misc
{
    [Serializable]
    public class ResponseObject<T> 
    {
        public readonly bool Succeeded;

        public T? _data { set; get; }
        public ResultCode _code { set; get; }
        public String? _message { set; get; }

        public void SetResponeData(T data, ResultCode code, String message)
        {
            _code = code;
            _data = data;
            _message = message;
        }

        public T GetResponseData()
        {
            return _data;
        }
    }
}
