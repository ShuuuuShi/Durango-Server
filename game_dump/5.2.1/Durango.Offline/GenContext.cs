using System;
using System.Collections.Generic;
using System.IO;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Newtonsoft.Json;

namespace Durango.Offline;

public class GenContext
{
	[JsonProperty("player_slot")]
	public int PlayerSlot;

	[JsonProperty("item_gen_dict")]
	public Dictionary<string, List<Generator>> ItemGenDict;

	[JsonProperty("persistent")]
	public bool Persistent;

	[JsonIgnore]
	public string Path { get; private set; }

	public void Initialize(string path)
	{
		if (ItemGenDict == null)
		{
			ItemGenDict = new Dictionary<string, List<Generator>>();
		}
		Path = path;
	}

	public void Save()
	{
		if (string.IsNullOrEmpty(Path))
		{
			return;
		}
		try
		{
			byte[] bytes = Json.WriteToBytes(this, indented: true);
			File.WriteAllBytes(Path, bytes);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
	}

	public static string MakePath(int slot, string clusterKey)
	{
		return global::System.IO.Path.Combine(AppData.CombinePath(WorldContext.GetBasePath(clusterKey)), slot + ".gen");
	}

	[CanBeNull]
	public static GenContext Load(string path)
	{
		GenContext genContext = null;
		try
		{
			genContext = Json.Read<GenContext>(File.ReadAllBytes(path));
			if (genContext == null)
			{
				return null;
			}
			genContext.Initialize(path);
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		return genContext;
	}
}
