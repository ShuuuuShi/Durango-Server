using UnityEngine;

public class AreaEffectIndicator : MonoBehaviour
{
	[SerializeField]
	private UISprite _effectSprite;

	[SerializeField]
	private TweenerPlayer _tweener;

	private float _radius;

	private float _validRadius;

	public bool FixedScale { get; private set; }

	public MapIndicator Indicator { get; private set; }

	private void Start()
	{
		EventDelegate.Set(_tweener.OnAllTweenerFinished, PlayAnimation);
	}

	private void PlayAnimation()
	{
		_tweener.Play();
	}

	public void Show()
	{
		((Component)this).gameObject.SetActive(true);
		AnimationWidget component = ((Component)this).GetComponent<AnimationWidget>();
		component.Alpha = 1f;
	}

	public void Set(MapIndicator ind, float radius, float validRadius, bool fixedScale)
	{
		Indicator = ind;
		_radius = radius;
		_validRadius = validRadius;
		FixedScale = fixedScale;
		int num = (int)(_radius * 2f);
		UIWidget component = ((Component)this).GetComponent<UIWidget>();
		component.width = num;
		component.height = num;
		UIUtility.ResizeToSquare(_effectSprite, num);
	}

	public void SetColor(Color color)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		_effectSprite.color = color;
	}

	public bool Check(Vector2 center)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Indicator != (Object)null && Indicator.IsValid())
		{
			if (_validRadius > 0f)
			{
				Vector2 val = center - Indicator.GetTile();
				float sqrMagnitude = ((Vector2)(ref val)).sqrMagnitude;
				if (sqrMagnitude > _validRadius * _validRadius)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public void Hide()
	{
		AnimationWidget component = ((Component)this).GetComponent<AnimationWidget>();
		component.Alpha = 0f;
	}
}
