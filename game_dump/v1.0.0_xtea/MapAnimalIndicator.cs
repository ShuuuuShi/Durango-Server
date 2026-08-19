using UnityEngine;

public class MapAnimalIndicator : MapIndicator
{
	[SerializeField]
	private UISprite _sprite;

	private AnimalBehavior _animal;

	private void OnEnable()
	{
		GameSystem<StatisticsSystem>.Instance().LevelChanged += OnPlayerLevelChanged;
	}

	private void OnDisable()
	{
		GameSystem<StatisticsSystem>.Instance().LevelChanged -= OnPlayerLevelChanged;
	}

	private void OnPlayerLevelChanged(int prev, int lv)
	{
		UpdateSprite();
	}

	public void SetAnimal(AnimalBehavior animal)
	{
		_animal = animal;
		SetTarget(((Component)animal).gameObject);
		UpdateSprite();
	}

	private void UpdateSprite()
	{
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_animal == (Object)null))
		{
			int num = _animal.Level - GameSystem<StatisticsSystem>.Instance().Level;
			Color32 val = default(Color32);
			if (num < -5)
			{
				((Color32)(ref val))._002Ector((byte)113, (byte)113, (byte)113, byte.MaxValue);
			}
			else if (num < -3)
			{
				((Color32)(ref val))._002Ector((byte)111, byte.MaxValue, (byte)91, byte.MaxValue);
			}
			else if (num < 2)
			{
				((Color32)(ref val))._002Ector(byte.MaxValue, (byte)216, (byte)91, byte.MaxValue);
			}
			else if (num < 4)
			{
				((Color32)(ref val))._002Ector(byte.MaxValue, (byte)128, (byte)0, byte.MaxValue);
			}
			else if (num < 9)
			{
				((Color32)(ref val))._002Ector(byte.MaxValue, (byte)47, (byte)0, byte.MaxValue);
			}
			else
			{
				((Color32)(ref val))._002Ector((byte)208, (byte)0, (byte)0, byte.MaxValue);
			}
			_sprite.color = Color32.op_Implicit(val);
		}
	}
}
