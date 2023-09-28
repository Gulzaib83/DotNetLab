using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExternalEntities.Misc
{
    public class ResponseObjectConverter<T> : JsonConverter<ResponseObject<T>>
    {
        public override ResponseObject<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }

            ResponseObject<T> response = new ResponseObject<T>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return response;
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    string propertyName = reader.GetString();

                    if (propertyName == "data")
                    {
                        reader.Read();

                        // Use JsonSerializer.Deserialize to deserialize the _data field
                        T data = JsonSerializer.Deserialize<T>(ref reader, options);

                        response._data = data;
                    }
                    else if (propertyName == "code")
                    {
                        reader.Read();
                        response._code = (ResultCode)Enum.Parse(typeof(ResultCode), reader.GetString());
                        //response._code = new ResultCode(reader.GetString());
                    }
                    else if (propertyName == "message")
                    {
                        reader.Read();
                        response._message = reader.GetString();
                    }
                    else
                    {
                        // Ignore unknown properties    
                        reader.Skip();
                    }
                }
            }

            throw new JsonException();
        }

        public override void Write(Utf8JsonWriter writer, ResponseObject<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("code", value._code.GetStringValue());
            writer.WriteString("message", value._message);
            writer.WritePropertyName("data");
            JsonSerializer.Serialize(writer, value._data, options);
            writer.WriteEndObject();
        }
    }

    
    public class ResponseObject<T> 
    {
        [JsonConverter(typeof(ResponseObjectConverter<>))]
        public T? _data { set; get; }
        public ResultCode _code { set; get; }
        public String? _message { set; get; }

        public ResponseObject()
        {

        }

        public ResponseObject(T data)
        {
            _data = data;
        }

        public T GetResponseData()
        {
            return _data;
        }

        ResultCode GetResultCode()
        {
            return _code;
        }

        public void SetResponeData(T data, ResultCode code, String message)
        {
            _code = code;
            _data = data;
            _message = message;
        }
    }
}
