using System.Collections.Generic;
using Holoville.HOTween;
using PlayerExtensionsPrologue;
using UnityEngine;

public class TrainTrexController : KSingleton<TrainTrexController>
{
	[SerializeField]
	private string _trainReferenceName = "prologue_train_Trex_RefWithAnim";

	[SerializeField]
	private string _raptorJumpMotionName = "RaptorJump";

	[SerializeField]
	private string _trexAttackMotionName = "Train07_AttackingByTrex";

	[SerializeField]
	private string _raptorActorName = "prologue_raptor";

	[SerializeField]
	private Vector3 _raptorSpawnPos = Vector3.zero;

	[SerializeField]
	private string _raporAIObjectName = "PrologueRaptorPrefab";

	[SerializeField]
	private string _cartNameToHide = "train_07_cart";

	[SerializeField]
	private AnimationClip _playerMotionMale;

	[SerializeField]
	private AnimationClip _playerMotionFemale;

	[SerializeField]
	private Vector3 _playerTeleportPosition = Vector3.zero;

	[SerializeField]
	private float _playerTeleportYaw = -90f;

	[SerializeField]
	private float _playerInterpTime = 0.5f;

	[SerializeField]
	private List<MeshRenderer> _thunderMeshes = new List<MeshRenderer>();

	[SerializeField]
	private List<AnimationClip> _animationClips = new List<AnimationClip>();

	[SerializeField]
	private GameObject _collideDoor;

	[SerializeField]
	private string _mainDummyName = "MainDummy";

	[SerializeField]
	private GameObject _raptorActorObject;

	[SerializeField]
	private AnimatingProp _animatingProp;

	[SerializeField]
	private GameObject _trexTrainObject;

	[SerializeField]
	private GameObject _cartObjToHide;

	private List<Material> _thunderMaterials = new List<Material>();

	private bool _beginAttack;

	private CutsceneCameraController _cutsceneCameraController;

	private GameObject _raptorAIObject;

	public Vector3 PlayerTeleportPosition => _playerTeleportPosition;

	public string TrainReferenceName => _trainReferenceName;

	public List<AnimationClip> AnimationClips => _animationClips;

	public void FillAuto()
	{
		_raptorActorObject = KUtility.FindObjectByName(((Component)this).gameObject, _raptorActorName, includeInactive: true);
		_trexTrainObject = ((Component)((Component)this).GetComponentInChildren<Animation>()).gameObject;
		_animatingProp = ((Component)this).GetComponent<AnimatingProp>();
		_cartObjToHide = KUtility.FindObjectByName(((Component)this).gameObject, _cartNameToHide, includeInactive: true);
	}

	protected override void OnAwake()
	{
		MainCamera mainCamera = KSingleton<MainCamera>.Instance();
		_cutsceneCameraController = ((Component)mainCamera).GetComponent<CutsceneCameraController>();
		_raptorAIObject = GameObject.Find(_raporAIObjectName);
		_raptorAIObject.SetActive(false);
		_cartObjToHide.SetActive(false);
		InitThunderMaterials();
		SetThunderMeshIntensity(0f);
		_collideDoor.SetActive(false);
	}

	private void Start()
	{
		AnimationClip clip = ((!PlayerBehavior.LocalPlayer.IsMale) ? _playerMotionFemale : _playerMotionMale);
		PlayerBehavior.LocalPlayer.AddClip(clip);
	}

	public void PlayRaptorJump()
	{
		UIBase.HideUI(UIBase.UIFlag.Base, hide: true);
		BeginCutSceneCamera();
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.ReturnToSeatSuccess);
		BoneLookAtTarget component = ((Component)PlayerBehavior.LocalPlayer).gameObject.GetComponent<BoneLookAtTarget>();
		KSingleton<PrologueManager>.Instance().DelayedCall(delegate
		{
			PlayerBehavior.LocalPlayer.RotateToTarget(_raptorActorObject);
		}, 2f);
		if (Object.op_Implicit((Object)(object)component))
		{
			component.SetLookTarget(_raptorActorObject);
		}
		if (Object.op_Implicit((Object)(object)_trexTrainObject))
		{
			_animatingProp.Play(_raptorJumpMotionName, loop: false);
			((MonoBehaviour)this).Invoke("ActivateRaptorAI", _animatingProp.GetCurrentAnimationClipInfo().Length);
		}
		_collideDoor.SetActive(true);
	}

	public void ActivateRaptorAI()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		UIBase.HideUI(UIBase.UIFlag.Base, hide: false);
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.AttackRaptor);
		_raptorActorObject.SetActive(false);
		KSingleton<PlayerController>.Instance().EndMove();
		_raptorAIObject.SetActive(true);
		_raptorAIObject.transform.position = _raptorSpawnPos;
		EndCutSceneCamera();
		KSingleton<PrologueTunnelController>.Instance().BeginLightning();
		BoneLookAtTarget component = ((Component)PlayerBehavior.LocalPlayer).gameObject.GetComponent<BoneLookAtTarget>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.SetLookTarget(_raptorAIObject, bFindHead: true);
		}
	}

	public void OnBeginAutoBattle()
	{
		if (!_beginAttack)
		{
			GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.OnBeginAutoBattle);
			_beginAttack = true;
		}
	}

	public void PlayTrexCutScene()
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Expected O, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Expected O, but got Unknown
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Expected O, but got Unknown
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)_trexTrainObject))
		{
			ProgressGaugeGroup progressGaugeGroup = UIManager.FindScript<ProgressGaugeGroup>();
			if (Object.op_Implicit((Object)(object)progressGaugeGroup))
			{
				((Component)progressGaugeGroup).gameObject.SetActive(false);
			}
			GameSystem<CombatSystem>.Instance().CombatMode = false;
			UIBase.HideUI(UIBase.UIFlag.Base, hide: true);
			KSingleton<PrologueTunnelController>.Instance().StopLightning();
			_animatingProp.Play(_trexAttackMotionName, loop: false);
			_raptorActorObject.SetActive(true);
			BeginCutSceneCamera();
			KSingleton<PlayerController>.Instance().CutScenePlayMode = true;
			AnimationClip clip = ((!PlayerBehavior.LocalPlayer.IsMale) ? _playerMotionFemale : _playerMotionMale);
			PlayerBehavior.LocalPlayer.PlayClip(clip);
			TweenParms val = new TweenParms();
			val.Ease((EaseType)5);
			val.Prop("position", (object)_playerTeleportPosition);
			HOTween.To((object)((Component)PlayerBehavior.LocalPlayer).transform, _playerInterpTime, val);
			TweenParms val2 = new TweenParms();
			val2.Ease((EaseType)5);
			val2.Prop("localRotation", (object)Quaternion.Euler(0f, _playerTeleportYaw, 0f));
			HOTween.To((object)((Component)PlayerBehavior.LocalPlayer).transform, _playerInterpTime, val2);
			GameObject val3 = KUtility.FindObjectByName(((Component)this).gameObject, _mainDummyName, includeInactive: true);
			GameObject val4 = new GameObject("PlayerFloor");
			val4.transform.parent = val3.transform;
			val4.transform.position = Vector3.zero;
			val4.transform.rotation = Quaternion.identity;
			((Component)PlayerBehavior.LocalPlayer).gameObject.transform.parent = val4.transform;
		}
	}

	private void BeginCutSceneCamera()
	{
		if (Object.op_Implicit((Object)(object)_cutsceneCameraController))
		{
			_cutsceneCameraController.Begin(((Component)this).gameObject);
		}
	}

	private void EndCutSceneCamera()
	{
		if (Object.op_Implicit((Object)(object)_cutsceneCameraController))
		{
			_cutsceneCameraController.End();
		}
	}

	private void OnFinishCutScene()
	{
		KSingleton<PrologueOverlayGroup>.Instance().PlayWhiteOutEffect();
	}

	private void ShowObject(string objName)
	{
		GameObject val = KUtility.FindObjectByName(((Component)this).gameObject, objName, includeInactive: true);
		if (Object.op_Implicit((Object)(object)val))
		{
			val.SetActive(true);
		}
	}

	private void HideObject(string objName)
	{
		GameObject val = KUtility.FindObjectByName(((Component)this).gameObject, objName, includeInactive: true);
		if (Object.op_Implicit((Object)(object)val))
		{
			val.SetActive(false);
		}
	}

	public void InitThunderMaterials()
	{
		int count = _thunderMeshes.Count;
		for (int i = 0; i < count; i++)
		{
			Material[] materials = ((Renderer)_thunderMeshes[i]).materials;
			int num = materials.Length;
			for (int j = 0; j < num; j++)
			{
				if (((Object)materials[j]).name.ToLower().Contains("thunder"))
				{
					_thunderMaterials.Add(materials[j]);
				}
			}
		}
	}

	public void SetThunderMeshIntensity(float intensity)
	{
		int count = _thunderMaterials.Count;
		for (int i = 0; i < count; i++)
		{
			_thunderMaterials[i].SetFloat("_Intensity", intensity);
		}
	}

	private void Lightning()
	{
		KSingleton<PrologueTunnelController>.Instance().ForceLightningOnce();
	}
}
