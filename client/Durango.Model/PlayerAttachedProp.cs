using System.Collections.Generic;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Model;

[RequireComponent(typeof(AnimatingModel))]
public class PlayerAttachedProp : MonoBehaviour
{
	private class AttachedPlayer
	{
		public PlayerBehavior Player;

		private Transform _attachment;

		private Vector3 _currentPosition;

		private Vector3 _restorePosition;

		private Quaternion _restoreRoation;

		public void Set([NotNull] PlayerBehavior player, [NotNull] Transform attach)
		{
			Player = player;
			_attachment = attach;
			_currentPosition = player.CurrentPosition;
			_restorePosition = player.MeshObjectTransform.localPosition;
			_restoreRoation = player.MeshObjectTransform.localRotation;
		}

		public bool Update()
		{
			if (Player == null || _attachment == null)
			{
				return false;
			}
			float sqrMagnitude = (Player.CurrentPosition - _currentPosition).sqrMagnitude;
			if (sqrMagnitude >= 10000f)
			{
				return false;
			}
			Player.MeshObjectTransform.position = _attachment.position;
			Player.MeshObjectTransform.rotation = _attachment.rotation;
			return true;
		}

		public void Restore()
		{
			if (!(Player == null))
			{
				Player.MeshObjectTransform.localPosition = _restorePosition;
				Player.MeshObjectTransform.localRotation = _restoreRoation;
			}
		}
	}

	[SerializeField]
	private string _attachedAnimName;

	[SerializeField]
	private string _detachedAnimName;

	[SerializeField]
	private int _maxPlayerCount;

	[SerializeField]
	private int _minPlayerCountToPlayAnim;

	private readonly List<AttachedPlayer> _players = new List<AttachedPlayer>();

	private void Start()
	{
		Transform[] componentsInChildren = GetComponentsInChildren<Transform>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			if (!transform.name.StartsWith("Attachment_Player"))
			{
				continue;
			}
			Vector3 pos = transform.position;
			PlayerBehavior playerIncludeLocalPlayer = Singleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(delegate(PlayerBehavior target)
			{
				if (!target.AttachedReady)
				{
					return false;
				}
				return !((pos - target.transform.position).sqrMagnitude > 900f);
			});
			if (playerIncludeLocalPlayer != null)
			{
				Attach(playerIncludeLocalPlayer);
			}
		}
	}

	private void OnDestroy()
	{
		if (!GameManager.IsSceneClosing)
		{
			for (int i = 0; i < _players.Count; i++)
			{
				_players[i].Restore();
			}
		}
	}

	private void LateUpdate()
	{
		for (int num = _players.Count - 1; num >= 0; num--)
		{
			AttachedPlayer attachedPlayer = _players[num];
			if (!attachedPlayer.Update())
			{
				RemoveAt(num);
			}
		}
	}

	public void Attach([NotNull] PlayerBehavior player)
	{
		Detach(player);
		if (_players.Count >= _maxPlayerCount)
		{
			return;
		}
		Transform transform = KUtility.FindTransformByDist(base.gameObject, player.CurrentPosition, "Attachment_Player");
		if (!(transform == null))
		{
			AttachedPlayer attachedPlayer = new AttachedPlayer();
			attachedPlayer.Set(player, transform);
			_players.Add(attachedPlayer);
			if (_players.Count >= _minPlayerCountToPlayAnim)
			{
				AnimatingModel component = GetComponent<AnimatingModel>();
				component.Play(_attachedAnimName);
			}
		}
	}

	public void Detach([NotNull] PlayerBehavior player, bool snapToExit = false)
	{
		int num = _players.FindIndex((AttachedPlayer x) => x.Player == player);
		if (num < 0)
		{
			return;
		}
		RemoveAt(num);
		if (snapToExit && player.IsLocalPlayer)
		{
			Transform transform = KUtility.FindTransformByName(base.gameObject, "Attachment_Exit");
			if (transform != null)
			{
				Singleton<PlayerController>.Instance().SnapToTarget(transform);
			}
		}
	}

	private void RemoveAt(int index)
	{
		AttachedPlayer attachedPlayer = _players[index];
		attachedPlayer.Restore();
		_players.RemoveAt(index);
		if (_players.Count < _minPlayerCountToPlayAnim)
		{
			AnimatingModel component = GetComponent<AnimatingModel>();
			component.Play(_detachedAnimName);
		}
	}
}
