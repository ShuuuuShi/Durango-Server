namespace Shared.Battle;

public enum DamageResult
{
	Invalid = -1,
	Hit,
	Guarded,
	Dodged,
	Missed,
	Evaded,
	Countered,
	AutoGuarded,
	AutoDodged,
	AutoCountered
}
