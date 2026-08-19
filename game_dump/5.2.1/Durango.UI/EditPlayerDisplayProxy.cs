using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Social;
using Durango.Model;
using Durango.Player;
using Durango.Prologue;
using Durango.Utils;
using Durango.Utils.Extensions;
using Messages;
using Shared.Player;
using UnityEngine;

namespace Durango.UI;

public class EditPlayerDisplayProxy
{
	public const int MaleVoiceTypeCount = 7;

	public const int FelaleVoiceTypeCount = 7;

	public readonly Observable<bool> Gender = new Observable<bool>();

	public readonly Observable<Job?> Job = new Observable<Job?>();

	public readonly Observable<int> Portrait = new Observable<int>();

	public readonly Observable<int> PortraitBg = new Observable<int>();

	public readonly Observable<Color> PortraitBgColor = new Observable<Color>();

	public readonly Observable<int> VoiceType = new Observable<int>();

	public readonly Observable<float> BodySize = new Observable<float>();

	public readonly Observable<Color> BodyColor1 = new Observable<Color>();

	public readonly Observable<Color> BodyColor2 = new Observable<Color>();

	public readonly Observable<Color> BodyColor3 = new Observable<Color>();

	public readonly Observable<Color> HeadColor1 = new Observable<Color>();

	public readonly Observable<Color> HeadColor2 = new Observable<Color>();

	public readonly Observable<Color> HeadColor3 = new Observable<Color>();

	public readonly Observable<Color> SkinColor = new Observable<Color>();

	public readonly Observable<Color> HairColor = new Observable<Color>();

	public readonly Observable<Color> EyeColor = new Observable<Color>();

	public readonly Observable<Color> LipColor = new Observable<Color>();

	public readonly Observable<string> Hair = new Observable<string>();

	public readonly Observable<string> Beard = new Observable<string>();

	public readonly Observable<string> Head = new Observable<string>();

	public readonly Observable<string> Body = new Observable<string>();

	private PlayerCostumeTable.ClothState? _clothState;

	private bool _isDirty;

	private readonly string[] _defaultHead = new string[2];

	private readonly string[] _defaultBody = new string[2];

	private readonly string[] _tornBody = new string[2];

	private readonly string[] _nudeBody = new string[2];

	private readonly string[] _hairs = new string[2];

	private readonly string[] _beard = new string[2];

	private readonly Observable<PlayerBehavior> _preview = new Observable<PlayerBehavior>();

	public string DefaultHead
	{
		get
		{
			if ((bool)Gender)
			{
				return _defaultHead[0] ?? _defaultHead[1];
			}
			return _defaultHead[1] ?? _defaultHead[0];
		}
	}

	public string DefaultBody
	{
		get
		{
			if ((bool)Gender)
			{
				return _defaultBody[0] ?? _defaultBody[1];
			}
			return _defaultBody[1] ?? _defaultBody[0];
		}
	}

	public string TornBody
	{
		get
		{
			if ((bool)Gender)
			{
				return _tornBody[0] ?? _tornBody[1];
			}
			return _tornBody[1] ?? _tornBody[0];
		}
	}

	public string NudeBody
	{
		get
		{
			if ((bool)Gender)
			{
				return _nudeBody[0] ?? _nudeBody[1];
			}
			return _nudeBody[1] ?? _nudeBody[0];
		}
	}

	public Observable<PlayerBehavior> Preview => _preview;

	public EditPlayerDisplayProxy()
	{
		Observable<bool> gender = Gender;
		gender.Changed = (Action<bool>)Delegate.Combine(gender.Changed, new Action<bool>(OnGenderChanged));
		Observable<Job?> job = Job;
		job.Changed = (Action<Job?>)Delegate.Combine(job.Changed, new Action<Job?>(OnJobChanged));
		Observable<Color> bodyColor = BodyColor1;
		bodyColor.Changed = (Action<Color>)Delegate.Combine(bodyColor.Changed, (Action<Color>)delegate
		{
			SetDirty();
		});
		Observable<Color> bodyColor2 = BodyColor2;
		bodyColor2.Changed = (Action<Color>)Delegate.Combine(bodyColor2.Changed, (Action<Color>)delegate
		{
			SetDirty();
		});
		Observable<Color> bodyColor3 = BodyColor3;
		bodyColor3.Changed = (Action<Color>)Delegate.Combine(bodyColor3.Changed, (Action<Color>)delegate
		{
			SetDirty();
		});
		Observable<Color> headColor = HeadColor1;
		headColor.Changed = (Action<Color>)Delegate.Combine(headColor.Changed, (Action<Color>)delegate
		{
			SetDirty();
		});
		Observable<Color> headColor2 = HeadColor2;
		headColor2.Changed = (Action<Color>)Delegate.Combine(headColor2.Changed, (Action<Color>)delegate
		{
			SetDirty();
		});
		Observable<Color> headColor3 = HeadColor3;
		headColor3.Changed = (Action<Color>)Delegate.Combine(headColor3.Changed, (Action<Color>)delegate
		{
			SetDirty();
		});
		Observable<Color> skinColor = SkinColor;
		skinColor.Changed = (Action<Color>)Delegate.Combine(skinColor.Changed, (Action<Color>)delegate
		{
			SetDirty();
		});
		Observable<Color> hairColor = HairColor;
		hairColor.Changed = (Action<Color>)Delegate.Combine(hairColor.Changed, (Action<Color>)delegate
		{
			SetDirty();
		});
		Observable<string> hair = Hair;
		hair.Changed = (Action<string>)Delegate.Combine(hair.Changed, (Action<string>)delegate(string value)
		{
			SetDirty();
			_hairs[(!Gender) ? 1u : 0u] = value;
		});
		Observable<string> beard = Beard;
		beard.Changed = (Action<string>)Delegate.Combine(beard.Changed, (Action<string>)delegate(string value)
		{
			SetDirty();
			_beard[(!Gender) ? 1u : 0u] = value;
		});
		Observable<string> head = Head;
		head.Changed = (Action<string>)Delegate.Combine(head.Changed, (Action<string>)delegate
		{
			SetDirty();
		});
		Observable<string> body = Body;
		body.Changed = (Action<string>)Delegate.Combine(body.Changed, (Action<string>)delegate
		{
			SetDirty();
		});
		Observable<float> bodySize = BodySize;
		bodySize.Changed = (Action<float>)Delegate.Combine(bodySize.Changed, (Action<float>)delegate
		{
			SetDirty();
		});
	}

	public void SetClothState(PlayerCostumeTable.ClothState? state)
	{
		_clothState = state;
		if (state.HasValue)
		{
			switch (state.Value)
			{
			case PlayerCostumeTable.ClothState.Normal:
				Head.Value = DefaultHead;
				Body.Value = DefaultBody;
				break;
			case PlayerCostumeTable.ClothState.Torn:
				Head.Value = null;
				Body.Value = TornBody;
				break;
			case PlayerCostumeTable.ClothState.Nothing:
				Head.Value = null;
				Body.Value = NudeBody;
				break;
			}
		}
	}

	public void Set(bool isMale, PlayerDisplay display)
	{
		for (int i = 0; i < 2; i++)
		{
			_defaultHead[i] = null;
			_defaultBody[i] = null;
			_tornBody[i] = null;
			_nudeBody[i] = null;
		}
		_clothState = null;
		Gender.Value = isMale;
		SetDisplay(display);
	}

	public void SetDefaultHead(string m, string f)
	{
		_defaultHead[0] = m;
		_defaultHead[1] = f;
		PlayerCostumeTable.ClothState? clothState = _clothState;
		if (clothState.HasValue && _clothState.Value == PlayerCostumeTable.ClothState.Normal)
		{
			SetDirty();
		}
	}

	public void SetDefaultBody(string m, string f)
	{
		_defaultBody[0] = m;
		_defaultBody[1] = f;
		PlayerCostumeTable.ClothState? clothState = _clothState;
		if (clothState.HasValue && _clothState.Value == PlayerCostumeTable.ClothState.Normal)
		{
			SetDirty();
		}
	}

	public void SetTornBody(string m, string f)
	{
		_tornBody[0] = m;
		_tornBody[1] = f;
		PlayerCostumeTable.ClothState? clothState = _clothState;
		if (clothState.HasValue && _clothState.Value == PlayerCostumeTable.ClothState.Torn)
		{
			SetDirty();
		}
	}

	public void SetNudeBody(string m, string f)
	{
		_nudeBody[0] = m;
		_nudeBody[1] = f;
		PlayerCostumeTable.ClothState? clothState = _clothState;
		if (clothState.HasValue && _clothState.Value == PlayerCostumeTable.ClothState.Nothing)
		{
			SetDirty();
		}
	}

	private void SetDisplay(PlayerDisplay display)
	{
		Portrait.Value = display.Portrait;
		PortraitBg.Value = display.PortraitBg;
		PortraitBgColor.Value = NGUIText.ParseColor(display.PortraitBgColor);
		VoiceType.Value = display.VoiceType;
		BodySize.Value = display.BodySize;
		BodyColor1.Value = ParseColor(display.BodyColor, 0, Color.gray);
		BodyColor2.Value = ParseColor(display.BodyColor, 1, BodyColor1);
		BodyColor3.Value = ParseColor(display.BodyColor, 2, BodyColor1);
		HeadColor1.Value = ParseColor(display.HeadColor, 0, Color.gray);
		HeadColor2.Value = ParseColor(display.HeadColor, 1, HeadColor1);
		HeadColor3.Value = ParseColor(display.HeadColor, 2, HeadColor1);
		HairColor.Value = ParseColor(display.HairColor, Color.gray);
		SkinColor.Value = ParseColor(display.SkinColor, Color.gray);
		LipColor.Value = ParseColor(display.LipColor, Color.gray);
		EyeColor.Value = ParseColor(display.EyeColor, Color.gray);
		Hair.Value = display.Hair;
		Body.Value = display.Body;
		Head.Value = display.Head;
		Beard.Value = display.Beard;
	}

	private Color ParseColor(string[] texts, int index, Color defaultColor)
	{
		if (texts == null || index < 0 || index >= texts.Length)
		{
			return defaultColor;
		}
		return ParseColor(texts[index], defaultColor);
	}

	private Color ParseColor(string text, Color defaultColor)
	{
		if (text == null || text.Length < 6)
		{
			return defaultColor;
		}
		return NGUIText.ParseColor(text);
	}

	private void OnGenderChanged(bool isMale)
	{
		MakePreview();
		if (_clothState.HasValue)
		{
			SetClothState(PlayerCostumeTable.ClothState.Normal);
		}
		int num = ((!isMale) ? 1 : 0);
		string text = _hairs[num];
		if (text == null)
		{
			text = ResourceSingleton<PlayerCostumeTable>.Instance().GetRandom(PlayerCostumeTable.Category.Hair, isMale).AssetBundlePathBase ?? string.Empty;
		}
		string text2 = _beard[num];
		if (text2 == null)
		{
			text2 = ResourceSingleton<PlayerCostumeTable>.Instance().GetRandom(PlayerCostumeTable.Category.Beard, isMale).AssetBundlePathBase ?? string.Empty;
		}
		Hair.Value = text;
		Beard.Value = text2;
	}

	private void OnJobChanged(Job? job)
	{
		if (job.HasValue)
		{
			PlayerCostumeTable playerCostumeTable = ResourceSingleton<PlayerCostumeTable>.Instance();
			for (int i = 0; i < 2; i++)
			{
				_defaultBody[i] = playerCostumeTable.GetPlayerDefaultBodyModelAssetBundlePath(i == 0, (int)job.Value, PlayerCostumeTable.ClothState.Normal);
				_tornBody[i] = playerCostumeTable.GetPlayerDefaultBodyModelAssetBundlePath(i == 0, (int)job.Value, PlayerCostumeTable.ClothState.Torn);
				_nudeBody[i] = playerCostumeTable.GetPlayerDefaultBodyModelAssetBundlePath(i == 0, (int)job.Value, PlayerCostumeTable.ClothState.Nothing);
			}
			SetClothState(PlayerCostumeTable.ClothState.Normal);
		}
	}

	private void SetDirty()
	{
		_isDirty = true;
	}

	public void MakePreview()
	{
		ReleasePreview();
		bool isPrologueMode = GameManager.IsPrologueMode;
		PlayerManager playerManager = Singleton<PlayerManager>.Instance();
		bool male = Gender;
		bool loadClips = !isPrologueMode;
		PlayerBehavior playerBehavior = playerManager.MakePreview(male, null, 180f, loadClips);
		if (isPrologueMode)
		{
			Singleton<PrologueManager>.Instance().SetPrologueModelAnimation(playerBehavior, Gender);
		}
		playerBehavior.ChangeEquipment(null);
		playerBehavior.PlayMotionForcely("Barehand_Stand", 1f, immediately: true);
		_preview.Value = playerBehavior;
		_preview.Value.transform.localScale = Vector3.zero;
		SetDirty();
	}

	public void ReleasePreview()
	{
		if (!(_preview.Value == null))
		{
			UnityEngine.Object.Destroy(_preview.Value.gameObject);
			_preview.Value = null;
		}
	}

	public void UpdatePreview()
	{
		if (_isDirty)
		{
			RefreshPreview();
		}
	}

	private void RefreshPreview()
	{
		_isDirty = false;
		if ((bool)_preview.Value)
		{
			PlayerManager.SetDisplay(Preview, MakeDisplay());
		}
	}

	public PlayerDisplay MakeDisplay()
	{
		PlayerDisplay result = default(PlayerDisplay);
		result.Portrait = Portrait;
		result.PortraitBg = PortraitBg;
		result.PortraitBgColor = NGUIText.EncodeColor(PortraitBgColor);
		result.VoiceType = VoiceType;
		result.BodySize = BodySize;
		result.BodyColor = new string[3]
		{
			NGUIText.EncodeColor(BodyColor1),
			NGUIText.EncodeColor(BodyColor2),
			NGUIText.EncodeColor(BodyColor3)
		};
		result.HeadColor = new string[3]
		{
			NGUIText.EncodeColor(HeadColor1),
			NGUIText.EncodeColor(HeadColor2),
			NGUIText.EncodeColor(HeadColor3)
		};
		result.SkinColor = NGUIText.EncodeColor(SkinColor);
		result.HairColor = NGUIText.EncodeColor(HairColor);
		result.EyeColor = NGUIText.EncodeColor(EyeColor);
		result.LipColor = NGUIText.EncodeColor(LipColor);
		result.Hair = Hair;
		result.Beard = Beard;
		result.Head = Head;
		result.Body = Body;
		return result;
	}

	public ChangePlayerDisplay MakeChangePlayerDisplay()
	{
		ChangePlayerDisplay result = default(ChangePlayerDisplay);
		result.Gender = ((!Gender) ? "female" : "male");
		result.Portrait = Portrait;
		result.PortraitBg = PortraitBg;
		result.PortraitBgColor = NGUIText.EncodeColor(PortraitBgColor);
		result.VoiceType = VoiceType;
		result.BodySize = BodySize;
		result.BodyColor = new string[3]
		{
			NGUIText.EncodeColor(BodyColor1),
			NGUIText.EncodeColor(BodyColor2),
			NGUIText.EncodeColor(BodyColor3)
		};
		result.HeadColor = new string[3]
		{
			NGUIText.EncodeColor(HeadColor1),
			NGUIText.EncodeColor(HeadColor2),
			NGUIText.EncodeColor(HeadColor3)
		};
		result.SkinColor = NGUIText.EncodeColor(SkinColor);
		result.HairColor = NGUIText.EncodeColor(HairColor);
		result.EyeColor = NGUIText.EncodeColor(EyeColor);
		result.LipColor = NGUIText.EncodeColor(LipColor);
		result.Hair = Hair;
		result.Beard = Beard;
		return result;
	}

	public PortraitBuilder.Argument GetPortraitArgument()
	{
		PortraitBuilder.Argument result = PortraitBuilder.MakeArgument(Portrait, PortraitBg, PortraitBgColor, Gender, PortraitEmotion.Normal, SkinColor, HairColor, EyeColor, LipColor);
		result.Mask = null;
		return result;
	}

	public void RandomCostume()
	{
		PlayerDisplay display = MakeDisplay();
		if (Job.Value.HasValue)
		{
			FillRandomPlayerDisplayData(Gender, Job.Value.Value, ref display);
		}
		else
		{
			FillRandomPlayerDisplayData(Gender, ref display);
		}
		FillRandomPortrait(Gender, ref display);
		SetDisplay(display);
		RandomVoice();
	}

	public void RandomVoice()
	{
		VoiceType.Value = UnityEngine.Random.Range(0, (!Gender) ? 7 : 7) + 1;
	}

	public static void FillRandomPlayerDisplayData(bool isMale, Job job, ref PlayerDisplay display)
	{
		FillRandomPlayerDisplayData(isMale, ref display);
		string[] suitableClothColor = GetSuitableClothColor((int)job);
		display.BodyColor = ((suitableClothColor == null) ? GetRandomClothColor(CharacterCostume.CostumeType.Body.ToRequiredColorCount()) : suitableClothColor);
		display.HeadColor = GetRandomClothColor(CharacterCostume.CostumeType.Body.ToRequiredColorCount());
	}

	public static void FillRandomPlayerDisplayData(bool isMale, ref PlayerDisplay display)
	{
		PlayerCostumeTable playerCostumeTable = ResourceSingleton<PlayerCostumeTable>.Instance();
		display.Hair = playerCostumeTable.GetRandom(PlayerCostumeTable.Category.Hair, isMale).AssetBundlePathBase;
		display.Beard = playerCostumeTable.GetRandom(PlayerCostumeTable.Category.Beard, isMale).AssetBundlePathBase;
		display.BodySize = UnityEngine.Random.value * 0.25f + 0.85f;
		display.HairColor = ColorTableLoader.GetRandom("color_hair.raw").ToHex();
		display.SkinColor = ColorTableLoader.GetRandom("color_skin.raw").ToHex();
		display.EyeColor = ColorTableLoader.GetRandom("color_eyes.raw").ToHex();
		display.LipColor = ColorTableLoader.GetRandom((!isMale) ? "color_lips_female.raw" : "color_lips_male.raw").ToHex();
	}

	public static void FillRandomPortrait(bool isMale, ref PlayerDisplay display)
	{
		PortraitBuilder portraitBuilder = ResourceSingleton<PortraitBuilder>.Instance();
		if (!(portraitBuilder == null))
		{
			display.Portrait = UnityEngine.Random.Range(0, int.MaxValue) % portraitBuilder.GetPortraitCount(isMale);
			display.PortraitBg = UnityEngine.Random.Range(0, int.MaxValue) % portraitBuilder.GetPortraitBgCount();
			display.PortraitBgColor = ColorTableLoader.GetRandom("color_portrait_bg.raw").ToHex();
		}
	}

	private static string[] GetSuitableClothColor(int jobIndex)
	{
		string targetModelKey = PlayerCostumeTable.NormalBodyModels.Get(jobIndex);
		if (string.IsNullOrEmpty(targetModelKey))
		{
			return null;
		}
		ClothesColorTableInfo clothesColorTableInfo = ColorTableLoader.ColorTableClothesMap.Find((ClothesColorTableInfo elem) => elem.Keyword.Contains(targetModelKey));
		if (clothesColorTableInfo == null)
		{
			return null;
		}
		Color[] all = ColorTableLoader.GetAll("color_create.raw");
		string[] array = new string[CharacterCostume.CostumeType.Body.ToRequiredColorCount()];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			string text = clothesColorTableInfo.ColorTableNames.Get(i);
			if (!string.IsNullOrEmpty(text))
			{
				Color targetColor = ColorTableLoader.GetRandom(text);
				Color c = all.MinBy((Color elem) => (targetColor - elem).SqrMagnitude());
				array[i] = NGUIText.EncodeColor(c);
			}
		}
		return array;
	}

	private static string[] GetRandomClothColor(int count)
	{
		return ColorTableLoader.GetAll("color_create.raw").ShuffleTake(count).Select(NGUIText.EncodeColor)
			.ToArray();
	}

	public static PlayerDisplay ParseCostume(Dictionary<string, string> costumes)
	{
		PlayerDisplay result = default(PlayerDisplay);
		if (costumes.TryGetValue("hair", out var value))
		{
			result.Hair = value.Replace("@", "/");
		}
		if (costumes.TryGetValue("default_body", out value))
		{
			result.DefaultBody = value;
		}
		if (costumes.TryGetValue("default_inner", out value))
		{
			result.DefaultInner = value;
		}
		if (costumes.TryGetValue("body", out value))
		{
			result.Body = value;
		}
		if (costumes.TryGetValue("head", out value))
		{
			result.Head = value;
		}
		if (costumes.TryGetValue("beard", out value))
		{
			result.Beard = value;
		}
		if (costumes.TryGetValue("hair", out value))
		{
			result.Hair = value;
		}
		if (costumes.TryGetValue("equip", out value))
		{
			result.Equip = value;
		}
		if (costumes.TryGetValue("body_size", out value))
		{
			result.BodySize = value.ToFloat();
		}
		result.VoiceType = (costumes.TryGetValue("voice_type", out value) ? value.ToInt() : 0);
		if (costumes.TryGetValue("portrait", out value))
		{
			result.Portrait = value.ToInt();
		}
		if (costumes.TryGetValue("portrait_bg", out value))
		{
			result.PortraitBg = value.ToInt();
		}
		if (costumes.TryGetValue("portrait_bg_color", out value))
		{
			result.PortraitBgColor = value;
		}
		result.Invisible = costumes.TryGetValue("invisible", out value) && bool.Parse(value);
		string[] array = ((result.BodyColor != null) ? result.BodyColor : new string[3] { "000000", "000000", "000000" });
		for (int i = 0; i < 3; i++)
		{
			string key = "body_color_" + i;
			if (costumes.TryGetValue(key, out value))
			{
				array[i] = value;
			}
		}
		result.BodyColor = array;
		array = ((result.HeadColor != null) ? result.HeadColor : new string[3] { "000000", "000000", "000000" });
		for (int j = 0; j < 3; j++)
		{
			string key2 = "head_color_" + j;
			if (costumes.TryGetValue(key2, out value))
			{
				array[j] = value;
			}
		}
		result.HeadColor = array;
		if (costumes.TryGetValue("skin_color", out value))
		{
			result.SkinColor = value;
		}
		if (costumes.TryGetValue("hair_color", out value))
		{
			result.HairColor = value;
		}
		if (costumes.TryGetValue("eye_color", out value))
		{
			result.EyeColor = value;
		}
		if (costumes.TryGetValue("lip_color", out value))
		{
			result.LipColor = value;
		}
		return result;
	}
}
