using System.Collections.Generic;
using Durango.Model;
using Durango.Render;
using Durango.Render.Camera;
using Durango.UI;
using Durango.UI.Control;
using Durango.UI.Prologue;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class TrainTrexController : Singleton<TrainTrexController>
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
	private AnimatingModel _animatingProp;

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
		_raptorActorObject = KUtility.FindObjectByName(base.gameObject, _raptorActorName, includeInactive: true);
		_trexTrainObject = GetComponentInChildren<Animation>().gameObject;
		_animatingProp = GetComponent<AnimatingModel>();
		_cartObjToHide = KUtility.FindObjectByName(base.gameObject, _cartNameToHide, includeInactive: true);
	}

	public void Initialize()
	{
		MainCamera mainCamera = Singleton<MainCamera>.Instance();
		_cutsceneCameraController = mainCamera.GetComponent<CutsceneCameraController>();
		_raptorAIObject = GameObject.Find(_raporAIObjectName);
		_raptorAIObject.SetActive(value: false);
		_cartObjToHide.SetActive(value: false);
		InitThunderMaterials();
		SetThunderMeshIntensity(0f);
		_collideDoor.SetActive(value: false);
	}

	private void Start()
	{
		AnimationClip clip = ((!PlayerBehavior.LocalPlayer.IsMale) ? _playerMotionFemale : _playerMotionMale);
		PlayerBehavior.LocalPlayer.AddClip(clip);
	}

	public void PlayRaptorJump()
	{
		VisibleController.Hide(VisibleType.Base, hide: true);
		BeginCutSceneCamera();
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.ReturnToSeatSuccess);
		BoneLookAtTarget component = PlayerBehavior.LocalPlayer.gameObject.GetComponent<BoneLookAtTarget>();
		Singleton<PrologueManager>.Instance().DelayedCall(delegate
		{
			Singleton<PlayerController>.Instance().RotateToObject(_raptorActorObject);
		}, 2f);
		if ((bool)component)
		{
			component.SetLookTarget(_raptorActorObject);
		}
		if ((bool)_trexTrainObject)
		{
			_animatingProp.Play(_raptorJumpMotionName, loop: false);
			Invoke("ActivateRaptorAI", _animatingProp.GetCurrentAnimationClipInfo().Length);
		}
		_collideDoor.SetActive(value: true);
	}

	public void ActivateRaptorAI()
	{
		VisibleController.Hide(VisibleType.Base, hide: false);
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.AttackRaptor);
		_raptorActorObject.SetActive(value: false);
		_raptorAIObject.SetActive(value: true);
		_raptorAIObject.transform.position = _raptorSpawnPos;
		EndCutSceneCamera();
		Singleton<PrologueTunnelController>.Instance().BeginLightning();
		BoneLookAtTarget component = PlayerBehavior.LocalPlayer.gameObject.GetComponent<BoneLookAtTarget>();
		if ((bool)component)
		{
			component.SetLookTarget(_raptorAIObject, findHead: true);
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
		if ((bool)_trexTrainObject)
		{
			ProgressGaugeGroup progressGaugeGroup = UIManager.FindScript<ProgressGaugeGroup>();
			if ((bool)progressGaugeGroup)
			{
				progressGaugeGroup.gameObject.SetActive(value: false);
			}
			GameSystem<CombatSystem>.Instance().CombatMode = false;
			VisibleController.Hide(VisibleType.Base, hide: true);
			Singleton<PrologueTunnelController>.Instance().StopLightning();
			_animatingProp.Play(_trexAttackMotionName, loop: false);
			_raptorActorObject.SetActive(value: true);
			BeginCutSceneCamera();
			Singleton<PlayerController>.Instance().CutScenePlayMode = true;
			AnimationClip clip = ((!PlayerBehavior.LocalPlayer.IsMale) ? _playerMotionFemale : _playerMotionMale);
			PlayerBehavior.LocalPlayer.PlayClip(clip);
			TweenPosition tweenPosition = TweenPosition.Begin(PlayerBehavior.LocalPlayer.gameObject, _playerInterpTime, _playerTeleportPosition);
			tweenPosition.method = UITweener.Method.EaseOut;
			tweenPosition.PlayForward();
			TweenRotation tweenRotation = TweenRotation.Begin(PlayerBehavior.LocalPlayer.gameObject, _playerInterpTime, Quaternion.Euler(0f, _playerTeleportYaw, 0f));
			tweenRotation.method = UITweener.Method.EaseOut;
			tweenRotation.PlayForward();
			GameObject gameObject = KUtility.FindObjectByName(base.gameObject, _mainDummyName, includeInactive: true);
			GameObject gameObject2 = new GameObject("PlayerFloor");
			gameObject2.transform.parent = gameObject.transform;
			gameObject2.transform.position = Vector3.zero;
			gameObject2.transform.rotation = Quaternion.identity;
			PlayerBehavior.LocalPlayer.gameObject.transform.parent = gameObject2.transform;
			Singleton<ContactShadowManager>.Instance().Remove(PlayerBehavior.LocalPlayer.gameObject);
		}
	}

	private void BeginCutSceneCamera()
	{
		if ((bool)_cutsceneCameraController)
		{
			_cutsceneCameraController.Begin(base.gameObject);
		}
	}

	private void EndCutSceneCamera()
	{
		if ((bool)_cutsceneCameraController)
		{
			_cutsceneCameraController.End();
		}
	}

	private void OnFinishCutScene()
	{
		Singleton<PrologueOverlayGroup>.Instance().PlayWhiteOutEffect();
	}

	private void ShowObject(string objName)
	{
		GameObject gameObject = KUtility.FindObjectByName(base.gameObject, objName, includeInactive: true);
		if ((bool)gameObject)
		{
			gameObject.SetActive(value: true);
		}
	}

	private void HideObject(string objName)
	{
		GameObject gameObject = KUtility.FindObjectByName(base.gameObject, objName, includeInactive: true);
		if ((bool)gameObject)
		{
			gameObject.SetActive(value: false);
		}
	}

	public void InitThunderMaterials()
	{
		int count = _thunderMeshes.Count;
		for (int i = 0; i < count; i++)
		{
			Material[] materials = _thunderMeshes[i].materials;
			int num = materials.Length;
			for (int j = 0; j < num; j++)
			{
				if (materials[j].name.ToLower().Contains("thunder"))
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
		Singleton<PrologueTunnelController>.Instance().ForceLightningOnce();
	}
}
