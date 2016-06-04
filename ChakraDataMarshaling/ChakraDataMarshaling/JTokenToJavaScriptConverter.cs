﻿using Chakra;
using Newtonsoft.Json.Linq;
using System;

namespace ChakraDataMarshaling
{
    public sealed class JTokenToJavaScriptValueConverter
    {
        private static readonly JTokenToJavaScriptValueConverter s_instance =
            new JTokenToJavaScriptValueConverter();

        private JTokenToJavaScriptValueConverter() { }

        public static JavaScriptValue Convert(JToken token)
        {
            return s_instance.Visit(token);
        }

        private JavaScriptValue Visit(JToken token)
        {
            if (token == null)
                throw new ArgumentNullException(nameof(token));

            switch (token.Type)
            {
                case JTokenType.Array:
                    return VisitArray((JArray)token);
                case JTokenType.Boolean:
                    return VisitBoolean((JValue)token);
                case JTokenType.Float:
                    return VisitFloat((JValue)token);
                case JTokenType.Integer:
                    return VisitInteger((JValue)token);
                case JTokenType.Null:
                    return VisitNull(token);
                case JTokenType.Object:
                    return VisitObject((JObject)token);
                case JTokenType.String:
                    return VisitString((JValue)token);
                case JTokenType.Undefined:
                    return VisitUndefined(token);
                case JTokenType.Constructor:
                case JTokenType.Property:
                case JTokenType.Comment:
                case JTokenType.Date:
                case JTokenType.Raw:
                case JTokenType.Bytes:
                case JTokenType.Guid:
                case JTokenType.Uri:
                case JTokenType.TimeSpan:
                case JTokenType.None:
                default:
                    throw new NotSupportedException();
            }
        }

        private JavaScriptValue VisitArray(JArray token)
        {
            var n = token.Count;
            var array = JavaScriptValue.CreateArray((uint)n);
            for (var i = 0; i < n; ++i)
            {
                var element = Visit(token[i]);
                array.SetIndexedProperty(JavaScriptValue.FromInt32(i), element);
            }

            return array;
        }

        private JavaScriptValue VisitBoolean(JValue token)
        {
            return JavaScriptValue.FromBoolean(token.Value<bool>());
        }

        private JavaScriptValue VisitFloat(JValue token)
        {
            return JavaScriptValue.FromDouble(token.Value<double>());
        }

        private JavaScriptValue VisitInteger(JValue token)
        {
            return JavaScriptValue.FromDouble(token.Value<double>());
        }

        private JavaScriptValue VisitNull(JToken token)
        {
            return JavaScriptValue.Null;
        }

        private JavaScriptValue VisitObject(JObject token)
        {
            var jsonObject = JavaScriptValue.CreateObject();
            foreach (var entry in token)
            {
                var value = Visit(entry.Value);
                var propertyId = JavaScriptPropertyId.FromString(entry.Key);
                jsonObject.SetProperty(propertyId, value, true);
            }

            return jsonObject;
        }

        private JavaScriptValue VisitString(JValue token)
        {
            return JavaScriptValue.FromString(token.Value<string>());
        }

        private JavaScriptValue VisitUndefined(JToken token)
        {
            return JavaScriptValue.Undefined;
        }
    }
}