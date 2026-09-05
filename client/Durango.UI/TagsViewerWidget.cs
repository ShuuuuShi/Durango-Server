using System.Collections.Generic;
using Durango.Logic.Item;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class TagsViewerWidget : UIWidget
{
	[SerializeField]
	private TagItemWidget _majorTagBase;

	[SerializeField]
	private TagItemWidget _minorTagBase;

	private ListObjectPool<TagItemWidget> _majorTagControls;

	private ListObjectPool<TagItemWidget> _minorTagControls;

	private UIWidget _parentWidget;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_majorTagControls = new ListObjectPool<TagItemWidget>();
			_majorTagControls.BaseObject = _majorTagBase;
			_majorTagControls.UseBase = true;
			_minorTagControls = new ListObjectPool<TagItemWidget>();
			_minorTagControls.BaseObject = _minorTagBase;
			_minorTagControls.UseBase = true;
			if (bottomAnchor.target != null)
			{
				_parentWidget = bottomAnchor.target.GetComponent<UIWidget>();
			}
			if (_parentWidget == null && topAnchor.target != null)
			{
				_parentWidget = topAnchor.target.GetComponent<UIWidget>();
			}
		}
	}

	public void PrepareForHost(int hostWidth)
	{
		leftAnchor.target = null;
		rightAnchor.target = null;
		topAnchor.target = null;
		bottomAnchor.target = null;
		updateAnchors = UIRect.AnchorUpdate.OnEnable;
		_parentWidget = null;
		if (hostWidth > 80)
		{
			base.width = hostWidth;
		}
		TagItemWidget[] kids = GetComponentsInChildren<TagItemWidget>(true);
		for (int i = 0; i < kids.Length; i++)
		{
			TagItemWidget kid = kids[i];
			if (kid != null && kid != _majorTagBase && kid != _minorTagBase)
			{
				kid.gameObject.SetActive(value: false);
			}
		}
	}

	public bool Set(IEnumerable<TagData> tags)
	{
		SettingBegin();
		foreach (TagData tag in tags)
		{
			AddTagData(tag.Id, tag.Level);
		}
		return SettingEnd();
	}

	public void SettingBegin()
	{
		Init();
		_majorTagControls.BeginLoad();
		_minorTagControls.BeginLoad();
	}

	public bool SettingEnd()
	{
		_majorTagControls.EndLoad();
		_minorTagControls.EndLoad();
		UIUtility.UpdateAnchors(base.transform);
		float num = UpdateTagsLayout(12);
		if (num > 0f)
		{
			if (_parentWidget == null)
			{
				base.height = (int)num + 24;
			}
			else
			{
				_parentWidget.height = (int)num + 24;
				UpdateAnchors();
			}
			return true;
		}
		return false;
	}

	public void AddTagData(string id, int level)
	{
		if (SingletonDict<string, Tag>.TryGetValue(id, out var value) && value.Visible)
		{
			bool flag = value.IsMajor();
			ListObjectPool<TagItemWidget> listObjectPool = ((!flag) ? _minorTagControls : _majorTagControls);
			TagItemWidget next = listObjectPool.GetNext();
			next.Set(value, level);
			if (!flag)
			{
				int num = next.NameLabel.width + 20;
				next.width = num;
			}
		}
	}

	private float UpdateTagsLayout(int padding)
	{
		float num = 0f;
		float num2 = 0f;
		int num3 = base.width - padding * 2;
		int columns = 3;
		if (num3 < 200)
		{
			columns = 1;
		}
		else if (num3 < 360)
		{
			columns = 2;
		}
		int num4;
		float num5;
		if (columns == 3)
		{
			num4 = (num3 - 20) / 3;
			num5 = (float)(num3 - num4 * 3) / 2f;
		}
		else if (columns == 2)
		{
			num4 = (num3 - 10) / 2;
			num5 = 10f;
		}
		else
		{
			num4 = num3;
			num5 = 0f;
		}
		Vector3 vector = localCorners[1] + new Vector3(padding, -padding);
		float num6 = 0f;
		for (int i = 0; i < _majorTagControls.Count; i++)
		{
			int num7 = i / columns;
			int num8 = i - num7 * columns;
			if (num8 == 0 && num6 > 0f)
			{
				num += num6 + 10f;
				num6 = 0f;
			}
			TagItemWidget component = _majorTagControls[i].GetComponent<TagItemWidget>();
			component.width = num4;
			component.NameLabel.UpdateAnchors();
			component.height = Mathf.CeilToInt(0f - component.NameLabel.GetPosition(0f, 0f).y);
			component.transform.localPosition = vector + new Vector3((float)num8 * ((float)num4 + num5), 0f - num);
			num6 = Mathf.Max(num6, component.height);
			num2 = num + num6;
		}
		float num9 = 0f;
		int num10 = _minorTagControls.BaseObject.GetComponent<UIWidget>().height;
		num = ((!(num2 > 0f)) ? 0f : (num2 + 10f));
		for (int j = 0; j < _minorTagControls.Count; j++)
		{
			TagItemWidget component2 = _minorTagControls[j].GetComponent<TagItemWidget>();
			if (num9 > 0f && num9 + (float)component2.width > (float)num3)
			{
				num9 = 0f;
				num += (float)num10 + 10f;
				j--;
			}
			else
			{
				component2.transform.localPosition = vector + new Vector3(num9, 0f - num);
				num9 += (float)component2.width + 10f;
				num2 = num + (float)num10;
			}
		}
		return num2;
	}
}
