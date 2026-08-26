using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DurangoMemoryBot
{

internal sealed class MemoryBotRequest
{
    public string RequestId;
    public string Op;
    public string Path;
    public string Name;
    public string Token;
    public string Sections;
    public string Filename;
    public string Kind;
    public string EntityId;
    public string ActionId;
    public string ItemId;
    public string Uri;
    public float X;
    public float Y;
    public bool HasX;
    public bool HasY;
}

internal static class MemoryBotProtocol
{
    public const int MaxLine = 32768;

    public static bool TryParse(string line, out MemoryBotRequest request, out string error)
    {
        request = null;
        error = null;
        if (string.IsNullOrEmpty(line) || line.Length > MaxLine)
        {
            error = "empty_or_oversized_request";
            return false;
        }
        Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.Ordinal);
        int i = 0;
        Skip(line, ref i);
        if (!ReadObject(line, ref i, values) || i != line.Length)
        {
            error = "invalid_json_object";
            return false;
        }
        request = new MemoryBotRequest
        {
            RequestId = Get(values, "request_id", "0"),
            Op = Get(values, "op", ""),
            Path = Get(values, "path", ""),
            Name = Get(values, "name", ""),
            Token = Get(values, "token", ""),
            Sections = Get(values, "sections", ""),
            Filename = Get(values, "filename", ""),
            Kind = Get(values, "kind", ""),
            EntityId = Get(values, "entity_id", ""),
            ActionId = Get(values, "action_id", ""),
            ItemId = Get(values, "item_id", ""),
            Uri = Get(values, "uri", "")
        };
        float f;
        if (TryFloat(Get(values, "x", ""), out f)) { request.X = f; request.HasX = true; }
        if (TryFloat(Get(values, "y", ""), out f)) { request.Y = f; request.HasY = true; }
        if (request.Op.Length == 0) { error = "missing_op"; return false; }
        return true;
    }

    private static string Get(Dictionary<string, string> values, string key, string fallback)
    {
        string value;
        return values.TryGetValue(key, out value) ? value : fallback;
    }

    private static bool TryFloat(string text, out float value)
    {
        return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            && !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static void Skip(string text, ref int i)
    {
        while (i < text.Length && char.IsWhiteSpace(text[i])) i++;
    }

    private static bool ReadObject(string text, ref int i, Dictionary<string, string> values)
    {
        Skip(text, ref i);
        if (i >= text.Length || text[i++] != '{') return false;
        Skip(text, ref i);
        if (i < text.Length && text[i] == '}') { i++; return true; }
        while (i < text.Length)
        {
            Skip(text, ref i);
            string key;
            if (!ReadString(text, ref i, out key)) return false;
            Skip(text, ref i);
            if (i >= text.Length || text[i++] != ':') return false;
            Skip(text, ref i);
            string value;
            if (i < text.Length && text[i] == '[')
            {
                if (!ReadArrayAsCsv(text, ref i, out value)) return false;
            }
            else if (i < text.Length && text[i] == '"')
            {
                if (!ReadString(text, ref i, out value)) return false;
            }
            else
            {
                int start = i;
                while (i < text.Length && text[i] != ',' && text[i] != '}') i++;
                value = text.Substring(start, i - start).Trim();
            }
            values[key] = value;
            Skip(text, ref i);
            if (i < text.Length && text[i] == '}') { i++; return true; }
            if (i >= text.Length || text[i++] != ',') return false;
        }
        return false;
    }

    private static bool ReadArrayAsCsv(string text, ref int i, out string value)
    {
        value = "";
        if (i >= text.Length || text[i++] != '[') return false;
        StringBuilder result = new StringBuilder();
        while (i < text.Length)
        {
            Skip(text, ref i);
            if (i < text.Length && text[i] == ']') { i++; value = result.ToString(); return true; }
            string item;
            if (i < text.Length && text[i] == '"')
            {
                if (!ReadString(text, ref i, out item)) return false;
            }
            else
            {
                int start = i;
                while (i < text.Length && text[i] != ',' && text[i] != ']') i++;
                item = text.Substring(start, i - start).Trim();
            }
            if (result.Length > 0) result.Append(',');
            result.Append(item);
            Skip(text, ref i);
            if (i < text.Length && text[i] == ',') { i++; continue; }
            if (i < text.Length && text[i] == ']') { i++; value = result.ToString(); return true; }
            return false;
        }
        return false;
    }

    private static bool ReadString(string text, ref int i, out string value)
    {
        value = null;
        if (i >= text.Length || text[i++] != '"') return false;
        StringBuilder result = new StringBuilder();
        while (i < text.Length)
        {
            char c = text[i++];
            if (c == '"') { value = result.ToString(); return true; }
            if (c != '\\') { result.Append(c); continue; }
            if (i >= text.Length) return false;
            c = text[i++];
            switch (c)
            {
                case '"': result.Append('"'); break;
                case '\\': result.Append('\\'); break;
                case '/': result.Append('/'); break;
                case 'n': result.Append('\n'); break;
                case 'r': result.Append('\r'); break;
                case 't': result.Append('\t'); break;
                default: return false;
            }
        }
        return false;
    }

    public static string Success(string id, string data)
    {
        return "{\"ok\":true,\"request_id\":" + Quote(id) + ",\"schema_version\":1,\"data\":" + data + "}";
    }

    public static string Error(string id, string error)
    {
        return "{\"ok\":false,\"request_id\":" + Quote(id) + ",\"error\":" + Quote(error) + "}";
    }

    public static string Quote(string value)
    {
        if (value == null) value = "";
        StringBuilder sb = new StringBuilder(value.Length + 2);
        sb.Append('"');
        for (int i = 0; i < value.Length; i++)
        {
            char c = value[i];
            if (c == '"') sb.Append("\\\"");
            else if (c == '\\') sb.Append("\\\\");
            else if (c == '\n') sb.Append("\\n");
            else if (c == '\r') sb.Append("\\r");
            else if (c == '\t') sb.Append("\\t");
            else if (c < 32) sb.Append(" ");
            else sb.Append(c);
        }
        return sb.Append('"').ToString();
    }
}
}
