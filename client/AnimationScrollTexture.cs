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
		_offset = new Vector2(0f, 0f);
		_material = GetComponent<Renderer>().material;
	}

	private void Update()
	{
		_offset.x = (_offset.x - Time.deltaTime * _speedX) % 1f;
		_offset.y = (_offset.y - Time.deltaTime * _speedY) % 1f;
		if (_material != null)
		{
			_material.mainTextureOffset = _offset;
		}
	}
}
