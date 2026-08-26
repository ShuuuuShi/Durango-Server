using System;
using System.Collections.Generic;
using System.Linq;
using Building;
using Crafting;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class RecipeListWidget : MonoBehaviour, IUIInitializable
{
	public class SubList
	{
		private readonly List<RecipeSubListWidget.Data> _list = new List<RecipeSubListWidget.Data>();

		private readonly bool _inEntireCategory;

		public int Count => _list.Count;

		public bool IsFavorites { get; private set; }

		public string Text { get; set; }

		public SubList(bool inEntireCategory, bool isFavorites = false)
		{
			_inEntireCategory = inEntireCategory;
			IsFavorites = isFavorites;
		}

		public void AddItem(CategoryItem item)
		{
			RecipeSubListWidget.Data item2 = default(RecipeSubListWidget.Data);
			item2.Item = item;
			_list.Add(item2);
		}

		public CategoryItem GetItem(string id, string searchText)
		{
			foreach (RecipeSubListWidget.Data item in EnumerateItems(searchText))
			{
				if (item.Item.Id == id)
				{
					return item.Item;
				}
			}
			return null;
		}

		public int IndexOf(string id, string searchText)
		{
			int num = 0;
			foreach (RecipeSubListWidget.Data item in EnumerateItems(searchText))
			{
				if (item.Item.Id == id)
				{
					return num;
				}
				num++;
			}
			return -1;
		}

		public bool ContainsFilteredItem(string searchText)
		{
			return string.IsNullOrEmpty(searchText) || _list.Any((RecipeSubListWidget.Data data) => PredicateSearchFilter(data, searchText));
		}

		/// <summary>
		/// [แก้เอง] **แสดงสูตรที่วัตถุดิบครบแล้ว — ทุกแท็บ**
		///
		/// เดิมเทสูตรทุกอันมากอง ต้องไล่กดทีละอันถึงจะรู้ว่าอันไหนทำได้
		/// ⇒ รายการคราฟตอบคำถามเดียวคือ "ตอนนี้ทำอะไรได้บ้าง"
		/// (รายการโปรดยังไม่กรอง เพราะผู้เล่นตั้งใจปักไว้เอง)
		///
		/// 🐛 **รอบทำอาหาร: กติกาเดิมทำให้ของหายไปทั้งกระดาน**
		/// พอ server บังคับ "ต้องยืนที่โต๊ะ/เตา + ต้องถือเครื่องมือ" จริง สูตรที่ต้องใช้โต๊ะ
		/// (587 จาก 720 · รวมสูตรทำอาหารทุกอัน) กลายเป็น "ทำไม่ได้ตอนนี้" เวลาไม่ได้ยืนที่โต๊ะ
		/// ⇒ ถูกซ่อนหมด ⇒ **ผู้เล่นไม่มีทางรู้เลยว่าต้องไปสร้างกองไฟก่อน** (เจอตอนเล่นจริง)
		///
		/// ตอนนี้แยกเป็น 3 ชั้น:
		///   · ทำได้เลย            → ขึ้นบนสุด
		///   · ของครบ แต่ขาดเครื่องมือ/โต๊ะ → ขึ้นถัดมา **แบบสีจาง + บอกว่าขาดอะไร**
		///   · วัตถุดิบไม่พอ        → ซ่อน (เจตนาเดิม — ดูจากช่องวัตถุดิบเอาได้)
		/// </summary>
		public IEnumerable<RecipeSubListWidget.Data> EnumerateItems(string searchText)
		{
			IEnumerable<RecipeSubListWidget.Data> items =
				(!string.IsNullOrEmpty(searchText))
					? _list.Where((RecipeSubListWidget.Data data) => PredicateSearchFilter(data, searchText))
					: _list;

			// เรียง "ของที่ทำได้ตอนนี้" ขึ้นบนสุดเสมอ แล้วซ่อนของที่ทำไม่ได้
			//
			// คิด CanCraftNow ทีเดียวต่อชิ้นแล้วเก็บใส่ Data.CanCraft ไปเลย
			// (RecipeItemWidget อ่านค่านี้ต่อ จะได้ไม่ต้องคำนวณซ้ำตอนวาด — HasMaterials
			//  ทำ bipartite matching ต่อสูตร ถ้าเรียกซ้ำทุกเฟรมจะหนักโดยไม่จำเป็น)
			var ordered = new List<RecipeSubListWidget.Data>();
			var blockers = new List<CraftBlocker>();
			foreach (RecipeSubListWidget.Data data in items)
			{
				RecipeSubListWidget.Data copy = data;
				CraftBlocker blocker = GameSystem<RecipeSystem>.Instance().WhyCannotCraft(copy.Item);
				copy.CanCraft = blocker == CraftBlocker.None;
				// [แก้เอง] 24 ส.ค. 2026 — เจ้าของสั่งเอา "ซ่อนของที่วัตถุดิบไม่พอ" ออก อยากให้เห็นสูตร
				// ครบทุกอันเหมือนโหมดออฟไลน์ (ผู้เล่นจะได้รู้ว่ามีสูตรอะไรอยู่บ้าง แม้ยังไม่มีของ) — เดิมมี
				// `if (!IsFavorites && blocker == CraftBlocker.Materials) continue;` ซ่อนไปเงียบ ๆ ตรงนี้
				// (นี่คือสาเหตุจริงที่ "ขวาน" ไม่เคยโผล่ในเมนู — ไม่ใช่บั๊ก แค่ไม่มีวัตถุดิบ 3 อย่างที่ต้องใช้
				// จริง ๆ (blade+เชือก+ด้าม) ในกระเป๋าเลย ดู HANDOFF.md)
				//
				// [แก้เอง] 24 ส.ค. 2026 (รอบ 2) — เจ้าของสั่งกลับด้าน "โต๊ะคราฟ": ของที่ต้องใช้โต๊ะ/เตาแต่
				// ไม่ได้ยืนอยู่ตรงนั้น ให้ **ซ่อน** ไปเลย (ต่างจาก Materials ที่ยังโชว์แบบจาง ๆ) — จะได้ไม่เห็น
				// สูตรระดับสูงที่ต้องมีโต๊ะเทียร์สูงเกลื่อนเมนูตอนยังไม่มีโต๊ะนั้น พอเดินไปยืนที่โต๊ะที่ใช่ค่อยโผล่
				if (!IsFavorites && blocker == CraftBlocker.Workbench)
				{
					continue;
				}
				ordered.Add(copy);
				blockers.Add(blocker);
			}
			// ⚠️ List.Sort ของ .NET **ไม่ stable** — คืน 0 ตอนเท่ากันแล้วลำดับเดิมในกลุ่มจะสลับมั่ว
			// จึงพ่วง index เดิมไปเป็นตัวตัดสินท้ายสุด เพื่อให้ของที่สถานะเท่ากันคงลำดับเดิมเป๊ะ
			var keys = new List<int>(ordered.Count);
			for (int i = 0; i < ordered.Count; i++)
			{
				keys.Add(i);
			}
			keys.Sort(delegate(int a2, int b2)
			{
				// เรียงตามความ "ใกล้ทำได้": ทำได้เลย → ขาดเครื่องมือ → ขาดโต๊ะ → วัตถุดิบไม่พอ
				int ba = (int)blockers[a2];
				int bb = (int)blockers[b2];
				if (ba != bb)
				{
					return ba.CompareTo(bb);
				}
				return a2.CompareTo(b2);          // เท่ากัน = ลำดับเดิม
			});
			var result = new List<RecipeSubListWidget.Data>(ordered.Count);
			for (int i = 0; i < keys.Count; i++)
			{
				result.Add(ordered[keys[i]]);
			}
			return result;
		}

		/// <summary>[แก้เอง] 23 ส.ค. 2026 — มีไอเทมที่ทำได้ตอนนี้เลยอยู่ในกลุ่มย่อยนี้บ้างไหม
		/// ใช้จัดลำดับ "กลุ่ม" (คนละเรื่องกับการจัดลำดับ "ไอเทมในกลุ่ม" ที่ EnumerateItems ทำอยู่แล้ว —
		/// อันนั้นจัดของทำได้ขึ้นก่อนแค่ภายในกลุ่มเดียวกัน แต่ลำดับกลุ่มเองยังเรียงตามตัวอักษรเฉย ๆ
		/// ⇒ เจ้าของสั่งให้ "ของที่คราฟได้อยู่บนสุด" ของทั้งช่องกลาง ไม่ใช่แค่ในกลุ่มตัวเอง)</summary>
		public bool HasCraftableNow()
		{
			foreach (RecipeSubListWidget.Data data in _list)
			{
				if (GameSystem<RecipeSystem>.Instance().WhyCannotCraft(data.Item) == CraftBlocker.None)
				{
					return true;
				}
			}
			return false;
		}

		public static int SortComparison(KeyValuePair<string, SubList> x, KeyValuePair<string, SubList> y)
		{
			if (y.Value.IsFavorites != x.Value.IsFavorites)
			{
				return (y.Value.IsFavorites ? 1 : 0) - (x.Value.IsFavorites ? 1 : 0);
			}
			// [แก้เอง] เจ้าของสั่ง: กลุ่มที่มีของคราฟต์ได้ตอนนี้อย่างน้อย 1 ชิ้น ขึ้นก่อนกลุ่มที่ทำไม่ได้เลย
			bool xHas = x.Value.HasCraftableNow();
			bool yHas = y.Value.HasCraftableNow();
			if (xHas != yHas)
			{
				return xHas ? -1 : 1;
			}
			return string.Compare(x.Key, y.Key, StringComparison.OrdinalIgnoreCase);
		}

		private bool PredicateSearchFilter(RecipeSubListWidget.Data data, string searchText)
		{
			if (_inEntireCategory && !IsFavorites)
			{
				return !data.Item.Like && SearchFilter(data.Item.Name, searchText);
			}
			return SearchFilter(data.Item.Name, searchText);
		}

		private static bool SearchFilter(string name, string searchText)
		{
			return name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) != -1;
		}
	}

	public struct SelectInfo
	{
		public RecipeSystem.RecipeType RecipeType;

		public string Id;

		public bool InFavorites;

		public SelectInfo([NotNull] CategoryItem item, bool inFavorites = false)
		{
			if (item is Recipe)
			{
				RecipeType = RecipeSystem.RecipeType.Crafting;
			}
			else if (item is Blueprint)
			{
				RecipeType = RecipeSystem.RecipeType.Building;
			}
			else
			{
				RecipeType = RecipeSystem.RecipeType.None;
			}
			Id = item.Id;
			InFavorites = inFavorites;
		}
	}

	[SerializeField]
	private RecipeFilterWidget _recipeFilterWidget;

	[SerializeField]
	private KInfiniteScrollView _recipeScrollView;

	[SerializeField]
	private RecipeSubListWidget _recipeSubListWidgetBase;

	[SerializeField]
	private UILabel _recipeNoData;

	[SerializeField]
	private bool _toggleRecipeSelection;

	private int _widthRecipeScrollView;

	private readonly List<KeyValuePair<string, SubList>> _listForRecipeView = new List<KeyValuePair<string, SubList>>();

	private KInfiniteScrollView.View<KeyValuePair<string, SubList>, RecipeSubListWidget> _recipeView;

	public SelectInfo? SelectedRecipe { get; private set; }

	public event Action RecipeSelected;

	public void ClearSearchText()
	{
		_recipeFilterWidget.SearchText = string.Empty;
	}

	public void SetRecipes([NotNull] IEnumerable<KeyValuePair<string, SubList>> subLists, bool resetPosition)
	{
		_widthRecipeScrollView = (int)_recipeScrollView.ViewSize.x;
		_recipeFilterWidget.SetRecipeItems(subLists);
		SelectedRecipe = null;
		RefreshRecipeItems(resetPosition);
	}

	public RecipeItemWidget FindRecipe(string id)
	{
		foreach (RecipeSubListWidget item in _recipeView.List)
		{
			RecipeItemWidget recipeItemWidget = item.FindRecipeComponent(id);
			if (recipeItemWidget != null)
			{
				return recipeItemWidget;
			}
		}
		return null;
	}

	public bool SelectRecipe(SelectInfo? info)
	{
		CategoryItem categoryItem = GetCategoryItem(info);
		if (categoryItem != null)
		{
			RefreshSelection(info);
			return true;
		}
		RefreshSelection();
		return false;
	}

	public void ScrollToRecipe(SelectInfo info, bool instant)
	{
		int num = 0;
		foreach (KeyValuePair<string, SubList> item in _listForRecipeView)
		{
			if (item.Value.IsFavorites == info.InFavorites)
			{
				int num2 = item.Value.IndexOf(info.Id, _recipeFilterWidget.SearchText);
				if (num2 != -1)
				{
					float recipeItemOffset = GetRecipeItemOffset(num, num2);
					recipeItemOffset -= (float)_recipeNoData.height / 3f;
					_recipeScrollView.MoveTo(recipeItemOffset, instant);
					break;
				}
			}
			num++;
		}
	}

	void IUIInitializable.Init()
	{
		_recipeView = _recipeScrollView.Initialize(delegate(RecipeSubListWidget widget, KeyValuePair<string, SubList> subList)
		{
			widget.SetRecipes(_widthRecipeScrollView, subList.Value, _recipeFilterWidget.SearchText, SelectedRecipe);
		}, delegate(RecipeSubListWidget widget)
		{
			widget.Init();
			widget.RecipeClicked += RecipeSubListWidget_RecipeClicked;
		});
		_recipeNoData.gameObject.SetActive(value: false);
		_recipeFilterWidget.Init();
		_recipeFilterWidget.SearchTextSubmitted += RecipeFilterWidget_SearchTextSubmitted;
	}

	private void RefreshRecipeItems(bool resetPosition)
	{
		_listForRecipeView.Clear();
		_listForRecipeView.AddRange(_recipeFilterWidget.EnumerateFilteredLists());
		_recipeView.SetList(_listForRecipeView);
		_recipeScrollView.UpdateLayout();
		if (resetPosition)
		{
			_recipeScrollView.MoveTo(0f, instant: true);
		}
		RefreshRecipeNoData();
	}

	private void RefreshRecipeNoData()
	{
		if (_listForRecipeView.Count == 0)
		{
			_recipeNoData.text = ((!string.IsNullOrEmpty(_recipeFilterWidget.SearchText)) ? T._("검색 결과가 없습니다") : T._("도안이 없습니다"));
			_recipeNoData.gameObject.SetActive(value: true);
		}
		else
		{
			_recipeNoData.gameObject.SetActive(value: false);
		}
	}

	private void RefreshSelection(SelectInfo? info = null)
	{
		SelectedRecipe = info;
		foreach (RecipeSubListWidget item in _recipeView.List)
		{
			item.RefreshSelectState(SelectedRecipe);
		}
		if (this.RecipeSelected != null)
		{
			this.RecipeSelected();
		}
	}

	private CategoryItem GetCategoryItem(SelectInfo? info)
	{
		if (info.HasValue)
		{
			foreach (KeyValuePair<string, SubList> item2 in _listForRecipeView)
			{
				if (item2.Value.IsFavorites == info.Value.InFavorites)
				{
					CategoryItem item = item2.Value.GetItem(info.Value.Id, _recipeFilterWidget.SearchText);
					if (item != null)
					{
						return item;
					}
				}
			}
		}
		return null;
	}

	private float GetRecipeItemOffset(int indexNode, int indexRecipe)
	{
		float nodeOffset = _recipeScrollView.GetNodeOffset(indexNode);
		nodeOffset += (float)_recipeSubListWidgetBase.TitleHeight;
		return nodeOffset + (float)(_recipeSubListWidgetBase.RecipeHeight * (indexRecipe / _recipeSubListWidgetBase.WidgetCountPerLine));
	}

	private void RecipeFilterWidget_SearchTextSubmitted()
	{
		RefreshRecipeItems(resetPosition: true);
		CategoryItem categoryItem = GetCategoryItem(SelectedRecipe);
		if (categoryItem == null)
		{
			RefreshSelection();
		}
	}

	private void RecipeSubListWidget_RecipeClicked(RecipeItemWidget widget, bool inFavorites)
	{
		if (widget != null)
		{
			RefreshSelection((!_toggleRecipeSelection || !SelectedRecipe.HasValue || !(SelectedRecipe.Value.Id == widget.Item.Id)) ? new SelectInfo?(new SelectInfo(widget.Item, inFavorites)) : null);
		}
	}
}
