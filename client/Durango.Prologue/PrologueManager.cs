using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using Durango.Logic;
using Durango.Logic.Combat;
using Durango.Logic.Item;
using Durango.Logic.PlayGuide;
using Durango.Model;
using Durango.Network;
using Durango.Player;
using Durango.Render.Camera;
using Durango.Render.Screen;
using Durango.System;
using Durango.System.Config;
using Durango.UI;
using Durango.UI.Control;
using Durango.UI.Prologue;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Newtonsoft.Json.Linq;
using Shared.Battle;
using Shared.Player;
using Shared.Teleport;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Prologue;

public class PrologueManager : Durango.Utils.Singleton<PrologueManager>
{
	public enum State
	{
		None,
		CharacterSelect,
		TrainPlayBegin,
		PlayMovie,
		CreatePlayer,
		RefreshSessionToken,
		RequestCreatePlayer,
		PrerequsiteLoading,
		Loading
	}

	private enum ProloguePhase
	{
		NormalTrain,
		AfterScoop,
		TrexCutScene
	}

	public struct PlayerDisplay
	{
		public string hair;

		public string[] body_color;

		public string[] head_color;

		public string skin_color;

		public string hair_color;

		public string lip_color;

		public string eye_color;

		public int portrait;

		public int portrait_bg;

		public string portrait_bg_color;

		public string beard;

		public int voice_type;

		public float body_size;
	}

	private class CreateCharacterInfo
	{
		public bool IsMale;

		public Shared.Player.Job Job;

		public string Name;

		public string Region;

		public Messages.PlayerDisplay Display;
	}

	[SerializeField]
	private GameObject _triggersGroup;

	[SerializeField]
	private List<GameObject> _deactivateAtStartList = new List<GameObject>();

	[SerializeField]
	private List<GameObject> _deactivateList = new List<GameObject>();

	[SerializeField]
	private List<GameObject> _activateList = new List<GameObject>();

	[SerializeField]
	private List<GameObject> _prologueEndDeactivateList = new List<GameObject>();

	[SerializeField]
	private List<TriggerDialog> _triggerOnPhase2List = new List<TriggerDialog>();

	[SerializeField]
	private List<Texture> _litSphereTextures = new List<Texture>();

	[SerializeField]
	private float _wetnessAtRainy = 0.5f;

	[SerializeField]
	private GameObject _colliderAtSpearGet;

	[SerializeField]
	private string _trainModelName = "prologue_train";

	[SerializeField]
	private GameObject _trainCoverAisle1;

	[SerializeField]
	private GameObject _trainCoverCabin1;

	[SerializeField]
	private GameObject _trainModel;

	[SerializeField]
	private Texture2D _fadeTexture;

	[SerializeField]
	private AnimationClip _playerTunnelMotionMale;

	[SerializeField]
	private AnimationClip _playerTunnelMotionFemale;

	[SerializeField]
	private AnimationClip[] _playerClipsMale;

	[SerializeField]
	private AnimationClip[] _playerClipsFemale;

	[SerializeField]
	private SoundEventType _sceneStartSound;

	[SerializeField]
	private SoundEventType _charSelectStartSound;

	[SerializeField]
	private SoundEventType _trainSound;

	[SerializeField]
	private SoundEventType _createCharBgm;

	[SerializeField]
	private float _createCharBgmFadeOutDuration = 1f;

	private uint _trainSoundId;

	private uint _createCharBgmId;

	private PrerequisiteLoader _prerequsiteLoader;

	private State _curState;

	private State _nextState;

	private PrologueNPCFloatingGroup _npcFloatingGroup;

	private PrologueCharacterSelectGroupBase _prologueCharacterSelectUI;

	private ProloguePhase _curPhase;

	private CreateCharacterInfo _createCharacterInfo = new CreateCharacterInfo();

	private HTTPRequest _request;

	private ProloguePlayGuideHelperGroup _prologuePlayGuideHelper;

	private Shared.Player.Job? _selectedJob;

	private bool? _selectedGender;

	private Messages.PlayerDisplay? _selectedDisplay;

	private Action _requestCreatePlayerFinished;

	private List<Durango.Logic.StatusEffect> _prologueEffects = new List<Durango.Logic.StatusEffect>();

	private readonly string _errorMsg = T.N_("캐릭터를 생성하지 못 했습니다. 잠시 후 다시 시도해주세요.");

	public State CurrentState => _curState;

	public PrologueNPCFloatingGroup NPCFloatingGroup => (!(_npcFloatingGroup != null)) ? (_npcFloatingGroup = UIManager.FindScript<PrologueNPCFloatingGroup>()) : _npcFloatingGroup;

	private PrologueCharacterSelectGroupBase PrologueCharacterSelectUI => (!(_prologueCharacterSelectUI != null)) ? (_prologueCharacterSelectUI = UIManager.FindScript<PrologueCharacterSelectGroupBase>()) : _prologueCharacterSelectUI;

	public PrologueTrainManager TrainManager { get; private set; }

	public bool BeginIntreaction { get; set; }

	public GameObject TriggersGroup => _triggersGroup;

	public GameObject TrainCoverAisle1 => _trainCoverAisle1;

	public GameObject TrainCoverCabin1 => _trainCoverCabin1;

	public GameObject TrainModel => _trainModel;

	public static bool ToBeSkipped { get; set; }

	public ProloguePlayGuideHelperGroup PlayGuideHelper
	{
		get
		{
			if (_prologuePlayGuideHelper == null)
			{
				_prologuePlayGuideHelper = UIManager.FindScript<ProloguePlayGuideHelperGroup>();
			}
			return _prologuePlayGuideHelper;
		}
	}

	private void CheckBackgroundDownloading()
	{
		if (AssetBundleManager.UseBundle)
		{
			switch (_curState)
			{
			case State.CharacterSelect:
				StartBackgroundDownloading();
				break;
			case State.CreatePlayer:
				_prerequsiteLoader.gameObject.SetActive(value: false);
				Durango.Utils.Singleton<AssetBundleManager>.Instance().StopBackgroundDownloading();
				break;
			}
		}
	}

	private void StartBackgroundDownloading()
	{
		_prerequsiteLoader.gameObject.SetActive(value: true);
		_prerequsiteLoader.TotalCount = Durango.Utils.Singleton<AssetBundleManager>.Instance().TotalFileCount;
		Durango.Utils.Singleton<AssetBundleManager>.Instance().StartBackgroundDownloading(_prerequsiteLoader.ProgressChanged, _prerequsiteLoader.DetailedProgressChanged, AssetBundleManager_BackgroundLoadingCompleted);
	}

	protected override void OnAwake()
	{
		Application.targetFrameRate = 60;
		Durango.Utils.Singleton<TrainTrexController>.Instance().Initialize();
		_trainModel = Durango.Utils.Singleton<PrologueTrainManager>.Instance().transform.Find(_trainModelName).gameObject;
		_trainCoverAisle1 = KUtility.FindObjectByName(_trainModel, "train_cover_short");
		_trainCoverCabin1 = KUtility.FindObjectByName(_trainModel, "train_cover_long");
		int count = _deactivateAtStartList.Count;
		for (int i = 0; i < count; i++)
		{
			if ((bool)_deactivateAtStartList[i])
			{
				_deactivateAtStartList[i].SetActive(value: false);
			}
		}
		ApplyWetness(Durango.Utils.Singleton<PrologueTrainManager>.Instance().gameObject, 0f);
		UIManager.FindScript<PrologueLeftMenuListGroupBase>().gameObject.SetActive(value: false);
		_prerequsiteLoader = UIManager.FindScript<PrologueWaitDownloadGroup>().PrerequsiteLoader;
		_prerequsiteLoader.gameObject.SetActive(value: false);
		UIManager.ShowLoadingCurtain<PrologueLoadingCurtain>();
	}

	private void Start()
	{
		Durango.Utils.Singleton<CustomColorCorrectionEffect>.Instance().PauseTime = true;
		UIManager.OnLoadingCurtainHidden(LoadingCurtainGroup_FadeOutFinished);
		Connections.Frontend.PushPacket(new SetBaseMoveSpeed
		{
			EntityId = GameManager.PlayerId,
			NormalSpeed = 200,
			BattleSpeed = 200
		});
		GameSystem<InputSystem>.Instance().MoveLock = true;
		PlayerBehavior.LocalPlayer.gameObject.SetActive(value: false);
		Durango.Utils.Singleton<UIManager>.Instance().VirtualStick.gameObject.SetActive(value: false);
		TrainManager = UnityEngine.Object.FindObjectOfType<PrologueTrainManager>();
		_triggersGroup.SetActive(value: true);
		PrologueCharacterSelectUI.OnSubmit = OnSubmitSelectCharacter;
		PrologueCharacterSelectUI.OnCloseSucceed += OnCancelSelectCharacter;
		PrologueCharacterSelectUI.OnChangeCostume = OnChangeCostumeSelectCharacter;
		_deactivateList.Add(KUtility.FindObjectByName(_trainModel, "train_01", includeInactive: true));
		_deactivateList.Add(KUtility.FindObjectByName(_trainModel, "train_02", includeInactive: true));
		_deactivateList.Add(KUtility.FindObjectByName(_trainModel, "train_03", includeInactive: true));
		_activateList.Add(KUtility.FindObjectByName(_trainModel, "train_05", includeInactive: true));
		_activateList.Add(KUtility.FindObjectByName(_trainModel, "train_06", includeInactive: true));
		UIManager.FindScript<PlayerFloatingGroup>().HideLocalPlayer();
		GameSystem<CombatSystem>.Instance().DamagedProcesser.Damaged += OnDamaged;
		Durango.Utils.Singleton<CameraController>.Instance().SetZoom(0.7f);
		Durango.Utils.Singleton<CameraController>.Instance().LockZoomControl(isLock: true);
		if (ToBeSkipped)
		{
			SkipPrologue();
		}
		Durango.Utils.Singleton<GameManager>.Instance().ForceMainSceneLoadedPrologue();
		SoundManager.PlayEvent(_sceneStartSound);
	}

	private void SetNextState(State next)
	{
		_nextState = next;
		if (_nextState == State.CreatePlayer)
		{
			NotifyRequestCreatePlayerFinished();
		}
	}

	private void UpdateState()
	{
		if (_nextState == _curState)
		{
			return;
		}
		State curState = _curState;
		_curState = _nextState;
		State curState2 = _curState;
		switch (curState2)
		{
		case State.None:
			break;
		case State.CharacterSelect:
			GameSystem<PrologueGuideSystem>.Instance().Init();
			PrologueInteractionButtonGroupBase.RefreshInteractions();
			UIManager.FindScript<PrologueLeftMenuListGroupBase>().gameObject.SetActive(value: true);
			_trainSoundId = SoundManager.PlayEvent(_trainSound, SoundPosition.Empty, exclusive: true);
			SoundManager.SetState(new SoundStates("train", "in_side"));
			SoundManager.PlayEvent(_charSelectStartSound);
			break;
		case State.TrainPlayBegin:
			break;
		case State.PlayMovie:
			break;
		case State.CreatePlayer:
		{
			GameSystem<PrologueGuideSystem>.Instance().ForceClearGuideMsg();
			if (_curState > curState)
			{
				Transform transform = Durango.Utils.Singleton<UIManager>.Instance().UIRoot.transform;
				int i = 0;
				for (int childCount = transform.childCount; i < childCount; i++)
				{
					UIRect component = transform.GetChild(i).GetComponent<UIRect>();
					if (component != null && component.gameObject.activeSelf)
					{
						component.alpha = 0f;
					}
				}
				UIManager.Popup.GetComponent<UIRect>().alpha = 1f;
				UIManager.MessageBox.GetComponent<UIRect>().alpha = 1f;
			}
			EditPlayerDisplayGroup editPlayerDisplayGroup = UIManager.FindScript<EditPlayerDisplayGroup>();
			if (!editPlayerDisplayGroup.IsOpened)
			{
				editPlayerDisplayGroup.OpenCreateCharacter(_selectedGender, _selectedDisplay, _selectedJob, delegate(string userName, string region, EditPlayerDisplayProxy display, Action callback)
				{
					_requestCreatePlayerFinished = callback;
					UIManager.MessageBox.Show(T._("[b][fad257]{0}[-][/b]{0:-으로} 캐릭터를 만드시겠습니까?", userName), delegate(bool ok)
					{
						if (ok)
						{
							SoundManager.PlayEvent("ui_new_character_created");
							FinishCreateCharacter(userName, region, display.Gender, display.Job.Value.Value, display.MakeDisplay());
						}
						else
						{
							NotifyRequestCreatePlayerFinished();
						}
					});
				});
			}
			editPlayerDisplayGroup.gameObject.SetActive(value: true);
			if (_createCharBgmId == 0)
			{
				_createCharBgmId = SoundManager.PlayEvent(_createCharBgm, SoundPosition.Empty, exclusive: true);
			}
			break;
		}
		case State.PrerequsiteLoading:
		{
			PrologueWaitDownloadGroup waitUI = UIManager.FindScript<PrologueWaitDownloadGroup>();
			waitUI.Show();
			UIManager.FindScript<EditPlayerDisplayGroup>().gameObject.SetActive(value: false);
			if (AssetBundleManager.UseBundle)
			{
				_prerequsiteLoader.gameObject.SetActive(value: true);
				_prerequsiteLoader.TotalCount = Durango.Utils.Singleton<AssetBundleManager>.Instance().PrerequsitesCount;
				Durango.Utils.Singleton<AssetBundleManager>.Instance().StartPrerequisiteLoading(_prerequsiteLoader.ProgressChanged, _prerequsiteLoader.DetailedProgressChanged, AssetBundleManager_PrerequisiteLoadingCompleted, delegate(int mega, int remainCount)
				{
					_prerequsiteLoader.TotalCount = remainCount;
					string prerequsiteDownloadWarningMessage = TitleMenuGroup.GetPrerequsiteDownloadWarningMessage(mega);
					waitUI.SetDonwloadWarning(prerequsiteDownloadWarningMessage);
				});
			}
			else
			{
				SetNextState(State.Loading);
			}
			break;
		}
		case State.Loading:
			GameManager.IsPlayerIdSelected = true;
			ConfigInstance.RefreshValue("fps");
			Durango.Utils.Singleton<GameManager>.Instance().MoveToTitle();
			break;
		case State.RefreshSessionToken:
		case State.RequestCreatePlayer:
			break;
		}
	}

	private void LoadingCurtainGroup_FadeOutFinished()
	{
		if (_curState == State.None)
		{
			// [ย้อนกลับ] เคยลองข้ามฉากรถไฟไปหน้าสร้างตัวละครเลย (เรียก SkipPrologue() ตรงนี้)
			// แต่พังจริง: HUD/ระบบต่อสู้ (PlayerHudGroupBase, BattleActionButtons, ArtifactManager)
			// เริ่มทำงานเร็วเกินไป**ก่อน Terrain โหลดเสร็จ** ⇒ NullReferenceException รัวทุกเฟรม
			// UI หลักหายหมดทั้งเกม (ไม่ใช่แค่ปุ่มข้ามที่พัง) — ฉากรถไฟเป็นตัวถ่วงเวลาโดยอ้อม
			// ให้ระบบพวกนี้พร้อมก่อนเข้าเกมจริง ข้ามไม่ได้แบบนี้
			// ⇒ กลับไปเข้า State.CharacterSelect (ฉากรถไฟ) ตามเดิม
			SetNextState(State.CharacterSelect);
		}
	}

	private void AssetBundleManager_BackgroundLoadingCompleted(bool succeed)
	{
		if (succeed)
		{
			_prerequsiteLoader.gameObject.SetActive(value: false);
		}
		else if (_curState < State.CreatePlayer)
		{
			StartBackgroundDownloading();
		}
	}

	private void AssetBundleManager_PrerequisiteLoadingCompleted(bool succeed)
	{
		_prerequsiteLoader.gameObject.SetActive(value: false);
		if (succeed)
		{
			SetNextState(State.Loading);
			return;
		}
		UIManager.MessageBox.Show(T._("오류: 필수 데이터 받기가 실패하였습니다.\n 로그인 화면으로 돌아갑니다."), (Action)delegate
		{
			Durango.Utils.Singleton<GameManager>.Instance().MoveToTitle();
		}, (string)null);
	}

	private Texture FindInsideLitSphereTexture(Texture currentTexture)
	{
		if ((bool)currentTexture)
		{
			string text = currentTexture.name + "_insidetrain";
			int count = _litSphereTextures.Count;
			for (int i = 0; i < count; i++)
			{
				if (text == _litSphereTextures[i].name)
				{
					return _litSphereTextures[i];
				}
			}
		}
		return null;
	}

	private void MakeMaterialsLitSphereOverride(Material[] materials)
	{
		int num = materials.Length;
		for (int i = 0; i < num; i++)
		{
			if (!materials[i].HasProperty("_LitSphereTex"))
			{
				continue;
			}
			Texture texture = materials[i].GetTexture("_LitSphereTex");
			if (!(texture == null))
			{
				Texture texture2 = FindInsideLitSphereTexture(texture);
				if ((bool)texture2)
				{
					materials[i].SetTexture("_LitSphereTex", texture2);
				}
			}
		}
	}

	public void MakeLitSphereOverride(Transform meshObjectTransform)
	{
		SkinnedMeshRenderer[] componentsInChildren = meshObjectTransform.GetComponentsInChildren<SkinnedMeshRenderer>();
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			MakeMaterialsLitSphereOverride(componentsInChildren[i].materials);
		}
	}

	public void SetPrologueModelAnimation(PlayerBehavior player, bool isMale)
	{
		AnimationClip[] array = ((!isMale) ? _playerClipsFemale : _playerClipsMale);
		AnimationClip[] array2 = ((!isMale) ? _playerClipsMale : _playerClipsFemale);
		Animation componentInChildren = player.GetComponentInChildren<Animation>();
		for (int i = 0; i < array2.Length; i++)
		{
			if ((bool)componentInChildren.GetClip(array2[i].name))
			{
				componentInChildren.RemoveClip(array2[i].name);
			}
		}
		AnimationClip[] array3 = array;
		foreach (AnimationClip animationClip in array3)
		{
			componentInChildren.AddClip(animationClip, animationClip.name);
		}
	}

	public AnimationClip[] GetPlayerClips(bool male)
	{
		return (!male) ? _playerClipsFemale : _playerClipsMale;
	}

	public void BeginPlayer(CostumeActorBehavior actor, Vector3 destPos)
	{
		GameSystem<InputSystem>.Instance().MoveLock = false;
		PrologueInteractionButtonGroupBase.HideInteractionButton();
		Durango.Utils.Singleton<PlayerController>.Instance().StopMove();
		Connections.Frontend.PushPacket(new SetBaseMoveSpeed
		{
			EntityId = GameManager.PlayerId,
			NormalSpeed = 500,
			BattleSpeed = 500
		});
		PlayerBehavior.LocalPlayer.gameObject.SetActive(value: true);
		Durango.Utils.Singleton<UIManager>.Instance().VirtualStick.gameObject.SetActive(value: true);
		UnityEngine.Object.Destroy(PlayerBehavior.LocalPlayer.gameObject);
		bool isMale = actor.IsMale;
		PlayerBehavior playerBehavior = Durango.Utils.Singleton<PlayerManager>.Instance().MakePlayerObject(isMale, destPos, string.Empty, "Barehand_Stand", loadClips: false);
		SetPrologueModelAnimation(playerBehavior, isMale);
		playerBehavior.TurnToYaw(actor.transform.localRotation.eulerAngles.y, bSnap: true);
		playerBehavior.gameObject.name = "Player";
		playerBehavior.EntityId = GameManager.PlayerId;
		playerBehavior.PlayerName = string.Empty;
		Durango.Utils.Singleton<PlayerController>.Instance().Teleport(destPos, TeleportType.Unknown, instance: true);
		Durango.Utils.Singleton<PlayerController>.Instance().TurnToYaw(actor.transform.localRotation.eulerAngles.y, snap: true);
		Durango.Utils.Singleton<PlayerController>.Instance().IgnoreOcclusionCheck = true;
		playerBehavior.MakePrologueMode();
		_selectedGender = isMale;
		_selectedDisplay = EditPlayerDisplayProxy.ParseCostume(actor.GetCostumeDictionary());
		PlayerManager.SetDisplay(playerBehavior, _selectedDisplay.Value);
		PlayerBehavior.LocalPlayer = playerBehavior;
		Durango.Utils.Singleton<CameraController>.Instance().Target(null, 0.3f).Offset(Vector3.zero, 0.3f)
			.ZoomRatio(1f, 0.3f)
			.Zoom(0.7f, 0.3f);
		Durango.Utils.Singleton<CameraController>.Instance().LockZoomControl(isLock: false);
		// [แก้เอง] ข้ามฉาก cutscene สอนเล่นบนรถไฟ (TrainPlayBegin/PlayMovie) ไปสร้างตัวละครเลย
		// ต่างจากที่เคยลองพังก่อนหน้า (ดู comment ที่ LoadingCurtainGroup_FadeOutFinished): ครั้งนั้น
		// skip ทันทีตอน curtain fade out (ก่อน CharacterSelect scene ได้เริ่มด้วยซ้ำ) ⇒ Terrain/HUD/
		// Combat ยังไม่พร้อมเลย ครั้งนี้ skip ทีหลัง — หลังจากผู้เล่นดู scene รถไฟ, เลือกตัวละครจริง ๆ
		// (ผ่านการโหลด/คลิก UI ใช้เวลาหลายวินาที) ระบบพื้นฐานมีเวลาโหลดตามธรรมชาติแล้ว ไม่ auto-skip
		// ตั้งแต่ต้นแบบเดิม — ตัวแปรที่ State.CreatePlayer ต้องใช้ (_selectedGender/_selectedDisplay/
		// _selectedJob) ถูกตั้งค่าครบก่อนหน้านี้แล้วทั้งหมด ไม่ขาดอะไร
		SetNextState(State.CreatePlayer);
	}

	public void AddStatusEffects()
	{
		GameSystem<PrologueToDoListSystem>.Instance().SetCompleted("move_character", completed: true);
		_prologueEffects.Clear();
		GameSystem<StatusEffectSystem>.Instance().GetStatusEffects().SetStatusEffects(_prologueEffects);
		StartCoroutine(CoAddStatusEffects());
	}

	private IEnumerator CoAddStatusEffects()
	{
		AddStatusEffect("hungry", T._("배고픔"), T._("배가 출출하다."), "icon_se_satietylow");
		yield return new WaitForSeconds(0.5f);
		AddStatusEffect("thirst", T._("갈증"), T._("목이 마르다."), "icon_se_waterlow");
	}

	public void AddStatusEffect(string id, string statusEffectName, string desc, string icon)
	{
		Durango.Logic.StatusEffect item = new Durango.Logic.StatusEffect(id, statusEffectName, desc, icon);
		_prologueEffects.Add(item);
		GameSystem<StatusEffectSystem>.Instance().GetStatusEffects().SetStatusEffects(_prologueEffects);
	}

	public void RemoveStatusEffect(string id)
	{
		_prologueEffects.RemoveAll((Durango.Logic.StatusEffect ef) => ef.Id == id);
		GameSystem<StatusEffectSystem>.Instance().GetStatusEffects().SetStatusEffects(_prologueEffects);
	}

	public void DelayedCall(Action func, float delay)
	{
		KUtility.DelayedCall(this, func, delay);
	}

	private void OnSubmitSelectCharacter()
	{
		TriggerPrologueSelectCharacter targetComponent = GameSystem<InteractionSystem>.Instance().LastInteractionTarget.GetTargetComponent<TriggerPrologueSelectCharacter>();
		if ((bool)targetComponent)
		{
			targetComponent.ChooseCharacter();
			targetComponent.Unselect();
			_selectedJob = targetComponent.Job;
		}
		Durango.Utils.Singleton<CameraController>.Instance().Target(null, 0.3f).Offset(Vector3.zero, 0.3f)
			.ZoomRatio(1f, 0.3f)
			.Zoom(0.7f, 0.3f);
		PrologueCharacterSelectUI.Close();
	}

	private void OnCancelSelectCharacter()
	{
		TriggerPrologueSelectCharacter targetComponent = GameSystem<InteractionSystem>.Instance().LastInteractionTarget.GetTargetComponent<TriggerPrologueSelectCharacter>();
		if ((bool)targetComponent)
		{
			targetComponent.Unselect();
		}
		if (_selectedJob.HasValue)
		{
			PrologueInteractionButtonGroupBase.ClearInteractions();
		}
		Durango.Utils.Singleton<CameraController>.Instance().Target(null, 0.3f).Offset(Vector3.zero, 0.3f)
			.ZoomRatio(1f, 0.3f)
			.Zoom(0.7f, 0.3f);
	}

	private void OnChangeCostumeSelectCharacter()
	{
		CostumeActorBehavior targetComponent = GameSystem<InteractionSystem>.Instance().LastInteractionTarget.GetTargetComponent<CostumeActorBehavior>();
		if ((bool)targetComponent)
		{
			PlayerCostumeTable playerCostumeTable = ResourceSingleton<PlayerCostumeTable>.Instance();
			string assetBundlePathBase = playerCostumeTable.GetRandom(PlayerCostumeTable.Category.Hair, targetComponent.IsMale).AssetBundlePathBase;
			targetComponent.ChangeCostume(CharacterCostume.CostumeType.Hair, assetBundlePathBase);
			targetComponent.RandomCostumeColors(targetComponent.GetCostumeName(CharacterCostume.CostumeType.Body), targetComponent.GetCostumeName(CharacterCostume.CostumeType.Head));
		}
	}

	private void OnDamaged(Damaged msg)
	{
		if (msg.VictimId == GameManager.PlayerId)
		{
			if (msg.Damage.Result != DamageResult.Dodged)
			{
				return;
			}
			ToDoBase toDoBase = GameSystem<PrologueToDoListSystem>.Instance().FindToDo("dodge");
			if (toDoBase != null)
			{
				GameSystem<PrologueToDoListSystem>.Instance().SetProgress("dodge", 1);
				DelayedCall(delegate
				{
					GameSystem<PrologueGuideSystem>.Instance().SetNextGuide("DodgeComplete");
				}, 3f);
				GameSystem<CombatSystem>.Instance().SetCurrentBattleActions(new BattleAction[2]
				{
					new BattleAction(SingletonDict<string, PlayerAction>.Get("twohand_default_a"))
					{
						Motion = "Novice_Twohand_Attack"
					},
					new BattleAction(SingletonDict<string, PlayerAction>.Get("onehand_dodge"))
					{
						Motion = "Novice_Dodge"
					}
				});
			}
			return;
		}
		bool isDead = false;
		ToDoBase toDoBase2 = GameSystem<PrologueToDoListSystem>.Instance().FindToDo("active_action");
		if (toDoBase2 != null)
		{
			GameSystem<PrologueToDoListSystem>.Instance().SetProgress("active_action", toDoBase2.CurrentProgress + 1);
			if (toDoBase2.CurrentProgress >= toDoBase2.TargetProgress)
			{
				GameSystem<PrologueToDoListSystem>.Instance().SetCompleted("active_action", completed: true);
				PlayTrexCutScene();
				isDead = true;
			}
			else if (toDoBase2.CurrentProgress == 1)
			{
				DelayedCall(delegate
				{
					GameSystem<PrologueGuideSystem>.Instance().SetNextGuide("FirstActiveActionHit");
				}, 1f);
			}
		}
		CharacterBehavior characterBehavior = Durango.Utils.Singleton<ObjectManager>.Instance().FindCharacter(msg.VictimId);
		if (!(characterBehavior == null))
		{
			PrologueAIRaptor component = characterBehavior.GetComponent<PrologueAIRaptor>();
			if (!(component == null))
			{
				component.OnTakeDamage(msg.Damage, isDead);
			}
		}
	}

	[UsedImplicitly]
	private void OnThanksToYou()
	{
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.ThanksToYou);
	}

	[UsedImplicitly]
	private void OnFinishKidSitDown()
	{
	}

	public void DoPhase2(bool skipTunnelEffect = false)
	{
		if (_curPhase >= ProloguePhase.AfterScoop)
		{
			return;
		}
		_curPhase = ProloguePhase.AfterScoop;
		BeginIntreaction = true;
		GameSystem<PrologueGuideSystem>.Instance().ForceClearGuideMsg();
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		PrologueInteractionButtonGroupBase.HideInteractionButton();
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.AfterEatAndDrink);
		int count = _deactivateList.Count;
		for (int i = 0; i < count; i++)
		{
			if ((bool)_deactivateList[i])
			{
				_deactivateList[i].SetActive(value: false);
			}
		}
		count = _activateList.Count;
		for (int j = 0; j < count; j++)
		{
			if ((bool)_activateList[j])
			{
				_activateList[j].SetActive(value: true);
				_activateList[j].transform.localPosition = Vector3.zero;
			}
		}
		PrologueTunnelController prologueTunnelController = UnityEngine.Object.FindObjectOfType<PrologueTunnelController>();
		if ((bool)prologueTunnelController)
		{
			prologueTunnelController.TunnelEffect(skipTunnelEffect);
		}
	}

	public void PlayFrightenMotion()
	{
		int count = _triggerOnPhase2List.Count;
		for (int i = 0; i < count; i++)
		{
			if ((bool)_triggerOnPhase2List[i])
			{
				_triggerOnPhase2List[i].BeginEvent();
			}
		}
		AnimationClip clip = ((!PlayerBehavior.LocalPlayer.IsMale) ? _playerTunnelMotionFemale : _playerTunnelMotionMale);
		PlayerBehavior.LocalPlayer.PlayClip(clip);
	}

	public void BeginRaining()
	{
		Durango.Utils.Singleton<PrologueTrainManager>.Instance().BeginRaining();
		ApplyWetness(Durango.Utils.Singleton<PrologueTrainManager>.Instance().gameObject, _wetnessAtRainy);
		ApplyWetness(Durango.Utils.Singleton<TrainTrexController>.Instance().gameObject, _wetnessAtRainy);
	}

	private static void ApplyWetness(GameObject obj, float wetness)
	{
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>(includeInactive: true);
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			int num2 = componentsInChildren[i].materials.Length;
			for (int j = 0; j < num2; j++)
			{
				Material material = componentsInChildren[i].materials[j];
				string text = material.shader.name;
				if (!text.Contains("TrainCover_Exterior_Transparent") && !text.Contains("LitSphere"))
				{
					continue;
				}
				if (wetness <= 0f)
				{
					material.DisableKeyword("WETNESS_ON");
				}
				else if (material.HasProperty("_Wetness"))
				{
					material.EnableKeyword("WETNESS_ON");
					if (text.Contains("TrainCover_Exterior_Transparent"))
					{
						float @float = material.GetFloat("_MaxWetness");
						material.SetFloat("_Wetness", wetness * @float);
					}
					else
					{
						material.SetFloat("_Wetness", wetness);
					}
				}
			}
		}
	}

	[UsedImplicitly]
	private void SetLookAround()
	{
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide("LookAround");
	}

	public void DoGetAxe()
	{
		PlayerBehavior.LocalPlayer.ChangeWeaponType(PlayerBehavior.WeaponFramework.TWOHAND);
		Connections.Frontend.PushPacket(new SetBaseMoveSpeed
		{
			EntityId = GameManager.PlayerId,
			NormalSpeed = 250,
			BattleSpeed = 250
		});
		PlayerBehavior.LocalPlayer.ChangeEquipment("Models/Equipment/Melee/axe_onehand_emergency_axe1.FBX");
		PlayerBehavior.LocalPlayer.ChangeEquipmentColor(new ItemColor("B40000".ToColor(), "7C7C7C".ToColor(), "A68E5A".ToColor()));
		ItemData itemData = new ItemData();
		itemData.Id = string.Empty;
		GameSystem<EquipSystem>.Instance().EquipItem(GameSystem<EquipSystem>.Instance().CurrentEquipPreset, "main", itemData);
		PlayerBehavior.LocalPlayer.SetWeaponData(new WeaponDisplayInfo
		{
			Projectile = null,
			DetonateDelay = null,
			ProjectileSpeed = null,
			WeaponFramework = PlayerBehavior.WeaponFramework.TWOHAND.ToString()
		});
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.GetAxeSuccess);
		_colliderAtSpearGet.SetActive(value: false);
	}

	public void PlayTrexCutScene()
	{
		if (_curPhase < ProloguePhase.TrexCutScene)
		{
			_curPhase = ProloguePhase.TrexCutScene;
		}
	}

	public void PrologueFinished()
	{
		int count = _prologueEndDeactivateList.Count;
		for (int i = 0; i < count; i++)
		{
			if ((bool)_prologueEndDeactivateList[i])
			{
				_prologueEndDeactivateList[i].SetActive(value: false);
			}
		}
		UIManager.FindScript<ProloguePlayerHudGroup>().gameObject.SetActive(value: false);
		GameSystem<PrologueToDoListSystem>.Instance().RemoveAll();
		VisibleController.Hide(VisibleType.Base, hide: true);
		PlayGuideHelper.ClearTarget();
		UIManager.FindScript<PrologueLeftMenuListGroupBase>().gameObject.SetActive(value: false);
		// [แก้เอง] 🐛 "กดข้ามบทนำแล้วไม่มีอะไรเกิดขึ้น"
		//    ถ้ายังไม่ได้เลือกตัวละครในฉากรถไฟ จะยังไม่มีตัวละครในฉาก ⇒ LocalPlayer เป็น null
		//    บรรทัดนี้เลยโยน NullReference กลางทาง SkipPrologue() แล้วตายเงียบ ๆ
		//    ผู้เล่นเห็นเป็น "ปุ่มกดไม่ติด" ทั้งที่จริงคือ exception
		if (PlayerBehavior.LocalPlayer != null)
		{
			PlayerBehavior.LocalPlayer.gameObject.SetActive(value: false);
		}
		Durango.Utils.Singleton<PlayerController>.Instance().CutScenePlayMode = false;
		// [แก้เอง] 🐛 **ผู้เล่นใหม่ค้างจอดำถาวร**
		//    จบบทนำแล้วเกมสตรีมหนังเปิดเรื่องจาก db.kyllox.pe.kr ซึ่ง **เว็บนั้นตายแล้ว**
		//    ⇒ callback onFinished ไม่มีวันถูกเรียก ไม่มีทางไปต่อ
		//    (เส้นทาง ToBeSkipped เดิมข้ามหนังอยู่แล้ว = พิสูจน์แล้วว่าข้ามได้ไม่มีปัญหา)
		// ⇒ ข้ามหนังเสมอ ไปหน้าสร้างตัวละครตรง ๆ
		//    อยากได้หนังคืน: เอาไฟล์มาวางเองแล้วชี้ Platform.PrologueMovieUrl ไปที่ไฟล์นั้น
		FullScreenMovie_Finished();
	}

	private static Camera GetNGUICamera()
	{
		return GameObject.Find("NGUICamera").GetComponent<Camera>();
	}

	private static Camera GetPrologueCamera()
	{
		return GameObject.Find("PrologueCamera").GetComponent<Camera>();
	}

	private void FullScreenMovie_Finished()
	{
		SetNextState(State.CreatePlayer);
	}

	public void SkipPrologue()
	{
		Time.timeScale = 1f;
		GameSystem<CombatSystem>.Instance().CombatMode = false;
		StopPrologueSounds();
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		PrologueInteractionButtonGroupBase.HideInteractionButton();
		GameSystem<PrologueGuideSystem>.Instance().SkipPrologue();
		PrologueFinished();
		ToBeSkipped = false;
	}

	private void FinishCreateCharacter(string userName, string region, bool isMale, Shared.Player.Job job, Messages.PlayerDisplay display)
	{
		SoundManager.StopEvent(_createCharBgmId, _createCharBgmFadeOutDuration);
		_createCharBgmId = 0u;
		_createCharacterInfo.Name = userName;
		_createCharacterInfo.Region = region;
		_createCharacterInfo.IsMale = isMale;
		_createCharacterInfo.Job = job;
		_createCharacterInfo.Display = display;
		RefreshSessionToken();
		GameSystem<PlayGuideSystem>.Instance().BeginFlow("tutorial_play_guide_flow");
	}

	private void RefreshSessionToken()
	{
		SetNextState(State.RefreshSessionToken);
		Dictionary<string, string> fields = Platform.Instance.BuildSessionForm();
		RequestUrl("/sessions", fields, auth: false, HTTPMethods.Post);
	}

	private void RequestCreatePlayer()
	{
		SetNextState(State.RequestCreatePlayer);
		CreateCharacterInfo createCharacterInfo = _createCharacterInfo;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("region_id", "0");
		dictionary.Add("gender", (!createCharacterInfo.IsMale) ? "female" : "male");
		dictionary.Add("name", createCharacterInfo.Name);
		dictionary.Add("region", createCharacterInfo.Region);
		int job = (int)createCharacterInfo.Job;
		dictionary.Add("job", job.ToString());
		dictionary.Add("slot", GameManager.PlayerSlotIndex.ToString());
		Messages.PlayerDisplay display = createCharacterInfo.Display;
		PlayerDisplay playerDisplay = default(PlayerDisplay);
		playerDisplay.hair = display.Hair;
		playerDisplay.body_color = display.BodyColor;
		playerDisplay.head_color = display.HeadColor;
		playerDisplay.skin_color = display.SkinColor;
		playerDisplay.hair_color = display.HairColor;
		playerDisplay.eye_color = display.EyeColor;
		playerDisplay.lip_color = display.LipColor;
		playerDisplay.portrait = display.Portrait;
		playerDisplay.portrait_bg = display.PortraitBg;
		playerDisplay.portrait_bg_color = display.PortraitBgColor;
		playerDisplay.beard = display.Beard;
		playerDisplay.voice_type = display.VoiceType;
		playerDisplay.body_size = display.BodySize;
		PlayerDisplay data = playerDisplay;
		string value = Json.Write(data);
		dictionary.Add("model_info", value);
		RequestUrl("/players", dictionary, auth: true, HTTPMethods.Post);
	}

	public void StopPrologueSounds(float fadeOutDuration = 0f)
	{
		PrologueTunnelController prologueTunnelController = UnityEngine.Object.FindObjectOfType<PrologueTunnelController>();
		if ((bool)prologueTunnelController)
		{
			prologueTunnelController.StopBgm(fadeOutDuration);
		}
		SoundManager.StopEvent(_trainSoundId, fadeOutDuration);
		_trainSoundId = 0u;
	}

	private void OnGUI()
	{
		if (_curState == State.Loading && !(null == _fadeTexture))
		{
			GUI.color = new Color(1f, 1f, 1f, 1f);
			GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _fadeTexture);
		}
	}

	private void RequestUrl(string postFix, Dictionary<string, string> fields = null, bool auth = false, HTTPMethods method = HTTPMethods.Get)
	{
		string url = GameManager.GatewayUrl + postFix;
		_request = Http.Request(url, null, disableCache: true, auth, fields, method);
	}

	private void Update()
	{
		UpdateState();
		if (_curState < State.RefreshSessionToken)
		{
			return;
		}
		if (GetResponse(out var result))
		{
			if (result.Length > 0)
			{
				OnRequestSucceeded(result);
			}
		}
		else
		{
			OnRequestFailed(result);
		}
	}

	private bool GetResponse(out string result)
	{
		result = string.Empty;
		if (_request == null || _request.MoveNext())
		{
			return true;
		}
		if (_request.Response != null && _request.Response.IsSuccess)
		{
			result = _request.Response.DataAsText;
			_request = null;
			return true;
		}
		if (_request.Response != null)
		{
			result = (string.IsNullOrEmpty(_request.Response.DataAsText) ? _request.Response.Message : _request.Response.DataAsText);
		}
		_request = null;
		return false;
	}

	private void OnRequestSucceeded(string response)
	{
		JObject jObject = Json.Read<JObject>(response);
		if (jObject == null)
		{
			UIManager.MessageBox.Show(T._(_errorMsg) + "\n(Unknown)");
			SetNextState(State.CreatePlayer);
			return;
		}
		switch (_curState)
		{
		case State.RefreshSessionToken:
		{
			string text2 = jObject.Get<string>("session_token");
			if (text2 == null)
			{
				UIManager.MessageBox.Show(T._(_errorMsg) + "\n(Session Failed)");
				SetNextState(State.CreatePlayer);
			}
			else
			{
				GameManager.SessionToken = text2;
				RequestCreatePlayer();
			}
			break;
		}
		case State.RequestCreatePlayer:
		{
			string text = jObject.Get<string>("entity_id");
			if (text == null)
			{
				UIManager.MessageBox.Show(T._(_errorMsg) + "\n(Empty entity_id)");
				SetNextState(State.CreatePlayer);
				break;
			}
			// [แก้เอง] สร้างตัวละครสำเร็จ — จำไว้ว่าเครื่องนี้มีตัวละครแล้ว
			// (ครั้งหน้าเปิดเกมจะเข้าหน้าเลือกตัวละครแทนหน้าสร้าง)
			Preferences.SetString("durango_char_created", "1");
			NotifyRequestCreatePlayerFinished();
			GameManager.PlayerId = text;
			// [แก้เอง] อัปเดต PlayerContext ให้เป็นตัวละครที่เพิ่งสร้าง
			// (ไม่งั้น /sessions ครั้งต่อไปยังส่งตัวละครเดิมไปให้ server)
			Durango.Offline.PlayerContext local = Durango.Offline.Server._localPlayer;
			if (local != null)
			{
				// [แก้เอง] set ทับเสมอ (เดิมเช็ค "ของเดิมต้องไม่ว่าง" ทำให้ id ค้างของเก่า)
				local.AppearPlayer.EntityId = text;
				local.AppearPlayer.Name = _createCharacterInfo.Name;
				// [แก้เอง] เดิมจำแค่ id กับชื่อ — **หน้าตาที่ปั้นไว้หายทั้งดุ้น**
				// ตัวละครเลยเข้าเกมมาด้วย display ตั้งต้นของ context เก่า (ทุกคนหน้าตาเหมือนกันหมด)
				local.AppearPlayer.EntityType = (ushort)(_createCharacterInfo.IsMale ? 1000 : 1001);
				Messages.PlayerDisplay created = _createCharacterInfo.Display;
				created.EntityId = text;
				created.DefaultBody = (_createCharacterInfo.IsMale
					? "Models/PC/Male/Body/m_body_nothing.FBX"
					: "Models/PC/Female/Body/f_body_nothing.FBX");
				created.DefaultInner = (_createCharacterInfo.IsMale
					? "Models/PC/Male/Inner/m_inner_basic.FBX"
					: "Models/PC/Female/Inner/f_inner_basic.FBX");
				created.Body = created.DefaultBody;
				local.AppearPlayer.Display = created;
				// id ย่อยพวกนี้ถูกตั้งไว้ตอน Initialize ด้วย id เดิม ต้องตามไปเปลี่ยนด้วย
				local.AppearPlayer.Title.EntityId = text;
				local.AppearPlayer.Member.EntityId = text;
				local.AppearPlayer.Move.EntityId = text;
				local.AppearPlayer.Survival.EntityId = text;
				if (local.PlayerInfo != null)
				{
					local.PlayerInfo.PlayerEntityId = text;
					local.PlayerInfo.PlayerName = _createCharacterInfo.Name;
				}
				// [แก้เอง] เขียนลงดิสก์ทันที — เปิดเกมรอบหน้าจะได้ส่ง JSON เต็มให้ /sessions
				// (เดิมแก้แต่ในหน่วยความจำ พอปิดเกมข้อมูลตัวละครก็หาย server เห็นชื่อว่าง)
				local.Save();
			}
			PlayerPrefs.SetString("new_object", string.Empty);
			LoadingCurtainGroup.IsFirstPlayAfterCreatePlayer = true;
			SetNextState(State.Loading);
			break;
		}
		}
	}

	private void NotifyRequestCreatePlayerFinished()
	{
		if (_requestCreatePlayerFinished != null)
		{
			_requestCreatePlayerFinished();
			_requestCreatePlayerFinished = null;
		}
	}

	private void OnRequestFailed(string response)
	{
		SetNextState(State.CreatePlayer);
		string text = null;
		JObject jObject = Json.Read<JObject>(response);
		if (jObject != null && jObject.Get("error") is JObject token)
		{
			text = token.Get<string>("message");
		}
		if (string.IsNullOrEmpty(text))
		{
			text = T._(_errorMsg) + "\n(Request Failed)";
			if (Debug.isDebugBuild)
			{
				text = text + "\n" + response;
			}
		}
		UIManager.MessageBox.Show(text);
	}
}
