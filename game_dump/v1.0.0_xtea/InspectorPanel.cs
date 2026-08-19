using System;
using System.Collections.Generic;
using Shared.Animal;
using UnityEngine;

public class InspectorPanel : MonoBehaviour
{
	[Serializable]
	private class StatusIcon
	{
		public AnimalStatus Status;

		public SpriteData Icon;
	}

	private class PosInfo
	{
		public CharacterBehavior target;

		public GameObject inspector;

		public UISprite sprite;

		public AnimalStatus status;
	}

	[SerializeField]
	private StatusIcon[] _statusIcons;

	[SerializeField]
	private GameObject _inspectorUI;

	[SerializeField]
	private int _iconOffsetX;

	[SerializeField]
	private int _iconOffsetY;

	private Dictionary<AnimalStatus, SpriteData> _statusIconsDictionary;

	private List<PosInfo> _targets = new List<PosInfo>();

	private void OnEnable()
	{
		if (_statusIconsDictionary == null)
		{
			_statusIconsDictionary = new Dictionary<AnimalStatus, SpriteData>();
			for (int i = 0; i < _statusIcons.Length; i++)
			{
				StatusIcon statusIcon = _statusIcons[i];
				_statusIconsDictionary[statusIcon.Status] = statusIcon.Icon;
			}
		}
	}

	private void LateUpdate()
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		for (int num = _targets.Count - 1; num >= 0; num--)
		{
			PosInfo posInfo = _targets[num];
			if ((Object)(object)posInfo.target != (Object)null)
			{
				MainCamera mainCamera = KSingleton<MainCamera>.Instance();
				Transform headTransform = posInfo.target.GetHeadTransform();
				if (!((Object)(object)headTransform == (Object)null))
				{
					Vector3 position = headTransform.position;
					Vector3 localPosition = MainCamera.WorldToNGUIPos(position);
					localPosition.x += (float)_iconOffsetX / mainCamera.ZoomScreenRatio;
					localPosition.y += (float)_iconOffsetY / mainCamera.ZoomScreenRatio;
					posInfo.inspector.transform.localPosition = localPosition;
					if (posInfo.status != posInfo.target.Status)
					{
						SetStatusIcon(posInfo, posInfo.target.Status);
					}
				}
			}
			else
			{
				Remove(posInfo);
			}
		}
	}

	public void Add(CharacterBehavior o)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		PosInfo posInfo = new PosInfo();
		posInfo.target = o;
		ref GameObject inspector = ref posInfo.inspector;
		Object obj = Object.Instantiate((Object)(object)_inspectorUI, new Vector3(0f, 0f, 0f), Quaternion.identity);
		inspector = (GameObject)(object)((obj is GameObject) ? obj : null);
		PosInfo posInfo2 = posInfo;
		posInfo2.inspector.transform.parent = ((Component)this).transform;
		posInfo2.inspector.transform.localScale = _inspectorUI.transform.localScale;
		posInfo2.sprite = posInfo2.inspector.GetComponentInChildren<UISprite>();
		PetAI component = ((Component)o).GetComponent<PetAI>();
		if ((Object)(object)component != (Object)null)
		{
			component.NameChanged += NameChanged;
		}
		SetName(posInfo2, o);
		SetStatusIcon(posInfo2, AnimalStatus.Invalid);
		NGUITools.SetLayer(posInfo2.inspector, LayerMask.NameToLayer("NGUI"));
		_targets.Add(posInfo2);
	}

	private static void SetName(PosInfo posInfo, CharacterBehavior o)
	{
		Vehicle component = ((Component)o).GetComponent<Vehicle>();
		if ((Object)(object)component != (Object)null && component.HasDriver)
		{
			Inspector component2 = posInfo.inspector.GetComponent<Inspector>();
			component2.SetName(component.Name, component.OwnerName, showName: true, component.IsLocalPlayers);
		}
	}

	private void Remove(PosInfo info)
	{
		if (Object.op_Implicit((Object)(object)info.target))
		{
			PetAI component = ((Component)info.target).GetComponent<PetAI>();
			if ((Object)(object)component != (Object)null)
			{
				component.NameChanged -= NameChanged;
			}
		}
		Object.Destroy((Object)(object)info.inspector);
		_targets.Remove(info);
	}

	private void SetStatusIcon(PosInfo info, AnimalStatus status)
	{
		info.status = status;
		if (_statusIconsDictionary.TryGetValue(status, out var value) && (Object)(object)value.atlas != (Object)null)
		{
			((Component)info.sprite).gameObject.SetActive(true);
			value.Set(info.sprite);
		}
		else
		{
			((Component)info.sprite).gameObject.SetActive(false);
		}
	}

	private PosInfo FindTarget(CharacterBehavior target)
	{
		int count = _targets.Count;
		for (int i = 0; i < count; i++)
		{
			if ((Object)(object)_targets[i].target == (Object)(object)target)
			{
				return _targets[i];
			}
		}
		return null;
	}

	private void NameChanged(CharacterBehavior target)
	{
		PosInfo posInfo = FindTarget(target);
		if (posInfo != null)
		{
			SetName(posInfo, target);
		}
	}
}
