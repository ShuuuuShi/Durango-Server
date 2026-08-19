using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class SocialPlayerListWidget : MonoBehaviour
{
	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private ListObjectPool _cards;

	[SerializeField]
	private GameObject _noData;

	private int _cardCountPerLine;

	private float _margin;

	private bool _updateLayout;

	private UIWidget _invisibleBox;

	private bool _isInit;

	public event Action<int> CardCountUpdated;

	private void Start()
	{
		Init();
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UIWidget component = _cards.BaseObject.GetComponent<UIWidget>();
			UIWidget component2 = ((Component)this).GetComponent<UIWidget>();
			int num = component2.width / component.width;
			int num2 = component2.width % component.width;
			_margin = (float)num2 / ((float)num - 1f);
			_cardCountPerLine = num;
		}
	}

	private void OnEnable()
	{
		_invisibleBox = UIUtility.SetScrollViewInvisibleBox(_scrollView, _invisibleBox);
	}

	private void LateUpdate()
	{
		if (_updateLayout)
		{
			_updateLayout = false;
			LateUpdateLayout();
		}
	}

	public void Set(IList<ulong> playerList)
	{
		Init();
		_cards.Clear();
		int i = 0;
		for (int num = playerList?.Count ?? 0; i < num; i++)
		{
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(playerList[i], ResponsePlayerInfo);
		}
		UpdateLayout();
	}

	public void Set(IList<PlayerInfo> playerList)
	{
		Init();
		_cards.Clear();
		int i = 0;
		for (int num = playerList?.Count ?? 0; i < num; i++)
		{
			if (playerList[i] != null)
			{
				KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(playerList[i].EntityId, ResponsePlayerInfo);
			}
		}
		UpdateLayout();
	}

	private void ResponsePlayerInfo(PlayerInfo info)
	{
		if (info.Valid)
		{
			SocialPlayerCardWidget socialPlayerCardWidget = ((ListObjectPoolBase<GameObject>)_cards).Add<SocialPlayerCardWidget>();
			socialPlayerCardWidget.Set(info);
			socialPlayerCardWidget.IsInitPosition = false;
			socialPlayerCardWidget.Activate = true;
			UpdateLayout();
			if (this.CardCountUpdated != null)
			{
				this.CardCountUpdated(_cards.Count);
			}
		}
	}

	public void UpdateLayout()
	{
		_updateLayout = true;
	}

	private void LateUpdateLayout()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = _cards.BaseObject.transform.localPosition;
		UIWidget component = _cards.BaseObject.GetComponent<UIWidget>();
		float num = (float)component.width + _margin;
		float num2 = (float)component.height + _margin;
		int num3 = 0;
		int i = 0;
		for (int count = _cards.Count; i < count; i++)
		{
			SocialPlayerCardWidget component2 = _cards[i].GetComponent<SocialPlayerCardWidget>();
			if (component2.Activate)
			{
				int num4 = num3 % _cardCountPerLine;
				int num5 = num3 / _cardCountPerLine;
				Vector3 val = localPosition + Vector3.right * (float)num4 * num + Vector3.down * (float)num5 * num2;
				if (component2.IsInitPosition)
				{
					component2.AnimWidget.Position = val;
				}
				else
				{
					component2.AnimWidget.SetPosition(val, useTween: false);
					component2.IsInitPosition = true;
				}
				num3++;
			}
		}
		_noData.gameObject.SetActive(num3 == 0);
		_scrollView.RestrictWithinBounds(instant: false);
	}
}
