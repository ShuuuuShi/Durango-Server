using System;
using L10N;
using Player;
using UnityEngine;

public class BlockUserGroup : UIBase
{
	[SerializeField]
	private UIWidget _container;

	[SerializeField]
	private UILabel _labelTitle;

	[SerializeField]
	private UITexture _texture;

	[SerializeField]
	private Texture _textureMask;

	[SerializeField]
	private UILabel _textUserName;

	[SerializeField]
	private UILabel _labelInfo;

	[SerializeField]
	private UIWidget _warningWidget;

	[SerializeField]
	private DefaultSelectableButton _buttonYes;

	[SerializeField]
	private DefaultSelectableButton _buttonNo;

	private PlayerInfo _playerInfo;

	private Vector3 _posForBlock;

	private Vector3 _posForUnblock;

	private int _heightForBlock;

	private int _heightForUnblock;

	private void Awake()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		_posForBlock = ((Component)_container).transform.localPosition;
		_posForUnblock = ((Component)_container).transform.localPosition;
		ref Vector3 posForUnblock = ref _posForUnblock;
		posForUnblock.y -= (float)(_warningWidget.height / 2);
		_heightForBlock = _container.height;
		_heightForUnblock = _container.height - _warningWidget.height;
		DefaultSelectableButton buttonYes = _buttonYes;
		buttonYes.Clicked = (Action)Delegate.Combine(buttonYes.Clicked, (Action)delegate
		{
			if (GetFollowingState(_playerInfo))
			{
				GameSystem<SocialSystem>.Instance().Follow(_playerInfo.EntityId);
			}
			GameSystem<SocialSystem>.Instance().Block(_playerInfo.EntityId, OnSuccessBlock);
			Close();
		});
		DefaultSelectableButton buttonNo = _buttonNo;
		buttonNo.Clicked = (Action)Delegate.Combine(buttonNo.Clicked, (Action)delegate
		{
			Close();
		});
		((Component)_container).gameObject.SetActive(false);
	}

	public void Open(PlayerInfo playerInfo)
	{
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		_playerInfo = playerInfo;
		bool blockState = GetBlockState(_playerInfo);
		PortraitBuilder.Argument portraitArgument = _playerInfo.GetPortraitArgument();
		portraitArgument.Mask = _textureMask;
		PortraitBuilder.Set(portraitArgument, _texture);
		_textUserName.text = _playerInfo.Name;
		if (blockState)
		{
			_labelTitle.text = T._("플레이어 차단 해제");
			_labelInfo.text = T._("님을 차단 해제 하시겠습니까?");
			((Component)_container).transform.localPosition = _posForUnblock;
			_container.height = _heightForUnblock;
			((Component)_warningWidget).gameObject.SetActive(false);
		}
		else
		{
			_labelTitle.text = T._("플레이어 차단");
			_labelInfo.text = T._("님을 차단하시겠습니까?");
			((Component)_container).transform.localPosition = _posForBlock;
			_container.height = _heightForBlock;
			((Component)_warningWidget).gameObject.SetActive(true);
		}
		Open();
	}

	protected override bool OnOpen()
	{
		((Component)_container).gameObject.SetActive(true);
		return true;
	}

	protected override bool OnClose()
	{
		((Component)_container).gameObject.SetActive(false);
		return true;
	}

	private void OnSuccessBlock()
	{
		string format = ((!GetBlockState(_playerInfo)) ? "#block_success_unblock_user" : "#block_success_block_user");
		UIManager.SystemMsg(LocalizeSystem.Format(format, _playerInfo.Name));
	}

	private static bool GetFollowingState(PlayerInfo playerInfo)
	{
		return GameSystem<SocialSystem>.Instance().FollowingList.Contains(playerInfo.EntityId);
	}

	private static bool GetBlockState(PlayerInfo playerInfo)
	{
		return GameSystem<SocialSystem>.Instance().BlockList.Contains(playerInfo.EntityId);
	}
}
