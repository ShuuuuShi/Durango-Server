using System.Collections;
using UnityEngine;

public class NpcAI_KBike : MonoBehaviour
{
	[SerializeField]
	private string _beginMotion = "K_Bike_Begin";

	[SerializeField]
	private string _cprLoopMotion = "K_Bike_CPR";

	[SerializeField]
	private string _getUpMotion = "K_Bike_Getup";

	[SerializeField]
	private string _duringMotion = "K_Bike_During";

	[SerializeField]
	private string _endMotion = "K_Bike_End";

	[SerializeField]
	private AudioClip _bikeAppearSound;

	[SerializeField]
	private AudioClip _bikeLeaveSound;

	[SerializeField]
	private GameObject _cameraTarget;

	[SerializeField]
	private float _cameraChaseBeginDelay = 1f;

	[SerializeField]
	private float _cprDuration = 5f;

	[SerializeField]
	private GameObject _bikeLight;

	[SerializeField]
	private NightLight _nightLight;

	[SerializeField]
	public Vector3 _introPosFromPlayer = Vector3.zero;

	[SerializeField]
	public float _introYaw;

	[SerializeField]
	private float bikeSceneZoomBeginTime = 3f;

	[SerializeField]
	private float bikeSceneZoomRatio = 10f;

	[SerializeField]
	private float bikeSceneZoomingTime = 15f;

	private GameObject _playerTarget;

	private AnimalBehavior _animalBehavior;

	private GameObject _head;

	private GameObject _pelvis;

	private bool _isRestoringStandingKCutScene;

	public GameObject Head
	{
		get
		{
			if ((Object)(object)_head == (Object)null)
			{
				_head = KUtility.FindObjectByName(((Component)this).gameObject, "Bip001_Head");
			}
			return _head;
		}
	}

	public GameObject Pelvis
	{
		get
		{
			if ((Object)(object)_pelvis == (Object)null)
			{
				_pelvis = KUtility.FindObjectByName(((Component)this).gameObject, "Bip001_Pelvis");
			}
			return _pelvis;
		}
	}

	[ExposedInEditor("Intro 위치 새로 잡기")]
	public void RepositionToIntro()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = KUtility.FindObjectByName(((Component)PlayerBehavior.LocalPlayer).gameObject, "Bip001_Spine2");
		Vector3 position = val.transform.position + _introPosFromPlayer;
		position.y = 0f;
		((Component)this).transform.position = position;
		((Component)this).transform.rotation = Quaternion.Euler(0f, _introYaw, 0f);
	}

	private IEnumerator Start()
	{
		_animalBehavior = ((Component)this).GetComponent<AnimalBehavior>();
		_animalBehavior.EntityId = 666uL;
		while (!TerrainA6.IsPlayerInitialized)
		{
			yield return null;
		}
		if (TimeGauge.CheckTime(18f, 4f))
		{
			_bikeLight.SetActive(true);
		}
		else
		{
			_nightLight.IsLightOn = false;
		}
		RepositionToIntro();
		while (true)
		{
			_playerTarget = ((Component)PlayerBehavior.LocalPlayer).gameObject;
			if ((Object)(object)_playerTarget != (Object)null)
			{
				break;
			}
			yield return (object)new WaitForSeconds(1f);
		}
		BoneLookAtTarget lookAt = ((Component)this).GetComponent<BoneLookAtTarget>();
		if ((Object)null != (Object)(object)lookAt)
		{
			lookAt.AutoChangeTarget = false;
			lookAt.SetLookTarget(((Component)PlayerBehavior.LocalPlayer).gameObject, bFindHead: true);
		}
		_animalBehavior.SetAnimationCullingType((AnimationCullingType)0);
		_animalBehavior.SetServerSideRootMotionEnable(serverSideRootMotionEnabled: false);
		if (_isRestoringStandingKCutScene)
		{
			_animalBehavior.Play(_duringMotion);
			yield break;
		}
		_animalBehavior.Play(_beginMotion, loop: false);
		PlayerBehavior.LocalPlayer.PlayAnimation("Bike_Begin", 0f, 1f, forceTransition: true);
		SoundManager.Play(_bikeAppearSound, _animalBehavior.CurrentPosition);
		SkinnedMeshRenderer[] objects = ((Component)_animalBehavior).GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int j = 0; j < objects.Length; j++)
		{
			((Renderer)objects[j]).enabled = false;
		}
		yield return null;
		for (int i = 0; i < objects.Length; i++)
		{
			((Renderer)objects[i]).enabled = true;
		}
		yield return (object)new WaitForSeconds(_cameraChaseBeginDelay);
		KSingleton<CameraController>.Instance().SetCameraTarget(_cameraTarget);
		KUtility.DelayedCall((MonoBehaviour)(object)this, delegate
		{
			KSingleton<CameraController>.Instance().SetCameraTarget(zoomRatio: bikeSceneZoomRatio, target: _cameraTarget, cameraMoveTime: 0.3f, zoomTime: bikeSceneZoomingTime, forceRetarget: true);
		}, bikeSceneZoomBeginTime);
		yield return (object)new WaitForSeconds(_animalBehavior.CurAnimState.length - _cameraChaseBeginDelay);
	}

	public void BeginCPR()
	{
		((MonoBehaviour)this).StartCoroutine(CoBeginCPR());
	}

	private IEnumerator CoBeginCPR()
	{
		_animalBehavior.Play(_cprLoopMotion);
		PlayerBehavior.LocalPlayer.PlayAnimation("Bike_CPR", 0f, 1f, forceTransition: true);
		yield return (object)new WaitForSeconds(_cprDuration);
		_animalBehavior.Play(_getUpMotion);
		GameSystem<PlayGuideSystem>.Instance().Command.StandUp(5f, 30f);
		yield return (object)new WaitForSeconds(_animalBehavior.CurAnimState.length);
		_animalBehavior.Play(_duringMotion);
	}

	public void RestoreStandingKCutScene()
	{
		_isRestoringStandingKCutScene = true;
	}

	private void Update()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		GameObject pelvis = Pelvis;
		if ((Object)(object)pelvis != (Object)null)
		{
			KSingleton<OccluderVisibleManager>.Instance().PushRayCastPosition(pelvis.transform.position);
		}
	}

	public void EventRun()
	{
		((MonoBehaviour)this).StartCoroutine(CoRun());
	}

	private IEnumerator CoRun()
	{
		_animalBehavior.Play(_endMotion, loop: false);
		yield return (object)new WaitForSeconds(_animalBehavior.CurAnimState.length);
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	private void OnPlayLeaveSound()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		SoundManager.Play(_bikeLeaveSound, _animalBehavior.CurrentPosition);
	}
}
