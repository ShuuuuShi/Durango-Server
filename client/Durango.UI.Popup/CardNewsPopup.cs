using Durango.UI.Control;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class CardNewsPopup : TooltipBase
{
	[SerializeField]
	private GameObject _close;

	[SerializeField]
	private GameObject _prev;

	[SerializeField]
	private GameObject _next;

	[SerializeField]
	private PageIndexSprite _pageIndex;

	[SerializeField]
	private KScrollView _scroll;

	private CardNewsAsset _source;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	protected override void OnAwake()
	{
		UIWidget rootAnchor = UIRootAnchor.GetRootAnchor(UIBase.AnchorType.Default);
		base.Widget.SetAnchor(rootAnchor.gameObject, 0, 0, 0, 0);
		UIEventListener.Get(_close).onClick = delegate
		{
			Hide();
		};
		UIEventListener.Get(_prev).onClick = delegate
		{
			GoPrevPage();
		};
		UIEventListener.Get(_next).onClick = delegate
		{
			GoNextPage();
		};
		GameSystem<InputSystem>.Instance().On(InputCommand.PrevOnModalPopup, delegate
		{
			GoPrevPage();
		});
		GameSystem<InputSystem>.Instance().On(InputCommand.NextOnModalPopup, delegate
		{
			GoNextPage();
		});
		_scroll.AttachPageIndexSprite(_pageIndex);
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		int num = Mathf.FloorToInt(_scroll.GoalOffset / _scroll.ViewLength);
		_prev.SetActive(num > 0);
		_next.SetActive(num < _scroll.GetNodeCount() - 1);
	}

	protected override void FillData()
	{
		_scroll.Nodes.BeginLoad();
		if (_source != null && _source.Cards != null)
		{
			foreach (CardNewsAsset.Card card in _source.Cards)
			{
				GameObject next = _scroll.Nodes.GetNext();
				UIWidget component = next.GetComponent<UIWidget>();
				component.width = (int)_scroll.GetComponent<UIPanel>().width;
				UITexture component2 = next.transform.Find("Texture").GetComponent<UITexture>();
				component2.mainTexture = card.Texture;
				component2.width = ((!UIManager.IsPortraitScreen) ? 1024 : component.width);
				float num = 1f - (float)component2.width / 1024f;
				component2.uvRect = new Rect(num / 2f, 0f, 1f - num, 1f);
				UILabel component3 = next.transform.Find("Subject").GetComponent<UILabel>();
				component3.text = T._(card.Subject);
				UILabel component4 = next.transform.Find("Explain").GetComponent<UILabel>();
				component4.text = T._(card.Explain);
			}
			_pageIndex.Make(_source.Cards.Count);
		}
		_scroll.Nodes.EndLoad();
	}

	protected override void UpdateLayout()
	{
		_scroll.ResetPosition();
		UIUtility.UpdateAnchors(base.transform);
	}

	public bool Load(string newsName)
	{
		string assetPath = $"UI/CardNews/{newsName}.asset";
		if (!Singleton<AssetBundleManager>.Instance().Contains(assetPath))
		{
			return false;
		}
		Singleton<AssetBundleManager>.Instance().RequestAsset(assetPath, typeof(CardNewsAsset), delegate(Object asset)
		{
			CardNewsAsset cardNewsAsset = asset as CardNewsAsset;
			if (!(cardNewsAsset == null))
			{
				_source = cardNewsAsset;
				MarkAsChanged();
			}
		});
		return true;
	}

	private void GoPrevPage()
	{
		if (_prev != null && _prev.activeInHierarchy)
		{
			_scroll.MoveToNode(_scroll.GetGoalNodeIndex() - 1, instant: false);
		}
	}

	private void GoNextPage()
	{
		if (_next != null && _next.activeInHierarchy)
		{
			_scroll.MoveToNode(_scroll.GetGoalNodeIndex() + 1, instant: false);
		}
	}

	protected override void OnTryConfirmOnModal()
	{
		GoNextPage();
	}

	protected override void OnTryCancelOnModal()
	{
		Hide();
	}
}
