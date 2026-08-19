using Shared.Battle;
using UnityEngine;

public class CombatDirectionArc : MonoBehaviour
{
	[SerializeField]
	private UITexture _texture;

	[SerializeField]
	private UIWidget _point;

	private UIWidget _uiWidget;

	public DamageDirection Direction { get; set; }

	public bool IsSelected { get; set; }

	public Vector3 GetNguiPosition()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return MainCamera.WorldToNGUIPos(((Component)_point).gameObject.transform.position);
	}

	public void SetSprite(Texture texture, Color color)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_texture.mainTexture = texture;
		_texture.color = color;
	}

	public void SetRadius(int length)
	{
		if ((Object)(object)_uiWidget == (Object)null)
		{
			_uiWidget = ((Component)this).GetComponent<UIWidget>();
		}
		_uiWidget.width = length;
		_uiWidget.height = length;
		NGUITools.UpdateWidgetCollider(((Component)this).gameObject);
		_texture.UpdateAnchors();
		_point.UpdateAnchors();
	}
}
