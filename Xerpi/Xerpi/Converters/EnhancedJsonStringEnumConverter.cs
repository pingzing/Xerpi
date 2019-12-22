using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Xerpi.Converters
{
    /// <summary>
    /// Honors [EnumMember] attributes and can handle nullable enums.
    /// </summary>
    public class EnhancedJsonStringEnumConverter : JsonConverterFactory
    {
        private readonly JsonNamingPolicy? _namingPolicy;
        private readonly bool _allowIntegerValues;
        private readonly JsonStringEnumConverter _baseConverter;

        public EnhancedJsonStringEnumConverter() : this(null, true) { }

        public EnhancedJsonStringEnumConverter(JsonNamingPolicy? namingPolicy = null, bool allowIntegerValues = true)
        {
            _namingPolicy = namingPolicy;
            _allowIntegerValues = allowIntegerValues;
            _baseConverter = new JsonStringEnumConverter(namingPolicy, allowIntegerValues);
        }

        public override bool CanConvert(Type typeToConvert) => _baseConverter.CanConvert(typeToConvert);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var query = from field in typeToConvert.GetFields(BindingFlags.Public | BindingFlags.Static)
                        let attr = field.GetCustomAttribute<EnumMemberAttribute>()
                        where attr != null
                        select (field.Name, attr.Value);
            var dictionary = query.ToDictionary(p => p.Item1, p => p.Item2);
            if (dictionary.Count > 0)
            {
                return new JsonStringEnumConverter(new DictionaryLookupNamingPolicy(dictionary, _namingPolicy), _allowIntegerValues).CreateConverter(typeToConvert, options);
            }
            else
            {
                return _baseConverter.CreateConverter(typeToConvert, options);
            }
        }        

        public class JsonNamingPolicyDecorator : JsonNamingPolicy
        {
            readonly JsonNamingPolicy _underlyingNamingPolicy;

            public JsonNamingPolicyDecorator(JsonNamingPolicy underlyingNamingPolicy) => this._underlyingNamingPolicy = underlyingNamingPolicy;

            public override string ConvertName(string name) => _underlyingNamingPolicy == null ? name : _underlyingNamingPolicy.ConvertName(name);
        }

        internal class DictionaryLookupNamingPolicy : JsonNamingPolicyDecorator
        {
            readonly Dictionary<string, string> _dictionary;

#pragma warning disable CS8604 // Possible null reference argument.
            public DictionaryLookupNamingPolicy(Dictionary<string, string> dictionary, JsonNamingPolicy? underlyingNamingPolicy) : base(underlyingNamingPolicy) => this._dictionary = dictionary;
#pragma warning restore CS8604 // Possible null reference argument.

            public override string ConvertName(string name)
            {
                if (!_dictionary.TryGetValue(name, out var value))
                    value = base.ConvertName(name);
                return value;
            }
        }
    }
}
