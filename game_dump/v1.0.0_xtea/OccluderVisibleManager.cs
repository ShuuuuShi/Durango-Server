using System.Collections.Generic;
using UnityEngine;

public class OccluderVisibleManager : KSingleton<OccluderVisibleManager>
{
	private readonly List<KSprite> _becomeVisibles = new List<KSprite>();

	private int _becomeVisibleCount;

	private readonly List<KSprite> _becomeInvisibles = new List<KSprite>();

	private int _becomeInvisibleCount;

	private readonly List<Vector3> _rayCastPositions = new List<Vector3>();

	private int _rayCastPositionCount;

	public bool IsOccluded { get; private set; }

	private void LateUpdate()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _rayCastPositionCount; i++)
		{
			Vector3 worldPos = _rayCastPositions[i];
			CheckOccluded(worldPos);
		}
		_rayCastPositionCount = 0;
		ProcessTransparency();
		MoveToVisibles();
	}

	private void CheckOccluded(Vector3 worldPos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		IsOccluded = false;
		Ray ray = MainCamera.WorldToScreenRay(worldPos);
		Vector3 val = ((Ray)(ref ray)).origin - worldPos;
		float magnitude = ((Vector3)(ref val)).magnitude;
		int count;
		RaycastHit[] array = KCollisionUtility.RayCast(ray, magnitude, LayerMask.op_Implicit(LayerHelper.PropMask), out count);
		for (int i = 0; i < count; i++)
		{
			RaycastHit val2 = array[i];
			GameObject gameObject = ((Component)((RaycastHit)(ref val2)).transform).gameObject;
			NaturalObject component = gameObject.GetComponent<NaturalObject>();
			if ((Object)(object)component != (Object)null)
			{
				if (component.KSprite != null)
				{
					KSprite kSprite = component.KSprite;
					if (kSprite != null && kSprite.SpriteObjectType != SpriteObjectType.Pebble)
					{
						AddToInvisibles(kSprite);
					}
				}
			}
			else if (gameObject.CompareTag("Blockable"))
			{
				IsOccluded = true;
			}
		}
	}

	public void PushRayCastPosition(Vector3 pos)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (_rayCastPositionCount < _rayCastPositions.Count)
		{
			_rayCastPositions[_rayCastPositionCount++] = pos;
			return;
		}
		_rayCastPositions.Add(pos);
		_rayCastPositionCount++;
	}

	private void MoveToVisibles()
	{
		for (int i = 0; i < _becomeInvisibleCount; i++)
		{
			KSprite kSprite = _becomeInvisibles[i];
			if (kSprite == null)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < _becomeVisibleCount; j++)
			{
				if (_becomeVisibles[j] == kSprite)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (_becomeVisibleCount < _becomeVisibles.Count)
				{
					_becomeVisibles[_becomeVisibleCount++] = kSprite;
					continue;
				}
				_becomeVisibles.Add(kSprite);
				_becomeVisibleCount++;
			}
		}
		_becomeInvisibleCount = 0;
	}

	private void AddToInvisibles(KSprite sprite)
	{
		if (_becomeInvisibleCount < _becomeInvisibles.Count)
		{
			_becomeInvisibles[_becomeInvisibleCount++] = sprite;
		}
		else
		{
			_becomeInvisibles.Add(sprite);
			_becomeInvisibleCount++;
		}
		for (int i = 0; i < _becomeVisibleCount; i++)
		{
			if (_becomeVisibles[i] == sprite)
			{
				_becomeVisibles[i] = null;
				break;
			}
		}
	}

	private void ProcessTransparency()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		float deltaTime = Time.deltaTime;
		int num = 0;
		for (int i = 0; i < _becomeVisibleCount; i++)
		{
			KSprite kSprite = _becomeVisibles[i];
			if (kSprite != null)
			{
				Color color = kSprite.GetColor();
				color.a += 2f * deltaTime;
				color.a = Mathf.Min(color.a, 1f);
				kSprite.SetColor(color);
				if (color.a >= 1f)
				{
					_becomeVisibles[i] = null;
				}
				else
				{
					num++;
				}
			}
		}
		if (num == 0)
		{
			_becomeVisibleCount = 0;
		}
		float num2 = Mathf.Max(0.3f, 0.55f - (float)_becomeInvisibleCount * 0.1f * 0.5f);
		for (int j = 0; j < _becomeInvisibleCount; j++)
		{
			KSprite kSprite2 = _becomeInvisibles[j];
			if (kSprite2 != null)
			{
				Color color2 = kSprite2.GetColor();
				if (color2.a > num2)
				{
					color2.a -= 2f * deltaTime;
					color2.a = Mathf.Max(color2.a, num2);
				}
				else
				{
					color2.a += 2f * deltaTime;
					color2.a = Mathf.Min(color2.a, num2);
				}
				kSprite2.SetColor(color2);
			}
		}
	}
}
