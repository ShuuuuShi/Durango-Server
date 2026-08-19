using System.Collections;
using Shared.Player;
using UnityEngine;

public class TriggerPrologueSelectCharacter : MonoBehaviour
{
	[SerializeField]
	private Job _job;

	[SerializeField]
	private string _standMotion;

	private Vector3 _initPosition;

	private Vector3 _walkDestPosition;

	private Quaternion _initRot;

	[SerializeField]
	private string _walkMotion;

	private PrologueCharacterSelectGroup _prologueCharacterSelectUI;

	private NPCActorBehavior _actor;

	public string StandMotion
	{
		get
		{
			return _standMotion;
		}
		set
		{
			_standMotion = value;
		}
	}

	public string WalkMotion
	{
		get
		{
			return _walkMotion;
		}
		set
		{
			_walkMotion = value;
		}
	}

	private PrologueCharacterSelectGroup PrologCharacterSelectUI
	{
		get
		{
			if ((Object)(object)_prologueCharacterSelectUI == (Object)null)
			{
				_prologueCharacterSelectUI = UIManager.FindScript<PrologueCharacterSelectGroup>();
			}
			return _prologueCharacterSelectUI;
		}
	}

	private void Start()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		_actor = ((Component)this).gameObject.GetComponent<NPCActorBehavior>();
		_initPosition = ((Component)this).gameObject.transform.position;
		_walkDestPosition = CalcDestPos();
		_initRot = ((Component)this).transform.rotation;
		_actor.UpdateCostumeColorsFromMaterials();
	}

	public void OnSelectedOnPrologue()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		KSingleton<CameraController>.Instance().SetCameraTarget(((Component)this).gameObject);
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.OnTouchCharacter);
		Transform transform = ((Component)this).transform;
		Transform parent = transform.parent;
		int i = 0;
		for (int childCount = parent.childCount; i < childCount; i++)
		{
			Transform child = parent.GetChild(i);
			Color color = (Color)((!((Object)(object)child == (Object)(object)transform)) ? new Color(0.2f, 0.2f, 0.2f) : Color.white);
			Renderer[] componentsInChildren = ((Component)child).GetComponentsInChildren<Renderer>();
			if (componentsInChildren == null)
			{
				continue;
			}
			int j = 0;
			for (int num = componentsInChildren.Length; j < num; j++)
			{
				Material[] materials = componentsInChildren[j].materials;
				if (materials != null)
				{
					int k = 0;
					for (int num2 = materials.Length; k < num2; k++)
					{
						materials[k].color = color;
					}
				}
			}
		}
	}

	public void OnSubmitOnPrologue()
	{
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.OnChooseCharacter);
		SubmitCharacter();
	}

	public void OnUnselectedOnPrologue()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		Transform parent = ((Component)this).transform.parent;
		int i = 0;
		for (int childCount = parent.childCount; i < childCount; i++)
		{
			Transform child = parent.GetChild(i);
			Color white = Color.white;
			Renderer[] componentsInChildren = ((Component)child).GetComponentsInChildren<Renderer>();
			if (componentsInChildren == null)
			{
				continue;
			}
			int j = 0;
			for (int num = componentsInChildren.Length; j < num; j++)
			{
				Material[] materials = componentsInChildren[j].materials;
				if (materials != null)
				{
					int k = 0;
					for (int num2 = materials.Length; k < num2; k++)
					{
						materials[k].color = white;
					}
				}
			}
		}
	}

	private bool IsValidMotion(string motionName)
	{
		return motionName != string.Empty && motionName != "_None_";
	}

	private void DoStandMotion()
	{
		KSingleton<CameraController>.Instance().SetCameraTarget(((Component)this).gameObject);
		if (IsValidMotion(StandMotion))
		{
			_actor.CrossFade(StandMotion, 0.5f);
		}
	}

	private void SubmitCharacter()
	{
		((MonoBehaviour)this).StopCoroutine("CoWalkToHall");
		((MonoBehaviour)this).StopCoroutine("CoWalkToInit");
		((MonoBehaviour)this).StartCoroutine("CoWalkToHall");
	}

	public void RotateToPosition(Vector3 pos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		float num = KMathUtil.CalcYawWithTarget(pos, ((Component)this).transform.position);
		((Component)this).transform.localRotation = Quaternion.Euler(0f, num, 0f);
	}

	private IEnumerator CoWalkToHall()
	{
		PlayerBehavior.LocalPlayer.CurrentPosition = ((Component)this).transform.position;
		KSingleton<PrologueManager>.Instance().BeginPlayer(_actor, _walkDestPosition);
		((Component)PlayerBehavior.LocalPlayer.MeshObjectTransform.FindChild("Body")).gameObject.GetComponent<Renderer>().enabled = false;
		((Component)PlayerBehavior.LocalPlayer.MeshObjectTransform.FindChild("Hair")).gameObject.GetComponent<Renderer>().enabled = false;
		((Component)PlayerBehavior.LocalPlayer.MeshObjectTransform.FindChild("Head")).gameObject.GetComponent<Renderer>().enabled = false;
		while (!PlayerBehavior.LocalPlayer.IsLoaded)
		{
			yield return (object)new WaitForEndOfFrame();
		}
		KSingleton<PrologueManager>.Instance().MakeLitSphereOverride(PlayerBehavior.LocalPlayer.MeshObjectTransform);
		((Component)PlayerBehavior.LocalPlayer.MeshObjectTransform.FindChild("Body")).gameObject.GetComponent<Renderer>().enabled = true;
		((Component)PlayerBehavior.LocalPlayer.MeshObjectTransform.FindChild("Hair")).gameObject.GetComponent<Renderer>().enabled = true;
		((Component)PlayerBehavior.LocalPlayer.MeshObjectTransform.FindChild("Head")).gameObject.GetComponent<Renderer>().enabled = true;
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	private IEnumerator CoWalkToInit()
	{
		_actor.CrossFade(WalkMotion, 0.5f);
		RotateToPosition(_initPosition);
		Vector3 val;
		do
		{
			((Component)this).transform.position = Vector3.MoveTowards(((Component)this).transform.position, _initPosition, 200f * Time.deltaTime);
			yield return null;
			val = _initPosition - ((Component)this).transform.position;
		}
		while (!(((Vector3)(ref val)).magnitude < 10f));
		_actor.CrossFade(_actor.GetDefaultMotionName(), 0.5f);
		((Component)this).transform.position = _initPosition;
		((Component)this).transform.rotation = _initRot;
	}

	private void CancelToDefaultMotion()
	{
		((MonoBehaviour)this).StopCoroutine("CoWalkToHall");
		((MonoBehaviour)this).StopCoroutine("CoWalkToInit");
		((MonoBehaviour)this).StartCoroutine("CoWalkToInit");
	}

	public void Touched()
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		PrologCharacterSelectUI.Open();
		PrologCharacterSelectUI.SetSelectCharactInfo(_job, _actor.IsMale);
		OnSelectedOnPrologue();
		GameSystem<InteractionSystem>.Instance().SetInteractionTarget(null);
		KSingleton<PrologueManager>.Instance().ZoomIn();
		KSingleton<CameraController>.Instance().SetCameraTargetPos(((Component)this).transform.localPosition + (Vector3.right + Vector3.back) * 150f, 0.5f);
	}

	private Vector3 CalcDestPos()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).gameObject.transform.position;
		if (Mathf.Abs(position.z) < 70f)
		{
			return position;
		}
		position.x -= 50f;
		position.y = 0f;
		position.z = 70f * Mathf.Sign(position.z);
		return position;
	}
}
