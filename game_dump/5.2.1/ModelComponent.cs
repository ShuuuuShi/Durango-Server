using System;
using System.Collections.Generic;
using Durango.Render;
using Durango.Utils;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class ModelComponent : ModelComponent.IModel
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

		private string _patternTexPath;

		private Texture2D _patternTex;

		private float _damaged;

		private bool _isValidMaterial;

		private Material _material;

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
				if (_object != null)
				{
					if (_rendererProxy == null)
					{
						_rendererProxy = new RendererProxy();
					}
					_rendererProxy.UpdateRenderers(_object, isAnimal: false, isProp: true);
					UpdatePatternTex();
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
			if (State == LoadState.Success && _object != null)
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
			_position = position;
			if (State == LoadState.Success && _object != null)
			{
				_object.transform.localPosition = _position;
			}
			return this;
		}

		public Vector3 GetPosition()
		{
			return _position;
		}

		public IModel SetScale(Vector3 scale)
		{
			_scale = scale;
			if (State == LoadState.Success && _object != null)
			{
				_object.transform.localScale = _scale;
			}
			return this;
		}

		public Vector3 GetScale()
		{
			return _scale;
		}

		public IModel SetAngle(Vector3 angle)
		{
			_angle = angle;
			if (State == LoadState.Success && _object != null)
			{
				_object.transform.localEulerAngles = _angle;
			}
			return this;
		}

		public Vector3 GetAngle()
		{
			return _angle;
		}

		public IModel SetColor(Color color)
		{
			_color = color;
			UpdateColor();
			return this;
		}

		public void SetMaterialsToBeShared(Dictionary<Material, Material> materials)
		{
			if (State == LoadState.Success && _object != null)
			{
				_rendererProxy.SetMaterialsToBeShared(materials);
			}
		}

		public Color GetColor()
		{
			return _color;
		}

		public IModel SetPatternTex(string texturePath)
		{
			_patternTexPath = texturePath;
			if (string.IsNullOrEmpty(_patternTexPath))
			{
				_patternTex = null;
				UpdatePatternTex();
			}
			else
			{
				string patternTexturePath = GetPatternTexturePath(texturePath);
				Durango.Utils.Singleton<AssetBundleManager>.Instance().RequestAsset(patternTexturePath, typeof(Texture2D), delegate(UnityEngine.Object o)
				{
					if (!(_patternTexPath != texturePath))
					{
						if (o == null)
						{
							_patternTex = null;
						}
						else
						{
							_patternTex = o as Texture2D;
						}
						UpdatePatternTex();
					}
				});
			}
			return this;
		}

		public string GetPatternTex()
		{
			return _patternTexPath;
		}

		public IModel SetDamaged(float damaged)
		{
			_damaged = damaged;
			UpdateDamaged();
			return this;
		}

		public float GetDamaged()
		{
			return _damaged;
		}

		public IModel SetMaterial(Material material)
		{
			_isValidMaterial = true;
			_material = material;
			return this;
		}

		public GameObject GetObject()
		{
			if (State == LoadState.Success && _object != null)
			{
				return _object;
			}
			return null;
		}

		public bool IsNull()
		{
			return Key == null;
		}

		public void Refresh()
		{
			SetActive(GetActive());
			SetPosition(GetPosition());
			SetScale(GetScale());
			SetAngle(GetAngle());
			UpdateColor();
			UpdateOutlineColor();
			UpdatePatternTex();
			UpdateDamaged();
			UpdateMaterial();
		}

		public void UpdateActive()
		{
			if (State == LoadState.Success && _object != null)
			{
				bool flag = _isActive;
				ModelComponent modelComponent = Parent;
				while (flag && modelComponent != null)
				{
					flag &= modelComponent._active;
					modelComponent = modelComponent.ParentComponent;
				}
				_object.gameObject.SetActive(flag);
			}
		}

		public void UpdateColor()
		{
			if (State == LoadState.Success && _object != null)
			{
				Color white = Color.white;
				for (ModelComponent modelComponent = Parent; modelComponent != null; modelComponent = modelComponent.ParentComponent)
				{
					white *= modelComponent._color;
				}
				_rendererProxy.SetColor(_color * white);
			}
		}

		public void UpdateOutlineColor()
		{
			if (State != LoadState.Success || !(_object != null))
			{
				return;
			}
			Color outline = Color.clear;
			for (ModelComponent modelComponent = Parent; modelComponent != null; modelComponent = modelComponent.ParentComponent)
			{
				if (modelComponent._outlineColor != Color.clear)
				{
					outline = modelComponent._outlineColor;
				}
			}
			_rendererProxy.SetOutline(outline);
		}

		public void UpdatePatternTex()
		{
			if (State != LoadState.Success || !(_object != null))
			{
				return;
			}
			Texture2D patternTex = _patternTex;
			if (patternTex == null)
			{
				ModelComponent modelComponent = Parent;
				while (modelComponent != null && patternTex == null)
				{
					patternTex = modelComponent._patternTex;
					modelComponent = modelComponent.ParentComponent;
				}
			}
			_rendererProxy.SetPatternTex(patternTex);
		}

		public bool HasPatternTex()
		{
			if (State == LoadState.Success && _object != null)
			{
				return _rendererProxy.HasPatternTex();
			}
			return false;
		}

		public void UpdateDamaged()
		{
			if (State == LoadState.Success && _object != null)
			{
				float b = 0f;
				for (ModelComponent modelComponent = Parent; modelComponent != null; modelComponent = modelComponent.ParentComponent)
				{
					b = Mathf.Max(modelComponent._damaged, b);
				}
				_rendererProxy.SetDamaged(Mathf.Max(_damaged, b));
			}
		}

		public void UpdateMaterial()
		{
			if (State == LoadState.Success && _object != null && _isValidMaterial)
			{
				_rendererProxy.SetMaterial(_material);
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

		IModel SetPatternTex(string texturePath);

		string GetPatternTex();

		IModel SetDamaged(float damaged);

		float GetDamaged();

		IModel SetMaterial(Material material);

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

	private bool _active;

	private Color _color;

	private Color _outlineColor;

	private float _damaged;

	private string _patternTexKey;

	private Texture2D _patternTex;

	private bool _isSetting;

	public GameObject Parent { get; private set; }

	public string Category { get; private set; }

	public ModelComponent ParentComponent { get; private set; }

	public int Count => _components.Count;

	public IModel this[int index] => _components[index];

	public int ChildCount
	{
		get
		{
			if (_childs == null)
			{
				return 0;
			}
			return _childs.Count;
		}
	}

	public event Action<bool> LoadCompleted;

	public event Action Unloaded;

	public event Action<IModel> ModelLoaded;

	public event Action<IModel> ModelUnloaded;

	public event Action MaterialPropertyChanged;

	public ModelComponent(GameObject parent, int randomSeed = 0)
	{
		Parent = parent;
		Category = null;
		ParentComponent = null;
		_active = true;
		_color = Color.white;
		_outlineColor = Color.clear;
		_randomSeed = randomSeed;
	}

	public ModelComponent(ModelComponent parent, string category)
	{
		Parent = parent.Parent;
		Category = category;
		ParentComponent = parent;
		_active = true;
		_color = Color.white;
		_outlineColor = Color.clear;
		_namePrefix = string.Empty;
		ModelComponent modelComponent = this;
		while (modelComponent != null && modelComponent.Category != null)
		{
			_namePrefix = modelComponent.Category + "/" + _namePrefix;
			modelComponent = modelComponent.ParentComponent;
		}
		ModelLoaded += parent.OnAssetLoaded;
		ModelUnloaded += parent.OnAssetUnloaded;
	}

	public void Reset(GameObject parent)
	{
		Clear();
		if (_childs != null)
		{
			_childs.Clear();
		}
		Parent = parent;
		Category = null;
		ParentComponent = null;
	}

	private bool IsSetting()
	{
		if (_isSetting)
		{
			return true;
		}
		int i = 0;
		for (int childCount = ChildCount; i < childCount; i++)
		{
			if (_childs[i].IsSetting())
			{
				return true;
			}
		}
		return false;
	}

	public void BeginLoad()
	{
		_isSetting = true;
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
		_isSetting = false;
		CheckLoadComplete();
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

	public ModelComponent GetChild(int index)
	{
		if (_childs == null)
		{
			return null;
		}
		return _childs[index];
	}

	public IModel Load(string key, string modelKey, string modelPostfix, string category = null)
	{
		return PathLoad(key, GetAssetPath(modelKey, modelPostfix, _randomSeed), category);
	}

	public IModel PathLoad(string key, string modelPath, string category = null, bool isFullPath = false)
	{
		ModelComponent category2 = GetCategory(category);
		string modelPath2 = (string.IsNullOrEmpty(modelPath) ? modelPath : ((!isFullPath) ? ArtifactUtil.GetAssetDirectory(modelPath) : modelPath));
		return category2.LoadModel(key, modelPath2);
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
		_isSetting = false;
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
		IModel model = GetCategory(key, make: false);
		if (model == null)
		{
			model = GetModel(null, key);
		}
		return model;
	}

	public IModel GetModel(string category, string key)
	{
		ModelComponent category2 = GetCategory(category, make: false);
		if (category2 == null)
		{
			return InvalidModel;
		}
		return category2.GetModelObject(key);
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
		string assetPath = model.ModelPath;
		Durango.Utils.Singleton<AssetBundleManager>.Instance().RequestAsset(assetPath, typeof(GameObject), delegate(UnityEngine.Object asset)
		{
			if (!(Parent == null) && assetPath.EndsWith(model.ModelPath))
			{
				if (model.State == LoadState.Unload)
				{
					Unload(model.Key);
					OnAssetLoaded(model);
				}
				else
				{
					GameObject gameObject = asset as GameObject;
					if (gameObject == null)
					{
						model.State = LoadState.Fail;
						OnAssetLoaded(model);
					}
					else
					{
						GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject, Parent.transform);
						if (gameObject2 == null)
						{
							Debug.LogError("Asset instantiation failed: " + assetPath);
							model.State = LoadState.Fail;
							OnAssetLoaded(model);
						}
						else
						{
							model.State = LoadState.Success;
							if (model.Object != null)
							{
								model.Object.SetActive(value: false);
								UnityEngine.Object.Destroy(model.Object);
							}
							model.Object = gameObject2;
							gameObject2.name = _namePrefix + model.Key;
							gameObject2.layer = Parent.layer;
							model.Refresh();
							PlaneShadowManager.ExpandBound(gameObject2);
							OnAssetLoaded(model);
						}
					}
				}
			}
		});
	}

	private void UnloadResourse(Model model)
	{
		bool flag = false;
		if (model.Object != null)
		{
			model.Object.SetActive(value: false);
			UnityEngine.Object.Destroy(model.Object);
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
		if (!IsSetting() && !IsLoading(out var isSuccess) && this.LoadCompleted != null)
		{
			this.LoadCompleted(isSuccess);
		}
	}

	private bool IsLoading(out bool isSuccess)
	{
		isSuccess = true;
		int i = 0;
		for (int count = _components.Count; i < count; i++)
		{
			switch (_components[i].State)
			{
			case LoadState.Fail:
				isSuccess = false;
				break;
			default:
				return true;
			case LoadState.Success:
			case LoadState.Unload:
				break;
			}
		}
		int j = 0;
		for (int childCount = ChildCount; j < childCount; j++)
		{
			if (_childs[j].IsLoading(out var isSuccess2))
			{
				return true;
			}
			isSuccess &= isSuccess2;
		}
		return false;
	}

	public void SetMaterialsToBeShared(Dictionary<Material, Material> materials)
	{
		int i = 0;
		for (int count = _components.Count; i < count; i++)
		{
			_components[i].SetMaterialsToBeShared(materials);
		}
		int j = 0;
		for (int childCount = ChildCount; j < childCount; j++)
		{
			_childs[j].SetMaterialsToBeShared(materials);
		}
	}

	public bool GetActive()
	{
		return _active;
	}

	public IModel SetPosition(Vector3 position)
	{
		throw new NotImplementedException();
	}

	public Vector3 GetPosition()
	{
		throw new NotImplementedException();
	}

	public IModel SetScale(Vector3 scale)
	{
		throw new NotImplementedException();
	}

	public Vector3 GetScale()
	{
		throw new NotImplementedException();
	}

	public IModel SetAngle(Vector3 angle)
	{
		throw new NotImplementedException();
	}

	public Vector3 GetAngle()
	{
		throw new NotImplementedException();
	}

	public IModel SetActive(bool active)
	{
		if (_active != active)
		{
			_active = active;
			UpdateModelActive();
		}
		return this;
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

	public float GetDamaged()
	{
		return _damaged;
	}

	public IModel SetMaterial(Material material)
	{
		throw new NotImplementedException();
	}

	public GameObject GetObject()
	{
		throw new NotImplementedException();
	}

	public bool IsNull()
	{
		throw new NotImplementedException();
	}

	public IModel SetDamaged(float damageRatio)
	{
		if (Mathf.Approximately(_damaged, damageRatio))
		{
			return this;
		}
		_damaged = damageRatio;
		if (this.MaterialPropertyChanged != null)
		{
			this.MaterialPropertyChanged();
		}
		UpdateModelDamaged();
		return this;
	}

	private void UpdateModelDamaged()
	{
		int i = 0;
		for (int count = _components.Count; i < count; i++)
		{
			_components[i].UpdateDamaged();
		}
		int j = 0;
		for (int childCount = ChildCount; j < childCount; j++)
		{
			_childs[j].UpdateModelDamaged();
		}
	}

	public string GetPatternTex()
	{
		return _patternTexKey;
	}

	public IModel SetPatternTex(string texture)
	{
		if (_patternTexKey == texture)
		{
			return this;
		}
		_patternTexKey = texture;
		if (string.IsNullOrEmpty(_patternTexKey))
		{
			SetPatternTex((Texture2D)null);
		}
		else
		{
			string patternTexturePath = GetPatternTexturePath(texture);
			Durango.Utils.Singleton<AssetBundleManager>.Instance().RequestAsset(patternTexturePath, typeof(Texture2D), delegate(UnityEngine.Object o)
			{
				if (!(o == null))
				{
					SetPatternTex((Texture2D)o);
				}
			});
		}
		return this;
	}

	public bool HasPatternTex()
	{
		int i = 0;
		for (int count = _components.Count; i < count; i++)
		{
			if (_components[i].HasPatternTex())
			{
				return true;
			}
		}
		int j = 0;
		for (int childCount = ChildCount; j < childCount; j++)
		{
			if (_childs[j].HasPatternTex())
			{
				return true;
			}
		}
		return false;
	}

	public void GetPatternCategory(HashSet<string> set)
	{
		int i = 0;
		for (int count = _components.Count; i < count; i++)
		{
			if (_components[i].HasPatternTex())
			{
				set.Add(_components[i].Key);
			}
		}
		int j = 0;
		for (int childCount = ChildCount; j < childCount; j++)
		{
			_childs[j].GetPatternCategory(set);
		}
	}

	private void SetPatternTex(Texture2D texture)
	{
		if (!(_patternTex == texture))
		{
			_patternTex = texture;
			UpdateModelPatternTex();
		}
	}

	private void UpdateModelPatternTex()
	{
		int i = 0;
		for (int count = _components.Count; i < count; i++)
		{
			_components[i].UpdatePatternTex();
		}
		int j = 0;
		for (int childCount = ChildCount; j < childCount; j++)
		{
			_childs[j].UpdateModelPatternTex();
		}
	}

	public Color GetColor()
	{
		return _color;
	}

	public IModel SetColor(Color color)
	{
		if (_color == color)
		{
			return this;
		}
		_color = color;
		if (this.MaterialPropertyChanged != null)
		{
			this.MaterialPropertyChanged();
		}
		UpdateModelColors();
		return this;
	}

	public void SetOutlineColor(Color color)
	{
		if (!(_outlineColor == color))
		{
			_outlineColor = color;
			UpdateOutlineColors();
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

	private void UpdateOutlineColors()
	{
		int i = 0;
		for (int count = _components.Count; i < count; i++)
		{
			_components[i].UpdateOutlineColor();
		}
		int j = 0;
		for (int childCount = ChildCount; j < childCount; j++)
		{
			_childs[j].UpdateOutlineColors();
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
			text = text + "_" + sub;
		}
		return artifactModel.path + "/" + text + ".prefab";
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
		string text = "preview_" + artifactModel.file_names[randomSeed % artifactModel.file_names.Length];
		return artifactModel.path + "/preview/" + text + ".prefab";
	}

	public static string GetPatternTexturePath(string texture)
	{
		return "Models/Prop/pattern/" + texture + ".psd";
	}
}
