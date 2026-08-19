using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ExploreAreaMissionMarker : MonoBehaviour
{
	public enum MissionState
	{
		Invalid = -1,
		Completed,
		NotInProgress,
		InProgress
	}

	[SerializeField]
	private GameObject _missionCompletedMarker;

	[SerializeField]
	private TweenerPlayer _missionMarker;

	private MissionState _missionState;

	public void ResetState()
	{
		_missionState = MissionState.Invalid;
	}

	public void TrySetState(MissionState state)
	{
		if (state > _missionState)
		{
			_missionState = state;
		}
	}

	public void Show()
	{
		switch (_missionState)
		{
		case MissionState.Completed:
			_missionCompletedMarker.SetActive(value: true);
			_missionMarker.gameObject.SetActive(value: false);
			break;
		case MissionState.InProgress:
			_missionCompletedMarker.SetActive(value: false);
			_missionMarker.gameObject.SetActive(value: true);
			_missionMarker.Play();
			break;
		default:
			_missionCompletedMarker.SetActive(value: false);
			_missionMarker.gameObject.SetActive(value: false);
			break;
		}
	}
}
