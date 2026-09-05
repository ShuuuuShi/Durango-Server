using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class RecipeMaterialInfoWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private Selectable _pinButton;

	[SerializeField]
	private RecipeMaterialInfoItem _materialBaseItem;

	private UIWidget _widget;

	private ListObjectPool<RecipeMaterialInfoItem> _materialItems;

	private IList<RecipeInfoWidget.SlotStruct> _slots;

	private bool _isInit;

	private TagsViewerWidget _tagsViewer;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public event Action PinClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_materialItems = new ListObjectPool<RecipeMaterialInfoItem>();
		_materialItems.BaseObject = _materialBaseItem;
		_materialItems.Init(delegate(RecipeMaterialInfoItem o)
		{
			UIUtility.ResetAndUpdateAnchors(o.transform);
			o.Clicked = (Action<RecipeMaterialInfoItem>)Delegate.Combine(o.Clicked, new Action<RecipeMaterialInfoItem>(OnClickMaterialItem));
		});
		Selectable pinButton = _pinButton;
		pinButton.Clicked = (Action)Delegate.Combine(pinButton.Clicked, (Action)delegate
		{
			if (this.PinClicked != null)
			{
				this.PinClicked();
			}
		});
	}

	public void Set(string title, IList<RecipeInfoWidget.SlotStruct> list)
	{
		Init();
		_slots = list;
		_titleLabel.text = title;
		_materialItems.Set(list.Count);
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			RecipeMaterialInfoItem recipeMaterialInfoItem = _materialItems[i];
			recipeMaterialInfoItem.Set(list[i]);
		}
		_titleWidget.SetPosition(Vector3.zero, 0.5f, 1f);
		Vector3 position = _titleWidget.GetPosition(0.5f, 0f);
		float num = UIUtility.WidgetsReposition(_materialItems, Vector3.down, position);
		num += (float)_titleWidget.height;
		num += ShowSlotKindTags(list, num);
		Widget.height = (int)num;
	}

	private float ShowSlotKindTags(IList<RecipeInfoWidget.SlotStruct> list, float yOffset)
	{
		if (!EnsureTagsViewer())
		{
			return 0f;
		}
		_tagsViewer.PrepareForHost(Widget.width > 80 ? Widget.width : 280);
		_tagsViewer.SettingBegin();
		HashSet<string> seen = new HashSet<string>();
		int n = 0;
		for (int i = 0; i < list.Count; i++)
		{
			OrTagFilter tags = list[i].Tags;
			if (tags == null)
			{
				continue;
			}
			for (int t = 0; t < tags.Length; t++)
			{
				TagFilterBase.Tag tag = tags[t];
				if (string.IsNullOrEmpty(tag.Id) || !seen.Add(tag.Id))
				{
					continue;
				}
				_tagsViewer.AddTagData(tag.Id, tag.Level < 1 ? 1 : tag.Level);
				n++;
			}
		}
		bool any = _tagsViewer.SettingEnd() && n > 0;
		_tagsViewer.gameObject.SetActive(any);
		if (!any)
		{
			return 0f;
		}
		_tagsViewer.transform.localPosition = new Vector3(0f, 0f - yOffset - 8f, 0f);
		return _tagsViewer.height + 16;
	}

	private bool EnsureTagsViewer()
	{
		if (_tagsViewer != null)
		{
			return true;
		}
		GameObject src = RecipeInfoWidget.FindBagTagsTemplate();
		if (src == null)
		{
			return false;
		}
		GameObject go = gameObject.AddChild(src);
		go.name = "SlotKindTags";
		go.SetActive(value: true);
		_tagsViewer = go.GetComponent<TagsViewerWidget>();
		if (_tagsViewer != null)
		{
			_tagsViewer.PrepareForHost(Widget.width > 80 ? Widget.width : 280);
		}
		return _tagsViewer != null;
	}

	public void SetPinButton(bool? isPin)
	{
		if (!isPin.HasValue || GameManager.Region.IsWarpRush())
		{
			_pinButton.gameObject.SetActive(value: false);
			return;
		}
		_pinButton.gameObject.SetActive(value: true);
		_pinButton.Selected = isPin.Value;
	}

	private void OnClickMaterialItem(RecipeMaterialInfoItem obj)
	{
		if (_slots != null)
		{
			int index = _materialItems.IndexOf(obj);
			RecipeInfoWidget.SlotStruct slotStruct = _slots.Get(index);
			SlotInfoPopup slotInfoPopup = UIManager.Popup.Tooltip<SlotInfoPopup>();
			slotInfoPopup.Set(slotStruct.Name, slotStruct.RequiredLevel, slotStruct.Tags, slotStruct.Materials, slotStruct.SourceInfos);
			slotInfoPopup.Show();
		}
	}
}
