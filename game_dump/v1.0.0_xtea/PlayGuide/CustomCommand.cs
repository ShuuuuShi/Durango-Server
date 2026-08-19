using System;
using System.Collections;
using System.Collections.Generic;
using Messages;
using Shared.Faction;
using UnityEngine;

namespace PlayGuide;

public class CustomCommand
{
	private readonly Dictionary<string, Action> _registeredEvents = new Dictionary<string, Action>();

	private readonly Dictionary<string, Action<Dictionary<string, string>>> _registeredEventsWithParams = new Dictionary<string, Action<Dictionary<string, string>>>();

	private readonly PlayGuideSystem _system;

	private float _defaultRotateSpeed = 540f;

	private IEnumerator _delayedFaceToKCoroutine;

	private NpcAI_KBike _npcAiK;

	private NpcAIDog _npcAiDog;

	private DogGuideState _lastDogGuideState;

	private Vector2 _dogPOIOnLoaded;

	private bool _isNpcDogSpawned;

	public event Action Ready;

	public event Action GuideOfKBegin;

	public event Action GuideOfKEnd;

	public CustomCommand(PlayGuideSystem system)
	{
		_system = system;
		RegisterEventCommands();
	}

	private void RegisterEventCommands()
	{
		RegisterEventCommand("Event_BeginMMO", Event_BeginMMO);
		RegisterEventCommand("Event_Appear_K", Event_Appear_K);
		RegisterEventCommand("Event_BeginCPR", Event_BeginCPR);
		RegisterEventCommand("Event_Show_BottomMenu", Event_Show_BottomMenu);
		RegisterEventCommand("Event_Disappear_K", Event_Disappear_K);
		RegisterEventCommand("Dog_NormalMode", Dog_NormalMode);
		RegisterEventCommand("Dog_Introduce", Dog_Introduce);
		RegisterEventCommand("Dog_Happy", Dog_Happy);
		RegisterEventCommand("Event_UnLockPlayerMove", Event_UnLockPlayerMove);
		RegisterEventCommand("Event_ShowOtherPlayer", Event_ShowOtherPlayer);
		RegisterEventCommand("Ancora_Event_Init_Health", Ancora_Event_Init_Health);
		RegisterEventCommand("Ancora_Event_Resurrect", Ancora_Event_Resurrect);
		RegisterEventCommand("Ancora_Event_Restore_Food_K", Ancora_Event_Restore_Food_K);
		RegisterEventCommand("Ancora_Event_Tired", Ancora_Event_Tired);
		RegisterEventCommand("Event_Restore_Standing_K_CutScene", Event_Restore_Standing_K_CutScene);
		RegisterEventCommand("Dog_SetPOI_Tile", Dog_SetPOI_Tile);
		RegisterEventCommand("Dog_Set_Farewell_Tile", Dog_Set_Farewell_Tile);
		RegisterEventCommand("Guide_Progress", Guide_Progress);
		RegisterEventCommand("Activate_Faction", Activate_Faction);
	}

	public void ClearAll()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_npcAiDog != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_npcAiDog).gameObject);
		}
		if ((Object)(object)_npcAiK != (Object)null)
		{
			Object.Destroy((Object)(object)((Component)_npcAiK).gameObject);
		}
		_npcAiK = null;
		_npcAiDog = null;
		_lastDogGuideState = DogGuideState.Intro;
		_dogPOIOnLoaded = Vector2.zero;
		_isNpcDogSpawned = false;
	}

	public void DispatchCustomCmd(string customCmd)
	{
		if (string.IsNullOrEmpty(customCmd))
		{
			return;
		}
		string[] array = customCmd.Split(new char[1] { ';' }, StringSplitOptions.RemoveEmptyEntries);
		int num = array.Length;
		for (int i = 0; i < num; i++)
		{
			string cmdString = array[i].Trim();
			ExtractParameters(cmdString, out var cmd, out var parameters);
			if (!string.IsNullOrEmpty(cmd))
			{
				if (parameters != null && parameters.Count > 0)
				{
					ExecuteCustomCmd(cmd, parameters);
				}
				else
				{
					ExecuteCustomCmd(cmd);
				}
			}
		}
	}

	public void LoadDogGuideProgress(PlayGuideSystem.GuideStorageData guideStorageData)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_lastDogGuideState = guideStorageData.LastDogGuideState;
		_dogPOIOnLoaded = guideStorageData.LastDogPOITile;
	}

	public void SaveDogGuideProgress(PlayGuideSystem.GuideStorageData guideStorageData)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		guideStorageData.LastDogGuideState = _lastDogGuideState;
		guideStorageData.LastDogPOITile = ((!((Object)(object)_npcAiDog == (Object)null)) ? _npcAiDog.GetPOIPosTile() : Vector2.zero);
	}

	public void ReservedSpawnDogAfterMainSceneLoaded()
	{
		switch (_lastDogGuideState)
		{
		case DogGuideState.Intro:
		case DogGuideState.AfterCpr:
			KSingleton<BGMManager>.Instance().AllowlslandBGM(allow: false);
			break;
		case DogGuideState.Normal:
		{
			CallDogCommand(delegate
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				_npcAiDog.SetPOIPosTile(_dogPOIOnLoaded);
			});
			LoadingCurtainGroup loadingCurtainGroup = UIManager.FindScript<LoadingCurtainGroup>();
			if ((Object)(object)loadingCurtainGroup != (Object)null && loadingCurtainGroup.IsVisible)
			{
				EventDelegate.Add(loadingCurtainGroup.FadeOutFinished, LetDogMoveCloseToPlayer, oneShot: true);
			}
			break;
		}
		case DogGuideState.Finished:
			break;
		}
	}

	private void LetDogMoveCloseToPlayer()
	{
		if ((Object)(object)_npcAiDog != (Object)null)
		{
			_npcAiDog.MoveCloseToPlayer();
		}
	}

	private void RegisterEventCommand(string cmdName, Action<Dictionary<string, string>> function)
	{
		_registeredEventsWithParams[cmdName] = function;
	}

	private void RegisterEventCommand(string cmdName, Action function)
	{
		_registeredEvents[cmdName] = function;
	}

	private void ExecuteCustomCmd(string cmdName, Dictionary<string, string> parameters)
	{
		if (_registeredEventsWithParams.TryGetValue(cmdName, out var value))
		{
			value(parameters);
		}
	}

	private void ExecuteCustomCmd(string cmdName)
	{
		if (_registeredEvents.TryGetValue(cmdName, out var value))
		{
			value();
		}
	}

	public static void ExtractParameters(string cmdString, out string cmd, out Dictionary<string, string> parameters)
	{
		cmd = cmdString;
		parameters = null;
		if (!cmdString.Contains("("))
		{
			return;
		}
		string[] array = cmdString.Split(new string[4] { "(", ")", ",", " " }, StringSplitOptions.RemoveEmptyEntries);
		if (array.Length >= 1)
		{
			cmd = array[0];
		}
		if (array.Length < 2)
		{
			return;
		}
		parameters = new Dictionary<string, string>();
		for (int i = 1; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new string[2] { ":", " " }, StringSplitOptions.RemoveEmptyEntries);
			if (array2.Length == 2)
			{
				parameters.Add(array2[0], array2[1]);
			}
		}
	}

	private void Event_BeginMMO()
	{
		KSingleton<PlayerController>.Instance().MoveLock = true;
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		if ((Object)(object)localPlayer != (Object)null)
		{
			localPlayer.TurnToYaw(0f, bSnap: true);
			localPlayer.IsProhibitAnimRefresh = true;
			localPlayer.PlayAnimation("Bike_Begin", 0f, 1f, forceTransition: true);
			localPlayer.IsOutlineEnabled = false;
			localPlayer.LookAtController.Activated = false;
		}
		LoadingCurtainGroup loadingCurtainGroup = UIManager.FindScript<LoadingCurtainGroup>();
		bool flag = (Object)(object)loadingCurtainGroup == (Object)null || loadingCurtainGroup.IsFadeoutStarted;
		UIBase.HideUIExceptFor(UIBase.UIFlag.CutScene, hide: true, "BeginMMO");
		ShowBottomMenu(isShow: false);
		SpawnNpcDog(isEventIntro: true, flag, 1f);
		CallDogCommand(delegate
		{
			KSingleton<CameraController>.Instance().SetCameraTarget(((Component)_npcAiDog).gameObject, 0.3f, 10f);
		});
		_system.PauseUpdate = true;
		if (flag)
		{
			LoadingCurtain_FadeoutStarted();
			LoadingCurtain_FadeoutFinished();
		}
		else
		{
			EventDelegate.Add(loadingCurtainGroup.FadeOutStarted, LoadingCurtain_FadeoutStarted, oneShot: true);
			EventDelegate.Add(loadingCurtainGroup.FadeOutFinished, LoadingCurtain_FadeoutFinished, oneShot: true);
		}
	}

	private void LoadingCurtain_FadeoutStarted()
	{
		if ((Object)(object)_npcAiDog != (Object)null)
		{
			_npcAiDog.RepositionToIntro();
			_npcAiDog.PlayIntroAnim();
		}
		_system.PauseUpdate = false;
	}

	private void LoadingCurtain_FadeoutFinished()
	{
		if (this.Ready != null)
		{
			this.Ready();
		}
	}

	private void Event_Appear_K()
	{
		SpawnBikeK();
		if (this.GuideOfKBegin != null)
		{
			this.GuideOfKBegin();
		}
	}

	private void Event_BeginCPR()
	{
		_npcAiK.BeginCPR();
		KSingleton<CameraController>.Instance().SetCameraTarget(((Component)PlayerBehavior.LocalPlayer).gameObject, 0.3f, 10f, 0.3f, forceRetarget: true);
	}

	private static void Event_Show_BottomMenu()
	{
		ShowBottomMenu(isShow: true);
	}

	private static void ShowBottomMenu(bool isShow)
	{
		UIManager.FindScript<LeftMenuListGroup>().SetVisible(isShow);
		UIManager.FindScript<BottomLeftMenuGroup>().SetVisible(isShow);
	}

	private void SpawnBikeK(Action func = null)
	{
		KSingleton<AssetBundleManager>.Instance().RequestAsset("Models/NPC/NPC_KBikePrefab.prefab", typeof(GameObject), delegate(Object asset)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected O, but got Unknown
			if (!(asset == (Object)null))
			{
				GameObject val = (GameObject)Object.Instantiate(asset, Vector3.zero, Quaternion.identity);
				_npcAiK = val.GetComponent<NpcAI_KBike>();
				_npcAiK.RepositionToIntro();
				if (func != null)
				{
					func();
				}
			}
		});
	}

	private void SpawnNpcDog(bool isEventIntro, bool isReload = false, float delay = -1f)
	{
		if (_isNpcDogSpawned)
		{
			return;
		}
		if ((Object)(object)_npcAiDog == (Object)null)
		{
			KUtility.DelayedCall((MonoBehaviour)(object)_system, delegate
			{
				KSingleton<AssetBundleManager>.Instance().RequestAsset("Models/Ancora/Animals/Dog/DogPrefab.prefab", typeof(GameObject), delegate(Object asset)
				{
					//IL_000e: Unknown result type (might be due to invalid IL or missing references)
					//IL_0013: Unknown result type (might be due to invalid IL or missing references)
					//IL_001d: Unknown result type (might be due to invalid IL or missing references)
					//IL_0023: Expected O, but got Unknown
					//IL_005f: Unknown result type (might be due to invalid IL or missing references)
					if (!(asset == (Object)null))
					{
						GameObject val = (GameObject)Object.Instantiate(asset, Vector3.zero, Quaternion.identity);
						_npcAiDog = val.GetComponent<NpcAIDog>();
						if (isEventIntro)
						{
							_npcAiDog.PrepareIntroMMO();
							_npcAiDog.SetPOIPos(PlayerBehavior.LocalPlayer.CurrentPosition);
							if (isReload)
							{
								_npcAiDog.RepositionToIntro();
								_npcAiDog.PlayIntroAnim();
							}
						}
					}
				});
			}, delay);
		}
		_isNpcDogSpawned = true;
	}

	public void StandUp(float rotBeginTime, float rotSpeed)
	{
		PlayerBehavior.LocalPlayer.PlayAnimation("Bike_Getup", 0f, 1f, forceTransition: true);
		_defaultRotateSpeed = PlayerBehavior.LocalPlayer.RotateSpeed;
		_delayedFaceToKCoroutine = CoDelayedFaceToK(rotBeginTime, rotSpeed);
		((MonoBehaviour)_system).StartCoroutine(_delayedFaceToKCoroutine);
		UIBase.HideUIExceptFor(UIBase.UIFlag.CutScene, hide: false, "BeginMMO");
		ShowBottomMenu(isShow: false);
	}

	private IEnumerator CoDelayedFaceToK(float rotBeginTime, float rotSpeed)
	{
		yield return (object)new WaitForSeconds(rotBeginTime);
		PlayerBehavior player = PlayerBehavior.LocalPlayer;
		player.RotateSpeed = rotSpeed;
		player.RotateToTarget(_npcAiK.Head);
		player.LookAtController.Activated = true;
		player.LookAtController.SetLookTarget(_npcAiK.Head);
		player.LookAtController.AutoChangeTarget = false;
		player.IsProhibitAnimRefresh = false;
	}

	private void Event_Restore_Standing_K_CutScene()
	{
		_lastDogGuideState = DogGuideState.AfterCpr;
		KSingleton<CameraController>.Instance().SetCameraTarget(((Component)PlayerBehavior.LocalPlayer).gameObject, 0.3f, 10f, 0.3f, forceRetarget: true);
		if (!((Object)(object)_npcAiK != (Object)null))
		{
			KSingleton<PlayerController>.Instance().MoveLock = true;
			PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
			if ((Object)(object)localPlayer != (Object)null)
			{
				localPlayer.IsOutlineEnabled = false;
			}
			SpawnBikeK(delegate
			{
				_npcAiK.RestoreStandingKCutScene();
				PlayerBehavior.LocalPlayer.RotateToTarget(_npcAiK.Head);
				PlayerBehavior.LocalPlayer.LookAtController.Activated = true;
				PlayerBehavior.LocalPlayer.LookAtController.SetLookTarget(_npcAiK.Head);
				PlayerBehavior.LocalPlayer.LookAtController.AutoChangeTarget = false;
			});
			CallDogCommand(delegate
			{
				_npcAiDog.RestoreStandingKCutScene();
			});
		}
	}

	private void Event_Disappear_K()
	{
		KSingleton<CameraController>.Instance().ResetCameraTarget(3f, 3f, forceReset: true);
		_npcAiK.EventRun();
		if (this.GuideOfKEnd != null)
		{
			this.GuideOfKEnd();
		}
	}

	private void Dog_NormalMode()
	{
		_lastDogGuideState = DogGuideState.Normal;
	}

	private void Dog_Introduce()
	{
		if ((Object)(object)_npcAiDog != (Object)null)
		{
			_npcAiDog.Dog_Introduce();
		}
	}

	private void Dog_Happy()
	{
		if ((Object)(object)_npcAiDog != (Object)null)
		{
			_npcAiDog.Dog_Happy();
		}
	}

	public void Event_UnLockPlayerMove()
	{
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		if ((Object)(object)localPlayer != (Object)null)
		{
			localPlayer.LookAtController.AutoChangeTarget = true;
			if (_delayedFaceToKCoroutine != null)
			{
				((MonoBehaviour)_system).StopCoroutine(_delayedFaceToKCoroutine);
			}
			localPlayer.RotateSpeed = _defaultRotateSpeed;
			PlayerBehavior.LocalPlayer.IsProhibitAnimRefresh = false;
			localPlayer.IsOutlineEnabled = true;
		}
		KSingleton<PlayerController>.Instance().MoveLock = false;
		KSingleton<BGMManager>.Instance().AllowlslandBGM(allow: true);
	}

	private static void Event_ShowOtherPlayer()
	{
		KSingleton<PlayerManager>.Instance().HideOtherPlayer(hide: false);
	}

	private void Dog_SetPOI_Tile(Dictionary<string, string> parameters)
	{
		if (_system.IsGuideBegin)
		{
			CallDogCommand(delegate
			{
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				float num = parameters.Get("x").ToFloat();
				float num2 = parameters.Get("y").ToFloat();
				_npcAiDog.SetPOIPosTile(new Vector2(num, num2));
			});
		}
	}

	private void Dog_Set_Farewell_Tile(Dictionary<string, string> parameters)
	{
		_lastDogGuideState = DogGuideState.Finished;
		CallDogCommand(delegate
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			float num = parameters.Get("x").ToFloat();
			float num2 = parameters.Get("y").ToFloat();
			_npcAiDog.SetFarewellTile(new Vector2(num, num2));
		});
	}

	private void CallDogCommand(Action action)
	{
		if ((Object)(object)_npcAiDog == (Object)null)
		{
			SpawnNpcDog(isEventIntro: false);
		}
		((MonoBehaviour)_system).StartCoroutine(CoSetDogCommand(action));
	}

	private IEnumerator CoSetDogCommand(Action action)
	{
		while ((Object)(object)_npcAiDog == (Object)null)
		{
			yield return null;
		}
		action?.Invoke();
	}

	private static void Ancora_Event_Init_Health()
	{
		Connections.Frontend.Send(new TutorialEvent
		{
			Event = "init_health"
		});
	}

	private static void Ancora_Event_Resurrect()
	{
		Connections.Frontend.Send(new TutorialEvent
		{
			Event = "resurrect"
		});
	}

	private static void Ancora_Event_Restore_Food_K()
	{
		Connections.Frontend.Send(new TutorialEvent
		{
			Event = "restore_food_k"
		});
	}

	private static void Ancora_Event_Tired()
	{
		Connections.Frontend.Send(new TutorialEvent
		{
			Event = "set_tired"
		});
		PlayerBehavior.LocalPlayer.SurvivalGaugeUpdated += PlayerBehavior_SurvivalGaugeUpdated;
	}

	private static void PlayerBehavior_SurvivalGaugeUpdated(CharacterBehavior chcracter)
	{
		if ((Object)(object)chcracter == (Object)(object)PlayerBehavior.LocalPlayer && PlayerBehavior.LocalPlayer.IsTired)
		{
			PlayerBehavior.LocalPlayer.SurvivalGaugeUpdated -= PlayerBehavior_SurvivalGaugeUpdated;
			PlayerBehavior.LocalPlayer.UpdateMovingMotion();
		}
	}

	private static void Guide_Progress(Dictionary<string, string> parameters)
	{
		int num = parameters.Get("seq").ToInt();
		Connections.Frontend.Send(new GuideProgress
		{
			Seq = (byte)num
		});
	}

	private static void Activate_Faction(Dictionary<string, string> parameters)
	{
		FactionType faction = (FactionType)(int)Enum.Parse(typeof(FactionType), parameters.Get("faction"));
		Connections.Frontend.Send(new ActivateFaction
		{
			Faction = faction
		});
	}
}
