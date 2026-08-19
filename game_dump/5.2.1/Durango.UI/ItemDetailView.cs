using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Building;
using Crafting;
using Durango.Logic.Item;
using Durango.UI.Control;
using JetBrains.Annotations;
using Messages;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

public class ItemDetailView : KWidgetScrollView
{
	[CompilerGenerated]
	private sealed class _003CEnumerateAdjustedItemTags_003Ed__22 : IEnumerable<TagData>, IEnumerable, IEnumerator<TagData>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private TagData _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private IEnumerable<ReformSlot> reformSlots;

		public IEnumerable<ReformSlot> _003C_003E3__reformSlots;

		private IEnumerable<TagData> itemTags;

		public IEnumerable<TagData> _003C_003E3__itemTags;

		private IEnumerator<TagData> _003C_003E7__wrap1;

		TagData IEnumerator<TagData>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CEnumerateAdjustedItemTags_003Ed__22(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || (uint)(num - 1) <= 1u)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				switch (_003C_003E1__state)
				{
				default:
					return false;
				case 0:
					_003C_003E1__state = -1;
					_reformSlotTagLevels.Clear();
					foreach (ReformSlot reformSlot in reformSlots)
					{
						Tag[] tags = reformSlot.Tags;
						for (int i = 0; i < tags.Length; i++)
						{
							Tag tag = tags[i];
							int num = _reformSlotTagLevels.Get(tag.Id, 0);
							_reformSlotTagLevels[tag.Id] = num + tag.Level;
						}
					}
					_003C_003E7__wrap1 = itemTags.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				case 2:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap1.MoveNext())
				{
					TagData current = _003C_003E7__wrap1.Current;
					if (_reformSlotTagLevels.TryGetValue(current.Id, out var value))
					{
						if (current.Level - value > 0)
						{
							_003C_003E2__current = TagData.Create(current.Id, current.Level - value);
							_003C_003E1__state = 1;
							return true;
						}
						continue;
					}
					_003C_003E2__current = current;
					_003C_003E1__state = 2;
					return true;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = null;
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			if (_003C_003E7__wrap1 != null)
			{
				_003C_003E7__wrap1.Dispose();
			}
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<TagData> IEnumerable<TagData>.GetEnumerator()
		{
			_003CEnumerateAdjustedItemTags_003Ed__22 _003CEnumerateAdjustedItemTags_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CEnumerateAdjustedItemTags_003Ed__ = this;
			}
			else
			{
				_003CEnumerateAdjustedItemTags_003Ed__ = new _003CEnumerateAdjustedItemTags_003Ed__22(0);
			}
			_003CEnumerateAdjustedItemTags_003Ed__.itemTags = _003C_003E3__itemTags;
			_003CEnumerateAdjustedItemTags_003Ed__.reformSlots = _003C_003E3__reformSlots;
			return _003CEnumerateAdjustedItemTags_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<TagData>)this).GetEnumerator();
		}
	}

	private static bool _reformInfoOpend = true;

	private static bool _performanceInfoOpend = true;

	private static bool _repairInfoOpend = true;

	private static bool _recipeInfoOpend = true;

	private static bool _blueprintInfoOpend = true;

	private static readonly Dictionary<string, int> _reformSlotTagLevels = new Dictionary<string, int>();

	[SerializeField]
	private UIWidget _itemHelpWidget;

	[SerializeField]
	private UILabel _itemHelpLabel;

	[SerializeField]
	private NestedPrefabLinker _tagsViewerLinker;

	[SerializeField]
	private ItemContextReform _reformInfo;

	[SerializeField]
	private ItemContextPerformance _performanceInfo;

	[SerializeField]
	private ItemContextRepair _repairInfo;

	[SerializeField]
	private ItemContextCraftInfo _recipeInfo;

	private ItemContextCraftInfo _blueprintInfo;

	private readonly HashSet<Recipe> _availableRecipes = new HashSet<Recipe>();

	private readonly HashSet<Blueprint> _availableBlueprints = new HashSet<Blueprint>();

	private TagsViewerWidget _tagsViewer;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_blueprintInfo = _recipeInfo.transform.parent.gameObject.AddChild(_recipeInfo.gameObject).GetComponent<ItemContextCraftInfo>();
			ItemContextControlInitializer(_reformInfo);
			ItemContextControlInitializer(_performanceInfo);
			ItemContextControlInitializer(_repairInfo);
			ItemContextControlInitializer(_recipeInfo);
			ItemContextControlInitializer(_blueprintInfo);
			_tagsViewer = _tagsViewerLinker.Object.GetComponent<TagsViewerWidget>();
			List<UIWidget> widgets = base.Widgets;
			widgets.Add(_itemHelpWidget);
			widgets.Add(_tagsViewerLinker.GetComponent<UIWidget>());
			widgets.Add(_reformInfo);
			widgets.Add(_performanceInfo);
			widgets.Add(_repairInfo);
			widgets.Add(_recipeInfo);
			widgets.Add(_blueprintInfo);
		}
	}

	private void ItemContextControlInitializer(ItemContextBase control)
	{
		control.Init();
		control.OnExpandChanged += OnControlExpandChanged;
	}

	public void Set([NotNull] ItemData itemData, bool enableRecipeLink)
	{
		Init();
		TagsViewerWidget tagsViewer = _tagsViewer;
		IEnumerable<TagData> tags;
		if (itemData.ReformSlots.Count > 0)
		{
			tags = EnumerateAdjustedItemTags(itemData.Tags, itemData.ReformSlots);
		}
		else
		{
			IEnumerable<TagData> tags2 = itemData.Tags;
			tags = tags2;
		}
		bool active = tagsViewer.Set(tags);
		_tagsViewerLinker.gameObject.SetActive(active);
		if (itemData.IsEquipments)
		{
			_availableRecipes.Clear();
			_availableBlueprints.Clear();
		}
		else
		{
			GameSystem<RecipeSystem>.Instance().FillAvailableRecipesByItemData(_availableRecipes, itemData);
			GameSystem<RecipeSystem>.Instance().FillAvailableBlueprintsByItemData(_availableBlueprints, itemData);
		}
		if (itemData.Prototype == null || string.IsNullOrEmpty(itemData.Prototype.Help))
		{
			_itemHelpWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_itemHelpWidget.gameObject.SetActive(value: true);
			_itemHelpLabel.text = itemData.Prototype.Help;
			_itemHelpWidget.height = _itemHelpLabel.height + 22;
		}
		_reformInfo.Set(itemData);
		_performanceInfo.Set(itemData);
		_repairInfo.Set(itemData);
		_recipeInfo.Set(_availableRecipes, enableRecipeLink);
		_blueprintInfo.Set(_availableBlueprints, enableRecipeLink);
		OnDataFillFinished();
	}

	public void Set(Pet pet)
	{
		Init();
		PetStats stat = pet.Stat;
		_tagsViewer.SettingBegin();
		if (stat.Tags != null)
		{
			foreach (KeyValuePair<string, int> tag in stat.Tags)
			{
				_tagsViewer.AddTagData(tag.Key, tag.Value);
			}
		}
		_tagsViewerLinker.gameObject.SetActive(_tagsViewer.SettingEnd());
		_itemHelpWidget.gameObject.SetActive(value: false);
		_reformInfo.Clear();
		_performanceInfo.Set(pet);
		_repairInfo.Set(null);
		_recipeInfo.Clear();
		_blueprintInfo.Clear();
		OnDataFillFinished();
	}

	private static IEnumerable<TagData> EnumerateAdjustedItemTags(IEnumerable<TagData> itemTags, IEnumerable<ReformSlot> reformSlots)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateAdjustedItemTags_003Ed__22(-2)
		{
			_003C_003E3__itemTags = itemTags,
			_003C_003E3__reformSlots = reformSlots
		};
	}

	private void OnDataFillFinished()
	{
		if (_reformInfo.gameObject.activeSelf)
		{
			_reformInfo.SetExpand(_reformInfoOpend, instant: true);
		}
		if (_performanceInfo.gameObject.activeSelf)
		{
			_performanceInfo.SetExpand(_performanceInfoOpend, instant: true);
		}
		if (_repairInfo.gameObject.activeSelf)
		{
			_repairInfo.SetExpand(_repairInfoOpend, instant: true);
		}
		if (_recipeInfo.gameObject.activeSelf)
		{
			_recipeInfo.SetExpand(_recipeInfoOpend, instant: true);
		}
		if (_blueprintInfo.gameObject.activeSelf)
		{
			_blueprintInfo.SetExpand(_blueprintInfoOpend, instant: true);
		}
		ResetPosition();
	}

	private void OnControlExpandChanged(ItemContextBase comp)
	{
		bool flag = !comp.IsExpanded;
		comp.SetExpand(flag, instant: false);
		if (comp == _reformInfo)
		{
			_reformInfoOpend = flag;
		}
		else if (comp == _performanceInfo)
		{
			_performanceInfoOpend = flag;
		}
		else if (comp == _repairInfo)
		{
			_repairInfoOpend = flag;
		}
		else if (comp == _recipeInfo)
		{
			_recipeInfoOpend = flag;
		}
		else if (comp == _blueprintInfo)
		{
			_blueprintInfoOpend = flag;
		}
		UpdateLayout(instant: false);
	}
}
