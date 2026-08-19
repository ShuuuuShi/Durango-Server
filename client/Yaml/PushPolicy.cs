using Newtonsoft.Json;
using Shared.Push;

namespace Yaml;

public class PushPolicy
{
	[JsonProperty(PropertyName = "policy_name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "policy")]
	public Policy Policy;

	[JsonProperty(PropertyName = "local")]
	public bool IsLocal;

	[JsonProperty(PropertyName = "id")]
	public int Id;

	public string GetId()
	{
		return (1 << (int)Policy).ToString();
	}
}
