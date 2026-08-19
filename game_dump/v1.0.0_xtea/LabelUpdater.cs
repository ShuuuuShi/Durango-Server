using System;
using UnityEngine;

[RequireComponent(typeof(UILabel))]
public class LabelUpdater : MonoBehaviour
{
	public Action<LabelUpdater> Updated;

	private SyncString _syncString;

	private float _updateAt;

	private UISpriteLabel _spriteLabel;

	private UILabel _label;

	private void OnDisable()
	{
		((Behaviour)this).enabled = false;
		Object.Destroy((Object)(object)this);
	}

	private void Update()
	{
		if (_updateAt < Time.time)
		{
			LabelUpdate();
		}
	}

	private void LabelUpdate()
	{
		float period;
		string text = _syncString.Get(out period);
		if ((Object)(object)_spriteLabel == (Object)null)
		{
			_label.text = text;
		}
		else
		{
			_spriteLabel.text = text;
		}
		_updateAt = Time.time + period;
		if (Updated != null)
		{
			Updated(this);
		}
	}

	private static LabelUpdater Get(GameObject obj)
	{
		LabelUpdater labelUpdater = obj.GetComponent<LabelUpdater>();
		if ((Object)(object)labelUpdater == (Object)null || !((Behaviour)labelUpdater).enabled)
		{
			labelUpdater = obj.AddComponent<LabelUpdater>();
		}
		return labelUpdater;
	}

	public static LabelUpdater Set(UILabel label, SyncString synsString)
	{
		LabelUpdater labelUpdater = Get(((Component)label).gameObject);
		labelUpdater._label = label;
		labelUpdater._spriteLabel = null;
		labelUpdater._syncString = synsString;
		labelUpdater._updateAt = 0f;
		labelUpdater.Update();
		return labelUpdater;
	}

	public static LabelUpdater Set(UISpriteLabel label, SyncString synsString)
	{
		LabelUpdater labelUpdater = Get(((Component)label).gameObject);
		labelUpdater._label = null;
		labelUpdater._spriteLabel = label;
		labelUpdater._syncString = synsString;
		labelUpdater._updateAt = 0f;
		labelUpdater.Update();
		return labelUpdater;
	}
}
