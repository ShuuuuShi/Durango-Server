using System.Collections;
using System.Collections.Generic;
using Holoville.HOTween;
using UnityEngine;

public class NPCFloatingGroup : MonoBehaviour
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
	private float _playerHeightOffset = 30f;

	[SerializeField]
	private float inspectingDuringTime = 4f;

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

	public IndicatorControl _indicatorBase;

	public float _indicatorTimeMargin;

	private Stack<IndicatorControl> _indicatorPool;

	private Queue<IndicatorControl> _waitIndicators;

	private float _nextIndicatorTime;

	private void Awake()
	{
		_indicatorPool = new Stack<IndicatorControl>();
		_waitIndicators = new Queue<IndicatorControl>();
		((Component)_indicatorBase).gameObject.SetActive(false);
		_cutsceneCameraController = ((Component)KSingleton<MainCamera>.Instance()).GetComponent<CutsceneCameraController>();
	}

	private void Update()
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_cutsceneCameraController) && ((Behaviour)_cutsceneCameraController).enabled)
		{
			_cutsceneCameraController.ForceUpdate();
		}
		for (int num = _targets.Count - 1; num >= 0; num--)
		{
			PosInfo posInfo = _targets[num];
			bool flag = posInfo.target.GetGameObject().activeInHierarchy && (posInfo.isClampPos || posInfo.target.IsTalkerVisible());
			if (posInfo.target != null && (Object)(object)posInfo.transform != (Object)null && flag)
			{
				Vector3 world = posInfo.transform.position + new Vector3(0f, _playerHeightOffset, 0f);
				Vector3 localPosition = MainCamera.WorldToNGUIPos(world);
				if (posInfo.isClampPos)
				{
					localPosition.x = Mathf.Clamp(localPosition.x, (float)(-Screen.width) * 0.5f, (float)Screen.width * 0.5f);
					localPosition.y = Mathf.Clamp(localPosition.y, (float)(-Screen.height) * 0.5f, (float)Screen.height * 0.5f);
				}
				posInfo.floatingUI.transform.localPosition = localPosition;
				posInfo.floatingUI.SetActive(true);
			}
			else
			{
				posInfo.floatingUI.SetActive(false);
			}
		}
		if (_waitIndicators.Count != 0 && _nextIndicatorTime < Time.time)
		{
			_nextIndicatorTime = Time.time + _indicatorTimeMargin;
			_waitIndicators.Dequeue().Begin();
		}
	}

	public PosInfo Add(IBubbleTalkable talker, TriggerDialog trigger, bool isClampPos = false)
	{
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
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
			posInfo2.floatingUI = ((Component)this).gameObject.AddChild(_floatingUI);
			posInfo = posInfo2;
			posInfo.chatBubble = ((Component)posInfo.floatingUI.transform.FindChild("ChatBubble")).gameObject;
			posInfo.chatBubbleWidget = posInfo.chatBubble.GetComponent<UIWidget>();
			posInfo.chatLabel = posInfo.chatBubble.GetComponentInChildren<UILabel>();
			posInfo.chatBubbleSprite = posInfo.chatBubble.GetComponentInChildren<UISprite>();
			posInfo.nametagLabel = ((Component)posInfo.floatingUI.transform.FindChild("Nametag")).GetComponent<UILabel>();
			posInfo.clantagLabel = ((Component)posInfo.floatingUI.transform.FindChild("Clantag")).GetComponent<UILabel>();
			posInfo.clantagLabel.color = Color.cyan;
			posInfo.statusLabel = ((Component)posInfo.floatingUI.transform.FindChild("Status")).GetComponent<UILabel>();
			posInfo.statusLabel.color = Color.white;
			posInfo.chatBubble.SetActive(false);
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
			if ((Object)(object)posInfo.trigger == (Object)(object)trigger || posInfo.target == talker)
			{
				posInfo.chatBubble.SetActive(false);
			}
		}
		((MonoBehaviour)this).StopCoroutine("coShowChatMsg");
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
				((MonoBehaviour)this).StartCoroutine("coShowChatMsg");
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
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		info.nametagLabel.color = _localPlayerNameColor;
		info.clantagLabel.color = _localPlayerClanColor;
		info.statusLabel.color = _localPlayerStatusColor;
	}

	private void Remove(PosInfo info)
	{
		Object.Destroy((Object)(object)info.floatingUI);
		_targets.Remove(info);
	}

	private IEnumerator coShowChatMsg()
	{
		PosInfo info = lastInfo;
		string msg = lastMsg;
		if (string.IsNullOrEmpty(msg))
		{
			info.chatBubble.SetActive(false);
			yield break;
		}
		info.chatBubble.SetActive(true);
		info.chatBubbleWidget.alpha = 1f;
		info.chatLabel.text = msg;
		info.chatBubbleSprite.width = (int)info.chatLabel.printedSize.x + 20;
		info.chatBubbleSprite.height = (int)info.chatLabel.printedSize.y + 22;
		info.inspectingEndTime = Time.time + inspectingDuringTime;
		yield return (object)new WaitForSeconds(inspectingDuringTime);
		TweenParms parms = new TweenParms();
		parms.Prop("alpha", (object)0f);
		parms.Ease((EaseType)4);
		HOTween.To((object)info.chatBubbleWidget, 0.2f, parms);
		info.inspectingEndTime = -1f;
		yield return (object)new WaitForSeconds(0.2f);
		if ((Object)null != (Object)(object)info.chatBubble)
		{
			info.chatBubble.SetActive(false);
		}
	}

	private void Indicator_Push(IndicatorControl indicator)
	{
		((Component)indicator).gameObject.SetActive(false);
		_indicatorPool.Push(indicator);
	}

	private IndicatorControl Indicator_Pop()
	{
		IndicatorControl indicatorControl = null;
		if (_indicatorPool.Count == 0)
		{
			indicatorControl = ((Component)((Component)_indicatorBase).transform.parent).gameObject.AddChild(((Component)_indicatorBase).gameObject).GetComponent<IndicatorControl>();
			indicatorControl.OnBegin = Indicator_OnBegin;
			indicatorControl.OnEnd = Indicator_OnEnd;
		}
		else
		{
			indicatorControl = _indicatorPool.Pop();
		}
		((Component)indicatorControl).GetComponent<UIWidget>().alpha = 0f;
		((Component)indicatorControl).gameObject.SetActive(true);
		return indicatorControl;
	}

	private void Indicator_OnBegin(IndicatorControl indicator)
	{
	}

	private void Indicator_OnEnd(IndicatorControl indicator)
	{
		Indicator_Push(indicator);
	}

	public void AddIndicator(string text, GameObject target = null)
	{
		IndicatorControl indicatorControl = Indicator_Pop();
		indicatorControl.Target = target;
		indicatorControl.Text = text;
		_waitIndicators.Enqueue(indicatorControl);
	}
}
