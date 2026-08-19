using UnityEngine;

public class EquipStatWidget : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _statInfos;

	[SerializeField]
	private string[] _statInfosIcon;

	[SerializeField]
	private string[] _statInfosTextKey;

	[SerializeField]
	private Color _goodColor;

	[SerializeField]
	private Color _badColor;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			int num = Mathf.Min(_statInfosIcon.Length, _statInfosTextKey.Length);
			_statInfos.Set(num);
			for (int i = 0; i < num; i++)
			{
				EquipStatItemWidget component = _statInfos[i].GetComponent<EquipStatItemWidget>();
				component.Set(_statInfosIcon[i], LocalizeSystem.Get(_statInfosTextKey[i]), i > 0);
			}
		}
	}

	public void SetEquipInfo(EquipSystem.PlayerEquipInfo info)
	{
		SetEquipInfo(info, null);
	}

	public void SetEquipInfo(EquipSystem.PlayerEquipInfo before, EquipSystem.PlayerEquipInfo after)
	{
		Init();
		if (after == null)
		{
			after = before;
		}
		_statInfos[0].GetComponent<EquipStatItemWidget>().SetValue(DiffString(before.Attack, after.Attack));
		_statInfos[1].GetComponent<EquipStatItemWidget>().SetValue(DiffString(before.Defenses, after.Defenses));
		_statInfos[2].GetComponent<EquipStatItemWidget>().SetValue(DiffString(before.Accuracy, after.Accuracy));
	}

	private string DiffString(float before, float after)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		float num = after - before;
		string text = after.ToString("0.#");
		if (num < 0f)
		{
			text = UIManager.ColorBBCode(_badColor) + text;
		}
		else if (num > 0f)
		{
			text = UIManager.ColorBBCode(_goodColor) + text;
		}
		return text;
	}

	private void OnLayout(Point2 size)
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		Init();
		UIWidget component = ((Component)_statInfos.BaseObject.transform.parent).GetComponent<UIWidget>();
		int num = Mathf.RoundToInt((float)component.width / 3f);
		Vector3 localPosition = _statInfos.BaseObject.transform.localPosition;
		localPosition.x -= (float)num;
		for (int i = 0; i < _statInfos.Count; i++)
		{
			EquipStatItemWidget component2 = _statInfos[i].GetComponent<EquipStatItemWidget>();
			UIWidget component3 = ((Component)component2).GetComponent<UIWidget>();
			component3.width = num;
			component3.height = component.height;
			((Component)component2).transform.localPosition = localPosition + Vector3.right * (float)num * (float)i;
			component2.UpdateLayout();
		}
	}
}
