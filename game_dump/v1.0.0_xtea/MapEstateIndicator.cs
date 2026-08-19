using UnityEngine;

public class MapEstateIndicator : MapIndicator
{
	[SerializeField]
	private UISprite _tileSprite;

	private int _size;

	public override bool FixedScale => false;

	public void Set(Point2 grid, int size, Color color)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		_size = size;
		SetTarget(grid * _size);
		_tileSprite.color = color;
	}

	public override void OnUpdate()
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		MapContext mapContext = KSingleton<MapContext>.Instance();
		float num = (float)_size * (float)mapContext.MapNGUISize / (float)mapContext.MapSize;
		((Component)_tileSprite).transform.localScale = new Vector3(num / (float)_tileSprite.width, num / (float)_tileSprite.height, 1f);
	}
}
