using System;
using System.Collections.Generic;
using System.Linq;
using Building;
using Durango.Network;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class SelectModularPartTexturePopup : TooltipBase
{
	private static readonly string[] AvailableTextures = new string[47]
	{
		"fur_common_01", "fur_mammoth_01", "fur_megaloceros_snowy_01", "fur_sabertooth_tem_01", "fur_sabertooth_tem_02", "fur_skunkodus_01", "fur_skunkodus_02", "leather_allosaurus_01", "leather_amargasaurus", "leather_apatosaurus_01",
		"leather_apatosaurus_02", "leather_blue_01", "leather_bm_01", "leather_common_01", "leather_compsognathus_01", "leather_euoplocephalus_01", "leather_halloween_01", "leather_halloween_02", "leather_iguana", "leather_iguana_01",
		"leather_iguana_02", "leather_macrauchenia_01", "leather_pachycephalosaurus_01", "leather_parasaurolophus_01", "leather_phenacodus_01", "leather_pink_01", "leather_styracosaurus_deer_01", "leather_tarbosaurus_01", "leather_warehouse_common_01", "leather_xmas01",
		"leather_xmas02", "leather_yellow_01", "leather_zebraceratops_01", "stone_basalt_01", "stone_bm_01", "stone_common_01", "stone_granite_01", "stone_marble_01", "stone_obsidian_01", "woodplank_almond01",
		"woodplank_common_01", "woodplank_mahogany01", "woodplank_mangrove01", "woodplank_oak01", "woodplank_pinaceae01", "woodplank_pinaceae02", "woodplank_pinaceae03"
	};

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIModelViewer _previewTexture;

	[SerializeField]
	private UIWidget _partsContainer;

	[SerializeField]
	private ListObjectPool _partsNodes;

	[SerializeField]
	private UIWidget _texturesContainer;

	[SerializeField]
	private KGridScrollView _textureScrollView;

	[SerializeField]
	private SelectableButton _confirmButton;

	private Artifact _artifact;

	private readonly Dictionary<string, Blueprint> _remodelingParts = new Dictionary<string, Blueprint>();

	private readonly HashSet<string> _categories = new HashSet<string>();

	private List<string> _textures;

	private string _selectedId;

	private string _selectedTexture;

	private ModelComponent _previewModel;

	private readonly Dictionary<string, ModelComponent.IModel> _previewParts = new Dictionary<string, ModelComponent.IModel>();

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		base.OnAwake();
		_partsNodes.Init(delegate(GameObject obj)
		{
			SelectableWidget component2 = obj.GetComponent<SelectableWidget>();
			component2.Clicked = (Action)Delegate.Combine(component2.Clicked, new Action(OnClickPart));
		});
		_textureScrollView.Nodes.Init(delegate(GameObject obj)
		{
			SelectableWidget component = obj.GetComponent<SelectableWidget>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickTexture));
		});
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(OnConfirm));
	}

	protected override void OnShow()
	{
		base.OnShow();
		Singleton<PlayerController>.Instance().MoveStarted += PlayerController_MoveStarted;
		MakePreview();
		ShowPartSelectPage();
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (Singleton<PlayerController>.HasInstance())
		{
			Singleton<PlayerController>.Instance().MoveStarted -= PlayerController_MoveStarted;
		}
		_selectedId = null;
		_previewParts.Clear();
		_previewModel = null;
		_categories.Clear();
	}

	protected override void FillData()
	{
		_partsNodes.BeginLoad();
		foreach (KeyValuePair<string, Blueprint> remodelingPart in _remodelingParts)
		{
			GameObject next = _partsNodes.GetNext();
			next.transform.Find("Text").GetComponent<UILabel>().text = remodelingPart.Value.Name;
		}
		_partsNodes.EndLoad();
		if (_textures == null)
		{
			FillTextureList();
		}
		SelectPart(_selectedId);
	}

	protected override void UpdateLayout()
	{
		Vector3[] localCorners = _partsContainer.localCorners;
		Vector3 vector = Vector3.Lerp(localCorners[1], localCorners[2], 0.5f);
		UIUtility.WidgetsReposition(_partsNodes, Vector3.down, vector + Vector3.down * 20f, 20f);
		GetComponent<RectLayoutComponent>().UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void FillTextureList()
	{
		_textures = new List<string>();
		_textures.Add(string.Empty);
		_textures.AddRange(AvailableTextures);
		_textureScrollView.Nodes.BeginLoad();
		foreach (string texture in _textures)
		{
			GameObject next = _textureScrollView.Nodes.GetNext();
			SelectableWidget component = next.GetComponent<SelectableWidget>();
			component.Selected = false;
			UITexture comp = component.transform.Find("Texture").GetComponent<UITexture>();
			if (string.IsNullOrEmpty(texture))
			{
				comp.mainTexture = Texture2D.whiteTexture;
				continue;
			}
			string patternTexturePath = ModelComponent.GetPatternTexturePath(texture);
			Singleton<AssetBundleManager>.Instance().RequestAsset(patternTexturePath, typeof(Texture), delegate(UnityEngine.Object obj)
			{
				comp.mainTexture = obj as Texture;
			});
		}
		_textureScrollView.Nodes.EndLoad();
	}

	private void ShowPartSelectPage()
	{
		_partsContainer.gameObject.SetActive(value: true);
		_texturesContainer.gameObject.SetActive(value: false);
		_titleLabel.text = T._("무엇을 개조하시겠습니까?");
		_confirmButton.Text = T._("개조");
	}

	private void ShowTextureSelectPage()
	{
		_partsContainer.gameObject.SetActive(value: false);
		_texturesContainer.gameObject.SetActive(value: true);
		Blueprint blueprint = ((!string.IsNullOrEmpty(_selectedId)) ? _remodelingParts.Get(_selectedId) : null);
		_titleLabel.text = ((blueprint != null) ? blueprint.Name : string.Empty);
		_confirmButton.Text = T._("확인");
		ModelComponent.IModel model = _previewParts.Get(_selectedId);
		string texture = null;
		if (model != null)
		{
			texture = model.GetPatternTex();
		}
		SelectTexture(texture);
	}

	private void MakePreview()
	{
		if (_artifact == null)
		{
			return;
		}
		bool isModular = _artifact.Blueprint.IsModular;
		_previewModel = _previewTexture.SetArtifactModel(new UIModelViewer.ArtifactArguments
		{
			Display = _artifact.Display,
			Size = _artifact.Size,
			Rotation = _artifact.Rotation,
			Stories = _artifact.Stories.Value.GetValueOrDefault(),
			HasRoof = _artifact.HasRoof.Value,
			IsModular = isModular
		}, new UIModelViewer.Arguments
		{
			CameraAngle = 35f,
			Rotation = -45f
		});
		_previewParts.Clear();
		foreach (KeyValuePair<string, Blueprint> remodelingPart in _remodelingParts)
		{
			string key = remodelingPart.Key;
			ModelComponent category = _previewModel.GetCategory(key, make: false);
			if (category != null)
			{
				_previewParts.Add(key, category);
			}
		}
	}

	private void SelectPart(string id)
	{
		_selectedId = id;
		int num = 0;
		foreach (KeyValuePair<string, Blueprint> remodelingPart in _remodelingParts)
		{
			Selectable component = _partsNodes[num].GetComponent<Selectable>();
			component.Selected = remodelingPart.Key == id;
			num++;
		}
	}

	private void SelectTexture(string texture)
	{
		_selectedTexture = texture;
		for (int i = 0; i < _textures.Count; i++)
		{
			bool selected = _textures[i] == texture;
			Selectable component = _textureScrollView.Nodes[i].GetComponent<Selectable>();
			component.Selected = selected;
		}
		if (_categories.Count > 0)
		{
			for (int j = 0; j < _previewModel.Count; j++)
			{
				_previewModel[j].SetPatternTex(texture);
			}
		}
		else
		{
			_previewParts.Get(_selectedId)?.SetPatternTex(texture);
		}
	}

	private void OnClickPart()
	{
		int num = _partsNodes.IndexOf(Selectable.Current.gameObject);
		if (num == -1)
		{
			return;
		}
		KeyValuePair<string, Blueprint>? keyValuePair = null;
		foreach (KeyValuePair<string, Blueprint> remodelingPart in _remodelingParts)
		{
			if (num == 0)
			{
				keyValuePair = remodelingPart;
				break;
			}
			num--;
		}
		if (keyValuePair.HasValue)
		{
			SelectPart(keyValuePair.Value.Key);
		}
	}

	private void OnClickTexture()
	{
		int num = _textureScrollView.Nodes.IndexOf(Selectable.Current.gameObject);
		if (num >= 0 && num < KUtility.GetSize(_textures))
		{
			string text = _textures[num];
			if (string.IsNullOrEmpty(text))
			{
				text = null;
			}
			if (!(text == _selectedTexture))
			{
				SelectTexture(text);
			}
		}
	}

	protected override void OnTryConfirmOnModal()
	{
		OnConfirm();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}

	private void OnConfirm()
	{
		if (_selectedId == null)
		{
			return;
		}
		Blueprint blueprint = _remodelingParts[_selectedId];
		if (blueprint == null)
		{
			return;
		}
		if (_partsContainer.gameObject.activeSelf)
		{
			ShowTextureSelectPage();
			return;
		}
		ArtifactDisplay display = _artifact.Display;
		string[] array = ((_categories.Count > 0) ? _categories.ToArray() : new string[1] { _selectedId });
		string selectedTexture = _selectedTexture;
		Hide();
		string[] array2 = array;
		foreach (string key in array2)
		{
			Dictionary<string, string> dictionary = display.Textures;
			if (string.IsNullOrEmpty(selectedTexture))
			{
				if (dictionary == null)
				{
					return;
				}
				string value = dictionary.Get(key);
				if (string.IsNullOrEmpty(value))
				{
					return;
				}
				dictionary.Remove(key);
			}
			else
			{
				if (dictionary == null)
				{
					dictionary = (display.Textures = new Dictionary<string, string>());
				}
				string text = dictionary.Get(key);
				if (text == selectedTexture)
				{
					return;
				}
				dictionary[key] = selectedTexture;
			}
		}
		Connections.Frontend.Send(display);
		BuildSystem.PlayBuildFinished(_artifact.Center);
	}

	private void PlayerController_MoveStarted()
	{
		Hide();
	}

	public void Show([NotNull] Artifact artifact)
	{
		_artifact = artifact;
		_remodelingParts.Clear();
		_categories.Clear();
		bool flag = false;
		ModularArtifact artifactComponent = artifact.GetArtifactComponent<ModularArtifact>();
		if (artifactComponent != null)
		{
			Dictionary<string, Blueprint> dictionary = GameSystem<RecipeSystem>.Instance().RemodelingBlueprints.Get(_artifact.BlueprintId);
			if (dictionary == null)
			{
				return;
			}
			foreach (KeyValuePair<string, Blueprint> item in dictionary)
			{
				ModelComponent category = _artifact.Models.GetCategory(item.Key, make: false);
				if (category != null && category.HasPatternTex())
				{
					_remodelingParts.Add(item.Key, item.Value);
				}
			}
		}
		else
		{
			_artifact.Models.GetPatternCategory(_categories);
			if (_categories.Count > 0)
			{
				_remodelingParts.Add("common", new Blueprint
				{
					Name = T._("개조")
				});
				flag = true;
			}
		}
		if (KUtility.GetSize(_remodelingParts) == 0)
		{
			UIManager.SystemMsg(T._("개조 불가"));
			return;
		}
		_selectedId = _remodelingParts.First().Key;
		Show();
		if (flag)
		{
			OnConfirm();
		}
	}
}
