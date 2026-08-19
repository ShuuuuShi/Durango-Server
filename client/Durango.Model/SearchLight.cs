using Shared.Battle;
using UnityEngine;

namespace Durango.Model;

public class SearchLight : MonoBehaviour
{
	[SerializeField]
	private float _searchLightDuration = 5f;

	[SerializeField]
	private Vector3 _fixBoneRotation = new Vector3(0f, -90f, -90f);

	[SerializeField]
	private float _searchLightMaxDistance = 1000f;

	[SerializeField]
	private float _searchLightDistanceRatio = 1f;

	private float _searchLightUntil;

	private Transform _targetTransform;

	private bool _lightOn;

	private void Start()
	{
		ActivateChildren(activate: false, updateForcibly: true);
	}

	private void Update()
	{
		if (_targetTransform == null)
		{
			return;
		}
		if (Time.time > _searchLightUntil)
		{
			_targetTransform = null;
			ActivateChildren(activate: false);
			return;
		}
		float magnitude = (base.transform.position - _targetTransform.position).magnitude;
		if (magnitude > _searchLightMaxDistance)
		{
			ActivateChildren(activate: false);
			return;
		}
		if (!_lightOn)
		{
			ActivateChildren(activate: true);
		}
		base.transform.LookAt(_targetTransform.position);
		base.transform.Rotate(_fixBoneRotation);
		base.transform.localScale = new Vector3(magnitude * _searchLightDistanceRatio, 1f, 1f);
	}

	private void ActivateChildren(bool activate, bool updateForcibly = false)
	{
		if (updateForcibly || _lightOn != activate)
		{
			int childCount = base.transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				base.transform.GetChild(i).gameObject.SetActive(activate);
			}
			_lightOn = activate;
		}
	}

	public void SetEnemy(string enemyId)
	{
		DamageableEntity damageableEntity = ((!string.IsNullOrEmpty(enemyId)) ? GameSystem<CombatSystem>.Instance().DamageableEntities.Find(enemyId) : null);
		if (damageableEntity == null)
		{
			_targetTransform = null;
			ActivateChildren(activate: false);
		}
		else
		{
			_targetTransform = damageableEntity.GetBodyPartTransform(BodyPart.Body);
			_searchLightUntil = Time.time + _searchLightDuration;
			ActivateChildren(activate: true);
		}
	}
}
