using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class CombatTargetWidget : MonoBehaviour
{
	[SerializeField]
	private HyperGaugeViewer _targetHp;

	[SerializeField]
	private UILabel _targetName;

	[SerializeField]
	private CombatTargetPortrait _targetPortrait;

	[SerializeField]
	private GameObject _aggroObject;

	private DamageableEntity _target;

	private int _targetGaugeUpdateFrame;

	private bool? _aggroAlertOn;

	public void SetTarget(DamageableEntity target)
	{
		_target = target;
		_targetGaugeUpdateFrame = 0;
		if (_target == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		PlayerBehavior component = target.GameObject.GetComponent<PlayerBehavior>();
		if ((bool)component)
		{
			_targetName.text = _target.GetName();
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(component.EntityId, OnPlayer);
		}
		else
		{
			_targetName.text = T._("{0} [E3494B]{1:lv:}[-]", _target.GetName(), _target.GetLevel());
		}
		_targetPortrait.SetPortrait(_target);
		UpdateTargetGauge(removeTrail: true);
		base.gameObject.gameObject.SetActive(value: true);
	}

	private void OnPlayer(PlayerInfo player)
	{
		_targetName.text = T._("{0} [E3494B]{1:lv:}[-]", player.Name, player.Level);
	}

	private void UpdateTargetGauge(bool removeTrail = false)
	{
		if (_targetGaugeUpdateFrame != _target.GaugeChanged)
		{
			_targetHp.Set(_target.GetLife(), smooth: true, _target.GetGaugeScale(), _target.GetLifeGaugeRatio());
			if (removeTrail)
			{
				_targetHp.RemoveTrail();
			}
			_targetGaugeUpdateFrame = _target.GaugeChanged;
		}
	}

	private void AggroAlert(bool show)
	{
		if (!_aggroAlertOn.HasValue || _aggroAlertOn != show)
		{
			_aggroAlertOn = show;
			_aggroObject.SetActive(show);
		}
	}

	private void UpdateAggro()
	{
		bool show = _target.TargetId == GameManager.PlayerId;
		AggroAlert(show);
	}

	private void LateUpdate()
	{
		if ((object)_target != null)
		{
			if (_target == null)
			{
				SetTarget(null);
				return;
			}
			UpdateTargetGauge();
			UpdateAggro();
		}
	}
}
