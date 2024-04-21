using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BRIE.Classes.Etc
{
    class IgnoreReadOnlyPropertiesResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            return properties.Where(p => !IsReadOnly(p)).ToList();
        }

        private bool IsReadOnly(JsonProperty property)
        {
            return !property.Writable;
        }
    }
}
