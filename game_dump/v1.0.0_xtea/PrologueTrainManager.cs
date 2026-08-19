using System.Collections.Generic;
using Holoville.HOTween;
using Holoville.HOTween.Core;
using UnityEngine;

public class PrologueTrainManager : KSingleton<PrologueTrainManager>
{
	[SerializeField]
	private float _trainLength = 3700f;

	[SerializeField]
	private GameObject _bokehEffect;

	[SerializeField]
	private List<MeshRenderer> _thunderMeshes = new List<MeshRenderer>();

	[SerializeField]
	private Material _rainDropsMaterial;

	private readonly GameObject[] _covers = (GameObject[])(object)new GameObject[7];

	private readonly List<Material> _thunderMaterials = new List<Material>();

	private bool _rainDropsInitialized;

	private float _speed = 1f;

	private float _speed2 = 1f;

	private void Start()
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		GameObject trainCoverAisle = KSingleton<PrologueManager>.Instance().TrainCoverAisle1;
		GameObject trainCoverCabin = KSingleton<PrologueManager>.Instance().TrainCoverCabin1;
		int num = _covers.Length;
		_covers[0] = trainCoverCabin;
		_covers[1] = trainCoverAisle;
		for (int i = 0; i < num; i++)
		{
			if (i >= 2)
			{
				GameObject val = ((i % 2 != 1) ? trainCoverCabin : trainCoverAisle);
				GameObject val2 = Object.Instantiate<GameObject>(val);
				val2.transform.parent = val.transform.parent;
				val2.transform.localRotation = val.transform.localRotation;
				_covers[i] = val2;
			}
			int num2 = i / 2;
			_covers[i].transform.localPosition = new Vector3(0f, 0f, (float)num2 * _trainLength);
		}
		SetTrainShow(0);
		_bokehEffect.SetActive(false);
		ActivateRaining(bActivate: false);
		InitThunderMaterials();
		SetThunderMeshIntensity(0f);
	}

	public void SetTrainShow(int trainSection)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Expected O, but got Unknown
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Expected O, but got Unknown
		int num = _covers[trainSection].GetComponent<Renderer>().materials.Length;
		for (int i = 0; i < num; i++)
		{
			TweenParms val = new TweenParms();
			_covers[trainSection].GetComponent<Renderer>().materials[i].color = new Color(_covers[trainSection].GetComponent<Renderer>().materials[i].color.r, _covers[trainSection].GetComponent<Renderer>().materials[i].color.g, _covers[trainSection].GetComponent<Renderer>().materials[i].color.b, 1f);
			val.Prop("color", (object)new Color(_covers[trainSection].GetComponent<Renderer>().materials[i].color.r, _covers[trainSection].GetComponent<Renderer>().materials[i].color.g, _covers[trainSection].GetComponent<Renderer>().materials[i].color.b, 0f));
			val.Ease((EaseType)5);
			val.OnComplete((TweenCallback)delegate
			{
				_covers[trainSection].SetActive(false);
			});
			HOTween.To((object)_covers[trainSection].GetComponent<Renderer>().materials[i], 0.2f, val);
		}
	}

	public void SetTrainCover(int trainSection)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Expected O, but got Unknown
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		_covers[trainSection].SetActive(true);
		int num = _covers[trainSection].GetComponent<Renderer>().materials.Length;
		for (int i = 0; i < num; i++)
		{
			TweenParms val = new TweenParms();
			_covers[trainSection].GetComponent<Renderer>().materials[i].color = new Color(_covers[trainSection].GetComponent<Renderer>().materials[i].color.r, _covers[trainSection].GetComponent<Renderer>().materials[i].color.g, _covers[trainSection].GetComponent<Renderer>().materials[i].color.b, 0f);
			val.Prop("color", (object)new Color(_covers[trainSection].GetComponent<Renderer>().materials[i].color.r, _covers[trainSection].GetComponent<Renderer>().materials[i].color.g, _covers[trainSection].GetComponent<Renderer>().materials[i].color.b, 1f));
			val.Ease((EaseType)5);
			HOTween.To((object)_covers[trainSection].GetComponent<Renderer>().materials[i], 0.2f, val);
		}
	}

	public void BeginRaining()
	{
		_bokehEffect.SetActive(true);
		ActivateRaining(bActivate: true);
	}

	private void ActivateRaining(bool bActivate)
	{
		int num = _covers.Length;
		for (int i = 0; i < num; i++)
		{
			Transform[] componentsInChildren = _covers[i].GetComponentsInChildren<Transform>(true);
			int num2 = componentsInChildren.Length;
			for (int j = 0; j < num2; j++)
			{
				if ((Object)(object)_covers[i] != (Object)(object)((Component)componentsInChildren[j]).gameObject)
				{
					((Component)componentsInChildren[j]).gameObject.SetActive(bActivate);
				}
			}
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

	private void Update()
	{
		if (!_rainDropsInitialized)
		{
			_speed = _rainDropsMaterial.GetFloat("_Speed");
			_speed2 = _rainDropsMaterial.GetFloat("_Speed2");
			_rainDropsInitialized = true;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		Shader.SetGlobalFloat("_RainDropsTime", realtimeSinceStartup % (1f / _speed));
		Shader.SetGlobalFloat("_RainDropsTime2", realtimeSinceStartup % (1f / _speed2));
	}
}
