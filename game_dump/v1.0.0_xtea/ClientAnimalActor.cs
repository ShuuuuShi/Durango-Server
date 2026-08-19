using System;
using System.Collections.Generic;
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

	public bool HasBasePosition { get; private set; }

	public Vector3 BasePosition { get; private set; }

	public float BaseYaw { get; private set; }

	private void Awake()
	{
		_animalBehavior = ((Component)this).GetComponent<AnimalBehavior>();
		_animalBehavior.SetServerSideRootMotionEnable(serverSideRootMotionEnabled: false);
	}

	private void Update()
	{
		bool flag = !HasMovingPath();
		if (flag & ((_nextActionTime >= 0f && Time.time >= _nextActionTime) || !_animalBehavior.IsAnimPlaying))
		{
			PlayRandomAction();
		}
	}

	private void InitializeBasePosition()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (!HasBasePosition)
		{
			BasePosition = _animalBehavior.CurrentPosition;
			BaseYaw = _animalBehavior.CurrentYaw;
			HasBasePosition = true;
		}
	}

	private void PlayRandomAction()
	{
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
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
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
				Vector3 begin = TerrainA6.ClientPositionToWorldPosition(clientPosition);
				Vector3 end = TerrainA6.ClientPositionToWorldPosition(moveTarget[i]);
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
		_animalBehavior.Suicide();
	}

	private void PlayWander()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		float num = ChooseWanderRadian();
		Vector3 clientPosition = BasePosition + new Vector3(Mathf.Cos(num) * _wanderRadius, 0f, Mathf.Sin(num) * _wanderRadius);
		Vector3 end = TerrainA6.ClientPositionToWorldPosition(clientPosition);
		Vector3 begin = TerrainA6.ClientPositionToWorldPosition(_animalBehavior.CurrentPosition);
		Move msg = GenerateMove(begin, end);
		_animalBehavior.HandleMoveMsg(msg);
		double time = msg.Movements[0].Path[0].Time;
		double time2 = msg.Movements[0].Path[msg.Movements[0].Path.Length - 1].Time;
		_nextActionTime = (float)((double)Time.time + time2 - time + 0.10000000149011612);
	}

	private float ChooseWanderRadian()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (HasBasePosition)
		{
			Vector3 val = _animalBehavior.CurrentPosition - BasePosition;
			val.y = 0f;
			((Vector3)(ref val)).Normalize();
			float num = Mathf.Atan2(val.x, val.z);
			num += (float)Math.PI / 2f;
			return num + (float)Math.PI * Random.value;
		}
		InitializeBasePosition();
		return (float)Math.PI * 2f * Random.value;
	}

	private Move GenerateMove(Vector3 begin, Vector3 end)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		double bufferedServerTime = Connections.Frontend.GetBufferedServerTime();
		Move result = default(Move);
		result.Movements = new Movement[1];
		ref Movement reference = ref result.Movements[0];
		reference = GenerateMovement(begin, end, _animalBehavior.CurrentYaw, bufferedServerTime);
		return result;
	}

	private Movement GenerateMovement(Vector3 begin, Vector3 end, float beginYaw, double beginTime)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
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
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
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
			float magnitude = ((Vector3)(ref dir)).magnitude;
			((Vector3)(ref dir)).Normalize();
			float num = KMathUtil.CalcYaw(dir);
			float num2 = KMathUtil.DistanceAngDeg(beginYaw, num);
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
			Vector3 val = KMathUtil.CalcDirectionFromYaw(beginYaw);
			beginPos += val * _movingSpeed * 0.5f * num4;
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
