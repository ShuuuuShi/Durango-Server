using System;
using K1Network;
using Messages;
using Player;
using Shared.System;
using TimerData;
using UnityEngine;

public class BuildPostprocessHelpGroup : UIBase
{
	private const int PortraitRowCount = 4;

	private const int MaxPortraitCount = 7;

	[SerializeField]
	private GameObject _container;

	[SerializeField]
	private ListObjectPool _portraits;

	[SerializeField]
	private GameObject _buttonHelp;

	[SerializeField]
	private TimerProgressGauge _timerProgressGauge;

	[SerializeField]
	private GameObject _touchBox;

	private Artifact _artifact;

	private void Awake()
	{
		UIEventListener.Get(_buttonHelp).onClick = OnClickButtonHelp;
		_portraits.Init(delegate(GameObject gameObject)
		{
			UIEventListener.Get(gameObject).onClick = OnClickPortrait;
		});
		_container.gameObject.SetActive(false);
		UIEventListener uIEventListener = UIEventListener.Get(_touchBox);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, (UIEventListener.BoolDelegate)delegate(GameObject go, bool press)
		{
			if (!press)
			{
				ForceClose();
			}
		});
	}

	private void Start()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.HelpPostprocess, delegate(InteractionObject target)
		{
			_artifact = target.GetTargetComponent<Artifact>();
			if ((Object)(object)_artifact != (Object)null && _artifact.ArtifactState.Postprocess.HasValue)
			{
				RequestPlayerInfo(_artifact.ArtifactState.Postprocess.Value.Helpers);
				Open();
			}
		});
	}

	protected override bool OnOpen()
	{
		_container.gameObject.SetActive(true);
		if (_artifact.PostProcessTimer != null)
		{
			_timerProgressGauge.Play(_artifact.PostProcessTimer);
			_artifact.PostProcessTimer.Finished += PostProcessTimer_Finished;
		}
		return true;
	}

	protected override bool OnClose()
	{
		_container.gameObject.SetActive(false);
		if (_artifact.PostProcessTimer != null)
		{
			_artifact.PostProcessTimer.Finished -= PostProcessTimer_Finished;
		}
		_artifact = null;
		return true;
	}

	private void RequestPlayerInfo(ulong[] ids)
	{
		_portraits.Clear();
		UpdatePortraitsLayout();
		for (int i = 0; i < ids.Length; i++)
		{
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfo(ids[i], delegate(Player.PlayerInfo info)
			{
				if (info.Valid)
				{
					BuildPostprocessPortrait buildPostprocessPortrait = ((ListObjectPoolBase<GameObject>)_portraits).Add<BuildPostprocessPortrait>();
					buildPostprocessPortrait.SetPlayerInfo(info);
					UpdatePortraitsLayout();
				}
			});
		}
	}

	private void UpdatePortraitsLayout()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = _portraits.BaseObject.GetComponent<UIWidget>();
		Vector3 localPosition = _portraits.BaseObject.transform.localPosition;
		int width = (((Object)(object)component != (Object)null) ? component.width : 0);
		int height = (((Object)(object)component != (Object)null) ? component.height : 0);
		int num = Math.Min(_portraits.Count, 7);
		for (int i = 0; i < num; i++)
		{
			_portraits[i].transform.localPosition = GetPortraitPosition(localPosition, i, width, height);
		}
		_buttonHelp.transform.localPosition = GetPortraitPosition(localPosition, num, width, height);
	}

	private void OnClickPortrait(GameObject go)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		BuildPostprocessPortrait component = go.GetComponent<BuildPostprocessPortrait>();
		if ((Object)(object)component != (Object)null)
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(string.Empty, component.PlayerName);
			widgetTooltipControl.Show(go, Vector2.zero, 5f);
		}
	}

	private void OnClickButtonHelp(GameObject go)
	{
		if ((Object)(object)_artifact != (Object)null)
		{
			HelpPostprocess helpPostprocess = default(HelpPostprocess);
			helpPostprocess.EntityId = _artifact.EntityId;
			helpPostprocess.Tile = _artifact.WorldTile;
			HelpPostprocess msg = helpPostprocess;
			Connections.Frontend.Send(msg).On(delegate(HelpedPostprocess helped, PacketHeader _)
			{
				UIManager.SystemMsg(LocalizeSystem.Format("#artifact_postprocess_help_result", TimerSystem.TimeToString(Mathf.CeilToInt(0f - helped.Timedelta)), helped.LeftHelpableCount.ToString()));
			});
		}
		Close();
	}

	private void PostProcessTimer_Finished(TimerData.Timer obj)
	{
		Close();
	}

	private static Vector3 GetPortraitPosition(Vector3 origin, int index, int width, int height)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		float num = origin.x + (float)(width * (index % 4));
		float num2 = origin.y - (float)(height * (index / 4));
		return Vector3.up * num2 + Vector3.right * num;
	}
}
