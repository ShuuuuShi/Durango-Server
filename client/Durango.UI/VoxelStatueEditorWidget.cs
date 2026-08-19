using System;
using Durango.Logic.Statue;
using Durango.Render;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class VoxelStatueEditorWidget : UIWidget
{
	private enum PlaneType
	{
		Zx,
		Xy,
		Yz
	}

	[SerializeField]
	private VoxelPlaneEditCanvas _planeCanvas;

	[SerializeField]
	private UITexture _modelViewer;

	[SerializeField]
	private IntSelector _planeSelector;

	[SerializeField]
	private IntSelector _floorSelector;

	[SerializeField]
	private KScrollView _colorIndexSelector;

	private Texture _voxelTexture;

	private Shader _voxelShader;

	private VoxelStatue _voxel;

	private int _selectedFloor;

	private PlaneType _selectedPlane;

	private Mesh _mesh;

	private GameObject _modelObject;

	private UIModelRender _uiModelRender;

	private Transform _selectedPlaneQuad;

	private bool _isInit;

	public event Action Confirmed;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_planeSelector.Set(0, 0, 2);
		IntSelector planeSelector = _planeSelector;
		planeSelector.ValueChanged = (Action)Delegate.Combine(planeSelector.ValueChanged, (Action)delegate
		{
			Select((PlaneType)_planeSelector.Value, _selectedFloor);
		});
		IntSelector floorSelector = _floorSelector;
		floorSelector.ValueChanged = (Action)Delegate.Combine(floorSelector.ValueChanged, (Action)delegate
		{
			Select(_selectedPlane, _floorSelector.Value);
		});
		_colorIndexSelector.Nodes.Init(delegate(GameObject obj)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(obj);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickColorIndexNode));
		});
		_planeCanvas.Changed += UpdateModel;
		UIEventListener uIEventListener = UIEventListener.Get(_modelViewer.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (this.Confirmed != null)
			{
				this.Confirmed();
			}
		});
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			DestroyModel();
		}
	}

	private void OnClickColorIndexNode(GameObject obj)
	{
		int index = _colorIndexSelector.Nodes.IndexOf(obj);
		SelectColor(index);
	}

	private void SelectColor(int index)
	{
		for (int i = 0; i < _colorIndexSelector.Nodes.Count; i++)
		{
			SelectableWidget component = _colorIndexSelector.Nodes[i].GetComponent<SelectableWidget>();
			component.Selected = i == index;
		}
		if (index != -1)
		{
			_planeCanvas.Value = (byte)index;
		}
	}

	public void Set(VoxelStatue voxel, Texture voxelTexture, Shader voxelShader)
	{
		Init();
		_voxel = voxel;
		_voxelTexture = voxelTexture;
		_voxelShader = voxelShader;
		ListObjectPool nodes = _colorIndexSelector.Nodes;
		nodes.Set(KUtility.GetSize(_voxel.Colors) + 1);
		SetNode(nodes[0], Color.clear);
		for (int i = 1; i < nodes.Count; i++)
		{
			SetNode(nodes[i], _voxel.Colors[i - 1]);
		}
		_colorIndexSelector.ResetPosition();
		SelectColor(1);
		UpdateModel();
		Select(PlaneType.Zx, 0);
	}

	private void SetNode(GameObject obj, Color col)
	{
		UISprite component = obj.transform.Find("Sprite").GetComponent<UISprite>();
		component.color = col;
	}

	private void UpdateModel()
	{
		_mesh = VoxelMeshBuilder.Make(_mesh, _voxel);
		if (!(_modelObject != null))
		{
			_modelObject = new GameObject();
			MeshFilter meshFilter = _modelObject.AddComponent<MeshFilter>();
			meshFilter.sharedMesh = _mesh;
			MeshRenderer meshRenderer = _modelObject.AddComponent<MeshRenderer>();
			Material material = new Material(_voxelShader);
			material.mainTexture = _voxelTexture;
			meshRenderer.sharedMaterial = material;
			GameObject gameObject = _modelObject.AddChild();
			MeshFilter meshFilter2 = gameObject.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer2 = gameObject.AddComponent<MeshRenderer>();
			Material material2 = new Material(Shader.Find("Durango/Unlit/Diffuse"));
			BlendUtil.SetBlendMode(material2, BlendMode.Transparent);
			material2.SetFloat("_Culling", 0f);
			material2.mainTexture = Texture2D.whiteTexture;
			meshRenderer2.sharedMaterial = material2;
			Mesh mesh = new Mesh();
			mesh.vertices = new Vector3[4]
			{
				new Vector3(-0.5f, 0f, -0.5f),
				new Vector3(-0.5f, 0f, 0.5f),
				new Vector3(0.5f, 0f, 0.5f),
				new Vector3(0.5f, 0f, -0.5f)
			};
			mesh.uv = new Vector2[4]
			{
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
				new Vector2(1f, 0f)
			};
			Color white = Color.white;
			white.a = 0.5f;
			mesh.colors = new Color[4] { white, white, white, white };
			mesh.triangles = new int[6] { 0, 1, 2, 2, 3, 0 };
			meshFilter2.sharedMesh = mesh;
			_selectedPlaneQuad = gameObject.transform;
			_uiModelRender = UIModelRenderBuilder.Make();
			Vector3 vector = new Vector3(_voxel.Size.X, _voxel.Size.Y, _voxel.Size.Z) * 1.5f;
			Bounds value = new Bounds(vector * 0.5f, vector);
			UIModelRender uiModelRender = _uiModelRender;
			GameObject modelObject = _modelObject;
			float cameraAngle = 35f;
			Bounds? bounds = value;
			uiModelRender.SetModel(modelObject, cameraAngle, 1f, bounds);
			_uiModelRender.FillTexture(_modelViewer);
		}
	}

	private void DestroyModel()
	{
		if (!(_modelObject == null))
		{
			_modelViewer.mainTexture = null;
			UIModelRenderBuilder.Release(_uiModelRender);
			_uiModelRender = null;
		}
	}

	private void Select(PlaneType plane, int floor)
	{
		_selectedPlane = plane;
		_selectedFloor = floor;
		int num;
		switch (_selectedPlane)
		{
		default:
			return;
		case PlaneType.Xy:
			num = _voxel.Size.Z;
			_selectedPlaneQuad.localScale = new Vector3((float)_voxel.Size.X * 1.1f, 1f, (float)_voxel.Size.Y * 1.1f);
			_selectedPlaneQuad.localEulerAngles = new Vector3(90f, 0f, 0f);
			_selectedPlaneQuad.localPosition = new Vector3((float)_voxel.Size.X * 0.5f, (float)_voxel.Size.Y * 0.5f, (float)_voxel.Size.Z - ((float)floor + 0.5f));
			break;
		case PlaneType.Yz:
			num = _voxel.Size.X;
			_selectedPlaneQuad.localScale = new Vector3((float)_voxel.Size.Y * 1.1f, 1f, (float)_voxel.Size.Z * 1.1f);
			_selectedPlaneQuad.localEulerAngles = new Vector3(0f, 0f, 90f);
			_selectedPlaneQuad.localPosition = new Vector3((float)_voxel.Size.X - ((float)floor + 0.5f), (float)_voxel.Size.Y * 0.5f, (float)_voxel.Size.Z * 0.5f);
			break;
		case PlaneType.Zx:
			num = _voxel.Size.Y;
			_selectedPlaneQuad.localScale = new Vector3((float)_voxel.Size.X * 1.1f, 1f, (float)_voxel.Size.Z * 1.1f);
			_selectedPlaneQuad.localEulerAngles = new Vector3(0f, 0f, 0f);
			_selectedPlaneQuad.localPosition = new Vector3((float)_voxel.Size.X * 0.5f, (float)floor + 0.5f, (float)_voxel.Size.Z * 0.5f);
			break;
		}
		_floorSelector.Set(_selectedFloor, 0, num - 1);
		_planeSelector.Value = (int)plane;
		_floorSelector.Value = floor;
		Vector3 side;
		switch (plane)
		{
		default:
			return;
		case PlaneType.Xy:
			side = Vector3.back;
			break;
		case PlaneType.Yz:
			side = Vector3.left;
			break;
		case PlaneType.Zx:
			side = Vector3.up;
			break;
		}
		_planeCanvas.SetVoxel(_voxel, side, floor);
	}
}
