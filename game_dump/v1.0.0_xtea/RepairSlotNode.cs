using BuildData;
using ItemSystem;
using UnityEngine;

public class RepairSlotNode : MonoBehaviour
{
	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UISprite _lineSprite;

	private UIWidget _widget;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void RefreshLayout()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f - ((Component)_descriptionLabel).transform.localPosition.y;
		num += _descriptionLabel.printedSize.y;
		num += 0f - ((Component)_nameLabel).transform.localPosition.y;
		Widget.height = (int)num;
		_lineSprite.UpdateAnchors();
	}

	public void Set(RepairSlot slot)
	{
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		_nameLabel.text = slot.name;
		int num = ((slot.materials != null) ? slot.materials.Length : 0) + slot.selectItems.Count;
		_countLabel.text = $"{num}/{slot.count}";
		_descriptionLabel.text = $"[{Util.LocalizedTagRequiredMsg(slot.requiredTags, showLevel: false)}][{Util.LocalizedTagRequiredMsg(slot.requiredMaterials, showLevel: false)}]\n\n{slot.description}";
		float alpha = _background.alpha;
		_background.color = ((num != slot.count) ? UIManager.UIBlack : UIManager.UIGreen);
		_background.alpha = alpha;
		RefreshLayout();
	}
}
