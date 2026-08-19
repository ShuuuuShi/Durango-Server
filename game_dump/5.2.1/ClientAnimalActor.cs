using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Terrain;
using Durango.Utils;
using Messages;
using UnityEngine;

public class ClientAnimalActor : MonoBehaviour
{
	[Serializable]
	private class MotionCandidate : WeightedCandidate
	{
		public string Name;

		public float Time;
	}

	[SerializeField]
	private List<MotionCandidate> _motionList;

	[SerializeField]
	private float _wanderRadius = 500f;

	[SerializeField]
	private string _movingMotion;

	[SerializeField]
	private float _movingSpeed = 100f;

	[SerializeField]
	private float _rotateSpeed = 100f;

	private AnimalBehavior _animalBehavior;

	private float _nextActionTime;

	private Animation _anim;

	public bool HasBasePosition { get; private set; }

	public Vector3 BasePosition { get; private set; }

	public float BaseYaw { get; private set; }

	private void Awake()
	{
		_animalBehavior = GetComponent<AnimalBehavior>();
		_animalBehavior.SetActivateRootMotion(active: false);
		_anim = GetComponentInChildren<Animation>(includeInactive: true);
	}

	private void Update()
	{
		if (!HasMovingPath() & ((_nextActionTime >= 0f && Time.time >= _nextActionTime) || !_animalBehavior.IsAnimPlaying))
		{
			PlayRandomAction();
		}
	}

	private void InitializeBasePosition()
	{
		if (!HasBasePosition)
		{
			BasePosition = _animalBehavior.CurrentPosition;
			BaseYaw = _animalBehavior.CurrentYaw;
			HasBasePosition = true;
		}
	}

	private void PlayRandomAction()
	{
		if (_motionList == null || _motionList.Count == 0)
		{
			return;
		}
		MotionCandidate motionCandidate = WeightedCandidate.Select(_motionList);
		if (motionCandidate != null)
		{
			if (motionCandidate.Name == _movingMotion && _wanderRadius > 0f)
			{
				PlayWander();
			}
			else
			{
				PlayMotion(motionCandidate);
			}
		}
	}

	public void MoveTo(List<Vector3> moveTarget)
	{
		if (moveTarget.Count >= 1)
		{
			InitializeBasePosition();
			Vector3 clientPosition = _animalBehavior.CurrentPosition;
			double beginTime = Connections.Frontend.GetBufferedServerTime();
			float beginYaw = _animalBehavior.CurrentYaw;
			Move msg = default(Move);
			msg.Movements = new Movement[moveTarget.Count];
			for (int i = 0; i < moveTarget.Count; i++)
			{
				Vector3 begin = Util.ClientPositionToWorldPosition(clientPosition);
				Vector3 end = Util.ClientPositionToWorldPosition(moveTarget[i]);
				Movement movement = GenerateMovement(begin, end, beginYaw, beginTime);
				msg.Movements[i] = movement;
				clientPosition = moveTarget[i];
				beginTime = movement.Path[movement.Path.Length - 1].Time;
				beginYaw = movement.Path[movement.Path.Length - 1].Yaw;
			}
			_animalBehavior.HandleMoveMsg(msg);
			Movement movement2 = msg.Movements[msg.Movements.Length - 1];
			double time = movement2.Path[movement2.Path.Length - 1].Time;
			_nextActionTime = (float)((double)Time.time + time - Connections.Frontend.GetBufferedServerTime() + 0.10000000149011612);
		}
	}

	public bool HasMovingPath()
	{
		return _animalBehavior.HasMovingPath();
	}

	public void Suicide()
	{
		_nextActionTime = -1f;
		_animalBehavior.Disappear();
	}

	private void PlayWander()
	{
		float f = ChooseWanderRadian();
		Vector3 end = Util.ClientPositionToWorldPosition(BasePosition + new Vector3(Mathf.Cos(f) * _wanderRadius, 0f, Mathf.Sin(f) * _wanderRadius));
		Vector3 begin = Util.ClientPositionToWorldPosition(_animalBehavior.CurrentPosition);
		Move msg = GenerateMove(begin, end);
		_animalBehavior.HandleMoveMsg(msg);
		double time = msg.Movements[0].Path[0].Time;
		double time2 = msg.Movements[0].Path[msg.Movements[0].Path.Length - 1].Time;
		_nextActionTime = (float)((double)Time.time + time2 - time + 0.10000000149011612);
	}

	private float ChooseWanderRadian()
	{
		if (HasBasePosition)
		{
			Vector3 vector = _animalBehavior.CurrentPosition - BasePosition;
			vector.y = 0f;
			vector.Normalize();
			return Mathf.Atan2(vector.x, vector.z) + (float)Math.PI / 2f + (float)Math.PI * UnityEngine.Random.value;
		}
		InitializeBasePosition();
		return (float)Math.PI * 2f * UnityEngine.Random.value;
	}

	private Move GenerateMove(Vector3 begin, Vector3 end)
	{
		double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
		Move result = default(Move);
		result.Movements = new Movement[1];
		ref Movement reference = ref result.Movements[0];
		reference = GenerateMovement(begin, end, _animalBehavior.CurrentYaw, bufferedServerTime);
		return result;
	}

	public Movement GenerateMovement(Vector3 begin, Vector3 end, float beginYaw, double beginTime)
	{
		Movement result = default(Movement);
		result.RotSpeed = _rotateSpeed;
		result.MotionName = _movingMotion;
		result.MotionOption = 5;
		result.PlaybackRate = 1f;
		List<Location> list = GeneratePath(begin, end, beginYaw, beginTime);
		result.Path = list.ToArray();
		return result;
	}

	private List<Location> GeneratePath(Vector3 beginPos, Vector3 endPos, float beginYaw, double beginTime)
	{
		List<Location> list = new List<Location>();
		Location item = default(Location);
		item.Time = beginTime;
		item.Position = new WorldPosition(beginPos.x, beginPos.z);
		item.Yaw = beginYaw;
		list.Add(item);
		while (true)
		{
			Vector3 dir = endPos - beginPos;
			dir.y = 0f;
			float magnitude = dir.magnitude;
			dir.Normalize();
			float num = Maths.CalcYaw(dir);
			float num2 = Maths.DistanceAngDeg(beginYaw, num);
			if (num2 < 1f)
			{
				item.Time = beginTime + (double)(magnitude / _movingSpeed);
				item.Position = new WorldPosition(endPos.x, endPos.z);
				item.Yaw = num;
				list.Add(item);
				break;
			}
			if (magnitude <= _movingSpeed || list.Count >= 100)
			{
				break;
			}
			beginTime += 0.5;
			float num3 = _rotateSpeed;
			if (magnitude <= _movingSpeed * 2f)
			{
				num3 += (1f - magnitude / _movingSpeed / 2f) * _rotateSpeed;
			}
			beginYaw = Mathf.MoveTowardsAngle(beginYaw, num, 0.5f * num3);
			float num4 = 0.2f + Math.Max(0f, 1f - num2 / 180f) * 0.8f;
			Vector3 vector = Maths.CalcDirectionFromYaw(beginYaw);
			beginPos += vector * _movingSpeed * 0.5f * num4;
			item.Time = beginTime;
			item.Position = new WorldPosition(beginPos.x, beginPos.z);
			item.Yaw = beginYaw;
			list.Add(item);
		}
		return list;
	}

	private void PlayMotion(MotionCandidate candidate)
	{
		bool flag = candidate.Time > 0f;
		float fadeTime = _animalBehavior.GetFadeTime(candidate.Name);
		float num = _animalBehavior.CrossFade(candidate.Name, fadeTime, flag);
		_nextActionTime = Time.time + ((!flag) ? (num - fadeTime) : candidate.Time);
	}
}
