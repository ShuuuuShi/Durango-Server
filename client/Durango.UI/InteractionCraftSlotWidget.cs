using System;
using System.Linq;
using Crafting;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using InteractionData;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI;

public class InteractionCraftSlotWidget : UIWidget
{
	public static readonly Interaction[] EmptyInteractionPriority = new Interaction[2]
	{
		Interaction.Craft,
		Interaction.Dye
	};

	[SerializeField]
	private UIWidget _emphasisBorder;

	[SerializeField]
	private GlitteringDots _emphasisDots;

	[SerializeField]
	private ItemIconTex _itemTexture;

	[SerializeField]
	private GameObject _emptyIcon;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private ItemGradeViewer _itemGradeViewer;

	[SerializeField]
	private ItemModifiedCountViewer _itemModifiedCountViewer;

	[SerializeField]
	private GameObject _cancelButton;

	[SerializeField]
	private UILabel _timerLabel;

	[SerializeField]
	private UISprite _timerSprite;

	private bool _isCancelMode;

	private CraftedResult? _crafted;

	private Messages.Crafting? _crafting;

	private bool _isActive;

	public bool IsEmpty
	{
		get
		{
			Messages.Crafting? crafting = _crafting;
			int result;
			if (!crafting.HasValue)
			{
				CraftedResult? crafted = _crafted;
				result = ((!crafted.HasValue) ? 1 : 0);
			}
			else
			{
				result = 0;
			}
			return (byte)result != 0;
		}
	}

	public string Id
	{
		get
		{
			CraftedResult? crafted = _crafted;
			if (crafted.HasValue)
			{
				return _crafted.Value.Id;
			}
			Messages.Crafting? crafting = _crafting;
			if (crafting.HasValue)
			{
				return _crafting.Value.Id;
			}
			return null;
		}
	}

	public bool Valid { get; set; }

	public event Action<bool> CancelModeChanged;

	public event Action EmptyClicked;

	public void SetCrafted(CraftedResult crafted)
	{
		_crafted = crafted;
		_crafting = null;
		Valid = true;
		Refresh();
	}

	public void SetCrafting(Messages.Crafting crafting)
	{
		_crafted = null;
		_crafting = crafting;
		Valid = true;
		Refresh();
	}

	public void SetEmpty()
	{
		_crafted = null;
		_crafting = null;
		Valid = true;
		Refresh();
	}

	public virtual void SetIndex(int index)
	{
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			_isActive = true;
			Refresh();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			_isActive = false;
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying)
		{
			Messages.Crafting? crafting = _crafting;
			if (crafting.HasValue)
			{
				Messages.Crafting value = _crafting.Value;
				_timerSprite.fillAmount = (float)(value.Since + (double)value.Duration - Connections.Frontend.GetPredictedServerTime()) / value.Duration;
			}
		}
	}

	private void Refresh()
	{
		if (!_isActive)
		{
			return;
		}
		Messages.Crafting? crafting = _crafting;
		bool hasValue = crafting.HasValue;
		CraftedResult? crafted = _crafted;
		bool hasValue2 = crafted.HasValue;
		bool isEmpty = IsEmpty;
		_emphasisBorder.gameObject.SetActive(hasValue2);
		_itemTexture.gameObject.SetActive(!isEmpty);
		_emptyIcon.gameObject.SetActive(isEmpty);
		_timerLabel.gameObject.SetActive(hasValue);
		_timerSprite.gameObject.SetActive(hasValue);
		_emphasisDots.gameObject.SetActive(hasValue2);
		_countLabel.gameObject.SetActive(hasValue2);
		_itemGradeViewer.gameObject.SetActive(hasValue2);
		_itemModifiedCountViewer.gameObject.SetActive(hasValue2);
		if (hasValue2)
		{
			CraftedResult value = _crafted.Value;
			int size = KUtility.GetSize(value.Items);
			ItemData itemData = null;
			if (size > 0)
			{
				itemData = new ItemData(value.Items[0]);
				_itemTexture.SetIcon(itemData);
			}
			else
			{
				_itemTexture.SetIcon(string.Empty);
			}
			_countLabel.text = ((size <= 1) ? string.Empty : $"x{size}");
			_itemGradeViewer.Set(itemData);
			_itemModifiedCountViewer.Set(itemData?.ModifiedCount ?? 0);
			_emphasisDots.Play();
		}
		else
		{
			if (!hasValue)
			{
				return;
			}
			Messages.Crafting value2 = _crafting.Value;
			Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(value2.RecipeId);
			_itemTexture.SetIcon((recipe != null) ? recipe.Icon : string.Empty);
			_timerSprite.fillAmount = (float)(value2.Since + (double)value2.Duration - Connections.Frontend.GetPredictedServerTime()) / value2.Duration;
			_timerLabel.SetText(new SyncString(delegate(out string text, out float period)
			{
				Messages.Crafting? crafting2 = _crafting;
				if (!crafting2.HasValue)
				{
					text = string.Empty;
					period = 0f;
				}
				else
				{
					Messages.Crafting value3 = _crafting.Value;
					double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
					double num = value3.Since + (double)value3.Duration - predictedServerTime;
					if (num > 0.0)
					{
						text = TimedeltaFormatter.Format(num);
						period = (float)(num % (double)TimedeltaFormatter.CurrentMinUnit());
					}
					else
					{
						text = string.Empty;
						period = 0f;
					}
				}
			}));
		}
	}

	public void SetCancelMode(bool on)
	{
		Messages.Crafting? crafting = _crafting;
		if (!crafting.HasValue)
		{
			on = false;
		}
		_isCancelMode = on;
		_cancelButton.gameObject.SetActive(on);
	}

	private void OnPress(bool press)
	{
		if (!press)
		{
			return;
		}
		CraftedResult? crafted = _crafted;
		if (crafted.HasValue)
		{
			int size = KUtility.GetSize(_crafted.Value.Items);
			if (size != 0)
			{
				Item item = _crafted.Value.Items[0];
				WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
				widgetTooltipControl.Set(null, (size <= 1) ? $"<em>{LocalizeUtil.FormatLevel(item.Level)}</em> {item.Name}" : $"<em>{LocalizeUtil.FormatLevel(item.Level)}</em> {item.Name} x{size}");
				widgetTooltipControl.AutoPosition = false;
				widgetTooltipControl.Show(5f);
				Vector3[] array = worldCorners;
				Vector3 position = Vector3.Lerp(array[1], array[2], 0.5f);
				position = widgetTooltipControl.transform.parent.InverseTransformPoint(position);
				widgetTooltipControl.Widget.SetPosition(position + Vector3.up * 20f, 0.5f, 0f);
				widgetTooltipControl.UpdateArrowPosition(position);
			}
		}
	}

	protected void OnClick()
	{
		CraftedResult? crafted = _crafted;
		if (crafted.HasValue)
		{
			TakeCrafted();
			return;
		}
		Messages.Crafting? crafting = _crafting;
		if (crafting.HasValue)
		{
			if (_isCancelMode)
			{
				CancelCrafting();
			}
			else
			{
				SkipCrafting();
			}
		}
		else
		{
			ClickEmptySlot();
		}
	}

	private void OnLongPress()
	{
		CraftedResult? crafted = _crafted;
		if (!crafted.HasValue)
		{
			Messages.Crafting? crafting = _crafting;
			if (crafting.HasValue && this.CancelModeChanged != null)
			{
				this.CancelModeChanged(!_isCancelMode);
			}
		}
	}

	private void TakeCrafted()
	{
		CraftedResult? crafted2 = _crafted;
		if (!crafted2.HasValue)
		{
			return;
		}
		InteractionObject lastInteractionTarget = GameSystem<InteractionSystem>.Instance().LastInteractionTarget;
		if (lastInteractionTarget == null)
		{
			return;
		}
		CraftedResult crafted = _crafted.Value;
		string[] items = crafted.Items.Select((Item item) => item.Id).ToArray();
		InventorySystem.TakeOutItems(lastInteractionTarget.EntityId, new Point2(lastInteractionTarget.Tile), items, delegate(bool success)
		{
			if (!success && IsEmpty)
			{
				SetCrafted(crafted);
			}
		});
		SetEmpty();
	}

	private void SkipCrafting()
	{
		Messages.Crafting? crafting2 = _crafting;
		if (!crafting2.HasValue || _crafting.Value.SkipCost == null)
		{
			return;
		}
		InteractionObject lastInteractionTarget = GameSystem<InteractionSystem>.Instance().LastInteractionTarget;
		if (lastInteractionTarget == null)
		{
			return;
		}
		Messages.Crafting crafting = _crafting.Value;
		MessageBox messageBox = UIManager.MessageBox;
		int num = Mathf.FloorToInt(crafting.SkipCost.Get());
		PropKey prop = new PropKey
		{
			EntityId = lastInteractionTarget.EntityId,
			Tile = new Point2(lastInteractionTarget.Tile)
		};
		messageBox.ShowPayConfirm(num, Currency.Gem, T._("즉시 완료하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				CraftSystem.SkipEntrustedCraft(prop, crafting.Id, delegate(bool success)
				{
					if (success)
					{
						GameSystem<InteractionSystem>.Instance().SendTouchMsg();
					}
				});
			}
		});
	}

	private void CancelCrafting()
	{
		Messages.Crafting? crafting2 = _crafting;
		if (!crafting2.HasValue)
		{
			return;
		}
		InteractionObject lastInteractionTarget = GameSystem<InteractionSystem>.Instance().LastInteractionTarget;
		if (lastInteractionTarget == null)
		{
			return;
		}
		Messages.Crafting crafting = _crafting.Value;
		PropKey prop = new PropKey
		{
			EntityId = lastInteractionTarget.EntityId,
			Tile = new Point2(lastInteractionTarget.Tile)
		};
		UIManager.MessageBox.Show(T._("진행 중인 제작을 취소하시겠습니까? 취소하면 제작 중인 물건은 사라집니다"), delegate(bool ok)
		{
			if (ok)
			{
				CraftSystem.CancelCrafting(prop, crafting.Id, delegate(bool success)
				{
					if (success)
					{
						GameSystem<InteractionSystem>.Instance().SendTouchMsg();
					}
				});
			}
		});
	}

	private void ClickEmptySlot()
	{
		if (this.EmptyClicked != null)
		{
			this.EmptyClicked();
		}
		InteractionObject lastInteractionTarget = GameSystem<InteractionSystem>.Instance().LastInteractionTarget;
		if (lastInteractionTarget == null)
		{
			return;
		}
		InteractionMenuList menuList = GameSystem<InteractionSystem>.Instance().MenuList;
		for (int i = 0; i < EmptyInteractionPriority.Length; i++)
		{
			int num = menuList.IndexOf(EmptyInteractionPriority[i]);
			if (num != -1)
			{
				GameSystem<InteractionSystem>.Instance().SelectTargetInteractionMenu(menuList[num]);
				break;
			}
		}
	}
}
