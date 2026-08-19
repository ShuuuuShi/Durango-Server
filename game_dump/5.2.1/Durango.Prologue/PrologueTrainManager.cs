using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class PrologueTrainManager : Singleton<PrologueTrainManager>
{
	[SerializeField]
	private float _trainLength = 3700f;

	[SerializeField]
	private GameObject _bokehEffect;

	[SerializeField]
	private List<MeshRenderer> _thunderMeshes = new List<MeshRenderer>();

	[SerializeField]
	private Material _rainDropsMaterial;

	private readonly GameObject[] _covers = new GameObject[7];

	private readonly List<Material> _thunderMaterials = new List<Material>();

	private bool _rainDropsInitialized;

	private float _speed = 1f;

	private float _speed2 = 1f;

	private void Start()
	{
		GameObject trainCoverAisle = Singleton<PrologueManager>.Instance().TrainCoverAisle1;
		GameObject trainCoverCabin = Singleton<PrologueManager>.Instance().TrainCoverCabin1;
		int num = _covers.Length;
		_covers[0] = trainCoverCabin;
		_covers[1] = trainCoverAisle;
		for (int i = 0; i < num; i++)
		{
			if (i >= 2)
			{
				GameObject gameObject = ((i % 2 != 1) ? trainCoverCabin : trainCoverAisle);
				GameObject gameObject2 = Object.Instantiate(gameObject);
				gameObject2.transform.parent = gameObject.transform.parent;
				gameObject2.transform.localRotation = gameObject.transform.localRotation;
				_covers[i] = gameObject2;
			}
			int num2 = i / 2;
			_covers[i].transform.localPosition = new Vector3(0f, 0f, (float)num2 * _trainLength);
		}
		SetTrainShow(0);
		_bokehEffect.SetActive(value: false);
		ActivateRaining(bActivate: false);
		InitThunderMaterials();
		SetThunderMeshIntensity(0f);
	}

	public void SetTrainShow(int trainSection)
	{
		GameObject cover = _covers[trainSection];
		TweenMultipleAlpha tweenMultipleAlpha = TweenMultipleAlpha.Begin(cover, 0.2f, 0f);
		tweenMultipleAlpha.method = UITweener.Method.EaseOut;
		tweenMultipleAlpha.value = 1f;
		tweenMultipleAlpha.SetOnFinished(delegate
		{
			cover.SetActive(value: false);
		});
		tweenMultipleAlpha.PlayForward();
	}

	public void SetTrainCover(int trainSection)
	{
		GameObject obj = _covers[trainSection];
		obj.SetActive(value: true);
		TweenMultipleAlpha tweenMultipleAlpha = TweenMultipleAlpha.Begin(obj, 0.2f, 1f);
		tweenMultipleAlpha.method = UITweener.Method.EaseOut;
		tweenMultipleAlpha.value = 0f;
		tweenMultipleAlpha.onFinished.Clear();
		tweenMultipleAlpha.PlayForward();
	}

	public void BeginRaining()
	{
		_bokehEffect.SetActive(value: true);
		ActivateRaining(bActivate: true);
	}

	private void ActivateRaining(bool bActivate)
	{
		int num = _covers.Length;
		for (int i = 0; i < num; i++)
		{
			Transform[] componentsInChildren = _covers[i].GetComponentsInChildren<Transform>(includeInactive: true);
			int num2 = componentsInChildren.Length;
			for (int j = 0; j < num2; j++)
			{
				if (_covers[i] != componentsInChildren[j].gameObject)
				{
					componentsInChildren[j].gameObject.SetActive(bActivate);
				}
			}
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
