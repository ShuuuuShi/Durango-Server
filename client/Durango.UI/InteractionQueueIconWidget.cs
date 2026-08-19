using Durango.Logic.Item;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class InteractionQueueIconWidget : MonoBehaviour
{
	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UISprite _border;

	[SerializeField]
	private UISprite _shadow;

	public int Id { get; private set; }

	public int Index { get; set; }

	public int PrevIndex { get; set; }

	public void Reset()
	{
		Id = -1;
		Index = -1;
		PrevIndex = -1;
		_iconTexture.color = PresetColor.UIWhite;
		_border.color = PresetColor.UIBlack;
		_shadow.color = PresetColor.UIBlack;
		_shadow.alpha = 0.6f;
	}

	public void Set(int id, int index, ItemIcon icon)
	{
		Id = id;
		Index = index;
		_iconTexture.SetIcon(icon);
	}
}
