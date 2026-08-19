using System;
using System.Collections.Generic;

namespace Algorithms;

[Author("Franco, Gustavo")]
public class PathFinder : IPathFinder
{
	[Author("Franco, Gustavo")]
	internal class ComparePFNode : IComparer<PathFinderNode>
	{
		public int Compare(PathFinderNode x, PathFinderNode y)
		{
			if (x.F > y.F)
			{
				return 1;
			}
			if (x.F < y.F)
			{
				return -1;
			}
			return 0;
		}
	}

	private byte[,] mGrid;

	private PriorityQueueB<PathFinderNode> mOpen = new PriorityQueueB<PathFinderNode>(new ComparePFNode());

	private List<PathFinderNode> mClose = new List<PathFinderNode>();

	private bool mStop;

	private bool mStopped = true;

	private int mHoriz;

	private HeuristicFormula mFormula = HeuristicFormula.Manhattan;

	private bool mDiagonals = true;

	private int mHEstimate = 2;

	private bool mPunishChangeDirection;

	private bool mReopenCloseNodes;

	private bool mTieBreaker;

	private bool mHeavyDiagonals;

	private int mSearchLimit = 2000;

	private bool mDebugProgress;

	private bool mDebugFoundPath;

	public bool Stopped => mStopped;

	public HeuristicFormula Formula
	{
		get
		{
			return mFormula;
		}
		set
		{
			mFormula = value;
		}
	}

	public bool Diagonals
	{
		get
		{
			return mDiagonals;
		}
		set
		{
			mDiagonals = value;
		}
	}

	public bool HeavyDiagonals
	{
		get
		{
			return mHeavyDiagonals;
		}
		set
		{
			mHeavyDiagonals = value;
		}
	}

	public int HeuristicEstimate
	{
		get
		{
			return mHEstimate;
		}
		set
		{
			mHEstimate = value;
		}
	}

	public bool PunishChangeDirection
	{
		get
		{
			return mPunishChangeDirection;
		}
		set
		{
			mPunishChangeDirection = value;
		}
	}

	public bool ReopenCloseNodes
	{
		get
		{
			return mReopenCloseNodes;
		}
		set
		{
			mReopenCloseNodes = value;
		}
	}

	public bool TieBreaker
	{
		get
		{
			return mTieBreaker;
		}
		set
		{
			mTieBreaker = value;
		}
	}

	public int SearchLimit
	{
		get
		{
			return mSearchLimit;
		}
		set
		{
			mSearchLimit = value;
		}
	}

	public bool DebugProgress
	{
		get
		{
			return mDebugProgress;
		}
		set
		{
			mDebugProgress = value;
		}
	}

	public bool DebugFoundPath
	{
		get
		{
			return mDebugFoundPath;
		}
		set
		{
			mDebugFoundPath = value;
		}
	}

	public PathFinder(byte[,] grid)
	{
		if (grid == null)
		{
			throw new Exception("Grid cannot be null");
		}
		mGrid = grid;
	}

	public void FindPathStop()
	{
		mStop = true;
	}

	public List<PathFinderNode> FindPath(Point start, Point end)
	{
		bool flag = false;
		int upperBound = mGrid.GetUpperBound(0);
		int upperBound2 = mGrid.GetUpperBound(1);
		mStop = false;
		mStopped = false;
		mOpen.Clear();
		mClose.Clear();
		sbyte[,] array = ((!mDiagonals) ? new sbyte[4, 2]
		{
			{ 0, -1 },
			{ 1, 0 },
			{ 0, 1 },
			{ -1, 0 }
		} : new sbyte[8, 2]
		{
			{ 0, -1 },
			{ 1, 0 },
			{ 0, 1 },
			{ -1, 0 },
			{ 1, -1 },
			{ 1, 1 },
			{ -1, 1 },
			{ -1, -1 }
		});
		PathFinderNode item = default(PathFinderNode);
		item.G = 0;
		item.H = mHEstimate;
		item.F = item.G + item.H;
		item.X = start.X;
		item.Y = start.Y;
		item.PX = item.X;
		item.PY = item.Y;
		mOpen.Push(item);
		PathFinderNode item2 = default(PathFinderNode);
		while (mOpen.Count > 0 && !mStop)
		{
			item = mOpen.Pop();
			if (item.X == end.X && item.Y == end.Y)
			{
				mClose.Add(item);
				flag = true;
				break;
			}
			if (mClose.Count > mSearchLimit)
			{
				mStopped = true;
				return null;
			}
			if (mPunishChangeDirection)
			{
				mHoriz = item.X - item.PX;
			}
			for (int i = 0; i < ((!mDiagonals) ? 4 : 8); i++)
			{
				item2.X = item.X + array[i, 0];
				item2.Y = item.Y + array[i, 1];
				if (item2.X < 0 || item2.Y < 0 || item2.X >= upperBound || item2.Y >= upperBound2)
				{
					continue;
				}
				int num = ((!mHeavyDiagonals || i <= 3) ? (item.G + mGrid[item2.X, item2.Y]) : (item.G + (int)((double)(int)mGrid[item2.X, item2.Y] * 2.41)));
				if (num == item.G)
				{
					continue;
				}
				if (mPunishChangeDirection)
				{
					if (item2.X - item.X != 0 && mHoriz == 0)
					{
						num += 20;
					}
					if (item2.Y - item.Y != 0 && mHoriz != 0)
					{
						num += 20;
					}
				}
				int num2 = -1;
				for (int j = 0; j < mOpen.Count; j++)
				{
					if (mOpen[j].X == item2.X && mOpen[j].Y == item2.Y)
					{
						num2 = j;
						break;
					}
				}
				if (num2 != -1 && mOpen[num2].G <= num)
				{
					continue;
				}
				int num3 = -1;
				for (int k = 0; k < mClose.Count; k++)
				{
					if (mClose[k].X == item2.X && mClose[k].Y == item2.Y)
					{
						num3 = k;
						break;
					}
				}
				if (num3 == -1 || (!mReopenCloseNodes && mClose[num3].G > num))
				{
					item2.PX = item.X;
					item2.PY = item.Y;
					item2.G = num;
					switch (mFormula)
					{
					default:
						item2.H = mHEstimate * (Math.Abs(item2.X - end.X) + Math.Abs(item2.Y - end.Y));
						break;
					case HeuristicFormula.MaxDXDY:
						item2.H = mHEstimate * Math.Max(Math.Abs(item2.X - end.X), Math.Abs(item2.Y - end.Y));
						break;
					case HeuristicFormula.DiagonalShortCut:
					{
						int num6 = Math.Min(Math.Abs(item2.X - end.X), Math.Abs(item2.Y - end.Y));
						int num7 = Math.Abs(item2.X - end.X) + Math.Abs(item2.Y - end.Y);
						item2.H = mHEstimate * 2 * num6 + mHEstimate * (num7 - 2 * num6);
						break;
					}
					case HeuristicFormula.Euclidean:
						item2.H = (int)((double)mHEstimate * Math.Sqrt(Math.Pow(item2.X - end.X, 2.0) + Math.Pow(item2.Y - end.Y, 2.0)));
						break;
					case HeuristicFormula.EuclideanNoSQR:
						item2.H = (int)((double)mHEstimate * (Math.Pow(item2.X - end.X, 2.0) + Math.Pow(item2.Y - end.Y, 2.0)));
						break;
					case HeuristicFormula.Custom1:
					{
						Point point = new Point(Math.Abs(end.X - item2.X), Math.Abs(end.Y - item2.Y));
						int num4 = Math.Abs(point.X - point.Y);
						int num5 = Math.Abs((point.X + point.Y - num4) / 2);
						item2.H = mHEstimate * (num5 + num4 + point.X + point.Y);
						break;
					}
					}
					if (mTieBreaker)
					{
						int num8 = item.X - end.X;
						int num9 = item.Y - end.Y;
						int num10 = start.X - end.X;
						int num11 = start.Y - end.Y;
						int num12 = Math.Abs(num8 * num11 - num10 * num9);
						item2.H = (int)((double)item2.H + (double)num12 * 0.001);
					}
					item2.F = item2.G + item2.H;
					mOpen.Push(item2);
				}
			}
			mClose.Add(item);
		}
		if (flag)
		{
			PathFinderNode pathFinderNode = mClose[mClose.Count - 1];
			for (int num13 = mClose.Count - 1; num13 >= 0; num13--)
			{
				if ((pathFinderNode.PX == mClose[num13].X && pathFinderNode.PY == mClose[num13].Y) || num13 == mClose.Count - 1)
				{
					pathFinderNode = mClose[num13];
				}
				else
				{
					mClose.RemoveAt(num13);
				}
			}
			mStopped = true;
			return mClose;
		}
		mStopped = true;
		return null;
	}
}
