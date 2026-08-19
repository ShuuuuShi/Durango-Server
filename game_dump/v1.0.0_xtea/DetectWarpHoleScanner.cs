using System;
using System.Collections.Generic;
using Messages;
using UnityEngine;

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
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			return (int)Vector3.Distance(Position, TerrainA6.TilePositionToClientPosition(result.Tile));
		}
	}

	private const string _searchingModifierName = "poi_searching_plus";

	[SerializeField]
	private DetectWarpHoleRadar _detectWarpHoleRadar;

	[SerializeField]
	private TweenAlpha _tweenAlphaFadeOut;

	[SerializeField]
	private ListObjectPool _detectWarpHoleArrows;

	[SerializeField]
	private int _preSpinCount;

	[SerializeField]
	private int _postSpinCount;

	private int _additionalSpinCount;

	public void Show(SearchResult[] results, Vector3 position)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		_tweenAlphaFadeOut.ResetToBeginning();
		((Behaviour)_tweenAlphaFadeOut).enabled = false;
		((Component)this).gameObject.SetActive(true);
		SetSearchResults(results, position);
		_additionalSpinCount = GetAdditionalCountBySkill();
		_detectWarpHoleRadar.BeginSpinning();
	}

	public void Hide()
	{
		((Component)this).gameObject.SetActive(false);
	}

	public void UpdatePosition(Vector3 position)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (((Component)this).gameObject.activeSelf)
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
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		SearchResultCompare.Comparer.Position = position;
		Array.Sort(results, SearchResultCompare.Comparer);
		_detectWarpHoleArrows.Set(results.Length);
		for (int i = 0; i < _detectWarpHoleArrows.Count; i++)
		{
			DetectWarpHoleArrow detectWarpHoleArrow = ((ListObjectPoolBase<GameObject>)_detectWarpHoleArrows).Get<DetectWarpHoleArrow>(i);
			Vector3 target = TerrainA6.TilePositionToClientPosition(results[i].Tile);
			detectWarpHoleArrow.SetTarget(target);
			((Component)detectWarpHoleArrow).gameObject.SetActive(false);
		}
		UpdateArrows(position);
	}

	private int GetAdditionalCountBySkill()
	{
		Dictionary<string, float> modifiers = GameSystem<StatisticsSystem>.Instance().Modifiers;
		if (modifiers != null && modifiers.TryGetValue("poi_searching_plus", out var value))
		{
			return (int)value;
		}
		return 0;
	}

	private void BeginFadeOut()
	{
		if (!((Behaviour)_tweenAlphaFadeOut).enabled)
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
			DetectWarpHoleArrow detectWarpHoleArrow = ((ListObjectPoolBase<GameObject>)_detectWarpHoleArrows).Get<DetectWarpHoleArrow>(index);
			if (!((Component)detectWarpHoleArrow).gameObject.activeSelf && _detectWarpHoleRadar.CurrentAngle <= detectWarpHoleArrow.CurrentAngle)
			{
				((Component)detectWarpHoleArrow).gameObject.SetActive(true);
			}
		}
	}

	private void ShowPreviousArrows(int index)
	{
		int num = Mathf.Min(index, _detectWarpHoleArrows.Count);
		for (int i = 0; i < num; i++)
		{
			DetectWarpHoleArrow detectWarpHoleArrow = ((ListObjectPoolBase<GameObject>)_detectWarpHoleArrows).Get<DetectWarpHoleArrow>(i);
			if (!((Component)detectWarpHoleArrow).gameObject.activeSelf)
			{
				((Component)detectWarpHoleArrow).gameObject.SetActive(true);
			}
		}
	}

	private void UpdateArrows(Vector3 position)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _detectWarpHoleArrows.Count; i++)
		{
			DetectWarpHoleArrow detectWarpHoleArrow = ((ListObjectPoolBase<GameObject>)_detectWarpHoleArrows).Get<DetectWarpHoleArrow>(i);
			detectWarpHoleArrow.UpdatePosition(position);
		}
	}
}
