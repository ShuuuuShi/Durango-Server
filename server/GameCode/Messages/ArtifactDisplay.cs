using System.Collections.Generic;
using MsgPack;
using Shared.Building;

namespace Messages;

public struct ArtifactDisplay
{
	public const uint TypeCode = 2433u;

	public string EntityId;

	public Shared.Building.Condition Condition;

	public string Color;

	public Dictionary<string, string> Parts;

	public Dictionary<string, string> Textures;

	public Dictionary<string, Pair<string, string>> Decorations;

	public Dictionary<int, Pair<string, string>> AddOns;

	public string Crop;

	public ushort[] PetEntityTypes;

	public string Effect;

	public float? Yaw;

	public string IndoorColor;

	public Pair<string, double>? Music;

	public Pair<string, double>[] Animations;

	public MannequinDisplayInfo? MannequinInfo;

	public static void Pack(Packer packer, ArtifactDisplay val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(16);
			packer.Pack(2433u);
		}
		else
		{
			packer.PackArrayHeader(15);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack((int)val.Condition);
		if (val.Color == null)
		{
			packer.PackNull();
		}
		else if (val.Color == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Color);
		}
		if (val.Parts == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Parts.Count);
			foreach (KeyValuePair<string, string> part in val.Parts)
			{
				if (part.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(part.Key);
				}
				if (part.Value == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(part.Value);
				}
			}
		}
		if (val.Textures == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Textures.Count);
			foreach (KeyValuePair<string, string> texture in val.Textures)
			{
				if (texture.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(texture.Key);
				}
				if (texture.Value == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(texture.Value);
				}
			}
		}
		if (val.Decorations == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Decorations.Count);
			foreach (KeyValuePair<string, Pair<string, string>> decoration in val.Decorations)
			{
				if (decoration.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(decoration.Key);
				}
				packer.PackArrayHeader(2);
				if (decoration.Value.Item1 == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(decoration.Value.Item1);
				}
				if (decoration.Value.Item2 == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(decoration.Value.Item2);
				}
			}
		}
		if (val.AddOns == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.AddOns.Count);
			foreach (KeyValuePair<int, Pair<string, string>> addOn in val.AddOns)
			{
				packer.Pack(addOn.Key);
				packer.PackArrayHeader(2);
				if (addOn.Value.Item1 == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(addOn.Value.Item1);
				}
				if (addOn.Value.Item2 == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(addOn.Value.Item2);
				}
			}
		}
		if (val.Crop == null)
		{
			packer.PackNull();
		}
		else if (val.Crop == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Crop);
		}
		if (val.PetEntityTypes == null)
		{
			packer.PackNull();
		}
		else if (val.PetEntityTypes == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.PetEntityTypes.Length);
			for (int i = 0; i < val.PetEntityTypes.Length; i++)
			{
				packer.Pack(val.PetEntityTypes[i]);
			}
		}
		if (val.Effect == null)
		{
			packer.PackNull();
		}
		else if (val.Effect == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Effect);
		}
		if (!val.Yaw.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Yaw.Value);
		}
		if (val.IndoorColor == null)
		{
			packer.PackNull();
		}
		else if (val.IndoorColor == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.IndoorColor);
		}
		if (!val.Music.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			if (val.Music.Value.Item1 == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Music.Value.Item1);
			}
			packer.Pack(val.Music.Value.Item2);
		}
		if (val.Animations == null)
		{
			packer.PackNull();
		}
		else if (val.Animations == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Animations.Length);
			for (int j = 0; j < val.Animations.Length; j++)
			{
				packer.PackArrayHeader(2);
				if (val.Animations[j].Item1 == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.Animations[j].Item1);
				}
				packer.Pack(val.Animations[j].Item2);
			}
		}
		if (!val.MannequinInfo.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			MannequinDisplayInfo.Pack(packer, val.MannequinInfo.Value);
		}
	}

	public static ArtifactDisplay Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArtifactDisplay result = default(ArtifactDisplay);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 3 < num)
		{
			result.Condition = Shared.Building.Condition.Invalid;
		}
		else
		{
			result.Condition = (Shared.Building.Condition)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Color = null;
		}
		else
		{
			string color = unpacker.LastReadData.AsString();
			result.Color = color;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Parts = new Dictionary<string, string>(num2);
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			string value = unpacker.LastReadData.AsString();
			result.Parts.Add(key, value);
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.Textures = new Dictionary<string, string>(num3);
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			string key2 = unpacker.LastReadData.AsString();
			unpacker.Read();
			string value2 = unpacker.LastReadData.AsString();
			result.Textures.Add(key2, value2);
		}
		unpacker.Read();
		int num4 = unpacker.LastReadData.AsInt32();
		result.Decorations = new Dictionary<string, Pair<string, string>>(num4);
		for (int k = 0; k < num4; k++)
		{
			unpacker.Read();
			string key3 = unpacker.LastReadData.AsString();
			unpacker.Read();
			unpacker.Read();
			string item = unpacker.LastReadData.AsString();
			unpacker.Read();
			string item2 = unpacker.LastReadData.AsString();
			Pair<string, string> value3 = new Pair<string, string>(item, item2);
			result.Decorations.Add(key3, value3);
		}
		unpacker.Read();
		int num5 = unpacker.LastReadData.AsInt32();
		result.AddOns = new Dictionary<int, Pair<string, string>>(num5);
		for (int l = 0; l < num5; l++)
		{
			unpacker.Read();
			int key4 = unpacker.LastReadData.AsInt32();
			unpacker.Read();
			unpacker.Read();
			string item3 = unpacker.LastReadData.AsString();
			unpacker.Read();
			string item4 = unpacker.LastReadData.AsString();
			Pair<string, string> value4 = new Pair<string, string>(item3, item4);
			result.AddOns.Add(key4, value4);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Crop = null;
		}
		else
		{
			string crop = unpacker.LastReadData.AsString();
			result.Crop = crop;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PetEntityTypes = null;
		}
		else
		{
			int num6 = unpacker.LastReadData.AsInt32();
			ushort[] array = new ushort[num6];
			for (int m = 0; m < num6; m++)
			{
				unpacker.Read();
				array[m] = unpacker.LastReadData.AsUInt16();
			}
			result.PetEntityTypes = array;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Effect = null;
		}
		else
		{
			string effect = unpacker.LastReadData.AsString();
			result.Effect = effect;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Yaw = null;
		}
		else
		{
			float value5 = unpacker.LastReadData.AsSingle();
			result.Yaw = value5;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.IndoorColor = null;
		}
		else
		{
			string indoorColor = unpacker.LastReadData.AsString();
			result.IndoorColor = indoorColor;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Music = null;
		}
		else
		{
			unpacker.Read();
			string item5 = unpacker.LastReadData.AsString();
			unpacker.Read();
			double item6 = unpacker.LastReadData.AsDouble();
			Pair<string, double> value6 = new Pair<string, double>(item5, item6);
			result.Music = value6;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Animations = null;
		}
		else
		{
			int num7 = unpacker.LastReadData.AsInt32();
			Pair<string, double>[] array2 = new Pair<string, double>[num7];
			for (int n = 0; n < num7; n++)
			{
				unpacker.Read();
				unpacker.Read();
				string item7 = unpacker.LastReadData.AsString();
				unpacker.Read();
				double item8 = unpacker.LastReadData.AsDouble();
				ref Pair<string, double> reference = ref array2[n];
				reference = new Pair<string, double>(item7, item8);
			}
			result.Animations = array2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.MannequinInfo = null;
		}
		else
		{
			MannequinDisplayInfo value7 = MannequinDisplayInfo.Unpack(unpacker);
			result.MannequinInfo = value7;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactDisplay EntityId={EntityId} Condition={Condition} Color={Color} Parts={Parts} Textures={Textures} Decorations={Decorations} AddOns={AddOns} Crop={Crop} PetEntityTypes={PetEntityTypes} Effect={Effect} Yaw={Yaw} IndoorColor={IndoorColor} Music={Music} Animations={Animations} MannequinInfo={MannequinInfo}>";
	}
}
