using L10N;
using UnityEngine;

public class RecipeStepSelectWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _labelProgress;

	[SerializeField]
	private UILabel _textProgress;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private ListObjectPool _recipeSlotsWidgets;

	private bool _initialized;

	private SlotContainer _slotContainer;

	public void Set(SlotContainer slotContainer)
	{
		Init();
		_slotContainer = slotContainer;
	}

	public void Refresh()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = _recipeSlotsWidgets.BaseObject.transform.localPosition;
		int height = _recipeSlotsWidgets.BaseObject.GetComponent<UIWidget>().height;
		_recipeSlotsWidgets.Clear();
		for (int i = 0; i < _slotContainer.SlotCount; i++)
		{
			RecipeSlotWidget recipeSlotWidget = ((ListObjectPoolBase<GameObject>)_recipeSlotsWidgets).Add<RecipeSlotWidget>();
			recipeSlotWidget.SlotInfo = _slotContainer.GetSlotInfo(i);
			recipeSlotWidget.Refresh(_slotContainer.CurrentSlot);
			((Component)recipeSlotWidget).transform.localPosition = localPosition + Vector3.down * (float)(height * i);
		}
		RefreshProgressPercentage();
		_scrollView.ResetPosition();
	}

	public void RefreshSlot(int index)
	{
		RecipeSlotWidget slotWidget = GetSlotWidget(index);
		if ((Object)(object)slotWidget != (Object)null)
		{
			slotWidget.Refresh(_slotContainer.CurrentSlot);
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
				num2 += slotInfo.MaxCount;
			}
		}
		int num3 = ((num2 != 0) ? (100 * num / num2) : 100);
		UIUtility.SetLabelText(_textProgress, $"{num3}%");
	}

	public RecipeSlotWidget GetNextRecipeSlotWidget()
	{
		if (_slotContainer == null || _slotContainer.CurrentSlot.State != SlotInfo.SlotState.FullSelected)
		{
			return null;
		}
		for (int i = 0; i < _recipeSlotsWidgets.Count; i++)
		{
			RecipeSlotWidget slotWidget = GetSlotWidget(i);
			if (!((Object)(object)slotWidget == (Object)null) && slotWidget.SlotInfo.State != SlotInfo.SlotState.FullSelected)
			{
				return slotWidget;
			}
		}
		return null;
	}

	private void Init()
	{
		if (!_initialized)
		{
			_recipeSlotsWidgets.Init(delegate(GameObject obj)
			{
				RecipeSlotWidget component = obj.GetComponent<RecipeSlotWidget>();
				component.Init();
				component.OnClick += SlotWidget_OnClick;
			});
			UIUtility.SetLabelText(_labelProgress, T._("작업 완료"));
			UIUtility.SetScrollViewInvisibleBox(_scrollView);
			_initialized = true;
		}
	}

	private RecipeSlotWidget GetSlotWidget(int index)
	{
		if (0 <= index && index <= _recipeSlotsWidgets.Count)
		{
			return ((ListObjectPoolBase<GameObject>)_recipeSlotsWidgets).Get<RecipeSlotWidget>(index);
		}
		return null;
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
