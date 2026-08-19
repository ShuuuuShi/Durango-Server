using System;
using System.Collections.Generic;
using L10N;
using Messages;
using Player;
using UnityEngine;

public class ProfileTooltip : TooltipBase
{
	[SerializeField]
	private UIWidget _contentsWidget;

	[SerializeField]
	private UIWidget _portraitWidget;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISpriteLabel _radioLabel;

	[SerializeField]
	private UIWidget _infoWidget;

	[SerializeField]
	private ListObjectPool _infoItems;

	[SerializeField]
	private UIWidget _previewWidget;

	[SerializeField]
	private Transform _previewParent;

	[SerializeField]
	private UIWidget _buttonsWidget;

	[SerializeField]
	private DefaultSelectableButton _chatBtn;

	[SerializeField]
	private DefaultSelectableButton _followBtn;

	[SerializeField]
	private DefaultSelectableButton _reportBtn;

	[SerializeField]
	private DefaultSelectableButton _blockBtn;

	private PlayerBehavior _previewModel;

	private Player.PlayerInfo _playerInfo;

	private PortraitBuilder.Argument _portraitArgument;

	private string _userName;

	private int _userFrequency;

	private IList<KeyValuePair<string, string>> _infos;

	private PlayerDisplay _modelDisplay;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_previewWidget).gameObject);
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragPreviewModel));
		DefaultSelectableButton chatBtn = _chatBtn;
		chatBtn.Clicked = (Action)Delegate.Combine(chatBtn.Clicked, new Action(OnClickChatButton));
		DefaultSelectableButton followBtn = _followBtn;
		followBtn.Clicked = (Action)Delegate.Combine(followBtn.Clicked, new Action(OnClickFollowingButton));
		DefaultSelectableButton reportBtn = _reportBtn;
		reportBtn.Clicked = (Action)Delegate.Combine(reportBtn.Clicked, new Action(OnClickReportButton));
		DefaultSelectableButton blockBtn = _blockBtn;
		blockBtn.Clicked = (Action)Delegate.Combine(blockBtn.Clicked, new Action(OnClickBlockButton));
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		GameSystem<SocialSystem>.Instance().FollowingListUpdated += RegisterButtonTextUpdate;
		GameSystem<SocialSystem>.Instance().BlockListUpdated += BlockButtonTextUpdate;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		GameSystem<SocialSystem>.Instance().FollowingListUpdated -= RegisterButtonTextUpdate;
		GameSystem<SocialSystem>.Instance().BlockListUpdated -= BlockButtonTextUpdate;
	}

	public void Set(Player.PlayerInfo playerInfo)
	{
		_playerInfo = playerInfo;
		SetPortrait(playerInfo.GetPortraitArgument());
		SetName(playerInfo.Name, playerInfo.Freq);
		SetInfos(new KeyValuePair<string, string>[4]
		{
			new KeyValuePair<string, string>("Level", T.Format("{0:lv:}", playerInfo.Level)),
			new KeyValuePair<string, string>("Location", playerInfo.Region.Name),
			new KeyValuePair<string, string>("Clan", playerInfo.ClanName),
			new KeyValuePair<string, string>("Home", playerInfo.ReturningRegion.Name)
		});
		SetModelDisplay(playerInfo.Display);
	}

	private void SetPortrait(PortraitBuilder.Argument argument)
	{
		_portraitArgument = argument;
	}

	private void SetName(string userName, int frequency)
	{
		_userName = userName;
		_userFrequency = frequency;
	}

	private void SetInfos(IList<KeyValuePair<string, string>> infos)
	{
		_infos = infos;
	}

	private void SetModelDisplay(PlayerDisplay display)
	{
		_modelDisplay = display;
	}

	private void RegisterButtonTextUpdate()
	{
		_followBtn.Text = LocalizeSystem.Get((!GetFollowingState(_playerInfo)) ? "#profile_follow_button_label" : "#profile_unfollow_button_label");
	}

	private void BlockButtonTextUpdate()
	{
		_blockBtn.Text = LocalizeSystem.Get((!GetBlockState(_playerInfo)) ? "#profile_block_button_label" : "#profile_unblock_button_label");
	}

	private static bool GetFollowingState(Player.PlayerInfo playerInfo)
	{
		return GameSystem<SocialSystem>.Instance().FollowingList.Contains(playerInfo.EntityId);
	}

	private static bool GetBlockState(Player.PlayerInfo playerInfo)
	{
		return GameSystem<SocialSystem>.Instance().BlockList.Contains(playerInfo.EntityId);
	}

	private void OnClickChatButton()
	{
		if (!_chatBtn.Disable)
		{
			UIManager.FindScript<ChattingGroup>().Open(_playerInfo.EntityId);
			Hide();
		}
	}

	private void OnClickFollowingButton()
	{
		if (!_followBtn.Disable && _playerInfo != null)
		{
			GameSystem<SocialSystem>.Instance().Follow(_playerInfo.EntityId, OnSuccessFollow);
		}
	}

	private void OnClickReportButton()
	{
		if (!_reportBtn.Disable)
		{
			Hide(instant: true);
			UIBase.CloseAllUI();
			UIManager.FindScript<SendReportGroup>().OpenForPlayer(_playerInfo);
		}
	}

	private void OnClickBlockButton()
	{
		if (!_blockBtn.Disable)
		{
			Hide(instant: true);
			UIBase.CloseAllUI();
			UIManager.FindScript<BlockUserGroup>().Open(_playerInfo);
		}
	}

	private void OnSuccessFollow()
	{
		string format = ((!GetFollowingState(_playerInfo)) ? "#profile_success_unregist_radio_id" : "#profile_success_regist_radio_id");
		UIManager.SystemMsg(LocalizeSystem.Format(format, _playerInfo.Name));
	}

	protected override void FillData()
	{
		PortraitBuilder.Set(_portraitArgument, _portraitTexture);
		_nameLabel.text = _userName;
		_radioLabel.text = $"[icon_radio:1.1] {_userFrequency:0000}";
		_infoItems.Set((_infos != null) ? _infos.Count : 0);
		int i = 0;
		for (int count = _infos.Count; i < count; i++)
		{
			KeyValueLabel component = _infoItems[i].GetComponent<KeyValueLabel>();
			component.Set(_infos[i].Key, _infos[i].Value);
		}
		bool flag = _playerInfo.EntityId == GameManager.PlayerId;
		((Component)_chatBtn).gameObject.SetActive(!flag);
		((Component)_followBtn).gameObject.SetActive(!flag);
		((Component)_reportBtn).gameObject.SetActive(!flag);
		((Component)_blockBtn).gameObject.SetActive(!flag);
		_followBtn.Disable = GetBlockState(_playerInfo);
		RegisterButtonTextUpdate();
		BlockButtonTextUpdate();
	}

	protected override void UpdateLayout()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		Vector3 localPosition = _infoItems.BaseObject.transform.localPosition;
		int i = 0;
		for (int count = _infoItems.Count; i < count; i++)
		{
			UIWidget component = _infoItems[i].GetComponent<UIWidget>();
			((Component)component).transform.localPosition = localPosition + Vector3.down * (float)num;
			num += component.height;
		}
		_infoWidget.height = num;
		localPosition = ((Component)_infoWidget).transform.localPosition;
		localPosition.y -= (float)_infoWidget.height;
		((Component)_buttonsWidget).transform.localPosition = localPosition;
		base.Widget.height = _titleWidget.height + _infoWidget.height + _buttonsWidget.height + 74;
		UIUtility.UpdateAnchors(((Component)this).transform);
	}

	private void MakePreviewModel()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_previewModel != (Object)null)
		{
			return;
		}
		bool male = !_modelDisplay.DefaultBody.Contains("Female");
		_previewModel = KSingleton<PlayerManager>.Instance().MakePlayerObject(male, Vector3.zero, 0uL, isPreview: true);
		PlayerManager.SetCostume(_previewModel, _modelDisplay);
		_previewModel.ChangeEquipment(null);
		NGUITools.SetLayer(((Component)_previewModel).gameObject, LayerMask.NameToLayer("NGUI"));
		((Component)_previewModel).transform.parent = _previewParent;
		((Component)_previewModel).transform.localPosition = Vector3.zero;
		((Component)_previewModel).transform.localScale = Vector3.one;
		((Component)_previewModel).transform.localRotation = Quaternion.Euler(0f, 200f, 0f);
		_previewModel.UpdateScaleBody();
		SkinnedMeshRenderer[] componentsInChildren = ((Component)_previewModel).GetComponentsInChildren<SkinnedMeshRenderer>();
		int i = 0;
		for (int num = componentsInChildren.Length; i < num; i++)
		{
			int j = 0;
			for (int num2 = ((Renderer)componentsInChildren[i]).materials.Length; j < num2; j++)
			{
				Material val = ((Renderer)componentsInChildren[i]).materials[j];
				val.renderQueue += 2000;
			}
		}
	}

	private void DestoryPreviewModel()
	{
		if ((Object)(object)_previewModel != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_previewModel).gameObject);
			_previewModel = null;
		}
	}

	private void OnDragPreviewModel(GameObject obj, Vector2 delta)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_previewModel == (Object)null))
		{
			Transform mainTransform = _previewModel.MainTransform;
			mainTransform.localEulerAngles += Vector3.down * delta.x;
		}
	}

	protected override void OnChangeState()
	{
		switch (base.State)
		{
		case VisibleState.Wait:
		case VisibleState.FadeIn:
		case VisibleState.FadeOut:
		case VisibleState.Hide:
			DestoryPreviewModel();
			break;
		case VisibleState.Show:
			MakePreviewModel();
			break;
		}
	}
}
