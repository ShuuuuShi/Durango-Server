using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
namespace DurangoServer.Core;
internal sealed class ModManifest
{
 internal static readonly Regex IdRx=new("^[a-z0-9][a-z0-9._-]{0,63}$");
 static readonly Regex VerRx=new("^[0-9]+(?:\\.[0-9]+){0,3}(?:[-+][0-9A-Za-z.-]+)?$");
 static readonly JsonSerializerOptions Opt=new(){PropertyNameCaseInsensitive=true,ReadCommentHandling=JsonCommentHandling.Skip,AllowTrailingCommas=true};
 [JsonPropertyName("id")] public string Id{get;set;}="";
 [JsonPropertyName("name")] public string Name{get;set;}="";
 [JsonPropertyName("version")] public string Version{get;set;}="";
 [JsonPropertyName("api_version")] public string ApiVersion{get;set;}="1.0";
 [JsonPropertyName("assembly")] public string Assembly{get;set;}="";
 [JsonPropertyName("dependencies")] public List<string> Dependencies{get;set;}=new();
 [JsonPropertyName("required")] public bool Required{get;set;}
 [JsonPropertyName("sha256")] public string Sha256{get;set;}="";
 [JsonPropertyName("content_sha256")] public string ContentSha256{get;set;}="";
 [JsonPropertyName("signature")] public string Signature{get;set;}="";
 [JsonPropertyName("public_key")] public string PublicKey{get;set;}="";
 public static bool TryRead(string path,out ModManifest m,out string error)
 {
  m=null!;error="";
  try{m=JsonSerializer.Deserialize<ModManifest>(File.ReadAllText(path),Opt)!;}catch(Exception e){error="invalid mod.json: "+e.Message;return false;}
  if(m==null){error="mod.json is empty";return false;}
  if(!IdRx.IsMatch(m.Id??"")){error="invalid id";return false;}
  if(string.IsNullOrWhiteSpace(m.Name)||m.Name.Length>120){error="invalid name";return false;}
  if(!VerRx.IsMatch(m.Version??"")){error="invalid version";return false;}
  if(string.IsNullOrWhiteSpace(m.ApiVersion)||m.ApiVersion.Split('.',2)[0]!="1"){error="unsupported api_version";return false;}
  if(string.IsNullOrWhiteSpace(m.Assembly)||Path.GetExtension(m.Assembly)!=".dll"||Path.GetFileName(m.Assembly)!=m.Assembly||m.Assembly.Contains("..")){error="assembly must be a local .dll filename";return false;}
  if(!string.IsNullOrWhiteSpace(m.Sha256)&&!IsSha256(m.Sha256)){error="invalid sha256";return false;}
  if(!string.IsNullOrWhiteSpace(m.ContentSha256)&&!IsSha256(m.ContentSha256)){error="invalid content_sha256";return false;}
  m.Dependencies??=new();
  var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  foreach(string d in m.Dependencies){if(!IdRx.IsMatch(d??"")||!seen.Add(d)){error="invalid or duplicate dependency";return false;}if(string.Equals(d,m.Id,StringComparison.OrdinalIgnoreCase)){error="self dependency";return false;}}
  return true;
 }
 public string AssemblyPath(string dir)=>Path.Combine(dir,Assembly);
 public static bool IsSha256(string value)=>Regex.IsMatch(value??"", "^[0-9a-fA-F]{64}$", RegexOptions.CultureInvariant);
}
