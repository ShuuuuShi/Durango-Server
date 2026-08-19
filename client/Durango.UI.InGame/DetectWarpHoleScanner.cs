using System;
using System.Collections.Generic;
using Durango.Terrain;
using Durango.Utils.Extensions;
using Messages;
using Shared.System;
using UnityEngine;

namespace Durango.UI.InGame;

public class DetectWarpHoleScanner : MonoBehaviour
{
	private class SearchResultCompare : IComparer<SearchResult>
	{
		public static SearchResultCompare Comparer = new SearchResultCompare();

		public Vector3 Position { get; set; }

		public int Compare(SearchResult x, SearchResult y)
		{
			return GetDistance(x) - GetDistance(y);
		}

		private int GetDistance(SearchResult result)
		{
			return (int)Vector3.Distance(Position, Util.TilePositionToClientPosition(result.Tile));
		}
	}

	[SerializeField]
	private DetectWarpHoleRadar _detectWarpHoleRadar;

	[SerializeField]
	private TweenAlpha _tweenAlphaFadeOut;

	[SerializeField]
	private ListObjectPool _detectWarpHoleArrows;

	[SerializeField]
	[EnumList(typeof(Shared.System.PointOfInterest), false, 0, -1)]
	private Color[] _arrowColors;

	[SerializeField]
	private int _preSpinCount;

	[SerializeField]
	private int _postSpinCount;

	private int _additionalSpinCount;

	public bool IsShow { get; private set; }

	public event Action Finished;

	public void Show(SearchResult[] results, Vector3 position)
	{
		IsShow = true;
		_tweenAlphaFadeOut.ResetToBeginning();
		_tweenAlphaFadeOut.enabled = false;
		base.gameObject.SetActive(value: true);
		SetSearchResults(results, position);
		_additionalSpinCount = GetAdditionalCountBySkill();
		_detectWarpHoleRadar.BeginSpinning();
	}

	public void Hide()
	{
		if (IsShow)
		{
			IsShow = false;
			base.gameObject.SetActive(value: false);
			if (this.Finished != null)
			{
				this.Finished();
			}
		}
	}

	public void UpdatePosition(Vector3 position)
	{
		if (base.gameObject.activeSelf)
		{
			UpdateArrows(position);
			int num = _detectWarpHoleRadar.CurrentSpinCount - _preSpinCount;
			ShowCurrentArrow(num);
			ShowPreviousArrows(num);
			if (num >= _detectWarpHoleArrows.Count + _postSpinCount + _additionalSpinCount)
			{
				BeginFadeOut();
			}
		}
	}

	public void Init()
	{
		_detectWarpHoleRadar.Init();
		_detectWarpHoleArrows.Init(null);
		_tweenAlphaFadeOut.AddOnFinished(OnFinishedTweenAlphaFadeOut);
		Hide();
	}

	private void SetSearchResults(SearchResult[] results, Vector3 position)
	{
		SearchResultCompare.Comparer.Position = position;
		Array.Sort(results, SearchResultCompare.Comparer);
		_detectWarpHoleArrows.Set(results.Length);
		for (int i = 0; i < _detectWarpHoleArrows.Count; i++)
		{
			DetectWarpHoleArrow detectWarpHoleArrow = _detectWarpHoleArrows.Get<DetectWarpHoleArrow>(i);
			Vector3 target = Util.TilePositionToClientPosition(results[i].Tile);
			Color color = _arrowColors.Get((int)results[i].Type, Color.white);
			detectWarpHoleArrow.SetTarget(target, color);
			detectWarpHoleArrow.gameObject.SetActive(value: false);
		}
		UpdateArrows(position);
	}

	private static int GetAdditionalCountBySkill()
	{
		return (int)GameSystem<StatisticsSystem>.Instance().GetModifier("poi_searching_plus");
	}

	private void BeginFadeOut()
	{
		if (!_tweenAlphaFadeOut.enabled)
		{
			_tweenAlphaFadeOut.tweenFactor = 0f;
			_tweenAlphaFadeOut.PlayForward();
		}
	}

	private void OnFinishedTweenAlphaFadeOut()
	{
		_detectWarpHoleRadar.FinishSpinning();
		Hide();
	}

	private void ShowCurrentArrow(int index)
	{
		if (0 <= index && index < _detectWarpHoleArrows.Count)
		{
			DetectWarpHoleArrow detectWarpHoleArrow = _detectWarpHoleArrows.Get<DetectWarpHoleArrow>(index);
			if (!detectWarpHoleArrow.gameObject.activeSelf && _detectWarpHoleRadar.CurrentAngle <= detectWarpHoleArrow.CurrentAngle)
			{
				detectWarpHoleArrow.gameObject.SetActive(value: true);
			}
		}
	}

	private void ShowPreviousArrows(int index)
	{
		int num = Mathf.Min(index, _detectWarpHoleArrows.Count);
		for (int i = 0; i < num; i++)
		{
			DetectWarpHoleArrow detectWarpHoleArrow = _detectWarpHoleArrows.Get<DetectWarpHoleArrow>(i);
			if (!detectWarpHoleArrow.gameObject.activeSelf)
			{
				detectWarpHoleArrow.gameObject.SetActive(value: true);
			}
		}
	}

	private void UpdateArrows(Vector3 position)
	{
		for (int i = 0; i < _detectWarpHoleArrows.Count; i++)
		{
			DetectWarpHoleArrow detectWarpHoleArrow = _detectWarpHoleArrows.Get<DetectWarpHoleArrow>(i);
			detectWarpHoleArrow.UpdatePosition(position);
		}
	}
}
