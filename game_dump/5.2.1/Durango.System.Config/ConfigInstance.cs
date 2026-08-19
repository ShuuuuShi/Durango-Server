using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Network;
using Durango.Offline;
using Durango.Render;
using Durango.Render.Camera;
using Durango.Render.Particle;
using Durango.Render.Screen;
using Durango.UI;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using Newtonsoft.Json.Linq;
using Shared.Social;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Rendering.PostProcessing;
using Yaml.Util;

namespace Durango.System.Config;

public static class ConfigInstance
{
	[CompilerGenerated]
	private sealed class _003CNoRestartChangeLocalize_003Ed__39 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CNoRestartChangeLocalize_003Ed__39(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			switch (_003C_003E1__state)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				UIManager.ShowLoadingIcon(show: true);
				Loader.Cached = false;
				Loader.Load(Durango.Utils.Singleton<GameManager>.Instance());
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Loader.LoadState != Loader.State.Succees)
			{
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			Connections.Frontend.Close();
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private static string _targetVoiceLocale;

	public static Dictionary<string, List<Setting>> Settings { get; private set; }

	public static event Action<string> ValueChanged;

	public static void Initialize()
	{
		LoadFromJson();
		LoadConfigValue();
		GameManager.Reset += delegate
		{
			ConfigInstance.ValueChanged = null;
		};
		if (Platform.Instance.UsePCUI)
		{
			UIScrollView.FixedScrollWheelFactor = 0.4f;
		}
	}

	private static void LoadFromJson()
	{
		Dictionary<string, JObject> dictionary = Json.ReadFromFile<Dictionary<string, JObject>>((!Platform.Instance.UsePCUI) ? "config_menu" : "config_menu_pc");
		Settings = new Dictionary<string, List<Setting>>();
		foreach (KeyValuePair<string, JObject> item in dictionary)
		{
			string key = item.Key;
			if (!Settings.ContainsKey(key))
			{
				Settings[key] = new List<Setting>();
			}
			foreach (KeyValuePair<string, JToken> item2 in item.Value)
			{
				Setting setting;
				switch (item2.Value.Value<string>("Type").ToEnum(SettingType.Invalid))
				{
				case SettingType.Toggle:
				case SettingType.Locale:
				{
					setting = Json.Read<ToggleSetting>(item2.Value);
					if (!(item2.Key == "resolution"))
					{
						break;
					}
					ToggleSetting toggleSetting = (ToggleSetting)setting;
					if (DeviceInfo.IsLowResolutionAllowed() || !toggleSetting.Options.Contains("low"))
					{
						break;
					}
					string[] array = new string[toggleSetting.Options.Length - 1];
					int num = 0;
					string[] options = toggleSetting.Options;
					foreach (string text in options)
					{
						if (text != "low")
						{
							array[num++] = text;
						}
					}
					toggleSetting.Options = array;
					break;
				}
				case SettingType.Slider:
					setting = Json.Read<SliderSetting>(item2.Value);
					break;
				case SettingType.TextInput:
				case SettingType.Category:
				case SettingType.Switch:
				case SettingType.CheckBox:
				case SettingType.ButtonBox:
				case SettingType.Banner:
					setting = Json.Read<ValueSetting>(item2.Value);
					break;
				case SettingType.TextLabel:
					setting = Json.Read<LabelSetting>(item2.Value);
					break;
				case SettingType.Grid:
					setting = Json.Read<GridSetting>(item2.Value);
					break;
				case SettingType.Dropdown:
					setting = Json.Read<DropdownSetting>(item2.Value);
					break;
				default:
					setting = Json.Read<Setting>(item2.Value);
					break;
				case SettingType.Invalid:
					continue;
				}
				if (setting != null && (Debug.isDebugBuild || !setting.DebugBuild))
				{
					setting.Key = item2.Key;
					Settings[key].Add(setting);
				}
			}
		}
	}

	private static void LoadConfigValue()
	{
		foreach (KeyValuePair<string, List<Setting>> setting in Settings)
		{
			foreach (Setting item in setting.Value)
			{
				if (!(item is ValueSetting valueSetting))
				{
					continue;
				}
				string saveKey = GetSaveKey(valueSetting.Key);
				switch (valueSetting.Type)
				{
				case SettingType.Toggle:
				case SettingType.TextInput:
				case SettingType.Category:
				{
					string value2 = PlayerPrefs.GetString(saveKey, null);
					if (item is ToggleSetting toggleSetting && !toggleSetting.Contains(value2))
					{
						value2 = valueSetting.Default;
					}
					ChangeValue(valueSetting.Key, value2, save: false);
					break;
				}
				case SettingType.Locale:
				{
					string @string = PlayerPrefs.GetString(saveKey, null);
					ChangeValue(valueSetting.Key, @string, save: false);
					break;
				}
				case SettingType.Slider:
				{
					float result2 = PlayerPrefs.GetFloat(saveKey, float.NaN);
					if (float.IsNaN(result2))
					{
						float.TryParse(valueSetting.Default, out result2);
					}
					ChangeValue(valueSetting.Key, result2, save: false);
					break;
				}
				case SettingType.Switch:
				case SettingType.CheckBox:
				{
					int @int = PlayerPrefs.GetInt(saveKey, -1);
					bool result;
					if (@int == -1)
					{
						bool.TryParse(valueSetting.Default, out result);
					}
					else
					{
						result = @int == 1;
					}
					ChangeValue(valueSetting.Key, result, save: false);
					break;
				}
				case SettingType.Dropdown:
					if (item is DropdownSetting dropdownSetting)
					{
						string value = PlayerPrefs.GetString(saveKey, null);
						if (dropdownSetting.Key != "resolution_pc" && !dropdownSetting.Contains(value))
						{
							value = valueSetting.Default;
						}
						ChangeValue(valueSetting.Key, value, save: false);
					}
					break;
				}
			}
		}
	}

	private static string GetSaveKey(string key)
	{
		return "option:" + key;
	}

	public static void UpdateValue(string key, string resolution)
	{
		SetValue(key, resolution);
		SaveValue(key, resolution);
	}

	public static string ChangeValue(string key, string value, bool save = true)
	{
		switch (key)
		{
		case "resolution":
			value = ChangeResolution(value);
			break;
		case "resolution_pc":
			value = ChangeResolution_PC(value);
			break;
		case "screen_mode":
			value = ChangeScreenMode(value);
			break;
		case "anti_aliasing":
			value = ChangeAntiAliasing(value);
			break;
		case "ui_size":
			ChangeUISize(value);
			break;
		case "fps":
			ChangeFps(value);
			break;
		case "locale":
			value = ChangeLocale(value);
			break;
		case "voice_locale":
			value = ChangeVoiceLocale(value);
			break;
		case "orientation":
			ChangeOrientation(value, save);
			break;
		case "shadow":
			ChangeShadowOption(value);
			break;
		case "visual_effect":
			Firefly.ChangeFireflyOption(value == "high");
			break;
		}
		SetValue(key, value);
		if (save)
		{
			SaveValue(key, value);
		}
		return value;
	}

	public static void MuteAll()
	{
		ChangeSfxVolume(0f);
		ChangeAmbienceVolume(0f);
		ChangeBgmVolume(0f);
		ChangeMidiVolume(0f);
	}

	public static void UnMuteAll()
	{
		ChangeSfxVolume(GetValue("sfx_volume", 0f));
		ChangeAmbienceVolume(GetValue("ambience_volume", 0f));
		ChangeBgmVolume(GetValue("bgm_volume", 0f));
		ChangeMidiVolume(GetValue("midi_volume", 0f));
	}

	public static float ChangeValue(string key, float value, bool save = true)
	{
		switch (key)
		{
		case "sfx_volume":
			ChangeSfxVolume(value);
			break;
		case "ambience_volume":
			ChangeAmbienceVolume(value);
			break;
		case "bgm_volume":
			ChangeBgmVolume(value);
			break;
		case "midi_volume":
			ChangeMidiVolume(value);
			break;
		case "max_frame_rate":
			ChangeMaxFrameRate(value);
			break;
		}
		SetValue(key, value);
		if (save)
		{
			SaveValue(key, value);
		}
		return value;
	}

	public static bool ChangeValue(string key, bool value, bool save = true)
	{
		switch (key)
		{
		case "capture_filter":
			ScreenCapture.ApplyFilter = value;
			break;
		case "auto_translation":
			SocialSystem.AutoTranslation = value;
			break;
		case "show_draw_line":
			PlayerManager.ShowDrawLine = value;
			break;
		case "hide_debug_ui":
			if (Durango.Utils.Singleton<UIManager>.HasInstance())
			{
				DevelopmentGroup developmentGroup = UIManager.FindScript<DevelopmentGroup>();
				if (developmentGroup != null)
				{
					developmentGroup.gameObject.SetActive(!value);
				}
			}
			break;
		case "show_chat_bubble":
			ChatBubbleGroup.On = value;
			break;
		case "show_line_chat":
			LineChatWidget.On = value;
			break;
		case "permit_conversation":
			GameSystem<SocialSystem>.Instance().SetSocialOption(SocialOptionType.AllowOutlanderConversation, value);
			save = false;
			break;
		case "chromatic_aberration":
			ChangePostProcessing(key, value);
			break;
		case "vignette":
			ChangeVignettingEffect(value);
			break;
		case "visual_effect":
			Firefly.ChangeFireflyOption(value);
			break;
		case "v_sync":
			ChangeVerticalSync(value);
			break;
		case "mouse_reversed":
			ChangeMouseReversed(value);
			break;
		}
		SetValue(key, value);
		if (save)
		{
			SaveValue(key, value);
		}
		return value;
	}

	private static void ChangePostProcessing(string key, bool value)
	{
		if (!Durango.Utils.Singleton<MainCamera>.HasInstance())
		{
			return;
		}
		Camera camera = Durango.Utils.Singleton<MainCamera>.Instance().Camera;
		if (camera == null)
		{
			return;
		}
		PostProcessVolume component = camera.GetComponent<PostProcessVolume>();
		if (!(component == null))
		{
			PostProcessEffectSettings postProcessEffectSettings = null;
			if (key != null && key == "chromatic_aberration")
			{
				postProcessEffectSettings = component.profile.GetSetting<ChromaticAberration>();
			}
			if (postProcessEffectSettings != null)
			{
				postProcessEffectSettings.enabled.value = value;
			}
		}
	}

	private static void ChangeVignettingEffect(bool value)
	{
		ScreenEffect.UseVignettingEffect = value;
	}

	private static void ChangeVerticalSync(bool value)
	{
		QualitySettings.vSyncCount = (value ? 1 : 0);
	}

	public static void UpdatePostProcessingSettings()
	{
		string[] array = new string[1] { "chromatic_aberration" };
		foreach (string key in array)
		{
			string @string = PlayerPrefs.GetString(GetSaveKey(key), null);
			if (!string.IsNullOrEmpty(@string))
			{
				ChangeValue(key, @string);
			}
		}
	}

	private static void SaveValue(string key, string value)
	{
		PlayerPrefs.SetString("option:" + key, value);
		PlayerPrefs.Save();
	}

	private static void SaveValue(string key, float value)
	{
		PlayerPrefs.SetFloat("option:" + key, value);
		PlayerPrefs.Save();
	}

	private static void SaveValue(string key, bool value)
	{
		PlayerPrefs.SetInt("option:" + key, value ? 1 : 0);
		PlayerPrefs.Save();
	}

	public static void RefreshValue(string key)
	{
		foreach (KeyValuePair<string, List<Setting>> setting in Settings)
		{
			List<Setting> value = setting.Value;
			int i = 0;
			for (int count = value.Count; i < count; i++)
			{
				if (value[i] is ValueSetting valueSetting && !(valueSetting.Key != key))
				{
					switch (value[i].Type)
					{
					case SettingType.Toggle:
					case SettingType.TextInput:
					case SettingType.Locale:
					case SettingType.Category:
					case SettingType.Dropdown:
						ChangeValue(valueSetting.Key, (string)valueSetting.Value, save: false);
						break;
					case SettingType.Slider:
						ChangeValue(valueSetting.Key, (float)valueSetting.Value, save: false);
						break;
					case SettingType.Switch:
					case SettingType.CheckBox:
						ChangeValue(valueSetting.Key, (bool)valueSetting.Value, save: false);
						break;
					case SettingType.TextLabel:
					case SettingType.Button:
					case SettingType.Grid:
					case SettingType.Account:
					case SettingType.ButtonBox:
					case SettingType.TinyButton:
					case SettingType.Banner:
						break;
					}
					return;
				}
			}
		}
	}

	private static void SetValue<TV>(string key, TV value)
	{
		foreach (KeyValuePair<string, List<Setting>> setting in Settings)
		{
			List<Setting> value2 = setting.Value;
			int i = 0;
			for (int count = value2.Count; i < count; i++)
			{
				if (value2[i] is ValueSetting valueSetting && valueSetting.Key == key)
				{
					valueSetting.Value = value;
					if (ConfigInstance.ValueChanged != null)
					{
						ConfigInstance.ValueChanged(key);
					}
					break;
				}
			}
		}
	}

	private static ValueSetting GetValue(string key)
	{
		foreach (KeyValuePair<string, List<Setting>> setting in Settings)
		{
			List<Setting> value = setting.Value;
			int i = 0;
			for (int count = value.Count; i < count; i++)
			{
				if (value[i] is ValueSetting valueSetting && valueSetting.Key == key)
				{
					return valueSetting;
				}
			}
		}
		return null;
	}

	public static TV GetValue<TV>(string key, TV defaultValue = default(TV))
	{
		foreach (KeyValuePair<string, List<Setting>> setting in Settings)
		{
			List<Setting> value = setting.Value;
			int i = 0;
			for (int count = value.Count; i < count; i++)
			{
				if (value[i] is ValueSetting valueSetting && valueSetting.Key == key)
				{
					if (valueSetting.Value is TV)
					{
						return (TV)valueSetting.Value;
					}
					return defaultValue;
				}
			}
		}
		return defaultValue;
	}

	public static void NotifyAction(string key, ValueSetting op = null)
	{
		switch (key)
		{
		case "select_character":
			Durango.Utils.Singleton<GameManager>.Instance().MoveToTitle();
			break;
		case "logout":
			Logout();
			break;
		case "engagement":
			UIManager.Popup.Tooltip<EngagementConfigPopup>().Show();
			break;
		case "show_baseplate":
			Platform.Instance.ShowPlate();
			break;
		case "dump_personal_island":
		{
			DumpPersonalIslandPopup popup = UIManager.Popup.Tooltip<DumpPersonalIslandPopup>();
			popup.Show();
			Connections.Frontend.Send(new RequestDumpedPersonalIsland
			{
				PlayerEntityId = GameManager.PlayerId
			}).On(delegate(DumpedPersonalIsland msg, PacketHeader header)
			{
				AppData.CreateDirectory(WorldContext.GetBasePath(GameManager.ClusterKey));
				WorldContext worldContext = new WorldContext();
				worldContext.Initialize(WorldContext.MakePath(msg.PlayerSlot, GameManager.ClusterKey));
				worldContext.PlayerSlot = msg.PlayerSlot;
				worldContext.TerrainId = msg.TerrainId;
				worldContext.Garden = msg.Garden;
				PlayerContext playerContext = new PlayerContext();
				playerContext.PlayerSlot = msg.PlayerSlot;
				playerContext.Initialize(PlayerContext.MakePath(msg.PlayerSlot, GameManager.ClusterKey));
				playerContext.AppearPlayer = msg.AppearPlayer;
				if (msg.InventoryItems != null)
				{
					playerContext.InventoryItems.AddRange(msg.InventoryItems);
				}
				for (int i = 0; i < playerContext.InventoryItems.Count; i++)
				{
					Item value = playerContext.InventoryItems[i];
					value.Ext = null;
					playerContext.InventoryItems[i] = value;
				}
				playerContext.PlayerInfo.PlayerLevel = msg.AppearPlayer.Level;
				playerContext.PlayerInfo.PlayerName = msg.AppearPlayer.Name;
				playerContext.PlayerInfo.PlayerEntityId = msg.AppearPlayer.EntityId;
				for (int j = 0; j < KUtility.GetSize(msg.Artifacts); j++)
				{
					AppearArtifact value2 = msg.Artifacts[j];
					value2.States.Cage = null;
					worldContext.Artifacts[value2.EntityId] = value2;
				}
				worldContext.Save(persistent: true);
				playerContext.Save();
				popup.Hide();
				UIManager.SystemMsg(T._("개인섬 기록이 완료되었습니다."));
			}).Rest(delegate
			{
				popup.Hide();
			});
			break;
		}
		case "coupon":
			UIManager.Popup.Tooltip<TextInputPopup>().Show(SendCoupon, T._("쿠폰번호를 입력해주세요."));
			break;
		case "credit":
			UIManager.FindScript<CreditGroup>().Open();
			break;
		case "terms":
			ShowTerms();
			break;
		case "change_account":
			ConnectSnsAccount();
			break;
		case "leave":
			DeleteAccount();
			break;
		case "balance_info":
			UIManager.Popup.Tooltip<PaidCurrencyInfoPopup>().DefaultSetting().Show();
			break;
		default:
			OpenUrl(key, op);
			break;
		}
	}

	private static void SendCoupon(string coupon)
	{
		if (!string.IsNullOrEmpty(coupon))
		{
			Connections.Frontend.Send(new AcceptTENCoupon
			{
				CouponNum = coupon,
				ToyToken = Platform.Instance.Token
			}).On<OK>(delegate
			{
				UIManager.Alarm.ShowNotify(T._("선물이 지급되었습니다. 우편함을 확인해주세요."), "icon_mainhud_shop", major: false);
			});
		}
	}

	private static string ChangeResolution(string value)
	{
		if (value.TryEnum<DeviceInfo.Resolution>(out var value2))
		{
			DeviceInfo.ChangeResolution(value2);
		}
		else
		{
			DeviceInfo.ChangeResolution(DeviceInfo.DefaultResolution);
			value = DeviceInfo.DefaultResolution.ToString().ToLower();
		}
		return value;
	}

	private static string ChangeResolution_PC(string value)
	{
		ScreenInfo.SetScreenSize(value);
		return value;
	}

	private static string ChangeScreenMode(string value)
	{
		ScreenInfo.SetScreenMode(value == "fullscreen");
		return value;
	}

	private static string ChangeAntiAliasing(string value)
	{
		BlitScreen.AntiAliasingChanged = true;
		BlitScreen.AntiAliasingValue = value.ToInt();
		return value;
	}

	private static void ChangeUISize(string value)
	{
		if (int.TryParse(value, out var result))
		{
			UIManager.SetUISize(result);
		}
	}

	private static void ChangeFps(string value)
	{
		Application.targetFrameRate = ((!(value == "quality")) ? 30 : 60);
	}

	private static string ChangeLocale(string value)
	{
		if (!string.IsNullOrEmpty(value) && LocalizeSystem.Locale == value)
		{
			return value;
		}
		if (GameManager.IsTitleScene || string.IsNullOrEmpty(LocalizeSystem.Locale))
		{
			return LocalizeSystem.SetLocale(value);
		}
		MessageBox.Button[] items = ((!Application.isEditor) ? new MessageBox.Button[2]
		{
			new MessageBox.Button(T._("확인")),
			T._("취소")
		} : new MessageBox.Button[3]
		{
			new MessageBox.Button(T._("확인")),
			"변경 후 재시작 하지 않음",
			T._("취소")
		});
		UIManager.MessageBox.Show(T._("언어 설정을 바꾸면 게임을 재시작해야 합니다.\n재시작하시겠습니까?"), delegate(int index)
		{
			string targetVoiceLocale = _targetVoiceLocale;
			_targetVoiceLocale = null;
			if (Application.isEditor)
			{
				if (index >= 2)
				{
					return;
				}
			}
			else if (index >= 1)
			{
				return;
			}
			string value2 = LocalizeSystem.SetLocale(value);
			SaveValue("locale", value2);
			if (!string.IsNullOrEmpty(targetVoiceLocale))
			{
				string value3 = LocalizeSystem.SetVoiceLocale(targetVoiceLocale);
				SaveValue("voice_locale", value3);
			}
			if (index == 0)
			{
				Loader.Cached = false;
				Durango.Utils.Singleton<GameManager>.Instance().MoveToTitle();
			}
			else
			{
				Durango.Utils.Singleton<GameManager>.Instance().StartCoroutine(NoRestartChangeLocalize());
			}
		}, items);
		return LocalizeSystem.Locale;
	}

	private static string ChangeVoiceLocale(string value)
	{
		if (!string.IsNullOrEmpty(value) && LocalizeSystem.VoiceLocale == value)
		{
			return value;
		}
		if (GameManager.IsTitleScene || string.IsNullOrEmpty(LocalizeSystem.VoiceLocale))
		{
			return LocalizeSystem.SetVoiceLocale(value);
		}
		if (UIManager.MessageBox.IsShow)
		{
			_targetVoiceLocale = value;
			return value;
		}
		UIManager.MessageBox.Show(T._("음성 설정을 바꾸면 게임을 재시작해야 합니다.\n재시작하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				string value2 = LocalizeSystem.SetVoiceLocale(value);
				SaveValue("voice_locale", value2);
				Durango.Utils.Singleton<GameManager>.Instance().MoveToTitle();
			}
		});
		return LocalizeSystem.VoiceLocale;
	}

	private static IEnumerator NoRestartChangeLocalize()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CNoRestartChangeLocalize_003Ed__39(0);
	}

	private static void ChangeOrientation(string value, bool update)
	{
		OrientationController.Orientation orientation = value.ToEnum(OrientationController.Orientation.Landscape);
		OrientationController.SetTargetOrientation(orientation, update);
		if (orientation != 0 && !Application.isEditor && !update)
		{
			Analytics.CustomEvent(orientation.ToString());
		}
	}

	private static void ChangeShadowOption(string value)
	{
		ShadowOption shadowOption = ShadowOption.Normal;
		if (!(value == "high"))
		{
			if (value == "low")
			{
				shadowOption = ShadowOption.Simple;
			}
		}
		else
		{
			shadowOption = ShadowOption.Normal;
		}
		PlaneShadowManager.ChangeOption(shadowOption);
	}

	private static void ChangeMouseReversed(bool value)
	{
		InputSystem.IsMouseButtonReversed = value;
	}

	private static void ChangeSfxVolume(float val)
	{
		SoundManager.SetSfxVolume(val);
	}

	private static void ChangeAmbienceVolume(float val)
	{
		SoundManager.SetAmbienceVolume(val);
	}

	private static void ChangeMidiVolume(float val)
	{
		SoundManager.SetMidiVolume(val);
	}

	private static void ChangeBgmVolume(float val)
	{
		if (Durango.Utils.Singleton<BgmManager>.HasInstance())
		{
			Durango.Utils.Singleton<BgmManager>.Instance().SetMute(val == 0f);
		}
		SoundManager.SetBgmVolume(val);
	}

	private static void ChangeMaxFrameRate(float val)
	{
		Application.targetFrameRate = Mathf.FloorToInt(val);
	}

	private static void ConnectSnsAccount()
	{
		if (Platform.Instance.IsLoginTypeGuest)
		{
			Platform.Instance.ShowAccountMenu();
		}
		else
		{
			UIManager.MessageBox.Show(T._("이미 연동된 계정입니다."));
		}
	}

	private static void DeleteAccount()
	{
		string mainText = ((!Platform.Instance.IsLoginTypeGuest) ? T._("탈퇴하시겠습니까?\n탈퇴하시면 <alert>모든 게임 데이터</alert>가 삭제됩니다.\n(계정 내 모든 캐릭터, 캐시샵 구매 내역 등)\n\n탈퇴 후 7일 안에 재접속하시면 복구할 수 있습니다.") : T._("탈퇴하시겠습니까?\n탈퇴하시면 <alert>모든 게임 데이터</alert>가 삭제됩니다.\n(계정 내 모든 캐릭터, 캐시샵 구매 내역 등)\n게스트 계정은 복구가 불가능합니다."));
		UIManager.MessageBox.Show(mainText, delegate(bool ok)
		{
			if (ok)
			{
				Connections.Frontend.Send(default(DeregisterUser)).On<OK>(delegate
				{
					Platform.Instance.Leave(delegate(bool success)
					{
						if (success)
						{
							Durango.Utils.Singleton<GameManager>.Instance().MoveToTitle();
						}
					});
				}).Rest(delegate
				{
					UIManager.MessageBox.Show(T._("요청을 처리하지 못했습니다."));
				});
			}
		});
	}

	public static void Logout()
	{
		string mainText = ((!Platform.Instance.IsLoginTypeGuest) ? T._("정말로 로그아웃하시겠습니까?") : T._("계정 연동을 하지 않은 상태로 로그아웃하면,\n다시 로그인할 수 없습니다.\n\n정말로 로그아웃하시겠습니까?"));
		UIManager.MessageBox.Show(mainText, delegate(bool ok)
		{
			if (ok)
			{
				Platform.Instance.Logout(OnLogout);
			}
		});
	}

	private static void OnLogout(bool success)
	{
		if (success)
		{
			Durango.Utils.Singleton<GameManager>.Instance().MoveToTitle();
			return;
		}
		MessageBox messageBox = UIManager.MessageBox;
		if (messageBox != null)
		{
			messageBox.Show(T._("로그아웃에 실패했습니다."));
		}
	}

	public static string GetPresetValue(PresetValue value)
	{
		return value switch
		{
			PresetValue.NPA => (!Debug.isDebugBuild) ? Platform.Instance.NPA : (Platform.Instance.NPA + " (EntityId: " + GameManager.PlayerId + ")"), 
			PresetValue.PlayerName => PlayerBehavior.LocalPlayer.PlayerName, 
			PresetValue.Facebook => Platform.Instance.IsConnectFacebook.ToString(), 
			PresetValue.GooglePlus => Platform.Instance.IsConnectGooglePlus.ToString(), 
			_ => null, 
		};
	}

	public static void OpenOfficialCommunityUrl()
	{
		OpenUrl("official_community", GetValue("official_community"));
	}

	private static void OpenUrl(string key, ValueSetting op)
	{
		if (op == null)
		{
			return;
		}
		if (op.Value is JToken jToken)
		{
			op.Value = Json.Read<Dictionary<string, string>>(jToken);
		}
		if (op.Value is Dictionary<string, string> dict)
		{
			string text = dict.Get(LocalizeSystem.Locale);
			if (string.IsNullOrEmpty(text))
			{
				text = dict.Get("default");
			}
			if (!string.IsNullOrEmpty(text))
			{
				Platform.Instance.ShowWeb(LocalizeSystem.Get("#config_" + key), text);
			}
		}
	}

	private static void ShowTerms()
	{
		SimpleTextListPopup simpleTextListPopup = UIManager.Popup.Tooltip<SimpleTextListPopup>();
		string locale = LocalizeSystem.Locale;
		if (locale != null && locale == "ko_KR")
		{
			simpleTextListPopup.Set("저작권 관련 제반 사항", new string[3] { "저작권의 귀속\n주식회사 넥슨코리아(이하 \"회사\"라 함)가 제공하는 \"야생의 땅: 듀랑고\"(이하 \"본 게임물\"이라 함)의 저작권 등 지적재산권 및 소유권을 포함한 모든 권리는 \"회사\"에게 있습니다.", "본 게임물의 이용 및 제한\n1.  이용자는 \"본 게임물\"을 게임 플레이 목적으로 무상 이용할 수 있습니다.\n2.  이용자는 \"본 게임물\"을 제1항의 목적을 넘어 영리 목적으로 이용하거나 \"회사\"의 사전 승낙 없이 복제, 전송, 출판, 배포, 방송, 기타 방법에 의하여 이용하거나 타인에게 이용하게 하여서는 안 됩니다.\n3.  이용자는 사설 서버 운영, 리버스 엔지니어링 기타 불법적인 목적이나 관련 법령을 위반하는 방법으로 \"본 게임물\"을 이용하거나 타인에게 이용하게 하여서는 안 됩니다.", "면책조항\n\"본 게임물\"은 \"회사\"가 무료로 일시적으로 배포하는 형태로, \"회사\"는 \"본 게임물\"과 관련하여 발생하는 손해에 대해서 어떠한 책임도 지지 않습니다." });
		}
		else
		{
			simpleTextListPopup.Set("Various Copyright Matters", new string[3] { "Copyright Ownership\nAll copyrights, IP rights, ownership rights, and any other rights related to Durango: Wild Lands (hereafter referred to as \"this game\"), a title provided by NEXON Korea Inc (hereafter referred to as \"the company\"), are the sole property of the company.", "This Game's Usage Restrictions\n1.  Users can continue to use this game at no cost for the purpose of accessing the intended gameplay in the intended manner.\n2.  Users are not allowed to use this game for any purpose outside the one listed above. The company does not give users permission to use this game for monetarygain, or to copy, send, publish, distribute, broadcast, or use this game in any way outside of basic gameplay.\n3.  Users are not allowed to run private servers, reverse engineer elements of this game, or use any other method to employ this game in an illegal manner or for illegal purposes. They are likewise forbidden from helping other users do the same.", "Exemption Provision\nThe company has distributed this game for free for a set duration. The company bears no responsibility for any damages caused by or related to this game." });
		}
		simpleTextListPopup.Show();
	}
}
