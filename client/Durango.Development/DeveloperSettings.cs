using System;
using UnityEngine;

namespace Durango.Development;

public static class DeveloperSettings
{
	public static readonly string[] UserBotTypes = new string[5] { "Default", "BuildAndTouch", "ScatterAndGather", "Ancora", "WarpRush" };

	public static string GatewayUrl
	{
		get
		{
			string text = PlayerPrefs.GetString("editor_gateway_url" + Application.dataPath);
			if (text.Length > 0 && text[text.Length - 1] == '/')
			{
				text = (GatewayUrl = text.Substring(0, text.Length - 1));
			}
			return text;
		}
		set
		{
			PlayerPrefs.SetString("editor_gateway_url" + Application.dataPath, value);
		}
	}

	public static string InspectorUrl
	{
		get
		{
			string text = PlayerPrefs.GetString("editor_inspector_url" + Application.dataPath);
			if (text.Length > 0 && text[text.Length - 1] == '/')
			{
				text = (InspectorUrl = text.Substring(0, text.Length - 1));
			}
			return text;
		}
		set
		{
			PlayerPrefs.SetString("editor_inspector_url" + Application.dataPath, value);
		}
	}

	public static int SelectedUserBotTypeIndex
	{
		get
		{
			return PlayerPrefs.GetInt("editor_selected_userbot_type_index");
		}
		set
		{
			PlayerPrefs.SetInt("editor_selected_userbot_type_index", value);
		}
	}

	public static string ClusterListDownloadUrlFormat
	{
		get
		{
			return PlayerPrefs.GetString("editor_cluster_list_download_url_format");
		}
		set
		{
			PlayerPrefs.SetString("editor_cluster_list_download_url_format", value);
		}
	}

	public static bool UseClusterList
	{
		get
		{
			return Convert.ToBoolean(PlayerPrefs.GetInt("editor_use_cluster_list", 0));
		}
		set
		{
			PlayerPrefs.SetInt("editor_use_cluster_list", Convert.ToInt32(value));
		}
	}

	public static bool WillUseServerSecret
	{
		get
		{
			return Convert.ToBoolean(PlayerPrefs.GetInt("editor_will_use_server_sign", 0));
		}
		set
		{
			PlayerPrefs.SetInt("editor_will_use_server_sign", Convert.ToInt32(value));
		}
	}

	public static string ServerSecret
	{
		get
		{
			return PlayerPrefs.GetString("editor_server_sign");
		}
		set
		{
			PlayerPrefs.SetString("editor_server_sign", value);
		}
	}

	public static bool WillOverrideFrontendEndPoint
	{
		get
		{
			return Convert.ToBoolean(PlayerPrefs.GetInt("editor_will_override_frontend_url", 0));
		}
		set
		{
			PlayerPrefs.SetInt("editor_will_override_frontend_url", Convert.ToInt32(value));
		}
	}

	public static string OverrideFrontendEndPoint
	{
		get
		{
			return PlayerPrefs.GetString("editor_override_frontend_url" + Application.dataPath);
		}
		set
		{
			PlayerPrefs.SetString("editor_override_frontend_url" + Application.dataPath, value);
		}
	}

	public static bool WillOverrideRadiotowerEndPoint
	{
		get
		{
			return Convert.ToBoolean(PlayerPrefs.GetInt("editor_will_override_radiotower_url", 0));
		}
		set
		{
			PlayerPrefs.SetInt("editor_will_override_radiotower_url", Convert.ToInt32(value));
		}
	}

	public static string OverrideRadiotowerEndPoint
	{
		get
		{
			return PlayerPrefs.GetString("editor_override_radiotower_url" + Application.dataPath);
		}
		set
		{
			PlayerPrefs.SetString("editor_override_radiotower_url" + Application.dataPath, value);
		}
	}

	public static bool WillOverrideArenaAuthServerEndPoint
	{
		get
		{
			return Convert.ToBoolean(PlayerPrefs.GetInt("editor_will_override_arena_auth_url", 0));
		}
		set
		{
			PlayerPrefs.SetInt("editor_will_override_arena_auth_url", Convert.ToInt32(value));
		}
	}

	public static string OverrideArenaAuthServerEndPoint
	{
		get
		{
			return PlayerPrefs.GetString("editor_override_arena_auth_url" + Application.dataPath);
		}
		set
		{
			PlayerPrefs.SetString("editor_override_arena_auth_url" + Application.dataPath, value);
		}
	}

	public static string DevSN
	{
		get
		{
			string text = PlayerPrefs.GetString("editor_devsn" + Application.dataPath);
			if (string.IsNullOrEmpty(text))
			{
				text = (DevSN = (SystemInfo.deviceUniqueIdentifier + Application.dataPath).GetHashCode().ToString());
			}
			return text;
		}
		set
		{
			PlayerPrefs.SetString("editor_devsn" + Application.dataPath, value);
		}
	}

	public static string DevToken
	{
		get
		{
			string text = PlayerPrefs.GetString("editor_devtoken" + Application.dataPath);
			if (string.IsNullOrEmpty(text))
			{
				text = (DevToken = (SystemInfo.deviceUniqueIdentifier + Application.dataPath).GetHashCode().ToString());
			}
			return text;
		}
		set
		{
			PlayerPrefs.SetString("editor_devtoken" + Application.dataPath, value);
		}
	}

	public static string DevSteamToken
	{
		get
		{
			return PlayerPrefs.GetString("editor_devsteamtoken" + Application.dataPath);
		}
		set
		{
			PlayerPrefs.SetString("editor_devsteamtoken" + Application.dataPath, value);
		}
	}

	public static string ForcedTerrainId
	{
		get
		{
			return PlayerPrefs.GetString("forced_terrain_id");
		}
		set
		{
			PlayerPrefs.SetString("forced_terrain_id", value);
		}
	}

	public static bool SkipPrologue
	{
		get
		{
			return PlayerPrefs.GetInt("skip_prologue") != 0;
		}
		set
		{
			PlayerPrefs.SetInt("skip_prologue", value ? 1 : 0);
		}
	}

	public static bool AllowClonedPlayer
	{
		get
		{
			return PlayerPrefs.GetInt("allow_cloned_player") != 0;
		}
		set
		{
			PlayerPrefs.SetInt("allow_cloned_player", value ? 1 : 0);
		}
	}

	public static bool TranslatesMyChatMsg
	{
		get
		{
			return PlayerPrefs.GetInt("translates_my_chat_msg") != 0;
		}
		set
		{
			PlayerPrefs.SetInt("translates_my_chat_msg", value ? 1 : 0);
		}
	}

	public static int ForceGender
	{
		get
		{
			return PlayerPrefs.GetInt("force_gender");
		}
		set
		{
			PlayerPrefs.SetInt("force_gender", value);
		}
	}

	public static int UseAssetBundle
	{
		get
		{
			return PlayerPrefs.GetInt("use_asset_bundle");
		}
		set
		{
			PlayerPrefs.SetInt("use_asset_bundle", value);
		}
	}

	public static RuntimePlatform AssetBundlePlatform => UseAssetBundle switch
	{
		1 => RuntimePlatform.Android, 
		2 => RuntimePlatform.IPhonePlayer, 
		3 => RuntimePlatform.WindowsPlayer, 
		_ => Application.platform, 
	};

	public static bool UsePseudoServer
	{
		get
		{
			return PlayerPrefs.GetInt("use_pseudo_server") != 0;
		}
		set
		{
			PlayerPrefs.SetInt("use_pseudo_server", value ? 1 : 0);
		}
	}

	public static bool GuideUseLocalProgress
	{
		get
		{
			return PlayerPrefs.GetInt("guide_use_local_progress") != 0;
		}
		set
		{
			PlayerPrefs.SetInt("guide_use_local_progress", value ? 1 : 0);
		}
	}

	public static bool GuideSkipNormalFlow
	{
		get
		{
			return PlayerPrefs.GetInt("guide_skip_normal_flow") != 0;
		}
		set
		{
			PlayerPrefs.SetInt("guide_skip_normal_flow", value ? 1 : 0);
		}
	}

	public static bool GuideSkipAll
	{
		get
		{
			return PlayerPrefs.GetInt("guide_skip_all") != 0;
		}
		set
		{
			PlayerPrefs.SetInt("guide_skip_all", value ? 1 : 0);
		}
	}

	public static string GuideType
	{
		get
		{
			return PlayerPrefs.GetString("guide_type");
		}
		set
		{
			PlayerPrefs.SetString("guide_type", value);
		}
	}

	public static bool ActiveUserBot
	{
		get
		{
			return PlayerPrefs.GetInt("active_user_bot") != 0;
		}
		set
		{
			PlayerPrefs.SetInt("active_user_bot", value ? 1 : 0);
		}
	}

	public static bool UsePCUI
	{
		get
		{
			return PlayerPrefs.GetInt("use_pc_ui") != 0;
		}
		set
		{
			PlayerPrefs.SetInt("use_pc_ui", value ? 1 : 0);
		}
	}

	public static bool UsePCRenderer
	{
		get
		{
			return PlayerPrefs.GetInt("use_pc_renderer") != 0;
		}
		set
		{
			PlayerPrefs.SetInt("use_pc_renderer", value ? 1 : 0);
		}
	}
}
