using UnityEngine;

public class AnimationSpriteSheet : MonoBehaviour
{
	[SerializeField]
	private int _uvX = 4;

	[SerializeField]
	private int _uvY = 2;

	[SerializeField]
	private float _fps = 24f;

	private Vector2 _size;

	private Vector2 _offset;

	private float _index;

	private Material _material;

	private void Start()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		_size = new Vector2(1f / (float)_uvX, 1f / (float)_uvY);
		_material = ((Component)this).GetComponent<Renderer>().material;
	}

	private void Update()
	{
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		_index = (_index + Time.deltaTime * _fps) % (float)(_uvX * _uvY);
		float num = _index % (float)_uvX;
		float num2 = _index / (float)_uvX;
		_offset.x = num * _size.x;
		_offset.y = 1f - _size.y - num2 * _size.y;
		if ((Object)(object)_material != (Object)null)
		{
			_material.SetTextureOffset("_MainTex", _offset);
			_material.SetTextureScale("_MainTex", _size);
		}
	}
}
