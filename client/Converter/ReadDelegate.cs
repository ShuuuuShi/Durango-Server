using System;
using Newtonsoft.Json;

namespace Converter;

public delegate object ReadDelegate(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer);
