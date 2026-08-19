using UnityEngine;

[AddComponentMenu("NGUI/UI/Image Button")]
public class UIImageButton : MonoBehaviour
{
	public UISprite target;

	public string normalSprite;

	public string hoverSprite;

	public string pressedSprite;

	public string disabledSprite;

	public bool pixelSnap = true;

	public bool isEnabled
	{
		get
		{
			Collider component = ((Component)this).gameObject.GetComponent<Collider>();
			return Object.op_Implicit((Object)(object)component) && component.enabled;
		}
		set
		{
			Collider component = ((Component)this).gameObject.GetComponent<Collider>();
			if (Object.op_Implicit((Object)(object)component) && component.enabled != value)
			{
				component.enabled = value;
				UpdateImage();
			}
		}
	}

	private void OnEnable()
	{
		if ((Object)(object)target == (Object)null)
		{
			target = ((Component)this).GetComponentInChildren<UISprite>();
		}
		UpdateImage();
	}

	private void OnValidate()
	{
		if ((Object)(object)target != (Object)null)
		{
			if (string.IsNullOrEmpty(normalSprite))
			{
				normalSprite = target.spriteName;
			}
			if (string.IsNullOrEmpty(hoverSprite))
			{
				hoverSprite = target.spriteName;
			}
			if (string.IsNullOrEmpty(pressedSprite))
			{
				pressedSprite = target.spriteName;
			}
			if (string.IsNullOrEmpty(disabledSprite))
			{
				disabledSprite = target.spriteName;
			}
		}
	}

	private void UpdateImage()
	{
		if ((Object)(object)target != (Object)null)
		{
			if (isEnabled)
			{
				SetSprite((!UICamera.IsHighlighted(((Component)this).gameObject)) ? normalSprite : hoverSprite);
			}
			else
			{
				SetSprite(disabledSprite);
			}
		}
	}

	private void OnHover(bool isOver)
	{
		if (isEnabled && (Object)(object)target != (Object)null)
		{
			SetSprite((!isOver) ? normalSprite : hoverSprite);
		}
	}

	private void OnPress(bool pressed)
	{
		if (pressed)
		{
			SetSprite(pressedSprite);
		}
		else
		{
			UpdateImage();
		}
	}

	private void SetSprite(string sprite)
	{
		if (!((Object)(object)target.atlas == (Object)null) && target.atlas.GetSprite(sprite) != null)
		{
			target.spriteName = sprite;
			if (pixelSnap)
			{
				target.MakePixelPerfect();
			}
		}
	}
}
