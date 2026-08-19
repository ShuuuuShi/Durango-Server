using System;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class InstrumentSelector : MonoBehaviour
{
	[SerializeField]
	private KScrollView _scrollView;

	private bool _isInit;

	public string Instrument { get; private set; }

	public event Action<string> InstrumentChanged;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			MusicManager.Instrument[] instruments = Singleton<MusicManager>.Instance().GetInstruments();
			ListObjectPool nodes = _scrollView.Nodes;
			nodes.Set(KUtility.GetSize(instruments));
			for (int i = 0; i < nodes.Count; i++)
			{
				Transform obj = nodes[i].transform;
				obj.Find("Icon").GetComponent<UISprite>().spriteName = instruments[i].Icon.sprite;
				SelectableWidget component = obj.GetComponent<SelectableWidget>();
				component.Selected = false;
				component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickNode));
			}
			_scrollView.ResetPosition();
		}
	}

	private void OnClickNode()
	{
		int num = _scrollView.Nodes.IndexOf(Selectable.Current.gameObject);
		if (num != -1)
		{
			string id = Singleton<MusicManager>.Instance().GetInstruments()[num].Id;
			if (!(Instrument == id) && this.InstrumentChanged != null)
			{
				this.InstrumentChanged(id);
			}
		}
	}

	public void Set(string instrument)
	{
		Init();
		SetInstrument(instrument);
	}

	private void SetInstrument(string instrument)
	{
		Instrument = instrument;
		MusicManager.Instrument[] instruments = Singleton<MusicManager>.Instance().GetInstruments();
		for (int i = 0; i < _scrollView.Nodes.Count; i++)
		{
			Selectable component = _scrollView.Nodes[i].GetComponent<Selectable>();
			component.Selected = instrument == instruments[i].Id;
			if (component.Selected)
			{
				_scrollView.MoveToVisibleArea(i, instant: false, 10f, 10f);
			}
		}
	}
}
