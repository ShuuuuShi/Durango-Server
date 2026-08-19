using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
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

	[CompilerGenerated]
	private sealed class _003CCoShowChatMsg_003Ed__25 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueNPCFloatingGroup _003C_003E4__this;

		private PosInfo _003Cinfo_003E5__2;

		object IEnumerator<object>.Current
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
		public _003CCoShowChatMsg_003Ed__25(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003Cinfo_003E5__2 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PrologueNPCFloatingGroup prologueNPCFloatingGroup = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
			{
				_003C_003E1__state = -1;
				_003Cinfo_003E5__2 = prologueNPCFloatingGroup.lastInfo;
				string lastMsg = prologueNPCFloatingGroup.lastMsg;
				if (string.IsNullOrEmpty(lastMsg))
				{
					_003Cinfo_003E5__2.chatBubble.SetActive(value: false);
					return false;
				}
				_003Cinfo_003E5__2.chatBubble.SetActive(value: true);
				_003Cinfo_003E5__2.chatBubbleWidget.alpha = 1f;
				_003Cinfo_003E5__2.chatLabel.text = lastMsg;
				_003Cinfo_003E5__2.chatBubbleSprite.width = (int)_003Cinfo_003E5__2.chatLabel.printedSize.x + (int)prologueNPCFloatingGroup.bubbleSizeOffset.x;
				_003Cinfo_003E5__2.chatBubbleSprite.height = (int)_003Cinfo_003E5__2.chatLabel.printedSize.y + (int)prologueNPCFloatingGroup.bubbleSizeOffset.y;
				_003Cinfo_003E5__2.inspectingEndTime = Time.time + prologueNPCFloatingGroup.inspectingDuringTime;
				_003C_003E2__current = new WaitForSeconds(prologueNPCFloatingGroup.inspectingDuringTime);
				_003C_003E1__state = 1;
				return true;
			}
			case 1:
			{
				_003C_003E1__state = -1;
				TweenAlpha tweenAlpha = TweenAlpha.Begin(_003Cinfo_003E5__2.chatBubbleWidget.gameObject, 0.2f, 0f);
				tweenAlpha.method = UITweener.Method.EaseIn;
				tweenAlpha.PlayForward();
				_003Cinfo_003E5__2.inspectingEndTime = -1f;
				_003C_003E2__current = new WaitForSeconds(0.2f);
				_003C_003E1__state = 2;
				return true;
			}
			case 2:
				_003C_003E1__state = -1;
				if (null != _003Cinfo_003E5__2.chatBubble)
				{
					_003Cinfo_003E5__2.chatBubble.SetActive(value: false);
				}
				return false;
			}
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
				Vector3 localPosition = MainCamera.WorldToNGUIPos(posInfo.transform.position + new Vector3(0f, _playerHeightOffset, 0f));
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
			posInfo = new PosInfo
			{
				inspectingEndTime = -1f,
				target = talker,
				transform = talker.GetTalkBubbleTransform(),
				floatingUI = base.gameObject.AddChild(_floatingUI)
			};
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
		UnityEngine.Object.Destroy(info.floatingUI);
		_targets.Remove(info);
	}

	private IEnumerator CoShowChatMsg()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoShowChatMsg_003Ed__25(0)
		{
			_003C_003E4__this = this
		};
	}
}
