using System.Collections;
using System.Collections.Generic;
using Durango.Prologue;
using Durango.Render.Camera;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueNPCFloatingGroup : MonoBehaviour
{
	public class PosInfo
	{
		public float inspectingEndTime;

		public IBubbleTalkable target;

		public Transform transform;

		public GameObject floatingUI;

		public UILabel nametagLabel;

		public UILabel clantagLabel;

		public UILabel statusLabel;

		public GameObject chatBubble;

		public UILabel chatLabel;

		public UISprite chatBubbleSprite;

		public UIWidget chatBubbleWidget;

		public TriggerDialog trigger;

		public bool isClampPos;
	}

	[SerializeField]
	private GameObject _floatingUI;

	[SerializeField]
	private float _playerHeightOffset;

	[SerializeField]
	private float _horizontalOffset;

	[SerializeField]
	private float inspectingDuringTime;

	[SerializeField]
	private Vector2 bubbleSizeOffset;

	[SerializeField]
	private Color _localPlayerNameColor;

	[SerializeField]
	private Color _allyPlayerNameColor;

	[SerializeField]
	private Color _playerNameColor;

	[SerializeField]
	private Color _localPlayerClanColor;

	[SerializeField]
	private Color _allyPlayerClanColor;

	[SerializeField]
	private Color _playerClanColor;

	[SerializeField]
	private Color _localPlayerStatusColor;

	[SerializeField]
	private Color _playerStatusColor;

	private List<PosInfo> _targets = new List<PosInfo>();

	private CutsceneCameraController _cutsceneCameraController;

	private PosInfo lastInfo;

	private string lastMsg;

	private void Awake()
	{
		_cutsceneCameraController = Singleton<MainCamera>.Instance().GetComponent<CutsceneCameraController>();
	}

	private void LateUpdate()
	{
		if ((bool)_cutsceneCameraController && _cutsceneCameraController.enabled)
		{
			_cutsceneCameraController.ForceUpdate();
		}
		for (int num = _targets.Count - 1; num >= 0; num--)
		{
			PosInfo posInfo = _targets[num];
			bool flag = posInfo.target.GetGameObject().activeInHierarchy && (posInfo.isClampPos || posInfo.target.IsTalkerVisible());
			if (posInfo.target != null && posInfo.transform != null && flag)
			{
				Vector3 world = posInfo.transform.position + new Vector3(0f, _playerHeightOffset, 0f);
				Vector3 localPosition = MainCamera.WorldToNGUIPos(world);
				localPosition.x += _horizontalOffset;
				if (posInfo.isClampPos)
				{
					localPosition.x = Mathf.Clamp(localPosition.x, (float)(-Screen.width) * 0.5f, (float)Screen.width * 0.5f);
					localPosition.y = Mathf.Clamp(localPosition.y, (float)(-Screen.height) * 0.5f, (float)Screen.height * 0.5f);
				}
				posInfo.floatingUI.transform.localPosition = localPosition;
				posInfo.floatingUI.SetActive(value: true);
			}
			else
			{
				posInfo.floatingUI.SetActive(value: false);
			}
		}
	}

	public PosInfo Add(IBubbleTalkable talker, TriggerDialog trigger, bool isClampPos = false)
	{
		PosInfo posInfo = null;
		int count = _targets.Count;
		for (int i = 0; i < count; i++)
		{
			if (_targets[i].target == talker)
			{
				posInfo = _targets[i];
				break;
			}
		}
		if (posInfo == null)
		{
			PosInfo posInfo2 = new PosInfo();
			posInfo2.inspectingEndTime = -1f;
			posInfo2.target = talker;
			posInfo2.transform = talker.GetTalkBubbleTransform();
			posInfo2.floatingUI = base.gameObject.AddChild(_floatingUI);
			posInfo = posInfo2;
			posInfo.chatBubble = posInfo.floatingUI.transform.Find("ChatBubble").gameObject;
			posInfo.chatBubbleWidget = posInfo.chatBubble.GetComponent<UIWidget>();
			posInfo.chatLabel = posInfo.chatBubble.GetComponentInChildren<UILabel>();
			posInfo.chatBubbleSprite = posInfo.chatBubble.GetComponentInChildren<UISprite>();
			posInfo.nametagLabel = posInfo.floatingUI.transform.Find("Nametag").GetComponent<UILabel>();
			posInfo.clantagLabel = posInfo.floatingUI.transform.Find("Clantag").GetComponent<UILabel>();
			posInfo.clantagLabel.color = Color.cyan;
			posInfo.statusLabel = posInfo.floatingUI.transform.Find("Status").GetComponent<UILabel>();
			posInfo.statusLabel.color = Color.white;
			posInfo.chatBubble.SetActive(value: false);
			posInfo.trigger = trigger;
			posInfo.isClampPos = isClampPos;
			_targets.Add(posInfo);
		}
		return posInfo;
	}

	public void ShowChatMsg(IBubbleTalkable talker, string msg, TriggerDialog trigger)
	{
		int count = _targets.Count;
		for (int i = 0; i < count; i++)
		{
			PosInfo posInfo = _targets[i];
			if (posInfo.trigger == trigger || posInfo.target == talker)
			{
				posInfo.chatBubble.SetActive(value: false);
			}
		}
		StopCoroutine("CoShowChatMsg");
		if (msg == null)
		{
			return;
		}
		for (int j = 0; j < count; j++)
		{
			PosInfo posInfo2 = _targets[j];
			if (posInfo2.target == talker)
			{
				lastInfo = posInfo2;
				lastMsg = msg;
				StartCoroutine("CoShowChatMsg");
				break;
			}
		}
	}

	public void SetNametag(IBubbleTalkable talker, string name)
	{
		PosInfo posInfo = null;
		int count = _targets.Count;
		for (int i = 0; i < count; i++)
		{
			if (_targets[i].target == talker)
			{
				posInfo = _targets[i];
				break;
			}
		}
		posInfo.nametagLabel.text = name;
		posInfo.clantagLabel.text = string.Empty;
		RefreshLabelColor(posInfo);
	}

	private void RefreshLabelColor(PosInfo info)
	{
		info.nametagLabel.color = _localPlayerNameColor;
		info.clantagLabel.color = _localPlayerClanColor;
		info.statusLabel.color = _localPlayerStatusColor;
	}

	private void Remove(PosInfo info)
	{
		Object.Destroy(info.floatingUI);
		_targets.Remove(info);
	}

	private IEnumerator CoShowChatMsg()
	{
		PosInfo info = lastInfo;
		string msg = lastMsg;
		if (string.IsNullOrEmpty(msg))
		{
			info.chatBubble.SetActive(value: false);
			yield break;
		}
		info.chatBubble.SetActive(value: true);
		info.chatBubbleWidget.alpha = 1f;
		info.chatLabel.text = msg;
		info.chatBubbleSprite.width = (int)info.chatLabel.printedSize.x + (int)bubbleSizeOffset.x;
		info.chatBubbleSprite.height = (int)info.chatLabel.printedSize.y + (int)bubbleSizeOffset.y;
		info.inspectingEndTime = Time.time + inspectingDuringTime;
		yield return new WaitForSeconds(inspectingDuringTime);
		TweenAlpha tween = TweenAlpha.Begin(info.chatBubbleWidget.gameObject, 0.2f, 0f);
		tween.method = UITweener.Method.EaseIn;
		tween.PlayForward();
		info.inspectingEndTime = -1f;
		yield return new WaitForSeconds(0.2f);
		if (null != info.chatBubble)
		{
			info.chatBubble.SetActive(value: false);
		}
	}
}
