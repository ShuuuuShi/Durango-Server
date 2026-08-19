using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using BestHTTP;
using Durango.Logic.Clusters;
using Durango.Logic.Encyclopedia;
using Durango.Network;
using Durango.System;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using L10N;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yaml.Util;

namespace Durango.UI;

public class TitleMenuGroup : MonoBehaviour
{
	public enum State
	{
		Invalid = -1,
		Initial,
		GetClusterList,
		SelectCluster,
		SelectPlayer,
		Knock,
		CheckDataLoaded,
		CheckSoundManager,
		CheckSpriteManager,
		GetUser,
		NPAGetUser,
		FadeOutPrologue,
		PrologueLoading,
		CheckPrerequsite,
		PostPrerequsite,
		GetAdmission,
		GetFrontend,
		TryConnect,
		Connecting,
		Welcome,
		FadeOutLoading,
		Loading,
		Error,
		IdleInHardcapPosition,
		GetTimedTicketInfo,
		IdleInTimedTicketWaiting
	}

	[Serializable]
	private struct TitleOptions
	{
		public string VideoName;

		public SoundEventType SoundEvent;

		public GameObject[] Objects;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass69_0
	{
		public TitleMenuGroup _003C_003E4__this;

		public string text2;

		internal void _003CCheckUpdate_003Eb__0()
		{
			_003C_003E4__this.StartCoroutine(_003C_003E4__this.DownloadUpdate());
		}

		internal void _003CCheckUpdate_003Eb__1()
		{
			if (bool.Parse(text2))
			{
				Application.Quit();
			}
			else
			{
				_003C_003E4__this.KnockSystem();
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003CCheckUpdate_003Ed__69 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TitleMenuGroup _003C_003E4__this;

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
		public _003CCheckUpdate_003Ed__69(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			TitleMenuGroup titleMenuGroup = _003C_003E4__this;
			if (num != 0)
			{
				return false;
			}
			_003C_003E1__state = -1;
			_003C_003Ec__DisplayClass69_0 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass69_0
			{
				_003C_003E4__this = _003C_003E4__this
			};
			string text = Directory.GetCurrentDirectory() + "\\" + Process.GetCurrentProcess().ProcessName + "_Data\\Programs\\UpdateManager\\DurangoV2_UpdateManager.exe";
			if (!new FileInfo(text).Exists)
			{
				titleMenuGroup.KnockSystem();
				return false;
			}
			string sourceFileName = Directory.GetCurrentDirectory() + "\\" + Process.GetCurrentProcess().ProcessName + "_Data\\Programs\\UpdateManager\\DurangoV2_UpdateManager.exe2";
			if (new FileInfo(text + "2").Exists)
			{
				File.Delete(text);
				File.Move(sourceFileName, text);
				Thread.Sleep(100);
			}
			string text2 = Directory.GetCurrentDirectory() + "\\" + Process.GetCurrentProcess().ProcessName + "_Data\\Programs\\UpdateManager\\DurangoV2_UpdateManager.pdb2";
			if (new FileInfo(text2).Exists)
			{
				File.Delete(text.Replace(".exe", ".pdb"));
				File.Move(text2, text.Replace(".exe", ".pdb"));
				Thread.Sleep(100);
			}
			Process process = new Process();
			process.StartInfo = new ProcessStartInfo
			{
				FileName = text,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden
			};
			process.Start();
			string text3 = process.StandardOutput.ReadToEnd();
			process.WaitForExit();
			if (text3.IndexOf("Error") != -1 || text3.IndexOf("True") == -1)
			{
				titleMenuGroup.KnockSystem();
				return false;
			}
			Action okAction = delegate
			{
				CS_0024_003C_003E8__locals0._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals0._003C_003E4__this.DownloadUpdate());
			};
			WebClient webClient = new WebClient();
			CS_0024_003C_003E8__locals0.text2 = webClient.DownloadString("http://db.kyllox.pe.kr/durango/update/indispensable.txt");
			Action cancelAction = delegate
			{
				if (bool.Parse(CS_0024_003C_003E8__locals0.text2))
				{
					Application.Quit();
				}
				else
				{
					CS_0024_003C_003E8__locals0._003C_003E4__this.KnockSystem();
				}
			};
			if (LocalizeSystem.LocaleLanguage == "ko")
			{
				string title = T._("업데이트 발견");
				string explain = T._("새로운 버전이 발견되었습니다.\n업데이트 하시겠습니까?");
				string okButtonLabel = T._("확인");
				string cancelButtonLabel = T._("취소");
				titleMenuGroup.UserControl.ShowMessageBox(title, explain, okAction, cancelAction, okButtonLabel, cancelButtonLabel);
			}
			else
			{
				string title2 = T._("Found Update");
				string explain2 = T._("Found a new version.\nDo you want to update?");
				string okButtonLabel2 = T._("Update");
				string cancelButtonLabel2 = T._("Cancel");
				titleMenuGroup.UserControl.ShowMessageBox(title2, explain2, okAction, cancelAction, okButtonLabel2, cancelButtonLabel2);
			}
			return false;
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

	[CompilerGenerated]
	private sealed class _003CCoLoadingLevel_003Ed__48 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TitleMenuGroup _003C_003E4__this;

		public string level;

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
		public _003CCoLoadingLevel_003Ed__48(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			TitleMenuGroup titleMenuGroup = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				titleMenuGroup._videoPlayer.Stop();
				_003C_003E2__current = new WaitForEndOfFrame();
				_003C_003E1__state = 1;
				return true;
			case 1:
				_003C_003E1__state = -1;
				titleMenuGroup._videoPlayer.Destroy();
				SceneManager.LoadSceneAsync(level);
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

	[CompilerGenerated]
	private sealed class _003CDownloadUpdate_003Ed__72 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TitleMenuGroup _003C_003E4__this;

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
		public _003CDownloadUpdate_003Ed__72(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			TitleMenuGroup titleMenuGroup = _003C_003E4__this;
			if (num != 0)
			{
				return false;
			}
			_003C_003E1__state = -1;
			Process.Start(Directory.GetCurrentDirectory() + "\\" + Process.GetCurrentProcess().ProcessName + "_Data\\Programs\\UpdateManager\\DurangoV2_UpdateManager.exe", "update");
			if (LocalizeSystem.LocaleLanguage == "ko")
			{
				string title = T._("업데이트 중...");
				string explain = T._("업데이트 다운로드 프로그램을 강제종료하면 게임 파일에 손상이 생길 수 있습니다.\n업데이트 완료 후 게임이 재실행 됩니다. 업데이트 중 프로세스가 응답하지 않을 수 있습니다.");
				Action okAction = titleMenuGroup.UserControl.CloseMessageBox;
				titleMenuGroup.UserControl.ShowMessageBox(title, explain, okAction);
			}
			else
			{
				string title2 = T._("Now Updating...");
				string explain2 = T._("Force termination of the update download program can cause damage to the game file.\nThe game will be re-executed after the update is completed. The process may not respond during the update.");
				Action okAction2 = titleMenuGroup.UserControl.CloseMessageBox;
				titleMenuGroup.UserControl.ShowMessageBox(title2, explain2, okAction2);
			}
			return false;
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

	private const float IdleTimeInHardcap = 15f;

	public Action<bool, State, string> WebResponsed;

	[SerializeField]
	private MediaPlayerCtrl _videoPlayer;

	[SerializeField]
	private UIWidget _videoWidget;

	[EnumList(typeof(GameManager.EmigratedType), false, 0, -1)]
	[SerializeField]
	private List<TitleOptions> _titleList;

	[SerializeField]
	private UIFontSetting _fontSetting;

	[SerializeField]
	private UILabel _debugLogLabel;

	[SerializeField]
	private UILabel _debugWarningLabel;

	[SerializeField]
	private UILabel _debugErrorLabel;

	[SerializeField]
	protected TitleMenuUserControlBase UserControl;

	private string _errorMsg = string.Empty;

	private string _errorMsgDetail = string.Empty;

	private HTTPRequest _request;

	private int _connectAttempt;

	private State _curState = State.Invalid;

	private bool _autoSelectCluster;

	private float _initialStateTime;

	private float _lastHardCapRequestTime;

	private int _lastHardcapPosition = -1;

	private float _lastHardcapDelta = -1f;

	private bool _requestedRetryAdmission;

	private DateTime? _timedTicketInfoUpdatedTimeOnArrival;

	private DateTime _timedTicketClientUtcTimeOnArrival;

	private const float TimedTicketCheckPeriod = 15f;

	private float _timedTicketLastCheckTime;

	private bool _timedTicketInfoRequested;

	private const double TimedTicketInfoUpdateTimeLimit = 180.0;

	private static HashSet<string> _timedTicketSkippableClusters;

	private int _errorFrame;

	private uint _soundInstanceId;

	private TitleLoadingGroup _loadingCurtain;

	private static bool IsLoginProcess
	{
		get
		{
			if (!GameManager.IsPlayerIdSelected)
			{
				return GameManager.Emigrated == GameManager.EmigratedType.None;
			}
			return false;
		}
	}

	private State CurState
	{
		get
		{
			return _curState;
		}
		set
		{
			State curState = _curState;
			_curState = value;
			if (_curState == State.Initial)
			{
				_initialStateTime = Time.realtimeSinceStartup;
			}
			_ = Time.realtimeSinceStartup;
			_ = _initialStateTime;
			State state = _curState;
			State curState2 = _curState;
			if (_curState != State.GetAdmission && _curState != State.GetTimedTicketInfo)
			{
				UserControl.SetExplainLabel((!IsDataLoadState(_curState)) ? string.Empty : ManualTranslator.CheckingGameData);
			}
			UserControl.OnStateChanged(value);
			switch (_curState)
			{
			case State.Initial:
				_lastHardcapPosition = -1;
				_lastHardcapDelta = -1f;
				GameManager.SessionToken = string.Empty;
				if (IsLoginProcess)
				{
					GameManager.PlayerId = string.Empty;
				}
				UserControl.Clear();
				UserControl.IsLoginProcess = IsLoginProcess;
				_autoSelectCluster = false;
				if (string.IsNullOrEmpty(GameManager.LastEvictedMsg))
				{
					state = State.GetClusterList;
					break;
				}
				_errorMsg = GameManager.LastEvictedMsg;
				GameManager.LastEvictedMsg = string.Empty;
				state = State.Error;
				break;
			case State.GetClusterList:
			{
				TextAsset textAsset = Resources.Load("offline/clusters") as TextAsset;
				state = ((!(textAsset != null) || !UserControl.TryUpdateClusters(textAsset.text)) ? State.Error : State.SelectCluster);
				break;
			}
			case State.SelectCluster:
			{
				UserControl.SetExplainLabel(ManualTranslator.TouchTheScreen);
				bool confirmed = false;
				Action action = delegate
				{
					if (!confirmed)
					{
						if (GameManager.PlayerId == null || string.IsNullOrEmpty(GameManager.GatewayUrl))
						{
							CurState = State.Error;
						}
						else
						{
							Cluster selectedCluster = UserControl.GetSelectedCluster();
							if (selectedCluster.OnConfirm != null)
							{
								selectedCluster.OnConfirm(GameManager.PlayerId);
							}
							confirmed = true;
							KUtility.DelayedCall(this, delegate
							{
								if (GameManager.Emigrated != 0)
								{
									int randomMemo = MemoSystem.GetRandomMemo(MemoType.Tooltip);
									string text = ((randomMemo != -1) ? MemoSystem.GetMemoText(MemoType.Tooltip, randomMemo) : string.Empty);
									UserControl.SetExplainLabel(text, important: true);
								}
								CurState = State.Knock;
							}, (selectedCluster.Mode != 0) ? 1f : (-1f));
						}
					}
				};
				Action onPlayerSelection = delegate
				{
					CurState = State.SelectPlayer;
				};
				if (IsLoginProcess)
				{
					UserControl.ShowCluster(action, onPlayerSelection, delegate
					{
						if (CurState == State.SelectCluster)
						{
							Platform.Instance.Logout(delegate(bool success)
							{
								if (success)
								{
									CurState = State.Initial;
								}
								else
								{
									_errorMsg = T._("로그아웃에 실패했습니다.");
									CurState = State.Error;
								}
							});
						}
					}, _autoSelectCluster);
				}
				else
				{
					action();
				}
				break;
			}
			case State.SelectPlayer:
			{
				Cluster cluster = UserControl.GetSelectedCluster();
				Account account = UserControl.GetSelectedAccount();
				if (account == null)
				{
					state = State.Error;
					break;
				}
				int maxPlayerSlotCount = account.MaxPlayerSlotCount;
				int playerSlotCount = account.PlayerSlotCount;
				TitlePlayerSelectionGroupBase titlePlayerSelectionGroupBase = TitleUIManager.Find<TitlePlayerSelectionGroupBase>();
				Action<PlayerInfo> deleteClicked = null;
				if (cluster.Mode == Mode.Editable)
				{
					deleteClicked = delegate(PlayerInfo info)
					{
						UserControl.ShowMessageBox("<em>" + info.PlayerName + "</em> " + T._("캐릭터를 삭제하시겠습니까?"), T._("해당 캐릭터에 속한 창작섬과 건축물, 아이템이 모두 삭제되며, 복구할 수 없습니다.\n\n정말 삭제하시겠습니까?"), delegate
						{
							cluster.OnDeletePlayer(info.PlayerEntityId);
							UserControl.CloseMessageBox();
							UserControl.UpdateServerAndPlayerInfo(forceUpdate: true);
							CurState = State.SelectPlayer;
						}, delegate
						{
							UserControl.CloseMessageBox();
						});
					};
				}
				titlePlayerSelectionGroupBase.Show(account, cluster.GetName(LocalizeSystem.Locale), playerSlotCount, maxPlayerSlotCount, delegate(string id, int slotIdx)
				{
					account.ApplyRecommendedPlayer(new Pair<string, int>(id, slotIdx));
					UserControl.SetContentActive(isActive: true);
					_autoSelectCluster = false;
					CurState = State.SelectCluster;
				}, delegate(int slotIdx)
				{
					account.ApplyRecommendedPlayer(new Pair<string, int>(string.Empty, slotIdx));
					UserControl.SetContentActive(isActive: true);
					_autoSelectCluster = true;
					CurState = State.SelectCluster;
				}, deleteClicked);
				titlePlayerSelectionGroupBase.SetBackButtonEvent(delegate
				{
					UserControl.SetContentActive(isActive: true);
					_autoSelectCluster = false;
					CurState = State.SelectCluster;
				});
				UserControl.SetContentActive(isActive: false);
				break;
			}
			case State.Knock:
				StartCoroutine(CheckUpdate());
				break;
			case State.CheckDataLoaded:
				if (Loader.LoadState != Loader.State.Succees)
				{
					Loader.Load(this);
				}
				break;
			case State.CheckSoundManager:
				Durango.Utils.Singleton<SoundManager>.Instance().Initialize();
				ResourceSingleton<UISpriteManager>.Instance().Load();
				break;
			case State.NPAGetUser:
			{
				Dictionary<string, string> dictionary = Platform.Instance.BuildSessionForm();
				if (GameManager.ConnectCluster != null)
				{
					dictionary.Add("player", GameManager.ConnectCluster.LocalPlayer);
				}
				RequestUrl("/sessions", dictionary, auth: false, HTTPMethods.Post);
				break;
			}
			case State.FadeOutPrologue:
				ActiveFadeOutTweener();
				break;
			case State.PrologueLoading:
				OrientationController.SetOrientation(OrientationController.Orientation.Landscape);
				StartCoroutine(CoLoadingLevel("Prologue"));
				break;
			case State.CheckPrerequsite:
				UserControl.SetExplainLabel(T._("필수 파일을 확인 중입니다."));
				CheckPrerequsite();
				break;
			case State.PostPrerequsite:
				UserControl.SetExplainLabel(T._("필수 파일을 불러오는 중입니다."));
				Durango.Utils.Singleton<AssetBundleManager>.Instance().PrecacheAssets();
				break;
			case State.GetAdmission:
				_timedTicketSkippableClusters.Add(UserControl.GetSelectedClusterKey());
				RequestUrl("/admission", null, auth: true, HTTPMethods.Get, _requestedRetryAdmission);
				break;
			case State.GetFrontend:
				RquestEntry(string.Empty);
				break;
			case State.TryConnect:
				UserControl.SetExplainLabel(T._("서버에 접속 중입니다."));
				state = OnTryConnect();
				break;
			case State.Welcome:
				UserControl.SetExplainLabel(T._("서버와 통신 중입니다."));
				Durango.Utils.Singleton<GameManager>.Instance().SendAuthMessage(delegate
				{
					CurState = State.FadeOutLoading;
				}, delegate(string error)
				{
					Connections.Frontend.Close(callClosedHandler: false);
					_errorMsg = error;
					CurState = State.Error;
				});
				break;
			case State.FadeOutLoading:
				ActiveFadeOutTweener();
				break;
			case State.Loading:
				StartCoroutine(CoLoadingLevel(Platform.Instance.MainSceneName));
				break;
			case State.Error:
				OnErrorState(curState);
				if (!string.IsNullOrEmpty(_errorMsgDetail))
				{
					UserControl.ShowMessageBox(T._("접속 불가"), _errorMsgDetail, delegate
					{
						if (UserControl.QuitWhenErrorOccurred)
						{
							Platform.Instance.Quit();
						}
						else
						{
							UserControl.CloseMessageBox();
						}
					});
					_errorMsgDetail = string.Empty;
				}
				_errorMsg = string.Empty;
				break;
			case State.IdleInHardcapPosition:
				OnErrorState(curState);
				_requestedRetryAdmission = false;
				break;
			case State.GetTimedTicketInfo:
			{
				string timedTicketUrl = UserControl.GetSelectedCluster().TimedTicketUrl;
				bool timedTicketInfoRequested = _timedTicketInfoRequested;
				RequestHttpUrl(timedTicketUrl, null, auth: false, HTTPMethods.Get, timedTicketInfoRequested);
				break;
			}
			case State.IdleInTimedTicketWaiting:
				OnErrorState(curState);
				_timedTicketInfoRequested = false;
				break;
			}
			if (state != curState2)
			{
				CurState = state;
			}
		}
	}

	private TitleLoadingGroup LoadingGroup
	{
		get
		{
			if (_loadingCurtain == null)
			{
				_loadingCurtain = TitleUIManager.Find<TitleLoadingGroup>();
			}
			return _loadingCurtain;
		}
	}

	private static string GetLastErrorMsg(State prevState)
	{
		string arg = T._("게임에 접속할 수 없습니다.\n네트워크 상태를 확인 후 다시 시도해 주세요.");
		if (IsDataLoadState(prevState))
		{
			arg = ManualTranslator.DataLoadErrorAndRetry;
		}
		else if (prevState == State.Connecting)
		{
			string text = T._("서버와의 연결 중 에러가 발생하였습니다.");
			string text2 = T._("화면을 터치하여 다시 시도해 주세요.");
			arg = ((!Platform.Instance.UsePCUI) ? (text + "\n" + text2) : text);
		}
		return $"{arg} ({prevState})";
	}

	private static bool IsDataLoadState(State state)
	{
		if (state != State.CheckDataLoaded && state != State.CheckSoundManager)
		{
			return state == State.CheckSpriteManager;
		}
		return true;
	}

	private State OnTryConnect()
	{
		if (_connectAttempt >= 3)
		{
			return State.Error;
		}
		Durango.Utils.Singleton<GameManager>.Instance().TryConnect();
		_connectAttempt++;
		return State.Connecting;
	}

	public void StartGame()
	{
		base.gameObject.SetActive(value: true);
		SoundManager.SetSfxVolume(SoundManager.VolumeForSfx);
		SoundManager.SetAmbienceVolume(SoundManager.VolumeForAmbience);
		SoundManager.SetMidiVolume(SoundManager.VolumeForMidi);
		SoundManager.SetBgmVolume(SoundManager.VolumeForBgm);
		TitleUIRootResizer.AddOnScreenResized(OnScreenResized);
		if (GameManager.IsPlayerIdSelected)
		{
			LoadingGroup.HideTitleSceneWithCurtain();
		}
		else
		{
			ApplyEmigrationMode();
		}
		if (_fontSetting != null)
		{
			_fontSetting.Init();
		}
		float delay = -1f;
		KUtility.DelayedCall(this, delegate
		{
			CurState = State.Initial;
		}, delay);
	}

	private void ApplyEmigrationMode()
	{
		if (_titleList == null)
		{
			return;
		}
		foreach (TitleOptions title in _titleList)
		{
			GameObject[] objects = title.Objects;
			if (objects != null)
			{
				GameObject[] array = objects;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: false);
				}
			}
		}
		int emigrated = (int)GameManager.Emigrated;
		if (_titleList.Count <= emigrated)
		{
			return;
		}
		TitleOptions titleOptions = _titleList[emigrated];
		if (titleOptions.Objects != null)
		{
			GameObject[] objects2 = titleOptions.Objects;
			for (int j = 0; j < objects2.Length; j++)
			{
				objects2[j].SetActive(value: true);
			}
		}
		_videoPlayer.Load(titleOptions.VideoName);
		AkAudioListener akAudioListener = UnityEngine.Object.FindObjectOfType<AkAudioListener>();
		if (akAudioListener != null)
		{
			SoundManager.SetListenerObject(akAudioListener.gameObject);
		}
		if (!string.IsNullOrEmpty(titleOptions.SoundEvent.Path))
		{
			SoundManager.IgnorePreparedCheck = true;
			_soundInstanceId = SoundManager.PlayEvent(titleOptions.SoundEvent, SoundPosition.Empty, exclusive: true);
			SoundManager.IgnorePreparedCheck = false;
		}
	}

	private void CheckPrerequsite()
	{
		PrerequisiteLoader loader = LoadingGroup.PrerequisiteLoader;
		LoadingGroup.gameObject.SetActive(value: true);
		loader.TotalCount = Durango.Utils.Singleton<AssetBundleManager>.Instance().PrerequsitesCount;
		Durango.Utils.Singleton<AssetBundleManager>.Instance().StartPrerequisiteLoading(loader.ProgressChanged, loader.DetailedProgressChanged, delegate(bool isSuccessed)
		{
			loader.gameObject.SetActive(value: false);
			if (isSuccessed)
			{
				CurState = State.PostPrerequsite;
			}
			else
			{
				CurState = State.Error;
			}
		}, delegate(int mega, int remainCount)
		{
			loader.TotalCount = remainCount;
			if (remainCount > 0)
			{
				UserControl.SetExplainLabel(GetPrerequsiteDownloadWarningMessage(mega), important: true);
			}
		});
	}

	private void ActiveFadeOutTweener()
	{
		SoundManager.StopEvent(_soundInstanceId, LoadingGroup.Duration);
		_soundInstanceId = 0u;
		if (GameManager.IsPlayerIdSelected)
		{
			FadeOutFinished();
		}
		else
		{
			LoadingGroup.Play(FadeOutFinished);
		}
	}

	private void FadeOutFinished()
	{
		GameManager.IsPlayerIdSelected = false;
		switch (CurState)
		{
		case State.FadeOutLoading:
			CurState = State.Loading;
			break;
		case State.FadeOutPrologue:
			CurState = State.PrologueLoading;
			break;
		}
	}

	private IEnumerator CoLoadingLevel(string level)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoLoadingLevel_003Ed__48(0)
		{
			_003C_003E4__this = this,
			level = level
		};
	}

	private void Update()
	{
		ProcessState();
		ProcessResponse();
	}

	private void ProcessState()
	{
		switch (CurState)
		{
		case State.Connecting:
			if (Connections.Frontend.Connected())
			{
				CurState = State.Welcome;
			}
			else if (!Connections.Frontend.IsAttemptingToConnect())
			{
				CurState = State.TryConnect;
			}
			break;
		case State.Welcome:
			if (!Connections.Frontend.Connected())
			{
				CurState = State.Error;
			}
			break;
		case State.CheckDataLoaded:
			switch (Durango.Utils.Singleton<AssetBundleManager>.Instance().CurrentStatus)
			{
			case AssetBundleManager.Status.Ready:
				if (Loader.LoadState == Loader.State.Succees)
				{
					CurState = State.CheckSoundManager;
				}
				else if (Loader.LoadState == Loader.State.Failure)
				{
					Loader.Stop();
					CurState = State.Error;
				}
				break;
			case AssetBundleManager.Status.Failed:
				CurState = State.Error;
				break;
			}
			break;
		case State.CheckSoundManager:
			switch (Durango.Utils.Singleton<SoundManager>.Instance().BankLoadState)
			{
			case SoundBanksLoader.State.LoadFailed:
				CurState = State.Error;
				break;
			case SoundBanksLoader.State.Loaded:
				CurState = State.CheckSpriteManager;
				break;
			}
			break;
		case State.CheckSpriteManager:
			switch (ResourceSingleton<UISpriteManager>.Instance().LoadingStatus)
			{
			case UISpriteManager.Status.Failed:
				CurState = State.Error;
				break;
			case UISpriteManager.Status.Ready:
				CurState = State.NPAGetUser;
				break;
			}
			break;
		case State.PostPrerequsite:
			if (Durango.Utils.Singleton<AssetBundleManager>.Instance().IsPrecachedAssetsReady())
			{
				if (string.IsNullOrEmpty(UserControl.GetSelectedCluster().TimedTicketUrl) || _timedTicketSkippableClusters.Contains(UserControl.GetSelectedClusterKey()))
				{
					CurState = State.GetAdmission;
					break;
				}
				_timedTicketInfoUpdatedTimeOnArrival = null;
				CurState = State.GetTimedTicketInfo;
			}
			break;
		case State.Error:
		{
			bool flag = ((!Platform.Instance.UsePCUI) ? Input.GetMouseButtonDown(0) : UserControl.RetryConnect);
			UserControl.RetryConnect = false;
			if (!UserControl.IsMessageBoxOpen && flag && _errorFrame != Time.frameCount)
			{
				if (UserControl.QuitWhenErrorOccurred)
				{
					Platform.Instance.Quit();
					break;
				}
				CurState = State.Initial;
				_debugLogLabel.text = string.Empty;
				_debugWarningLabel.text = string.Empty;
				_debugErrorLabel.text = string.Empty;
			}
			break;
		}
		case State.IdleInHardcapPosition:
			if (Time.realtimeSinceStartup - _lastHardCapRequestTime >= 15f)
			{
				_requestedRetryAdmission = true;
				CurState = State.GetAdmission;
			}
			break;
		case State.IdleInTimedTicketWaiting:
			if (Time.realtimeSinceStartup - _timedTicketLastCheckTime >= 15f)
			{
				_timedTicketInfoRequested = true;
				CurState = State.GetTimedTicketInfo;
			}
			break;
		}
	}

	private void ProcessResponse()
	{
		if (_request == null || _request.MoveNext())
		{
			return;
		}
		if (_request.Response != null && _request.Response.IsSuccess)
		{
			string dataAsText = _request.Response.DataAsText;
			_request = null;
			if (WebResponsed != null)
			{
				WebResponsed(arg1: true, CurState, dataAsText);
			}
			OnRequestSucceed(dataAsText);
		}
		else
		{
			CheckError(_request.Response);
			_request = null;
			if (WebResponsed != null)
			{
				WebResponsed(arg1: false, CurState, string.Empty);
			}
			CurState = State.Error;
		}
	}

	private void OnRequestSucceed(string response)
	{
		JObject jObject = Json.Read<JObject>(response);
		if (jObject == null)
		{
			CurState = State.Error;
			return;
		}
		switch (CurState)
		{
		case State.GetTimedTicketInfo:
		case State.IdleInTimedTicketWaiting:
		{
			if (!jObject.Get("activated", defaultVal: false))
			{
				CurState = State.GetAdmission;
				break;
			}
			double result = 0.0;
			double.TryParse(JTokenExtensions.Get(jObject, "enterable_before", "0"), out result);
			DateTime dateTime = Times.UnixTimeToDateTimeUtc(result);
			double result2 = 0.0;
			double.TryParse(JTokenExtensions.Get(jObject, "info_updated_at", "0"), out result2);
			DateTime dateTime2 = Times.UnixTimeToDateTimeUtc(result2);
			if (!_timedTicketInfoUpdatedTimeOnArrival.HasValue)
			{
				_timedTicketInfoUpdatedTimeOnArrival = dateTime2;
				_timedTicketClientUtcTimeOnArrival = DateTime.UtcNow;
			}
			bool flag = _timedTicketInfoUpdatedTimeOnArrival.Value <= dateTime;
			TimeSpan value2 = DateTime.UtcNow - _timedTicketClientUtcTimeOnArrival;
			if ((_timedTicketInfoUpdatedTimeOnArrival.Value.Add(value2) - dateTime2).TotalSeconds > 180.0)
			{
				flag = true;
			}
			if (flag)
			{
				_timedTicketSkippableClusters.Add(UserControl.GetSelectedClusterKey());
				CurState = State.NPAGetUser;
				break;
			}
			_timedTicketLastCheckTime = Time.realtimeSinceStartup;
			double num3 = (_timedTicketInfoUpdatedTimeOnArrival.Value - dateTime).TotalSeconds;
			if (num3 < 0.0)
			{
				num3 = 0.0;
			}
			float result3 = 0f;
			float.TryParse(JTokenExtensions.Get(jObject, "estimated_position_per_seconds", "0"), out result3);
			float result4 = 0f;
			float.TryParse(JTokenExtensions.Get(jObject, "estimated_duration_per_seconds", "0"), out result4);
			int position = (int)((double)result3 * num3);
			float estimatedWaitingTime = (float)((double)result4 * num3);
			_errorMsg = MakeTimedTicketMessage(position, estimatedWaitingTime);
			CurState = State.IdleInTimedTicketWaiting;
			break;
		}
		case State.Knock:
		{
			UserControl.UpdateVersionInfo(jObject.Get<string>("server_version"));
			if (!jObject.Get("compatible", defaultVal: false))
			{
				RedirectToDownloadUrl(jObject.Get<string>("download_url"));
				break;
			}
			string urlRoot = jObject.Get<string>("assetbundle_url_root");
			string infoHolderPath = jObject.Get<string>("assetbundle_index_url");
			Durango.Utils.Singleton<AssetBundleManager>.Instance().Initialize(infoHolderPath, urlRoot);
			CurState = State.CheckDataLoaded;
			break;
		}
		case State.NPAGetUser:
		{
			string text5 = jObject.Get<string>("session_token");
			if (string.IsNullOrEmpty(text5))
			{
				CurState = State.Error;
				break;
			}
			GameManager.SessionToken = text5;
			CurState = ((!string.IsNullOrEmpty(GameManager.PlayerId)) ? State.PostPrerequsite : State.FadeOutPrologue);
			break;
		}
		case State.GetFrontend:
		{
			string text3 = jObject.Get<string>("dispatch_to");
			if (text3 != null)
			{
				Uri uri = new Uri(text3);
				string gatewayUrl = uri.Scheme + "://" + uri.Authority;
				RquestEntry(gatewayUrl);
				break;
			}
			List<KeyValuePair<string, int>> list = ParseAddresses(jObject.Get("frontend_addresses") as JArray);
			if (KUtility.GetSize(list) == 0)
			{
				CurState = State.Error;
				break;
			}
			if (GameManager.ConnectCluster != null)
			{
				string text4 = GameManager.ConnectCluster.GatewayUrlRoot;
				if (text4.StartsWith("http://"))
				{
					text4 = text4.Substring(7);
				}
				int num2 = text4.LastIndexOf(":");
				if (num2 != -1)
				{
					text4 = text4.Substring(0, num2);
				}
				int value = list[0].Value;
				list[0] = new KeyValuePair<string, int>(text4, value);
				string source = jObject.Get<string>("cluster_mode");
				GameManager.SetCluster(GameManager.ClusterKey, GameManager.GatewayUrl, source.ToEnum(Mode.Offline));
				GameManager.ConnectCluster = null;
			}
			List<KeyValuePair<string, int>> endpoints = ParseAddresses(jObject.Get("radiotower_addresses") as JArray);
			Durango.Utils.Singleton<GameManager>.Instance().SetEndpoints(list);
			GameSystem<SocialSystem>.Instance().SetEndpoints(endpoints);
			_connectAttempt = 0;
			CurState = State.TryConnect;
			break;
		}
		case State.GetAdmission:
		case State.IdleInHardcapPosition:
		{
			if (jObject.Get("admitted", defaultVal: false))
			{
				CurState = State.GetFrontend;
				break;
			}
			int num = JTokenExtensions.Get(jObject, "position", -1);
			if (num >= 0)
			{
				float realtimeSinceStartup = Time.realtimeSinceStartup;
				float duration = realtimeSinceStartup - _lastHardCapRequestTime;
				Cluster selectedCluster = UserControl.GetSelectedCluster();
				_errorMsg = ((!string.IsNullOrEmpty(selectedCluster.HardCap)) ? selectedCluster.HardCap : MakeHardcapMessage(num, _lastHardcapPosition, duration));
				_lastHardcapPosition = num;
				_lastHardCapRequestTime = realtimeSinceStartup;
				CurState = State.IdleInHardcapPosition;
			}
			else
			{
				string text = T._("현재 게임 서버가 혼잡하여 접속이 불가능합니다.\n잠시 후 다시 이용해 주시길 바랍니다.");
				string text2 = T._("확인 버튼을 터치하면 게임이 종료됩니다.");
				string errorMsgDetail = ((!Platform.Instance.UsePCUI) ? (text + "\n" + text2) : text);
				_errorMsgDetail = errorMsgDetail;
				UserControl.QuitWhenErrorOccurred = true;
				CurState = State.Error;
			}
			break;
		}
		}
	}

	private static List<KeyValuePair<string, int>> ParseAddresses(JArray addresses)
	{
		if (addresses == null || addresses.Count == 0)
		{
			return null;
		}
		List<KeyValuePair<string, int>> list = new List<KeyValuePair<string, int>>();
		foreach (JToken item in (IEnumerable<JToken>)addresses)
		{
			if (Connection.TryParse(item.GetString(), out var host, out var port))
			{
				list.Add(new KeyValuePair<string, int>(host, port));
			}
		}
		return list;
	}

	private string MakeTimedTicketMessage(int position, float estimatedWaitingTime)
	{
		string text = T._("현재 {0}명이 접속 대기 중입니다.", position);
		string text2 = ((estimatedWaitingTime <= 3600f) ? TimedeltaFormatter.Format(estimatedWaitingTime) : T._("{0} 이상", TimedeltaFormatter.Format(3600.0)));
		return text + T._("\n예상 대기 시간은 {0} 입니다.", text2);
	}

	private string MakeHardcapMessage(int position, int lastPosition, float duration)
	{
		string text = T._("현재 {0}명이 접속 대기 중입니다.", position);
		if (position >= lastPosition && _lastHardcapDelta <= 0f)
		{
			return text;
		}
		float num = _lastHardcapDelta;
		int num2 = lastPosition - position;
		if (num2 > 0)
		{
			num = ((duration <= 0f) ? 15f : duration) / (float)num2;
		}
		if (_lastHardcapDelta > 0f)
		{
			num = (num + _lastHardcapDelta) * 0.5f;
		}
		_lastHardcapDelta = num;
		float num3 = num * (float)position;
		string text2 = ((num3 <= 3600f) ? TimedeltaFormatter.Format(num3) : T._("{0} 이상", TimedeltaFormatter.Format(3600.0)));
		return text + T._("\n예상 대기 시간은 {0} 입니다.", text2);
	}

	private void CheckError(HTTPResponse response)
	{
		if (response != null)
		{
			_ = response.Message;
			JObject jObject = Json.Read<JObject>(response.DataAsText);
			JObject jObject2 = ((jObject == null) ? null : (jObject.Get("error") as JObject));
			if (jObject2 != null)
			{
				_errorMsgDetail = jObject2.Get<string>("message");
			}
			else
			{
				_errorMsgDetail = string.Format("[{0}] {1}\n{2}", response.StatusCode, response.Message, (!Debug.isDebugBuild) ? T._("로그인에 실패했습니다. 잠시 후 다시 시도해 주세요.") : response.DataAsText);
			}
		}
	}

	private void RequestHttpUrl(string url, Dictionary<string, string> fields = null, bool auth = false, HTTPMethods method = HTTPMethods.Get, bool skipExplainLabel = false)
	{
		_request = Http.Request(url, null, disableCache: true, auth, fields, method);
		if (!skipExplainLabel)
		{
			UserControl.SetExplainLabel(T._("서버와 통신 중입니다."));
		}
	}

	private void RequestUrl(string postFix, Dictionary<string, string> fields = null, bool auth = false, HTTPMethods method = HTTPMethods.Get, bool skipExplainLabel = false)
	{
		string url = GameManager.GatewayUrl + postFix;
		RequestHttpUrl(url, fields, auth, method, skipExplainLabel);
	}

	private void RquestEntry(string gatewayUrl = "")
	{
		if (string.IsNullOrEmpty(gatewayUrl))
		{
			gatewayUrl = GameManager.GatewayUrl;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(gatewayUrl);
		stringBuilder.Append("/entry");
		stringBuilder.Append("?entity_id=");
		stringBuilder.Append(WWW.EscapeURL(GameManager.PlayerId));
		stringBuilder.Append("&platform=");
		stringBuilder.Append(WWW.EscapeURL(Platform.Instance.AssetBundlePlatform.ToString()));
		RequestHttpUrl(stringBuilder.ToString(), null, auth: true);
	}

	[Conditional("DEBUG_LEVEL_LOG")]
	private void Log(string text)
	{
		_debugLogLabel.text = text;
	}

	[Conditional("DEBUG_LEVEL_LOG")]
	[Conditional("DEBUG_LEVEL_WARN")]
	private void LogWarning(string text)
	{
		_debugWarningLabel.text = text;
	}

	private void LogError(string text)
	{
		if (Debug.isDebugBuild)
		{
			_debugErrorLabel.text = text;
		}
		Debug.LogError(text);
	}

	public static string GetPrerequsiteDownloadWarningMessage(int mega)
	{
		if (Platform.Instance.UsePCUI)
		{
			return T._("게임 플레이를 위해 추가 다운로드가 필요합니다. ({0}MB)", mega);
		}
		return T._("게임 플레이를 위해 추가 다운로드가 필요합니다.\nWi-Fi 사용을 권장합니다. ({0}MB)", mega);
	}

	private void OnErrorState(State prevState)
	{
		if (GameManager.IsPlayerIdSelected)
		{
			ApplyEmigrationMode();
		}
		GameManager.IsPlayerIdSelected = false;
		GameManager.Emigrated = GameManager.EmigratedType.None;
		GameManager.ConnectCluster = null;
		LoadingGroup.LoadingCurtain.SetActive(value: false);
		if (string.IsNullOrEmpty(_errorMsg))
		{
			_errorMsg = GetLastErrorMsg(prevState);
		}
		UserControl.SetExplainLabel(_errorMsg, important: true);
		_errorFrame = Time.frameCount;
	}

	private void OnScreenResized()
	{
		UpdateVideoLayout(TitleUIRootResizer.IsPortrait);
	}

	private void UpdateVideoLayout(bool isPortrait)
	{
		Platform.Instance.GetScreenResolution(isPortrait, out var width, out var height);
		float num = (float)width / (float)height;
		float num2 = (float)Screen.width / (float)Screen.height;
		if (isPortrait)
		{
			num = 1f / num;
			num2 = 1f / num2;
		}
		if (isPortrait)
		{
			float num3 = 1f;
			if (num < num2)
			{
				num3 = num2 / num;
			}
			_videoWidget.width = (int)((float)height * num * num3);
			_videoWidget.height = (int)((float)height * num3);
		}
		else
		{
			float num4 = 1f;
			if (num < num2)
			{
				num4 = num2 / num;
			}
			else if (num > num2)
			{
				num4 = num / num2;
			}
			_videoWidget.width = (int)((float)width * num4);
			_videoWidget.height = (int)((float)height * num4);
		}
		if (GameManager.Emigrated == GameManager.EmigratedType.None && isPortrait)
		{
			float x = (float)(_videoWidget.width - TitleUIRootResizer.ScreenWidth) * 0.5f;
			_videoWidget.transform.localPosition = new Vector3(x, 0f, 0f);
		}
		else
		{
			_videoWidget.transform.localPosition = Vector3.zero;
		}
	}

	protected virtual void RedirectToDownloadUrl(string downloadUrl)
	{
		if (downloadUrl != null)
		{
			UserControl.ShowMessageBox(T._("업데이트"), T._("새 버전 업데이트를 위해 다운로드 페이지로 이동합니다."), delegate
			{
				Application.OpenURL(downloadUrl);
			});
		}
		else
		{
			LogError("No download url");
			CurState = State.Error;
		}
	}

	static TitleMenuGroup()
	{
		_timedTicketSkippableClusters = new HashSet<string>();
	}

	private IEnumerator CheckUpdate()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCheckUpdate_003Ed__69(0)
		{
			_003C_003E4__this = this
		};
	}

	private void KnockSystem()
	{
		UserControl.CloseMessageBox();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("/knock");
		stringBuilder.Append("?version=");
		stringBuilder.Append(WWW.EscapeURL(CurrentBundleVersion.GetClientVersion()));
		stringBuilder.Append("&platform=");
		stringBuilder.Append(WWW.EscapeURL(Platform.Instance.AssetBundlePlatform.ToString()));
		stringBuilder.Append("&bundle_id=");
		stringBuilder.Append(WWW.EscapeURL(Platform.Instance.AppBundleId));
		RequestUrl(stringBuilder.ToString());
	}

	private void OpenUpdateUrl()
	{
		UserControl.CloseMessageBox();
		Application.OpenURL("https://github.com/KylloxStudio/Durango_V2/releases");
		Application.Quit();
	}

	private IEnumerator DownloadUpdate()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CDownloadUpdate_003Ed__72(0)
		{
			_003C_003E4__this = this
		};
	}

	private void AgreeAutoUpdate()
	{
	}
}
