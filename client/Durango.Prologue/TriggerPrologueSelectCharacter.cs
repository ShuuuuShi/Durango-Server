using System.Collections;
using Durango.Render.Camera;
using Durango.UI.Prologue;
using Durango.Utils;
using Shared.Player;
using UnityEngine;

namespace Durango.Prologue;

public class TriggerPrologueSelectCharacter : MonoBehaviour
{
	[SerializeField]
	private Job _job;

	[SerializeField]
	private string _standMotion;

	[SerializeField]
	private string _walkMotion;

	private Vector3 _walkDestPosition;

	private PrologueCharacterSelectGroupBase _prologueCharacterSelectUI;

	private CostumeActorBehavior _actor;

	public Job Job => _job;

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

	private PrologueCharacterSelectGroupBase PrologCharacterSelectUI
	{
		get
		{
			if (_prologueCharacterSelectUI == null)
			{
				_prologueCharacterSelectUI = UIManager.FindScript<PrologueCharacterSelectGroupBase>();
			}
			return _prologueCharacterSelectUI;
		}
	}

	private void Start()
	{
		_actor = base.gameObject.GetComponent<CostumeActorBehavior>();
		_walkDestPosition = CalcDestPos();
	}

	public void Select()
	{
		MaskOthers(mask: true);
		PrologCharacterSelectUI.Open();
		PrologCharacterSelectUI.SetSelectCharactInfo(_job, _actor.IsMale);
		Singleton<CameraController>.Instance().Target(base.gameObject, 0.3f);
		Singleton<CameraController>.Instance().Zoom(2f, 0.4f, NgInterpolate.EaseType.EaseInQuad).Offset(new Vector3(75f, 100f, -75f), 0.4f)
			.Target(base.gameObject, 0.4f);
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.OnTouchCharacter);
	}

	public void Unselect()
	{
		MaskOthers(mask: false);
	}

	private void MaskOthers(bool mask)
	{
		Transform transform = base.transform;
		Transform parent = transform.parent;
		int i = 0;
		for (int childCount = parent.childCount; i < childCount; i++)
		{
			Transform child = parent.GetChild(i);
			Color color = ((!mask || !(child != transform)) ? Color.white : new Color(0.2f, 0.2f, 0.2f));
			Renderer[] componentsInChildren = child.GetComponentsInChildren<Renderer>();
			if (componentsInChildren == null)
			{
				continue;
			}
			Renderer[] array = componentsInChildren;
			foreach (Renderer renderer in array)
			{
				Material[] materials = renderer.materials;
				if (materials != null)
				{
					Material[] array2 = materials;
					foreach (Material material in array2)
					{
						material.color = color;
					}
				}
			}
		}
	}

	public void ChooseCharacter()
	{
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(PrologueGuideSystem.PrologueGuideState.OnChooseCharacter);
		StopAllCoroutines();
		StartCoroutine(CoWalkToHall());
	}

	private IEnumerator CoWalkToHall()
	{
		Singleton<PrologueManager>.Instance().BeginPlayer(_actor, _walkDestPosition);
		PlayerBehavior.LocalPlayer.SetVisible(visible: false);
		float beginTime = Time.time;
		while (!PlayerBehavior.LocalPlayer.IsLoaded && !(Time.time - beginTime > 5f))
		{
			yield return null;
		}
		PlayerBehavior.LocalPlayer.SetVisible(visible: true);
		Singleton<PrologueManager>.Instance().MakeLitSphereOverride(PlayerBehavior.LocalPlayer.MeshObjectTransform);
		Object.Destroy(base.gameObject);
	}

	private Vector3 CalcDestPos()
	{
		Vector3 position = base.gameObject.transform.position;
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
