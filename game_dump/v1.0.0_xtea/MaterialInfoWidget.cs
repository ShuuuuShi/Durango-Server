using ItemSystem;
using L10N;
using UnityEngine;

public class MaterialInfoWidget : MonoBehaviour
{
	public enum WarningType
	{
		None,
		InsufficientTagLevel,
		Unmodifiable,
		SelectedByOtherSlot,
		PreviouslyAssigned,
		Locked
	}

	[SerializeField]
	private GameObject _noSelectedPanel;

	[SerializeField]
	private GameObject _selectedPanel;

	[SerializeField]
	private UIWidget _warningPanel;

	[SerializeField]
	private UILabel _textItemName;

	[SerializeField]
	private UILabel _textItemLevel;

	[SerializeField]
	private UISprite _iconDurability;

	[SerializeField]
	private UILabel _textDurability;

	[SerializeField]
	private UISprite IconModifiableCount;

	[SerializeField]
	private UILabel _textModifiableCount;

	[SerializeField]
	private UISpriteLabel _textWarning;

	[SerializeField]
	private ItemTagControlList _majorTagWidgets;

	[SerializeField]
	private ItemTagControlList _minorTagWidgets;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private string[] _durabilityIconNames;

	[SerializeField]
	private int[] _durabilityValues;

	[SerializeField]
	private string _modifiableIconName;

	[SerializeField]
	private string _unmodifiableIconName;

	private bool _initialized;

	private Vector3 _posTags;

	public void Init()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!_initialized)
		{
			_majorTagWidgets.Init(null);
			_minorTagWidgets.Init(null);
			_posTags = _majorTagWidgets.BaseObject.transform.localPosition;
			UIUtility.SetScrollViewInvisibleBox(_scrollView);
			_initialized = true;
		}
	}

	public void SetMaterial(ItemIcon2 material, WarningType warningType)
	{
		_noSelectedPanel.SetActive((Object)(object)material == (Object)null);
		_selectedPanel.SetActive((Object)(object)material != (Object)null);
		if ((Object)(object)material != (Object)null)
		{
			int contentCount = material.Item.ContentCount;
			ItemData itemData;
			if (contentCount > 0)
			{
				itemData = material.Item.GetContent(0);
				ShowItemNameAndLevel($"{itemData.Name} ({material.Item.Name})", T.Format("{0:lv:}", itemData.Level));
			}
			else
			{
				itemData = material.Item;
				ShowItemNameAndLevel(itemData.Name, T.Format("{0:lv:}", itemData.Level));
			}
			ShowDurability(itemData.Durability);
			ShowModifiableInfo(itemData.ModifiableCount);
			int offsetHeight = ShowWarningPanel(warningType);
			ShowTags(itemData, offsetHeight);
		}
	}

	private void ShowItemNameAndLevel(string name, string levelText)
	{
		UIUtility.SetLabelText(_textItemName, name);
		UIUtility.SetLabelText(_textItemLevel, levelText);
		UIUtility.AlignRightByLabel(_textItemLevel, _textItemName, 12);
	}

	private void ShowDurability(Gauge gauge)
	{
		float num = gauge.Get();
		float num2 = gauge.Max();
		int percentage = ((num2 > 0f) ? Mathf.CeilToInt(num / num2 * 100f) : 0);
		UIUtility.SetSpriteName(_iconDurability, UIUtility.GetValueByPercentage(percentage, _durabilityValues, _durabilityIconNames));
		UIUtility.SetLabelText(_textDurability, Util.LocalizedDurability(num, num2));
	}

	private void ShowModifiableInfo(int modifiableCount)
	{
		UIUtility.SetSpriteName(IconModifiableCount, (modifiableCount <= 0) ? _unmodifiableIconName : _modifiableIconName);
		UIUtility.SetLabelText(_textModifiableCount, Util.LocalizedModifiableCount(modifiableCount));
	}

	private int ShowWarningPanel(WarningType warningType)
	{
		string text = ((warningType == WarningType.None) ? string.Empty : ("#item_info_warning_" + warningType));
		if (text != string.Empty)
		{
			((Component)_textWarning).gameObject.SetActive(true);
			_textWarning.text = LocalizeSystem.Get(text);
			_warningPanel.ResetAndUpdateAnchors();
			return _warningPanel.height;
		}
		((Component)_textWarning).gameObject.SetActive(false);
		return 0;
	}

	private void ShowTags(ItemData itemData, int offsetHeight)
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		_majorTagWidgets.Clear();
		_minorTagWidgets.Clear();
		int count = itemData.Tags.Count;
		for (int i = 0; i < count; i++)
		{
			TagData tagData = itemData.Tags[i];
			if (tagData.Visible != TagData.VisibleType.Hide)
			{
				ItemTagControlList itemTagControlList = ((tagData.Display != 0) ? _minorTagWidgets : _majorTagWidgets);
				ItemTagControl itemTagControl = ((ListObjectPoolBase<GameObject>)itemTagControlList).Add<ItemTagControl>();
				itemTagControl.Name = tagData.LocalizedName;
				itemTagControl.Level = tagData.Level;
				itemTagControl.Icon = tagData.Icon;
			}
		}
		Vector3 posTags = _posTags;
		posTags.y -= (float)offsetHeight;
		posTags = _majorTagWidgets.UpdateLayout(posTags, 2);
		_minorTagWidgets.UpdateLayout(posTags, 1);
		_scrollView.ResetPosition();
	}
}
