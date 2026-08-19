using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using Durango.System.Config;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ConfigTabWidget : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CEnumerateSettings_003Ed__14 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		private Dictionary<string, List<Setting>>.Enumerator _003C_003E7__wrap1;

		string IEnumerator<string>.Current
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
		public _003CEnumerateSettings_003Ed__14(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E7__wrap1 = default(Dictionary<string, List<Setting>>.Enumerator);
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
					_003C_003E7__wrap1 = ConfigInstance.Settings.GetEnumerator();
					_003C_003E1__state = -3;
					break;
				case 1:
					_003C_003E1__state = -3;
					break;
				}
				while (_003C_003E7__wrap1.MoveNext())
				{
					KeyValuePair<string, List<Setting>> current = _003C_003E7__wrap1.Current;
					List<Setting> value = current.Value;
					if (current.Key != "default" && current.Key != "screen")
					{
						continue;
					}
					bool flag = true;
					for (int i = 0; i < value.Count; i++)
					{
						if (!Setting.IsHidden(value[i]))
						{
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						_003C_003E2__current = current.Key;
						_003C_003E1__state = 1;
						return true;
					}
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap1 = default(Dictionary<string, List<Setting>>.Enumerator);
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
			((IDisposable)_003C_003E7__wrap1).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				return this;
			}
			return new _003CEnumerateSettings_003Ed__14(0);
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
	}

	public Action<string> TabClicked;

	[SerializeField]
	private KScrollView _scrollView;

	private int _currentIndex = -1;

	public bool IsInit { get; private set; }

	public string CurrentCategory { get; private set; }

	public void Init()
	{
		if (!IsInit)
		{
			IsInit = true;
			CreateTabs();
			SelectTab(0);
		}
	}

	public void Reposition()
	{
		_scrollView.ScrollView.movement = ((!UIManager.IsPortraitWidget(base.gameObject)) ? UIScrollView.Movement.Vertical : UIScrollView.Movement.Horizontal);
		_scrollView.Reposition();
	}

	private void CreateTabs()
	{
		_scrollView.Nodes.Clear();
		foreach (string item in EnumerateSettings())
		{
			ConfigTabItem configTabItem = _scrollView.Nodes.Add<ConfigTabItem>();
			configTabItem.Set(item);
			configTabItem.Clicked = (Action)Delegate.Combine(configTabItem.Clicked, new Action(OnTabClick));
		}
	}

	private static IEnumerable<string> EnumerateSettings()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CEnumerateSettings_003Ed__14(-2);
	}

	private void OnTabClick()
	{
		int num = _scrollView.Nodes.IndexOf(Selectable.Current.gameObject);
		if (num != -1)
		{
			SelectTab(num);
		}
	}

	public void SelectTab(string category)
	{
		for (int i = 0; i < _scrollView.Nodes.Count; i++)
		{
			ConfigTabItem component = _scrollView.Nodes[i].GetComponent<ConfigTabItem>();
			if (component != null && component.Category == category)
			{
				SelectTab(i);
				break;
			}
		}
	}

	private void SelectTab(int index)
	{
		if (_currentIndex == index)
		{
			return;
		}
		for (int i = 0; i < _scrollView.Nodes.Count; i++)
		{
			ConfigTabItem component = _scrollView.Nodes[i].GetComponent<ConfigTabItem>();
			if (!(component == null))
			{
				component.Selected = i == index;
				if (i == index)
				{
					_currentIndex = index;
					CurrentCategory = component.Category;
				}
			}
		}
		if (TabClicked != null)
		{
			TabClicked(CurrentCategory);
		}
	}
}
