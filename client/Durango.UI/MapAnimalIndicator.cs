using UnityEngine;

namespace Durango.UI;

public class MapAnimalIndicator : MapIndicator
{
	[SerializeField]
	private UISprite _sprite;

	private AnimalBehavior _animal;

	private bool _isWarpGuard;

	public override void OnRefresh(Refresh type)
	{
		if (type == Refresh.LevelChanged)
		{
			UpdateSprite();
		}
	}

	public void SetAnimal(AnimalBehavior animal)
	{
		if ((bool)_animal)
		{
			_animal.Died -= Animal_Died;
		}
		_animal = animal;
		_animal.Died += Animal_Died;
		_isWarpGuard = _animal.Role == "warp_guard";
		base.CheckReveal = !_isWarpGuard;
		SetTarget(animal.gameObject);
		UpdateSprite();
	}

	private void Animal_Died(CharacterBehavior animal, bool fromInit)
	{
		UpdateSprite();
	}

	private void UpdateSprite()
	{
		if (_animal == null)
		{
			return;
		}
		Color32 color;
		if (_animal.IsAlive)
		{
			if (_isWarpGuard)
			{
				color = new Color32(171, 125, byte.MaxValue, byte.MaxValue);
			}
			else
			{
				int delta = _animal.Level - GameSystem<StatisticsSystem>.Instance().Level;
				color = GetLevelDeltaColor(delta);
			}
		}
		else
		{
			color = new Color32(32, 32, 32, 225);
		}
		_sprite.color = color;
	}

	private static Color GetLevelDeltaColor(int delta)
	{
		if (delta < -5)
		{
			return new Color32(150, 150, 150, byte.MaxValue);
		}
		if (delta < -3)
		{
			return new Color32(111, byte.MaxValue, 91, byte.MaxValue);
		}
		if (delta < 2)
		{
			return new Color32(byte.MaxValue, 216, 91, byte.MaxValue);
		}
		if (delta < 4)
		{
			return new Color32(byte.MaxValue, 128, 0, byte.MaxValue);
		}
		if (delta < 9)
		{
			return new Color32(byte.MaxValue, 47, 0, byte.MaxValue);
		}
		return new Color32(208, 0, 0, byte.MaxValue);
	}
}
