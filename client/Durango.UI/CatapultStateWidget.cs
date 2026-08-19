using Durango.Network;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class CatapultStateWidget : UIWidget
{
	[SerializeField]
	private UILabel _bulletCountLabel;

	[SerializeField]
	private GameObject _readyToFireObject;

	[SerializeField]
	private GameObject _cooltimeObject;

	[SerializeField]
	private UILabel _cooltimeLabel;

	[SerializeField]
	private UISprite _cooltimeSprite;

	private float _cooltimeStart;

	private float _cooltimeEnd;

	private int _cooltimePercent;

	private int _remainCooltime;

	private Catapult _catapult;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (!Application.isPlaying)
		{
			return;
		}
		Artifact.ArtifactStateChanged += OnCatapultStateChanged;
		GameSystem<CombatSystem>.Instance().VehicleProjectileFired += OnVehicleProjectileFired;
		_catapult = null;
		PlayerBehavior localPlayer = PlayerBehavior.LocalPlayer;
		if (localPlayer == null)
		{
			return;
		}
		VehicleBase vehicle = localPlayer.Driver.Vehicle;
		if (vehicle == null)
		{
			return;
		}
		VehicleCatapult vehicleCatapult = vehicle as VehicleCatapult;
		if (vehicleCatapult == null)
		{
			return;
		}
		Artifact artifact = vehicleCatapult.GetArtifact();
		if (!(artifact == null))
		{
			_catapult = artifact.GetArtifactComponent<Catapult>();
			if (_catapult != null)
			{
				OnCatapultStateChanged(_catapult.Artifact);
			}
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			Artifact.ArtifactStateChanged -= OnCatapultStateChanged;
			GameSystem<CombatSystem>.Instance().VehicleProjectileFired -= OnVehicleProjectileFired;
		}
	}

	private void OnCatapultStateChanged(Artifact artifact)
	{
		if (_catapult != null && !(_catapult.Artifact != artifact))
		{
			ArtifactState artifactState = _catapult.Artifact.ArtifactState;
			CatapultState? catapult = artifactState.Catapult;
			int current = (catapult.HasValue ? artifactState.Catapult.Value.RemainedProjectilesSize : 0);
			CatapultState? catapult2 = artifactState.Catapult;
			SetBulletCount(current, catapult2.HasValue ? artifactState.Catapult.Value.MaxProjectilesSize : 0);
		}
	}

	private void OnVehicleProjectileFired(VehicleProjectileFired msg)
	{
		if (!(_catapult.EntityId != msg.EntityId))
		{
			float time = Time.time;
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			float start = time;
			float end = time + (float)(msg.CoolingUntil - predictedServerTime);
			SoundManager.PlayEvent("ui_catapult_point_area");
			SetCooltime(start, end);
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (!Application.isPlaying)
		{
			return;
		}
		float time = Time.time;
		if (time > _cooltimeEnd)
		{
			if (!_readyToFireObject.gameObject.activeInHierarchy)
			{
				_readyToFireObject.gameObject.SetActive(value: true);
				SoundManager.PlayEvent("ui_catapult_available");
			}
			_cooltimeObject.gameObject.SetActive(value: false);
			return;
		}
		float num = _cooltimeEnd - _cooltimeStart;
		float num2 = time - _cooltimeStart;
		float num3 = num2 / num;
		int num4 = Mathf.CeilToInt(num3 * 100f);
		if (num4 == _cooltimePercent)
		{
			return;
		}
		_cooltimePercent = num4;
		_readyToFireObject.gameObject.SetActive(value: false);
		_cooltimeObject.gameObject.SetActive(value: true);
		int num5 = Mathf.CeilToInt(num - num2);
		if (_remainCooltime != num5)
		{
			if (_remainCooltime != -1 && num5 > 0)
			{
				SoundManager.PlayEvent("ui_catapult_cooltime");
			}
			_cooltimeLabel.text = num5.ToString();
			_remainCooltime = num5;
		}
		_cooltimeSprite.fillAmount = 1f - num3;
	}

	private void SetBulletCount(int current, int max)
	{
		_bulletCountLabel.text = T._("투척물 {0}/{1}", current, max);
	}

	private void SetCooltime(float start, float end)
	{
		_cooltimeStart = start;
		_cooltimeEnd = end;
		_cooltimePercent = -1;
		_remainCooltime = -1;
	}
}
