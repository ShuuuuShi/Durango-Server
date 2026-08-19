using System;
using System.Collections.Generic;
using L10N;
using Messages;
using NPA;
using Newtonsoft.Json.Linq;
using OptionData;
using TimerData;
using UnityEngine;

public class OptionSystem : GameSystem<OptionSystem>
{
	private double _escapeRequestAvailableAt;

	public List<global::OptionData.OptionData> Options { get; private set; }

	private void Awake()
	{
		InitOptionJson();
		LoadOptionValue();
	}

	private void InitOptionJson()
	{
		Dictionary<string, JObject> dictionary = KUtility.ParseJsonFile<Dictionary<string, JObject>>("game_option_menu");
		Options = new List<global::OptionData.OptionData>();
		foreach (KeyValuePair<string, JObject> item in dictionary)
		{
			string source = item.Value.Value<string>("Type");
			global::OptionData.OptionData optionData;
			switch (source.ToEnum(OptionType.Invalid))
			{
			case OptionType.Toggle:
			case OptionType.Locale:
				optionData = KUtility.ParseJson<ToggleOption>(item.Value);
				break;
			case OptionType.Slider:
				optionData = KUtility.ParseJson<SliderOption>(item.Value);
				break;
			case OptionType.TextInput:
				optionData = KUtility.ParseJson<ValueOption>(item.Value);
				break;
			case OptionType.Box:
				optionData = KUtility.ParseJson<BoxOption>(item.Value);
				break;
			default:
				optionData = KUtility.ParseJson<global::OptionData.OptionData>(item.Value);
				break;
			case OptionType.Invalid:
				continue;
			}
			if (optionData != null && (Debug.isDebugBuild || !optionData.DebugBuild))
			{
				optionData.Key = item.Key;
				Options.Add(optionData);
			}
		}
	}

	private void LoadOptionValue()
	{
		int i = 0;
		for (int count = Options.Count; i < count; i++)
		{
			if (!(Options[i] is ValueOption valueOption))
			{
				continue;
			}
			string text = $"option:{valueOption.Key}";
			switch (valueOption.Type)
			{
			case OptionType.Toggle:
			case OptionType.TextInput:
			case OptionType.Locale:
			{
				string value = PlayerPrefs.GetString(text, (string)null);
				if (string.IsNullOrEmpty(value))
				{
					value = valueOption.Default;
				}
				StringValueChanged(valueOption.Key, value, save: false);
				break;
			}
			case OptionType.Slider:
			{
				float result = PlayerPrefs.GetFloat(text, float.NaN);
				if (float.IsNaN(result))
				{
					float.TryParse(valueOption.Default, out result);
				}
				FloatValueChanged(valueOption.Key, result, save: false);
				break;
			}
			}
		}
	}

	public bool IsValidOption(global::OptionData.OptionData option)
	{
		return true;
	}

	public string StringValueChanged(string key, string value, bool save = true)
	{
		switch (key)
		{
		case "resolution":
			if (!ChangeResolution(value))
			{
				DeviceInfo.ChangeResolution(DeviceInfo.DefaultResolution);
				value = DeviceInfo.DefaultResolution.ToString().ToLower();
			}
			break;
		case "locale":
			ChangeLocale(value);
			break;
		case "portriat_mode":
			ChangeUsePortraitMode(value);
			break;
		}
		int i = 0;
		for (int count = Options.Count; i < count; i++)
		{
			if (Options[i] is ValueOption valueOption && valueOption.Key == key)
			{
				valueOption.Value = value;
				break;
			}
		}
		if (save)
		{
			PlayerPrefs.SetString($"option:{key}", value);
			PlayerPrefs.Save();
		}
		return value;
	}

	public float FloatValueChanged(string key, float value, bool save = true)
	{
		switch (key)
		{
		case "sfx_volume":
			ChangeSfxVolume(value);
			break;
		case "bgm_volume":
			ChangeBgmVolume(value);
			break;
		}
		int i = 0;
		for (int count = Options.Count; i < count; i++)
		{
			if (Options[i] is ValueOption valueOption && valueOption.Key == key)
			{
				valueOption.Value = value;
				break;
			}
		}
		if (save)
		{
			PlayerPrefs.SetFloat($"option:{key}", value);
			PlayerPrefs.Save();
		}
		return value;
	}

	public void ButtonClick(string key)
	{
		switch (key)
		{
		case "logout":
			Logout();
			break;
		case "font":
		{
			StringSelectPopup stringSelectPopup = UIManager.Popup.Tooltip<StringSelectPopup>();
			UIFontSetting component = ((Component)KSingleton<UIManager>.Instance().Font).GetComponent<UIFontSetting>();
			string item = component.FontNames[0];
			List<string> availableFontList = component.AvailableFontList;
			int index = availableFontList.IndexOf(item);
			stringSelectPopup.Set(availableFontList, OnSelectFont, index);
			stringSelectPopup.Show(60f);
			break;
		}
		case "escape_request":
			EscapeRequest();
			break;
		case "destroy_character":
			RemoveCharacter();
			break;
		case "show_baseplate":
		{
			NPCSInfo nPCSInfo = new NPCSInfo();
			nPCSInfo["Device"] = SystemInfo.deviceModel;
			nPCSInfo["Market"] = "Google Play";
			nPCSInfo["NPSN"] = ToyLoginHelper.NPSN;
			nPCSInfo["OS"] = SystemInfo.operatingSystem;
			try
			{
				nPCSInfo["PlayerName"] = PlayerBehavior.LocalPlayer.PlayerName;
			}
			catch (NullReferenceException)
			{
				nPCSInfo["PlayerName"] = string.Empty;
			}
			int? cachedFreq = GameSystem<StatisticsSystem>.Instance().CachedFreq;
			if (cachedFreq.HasValue)
			{
				nPCSInfo["PlayerFrequency"] = cachedFreq.Value.ToString();
			}
			else
			{
				nPCSInfo["PlayerFrequency"] = string.Empty;
			}
			nPCSInfo["EntityId"] = PlayerBehavior.LocalPlayer.EntityId.ToString();
			nPCSInfo["PlayerLevel"] = GameSystem<StatisticsSystem>.Instance().Level.ToString();
			nPCSInfo["Version"] = CurrentBundleVersion.GetClientVersion();
			NPAccount.Instance.ShowPlate(0, nPCSInfo);
			NPAccount.Instance.ShowBanner("2", null);
			break;
		}
		case "credit":
			UIManager.FindScript<CreditGroup>().Open();
			break;
		case "change_account":
			ConnectSNSAccount();
			break;
		case "leave":
			DeleteAccount();
			break;
		}
	}

	private bool ChangeResolution(string val)
	{
		if (string.IsNullOrEmpty(val))
		{
			return false;
		}
		try
		{
			DeviceInfo.Resolution resolution = (DeviceInfo.Resolution)(int)Enum.Parse(typeof(DeviceInfo.Resolution), val, ignoreCase: true);
			DeviceInfo.ChangeResolution(resolution);
		}
		catch (ArgumentException)
		{
			return false;
		}
		return true;
	}

	private void ChangeLocale(string val)
	{
		if (!string.IsNullOrEmpty(val))
		{
			KSingleton<GameManager>.Instance().SetLocale(val);
		}
	}

	private void ChangeSfxVolume(float val)
	{
		SoundManager.SetVolume(val);
		MusicManager.SetVolume(val);
	}

	private void ChangeBgmVolume(float val)
	{
		BGMManager.SetVolume(val);
	}

	private void ChangeUsePortraitMode(string val)
	{
		if (!string.IsNullOrEmpty(val))
		{
			ScreenOrientationController.PortraitModeType = val.ToEnum(ScreenOrientationController.PortraitModeUseType.None);
		}
	}

	private void RemoveCharacter()
	{
		UIManager.MessageBox.Show(T._("정말로 이렇게 정든 캐릭터를 지우시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				KSingleton<GameManager>.Instance().ForceMoveToTitle = true;
				Connections.Frontend.Send(new Cheat
				{
					_Cheat = "destroy"
				});
			}
		});
	}

	private void ConnectSNSAccount()
	{
		if (NPAccount.Instance.GetLoginType() == NPLoginType.NPLoginTypeGuest)
		{
			ToySNSConnector.ShowAccountMenu();
		}
		else
		{
			UIManager.MessageBox.Show(T._("이미 연동된 계정입니다."));
		}
	}

	private void DeleteAccount()
	{
		string text = null;
		text = ((NPAccount.Instance.GetLoginType() != NPLoginType.NPLoginTypeGuest) ? T._("탈퇴하시겠습니까?\n탈퇴하시면 모든 게임 데이터가 삭제됩니다.\n탈퇴 후 3일 안에 재접속하시면 복구할 수 있습니다.") : T._("탈퇴하시겠습니까?\n탈퇴하시면 모든 게임 데이터가 삭제됩니다.\n게스트 계정은 복구가 불가능합니다."));
		UIManager.MessageBox.Show(text, delegate(bool ok)
		{
			if (ok)
			{
				ToyLoginHelper.Leave(delegate(bool success)
				{
					if (success)
					{
						KSingleton<GameManager>.Instance().MoveToTitle();
					}
					else
					{
						UIManager.MessageBox.Show(T._("요청을 처리하지 못했습니다."));
					}
				});
			}
		});
	}

	private void Logout()
	{
		string text = null;
		text = ((NPAccount.Instance.GetLoginType() != NPLoginType.NPLoginTypeGuest) ? T._("정말로 로그아웃 하시겠습니까?") : T._("계정 연동을 하지 않은 상태로 로그아웃 시,\n다시 로그인할 수 없습니다.\n\n정말로 로그아웃 하시겠습니까?"));
		UIManager.MessageBox.Show(text, delegate(bool ok)
		{
			if (ok)
			{
				ToyLoginHelper.Logout(OnLogout);
			}
		});
	}

	private void OnLogout(bool success)
	{
		if (success)
		{
			KSingleton<GameManager>.Instance().MoveToTitle();
		}
		else
		{
			UIManager.MessageBox.Show(T._("로그아웃이 실패 하였습니다."));
		}
	}

	private void OnSelectFont(int index)
	{
		UIFontSetting component = ((Component)KSingleton<UIManager>.Instance().Font).GetComponent<UIFontSetting>();
		string customFont = component.AvailableFontList[index];
		component.SetCustomFont(customFont);
	}

	private void EscapeRequest()
	{
		double now = Connections.Frontend.GetPredictedServerTime();
		if (now < _escapeRequestAvailableAt)
		{
			UIManager.SystemMsg(LocalizeSystem.Format("#escape_request_cooltime", TimerSystem.TimeToString(_escapeRequestAvailableAt - now)));
			return;
		}
		ConfigGroup configGroup = UIManager.FindScript<ConfigGroup>();
		if ((Object)(object)configGroup != (Object)null)
		{
			configGroup.Close();
		}
		TimerData.Timer timer2 = new TimerData.Timer(GameManager.PlayerId, "escape", 10f);
		timer2.Finished += delegate(TimerData.Timer timer)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			if (!timer.IsInterrupt)
			{
				_escapeRequestAvailableAt = now + 600.0;
				Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
				float num = Random.Range(200f, 400f);
				float num2 = Random.Range(0f, (float)Math.PI * 2f);
				Vector3 pos = currentPosition + new Vector3(Mathf.Cos(num2), Mathf.Sin(num2)) * num;
				KSingleton<PlayerController>.Instance().Teleport(pos);
			}
		};
		TimerData.Timer.Play<DefaultProgressGauge>(timer2);
		PlayerBehavior.LocalPlayer.PlayAnimation("Avatar_Crying");
	}

	public static string GetPresteValue(PresetValue value)
	{
		return value switch
		{
			PresetValue.PlayerEntityId => GameManager.PlayerId.ToString(), 
			PresetValue.Facebook => ToyLoginHelper.IsConnectFacebook.ToString(), 
			PresetValue.GooglePlus => ToyLoginHelper.IsConnectGooglePlus.ToString(), 
			_ => null, 
		};
	}
}
