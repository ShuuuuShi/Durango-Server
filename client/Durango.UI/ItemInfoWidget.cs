using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class ItemInfoWidget : UIWidget
{
	[SerializeField]
	private ItemInfoView _infoView;

	[SerializeField]
	private ItemDetailView _detailView;

	[SerializeField]
	private UISprite _infoViewBg;

	[SerializeField]
	private UISprite _detailViewBg;

	[SerializeField]
	private UISprite _detailViewBorder;

	[SerializeField]
	private UITexture _infoViewBlur;

	[SerializeField]
	private UITexture _detailViewBlur;

	private bool _isInit;

	private RectLayoutComponent _layout;

	private bool _enableCraftLink;

	private float _detailExpandRatio;

	private float _detailViewOffset;

	public bool IsOpen { get; private set; }

	public ItemData CurrentItem { get; private set; }

	protected override void LateUpdate()
	{
		base.LateUpdate();
		SyncDetailViewOffset();
	}

	private void SyncDetailViewOffset()
	{
		float currentOffset = _detailView.CurrentOffset;
		if (_detailViewOffset == currentOffset)
		{
			return;
		}
		float num;
		if (currentOffset > 0f && _detailExpandRatio < 1f)
		{
			float b = (1f - _detailExpandRatio) * _infoView.ExpandHeight;
			num = Mathf.Min(currentOffset, b);
		}
		else
		{
			if (!(currentOffset < 0f) || !(_detailExpandRatio > 0f))
			{
				_detailViewOffset = currentOffset;
				return;
			}
			float b = (0f - _detailExpandRatio) * _infoView.ExpandHeight;
			num = Mathf.Max(currentOffset, b);
		}
		Vector3 currentMomentum = _detailView.ScrollView.currentMomentum;
		SetDetailExpandRatio(_detailExpandRatio + num / _infoView.ExpandHeight);
		_detailView.UpdateLayout();
		_detailView.MoveTo(0f, instant: true);
		_detailView.ScrollView.currentMomentum = currentMomentum;
		_detailViewOffset = 0f;
	}

	public void Init(bool enableCraftLink, Color infoBgColor, Color detailBgColor, bool bgBlur)
	{
		if (!_isInit)
		{
			_isInit = true;
			_layout = GetComponent<RectLayoutComponent>();
			if (infoBgColor != Color.clear)
			{
				_infoViewBg.color = infoBgColor;
			}
			if (detailBgColor != Color.clear)
			{
				_detailViewBg.color = detailBgColor;
			}
			_enableCraftLink = enableCraftLink;
			AnchorPoint anchorPoint = _detailViewBg.bottomAnchor;
			Transform target = base.transform.parent;
			_detailViewBorder.bottomAnchor.target = target;
			anchorPoint.target = target;
			_infoViewBlur.gameObject.SetActive(bgBlur);
			_detailViewBlur.gameObject.SetActive(bgBlur);
			_detailViewBlur.bottomAnchor.target = base.transform.parent;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if (Application.isPlaying)
		{
			UIPanel uIPanel = panel;
			UIPanel[] componentsInChildren = GetComponentsInChildren<UIPanel>(includeInactive: true);
			int i = 0;
			for (int num = componentsInChildren.Length; i < num; i++)
			{
				componentsInChildren[i].depth = uIPanel.depth + componentsInChildren[i].depth;
			}
		}
	}

	public void SetItemData(ItemData item, string warningText = null)
	{
		CurrentItem = item;
		if (item == null)
		{
			return;
		}
		if (item.Pet.HasValue)
		{
			Pet value = item.Pet.Value;
			if (string.IsNullOrEmpty(warningText) && item.IsDomesticatedPet() && !item.CanImprint())
			{
				warningText = T._("[icon=icon_make_alert] 생존 {0:lv:} 이상", item.Level);
			}
			SetPetData(value, warningText);
			return;
		}
		if (string.IsNullOrEmpty(warningText))
		{
			if (item.Unstable)
			{
				warningText = T._("사용하려면 안정화 <help>{0}</help> 필요", T._("불안정한 아이템들은 섬 간 이동시 사라져버리며, 사용이 제한됩니다. 안정화하려면 화물 워프홀을 통해 전송해야 합니다."));
			}
			else
			{
				using Reusable<List<string>> reusable = ReusableList<string>.Pop();
				List<string> value2 = reusable.Value;
				if (!item.Tradable)
				{
					value2.Add(T._("거래"));
				}
				if (!item.Dumpable)
				{
					value2.Add(T._("버리기"));
				}
				if (value2.Count > 0)
				{
					warningText = T._("[icon=icon_make_alert] {0:l:{}|, } 불가", value2);
				}
			}
		}
		_infoView.Set(item, warningText);
		int contentCount = item.ContentCount;
		int num = (int)item.GetFloatAttribute("capacity");
		ItemData itemData = ((contentCount <= 0 || num <= 0) ? item : item.GetContent(0));
		_detailView.Set(itemData, _enableCraftLink);
		SetDetailExpandRatio(0f);
	}

	public void SetPetData(Pet pet, string warningText = null)
	{
		CurrentItem = null;
		_infoView.Set(pet, warningText);
		_detailView.Set(pet);
		SetDetailExpandRatio(0f);
	}

	public void Open()
	{
		if (!IsOpen)
		{
			base.gameObject.SetActive(value: true);
			IsOpen = true;
		}
	}

	public void Close()
	{
		CurrentItem = null;
		base.gameObject.SetActive(value: false);
		IsOpen = false;
	}

	private void SetDetailExpandRatio(float ratio)
	{
		_detailExpandRatio = Mathf.Clamp01(ratio);
		_infoView.SetExpandRatio(1f - ratio);
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}
}
