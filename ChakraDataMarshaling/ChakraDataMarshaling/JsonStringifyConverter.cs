using Chakra;
using Newtonsoft.Json.Linq;

namespace ChakraDataMarshaling
{
    public sealed class JsonStringifyConverter
    {
        private readonly JavaScriptValue _globalObject;
        private JavaScriptValue _stringifyFunction;

        public JsonStringifyConverter()
        {
            _globalObject = JavaScriptValue.GlobalObject;
            _stringifyFunction = GetStringifyFunction(_globalObject);
        }

        public JToken Convert(JavaScriptValue value)
        {
            var jsonValue = _stringifyFunction.CallFunction(_globalObject, value);
            var json = jsonValue.ToString();
            return JToken.Parse(json);
        }

        private static JavaScriptValue GetStringifyFunction(JavaScriptValue globalObject)
        {
            var jsonObject = globalObject.GetProperty(JavaScriptPropertyId.FromString("JSON"));
            return jsonObject.GetProperty(JavaScriptPropertyId.FromString("stringify"));
        }
    }
}
