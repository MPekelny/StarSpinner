using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPuzzleScreen : PuzzleScreen
{
	public List<Star> Stars => _stars;
	public PuzzleData ActivePuzzle => _activePuzzle;
	public PuzzleSpinnersHelper PuzzleSpinnerHelper => _puzzleSpinnersHelper;
	public PuzzleSolutionChecker PuzzleSolutionChecker => _solutionChecker;
	public PuzzleOverlapResolver PuzzleOverlapResolver => _overlapResolver;

	public void TestSetupPuzzle()
	{
		SetupPuzzle();
	}

	public void TestCleanup()
	{
		Cleanup();
	}
}
