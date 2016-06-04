using Chakra;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ChakraDataMarshaling
{
    public sealed class JsonParseConverter
    {
        private readonly JavaScriptValue _globalObject;
        private JavaScriptValue _parseFunction;

        public JsonParseConverter()
        {
            _globalObject = JavaScriptValue.GlobalObject;
            _parseFunction = GetParseFunction(_globalObject);
        }

        public JavaScriptValue Convert(JToken value)
        {
            var json = value.ToString(Formatting.None);
            var jsonValue = JavaScriptValue.FromString(json);

            return _parseFunction.CallFunction(_globalObject, jsonValue);
        }

        private static JavaScriptValue GetParseFunction(JavaScriptValue globalObject)
        {
            var jsonObject = globalObject.GetProperty(JavaScriptPropertyId.FromString("JSON"));
            return jsonObject.GetProperty(JavaScriptPropertyId.FromString("parse"));
        }
    }
}
