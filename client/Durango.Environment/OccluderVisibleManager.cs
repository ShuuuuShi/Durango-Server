using System.Collections.Generic;
using Durango.Render.Camera;
using Durango.Render.Sprite;
using Durango.Utils;
using UnityEngine;

namespace Durango.Environment;

public class OccluderVisibleManager : Singleton<OccluderVisibleManager>
{
	private readonly List<Durango.Render.Sprite.Sprite> _becomeVisibles = new List<Durango.Render.Sprite.Sprite>();

	private int _becomeVisibleCount;

	private readonly List<Durango.Render.Sprite.Sprite> _becomeInvisibles = new List<Durango.Render.Sprite.Sprite>();

	private int _becomeInvisibleCount;

	private readonly List<Vector3> _rayCastPositions = new List<Vector3>();

	private int _rayCastPositionCount;

	public bool IsOccluded { get; private set; }

	private void LateUpdate()
	{
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
		IsOccluded = false;
		Ray ray = MainCamera.WorldToScreenRay(worldPos);
		float magnitude = (ray.origin - worldPos).magnitude;
		int count;
		RaycastHit[] array = Collisions.RayCast(ray, magnitude, LayerHelper.PropMask, out count);
		for (int i = 0; i < count; i++)
		{
			RaycastHit raycastHit = array[i];
			GameObject gameObject = raycastHit.transform.gameObject;
			NaturalSpriteObject component = gameObject.GetComponent<NaturalSpriteObject>();
			if (component != null)
			{
				if (component.Sprite != null)
				{
					Durango.Render.Sprite.Sprite sprite = component.Sprite;
					if (sprite != null && sprite.SpriteObjectType == SpriteObjectType.Rock)
					{
						AddToInvisibles(sprite);
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
			Durango.Render.Sprite.Sprite sprite = _becomeInvisibles[i];
			if (sprite == null)
			{
				continue;
			}
			bool flag = false;
			for (int j = 0; j < _becomeVisibleCount; j++)
			{
				if (_becomeVisibles[j] == sprite)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				if (_becomeVisibleCount < _becomeVisibles.Count)
				{
					_becomeVisibles[_becomeVisibleCount++] = sprite;
					continue;
				}
				_becomeVisibles.Add(sprite);
				_becomeVisibleCount++;
			}
		}
		_becomeInvisibleCount = 0;
	}

	private void AddToInvisibles(Durango.Render.Sprite.Sprite sprite)
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
		float deltaTime = Time.deltaTime;
		int num = 0;
		for (int i = 0; i < _becomeVisibleCount; i++)
		{
			Durango.Render.Sprite.Sprite sprite = _becomeVisibles[i];
			if (sprite != null)
			{
				Color color = sprite.GetColor();
				color.a += 2f * deltaTime;
				color.a = Mathf.Min(color.a, 1f);
				sprite.SetColor(color);
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
			Durango.Render.Sprite.Sprite sprite2 = _becomeInvisibles[j];
			if (sprite2 != null)
			{
				Color color2 = sprite2.GetColor();
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
				sprite2.SetColor(color2);
			}
		}
	}
}
