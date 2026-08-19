using System.Collections.Generic;
using System.Linq;
using Durango.Utils;

namespace Durango.Offline;

public static class CropYaml
{
	private static Dictionary<string, Dictionary<object, Crop>> _crops;

	public static Crop Get(string prototypeId)
	{
		if (_crops == null)
		{
			_crops = Json.ReadFromFile<Dictionary<string, Dictionary<object, Crop>>>("offline/assets/crops");
		}
		return _crops.Get(prototypeId)?.Select(delegate(KeyValuePair<object, Crop> crop)
		{
			KeyValuePair<object, Crop> keyValuePair = crop;
			return keyValuePair.Value;
		}).FirstOrDefault();
	}
}
