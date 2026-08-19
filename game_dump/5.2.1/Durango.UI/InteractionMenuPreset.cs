using UnityEngine;

namespace Durango.UI;

internal class InteractionMenuPreset : MonoBehaviour
{
	[SerializeField]
	private Vector3[] _positionPresets;

	[SerializeField]
	private float[] _rotationPresets;

	[SerializeField]
	private UIBasicSprite.Flip[] _spriteFlipPresets;

	[SerializeField]
	private UISprite[] _flipSprites;

	[ExposedInEditor(null)]
	public void SetPreset(int index)
	{
		SetPosition(index);
		SetRotation(index);
		SetSpriteFlip(index);
	}

	private void SetPosition(int index)
	{
		if (_positionPresets != null && index < _positionPresets.Length)
		{
			base.gameObject.transform.localPosition = _positionPresets[index];
		}
	}

	private void SetRotation(int index)
	{
		if (_rotationPresets != null && index < _rotationPresets.Length)
		{
			base.gameObject.transform.localEulerAngles = new Vector3(0f, 0f, _rotationPresets[index]);
		}
	}

	private void SetSpriteFlip(int index)
	{
		if (_spriteFlipPresets != null && index < _spriteFlipPresets.Length && _flipSprites != null)
		{
			UISprite[] flipSprites = _flipSprites;
			for (int i = 0; i < flipSprites.Length; i++)
			{
				flipSprites[i].flip = _spriteFlipPresets[index];
			}
		}
	}
}
