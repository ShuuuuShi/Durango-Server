using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class RecipeStepSelectWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _labelProgress;

	[SerializeField]
	private UILabel _textProgress;

	[SerializeField]
	private KScrollView _recipeSlots;

	private bool _initialized;

	private SlotContainer _slotContainer;

	private bool _resetFlag = true;

	private void OnDisable()
	{
		_resetFlag = true;
	}

	public void Set(SlotContainer slotContainer)
	{
		Init();
		_slotContainer = slotContainer;
	}

	public void Refresh()
	{
		if (_slotContainer != null)
		{
			ListObjectPool nodes = _recipeSlots.Nodes;
			nodes.Set(_slotContainer.SlotCount);
			for (int i = 0; i < _slotContainer.SlotCount; i++)
			{
				RecipeSlotWidget component = nodes[i].GetComponent<RecipeSlotWidget>();
				SlotInfo slotInfo = _slotContainer.GetSlotInfo(i);
				component.Set(slotInfo, slotInfo == _slotContainer.CurrentSlot);
			}
			RefreshProgressPercentage();
			_recipeSlots.Reposition(_resetFlag, !_resetFlag);
			_resetFlag = false;
		}
	}

	public void RefreshSlot(int index)
	{
		RecipeSlotWidget component = _recipeSlots.Nodes[index].GetComponent<RecipeSlotWidget>();
		if (component != null)
		{
			component.Refresh(component.SlotInfo == _slotContainer.CurrentSlot);
		}
	}

	public void RefreshProgressPercentage()
	{
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < _slotContainer.SlotCount; i++)
		{
			SlotInfo slotInfo = _slotContainer.GetSlotInfo(i);
			if (slotInfo != null)
			{
				num += slotInfo.CurrentCount;
				num2 += slotInfo.TotalCount;
			}
		}
		float num3 = Mathf.Clamp01((num2 != 0) ? ((float)num / (float)num2) : 0f);
		_textProgress.text = $"{num3:P0}";
	}

	public RecipeSlotWidget GetNextRecipeSlotWidget()
	{
		if (_slotContainer == null || _slotContainer.CurrentSlot.State != SlotInfo.SlotState.FullSelected)
		{
			return null;
		}
		for (int i = 0; i < _recipeSlots.Nodes.Count; i++)
		{
			RecipeSlotWidget component = _recipeSlots.Nodes[i].GetComponent<RecipeSlotWidget>();
			if (!(component == null) && component.SlotInfo.State != SlotInfo.SlotState.FullSelected)
			{
				return component;
			}
		}
		return null;
	}

	private void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			_recipeSlots.Nodes.Init(delegate(GameObject obj)
			{
				RecipeSlotWidget component = obj.GetComponent<RecipeSlotWidget>();
				component.Clicked += SlotWidget_OnClick;
			});
			if ((bool)_labelProgress)
			{
				_labelProgress.text = T._("작업 완료");
			}
		}
	}

	private void SlotWidget_OnClick(GameObject obj)
	{
		RecipeSlotWidget component = obj.GetComponent<RecipeSlotWidget>();
		if (component.SlotInfo != null)
		{
			_slotContainer.SetCurrentSlotIndex(component.SlotInfo.Index);
		}
	}
}
