using System.Collections;
using Durango.Environment;
using Durango.Model;
using Durango.Render.Camera;
using Durango.Terrain;
using Durango.Utils;
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
	private SoundEventType _bikeAppearAudio;

	[SerializeField]
	private SoundEventType _bikeLeaveAudio;

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
			if (_head == null)
			{
				_head = KUtility.FindObjectByName(base.gameObject, "Bip001_Head");
			}
			return _head;
		}
	}

	public GameObject Pelvis
	{
		get
		{
			if (_pelvis == null)
			{
				_pelvis = KUtility.FindObjectByName(base.gameObject, "Bip001_Pelvis");
			}
			return _pelvis;
		}
	}

	[ExposedInEditor("Intro 위치 새로 잡기")]
	public void RepositionToIntro()
	{
		GameObject gameObject = KUtility.FindObjectByName(PlayerBehavior.LocalPlayer.gameObject, "Bip001_Spine2");
		Vector3 position = gameObject.transform.position + _introPosFromPlayer;
		position.y = 0f;
		base.transform.position = position;
		base.transform.rotation = Quaternion.Euler(0f, _introYaw, 0f);
	}

	private IEnumerator Start()
	{
		_animalBehavior = GetComponent<AnimalBehavior>();
		_animalBehavior.EntityId = "666";
		while (!TerrainBase.IsPlayerInitialized)
		{
			yield return null;
		}
		if (TimeGauge.CheckTime(18f, 4f))
		{
			_bikeLight.SetActive(value: true);
		}
		else
		{
			_nightLight.IsLightOn = false;
		}
		RepositionToIntro();
		while (true)
		{
			_playerTarget = PlayerBehavior.LocalPlayer.gameObject;
			if (_playerTarget != null)
			{
				break;
			}
			yield return new WaitForSeconds(1f);
		}
		BoneLookAtTarget lookAt = GetComponent<BoneLookAtTarget>();
		if (null != lookAt)
		{
			lookAt.AutoChangeTarget = false;
			lookAt.SetLookTarget(PlayerBehavior.LocalPlayer.gameObject, findHead: true);
		}
		_animalBehavior.SetAnimationCullingType(AnimationCullingType.AlwaysAnimate);
		_animalBehavior.SetActivateRootMotion(active: false);
		if (_isRestoringStandingKCutScene)
		{
			_animalBehavior.Play(_duringMotion);
			yield break;
		}
		_animalBehavior.Play(_beginMotion, loop: false);
		PlayerBehavior.LocalPlayer.Anim.Stop();
		PlayerBehavior.LocalPlayer.PlayMotionForcely("Bike_Begin");
		SoundManager.PlayEvent(_bikeAppearAudio, SoundPosition.Chase(_animalBehavior.gameObject));
		SkinnedMeshRenderer[] objects = _animalBehavior.GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < objects.Length; i++)
		{
			objects[i].enabled = false;
		}
		yield return null;
		for (int j = 0; j < objects.Length; j++)
		{
			objects[j].enabled = true;
		}
		yield return new WaitForSeconds(_cameraChaseBeginDelay);
		Singleton<CameraController>.Instance().Target(_cameraTarget, 0.3f).Zoom(0.7f, 0.3f)
			.Delay(bikeSceneZoomBeginTime)
			.Target(null, bikeSceneZoomingTime)
			.Offset(Vector3.down * 70f, bikeSceneZoomingTime)
			.Zoom(bikeSceneZoomRatio, bikeSceneZoomingTime, NgInterpolate.EaseType.EaseInQuart);
	}

	public void BeginCPR()
	{
		StartCoroutine(CoBeginCPR());
	}

	private IEnumerator CoBeginCPR()
	{
		_animalBehavior.Play(_cprLoopMotion);
		PlayerController.MotionUpdater.Motion("Bike_CPR", 0f, 1f, forceTransition: true);
		yield return new WaitForSeconds(_cprDuration);
		_animalBehavior.Play(_getUpMotion);
		GameSystem<PlayGuideSystem>.Instance().Command.StandUp();
		yield return new WaitForSeconds(_animalBehavior.CurAnimState.length);
		_animalBehavior.Play(_duringMotion);
	}

	public void RestoreStandingKCutScene()
	{
		_isRestoringStandingKCutScene = true;
	}

	private void Update()
	{
		GameObject pelvis = Pelvis;
		if (pelvis != null)
		{
			Singleton<OccluderVisibleManager>.Instance().PushRayCastPosition(pelvis.transform.position);
		}
	}

	public void EventRun()
	{
		StartCoroutine(CoRun());
	}

	private IEnumerator CoRun()
	{
		_animalBehavior.Play(_endMotion, loop: false);
		yield return new WaitForSeconds(_animalBehavior.CurAnimState.length);
		SkinnedMeshRenderer[] objects = GetComponentsInChildren<SkinnedMeshRenderer>();
		for (int i = 0; i < KUtility.GetSize(objects); i++)
		{
			objects[i].enabled = false;
		}
		yield return new WaitForSeconds(3f);
		Object.Destroy(base.gameObject);
	}

	private void OnPlayLeaveSound()
	{
		SoundManager.PlayEvent(_bikeLeaveAudio, SoundPosition.Chase(_animalBehavior.gameObject));
	}
}
