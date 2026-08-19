using System;
using UnityEngine;

public class tk2dSystem : ScriptableObject
{
	public const string guidPrefix = "tk2d/tk2d_";

	public const string assetName = "tk2d/tk2dSystem";

	public const string assetFileName = "tk2dSystem.asset";

	[NonSerialized]
	public tk2dAssetPlatform[] assetPlatforms = new tk2dAssetPlatform[3]
	{
		new tk2dAssetPlatform("1x", 1f),
		new tk2dAssetPlatform("2x", 2f),
		new tk2dAssetPlatform("4x", 4f)
	};

	private static tk2dSystem _inst;

	private static string currentPlatform = string.Empty;

	[SerializeField]
	private tk2dResourceTocEntry[] allResourceEntries = new tk2dResourceTocEntry[0];

	public static tk2dSystem inst
	{
		get
		{
			if ((Object)(object)_inst == (Object)null)
			{
				_inst = Resources.Load("tk2d/tk2dSystem", typeof(tk2dSystem)) as tk2dSystem;
				if ((Object)(object)_inst == (Object)null)
				{
					_inst = ScriptableObject.CreateInstance<tk2dSystem>();
				}
				Object.DontDestroyOnLoad((Object)(object)_inst);
			}
			return _inst;
		}
	}

	public static tk2dSystem inst_NoCreate
	{
		get
		{
			if ((Object)(object)_inst == (Object)null)
			{
				_inst = Resources.Load("tk2d/tk2dSystem", typeof(tk2dSystem)) as tk2dSystem;
			}
			return _inst;
		}
	}

	public static string CurrentPlatform
	{
		get
		{
			return currentPlatform;
		}
		set
		{
			if (value != currentPlatform)
			{
				currentPlatform = value;
			}
		}
	}

	public static bool OverrideBuildMaterial => false;

	private tk2dSystem()
	{
	}

	public static tk2dAssetPlatform GetAssetPlatform(string platform)
	{
		tk2dSystem tk2dSystem2 = inst_NoCreate;
		if ((Object)(object)tk2dSystem2 == (Object)null)
		{
			return null;
		}
		for (int i = 0; i < tk2dSystem2.assetPlatforms.Length; i++)
		{
			if (tk2dSystem2.assetPlatforms[i].name == platform)
			{
				return tk2dSystem2.assetPlatforms[i];
			}
		}
		return null;
	}

	private T LoadResourceByGUIDImpl<T>(string guid) where T : Object
	{
		tk2dResource tk2dResource2 = Resources.Load("tk2d/tk2d_" + guid, typeof(tk2dResource)) as tk2dResource;
		if ((Object)(object)tk2dResource2 != (Object)null)
		{
			Object objectReference = tk2dResource2.objectReference;
			return (T)(object)((objectReference is T) ? objectReference : null);
		}
		return (T)(object)null;
	}

	private T LoadResourceByNameImpl<T>(string name) where T : Object
	{
		for (int i = 0; i < allResourceEntries.Length; i++)
		{
			if (allResourceEntries[i] != null && allResourceEntries[i].assetName == name)
			{
				return LoadResourceByGUIDImpl<T>(allResourceEntries[i].assetGUID);
			}
		}
		return (T)(object)null;
	}

	public static T LoadResourceByGUID<T>(string guid) where T : Object
	{
		return inst.LoadResourceByGUIDImpl<T>(guid);
	}

	public static T LoadResourceByName<T>(string guid) where T : Object
	{
		return inst.LoadResourceByNameImpl<T>(guid);
	}
}
