using System;
using System.Collections.Generic;
using StatusEffectData;

public interface IStatusEffectSystem
{
	IList<StatusEffect> StatusEffects { get; }

	event Action StatusEffectsUpdated;
}
