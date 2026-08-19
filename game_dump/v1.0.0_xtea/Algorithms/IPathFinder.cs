using System.Collections.Generic;

namespace Algorithms;

[Author("Franco, Gustavo")]
internal interface IPathFinder
{
	bool Stopped { get; }

	HeuristicFormula Formula { get; set; }

	bool Diagonals { get; set; }

	bool HeavyDiagonals { get; set; }

	int HeuristicEstimate { get; set; }

	bool PunishChangeDirection { get; set; }

	bool ReopenCloseNodes { get; set; }

	bool TieBreaker { get; set; }

	int SearchLimit { get; set; }

	bool DebugProgress { get; set; }

	bool DebugFoundPath { get; set; }

	void FindPathStop();

	List<PathFinderNode> FindPath(Point start, Point end);
}
