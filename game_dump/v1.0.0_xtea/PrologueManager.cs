using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Holoville.HOTween;
using ItemSystem;
using L10N;
using MsgPack;
using Newtonsoft.Json.Linq;
using PlayerExtensionsPrologue;
using StatusEffectData;
using UnityEngine;

public class PrologueManager : KSingleton<PrologueManager>
{
	public enum State
	{
		None,
		CharacterSelect,
		TrainPlayBegin,
		PlayMovie,
		CreatePlayer,
		PrerequsiteLoading,
		Loading
	}

	private enum ProloguePhase
	{
		NormalTrain,
		AfterScoop,
		TrexCutScene
	}

	private struct PlayerDisplay
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
	private float _endFadeInDuration = 1f;

	[SerializeField]
	private Texture2D _fadeTexture;

	[SerializeField]
	private PrerequsiteLoader _prerequsiteLoader;

	[SerializeField]
	private string _movieUrl;

	[SerializeField]
	private AnimationClip _playerTunnelMotionMale;

	[SerializeField]
	private AnimationClip _playerTunnelMotionFemale;

	private LoadingCurtainGroup _loadingCurtainGroup;

	private CreateCharacterPanel _createCharacterPanel;

	private State _curState;

	private State _nextState;

	private WWW _requestWWW;

	private NPCFloatingGroup _npcFloatingGroup;

	private PrologueCharacterSelectGroup _prologueCharacterSelectUI;

	private float _defaultCameraZoom = -1f;

	private bool _isCharacterSelected;

	private ProloguePhase _curPhase;

	private PrologueAIPlayer _aiPlayer;

	public State CurrentState => _curState;

	public NPCFloatingGroup NPCFloatingGroup => (!((Object)(object)_npcFloatingGroup != (Object)null)) ? (_npcFloatingGroup = UIManager.FindScript<NPCFloatingGroup>()) : _npcFloatingGroup;

	private PrologueCharacterSelectGroup PrologueCharacterSelectUI => (!((Object)(object)_prologueCharacterSelectUI != (Object)null)) ? (_prologueCharacterSelectUI = UIManager.FindScript<PrologueCharacterSelectGroup>()) : _prologueCharacterSelectUI;

	public PrologueTrainManager TrainManager { get; private set; }

	public MessagePackObjectDictionary LastCostumeInfo { get; private set; }

	public bool LastGender { get; set; }

	public bool BeginIntreaction { get; set; }

	public GameObject TriggersGroup => _triggersGroup;

	public GameObject TrainCoverAisle1 => _trainCoverAisle1;

	public GameObject TrainCoverCabin1 => _trainCoverCabin1;

	public GameObject TrainModel => _trainModel;

	public static PrologueAIPlayer PlayerBattleAi
	{
		get
		{
			if ((Object)(object)KSingleton<PrologueManager>.Instance()._aiPlayer == (Object)null)
			{
				KSingleton<PrologueManager>.Instance()._aiPlayer = ((Component)KSingleton<PrologueManager>.Instance()).GetComponent<PrologueAIPlayer>();
			}
			return KSingleton<PrologueManager>.Instance()._aiPlayer;
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
				((Component)_prerequsiteLoader).gameObject.SetActive(false);
				KSingleton<AssetBundleManager>.Instance().StopBackgroundDownloading();
				break;
			case State.TrainPlayBegin:
			case State.PlayMovie:
				break;
			}
		}
	}

	private void StartBackgroundDownloading()
	{
		((Component)_prerequsiteLoader).gameObject.SetActive(true);
		_prerequsiteLoader.TotalCount = KSingleton<AssetBundleManager>.Instance().TotalFileCount;
		KSingleton<AssetBundleManager>.Instance().StartBackgroundDownloading(_prerequsiteLoader.ProgressChanged, _prerequsiteLoader.DetailedProgressChanged, AssetBundleManager_BackgroundLoadingCompleted);
	}

	protected override void OnAwake()
	{
		_trainModel = ((Component)((Component)KSingleton<PrologueTrainManager>.Instance()).transform.FindChild(_trainModelName)).gameObject;
		_trainCoverAisle1 = KUtility.FindObjectByName(_trainModel, "train_cover_short");
		_trainCoverCabin1 = KUtility.FindObjectByName(_trainModel, "train_cover_long");
		int count = _deactivateAtStartList.Count;
		for (int i = 0; i < count; i++)
		{
			if (Object.op_Implicit((Object)(object)_deactivateAtStartList[i]))
			{
				_deactivateAtStartList[i].SetActive(false);
			}
		}
		ApplyWetness(((Component)KSingleton<PrologueTrainManager>.Instance()).gameObject, 0f);
		((Component)UIManager.FindScript<PrologueLeftMenuListGroup>()).gameObject.SetActive(false);
	}

	private void Start()
	{
		KSingleton<CustomColorCorrectionEffect>.Instance().PauseTime = true;
		_createCharacterPanel = UIManager.FindScript<CreateCharacterPanel>();
		_loadingCurtainGroup = UIManager.FindScript<LoadingCurtainGroup>();
		EventDelegate.Add(_loadingCurtainGroup.ShowRegionInfoFinished, LoadingCurtainGroup_ShowRegionInfoFinished, oneShot: true);
		EventDelegate.Add(_loadingCurtainGroup.FadeOutFinished, LoadingCurtainGroup_FadeOutFinished, oneShot: true);
		KSingleton<PlayerController>.Instance().MakePrologueMode();
		KSingleton<PlayerController>.Instance().MoveSpeed = 200f;
		KSingleton<PlayerController>.Instance().MoveLock = true;
		((Component)PlayerBehavior.LocalPlayer).gameObject.SetActive(false);
		((Component)UIManager.FindScript<MoveArrowGroup>()).gameObject.SetActive(false);
		((Component)KSingleton<UIManager>.Instance().VirtualStick).gameObject.SetActive(false);
		TrainManager = Object.FindObjectOfType<PrologueTrainManager>();
		_triggersGroup.SetActive(true);
		PrologueCharacterSelectUI.OnSubmit = OnSubmitSelectCharacter;
		PrologueCharacterSelectUI.OnCloseSucceed += OnCancelSelectCharacter;
		PrologueCharacterSelectUI.OnChangeCostume = OnChangeCostumeSelectCharacter;
		_deactivateList.Add(KUtility.FindObjectByName(_trainModel, "train_01", includeInactive: true));
		_deactivateList.Add(KUtility.FindObjectByName(_trainModel, "train_02", includeInactive: true));
		_deactivateList.Add(KUtility.FindObjectByName(_trainModel, "train_03", includeInactive: true));
		_activateList.Add(KUtility.FindObjectByName(_trainModel, "train_05", includeInactive: true));
		_activateList.Add(KUtility.FindObjectByName(_trainModel, "train_06", includeInactive: true));
		KSingleton<GameManager>.Instance().ForceMainSceneLoadedPrologue();
		KSingleton<UIManager>.Instance().PlayerFloatingGroup.HideLocalPlayer();
	}

	private void SetNextState(State next)
	{
		_nextState = next;
	}

	private void UpdateState()
	{
		if (_nextState == _curState)
		{
			return;
		}
		_curState = _nextState;
		CheckBackgroundDownloading();
		switch (_curState)
		{
		case State.None:
			break;
		case State.CharacterSelect:
			GameSystem<PrologueGuideSystem>.Instance().Init();
			PrologueInteractionExtension.MakePrologueMode();
			((Component)UIManager.FindScript<PrologueLeftMenuListGroup>()).gameObject.SetActive(true);
			break;
		case State.TrainPlayBegin:
			break;
		case State.PlayMovie:
			break;
		case State.CreatePlayer:
		{
			Transform transform = ((Component)KSingleton<UIManager>.Instance().UIRoot).transform;
			int i = 0;
			for (int childCount = transform.childCount; i < childCount; i++)
			{
				UIRect component = ((Component)transform.GetChild(i)).GetComponent<UIRect>();
				if ((Object)(object)component != (Object)null && ((Component)component).gameObject.activeSelf)
				{
					component.alpha = 0f;
				}
			}
			((Component)UIManager.FindScript<PopupGroup>()).GetComponent<UIRect>().alpha = 1f;
			((Component)UIManager.FindScript<MessageBox>()).GetComponent<UIRect>().alpha = 1f;
			((Component)_createCharacterPanel).gameObject.SetActive(true);
			break;
		}
		case State.PrerequsiteLoading:
			if (AssetBundleManager.UseBundle)
			{
				WaitDownloadGroup waitDownloadGroup = UIManager.FindScript<WaitDownloadGroup>();
				if ((Object)(object)waitDownloadGroup != (Object)null)
				{
					((Component)waitDownloadGroup).gameObject.SetActive(true);
				}
				((Component)_prerequsiteLoader).gameObject.SetActive(true);
				_prerequsiteLoader.TotalCount = KSingleton<AssetBundleManager>.Instance().PrerequsitesCount;
				KSingleton<AssetBundleManager>.Instance().StartPrerequisiteLoading(_prerequsiteLoader.ProgressChanged, _prerequsiteLoader.DetailedProgressChanged, AssetBundleManager_PrerequisiteLoadingCompleted);
			}
			else
			{
				SetNextState(State.Loading);
			}
			break;
		case State.Loading:
			AuthMenuGroup.StartByPrologue = true;
			KSingleton<GameManager>.Instance().MoveToTitle();
			break;
		}
	}

	private void LoadingCurtainGroup_ShowRegionInfoFinished()
	{
		_loadingCurtainGroup.EndLoading();
	}

	private void LoadingCurtainGroup_FadeOutFinished()
	{
		if (_curState == State.None)
		{
			SetNextState(State.CharacterSelect);
		}
	}

	private void AssetBundleManager_BackgroundLoadingCompleted(bool succeed)
	{
		if (succeed)
		{
			((Component)_prerequsiteLoader).gameObject.SetActive(false);
		}
		else if (_curState < State.CreatePlayer)
		{
			StartBackgroundDownloading();
		}
	}

	private void AssetBundleManager_PrerequisiteLoadingCompleted(bool succeed)
	{
		((Component)_prerequsiteLoader).gameObject.SetActive(false);
		WaitDownloadGroup waitDownloadGroup = UIManager.FindScript<WaitDownloadGroup>();
		if ((Object)(object)waitDownloadGroup != (Object)null)
		{
			((Component)waitDownloadGroup).gameObject.SetActive(false);
		}
		if (succeed)
		{
			SetNextState(State.Loading);
			return;
		}
		UIManager.MessageBox.Show(T._("오류: 필수 데이터 받기가 실패하였습니다.\n 로그인 화면으로 돌아갑니다."), (Action)delegate
		{
			KSingleton<GameManager>.Instance().MoveToTitle();
		});
	}

	private Texture FindInsideLitSphereTexture(Texture currentTexture)
	{
		if (Object.op_Implicit((Object)(object)currentTexture))
		{
			string text = ((Object)currentTexture).name + "_insidetrain";
			int count = _litSphereTextures.Count;
			for (int i = 0; i < count; i++)
			{
				if (text == ((Object)_litSphereTextures[i]).name)
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
			if (!((Object)(object)texture == (Object)null))
			{
				Texture val = FindInsideLitSphereTexture(texture);
				if (Object.op_Implicit((Object)(object)val))
				{
					materials[i].SetTexture("_LitSphereTex", val);
				}
			}
		}
	}

	public void MakeLitSphereOverride(Transform meshObjectTransform)
	{
		SkinnedMeshRenderer[] componentsInChildren = ((Component)meshObjectTransform).GetComponentsInChildren<SkinnedMeshRenderer>();
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			MakeMaterialsLitSphereOverride(((Renderer)componentsInChildren[i]).materials);
		}
	}

	public void BeginPlayer(NPCActorBehavior actor, Vector3 destPos)
	{
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Expected O, but got Unknown
		KSingleton<PlayerController>.Instance().MoveLock = false;
		InteractionGroupHelper.HideInteractionButton();
		KSingleton<PlayerController>.Instance().StopMove();
		KSingleton<PlayerController>.Instance().MoveSpeed = 500f;
		((Component)PlayerBehavior.LocalPlayer).gameObject.SetActive(true);
		((Component)UIManager.FindScript<MoveArrowGroup>()).gameObject.SetActive(true);
		((Component)KSingleton<UIManager>.Instance().VirtualStick).gameObject.SetActive(true);
		Object.Destroy((Object)(object)((Component)PlayerBehavior.LocalPlayer).gameObject);
		bool isMale = actor.IsMale;
		PlayerBehavior playerBehavior = KSingleton<PlayerManager>.Instance().MakePlayerObject(isMale, destPos, 0uL);
		((Object)((Component)playerBehavior).gameObject).name = "Player";
		playerBehavior.EntityId = GameManager.PlayerId;
		playerBehavior.PlayerName = string.Empty;
		playerBehavior.Teleport(destPos);
		((Component)playerBehavior).transform.localRotation = ((Component)actor).transform.localRotation;
		Quaternion localRotation = ((Component)actor).transform.localRotation;
		playerBehavior.TargetYaw = ((Quaternion)(ref localRotation)).eulerAngles.y;
		playerBehavior.MakePrologueMode();
		MessagePackObjectDictionary val = actor.AllocCostumeDict();
		PlayerManager.SetCostumeFromDict(playerBehavior, val);
		PlayerBehavior.LocalPlayer = playerBehavior;
		LastCostumeInfo = new MessagePackObjectDictionary((IDictionary<MessagePackObject, MessagePackObject>)val);
		LastGender = actor.IsMale;
		KSingleton<CameraController>.Instance().ResetCameraTarget();
		SetNextState(State.TrainPlayBegin);
	}

	public void AddStatusEffects()
	{
		GameSystem<PrologueGuideSystem>.Instance().RemoveClickHelper();
		GameSystem<PrologueToDoListSystem>.Instance().SetCompleted("move_character", completed: true);
		GameSystem<PlayerStatusEffectSystem>.Instance().ClearStatusEffectPrologue();
		((MonoBehaviour)this).StartCoroutine(CoAddStatusEffects());
	}

	private IEnumerator CoAddStatusEffects()
	{
		AddStatusEffect("hungry", T._("배고픔"), T._("배가 출출하다."), "icon_se_satietylow");
		yield return (object)new WaitForSeconds(0.5f);
		AddStatusEffect("thirst", T._("갈증"), T._("목이 마르다."), "icon_se_waterlow");
	}

	private void AddStatusEffect(string id, string statusEffectName, string desc, string icon)
	{
		StatusEffect effect = new StatusEffect(id, statusEffectName, desc, icon);
		GameSystem<PlayerStatusEffectSystem>.Instance().AddStatusEffectPrologue(effect);
	}

	public void ZoomIn()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		if (_defaultCameraZoom < 0f)
		{
			_defaultCameraZoom = KSingleton<MainCamera>.Instance().Zoom;
		}
		TweenParms val = new TweenParms();
		val.Prop("Zoom", (object)KSingleton<MainCamera>.Instance().MaxZoom);
		val.Ease((EaseType)0);
		HOTween.To((object)KSingleton<MainCamera>.Instance(), 0.5f, val);
	}

	public void ZoomToDefault()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Expected O, but got Unknown
		if (_defaultCameraZoom < 0f)
		{
			_defaultCameraZoom = KSingleton<MainCamera>.Instance().Zoom;
		}
		TweenParms val = new TweenParms();
		val.Prop("Zoom", (object)_defaultCameraZoom);
		val.Ease((EaseType)0);
		HOTween.To((object)KSingleton<MainCamera>.Instance(), 0.25f, val);
	}

	public void DelayedCall(Action func, float delay)
	{
		KUtility.DelayedCall((MonoBehaviour)(object)this, func, delay);
	}

	private void OnSubmitSelectCharacter()
	{
		TriggerPrologueSelectCharacter targetComponent = GameSystem<InteractionSystem>.Instance().LastInteractionTarget.GetTargetComponent<TriggerPrologueSelectCharacter>();
		if (Object.op_Implicit((Object)(object)targetComponent))
		{
			targetComponent.OnSubmitOnPrologue();
			targetComponent.OnUnselectedOnPrologue();
			_isCharacterSelected = true;
		}
		ZoomToDefault();
		PrologueCharacterSelectUI.Close();
	}

	private void OnCancelSelectCharacter()
	{
		TriggerPrologueSelectCharacter targetComponent = GameSystem<InteractionSystem>.Instance().LastInteractionTarget.GetTargetComponent<TriggerPrologueSelectCharacter>();
		if (Object.op_Implicit((Object)(object)targetComponent))
		{
			targetComponent.OnUnselectedOnPrologue();
		}
		if (_isCharacterSelected)
		{
			InteractionButtonGroup.ClearInteractions();
		}
		ZoomToDefault();
		KSingleton<CameraController>.Instance().ResetCamera();
	}

	private void OnChangeCostumeSelectCharacter()
	{
		NPCActorBehavior targetComponent = GameSystem<InteractionSystem>.Instance().LastInteractionTarget.GetTargetComponent<NPCActorBehavior>();
		if (Object.op_Implicit((Object)(object)targetComponent))
		{
			string random = ResourceSingleton<EquipmentTable>.Instance().GetRandom(EquipmentTable.Category.Hair, targetComponent.IsMale);
			targetComponent.ChangeCostume(CharacterCostume.CostumeType.Hair, random);
			targetComponent.RandomCostumeColors(targetComponent.GetCostumeName("Body"), targetComponent.GetCostumeName("Head"));
		}
	}

	private void OnThanksToYou()
	{
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.ThanksToYou);
	}

	private void OnGetEncyclopedia()
	{
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.GetEncyclopedia);
		KSingleton<DialogsManager>.Instance()._colliderWithKid.SetActive(false);
	}

	private void OnFinishKidSitDown()
	{
	}

	private void OnFoundCafeteria()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		KSingleton<UIManager>.Instance().PlayGuideHelper.SetArrowTarget(Vector3.zero);
	}

	public void DoPhase2(bool skipTunnelEffect = false)
	{
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if (_curPhase >= ProloguePhase.AfterScoop)
		{
			return;
		}
		_curPhase = ProloguePhase.AfterScoop;
		BeginIntreaction = true;
		GameSystem<PrologueGuideSystem>.Instance().ForceClearGuideMsg();
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		InteractionGroupHelper.HideInteractionButton();
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.AfterEatAndDrink);
		int count = _deactivateList.Count;
		for (int i = 0; i < count; i++)
		{
			if (Object.op_Implicit((Object)(object)_deactivateList[i]))
			{
				_deactivateList[i].SetActive(false);
			}
		}
		count = _activateList.Count;
		for (int j = 0; j < count; j++)
		{
			if (Object.op_Implicit((Object)(object)_activateList[j]))
			{
				_activateList[j].SetActive(true);
				_activateList[j].transform.localPosition = Vector3.zero;
			}
		}
		PrologueTunnelController prologueTunnelController = Object.FindObjectOfType<PrologueTunnelController>();
		if (Object.op_Implicit((Object)(object)prologueTunnelController))
		{
			prologueTunnelController.TunnelEffect(skipTunnelEffect);
		}
	}

	public void PlayFrightenMotion()
	{
		int count = _triggerOnPhase2List.Count;
		for (int i = 0; i < count; i++)
		{
			if (Object.op_Implicit((Object)(object)_triggerOnPhase2List[i]))
			{
				_triggerOnPhase2List[i].BeginEvent();
			}
		}
		AnimationClip clip = ((!PlayerBehavior.LocalPlayer.IsMale) ? _playerTunnelMotionFemale : _playerTunnelMotionMale);
		PlayerBehavior.LocalPlayer.PlayClip(clip);
	}

	public void BeginRaining()
	{
		KSingleton<PrologueTrainManager>.Instance().BeginRaining();
		ApplyWetness(((Component)KSingleton<PrologueTrainManager>.Instance()).gameObject, _wetnessAtRainy);
		ApplyWetness(((Component)KSingleton<TrainTrexController>.Instance()).gameObject, _wetnessAtRainy);
	}

	private static void ApplyWetness(GameObject obj, float wetness)
	{
		Renderer[] componentsInChildren = obj.GetComponentsInChildren<Renderer>(true);
		int num = componentsInChildren.Length;
		for (int i = 0; i < num; i++)
		{
			int num2 = componentsInChildren[i].materials.Length;
			for (int j = 0; j < num2; j++)
			{
				Material val = componentsInChildren[i].materials[j];
				string name = ((Object)val.shader).name;
				if (!name.Contains("TrainCover_Exterior_Transparent") && !name.Contains("LitSphere"))
				{
					continue;
				}
				if (wetness <= 0f)
				{
					val.DisableKeyword("WETNESS_ON");
				}
				else if (val.HasProperty("_Wetness"))
				{
					val.EnableKeyword("WETNESS_ON");
					if (name.Contains("TrainCover_Exterior_Transparent"))
					{
						float @float = val.GetFloat("_MaxWetness");
						val.SetFloat("_Wetness", wetness * @float);
					}
					else
					{
						val.SetFloat("_Wetness", wetness);
					}
				}
			}
		}
	}

	private void SetLookAround()
	{
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide("LookAround");
	}

	public void DoGetAxe()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		PlayerBattleAi.GetScared();
		PlayerBehavior.LocalPlayer.ChangeEquipment("Models/Equipment/Melee/axe_onehand_emergency_axe1.FBX");
		PlayerBehavior.LocalPlayer.ChangeCostumeColor(CharacterCostume.CostumeType.Equipment, new ItemColor(KUtility.ToColor("B40000"), KUtility.ToColor("7C7C7C"), KUtility.ToColor("A68E5A")));
		ItemData itemData = new ItemData();
		itemData.Id = 0uL;
		GameSystem<EquipSystem>.Instance().EquipItem("main", itemData);
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.GetAxeSuccess);
		_colliderAtSpearGet.SetActive(false);
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
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		int count = _prologueEndDeactivateList.Count;
		for (int i = 0; i < count; i++)
		{
			if (Object.op_Implicit((Object)(object)_prologueEndDeactivateList[i]))
			{
				_prologueEndDeactivateList[i].SetActive(false);
			}
		}
		UIBase.HideUI(UIBase.UIFlag.CoveredByClosable, hide: true);
		KSingleton<UIManager>.Instance().PlayGuideHelper.SetArrowTarget(Vector3.zero);
		((Component)UIManager.FindScript<PrologueLeftMenuListGroup>()).gameObject.SetActive(false);
		PlayerBehavior.LocalPlayer.UndoPrologueMode();
		((Component)PlayerBehavior.LocalPlayer).gameObject.SetActive(false);
		PrologueInteractionExtension.UndoPrologueMode();
		SetNextState(State.PlayMovie);
		Camera nGUICamera = GetNGUICamera();
		nGUICamera.clearFlags = (CameraClearFlags)2;
		nGUICamera.backgroundColor = Color.black;
		DelayedCall(delegate
		{
			FullScreenMovieGroup fullScreenMovieGroup = UIManager.FindScript<FullScreenMovieGroup>();
			fullScreenMovieGroup.Play(_movieUrl);
			fullScreenMovieGroup.Finished += FullScreenMovie_Finished;
		}, 1f);
	}

	private static Camera GetNGUICamera()
	{
		return GameObject.Find("NGUICamera").GetComponent<Camera>();
	}

	private void FullScreenMovie_Finished()
	{
		Camera nGUICamera = GetNGUICamera();
		nGUICamera.clearFlags = (CameraClearFlags)3;
		SetNextState(State.CreatePlayer);
	}

	public void SkipPrologue()
	{
		Time.timeScale = 1f;
		GameSystem<CombatSystem>.Instance().CombatMode = false;
		GameObject.Find("ENVSound").GetComponent<AudioSource>().volume = 0f;
		GameObject.Find("BGMSound").GetComponent<AudioSource>().volume = 0f;
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		InteractionGroupHelper.HideInteractionButton();
		GameSystem<PrologueGuideSystem>.Instance().SkipPrologue();
		PrologueFinished();
	}

	private static string[] ToJsonColorArray(ItemColor colors)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		string[] array = new string[colors.Count];
		for (int i = 0; i < colors.Count; i++)
		{
			array[i] = KUtility.ToString(colors[i]);
		}
		return array;
	}

	public void FinishCreateCharacter(bool isMale, int job, string playerName, string hairName, string beardName, ItemColor[] colors, PortraitBuilder.Argument portrait, int voiceType, float bodySize)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		WWWForm val = new WWWForm();
		val.AddField("region_id", 0);
		val.AddField("gender", (!isMale) ? "female" : "male");
		val.AddField("name", playerName, Encoding.UTF8);
		val.AddField("job", job);
		PlayerDisplay playerDisplay = default(PlayerDisplay);
		playerDisplay.hair = hairName;
		playerDisplay.body_color = ToJsonColorArray(colors[0]);
		playerDisplay.head_color = ToJsonColorArray(colors[1]);
		playerDisplay.skin_color = KUtility.ToString(colors[2][0]);
		playerDisplay.hair_color = KUtility.ToString(colors[3][0]);
		playerDisplay.eye_color = KUtility.ToString(colors[5][0]);
		playerDisplay.lip_color = KUtility.ToString(colors[6][0]);
		playerDisplay.portrait = portrait.Type;
		playerDisplay.portrait_bg = portrait.Background;
		playerDisplay.portrait_bg_color = KUtility.ToString(portrait.BgColor);
		playerDisplay.beard = beardName;
		playerDisplay.voice_type = voiceType;
		playerDisplay.body_size = bodySize;
		PlayerDisplay data = playerDisplay;
		string text = KUtility.SerializeJson(data);
		val.AddField("model_info", text, Encoding.UTF8);
		((MonoBehaviour)this).StartCoroutine(CoRequestUrl("/players", val, withSessionToken: true));
	}

	private void OnGUI()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		if (_curState == State.Loading && !((Object)null == (Object)(object)_fadeTexture))
		{
			GUI.color = new Color(1f, 1f, 1f, 1f);
			GUI.DrawTexture(new Rect(0f, 0f, (float)Screen.width, (float)Screen.height), (Texture)(object)_fadeTexture);
		}
	}

	private IEnumerator CoRequestUrl(string postFix, WWWForm form = null, bool withSessionToken = false)
	{
		byte[] data = ((form == null) ? null : form.data);
		Dictionary<string, string> headers = ((form == null) ? new Dictionary<string, string>() : form.headers);
		headers["Accept-Language"] = LocalizeSystem.Locale;
		if (withSessionToken)
		{
			headers["Authorization"] = KSingleton<GameManager>.Instance().SessionToken;
		}
		_requestWWW = new WWW(KSingleton<GameManager>.Instance().GatewayUrl + postFix, data, headers);
		yield return _requestWWW;
	}

	private void Update()
	{
		UpdateState();
		if (_curState < State.CreatePlayer)
		{
			return;
		}
		State curState = _curState;
		if (GetResponse(out var result))
		{
			if (result.Length > 0)
			{
				OnRequestSucceed(result);
			}
		}
		else
		{
			OnRequestFail(result);
		}
	}

	private bool GetResponse(out string result)
	{
		try
		{
			if (_requestWWW != null)
			{
				if (!string.IsNullOrEmpty(_requestWWW.error))
				{
					result = _requestWWW.text;
					_requestWWW.Dispose();
					_requestWWW = null;
					return false;
				}
				if (_requestWWW.isDone)
				{
					result = _requestWWW.text;
					_requestWWW.Dispose();
					_requestWWW = null;
					return true;
				}
			}
		}
		catch (Exception ex)
		{
			_requestWWW = null;
			result = ex.Message;
			return false;
		}
		result = string.Empty;
		return true;
	}

	private void OnRequestSucceed(string response)
	{
		JObject jObject = KUtility.ParseJson<JObject>(response);
		if (jObject == null)
		{
			UIManager.MessageBox.Show(T._("오류: 알 수 없는 서버 오류입니다. 다시 시도해 주세요."));
			return;
		}
		State curState = _curState;
		if (curState == State.CreatePlayer)
		{
			string text = jObject.Get<string>("entity_id");
			if (text == null)
			{
				UIManager.MessageBox.Show(T._("오류: 플레이어 id를 받아올 수 없습니다."));
				return;
			}
			((Component)_createCharacterPanel).gameObject.SetActive(false);
			KSingleton<GameManager>.Instance().SetPlayerId(text);
			PlayerPrefs.SetString("play_guide_progress", string.Empty);
			PlayerPrefs.SetString("new_object", string.Empty);
			LoadingCurtainGroup.IsFirstPlayAfterCreatePlayer = true;
			SetNextState(State.PrerequsiteLoading);
		}
	}

	private void OnRequestFail(string response)
	{
		State curState = _curState;
		if (curState == State.CreatePlayer)
		{
			string comment = response;
			JObject jObject = KUtility.ParseJson<JObject>(response);
			if (jObject != null)
			{
				comment = ((!(jObject.Get("error") is JObject token)) ? response : token.Get<string>("message"));
			}
			UIManager.MessageBox.Show(comment);
		}
	}
}
