using System;
using MsgPack;
using UnityEngine;

public class PlayerPreviewScene : MonoBehaviour
{
	[SerializeField]
	private Camera _camera;

	private Vector3 _defaultPlayerPos;

	private string _hairName;

	private string _beardName;

	private float _bodySize;

	public PlayerBehavior PreviewPlayer { get; private set; }

	public event Action<CharacterCostume.CostumeType, ItemColor> ColorChanged;

	private void OnEnable()
	{
		((Behaviour)_camera).enabled = true;
		_camera.targetTexture = KSingleton<MainCamera>.Instance().TargetTexture;
		if (KSingleton<PlayerController>.HasInstance())
		{
			KSingleton<PlayerController>.Instance().IsGestureProcessed += OnGestureProcessed;
		}
	}

	private void OnDisable()
	{
		if (KSingleton<PlayerController>.HasInstance())
		{
			KSingleton<PlayerController>.Instance().IsGestureProcessed -= OnGestureProcessed;
		}
		if ((Object)(object)PreviewPlayer != (Object)null)
		{
			Object.Destroy((Object)(object)PreviewPlayer);
		}
	}

	private void OnGestureProcessed(PlayerController.Gesture gesture, Vector3 vector3, bool touchedUI, ref bool result)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)PreviewPlayer == (Object)null) && gesture == PlayerController.Gesture.Zoom)
		{
			PlayerZoom(vector3.z, Vector2.op_Implicit(vector3));
			result = true;
		}
	}

	public void CreatePlayer(bool isMale)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)PreviewPlayer != (Object)null)
		{
			Object.Destroy((Object)(object)PreviewPlayer);
		}
		PlayerBehavior playerBehavior = KSingleton<PlayerManager>.Instance().MakePlayerObject(isMale, Vector3.zero, 0uL, isPreview: true);
		((Component)_camera).gameObject.SetActive(true);
		playerBehavior.SetCostumeVisible(CharacterCostume.CostumeType.Head, isVisible: false);
		float num = (float)UIManager.ScreenWidth / (float)Screen.width;
		float num2 = (float)UIManager.ScreenHeight / (float)Screen.height;
		_camera.orthographicSize = 170f * (num / num2);
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(640f / num, (float)Screen.height * 0.45f - 30f / num, 0f);
		Ray val2 = _camera.ScreenPointToRay(val);
		playerBehavior.CurrentPosition = (_defaultPlayerPos = ((Ray)(ref val2)).origin - ((Ray)(ref val2)).origin.y / ((Ray)(ref val2)).direction.y * ((Ray)(ref val2)).direction);
		((Component)playerBehavior).gameObject.transform.localRotation = Quaternion.Euler(0f, 200f, 0f);
		playerBehavior.MainTransform.localScale = Vector3.one;
		PreviewPlayer = playerBehavior;
	}

	public void RemovePlayer()
	{
		if ((Object)(object)PreviewPlayer != (Object)null)
		{
			((Component)PreviewPlayer).gameObject.SetActive(true);
			Object.Destroy((Object)(object)((Component)PreviewPlayer).gameObject);
			PreviewPlayer = null;
		}
	}

	public string GetNextHair(bool male)
	{
		return ResourceSingleton<EquipmentTable>.Instance().GetNext(EquipmentTable.Category.Hair, male, _hairName);
	}

	public string GetNextBeard(bool male)
	{
		return ResourceSingleton<EquipmentTable>.Instance().GetNext(EquipmentTable.Category.Beard, male, _beardName);
	}

	public void SelectRandomCostume()
	{
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		EquipmentTable equipmentTable = ResourceSingleton<EquipmentTable>.Instance();
		ChangeHair(equipmentTable.GetRandom(EquipmentTable.Category.Hair, PreviewPlayer.IsMale));
		if (PreviewPlayer.IsMale)
		{
			string random = equipmentTable.GetRandom(EquipmentTable.Category.Beard, PreviewPlayer.IsMale);
			ChangeBeard(random);
		}
		float bodySize = Random.Range(0.85f, 1.1f);
		ChangeBodySize(bodySize);
		for (int i = 0; i < 8; i++)
		{
			CharacterCostume.CostumeType type = (CharacterCostume.CostumeType)i;
			int costumeColorCount = CharacterCostume.GetCostumeColorCount(type);
			ItemColor color = new ItemColor(costumeColorCount);
			bool flag = true;
			for (int j = 0; j < costumeColorCount; j++)
			{
				switch (i)
				{
				case 2:
					color[j] = ColorTableLoader.GetRandomSkinColor();
					break;
				case 3:
					color[j] = ColorTableLoader.GetRandomHairColor();
					break;
				case 5:
					color[j] = ColorTableLoader.GetRandomEyeColor();
					break;
				case 6:
					color[j] = ColorTableLoader.GetRandomLipColor(PreviewPlayer.IsMale);
					break;
				case 0:
				case 1:
					color[j] = ColorTableLoader.GetRandomClothColor();
					break;
				default:
					flag = false;
					break;
				}
			}
			if (flag)
			{
				ChangeCostumeColor((CharacterCostume.CostumeType)i, color);
			}
		}
	}

	public void SetCostumeColors(ItemColor[] colors)
	{
		for (int i = 0; i < 8; i++)
		{
			ChangeCostumeColor((CharacterCostume.CostumeType)i, colors[i]);
		}
	}

	public void SetCostume(MessagePackObjectDictionary costumeDict, bool randomFillEmptyProperties)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (randomFillEmptyProperties)
		{
			if (!costumeDict.ContainsKey(MessagePackObject.op_Implicit("eye_color")))
			{
				costumeDict.Add(MessagePackObject.op_Implicit("eye_color"), MessagePackObject.op_Implicit(KUtility.ToString(ColorTableLoader.GetRandomEyeColor())));
			}
			if (!costumeDict.ContainsKey(MessagePackObject.op_Implicit("lip_color")))
			{
				costumeDict.Add(MessagePackObject.op_Implicit("lip_color"), MessagePackObject.op_Implicit(KUtility.ToString(ColorTableLoader.GetRandomLipColor(PreviewPlayer.IsMale))));
			}
		}
		PlayerManager.SetCostumeFromDict(PreviewPlayer, costumeDict);
		MessagePackObject val = default(MessagePackObject);
		if (costumeDict.TryGetValue(MessagePackObject.op_Implicit("hair"), ref val))
		{
			_hairName = ((MessagePackObject)(ref val)).AsString().Replace("@", "/");
		}
		if (costumeDict.TryGetValue(MessagePackObject.op_Implicit("body_size"), ref val))
		{
			_bodySize = ((MessagePackObject)(ref val)).AsSingle();
		}
		if (this.ColorChanged != null)
		{
			for (int i = 0; i < 8; i++)
			{
				ChangeCostumeColor((CharacterCostume.CostumeType)i, PreviewPlayer.CostumeColors[i]);
			}
		}
	}

	public void GetCostumeInfo(out string hairName, out string beardName, out ItemColor[] colors, out float bodySize)
	{
		hairName = _hairName;
		beardName = _beardName;
		colors = PreviewPlayer.CostumeColors;
		bodySize = _bodySize;
	}

	public void ChangeBody(string body)
	{
		string key = $"{body}.fbx";
		string contains = ResourceSingleton<EquipmentTable>.Instance().GetContains(EquipmentTable.Category.Body, PreviewPlayer.IsMale, key);
		if (!string.IsNullOrEmpty(contains))
		{
			PreviewPlayer.ChangeCostume(contains);
		}
	}

	public void ChangeHair(string hair)
	{
		_hairName = hair;
		PreviewPlayer.ChangeCostume(_hairName);
	}

	public void ChangeBeard(string beard)
	{
		bool flag = !string.IsNullOrEmpty(beard);
		PreviewPlayer.SetCostumeVisible(CharacterCostume.CostumeType.Beard, flag);
		_beardName = beard;
		if (flag)
		{
			PreviewPlayer.ChangeCostume(beard);
		}
	}

	public void ChangeCostumeColor(CharacterCostume.CostumeType type, ItemColor color)
	{
		PreviewPlayer.ChangeCostumeColor(type, color);
		if (this.ColorChanged != null)
		{
			this.ColorChanged(type, color);
		}
	}

	public void ChangeBodySize(float bodySize)
	{
		_bodySize = PreviewPlayer.ChangeBodySize(bodySize);
	}

	public void PlayerRotate(float delta)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		PlayerBehavior previewPlayer = PreviewPlayer;
		if (!((Object)(object)previewPlayer == (Object)null))
		{
			Transform mainTransform = previewPlayer.MainTransform;
			mainTransform.localEulerAngles += Vector3.down * delta;
		}
	}

	public void PlayerMoveY(float delta)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		PlayerBehavior previewPlayer = PreviewPlayer;
		if (!((Object)(object)previewPlayer == (Object)null))
		{
			float x = previewPlayer.MainTransform.localPosition.x;
			x += delta;
			float x2 = _defaultPlayerPos.x;
			float x3 = previewPlayer.MainTransform.localScale.x;
			x = Mathf.Clamp(x, x2 - 100f * x3, x2 - 50f * (x3 - 1f));
			previewPlayer.MainTransform.localPosition = new Vector3(x, 0f, x);
		}
	}

	public void PlayerZoom(float zoom, Vector2 pos)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		PlayerBehavior previewPlayer = PreviewPlayer;
		if (!((Object)(object)previewPlayer == (Object)null))
		{
			Ray val = _camera.ScreenPointToRay(Vector2.op_Implicit(pos));
			Vector3 val2 = ((Ray)(ref val)).origin - ((Ray)(ref val)).origin.y / ((Ray)(ref val)).direction.y * ((Ray)(ref val)).direction;
			float num = (val2.x + val2.z) * 0.5f - previewPlayer.MainTransform.localPosition.x;
			num = Mathf.Max(0f, num);
			float z = PreviewPlayer.MainTransform.localScale.z;
			float num2 = z;
			z = Mathf.Clamp(z + zoom, 1f, 2f);
			PreviewPlayer.MainTransform.localScale = Vector3.one * z;
			PlayerMoveY(num - z * num / num2);
		}
	}
}
