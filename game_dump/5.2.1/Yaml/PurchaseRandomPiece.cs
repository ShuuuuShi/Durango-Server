using Newtonsoft.Json;

namespace Yaml;

public struct PurchaseRandomPiece
{
	[JsonProperty(PropertyName = "need_warpgem")]
	public int NeedWarpGem;

	[JsonProperty(PropertyName = "give_r_piece")]
	public int GiveRandomPiece;

	[JsonProperty(PropertyName = "purchable_count")]
	public int PurchasableCount;
}
