using Durango.Logic.Item;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ItemContextRepair : ItemContextBase
{
	[SerializeField]
	private KeyValueLabel _simpleValueBase;

	private ListObjectPool<KeyValueLabel> _simpleValues;

	public override void Init()
	{
		base.Init();
		_simpleValues = new ListObjectPool<KeyValueLabel>();
		_simpleValues.BaseObject = _simpleValueBase;
		_simpleValues.UseBase = true;
	}

	public void Set([CanBeNull] ItemData item)
	{
		_simpleValues.BeginLoad();
		if (item != null)
		{
			if (item.HasTag("artifact_repair_kit") || item.HasTag("tool_repair_kit") || item.HasTag("clothes_repair_kit"))
			{
				base.HeaderText = T._("수리키트 정보");
				AddKeyValueInfo(T._("키트의 성능"), RepairKitsWidget.GetRepairPerformance(item).ToString());
			}
			else
			{
				base.HeaderText = T._("유지보수 정보");
				AddKeyValueInfo(T._("수리"), (!item.IsRepairable) ? T._("불가능") : T._("가능"));
				AddKeyValueInfo(T._("필요 성능"), (!item.IsRepairable) ? "-" : item.RepairRequirement.Value.RepairPerformance.ToString());
			}
		}
		_simpleValues.EndLoad();
		UpdateLayout();
	}

	private void AddKeyValueInfo(string key, string value)
	{
		KeyValueLabel next = _simpleValues.GetNext();
		next.Set(key, value);
	}

	private void UpdateLayout()
	{
		if (_simpleValues.Count == 0)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		int i = 0;
		for (int count = _simpleValues.Count; i < count; i++)
		{
			KeyValueLabel keyValueLabel = _simpleValues[i];
			keyValueLabel.UpdateLayout(_body.width);
		}
		Vector3[] array = _body.localCorners;
		float num = UIUtility.WidgetsReposition(_simpleValues, Vector3.down, Vector3.Lerp(array[1], array[2], 0.5f) + new Vector3(0f, -12f), 16f);
		_body.height = (int)(num + 24f);
		base.gameObject.SetActive(value: true);
	}
}
