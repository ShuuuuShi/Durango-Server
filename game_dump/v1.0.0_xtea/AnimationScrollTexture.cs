using UnityEngine;

public class AnimationScrollTexture : MonoBehaviour
{
	[SerializeField]
	private float _speedX;

	[SerializeField]
	private float _speedY;

	private Vector2 _offset;

	private Material _material;

	private void Start()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		_offset = new Vector2(0f, 0f);
		_material = ((Component)this).GetComponent<Renderer>().material;
	}

	private void Update()
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		_offset.x = (_offset.x - Time.deltaTime * _speedX) % 1f;
		_offset.y = (_offset.y - Time.deltaTime * _speedY) % 1f;
		if ((Object)(object)_material != (Object)null)
		{
			_material.mainTextureOffset = _offset;
		}
	}
}
