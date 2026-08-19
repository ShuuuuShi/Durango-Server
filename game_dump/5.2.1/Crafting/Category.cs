using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Building;
using Durango.Logic.Notification;

namespace Crafting;

public class Category : INotificationable
{
	[CompilerGenerated]
	private sealed class _003CGetItems_003Ed__12 : IEnumerable<CategoryItem>, IEnumerable, IEnumerator<CategoryItem>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private CategoryItem _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private RecipeSystem.RecipeType type;

		public RecipeSystem.RecipeType _003C_003E3__type;

		public Category _003C_003E4__this;

		private int _003Cj_003E5__2;

		CategoryItem IEnumerator<CategoryItem>.Current
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
		public _003CGetItems_003Ed__12(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			Category category = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				if (type == RecipeSystem.RecipeType.Crafting)
				{
					_003Cj_003E5__2 = 0;
					goto IL_0072;
				}
				goto IL_0085;
			case 1:
				_003C_003E1__state = -1;
				_003Cj_003E5__2++;
				goto IL_0072;
			case 2:
				{
					_003C_003E1__state = -1;
					_003Cj_003E5__2++;
					goto IL_00ce;
				}
				IL_0072:
				if (_003Cj_003E5__2 < category.Recipes.Count)
				{
					_003C_003E2__current = category.Recipes[_003Cj_003E5__2];
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_0085;
				IL_00ce:
				if (_003Cj_003E5__2 < category.Blueprints.Count)
				{
					_003C_003E2__current = category.Blueprints[_003Cj_003E5__2];
					_003C_003E1__state = 2;
					return true;
				}
				break;
				IL_0085:
				if (type != RecipeSystem.RecipeType.Building)
				{
					break;
				}
				_003Cj_003E5__2 = 0;
				goto IL_00ce;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<CategoryItem> IEnumerable<CategoryItem>.GetEnumerator()
		{
			_003CGetItems_003Ed__12 _003CGetItems_003Ed__;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				_003CGetItems_003Ed__ = this;
			}
			else
			{
				_003CGetItems_003Ed__ = new _003CGetItems_003Ed__12(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			_003CGetItems_003Ed__.type = _003C_003E3__type;
			return _003CGetItems_003Ed__;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<CategoryItem>)this).GetEnumerator();
		}
	}

	public string Id;

	public string Name;

	public readonly List<Recipe> Recipes = new List<Recipe>();

	public readonly List<Blueprint> Blueprints = new List<Blueprint>();

	private readonly Container _notification = new Container();

	public int ItemCount => Recipes.Count + Blueprints.Count;

	public Notification Notification => _notification;

	public void UpdateNotification()
	{
		_notification.ClearChild();
		for (int i = 0; i < ItemCount; i++)
		{
			CategoryItem item = GetItem(i);
			if (item.Available)
			{
				_notification.AddChild(item);
			}
		}
		_notification.Refresh();
	}

	public void ClearNotification()
	{
		Notification notification = Notification;
		notification.BeginSetting();
		for (int i = 0; i < ItemCount; i++)
		{
			GetItem(i).Notification.On = false;
		}
		notification.EndSetting();
	}

	public CategoryItem GetItem(int index)
	{
		if (index < Recipes.Count)
		{
			return Recipes[index];
		}
		return Blueprints[index - Recipes.Count];
	}

	private IEnumerable<CategoryItem> GetItems(RecipeSystem.RecipeType type)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetItems_003Ed__12(-2)
		{
			_003C_003E4__this = this,
			_003C_003E3__type = type
		};
	}

	public void SetAvailableList(string[] updatedRecipes, RecipeSystem.RecipeType type)
	{
		bool flag = false;
		foreach (CategoryItem item in GetItems(type))
		{
			bool available = item.Available;
			item.Available = updatedRecipes != null && Array.IndexOf(updatedRecipes, item.Id) != -1;
			flag = item.Available != available;
		}
		if (flag)
		{
			UpdateNotification();
		}
	}

	public void SetNewList(string[] newList, RecipeSystem.RecipeType type)
	{
		if (newList == null)
		{
			return;
		}
		foreach (CategoryItem item in GetItems(type))
		{
			if (item != null && item.Available && Array.IndexOf(newList, item.Id) != -1)
			{
				item.Notification.On = true;
			}
		}
	}

	public void SetLikeList(string[] likeList, RecipeSystem.RecipeType type)
	{
		foreach (CategoryItem item in GetItems(type))
		{
			item.Like = likeList != null && Array.IndexOf(likeList, item.Id) != -1;
		}
	}
}
