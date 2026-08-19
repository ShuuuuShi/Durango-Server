using System;
using Durango.Model;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Shared.Etc;
using UnityEngine;

namespace Durango.UI.Control;

public class UIModelViewer : UITexture
{
	public struct Arguments
	{
		public float CameraAngle;

		public float Rotation;

		public float? Scale;

		public Bounds? Bounds;

		public float? YPivot;

		public Action<GameObject> Loaded;
	}

	public struct ArtifactArguments
	{
		public ArtifactDisplay Display;

		public Point2 Size;

		public int Stories;

		public bool? HasRoof;

		public Rotation Rotation;

		public bool IsModular;
	}

	private AnimalBehavior _animal;

	private bool _isInit;

	private bool _dragLock;

	private readonly AnimationSequence _animationSequence = new AnimationSequence();

	private int _loadedFrame;

	private int _version;

	public GameObject ModelObject { get; private set; }

	public UIModelRender ModelRender { get; private set; }

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		if (Application.isPlaying && !_isInit)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying)
		{
			_animationSequence.Update();
			if (_loadedFrame < Time.frameCount && visibleRatio < 1f)
			{
				visibleRatio = Mathf.Clamp01(Time.deltaTime * 3f + visibleRatio);
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			Clear();
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		if (Application.isPlaying && _animal != null)
		{
			_animal.BoneMergeable.UpdateBoneMergeSet();
		}
	}

	[UsedImplicitly]
	private void OnDrag(Vector2 delta)
	{
		if (!_dragLock && !(ModelObject == null))
		{
			Transform transform = ModelObject.transform;
			transform.Rotate(transform.up, 0f - delta.x, Space.World);
		}
	}

	private void MakePlainModel(string path, Arguments args)
	{
		_dragLock = false;
		int requested = _version;
		Singleton<AssetBundleManager>.Instance().RequestAsset(path, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (requested == _version)
			{
				GameObject gameObject = ((!(asset != null)) ? null : (UnityEngine.Object.Instantiate(asset) as GameObject));
				if (gameObject != null)
				{
					AnimalBehavior component = gameObject.GetComponent<AnimalBehavior>();
					if ((bool)component)
					{
						args.Scale = ((!args.Scale.HasValue) ? 1f : args.Scale.Value) * component.UIViewScale;
						component.PlaneShadowEnabled = false;
						BoneLookAtTarget component2 = component.GetComponent<BoneLookAtTarget>();
						if (component2 != null)
						{
							UnityEngine.Object.Destroy(component2);
						}
						component.enabled = false;
					}
				}
				SetModelObject(gameObject, args);
			}
		});
	}

	private void MakePlayerModel(bool isMale, PlayerDisplay display, Arguments args)
	{
		_dragLock = false;
		PlayerBehavior playerBehavior = Singleton<PlayerManager>.Instance().MakePreview(isMale, display);
		SetModelObject(playerBehavior.gameObject, args);
		playerBehavior.UpdateBodyScale();
	}

	private ModelComponent MakeArtifactModel(ArtifactArguments artifact, Arguments args)
	{
		_dragLock = true;
		ModelComponent model = new ModelComponent(new GameObject("PreviewModel"));
		Point2 size = artifact.Size;
		size.x = Mathf.Max(1, size.x);
		size.y = Mathf.Max(1, size.y);
		int num = Mathf.Max(1, artifact.Stories);
		Bounds? bounds = args.Bounds;
		if (!bounds.HasValue)
		{
			args.Bounds = new Bounds(Vector3.zero, new Vector3((float)size.x + 1f, Mathf.Max((float)num + 0.5f, Mathf.Min(2, Mathf.Max(size.x, size.y))), (float)size.y + 1f) * 200f);
		}
		float? yPivot = args.YPivot;
		if (!yPivot.HasValue)
		{
			args.YPivot = 0.5f;
		}
		int requested = _version;
		model.LoadCompleted += delegate
		{
			GameObject gameObject = model.Parent;
			if (requested != _version)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			else
			{
				NGUITools.SetLayer(gameObject, gameObject.layer);
				RemoveParticle(gameObject);
				SetModelObject(gameObject, args);
			}
		};
		if (artifact.IsModular)
		{
			ModularArtifact.FillModels(model, artifact.Display, size, num, artifact.HasRoof.GetValueOrDefault(true), new Vector2(0.5f, 0.5f));
		}
		else
		{
			Artifact.FillModels(model, artifact.Display, Vector3.zero, artifact.Rotation);
		}
		return model;
	}

	public void Clear()
	{
		_version++;
		SetModelObject(null);
		_animationSequence.Reset();
	}

	public void SetPlainModel(string path, Arguments args)
	{
		Init();
		base.gameObject.SetActive(value: true);
		Clear();
		MakePlainModel(path, args);
	}

	public void SetPlayerModel(bool isMale, PlayerDisplay display, Arguments args)
	{
		Init();
		base.gameObject.SetActive(value: true);
		Clear();
		MakePlayerModel(isMale, display, args);
	}

	public ModelComponent SetArtifactModel(ArtifactArguments artifact, Arguments args)
	{
		Init();
		base.gameObject.SetActive(value: true);
		Clear();
		return MakeArtifactModel(artifact, args);
	}

	private void SetModelObject(GameObject obj, Arguments args = default(Arguments))
	{
		DestoryModelObject();
		ModelObject = obj;
		if (ModelObject == null || !base.gameObject.activeSelf)
		{
			UIModelRenderBuilder.Release(ModelRender);
			ModelRender = null;
			mainTexture = null;
			DestoryModelObject();
			return;
		}
		_animal = ModelObject.GetComponent<AnimalBehavior>();
		if (args.Loaded != null)
		{
			args.Loaded(ModelObject);
		}
		if (ModelRender == null)
		{
			ModelRender = UIModelRenderBuilder.Make();
			if (ModelRender == null)
			{
				return;
			}
		}
		UIModelRender modelRender = ModelRender;
		GameObject modelObject = ModelObject;
		float cameraAngle = args.CameraAngle;
		float modelScale = ((!args.Scale.HasValue) ? 1f : args.Scale.Value);
		Bounds? bounds = args.Bounds;
		float? yPivot = args.YPivot;
		modelRender.SetModel(modelObject, cameraAngle, modelScale, bounds, yPivot.HasValue ? args.YPivot.Value : 0f);
		if (ModelObject != null)
		{
			ModelObject.transform.localEulerAngles = new Vector3(0f, args.Rotation + 50f, 0f);
		}
		ModelRender.FillTexture(this);
		visibleRatio = 0f;
		_loadedFrame = Time.frameCount;
	}

	private void DestoryModelObject()
	{
		if (!(ModelObject == null))
		{
			UnityEngine.Object.Destroy(ModelObject);
			ModelObject = null;
			_animal = null;
		}
	}

	public Action<GameObject> DefaultAnimalPlay(string state = "stand", bool isOld = false)
	{
		return DefaultAnimalPlay(null, state, isOld);
	}

	public Action<GameObject> DefaultAnimalPlay(string enter, string state, bool isOld = false)
	{
		return delegate(GameObject obj)
		{
			DefaultAnimalMotionPlay(obj, enter, state, toLast: false, isOld);
		};
	}

	public Action<GameObject> DefaultDeadAnimalPlay(bool isOld = false)
	{
		return delegate(GameObject obj)
		{
			DefaultAnimalMotionPlay(obj, null, "dead", toLast: true, isOld);
		};
	}

	public Action<GameObject> SetupSaddle()
	{
		return delegate(GameObject obj)
		{
			VehiclePet component = obj.GetComponent<VehiclePet>();
			if (!(component == null))
			{
				component.SetupSaddle();
			}
		};
	}

	private void DefaultAnimalMotionPlay(GameObject obj, string enter, string state, bool toLast, bool isOld)
	{
		if (!(obj == null))
		{
			AnimalBehavior component = obj.GetComponent<AnimalBehavior>();
			if (component != null)
			{
				component.PrepareRendererProxy();
				component.SetOld(isOld);
			}
			if (toLast)
			{
				SetAnimalAnimation(state, null);
				_animationSequence.ToLast();
			}
			else
			{
				SetAnimalAnimation(enter, state);
				_animationSequence.Update();
			}
		}
	}

	public void SetAnimalAnimation(string state, string next)
	{
		if (_animal == null)
		{
			return;
		}
		AnimationElemBase animationElemBase = ((!string.IsNullOrEmpty(state)) ? _animal.AnimalFrameworkResource.GetAnimationElements(state) : null);
		if (animationElemBase == null)
		{
			if (!string.IsNullOrEmpty(next))
			{
				SetAnimalAnimation(next, null);
			}
			return;
		}
		if (string.IsNullOrEmpty(next))
		{
			_animationSequence.Set(_animal, animationElemBase, loop: true);
			return;
		}
		float num = 0f;
		foreach (AnimationSequenceClip item in animationElemBase)
		{
			if (!string.IsNullOrEmpty(item.Clip))
			{
				AnimationState animationState = _animal.Anim[item.Clip];
				if (animationState != null)
				{
					num += animationState.length;
				}
			}
		}
		if (!(num <= 0f))
		{
			_animationSequence.Set(_animal, animationElemBase, loop: false, num - 0.3f, 1f, delegate
			{
				SetAnimalAnimation(next, null);
			});
		}
	}

	private static void RemoveParticle(GameObject obj)
	{
		ParticleSystem[] componentsInChildren = obj.GetComponentsInChildren<ParticleSystem>();
		ParticleSystem[] array = componentsInChildren;
		foreach (ParticleSystem particleSystem in array)
		{
			UnityEngine.Object.Destroy(particleSystem.gameObject);
		}
		Renderer[] componentsInChildren2 = obj.GetComponentsInChildren<Renderer>();
		if (componentsInChildren2 == null)
		{
			return;
		}
		Renderer[] array2 = componentsInChildren2;
		foreach (Renderer renderer in array2)
		{
			bool flag = false;
			Material[] sharedMaterials = renderer.sharedMaterials;
			for (int k = 0; k < KUtility.GetSize(sharedMaterials); k++)
			{
				Material material = sharedMaterials[k];
				if (!(material == null))
				{
					if (material.name.Contains("LightMask"))
					{
						flag = true;
						break;
					}
					if (material.shader != null && material.shader.name.Contains("Multiply"))
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				renderer.enabled = false;
			}
		}
	}
}
