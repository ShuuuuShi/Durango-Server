using System;
using ItemSystem;
using L10N;
using Shared.System;
using UnityEngine;

public class CageManageGroup : UIBase
{
	[SerializeField]
	private GameObject _closeButton;

	[SerializeField]
	private CageAnimalListWidget _animalList;

	[SerializeField]
	private DefaultSelectableButton _actionButton;

	private Cage _cage;

	private void Awake()
	{
		base.OnClose();
	}

	private void Start()
	{
		_animalList.Selected += OnSelectAnimal;
		UIEventListener uIEventListener = UIEventListener.Get(_closeButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			Close();
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.Cage, delegate(InteractionObject o)
		{
			Open(o.GetTargetComponent<Artifact>());
		});
		DefaultSelectableButton actionButton = _actionButton;
		actionButton.Clicked = (Action)Delegate.Combine(actionButton.Clicked, new Action(OnClickActionButton));
	}

	public void Open(Artifact artifact)
	{
		if (!((Object)(object)artifact == (Object)null))
		{
			_cage = artifact.GetArtifactComponent<Cage>();
			Open();
		}
	}

	protected override bool OnOpen()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (_cage == null)
		{
			return false;
		}
		Rect val = default(Rect);
		((Rect)(ref val))._002Ector(_cage.WorldTile.ToVector2(), _cage.Size.ToVector2());
		SelectAreaUI.AreaStruct areaStruct = default(SelectAreaUI.AreaStruct);
		areaStruct.Pos = new Point2(((Rect)(ref val)).position);
		areaStruct.Size = new Point2(((Rect)(ref val)).size);
		areaStruct.Color = PresetColor.UIYellow;
		SelectAreaUI.AreaStruct areaStruct2 = areaStruct;
		KSingleton<SelectAreaUI>.Instance().Show(new SelectAreaUI.AreaStruct[1] { areaStruct2 });
		_animalList.Set(_cage);
		KSingleton<PlayerController>.Instance().OnPickObject += OnPickObject;
		Artifact.ArtifactStateChanged += OnArtifactStateChange;
		KSingleton<PlayerController>.Instance().MoveLock = true;
		return base.OnOpen();
	}

	protected override bool OnClose()
	{
		KSingleton<SelectAreaUI>.Instance().Hide();
		KSingleton<PlayerController>.Instance().OnPickObject -= OnPickObject;
		Artifact.ArtifactStateChanged -= OnArtifactStateChange;
		KSingleton<PlayerController>.Instance().MoveLock = false;
		return base.OnClose();
	}

	private void OnSelectAnimal(ItemData reins)
	{
		ulong num = reins?.Id ?? 0;
		int num2 = Util.IndexOf(_cage.ReinsList, num);
		_actionButton.Text = ((num2 != -1) ? T._("[icon=img_pet_arrow_down:1] 빼기") : T._("[icon=img_pet_arrow_up:1] 넣기"));
		_actionButton.Disable = num == 0;
	}

	private void OnPickObject(Ray ray, PlayerController.TouchEvent touch, ref bool result)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		if (KUtility.RayCastContextAction(ray, LayerMask.op_Implicit(LayerHelper.DefaultMask), null, out var pickingObject))
		{
			ulong entityId = ObjectIdentifier.GetEntityId(pickingObject);
			_animalList.SelectAnimal(entityId);
			touch.IsNguiTouched = true;
		}
		else
		{
			_animalList.SelectAnimal(0uL);
		}
		result = true;
	}

	private void OnArtifactStateChange(Artifact artifact)
	{
		if (!((Object)(object)artifact != (Object)(object)_cage.Artifact))
		{
			_animalList.Set(_cage);
		}
	}

	private void OnClickActionButton()
	{
		ulong selectedAnimal = _animalList.SelectedAnimal;
		if (selectedAnimal != 0L)
		{
			int num = Util.IndexOf(_cage.ReinsList, selectedAnimal);
			if (num == -1)
			{
				_animalList.PutInAnimation(selectedAnimal);
				GameSystem<BuildSystem>.Instance().PutInCage(_cage, selectedAnimal);
			}
			else
			{
				_animalList.TakeOutAnimation(selectedAnimal);
				GameSystem<BuildSystem>.Instance().TakeOutCage(_cage, selectedAnimal);
			}
		}
	}
}
