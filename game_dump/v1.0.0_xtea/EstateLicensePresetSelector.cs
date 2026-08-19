using System;
using System.Collections.Generic;
using Estate;
using L10N;
using Shared.Estate;
using UnityEngine;

public class EstateLicensePresetSelector : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _nodes;

	private bool _isInit;

	public event Action<AccessRights> RightChanged;

	private void Init()
	{
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		KeyValuePair<PresetLicense, AccessRights>[] presets = EstateSystem.Presets;
		_nodes.Set(presets.Length);
		for (int i = 0; i < presets.Length; i++)
		{
			EstateLicensePresetSelectorNode component = _nodes[i].GetComponent<EstateLicensePresetSelectorNode>();
			component.SetText(presets[i].Key.GetName());
			component.Clicked = OnClickPresetNode;
		}
		EstateLicensePresetSelectorNode estateLicensePresetSelectorNode = ((ListObjectPoolBase<GameObject>)_nodes).Add<EstateLicensePresetSelectorNode>();
		estateLicensePresetSelectorNode.SetText(T._("커스텀"));
		estateLicensePresetSelectorNode.Clicked = null;
		UIWidget component2 = _nodes.BaseObject.GetComponent<UIWidget>();
		UIWidget component3 = ((Component)this).GetComponent<UIWidget>();
		float num = component3.width - component2.width * _nodes.Count;
		if (_nodes.Count > 1)
		{
			num /= (float)(_nodes.Count - 1);
		}
		Vector3 pos = Vector3.Lerp(component3.localCorners[0], component3.localCorners[1], 0.5f);
		for (int j = 0; j < _nodes.Count; j++)
		{
			EstateLicensePresetSelectorNode component4 = _nodes[j].GetComponent<EstateLicensePresetSelectorNode>();
			component4.Widget.SetPosition(pos, 0f, 0.5f);
			pos.x += num + (float)component2.width;
			if (j > 0)
			{
				component4.EnableLine((int)num);
			}
			else
			{
				component4.DisableLine();
			}
		}
	}

	public void Set(AccessRights right)
	{
		Init();
		KeyValuePair<PresetLicense, AccessRights>[] presets = EstateSystem.Presets;
		int preset = -1;
		for (int i = 0; i < presets.Length; i++)
		{
			if (presets[i].Value == right)
			{
				preset = i;
				break;
			}
		}
		SetPreset(preset);
	}

	private void OnClickPresetNode()
	{
		int num = _nodes.IndexOf(((Component)Selectable.Current).gameObject);
		if (this.RightChanged != null)
		{
			this.RightChanged(EstateSystem.Presets[num].Value);
		}
	}

	private void SetPreset(int index)
	{
		KeyValuePair<PresetLicense, AccessRights>[] presets = EstateSystem.Presets;
		if (index < 0 || index >= presets.Length)
		{
			index = presets.Length;
		}
		for (int i = 0; i < _nodes.Count; i++)
		{
			Selectable component = _nodes[i].GetComponent<Selectable>();
			component.Select = index == i;
		}
	}
}
