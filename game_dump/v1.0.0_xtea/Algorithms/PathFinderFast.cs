using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Algorithms;

[Author("Franco, Gustavo")]
public class PathFinderFast : IPathFinder
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	[Author("Franco, Gustavo")]
	internal struct PathFinderNodeFast
	{
		public int F;

		public int G;

		public ushort PX;

		public ushort PY;

		public byte Status;
	}

	[Author("Franco, Gustavo")]
	internal class ComparePFNodeMatrix : IComparer<int>
	{
		private PathFinderNodeFast[] mMatrix;

		public ComparePFNodeMatrix(PathFinderNodeFast[] matrix)
		{
			mMatrix = matrix;
		}

		public int Compare(int a, int b)
		{
			if (mMatrix[a].F > mMatrix[b].F)
			{
				return 1;
			}
			if (mMatrix[a].F < mMatrix[b].F)
			{
				return -1;
			}
			return 0;
		}
	}

	private byte[,] mGrid;

	private PriorityQueueB<int> mOpen;

	private List<PathFinderNode> mClose = new List<PathFinderNode>();

	private bool mStop;

	private bool mStopped = true;

	private int mHoriz;

	private HeuristicFormula mFormula = HeuristicFormula.Manhattan;

	private bool mDiagonals = true;

	private int mHEstimate = 2;

	private bool mPunishChangeDirection;

	private bool mReopenCloseNodes = true;

	private bool mTieBreaker;

	private bool mHeavyDiagonals;

	private int mSearchLimit = 2000;

	private bool mDebugProgress;

	private bool mDebugFoundPath;

	private PathFinderNodeFast[] mCalcGrid;

	private byte mOpenNodeValue = 1;

	private byte mCloseNodeValue = 2;

	private int mH;

	private int mLocation;

	private int mNewLocation;

	private ushort mLocationX;

	private ushort mLocationY;

	private ushort mNewLocationX;

	private ushort mNewLocationY;

	private int mCloseNodeCounter;

	private ushort mGridX;

	private ushort mGridY;

	private ushort mGridXMinus1;

	private ushort mGridYLog2;

	private bool mFound;

	private sbyte[,] mDirection = new sbyte[8, 2]
	{
		{ 0, -1 },
		{ 1, 0 },
		{ 0, 1 },
		{ -1, 0 },
		{ 1, -1 },
		{ 1, 1 },
		{ -1, 1 },
		{ -1, -1 }
	};

	private int mEndLocation;

	private int mNewG;

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
			if (mDiagonals)
			{
				mDirection = new sbyte[8, 2]
				{
					{ 0, -1 },
					{ 1, 0 },
					{ 0, 1 },
					{ -1, 0 },
					{ 1, -1 },
					{ 1, 1 },
					{ -1, 1 },
					{ -1, -1 }
				};
			}
			else
			{
				mDirection = new sbyte[4, 2]
				{
					{ 0, -1 },
					{ 1, 0 },
					{ 0, 1 },
					{ -1, 0 }
				};
			}
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

	public PathFinderFast(byte[,] grid)
	{
		if (grid == null)
		{
			throw new Exception("Grid cannot be null");
		}
		mGrid = grid;
		mGridX = (ushort)(mGrid.GetUpperBound(0) + 1);
		mGridY = (ushort)(mGrid.GetUpperBound(1) + 1);
		mGridXMinus1 = (ushort)(mGridX - 1);
		mGridYLog2 = (ushort)Math.Log((int)mGridY, 2.0);
		if (Math.Log((int)mGridX, 2.0) != (double)(int)Math.Log((int)mGridX, 2.0) || Math.Log((int)mGridY, 2.0) != (double)(int)Math.Log((int)mGridY, 2.0))
		{
			throw new Exception("Invalid Grid, size in X and Y must be power of 2");
		}
		if (mCalcGrid == null || mCalcGrid.Length != mGridX * mGridY)
		{
			mCalcGrid = new PathFinderNodeFast[mGridX * mGridY];
		}
		mOpen = new PriorityQueueB<int>(new ComparePFNodeMatrix(mCalcGrid));
	}

	public void FindPathStop()
	{
		mStop = true;
	}

	private static int Clamp(int value, int min, int max)
	{
		return Math.Max(Math.Min(value, max), min);
	}

	private Point ClampPoint(Point point)
	{
		return new Point(Clamp(point.X, 0, mGridX - 1), Clamp(point.Y, 0, mGridY - 1));
	}

	public List<PathFinderNode> FindPath(Point start, Point end)
	{
		lock (this)
		{
			mFound = false;
			mStop = false;
			mStopped = false;
			mCloseNodeCounter = 0;
			mOpenNodeValue += 2;
			mCloseNodeValue += 2;
			mOpen.Clear();
			mClose.Clear();
			start = ClampPoint(start);
			end = ClampPoint(end);
			mLocation = (start.Y << (int)mGridYLog2) + start.X;
			mEndLocation = (end.Y << (int)mGridYLog2) + end.X;
			mCalcGrid[mLocation].G = 0;
			mCalcGrid[mLocation].F = mHEstimate;
			mCalcGrid[mLocation].PX = (ushort)start.X;
			mCalcGrid[mLocation].PY = (ushort)start.Y;
			mCalcGrid[mLocation].Status = mOpenNodeValue;
			mOpen.Push(mLocation);
			while (mOpen.Count > 0 && !mStop)
			{
				mLocation = mOpen.Pop();
				if (mCalcGrid[mLocation].Status == mCloseNodeValue)
				{
					continue;
				}
				mLocationX = (ushort)(mLocation & mGridXMinus1);
				mLocationY = (ushort)(mLocation >> (int)mGridYLog2);
				if (mLocation == mEndLocation)
				{
					mCalcGrid[mLocation].Status = mCloseNodeValue;
					mFound = true;
					break;
				}
				if (mCloseNodeCounter > mSearchLimit)
				{
					mStopped = true;
					return null;
				}
				if (mPunishChangeDirection)
				{
					mHoriz = mLocationX - mCalcGrid[mLocation].PX;
				}
				for (int i = 0; i < ((!mDiagonals) ? 4 : 8); i++)
				{
					mNewLocationX = (ushort)(mLocationX + mDirection[i, 0]);
					mNewLocationY = (ushort)(mLocationY + mDirection[i, 1]);
					mNewLocation = (mNewLocationY << (int)mGridYLog2) + mNewLocationX;
					if (mNewLocationX >= mGridX || mNewLocationY >= mGridY || (mCalcGrid[mNewLocation].Status == mCloseNodeValue && !mReopenCloseNodes) || mGrid[mNewLocationX, mNewLocationY] == 0)
					{
						continue;
					}
					if (mHeavyDiagonals && i > 3)
					{
						mNewG = mCalcGrid[mLocation].G + (int)((double)(int)mGrid[mNewLocationX, mNewLocationY] * 2.41);
					}
					else
					{
						mNewG = mCalcGrid[mLocation].G + mGrid[mNewLocationX, mNewLocationY];
					}
					if (mPunishChangeDirection)
					{
						if (mNewLocationX - mLocationX != 0 && mHoriz == 0)
						{
							mNewG += Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y);
						}
						if (mNewLocationY - mLocationY != 0 && mHoriz != 0)
						{
							mNewG += Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y);
						}
					}
					if ((mCalcGrid[mNewLocation].Status != mOpenNodeValue && mCalcGrid[mNewLocation].Status != mCloseNodeValue) || mCalcGrid[mNewLocation].G > mNewG)
					{
						mCalcGrid[mNewLocation].PX = mLocationX;
						mCalcGrid[mNewLocation].PY = mLocationY;
						mCalcGrid[mNewLocation].G = mNewG;
						switch (mFormula)
						{
						default:
							mH = mHEstimate * (Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y));
							break;
						case HeuristicFormula.MaxDXDY:
							mH = mHEstimate * Math.Max(Math.Abs(mNewLocationX - end.X), Math.Abs(mNewLocationY - end.Y));
							break;
						case HeuristicFormula.DiagonalShortCut:
						{
							int num3 = Math.Min(Math.Abs(mNewLocationX - end.X), Math.Abs(mNewLocationY - end.Y));
							int num4 = Math.Abs(mNewLocationX - end.X) + Math.Abs(mNewLocationY - end.Y);
							mH = mHEstimate * 2 * num3 + mHEstimate * (num4 - 2 * num3);
							break;
						}
						case HeuristicFormula.Euclidean:
							mH = (int)((double)mHEstimate * Math.Sqrt(Math.Pow(mNewLocationY - end.X, 2.0) + Math.Pow(mNewLocationY - end.Y, 2.0)));
							break;
						case HeuristicFormula.EuclideanNoSQR:
							mH = (int)((double)mHEstimate * (Math.Pow(mNewLocationX - end.X, 2.0) + Math.Pow(mNewLocationY - end.Y, 2.0)));
							break;
						case HeuristicFormula.Custom1:
						{
							Point point = new Point(Math.Abs(end.X - mNewLocationX), Math.Abs(end.Y - mNewLocationY));
							int num = Math.Abs(point.X - point.Y);
							int num2 = Math.Abs((point.X + point.Y - num) / 2);
							mH = mHEstimate * (num2 + num + point.X + point.Y);
							break;
						}
						}
						if (mTieBreaker)
						{
							int num5 = mLocationX - end.X;
							int num6 = mLocationY - end.Y;
							int num7 = start.X - end.X;
							int num8 = start.Y - end.Y;
							int num9 = Math.Abs(num5 * num8 - num7 * num6);
							mH = (int)((double)mH + (double)num9 * 0.001);
						}
						mCalcGrid[mNewLocation].F = mNewG + mH;
						mOpen.Push(mNewLocation);
						mCalcGrid[mNewLocation].Status = mOpenNodeValue;
					}
				}
				mCloseNodeCounter++;
				mCalcGrid[mLocation].Status = mCloseNodeValue;
			}
			if (mFound)
			{
				mClose.Clear();
				int x = end.X;
				int y = end.Y;
				PathFinderNodeFast pathFinderNodeFast = mCalcGrid[(end.Y << (int)mGridYLog2) + end.X];
				PathFinderNode item = default(PathFinderNode);
				item.F = pathFinderNodeFast.F;
				item.G = pathFinderNodeFast.G;
				item.H = 0;
				item.PX = pathFinderNodeFast.PX;
				item.PY = pathFinderNodeFast.PY;
				item.X = end.X;
				item.Y = end.Y;
				while (item.X != item.PX || item.Y != item.PY)
				{
					mClose.Add(item);
					x = item.PX;
					y = item.PY;
					pathFinderNodeFast = mCalcGrid[(y << (int)mGridYLog2) + x];
					item.F = pathFinderNodeFast.F;
					item.G = pathFinderNodeFast.G;
					item.H = 0;
					item.PX = pathFinderNodeFast.PX;
					item.PY = pathFinderNodeFast.PY;
					item.X = x;
					item.Y = y;
				}
				mClose.Add(item);
				mStopped = true;
				return mClose;
			}
			mStopped = true;
			return null;
		}
	}
}
