using System.Collections;
using UnityEngine;

public class NpcAIK : StateBasedAI<NpcAIK.State>
{
	public enum State
	{
		Invalid = -1,
		Normal,
		Chase,
		Run,
		Count
	}

	[SerializeField]
	private string _standMotion = "F_Barehand_Stand";

	[SerializeField]
	private string _moveMotion = "F_Barehand_Run";

	[SerializeField]
	private float _engageDistance = 450f;

	[SerializeField]
	private float _moveSpeed = 500f;

	[SerializeField]
	private float _appearDiatanceFromPlayer = 1000f;

	private Vector3 _initialPos;

	private GameObject _victim;

	private AnimalBehavior _targetAnimal;

	protected override State InvalidState => State.Invalid;

	protected override int StateEnumCount => 3;

	private AnimalBehavior TargetAnimal
	{
		get
		{
			if ((Object)null == (Object)(object)_targetAnimal)
			{
				_targetAnimal = ((Component)this).GetComponent<AnimalBehavior>();
			}
			return _targetAnimal;
		}
	}

	protected override void DefineStates()
	{
		AddState(State.Normal, new StateElem
		{
			Entered = NormalEntered,
			Doing = NormalDoing,
			Exited = NormalExited
		});
		AddState(State.Chase, new StateElem
		{
			Entered = ChaseEntered,
			Doing = ChaseDoing,
			Exited = ChaseExited
		});
		AddState(State.Run, new StateElem
		{
			Entered = RunEntered,
			Doing = RunDoing,
			Exited = RunExited
		});
	}

	protected override IEnumerator OnStart()
	{
		TargetAnimal.EntityId = 666uL;
		base.CurState = State.Normal;
		BoneLookAtTarget lookAt = ((Component)this).GetComponent<BoneLookAtTarget>();
		lookAt.AutoChangeTarget = false;
		while (!TerrainA6.IsPlayerInitialized)
		{
			yield return null;
		}
		Vector3 worldCenter = TerrainA6.WorldPositionToClientPosition(new Vector3(512f, 512f));
		NpcAIK npcAIK = this;
		Vector3 currentPosition = PlayerBehavior.LocalPlayer.CurrentPosition;
		Vector3 val = worldCenter - PlayerBehavior.LocalPlayer.CurrentPosition;
		npcAIK._initialPos = currentPosition + ((Vector3)(ref val)).normalized * _appearDiatanceFromPlayer;
		_initialPos.y = 0f;
		TargetAnimal.CurrentPosition = _initialPos;
	}

	protected override IEnumerator OnBeforeDoingState()
	{
		BoneLookAtTarget lookAt = ((Component)this).GetComponent<BoneLookAtTarget>();
		_victim = ((Component)PlayerBehavior.LocalPlayer).gameObject;
		if ((Object)(object)_victim == (Object)null)
		{
			yield return (object)new WaitForSeconds(1f);
		}
		else
		{
			lookAt.SetLookTarget(((Component)PlayerBehavior.LocalPlayer).gameObject, bFindHead: true);
		}
	}

	protected override IEnumerator OnAfterDoingState()
	{
		yield break;
	}

	protected override bool IsAIEnded()
	{
		return false;
	}

	protected override bool IsTerminalState(State state)
	{
		return false;
	}

	private void NormalEntered()
	{
		TargetAnimal.CrossFade(_standMotion, 0.1f);
	}

	private void NormalExited()
	{
	}

	private IEnumerator NormalDoing()
	{
		if ((Object)null != (Object)(object)_victim)
		{
			Vector3 val = _victim.transform.position - ((Component)this).transform.position;
			float distance = ((Vector3)(ref val)).magnitude;
			if (distance > _engageDistance)
			{
				base.CurState = State.Chase;
			}
		}
		yield return (object)new WaitForSeconds(0.3f);
	}

	private void ChaseEntered()
	{
	}

	private void ChaseExited()
	{
	}

	private IEnumerator ChaseDoing()
	{
		TargetAnimal.CrossFade(_moveMotion, 0.1f);
		float prevTime = Time.time;
		while (true)
		{
			if ((Object)null == (Object)(object)_victim || base.IsInterrupted)
			{
				yield break;
			}
			float dt = Time.time - prevTime;
			prevTime = Time.time;
			Vector3 disp = KMathUtil.Make2D(_victim.transform.position - ((Component)this).transform.position);
			float distance = ((Vector3)(ref disp)).magnitude;
			if (distance <= _engageDistance)
			{
				break;
			}
			float destYaw = KMathUtil.CalcYawWithTarget(_victim.transform.position, ((Component)this).transform.position);
			TargetAnimal.TurnToYaw(destYaw, bSnap: false);
			Vector3 velocity = ((Vector3)(ref disp)).normalized * _moveSpeed;
			AnimalBehavior targetAnimal = TargetAnimal;
			targetAnimal.CurrentPosition += velocity * dt;
			yield return null;
		}
		base.CurState = State.Normal;
	}

	private void RunEntered()
	{
	}

	private void RunExited()
	{
	}

	private IEnumerator RunDoing()
	{
		TargetAnimal.CrossFade(_moveMotion, 0.1f);
		float prevTime = Time.time;
		while (true)
		{
			if ((Object)null == (Object)(object)_victim || base.IsInterrupted)
			{
				yield break;
			}
			float dt = Time.time - prevTime;
			prevTime = Time.time;
			Vector3 disp = KMathUtil.Make2D(_initialPos - ((Component)this).transform.position);
			float distance = ((Vector3)(ref disp)).magnitude;
			if (distance <= _engageDistance)
			{
				break;
			}
			float destYaw = KMathUtil.CalcYawWithTarget(_initialPos, ((Component)this).transform.position);
			TargetAnimal.TurnToYaw(destYaw, bSnap: false);
			Vector3 velocity = ((Vector3)(ref disp)).normalized * _moveSpeed;
			AnimalBehavior targetAnimal = TargetAnimal;
			targetAnimal.CurrentPosition += velocity * dt;
			yield return null;
		}
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	public void EventRun()
	{
		base.CurState = State.Run;
	}
}
