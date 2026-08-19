using System;
using System.Collections.Generic;
using System.Linq;
using Building;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class SelectRemodelingPartPopup : TooltipBase
{
	private class PreviewPart
	{
		private bool _selected;

		public Color Color { get; private set; }

		public float? SelectedAt { get; set; }

		public float? UnselectedAt { get; set; }

		public ModelComponent.IModel Model { get; private set; }

		public void SetModel(ModelComponent.IModel model)
		{
			Model = model;
			if (model != null)
			{
				Color = model.GetColor();
			}
		}

		public void SetSelected(bool selected)
		{
			if (_selected != selected)
			{
				_selected = selected;
				if (selected)
				{
					SelectedAt = Time.time;
					UnselectedAt = null;
				}
				else
				{
					UnselectedAt = Time.time;
				}
			}
		}
	}

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIModelViewer _previewTexture;

	[SerializeField]
	private UIWidget _partsContainer;

	[SerializeField]
	private ListObjectPool _partsNodes;

	[SerializeField]
	private SelectableButton _confirmButton;

	private Artifact _artifact;

	private Dictionary<string, Blueprint> _remodelingParts;

	private string _selectedId;

	private readonly Dictionary<string, PreviewPart> _previewParts = new Dictionary<string, PreviewPart>();

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		base.OnAwake();
		_titleLabel.text = T._("무엇을 개조하시겠습니까?");
		_confirmButton.Text = T._("개조");
		_partsNodes.Init(delegate(GameObject obj)
		{
			SelectableWidget component = obj.GetComponent<SelectableWidget>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(PartClicked));
		});
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(OnConfirm));
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (_remodelingParts == null)
		{
			return;
		}
		foreach (KeyValuePair<string, PreviewPart> previewPart in _previewParts)
		{
			PreviewPart value = previewPart.Value;
			if (value.Model == null)
			{
				continue;
			}
			if (value.SelectedAt.HasValue)
			{
				float time = Time.time;
				float value2 = value.SelectedAt.Value;
				if (value.UnselectedAt.HasValue && (int)((value.UnselectedAt.Value - value2) / 1f) != (int)((time - value2) / 1f))
				{
					value.SelectedAt = null;
					value.UnselectedAt = null;
					value.Model.SetColor(value.Color);
				}
				else
				{
					float num = (time - value2) % 1f;
					num = Mathf.Abs(2f * num / 1f - 1f);
					Color color = value.Color;
					Color a = color * new Color(0.5f, 0.5f, 0.5f);
					value.Model.SetColor(Color.Lerp(a, color, num));
				}
			}
			else
			{
				value.Model.SetColor(value.Color);
			}
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		Singleton<PlayerController>.Instance().MoveStarted += PlayerController_MoveStarted;
		MakePreview();
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
	}

	protected override void FillData()
	{
		_partsNodes.BeginLoad();
		if (_remodelingParts != null)
		{
			foreach (KeyValuePair<string, Blueprint> remodelingPart in _remodelingParts)
			{
				_partsNodes.GetNext().transform.Find("Text").GetComponent<UILabel>().text = remodelingPart.Value.Name;
			}
		}
		_partsNodes.EndLoad();
		SelectPart(_selectedId);
	}

	protected override void UpdateLayout()
	{
		float num = UIUtility.WidgetsReposition(_partsNodes, Vector3.down, Vector3.zero, 20f, 0.5f);
		_partsContainer.height = (int)num + 40;
		GetComponent<RectLayoutComponent>().UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void MakePreview()
	{
		if (_artifact == null || _remodelingParts == null)
		{
			return;
		}
		bool isModular = _artifact.Blueprint.IsModular;
		ModelComponent modelComponent = _previewTexture.SetArtifactModel(new UIModelViewer.ArtifactArguments
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
			PreviewPart previewPart = new PreviewPart();
			previewPart.SetModel(modelComponent.GetCategory(key));
			previewPart.SetSelected(key == _selectedId);
			_previewParts.Add(key, previewPart);
		}
	}

	private void SelectPart(string id)
	{
		_selectedId = id;
		foreach (KeyValuePair<string, PreviewPart> previewPart in _previewParts)
		{
			previewPart.Value.SetSelected(previewPart.Key == id);
		}
		if (_remodelingParts == null)
		{
			return;
		}
		int num = 0;
		foreach (KeyValuePair<string, Blueprint> remodelingPart in _remodelingParts)
		{
			_partsNodes[num].GetComponent<Selectable>().Selected = remodelingPart.Key == id;
			num++;
		}
	}

	private void PartClicked()
	{
		if (_remodelingParts == null)
		{
			return;
		}
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
		if (_remodelingParts != null && _selectedId != null)
		{
			Blueprint blueprint = _remodelingParts[_selectedId];
			if (blueprint != null)
			{
				BuildSlotContainer slotContainer = GameSystem<BuildSystem>.Instance().SlotContainer;
				slotContainer.Set(_artifact, blueprint, GameSystem<InventorySystem>.Instance().PlayerInventory);
				UIManager.FindScript<CraftGroupBase>().Open(slotContainer);
				Hide();
			}
		}
	}

	private void PlayerController_MoveStarted()
	{
		Hide();
	}

	public void Show([NotNull] Artifact artifact)
	{
		_artifact = artifact;
		_remodelingParts = GameSystem<RecipeSystem>.Instance().RemodelingBlueprints.Get(_artifact.BlueprintId);
		switch (KUtility.GetSize(_remodelingParts))
		{
		case 1:
		{
			BuildSlotContainer slotContainer = GameSystem<BuildSystem>.Instance().SlotContainer;
			KeyValuePair<string, Blueprint> keyValuePair = _remodelingParts.First();
			slotContainer.Set(_artifact, keyValuePair.Value, GameSystem<InventorySystem>.Instance().PlayerInventory);
			UIManager.FindScript<CraftGroupBase>().Open(slotContainer);
			break;
		}
		default:
			_selectedId = _remodelingParts.First().Key;
			Show();
			break;
		case 0:
			break;
		}
	}
}
