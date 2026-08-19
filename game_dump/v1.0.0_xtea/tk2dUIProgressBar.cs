using System;
using UnityEngine;

[AddComponentMenu("2D Toolkit/UI/tk2dUIProgressBar")]
public class tk2dUIProgressBar : MonoBehaviour
{
	public Transform scalableBar;

	public tk2dClippedSprite clippedSpriteBar;

	public tk2dSlicedSprite slicedSpriteBar;

	private bool initializedSlicedSpriteDimensions;

	private Vector2 emptySlicedSpriteDimensions = Vector2.zero;

	private Vector2 fullSlicedSpriteDimensions = Vector2.zero;

	private Vector2 currentDimensions = Vector2.zero;

	[SerializeField]
	private float percent;

	private bool isProgressComplete;

	public GameObject sendMessageTarget;

	public string SendMessageOnProgressCompleteMethodName = string.Empty;

	public float Value
	{
		get
		{
			return percent;
		}
		set
		{
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
			percent = Mathf.Clamp(value, 0f, 1f);
			if (!Application.isPlaying)
			{
				return;
			}
			if ((Object)(object)clippedSpriteBar != (Object)null)
			{
				clippedSpriteBar.clipTopRight = new Vector2(Value, 1f);
			}
			else if ((Object)(object)scalableBar != (Object)null)
			{
				scalableBar.localScale = new Vector3(Value, scalableBar.localScale.y, scalableBar.localScale.z);
			}
			else if ((Object)(object)slicedSpriteBar != (Object)null)
			{
				InitializeSlicedSpriteDimensions();
				float num = Mathf.Lerp(emptySlicedSpriteDimensions.x, fullSlicedSpriteDimensions.x, Value);
				((Vector2)(ref currentDimensions)).Set(num, fullSlicedSpriteDimensions.y);
				slicedSpriteBar.dimensions = currentDimensions;
			}
			if (!isProgressComplete && Value == 1f)
			{
				isProgressComplete = true;
				if (this.OnProgressComplete != null)
				{
					this.OnProgressComplete();
				}
				if ((Object)(object)sendMessageTarget != (Object)null && SendMessageOnProgressCompleteMethodName.Length > 0)
				{
					sendMessageTarget.SendMessage(SendMessageOnProgressCompleteMethodName, (object)this, (SendMessageOptions)0);
				}
			}
			else if (isProgressComplete && Value < 1f)
			{
				isProgressComplete = false;
			}
		}
	}

	public event Action OnProgressComplete;

	private void Start()
	{
		InitializeSlicedSpriteDimensions();
		Value = percent;
	}

	private void InitializeSlicedSpriteDimensions()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (!initializedSlicedSpriteDimensions)
		{
			if ((Object)(object)slicedSpriteBar != (Object)null)
			{
				tk2dSpriteDefinition currentSprite = slicedSpriteBar.CurrentSprite;
				Vector3 val = currentSprite.boundsData[1];
				fullSlicedSpriteDimensions = slicedSpriteBar.dimensions;
				((Vector2)(ref emptySlicedSpriteDimensions)).Set((slicedSpriteBar.borderLeft + slicedSpriteBar.borderRight) * val.x / currentSprite.texelSize.x, fullSlicedSpriteDimensions.y);
			}
			initializedSlicedSpriteDimensions = true;
		}
	}
}
