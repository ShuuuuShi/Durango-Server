using System.IO;
using Building;
using Durango.Logic.Statue;
using Durango.Render;
using Durango.Terrain;
using Durango.UI.Control;
using Durango.UI.InGame;
using Durango.Utils;
using Durango.Utils.Extensions;
using L10N;
using UnityEngine;

namespace Durango.UI;

[Uri("Voxel")]
public class VoxelStatueEditorGroup : UIBase
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private VoxelStatueEditorWidget _editorWidget;

	[SerializeField]
	private Shader _voxelShader;

	[SerializeField]
	private Material _shadowMaterial;

	private VoxelStatue _voxel;

	private int _tileSize;

	private void Start()
	{
		_titleWidget.Object.SetTitle(T._("그림판"));
		SetChildrenActive(activated: false);
		_editorWidget.Confirmed += OnConfirmed;
	}

	public void Open(VoxelStatue voxel)
	{
		Open();
		_voxel = voxel;
		_editorWidget.Set(voxel, Texture2D.whiteTexture, _voxelShader);
	}

	private void OnConfirmed()
	{
		Close();
		using (FileStream fileStream = AppData.OpenFile("Player/Voxel/text_voxel.bytes"))
		{
			if (fileStream != null)
			{
				fileStream.SetLength(0L);
				fileStream.Write(_voxel.Bytes, 0, _voxel.Bytes.Length);
			}
		}
		Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint("bonfire");
		VoxelStatue voxel = _voxel;
		UIManager.FindScript<BuildGridGroup>().Open(blueprint, delegate
		{
			if (voxel != null)
			{
				BuildLocator buildLocator = Singleton<BuildLocator>.Instance();
				GameObject gameObject = new GameObject();
				MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
				Mesh sharedMesh = VoxelMeshBuilder.Make(null, voxel);
				meshFilter.sharedMesh = sharedMesh;
				MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
				Material material = new Material(_voxelShader)
				{
					mainTexture = Texture2D.whiteTexture
				};
				meshRenderer.sharedMaterials = new Material[2] { material, _shadowMaterial };
				float num = (float)(Mathf.Max(1, _tileSize) * 200) / (float)Mathf.Max(voxel.Size.X, voxel.Size.Z);
				gameObject.transform.position = Util.TilePositionToClientPosition(buildLocator.WorldTilePos);
				gameObject.transform.localScale = Vector3.one * num;
			}
		});
	}

	protected override void DefaultUri()
	{
		int num = UriParser.GetArgument("size").ToInt();
		int num2 = UriParser.GetArgument("tile").ToInt();
		if (num <= 0)
		{
			num = 16;
		}
		if (num2 <= 0)
		{
			num2 = 1;
		}
		byte[] array = new byte[num * num * num];
		using (FileStream fileStream = AppData.OpenFile("Player/Voxel/text_voxel.bytes", FileMode.Open))
		{
			if (fileStream != null && fileStream.Length == array.Length)
			{
				fileStream.Read(array, 0, array.Length);
			}
		}
		_tileSize = num2;
		VoxelStatue voxelStatue = new VoxelStatue
		{
			Bytes = array,
			Size = new Size3(num, num, num)
		};
		voxelStatue.Colors = ColorTableLoader.GetAll($"color_board_L{1:00}.raw");
		Open(voxelStatue);
	}
}
