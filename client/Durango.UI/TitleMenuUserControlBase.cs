using System;
using System.Collections.Generic;
using Durango.Logic.Clusters;
using Durango.System;
using Durango.UI.Control;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class TitleMenuUserControlBase : MonoBehaviour
{
	private const string ClusterPrefKey = "last_selected_cluster_key";

	protected readonly Clusters Clusters = new Clusters();

	protected TitleMenuGroup.State LastState;

	protected bool IsAccountReady;

	[SerializeField]
	protected UILabel _explainLabel;

	[SerializeField]
	protected UIWidget _mainContent;

	[SerializeField]
	private TitleClusterSelection _clusterSelection;

	[SerializeField]
	private UILabel _versionInfoLabel;

	[SerializeField]
	private TitleMessageBoxBase _messageBox;

	[SerializeField]
	private UILabel _clusterSelectionButtonLabel;

	[SerializeField]
	private UIWidget _selectionButtnoHolder;

	[SerializeField]
	private UIWidget _buttonSeperator;

	[SerializeField]
	private SelectableWidget _clusterSelectionButton;

	[SerializeField]
	private SelectableWidget _playerSelectionButton;

	[SerializeField]
	private UILabel _playerSelectionButtonLabel;

	[SerializeField]
	protected SelectableWidget _logoutButton;

	[SerializeField]
	private SelectableWidget _noticeButton;

	[SerializeField]
	private ListObjectPool _outlinks;

	[SerializeField]
	private RectLayoutComponent _bottomLayout;

	[SerializeField]
	private TweenerPlayer _tweener;

	[SerializeField]
	private SelectableWidget _clusterSelectionBackButton;

	private Action _onConfirm;

	private Action _onPlayerSelection;

	private float _nextMaintenanceCheckTime;

	public bool QuitWhenErrorOccurred { get; set; }

	public bool IsLoginProcess { get; set; }

	public virtual bool RetryConnect { get; set; }

	private string LastSelectedClusterKey
	{
		get
		{
			string text = Preferences.GetString("last_selected_cluster_key", string.Empty);
			if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(Clusters.GetCluster(text).GatewayUrlRoot))
			{
				text = (LastSelectedClusterKey = Clusters.GetRecommendableCluster());
			}
			return text;
		}
		set
		{
			if (!string.IsNullOrEmpty(value))
			{
				Preferences.SetString("last_selected_cluster_key", value);
			}
		}
	}

	public bool IsMessageBoxOpen => _messageBox.gameObject.activeInHierarchy;

	protected virtual void Start()
	{
		TitleUIRootResizer.AddOnScreenResized(OnScreenResized);
		_clusterSelectionButton.Clicked = ClusterSelectButton_Clicked;
		_playerSelectionButton.Clicked = PlayerSelectionButton_Clicked;
		_noticeButton.Clicked = Platform.Instance.ShowNotice;
		_messageBox.Close();
		UpdateVersionInfo(string.Empty);
		_tweener.Play(1f);
		_clusterSelectionBackButton.Clicked = delegate
		{
			OnReceiveBackMessage(null);
		};
		UIEventListener.Get(_mainContent.gameObject).onClick = delegate
		{
			OnConfirm();
		};
		GameSystem<InputSystem>.Instance().On(InputCommand.Back, OnReceiveBackMessage);
		GameSystem<InputSystem>.Instance().On(InputCommand.SelectCurrentCell, OnReceiveSelectCurrentCellMessage);
	}

	private void OnDestroy()
	{
		GameSystem<InputSystem>.Instance().Off(InputCommand.Back, OnReceiveBackMessage);
		GameSystem<InputSystem>.Instance().Off(InputCommand.SelectCurrentCell, OnReceiveSelectCurrentCellMessage);
	}

	private void Update()
	{
		if (LastState == TitleMenuGroup.State.SelectCluster && _nextMaintenanceCheckTime > 0f && _nextMaintenanceCheckTime < Time.realtimeSinceStartup)
		{
			UpdateServerAndPlayerInfo();
		}
	}

	public virtual void ShowCluster(Action onConfirm, Action onPlayerSelection, Action onLogout, bool autoConfirm)
	{
		HideOutlinks();
		_onConfirm = onConfirm;
		_onPlayerSelection = onPlayerSelection;
		Cluster selectedCluster = GetSelectedCluster();
		GameManager.SetCluster(LastSelectedClusterKey, selectedCluster.GatewayUrlRoot, selectedCluster.Mode);
		UpdateServerAndPlayerInfo();
		if (autoConfirm)
		{
			OnConfirm();
		}
	}

	private void ClusterSelectButton_Clicked()
	{
		_mainContent.gameObject.SetActive(value: false);
		_clusterSelection.gameObject.SetActive(value: true);
		_clusterSelection.ShowClusters(Clusters, OnClusterConfirmed, LastSelectedClusterKey);
	}

	private void PlayerSelectionButton_Clicked()
	{
		if (_onPlayerSelection != null)
		{
			_onPlayerSelection();
		}
	}

	protected virtual void OnConfirm()
	{
		UnityEngine.Debug.Log("[durango] OnConfirm: state=" + LastState + " accountReady=" + IsAccountReady + " autoTarget='" + Durango.Offline.Server.AutoConnectTarget + "' emigrated=" + GameManager.Emigrated);
		// [แก้เอง] 24 ส.ค. 2026 — เอา "โชว์กล่องกรอก IP ทันที" ออกจากตรงนี้แล้ว (เคยดักปุ่มยืนยันของ
		// **ทุกโหมด** ไว้หมด ตราบใดที่ Emigrated == None ⇒ เลือกโหมดจากหน้า "เลือกเซิร์ฟเวอร์" ไม่ได้จริง)
		// เจ้าของสั่งชัดว่า "mainUI ต้องเลือกได้ว่าจะเล่นโหมดไหน ไม่ใช่บังคับออนไลน์แบบนี้" — ตอนนี้ปุ่ม
		// "Online Server (For Test)" คือจุดที่ต่อเซิร์ฟจริงแทน (ดู Cluster.OnConfirm key "online" ใน
		// Durango.Offline/Server.cs) ส่วน free/solo/multi ยังเป็นเซิร์ฟจำลองในเครื่องเหมือนเดิม
		//
		// ⚠️ แต่ AutoConnectTarget (ตัวแปรที่ operator ตั้งเอง ผ่าน env DURANGO_AUTOCONNECT หรือ
		// server.txt ในชุดแจก tools/dist-template) **ต้อง intercept ตรงนี้เหมือนเดิม** — คนละเรื่องกับ
		// "โหมดที่ผู้เล่นเลือกเอง" ข้างบน นี่คือ operator บังคับ build นี้ให้ต่อเซิร์ฟเดียวเสมอ (ใช้แจกเพื่อน
		// ผ่าน เล่นเกม.bat ให้กดแค่แตะจอเดียวเข้าเกมเลย ไม่ต้องเลือกเมนู) ลืม intercept ตรงนี้ไปตอนแรก ⇒ คน
		// ที่รับชุดแจกไปต้องกดผ่านหน้า "เลือกเซิร์ฟเวอร์" เองก่อนเป็นอย่างน้อย 3 ครั้ง กว่า BeginServer()
		// (ที่เช็ค AutoConnectTarget เหมือนกัน) จะมีโอกาสทำงาน — ดูเหมือนเกม "ค้างหน้า main" เฉยๆ
		if (!string.IsNullOrEmpty(Durango.Offline.Server.AutoConnectTarget)
			&& GameManager.Emigrated == GameManager.EmigratedType.None)
		{
			Durango.Offline.Server._autoConnected = false;
			Durango.Offline.Server.ConnectTo(Durango.Offline.Server.AutoConnectTarget);
			return;
		}
		if (LastState != TitleMenuGroup.State.SelectCluster || !IsAccountReady)
		{
			return;
		}
		_clusterSelectionButton.Disabled = true;
		_playerSelectionButton.Disabled = true;
		if (_onConfirm != null)
		{
			Account selectedAccount = GetSelectedAccount();
			if (selectedAccount != null)
			{
				Pair<string, int> recommendedPlayer = selectedAccount.GetRecommendedPlayer();
				GameManager.PlayerId = recommendedPlayer.Item1;
				GameManager.PlayerSlotIndex = recommendedPlayer.Item2;
			}
			else
			{
				GameManager.PlayerId = null;
			}
			_onConfirm();
		}
	}

	private void OnClusterConfirmed(string selectedClusterKey)
	{
		_mainContent.gameObject.SetActive(value: true);
		_clusterSelection.gameObject.SetActive(value: false);
		LastSelectedClusterKey = selectedClusterKey;
		Cluster selectedCluster = GetSelectedCluster();
		GameManager.SetCluster(LastSelectedClusterKey, selectedCluster.GatewayUrlRoot, selectedCluster.Mode);
		UpdateServerAndPlayerInfo();
	}

	public virtual void OnStateChanged(TitleMenuGroup.State state)
	{
		LastState = state;
		bool active = state == TitleMenuGroup.State.SelectCluster;
		if (Clusters.Offline)
		{
			active = false;
		}
		_logoutButton.gameObject.SetActive(active);
		_noticeButton.gameObject.SetActive(active);
		if (state == TitleMenuGroup.State.Initial)
		{
			_selectionButtnoHolder.gameObject.SetActive(value: false);
			HideOutlinks();
		}
	}

	public void SetExplainLabel(string text, bool important = false)
	{
		if (GameManager.Emigrated == GameManager.EmigratedType.None || important)
		{
			_explainLabel.text = text;
		}
		_bottomLayout.UpdateLayout();
	}

	public void UpdateVersionInfo(string serverVersion = "")
	{
		string text = "* Client: " + CurrentBundleVersion.GetClientVersion();
		if (!string.IsNullOrEmpty(serverVersion))
		{
			text = text + " / Server: " + serverVersion;
		}
		string nPA = Platform.Instance.NPA;
		if (!string.IsNullOrEmpty(nPA))
		{
			text = text + " / NPA: " + nPA;
		}
		_versionInfoLabel.text = text;
	}

	public bool IsInMaintenance()
	{
		return Clusters.IsInMaintenance();
	}

	public virtual bool ShowMaintenance()
	{
		IList<Urls> outlinks = Clusters.GetOutlinks();
		string maintenanceText = Clusters.GetMaintenanceText(LocalizeSystem.Locale);
		if (outlinks.Count <= 0 || string.IsNullOrEmpty(maintenanceText))
		{
			HideOutlinks();
			return false;
		}
		SetExplainLabel(maintenanceText, important: true);
		_outlinks.BaseObject.transform.parent.gameObject.SetActive(value: true);
		_outlinks.BeginLoad();
		for (int i = 0; i < outlinks.Count; i++)
		{
			Urls urls = outlinks[i];
			string title = urls.GetTitle(LocalizeSystem.Locale);
			if (!string.IsNullOrEmpty(title))
			{
				TitleOutlinkNode component = _outlinks.GetNext().GetComponent<TitleOutlinkNode>();
				component.Set(title, urls);
			}
		}
		_outlinks.EndLoad();
		UpdateOutlinkLayout();
		return true;
	}

	protected virtual void HideOutlinks()
	{
		_outlinks.BaseObject.transform.parent.gameObject.SetActive(value: false);
		_bottomLayout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	public void UpdateServerAndPlayerInfo(bool forceUpdate = false)
	{
		string lastSelectedClusterKey = LastSelectedClusterKey;
		Cluster cluster = Clusters.GetCluster(lastSelectedClusterKey);
		_clusterSelectionButton.Disabled = false;
		_clusterSelectionButtonLabel.text = cluster.GetName(LocalizeSystem.Locale);
		_clusterSelectionButtonLabel.color = new Color32(194, 24, 91, 255);
		// [แก้เอง] 31 ส.ค. 2026 — จุดสถานะเซิร์ฟไปโชว์ที่ป้าย "Select Server" แทน (ดู TitleMenuUserControl_PC)
		Durango.Offline.Server.RefreshServerStatus();
		UpdateButtonLayout(showPlayerButton: false);
		IsAccountReady = false;
		SetExplainLabel(ManualTranslator.LoadingUserInfo);
		_nextMaintenanceCheckTime = 0f;
		Clusters.GetOrRequestAccounts(lastSelectedClusterKey, OnClusterAccountUpdated, forceUpdate);
	}

	/// <summary>
	/// [แก้เอง] 24 ส.ค. 2026 (รอบ 2) — ให้เรียกจากภายนอก (TitleMenuGroup, โหมด IsLoginProcess) แบบ
	/// async-safe จริง ๆ คือรอผลตอบจากเซิร์ฟก่อนค่อยเรียก callback
	///
	/// 🐛 เดิม TitleMenuGroup เรียก UpdateServerAndPlayerInfo(forceUpdate: true) แล้วอ่าน
	/// GetSelectedAccount() ทันทีบรรทัดถัดมาเลย — ตอนที่ /accounts ยังเป็นแค่ตัวแปรในหน่วยความจำ
	/// (ตอบทันทีแบบ sync) ก็ใช้ได้ แต่พอเปลี่ยนเป็นเรียก endpoint จริงทาง HTTP (async) คำตอบยังไม่ทันมาถึง
	/// ตอนอ่านค่า ⇒ เห็นเป็นค่าง่างเสมอ ⇒ คิดว่า "ไม่มีตัวละคร" ทั้งที่จริงมี บังคับสร้างใหม่ทุกครั้ง
	/// </summary>
	public void RequestAccountAsync(Action<Account> callback)
	{
		Clusters.GetOrRequestAccounts(LastSelectedClusterKey, callback, forceUpdate: true);
	}

	protected virtual void OnClusterAccountUpdated(Account account)
	{
		Cluster selectedCluster = GetSelectedCluster();
		if (account == null && selectedCluster.IsInMaintenance())
		{
			_nextMaintenanceCheckTime = Time.realtimeSinceStartup + 60f;
			SetExplainLabel(selectedCluster.GetMaintenanceText(LocalizeSystem.Locale), important: true);
			return;
		}
		bool flag = account != null && account.MaxPlayerSlotCount > 1 && account.PlayerSlotCount >= 1;
		UpdateButtonLayout(flag);
		if (flag)
		{
			_playerSelectionButton.Disabled = false;
			string playerInfoText = account.GetPlayerInfoText(account.GetRecommendedPlayer().Item1);
			_playerSelectionButtonLabel.text = (string.IsNullOrEmpty(playerInfoText) ? ManualTranslator.NoCharacter : playerInfoText);
		}
		IsAccountReady = true;
		SetExplainLabel(ManualTranslator.TouchTheScreen);
	}

	protected virtual void UpdateButtonLayout(bool showPlayerButton)
	{
		bool flag = Clusters.Count >= 2;
		_selectionButtnoHolder.gameObject.SetActive(flag || showPlayerButton);
		_selectionButtnoHolder.width = ((!flag || !showPlayerButton) ? 272 : 554);
		_clusterSelectionButton.gameObject.SetActive(flag);
		_playerSelectionButton.gameObject.SetActive(showPlayerButton);
		if ((bool)_buttonSeperator)
		{
			_buttonSeperator.gameObject.SetActive(showPlayerButton);
		}
		UIWidget[] nodes = new UIWidget[2] { _clusterSelectionButton.Widget, _playerSelectionButton.Widget };
		UIUtility.WidgetsGridReposition(nodes, null, Vector2.right, _selectionButtnoHolder.localCorners[1], _selectionButtnoHolder.height, new Vector2(272f, 48f), 0f, 0f);
		_bottomLayout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	[NotNull]
	public Cluster GetSelectedCluster()
	{
		return Clusters.GetCluster(LastSelectedClusterKey);
	}

	public string GetSelectedClusterKey()
	{
		return LastSelectedClusterKey;
	}

	[CanBeNull]
	public Account GetSelectedAccount()
	{
		return Clusters.GetAccount(LastSelectedClusterKey);
	}

	public bool TryUpdateClusters(string response)
	{
		Clusters.LoadFromJson(response);
		PromoteCommunityCluster();
		if (Clusters.Count > 0)
		{
			GameManager.SetArenaAuthServer(Clusters.ArenaAuthUrl);
			return true;
		}
		return false;
	}

	/// <summary>
	/// [แก้เอง] 1 ก.ย. 2026 — ดัน "DurangoTH Community Server" ให้เป็นคลัสเตอร์แนะนำ + ใส่ gateway ให้จริง
	///
	/// 🐛 บั๊กที่เจอตอนเทส: ผู้เล่นเลือกเซิร์ฟชุมชนแล้วเข้าเกมได้ปกติ แต่พอปิด-เปิดเกมใหม่กลับเด้งไป
	///    "Creative Island" (โหมดออฟไลน์ในเครื่อง) ทุกครั้ง แล้วขึ้น "ไม่สามารถโหลดข้อมูลผู้เล่นได้"
	///    ต้นเหตุอยู่ที่ LastSelectedClusterKey: มันเก็บคีย์ "online" ลง Preferences จริง แต่ตอนอ่านกลับมา
	///    มีเงื่อนไข `string.IsNullOrEmpty(GetCluster(text).GatewayUrlRoot)` — คลัสเตอร์ "online" ในไฟล์
	///    clusters ของเกมไม่มี gateway ใส่ไว้ (ของ NEXON ตายไปแล้ว) ⇒ ถือว่าคีย์ใช้ไม่ได้ แล้วรีเซ็ต
	///    กลับไปคลัสเตอร์แนะนำ ซึ่งเดิมคือ "free" = Creative Island
	///
	/// เติม gateway จาก server.txt ให้ตั้งแต่ตอนโหลดรายการ ⇒ คีย์ค้างได้จริง และเป็นค่าเริ่มต้นของคนใหม่
	/// (ยังเห็นรายการเซิร์ฟครบเหมือนเดิม เลือกโหมดออฟไลน์เองได้อยู่)
	/// </summary>
	private void PromoteCommunityCluster()
	{
		string target = Durango.Offline.Server.ResolveOnlineTarget();
		if (string.IsNullOrEmpty(target))
		{
			return;
		}
		string gateway = Durango.Offline.Server.ToGatewayUrl(target);
		if (string.IsNullOrEmpty(gateway))
		{
			return;
		}
		string[] keys = Clusters.GetClusterKeys();
		bool found = false;
		for (int i = 0; i < keys.Length; i++)
		{
			Durango.Logic.Clusters.Cluster cluster = Clusters.GetCluster(keys[i]);
			if (cluster == null)
			{
				continue;
			}
			// คีย์จริงที่ Clusters.LoadFromJson ลงทะเบียนคือ "<key>_offline" (ดู Clusters.cs)
			// เทียบกับ "online" เฉย ๆ จะไม่มีวันตรง แล้วจะไปล้างธง IsRecommendable ทิ้งทั้งหมด
			bool isOnline = keys[i] == "online" || keys[i] == "online_offline";
			cluster.IsRecommendable = isOnline;
			if (isOnline)
			{
				cluster.GatewayUrlRoot = gateway;
				found = true;
			}
		}
		if (!found)
		{
			UnityEngine.Debug.LogWarning("[durango] ไม่พบคลัสเตอร์ \"online\" ในรายการ — ใช้ค่าเริ่มต้นของเกมแทน");
		}
	}

	public void ForceSetClusters(string gateway)
	{
		Clusters.ForceSetCluster(gateway);
		// [แก้เอง] ตั้ง GatewayUrl ทันที (path ปกติตั้งใน ShowCluster ซึ่งเราข้ามไปแล้ว)
		// ต้องแปลง ip → http://ip:8190 เหมือน Server.ConnectTo ไม่งั้น /knock ต่อท้าย URL พัง
		string url = gateway.StartsWith("http://") ? gateway : "http://" + gateway + ":" + 8190;
		string recommendedKey = Clusters.GetRecommendableCluster();
		GameManager.SetCluster(recommendedKey, url, Mode.Offline);
		// [แก้เอง] 24 ส.ค. 2026 — ต้องอัปเดต LastSelectedClusterKey (คีย์นี้ผ่าน Preferences จริง ๆ)
		// ด้วย ไม่งั้นถ้าผู้เล่นเคยเลือกคลัสเตอร์อื่นค้างไว้ก่อนหน้า (เช่นกด "Online Server (For Test)"
		// ในหน้าเลือกเซิร์ฟเวอร์ ซึ่งเซฟคีย์ "online" ลง Preferences) — TitleMenuGroup.State.SelectCluster
		// จะเรียก ShowCluster() ต่อทันทีหลังจากนี้ ซึ่งอ่าน LastSelectedClusterKey (คีย์เก่าที่ค้างไว้) มา
		// เรียก GetSelectedCluster() ซ้ำ แล้วทับ GatewayUrl กลับไปเป็นของคลัสเตอร์เก่า (เซิร์ฟจำลองในเครื่อง
		// พอร์ต 8390) เงียบ ๆ — client เลยยิง /knock,/sessions ไปเซิร์ฟจำลองแทนเซิร์ฟจริงที่เราตั้งไว้ตรงนี้
		// (WebServer.cs ฝั่งนั้นตอบ 400 ให้ POST ที่ไม่ใช่ form-urlencoded ⇒ "[400] Bad Request" ที่เห็น)
		LastSelectedClusterKey = recommendedKey;
		// [แก้เอง] ต้องดึง account ทันที — เดิมเรียกใน ShowCluster ซึ่ง flow autoconnect ข้าม
		// ไม่งั้น State.SelectPlayer เจอ account null → State.Error "เชื่อมต่อไม่ได้ (SelectPlayer)"
		// (OnRequestAccount ของเราทำงาน sync → ออกจากบรรทัดนี้ account พร้อมใช้เลย)
		UpdateServerAndPlayerInfo();
	}

	private void OnScreenResized()
	{
		UpdateOutlinkLayout();
		Rect safeRect = TitleUIRootResizer.GetSafeRect();
		_mainContent.leftAnchor.relative = safeRect.xMin;
		_mainContent.rightAnchor.relative = safeRect.xMax;
		_mainContent.bottomAnchor.relative = safeRect.yMin;
		_mainContent.topAnchor.relative = safeRect.yMax;
		UIUtility.UpdateAnchors(_mainContent.transform);
	}

	private void UpdateOutlinkLayout()
	{
		bool isPortrait = TitleUIRootResizer.IsPortrait;
		UIWidget component = _outlinks.BaseObject.transform.parent.GetComponent<UIWidget>();
		Point2 point = default(Point2);
		if (isPortrait)
		{
			float num = UIUtility.WidgetsReposition(_outlinks, Vector3.down, Vector3.zero, 0f, 0.5f);
			point.x = _outlinks.BaseObject.GetComponent<UIWidget>().width;
			point.y = (int)num;
		}
		else
		{
			float num2 = UIUtility.WidgetsReposition(_outlinks, Vector3.right, Vector3.zero, 0f, 0.5f);
			point.x = (int)num2;
			point.y = _outlinks.BaseObject.GetComponent<UIWidget>().height;
		}
		component.SetDimensions(point.x, point.y);
		for (int i = 0; i < _outlinks.Count; i++)
		{
			TitleOutlinkNode component2 = _outlinks[i].GetComponent<TitleOutlinkNode>();
			component2.SetBorder(isPortrait, i == _outlinks.Count - 1);
		}
		_bottomLayout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	public virtual void ShowMessageBox(string title, string explain, Action okAction, Action cancelAction = null, string okButtonLabel = null, string cancelButtonLabel = null)
	{
		_messageBox.Show(title, explain, okAction, cancelAction, okButtonLabel, cancelButtonLabel);
	}

	public virtual void CloseMessageBox()
	{
		_messageBox.Close();
	}

	public void SetContentActive(bool isActive)
	{
		_mainContent.gameObject.SetActive(isActive);
	}

	public void Clear()
	{
		Clusters.Clear();
		_explainLabel.text = string.Empty;
		_nextMaintenanceCheckTime = 0f;
	}

	protected virtual void OnReceiveBackMessage(InputCommandMessage message)
	{
		_mainContent.gameObject.SetActive(value: true);
		_clusterSelection.gameObject.SetActive(value: false);
	}

	protected void OnReceiveSelectCurrentCellMessage(InputCommandMessage message)
	{
		_clusterSelection.ConfirmCluster();
	}
}
