using System;
using System.Collections.Generic;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class ModelComponent
{
	private class Model : IModel
	{
		public string Key;

		public string ModelPath;

		public ModelComponent Parent;

		public LoadState State;

		public bool IsValidFlag;

		private bool _isActive = true;

		private Vector3 _position = Vector3.zero;

		private Vector3 _scale = Vector3.one;

		private Vector3 _angle = Vector3.zero;

		private Color _color = Color.white;

		private GameObject _object;

		private RendererProxy _rendererProxy;

		public GameObject Object
		{
			get
			{
				return _object;
			}
			set
			{
				_object = value;
				if ((Object)(object)_object != (Object)null)
				{
					if (_rendererProxy == null)
					{
						_rendererProxy = new RendererProxy();
					}
					_rendererProxy.UpdateRenderers(_object.GetComponentsInChildren<Renderer>());
				}
				else if (_rendererProxy != null)
				{
					_rendererProxy.Clear();
				}
			}
		}

		public IModel SetActive(bool active)
		{
			_isActive = active;
			UpdateActive();
			if (State == LoadState.Success && (Object)(object)_object != (Object)null)
			{
				_object.gameObject.SetActive(active);
			}
			return this;
		}

		public bool GetActive()
		{
			return _isActive;
		}

		public IModel SetPosition(Vector3 position)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			_position = position;
			if (State == LoadState.Success && (Object)(object)_object != (Object)null)
			{
				_object.transform.localPosition = _position;
			}
			return this;
		}

		public Vector3 GetPosition()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _position;
		}

		public IModel SetScale(Vector3 scale)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			_scale = scale;
			if (State == LoadState.Success && (Object)(object)_object != (Object)null)
			{
				_object.transform.localScale = _scale;
			}
			return this;
		}

		public Vector3 GetScale()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _scale;
		}

		public IModel SetAngle(Vector3 angle)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			_angle = angle;
			if (State == LoadState.Success && (Object)(object)_object != (Object)null)
			{
				_object.transform.localEulerAngles = _angle;
			}
			return this;
		}

		public Vector3 GetAngle()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _angle;
		}

		public IModel SetColor(Color color)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_color = color;
			UpdateColor();
			return this;
		}

		public Color GetColor()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _color;
		}

		public GameObject GetObject()
		{
			if (State == LoadState.Success && (Object)(object)_object != (Object)null)
			{
				return _object;
			}
			return null;
		}

		public bool IsNull()
		{
			return Key == null;
		}

		public void UpdateTransform()
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			SetActive(GetActive());
			SetPosition(GetPosition());
			SetScale(GetScale());
			SetAngle(GetAngle());
			UpdateColor();
		}

		public void UpdateActive()
		{
			if (State == LoadState.Success && (Object)(object)_object != (Object)null)
			{
				bool flag = _isActive;
				ModelComponent modelComponent = Parent;
				while (flag && modelComponent != null)
				{
					flag &= modelComponent.Active;
					modelComponent = modelComponent.ParentComponent;
				}
				_object.gameObject.SetActive(flag);
			}
		}

		public void UpdateColor()
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			if (State == LoadState.Success && (Object)(object)_object != (Object)null)
			{
				Color val = Color.white;
				for (ModelComponent modelComponent = Parent; modelComponent != null; modelComponent = modelComponent.ParentComponent)
				{
					val *= modelComponent.Color;
				}
				_rendererProxy.Color = _color * val;
			}
		}
	}

	public interface IModel
	{
		IModel SetActive(bool active);

		bool GetActive();

		IModel SetPosition(Vector3 position);

		Vector3 GetPosition();

		IModel SetScale(Vector3 scale);

		Vector3 GetScale();

		IModel SetAngle(Vector3 angle);

		Vector3 GetAngle();

		IModel SetColor(Color color);

		Color GetColor();

		GameObject GetObject();

		bool IsNull();
	}

	private enum LoadState
	{
		None,
		Loading,
		Success,
		Fail,
		Unload
	}

	public static readonly IModel InvalidModel = new Model();

	private readonly int _randomSeed;

	private readonly List<Model> _components = new List<Model>();

	private List<ModelComponent> _childs;

	private readonly string _namePrefix;

	public GameObject Parent { get; private set; }

	public string Category { get; private set; }

	public ModelComponent ParentComponent { get; private set; }

	public bool Active { get; private set; }

	public Color Color { get; private set; }

	public int Count => _components.Count;

	public IModel this[int index] => _components[index];

	public int ChildCount => (_childs != null) ? _childs.Count : 0;

	public event Action<bool> LoadCompleted;

	public event Action Unloaded;

	public event Action<IModel> ModelLoaded;

	public event Action<IModel> ModelUnloaded;

	public ModelComponent(GameObject parent, int randomSeed = 0)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		Parent = parent;
		Category = null;
		ParentComponent = null;
		Active = true;
		Color = Color.white;
		_randomSeed = randomSeed;
	}

	public ModelComponent(ModelComponent parent, string category)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Parent = parent.Parent;
		Category = category;
		ParentComponent = parent;
		Active = true;
		Color = Color.white;
		_namePrefix = string.Empty;
		ModelComponent modelComponent = this;
		while (modelComponent != null && modelComponent.Category != null)
		{
			_namePrefix = $"{modelComponent.Category}/{_namePrefix}";
			modelComponent = modelComponent.ParentComponent;
		}
		this.ModelLoaded = (Action<IModel>)Delegate.Combine(this.ModelLoaded, new Action<IModel>(parent.OnAssetLoaded));
		this.ModelUnloaded = (Action<IModel>)Delegate.Combine(this.ModelUnloaded, new Action<IModel>(parent.OnAssetUnloaded));
	}

	public void Reset(GameObject parent)
	{
		Clear();
		_childs.Clear();
		Parent = parent;
		Category = null;
		ParentComponent = null;
	}

	public void BeginLoad()
	{
		int i = 0;
		for (int count = _components.Count; i < count; i++)
		{
			_components[i].IsValidFlag = false;
		}
		int j = 0;
		for (int childCount = ChildCount; j < childCount; j++)
		{
			_childs[j].BeginLoad();
		}
	}

	public void EndLoad()
	{
		for (int num = _components.Count - 1; num >= 0; num--)
		{
			if (!_components[num].IsValidFlag)
			{
				UnloadResourse(_components[num]);
				_components.RemoveAt(num);
			}
		}
		int i = 0;
		for (int childCount = ChildCount; i < childCount; i++)
		{
			_childs[i].EndLoad();
		}
	}

	private int IndexOf(string key)
	{
		int i = 0;
		for (int count = _components.Count; i < count; i++)
		{
			if (_components[i].Key == key)
			{
				return i;
			}
		}
		return -1;
	}

	private int CategoryIndexOf(string category)
	{
		int i = 0;
		for (int childCount = ChildCount; i < childCount; i++)
		{
			if (_childs[i].Category == category)
			{
				return i;
			}
		}
		return -1;
	}

	public ModelComponent GetCategory(string category, bool make = true)
	{
		if (string.IsNullOrEmpty(category))
		{
			return this;
		}
		ModelComponent modelComponent = null;
		int num = CategoryIndexOf(category);
		if (num == -1)
		{
			if (make)
			{
				if (_childs == null)
				{
					_childs = new List<ModelComponent>();
				}
				modelComponent = new ModelComponent(this, category);
				_childs.Add(modelComponent);
			}
		}
		else
		{
			modelComponent = _childs[num];
		}
		return modelComponent;
	}

	public ModelComponent GetCategory(int index)
	{
		return _childs[index];
	}

	public IModel Load(string key, string modelKey, string modelPostfix, string category = null)
	{
		return PathLoad(key, GetAssetPath(modelKey, modelPostfix, _randomSeed), category);
	}

	public IModel PathLoad(string key, string modelPath, string category = null)
	{
		ModelComponent category2 = GetCategory(category);
		return category2.LoadModel(key, modelPath);
	}

	private IModel LoadModel(string key, string modelPath)
	{
		if (string.IsNullOrEmpty(modelPath))
		{
			Unload(key);
			return InvalidModel;
		}
		Model model = null;
		int num = IndexOf(key);
		if (num != -1)
		{
			model = _components[num];
			if (!(_components[num].ModelPath == modelPath))
			{
				UnloadResourse(_components[num]);
			}
		}
		if (model == null || model.State == LoadState.Unload)
		{
			if (model == null)
			{
				model = new Model();
				model.Parent = this;
				_components.Add(model);
			}
			model.Key = key;
			model.ModelPath = modelPath;
			model.State = LoadState.None;
		}
		model.IsValidFlag = true;
		LoadResourse(model);
		return model;
	}

	public void Unload(string key)
	{
		Unload(null, key);
	}

	public void Unload(string category, string key)
	{
		GetCategory(category, make: false)?.UnloadModel(key);
	}

	private void UnloadModel(string key)
	{
		int num = IndexOf(key);
		if (num != -1)
		{
			UnloadResourse(_components[num]);
			_components.RemoveAt(num);
		}
	}

	public void Clear()
	{
		int i = 0;
		for (int count = _components.Count; i < count; i++)
		{
			UnloadResourse(_components[i]);
		}
		_components.Clear();
		int j = 0;
		for (int childCount = ChildCount; j < childCount; j++)
		{
			_childs[j].Clear();
		}
	}

	public IModel GetModel(string key)
	{
		return GetModel(null, key);
	}

	public IModel GetModel(string category, string key)
	{
		ModelComponent category2 = GetCategory(category, make: false);
		IModel result;
		if (category2 == null)
		{
			IModel invalidModel = InvalidModel;
			result = invalidModel;
		}
		else
		{
			result = category2.GetModelObject(key);
		}
		return result;
	}

	private IModel GetModelObject(string key)
	{
		int num = IndexOf(key);
		if (num == -1)
		{
			return InvalidModel;
		}
		return _components[num];
	}

	private void LoadResourse(Model model)
	{
		if (model.State != 0)
		{
			return;
		}
		model.State = LoadState.Loading;
		string assetPath = BuildManager.GetAssetDirectory(model.ModelPath);
		KSingleton<AssetBundleManager>.Instance().RequestAsset(assetPath, typeof(GameObject), delegate(Object asset)
		{
			if (!((Object)(object)Parent == (Object)null) && assetPath.EndsWith(model.ModelPath))
			{
				if (model.State == LoadState.Unload)
				{
					Unload(model.Key);
					OnAssetLoaded(model);
				}
				else if (asset == (Object)null)
				{
					model.State = LoadState.Fail;
					OnAssetLoaded(model);
				}
				else
				{
					Object obj = Object.Instantiate(asset);
					GameObject val = (GameObject)(object)((obj is GameObject) ? obj : null);
					if ((Object)(object)val == (Object)null)
					{
						Debug.LogError((object)("Asset instantiation failed: " + assetPath));
						model.State = LoadState.Fail;
						OnAssetLoaded(model);
					}
					else
					{
						val.transform.parent = Parent.transform;
						model.State = LoadState.Success;
						if ((Object)(object)model.Object != (Object)null)
						{
							Object.Destroy((Object)(object)model.Object);
						}
						model.Object = val;
						((Object)val).name = $"{_namePrefix}{model.Key}";
						model.UpdateTransform();
						OnAssetLoaded(model);
					}
				}
			}
		});
	}

	private void UnloadResourse(Model model)
	{
		bool flag = false;
		if ((Object)(object)model.Object != (Object)null)
		{
			Object.Destroy((Object)(object)model.Object);
			flag = true;
		}
		model.State = LoadState.Unload;
		if (flag)
		{
			OnAssetUnloaded(model);
		}
	}

	private void OnAssetLoaded(IModel model)
	{
		if (this.ModelLoaded != null)
		{
			this.ModelLoaded(model);
		}
		CheckLoadComplete();
	}

	private void OnAssetUnloaded(IModel model)
	{
		if (this.ModelUnloaded != null)
		{
			this.ModelUnloaded(model);
		}
		if (this.Unloaded != null)
		{
			this.Unloaded();
		}
	}

	private void CheckLoadComplete()
	{
		GetLoadState(out var isLoading, out var isSuccess);
		if (isLoading)
		{
			return;
		}
		int i = 0;
		for (int childCount = ChildCount; i < childCount; i++)
		{
			_childs[i].GetLoadState(out isLoading, out var isSuccess2);
			if (isLoading)
			{
				return;
			}
			isSuccess = isSuccess && isSuccess2;
		}
		if (this.LoadCompleted != null)
		{
			this.LoadCompleted(isSuccess);
		}
	}

	private void GetLoadState(out bool isLoading, out bool isSuccess)
	{
		isSuccess = true;
		isLoading = false;
		int i = 0;
		for (int count = _components.Count; i < count; i++)
		{
			switch (_components[i].State)
			{
			case LoadState.Fail:
				isSuccess = false;
				break;
			default:
				isLoading = true;
				return;
			case LoadState.Success:
			case LoadState.Unload:
				break;
			}
		}
	}

	public void SetActive(bool active)
	{
		if (Active != active)
		{
			Active = active;
			UpdateModelActive();
		}
	}

	private void UpdateModelActive()
	{
		int i = 0;
		for (int count = _components.Count; i < count; i++)
		{
			_components[i].UpdateActive();
		}
		int j = 0;
		for (int childCount = ChildCount; j < childCount; j++)
		{
			_childs[j].UpdateModelActive();
		}
	}

	public void SetColor(Color color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!(Color == color))
		{
			Color = color;
			UpdateModelColors();
		}
	}

	private void UpdateModelColors()
	{
		int i = 0;
		for (int count = _components.Count; i < count; i++)
		{
			_components[i].UpdateColor();
		}
		int j = 0;
		for (int childCount = ChildCount; j < childCount; j++)
		{
			_childs[j].UpdateModelColors();
		}
	}

	public void DoAllModels(Action<IModel> func)
	{
		if (func != null)
		{
			int i = 0;
			for (int count = _components.Count; i < count; i++)
			{
				Model obj = _components[i];
				func(obj);
			}
			int j = 0;
			for (int childCount = ChildCount; j < childCount; j++)
			{
				_childs[j].DoAllModels(func);
			}
		}
	}

	public static string GetAssetPath(string modelKey, string sub = null, int randomSeed = 0)
	{
		if (string.IsNullOrEmpty(modelKey))
		{
			return null;
		}
		ArtifactModel artifactModel = SingletonDict<string, ArtifactModel>.Get(modelKey);
		if (artifactModel == null || artifactModel.file_names == null || artifactModel.file_names.Length == 0)
		{
			return null;
		}
		string text = artifactModel.file_names[Mathf.Abs(randomSeed) % artifactModel.file_names.Length];
		if (!string.IsNullOrEmpty(sub))
		{
			text = $"{text}_{sub}";
		}
		return $"{artifactModel.path}/{text}.prefab";
	}

	public static string GetPreviewAssetPath(string modelKey, int randomSeed = 0)
	{
		if (string.IsNullOrEmpty(modelKey))
		{
			return null;
		}
		ArtifactModel artifactModel = SingletonDict<string, ArtifactModel>.Get(modelKey);
		if (artifactModel == null || artifactModel.file_names == null || artifactModel.file_names.Length == 0)
		{
			return null;
		}
		string arg = "preview_" + artifactModel.file_names[randomSeed % artifactModel.file_names.Length];
		return $"{artifactModel.path}/preview/{arg}.prefab";
	}
}
