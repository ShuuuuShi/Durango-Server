using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

public class MessageBoxInfoWidget : UIWidget, IUIInitializable
{
	[SerializeField]
	private MessageBoxInfoItem _baseItem;

	private ListObjectPool<MessageBoxInfoItem> _infos;

	private readonly List<KeyValuePair<SyncString, SyncString>> _texts = new List<KeyValuePair<SyncString, SyncString>>();

	public void Add(SyncString key, SyncString value)
	{
		_texts.Add(new KeyValuePair<SyncString, SyncString>(key, value));
	}

	public void Refresh()
	{
		if (_texts.Count == 0)
		{
			_infos.Clear();
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		_infos.Set(_texts.Count);
		float num = 0f;
		float num2 = 0f;
		for (int i = 0; i < _texts.Count; i++)
		{
			_infos[i].Set(_texts[i].Key, _texts[i].Value);
			num = Mathf.Max(num, _infos[i].KeyWidth);
			num2 = Mathf.Max(num2, _infos[i].TotalWidth);
		}
		base.height = _infos.Count * 28 + (_infos.Count - 1) * 25 + 60;
		Vector3 vector = localCorners[1];
		vector.x += ((float)base.width - num2) * 0.5f;
		vector.y -= 44f;
		for (int j = 0; j < _texts.Count; j++)
		{
			_infos[j].transform.localPosition = vector + new Vector3(num - _infos[j].KeyWidth, 0f);
			vector.y -= 53f;
		}
		_texts.Clear();
	}

	void IUIInitializable.Init()
	{
		_infos = new ListObjectPool<MessageBoxInfoItem>();
		_infos.BaseObject = _baseItem;
		_infos.Clear();
	}
}
