using System;
using System.Collections;
using System.Linq;
using Durango.Terrain;
using Durango.Utils;
using Messages;
using UnityEngine;

public class GrazingPetAI : StateBasedAI<GrazingPetAI.State>
{
	public enum State
	{
		Invalid = -1,
		Idle,
		Roming,
		Count
	}

	public AnimalBehavior TargetAnimal { get; private set; }

	protected override State InvalidState => State.Invalid;

	protected override int StateEnumCount => 2;

	public Pet Pet { get; set; }

	private float PlaybackRate => (!(Pet.Stat.PlaybackRate > 0f)) ? 1f : Pet.Stat.PlaybackRate;

	protected override void OnAwake()
	{
		TargetAnimal = GetComponent<AnimalBehavior>();
		TargetAnimal.SetActivateRootMotion(active: false);
	}

	protected override IEnumerator OnStart()
	{
		base.CurState = State.Idle;
		return base.OnStart();
	}

	private void Update()
	{
		Vector3 currentPosition = TargetAnimal.CurrentPosition;
		currentPosition.y = TargetAnimal.ProcessWaterDepth(currentPosition);
		TargetAnimal.CurrentPosition = currentPosition;
	}

	protected override void DefineStates()
	{
		AddState(State.Idle, new StateElem
		{
			Doing = OnIdle
		});
		AddState(State.Roming, new StateElem
		{
			Doing = OnRoming
		});
	}

	private IEnumerator OnIdle()
	{
		while (true)
		{
			float r = UnityEngine.Random.value;
			if (r > 0.5f)
			{
				break;
			}
			float duration = 1f;
			if (r < 0.2f)
			{
				if (TargetAnimal.AnimalFrameworkResource.GetAnimationElements("idle") is AnimationElem animationElem)
				{
					TargetAnimal.Play(animationElem.motion, loop: true, 0f, PlaybackRate);
					duration = animationElem.Clip.length / PlaybackRate;
				}
			}
			else if (TargetAnimal.AnimalFrameworkResource.GetAnimationElements("stand") is AnimationElem animationElem2)
			{
				TargetAnimal.Play(animationElem2.motion, loop: true, 0f, PlaybackRate);
				float num = animationElem2.Clip.length / PlaybackRate;
				duration = UnityEngine.Random.Range(num, num * 3f);
			}
			yield return new WaitForSeconds(duration);
		}
		base.CurState = State.Roming;
	}

	private IEnumerator OnRoming()
	{
		while (true)
		{
			float r = UnityEngine.Random.value;
			if (r > 0.7f)
			{
				break;
			}
			MoveSet moveSet = ((TargetAnimal.AnimalFrameworkResource.GetAnimationElements("move_motion_sets") is AnimationElemMoveSet elemMoveSet) ? elemMoveSet.elems.FirstOrDefault() : null);
			if (moveSet != null)
			{
				MoveMotionInfo i = moveSet.GetMoveMotion(0f);
				TargetAnimal.SetRotateSpeed(i.rot_speed);
				TargetAnimal.Play(i.motion, loop: true, 0f, PlaybackRate);
				float moveSpeed = i.base_move_speed;
				float yaw = UnityEngine.Random.value * 360f;
				float timer = UnityEngine.Random.Range(2f, 7f);
				TargetAnimal.TurnToYaw(yaw, bSnap: false);
				while (timer > 0f)
				{
					float curYaw = TargetAnimal.CurrentYaw;
					Vector3 dir = new Vector3(Mathf.Sin(curYaw * ((float)Math.PI / 180f)), 0f, Mathf.Cos(curYaw * ((float)Math.PI / 180f)));
					Vector3 delta = dir * moveSpeed * Time.deltaTime;
					Vector3 prevPos = TargetAnimal.CurrentPosition;
					Vector3 nextPos = ProcessCollisionWithSliding(TargetAnimal.CurrentPosition, delta);
					Vector2 tilePos = Util.ClientPositionToTilePosition(nextPos);
					float depth = Singleton<TerrainBase>.Instance().GetTileDepth(tilePos);
					if (TerrainWater.IsTooDeepToSwim(depth, 0f))
					{
						base.CurState = State.Idle;
						yield break;
					}
					if ((nextPos - prevPos).magnitude / Time.deltaTime / moveSpeed < 0.7f)
					{
						base.CurState = State.Idle;
						yield break;
					}
					TargetAnimal.CurrentPosition = nextPos;
					timer -= Time.deltaTime;
					yield return null;
				}
			}
			yield return null;
		}
		base.CurState = State.Idle;
	}

	private Vector3 ProcessCollisionWithSliding(Vector3 beginPos, Vector3 delta)
	{
		if (delta == Vector3.zero)
		{
			return beginPos;
		}
		CollisionParam param = Collisions.CreateCollisionParam(beginPos, delta);
		delta = Collisions.ProcessSimpleSliding(param);
		return beginPos + delta;
	}

	protected override bool IsAIEnded()
	{
		return false;
	}

	protected override bool IsTerminalState(State state)
	{
		return false;
	}
}
