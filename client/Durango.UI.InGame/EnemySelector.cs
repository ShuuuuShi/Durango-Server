using System.Linq;
using Durango.Utils;
using Messages;
using UnityEngine;

namespace Durango.UI.InGame;

public class EnemySelector : Singleton<EnemySelector>
{
	public struct Target
	{
		public Vector3? Position;

		public Transform Transform;

		public static implicit operator Target(Transform t)
		{
			Target result = default(Target);
			result.Transform = t;
			return result;
		}

		public static implicit operator Target(Vector3 p)
		{
			Target result = default(Target);
			result.Position = p;
			return result;
		}

		public bool IsValid()
		{
			Vector3? position = Position;
			return position.HasValue || Transform != null;
		}

		public Vector3 GetPosition()
		{
			if (Position.HasValue)
			{
				return Position.Value;
			}
			return Transform.position;
		}
	}

	[SerializeField]
	private EnemySelectorArrow _arrowBase;

	private ListObjectPool<EnemySelectorArrow> _arrows;

	protected override void OnAwake()
	{
		_arrows = new ListObjectPool<EnemySelectorArrow>();
		_arrows.BaseObject = _arrowBase;
		_arrows.UseBase = true;
		_arrows.Clear();
	}

	private void Start()
	{
		GameSystem<CombatSystem>.Instance().TargetChanged += OnTargetChanged;
	}

	private void OnTargetChanged(TargetChanged msg)
	{
		if (msg.EntityId == GameManager.PlayerId)
		{
			return;
		}
		GameObject gameObject = Singleton<ObjectManager>.Instance().FindObject(msg.EntityId);
		if (!(gameObject == null))
		{
			GameObject gameObject2 = Singleton<ObjectManager>.Instance().FindObject(msg.TargetId);
			if (!(gameObject2 == null))
			{
				SetTargetImpl(gameObject.transform, gameObject2.transform);
			}
		}
	}

	private void SetTargetImpl(Target start, Target end)
	{
		ListObjectPool<EnemySelectorArrow> arrows = _arrows;
		EnemySelectorArrow enemySelectorArrow = arrows.FirstOrDefault((EnemySelectorArrow t) => !t.enabled);
		if (enemySelectorArrow == null)
		{
			enemySelectorArrow = arrows.Add();
		}
		enemySelectorArrow.Show(start, end);
	}

	public static void SetTarget(Target start, Target end)
	{
		if (Singleton<EnemySelector>.HasInstance())
		{
			Singleton<EnemySelector>.Instance().SetTargetImpl(start, end);
		}
	}
}
