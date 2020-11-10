using System.Collections.Generic;
using NUnit.Framework;

namespace Tests.PuzzleTests
{
	public class PuzzleSolutionCheckerTests
	{
		[Test]
		public void CheckSolutionAllZeroTo360ShouldSucceed()
		{
			PuzzleSolutionChecker checker = new PuzzleSolutionChecker(10f);
			List<float> angles = new List<float>();
			angles.Add(25f);
			angles.Add(28f);
			angles.Add(30f);
			angles.Add(33f);

			bool isSolved = checker.CheckIfSolved(angles);
			Assert.IsTrue(isSolved);
		}

		[Test]
		public void CheckSolutionSomeBefore360ShouldSucceed()
		{
			PuzzleSolutionChecker checker = new PuzzleSolutionChecker(10f);
			List<float> angles = new List<float>();
			angles.Add(357f);
			angles.Add(359f);
			angles.Add(2f);
			angles.Add(4f);

			bool isSolved = checker.CheckIfSolved(angles);
			Assert.IsTrue(isSolved);
		}

		[Test]
		public void CheckSolutionBarelyInRange()
		{
			PuzzleSolutionChecker checker = new PuzzleSolutionChecker(10f);
			List<float> angles = new List<float>();
			angles.Add(25f);
			angles.Add(28f);
			angles.Add(30f);
			angles.Add(35f);

			bool isSolved = checker.CheckIfSolved(angles);
			Assert.IsTrue(isSolved);
		}

		[Test]
		public void CheckSolutionAllZeroTo360ShouldNotSucceed()
		{
			PuzzleSolutionChecker checker = new PuzzleSolutionChecker(10f);
			List<float> angles = new List<float>();
			angles.Add(20f);
			angles.Add(23f);
			angles.Add(28f);
			angles.Add(45f);

			bool isSolved = checker.CheckIfSolved(angles);
			Assert.IsFalse(isSolved);
		}

		[Test]
		public void CheckSolutionSomeBefore360ShouldNotSucceed()
		{
			PuzzleSolutionChecker checker = new PuzzleSolutionChecker(10f);
			List<float> angles = new List<float>();
			angles.Add(354f);
			angles.Add(358f);
			angles.Add(2f);
			angles.Add(15f);

			bool isSolved = checker.CheckIfSolved(angles);
			Assert.IsFalse(isSolved);
		}

		[Test]
		public void CheckSolutionLargeNumberOfSpinnersShouldSucceed()
		{
			PuzzleSolutionChecker checker = new PuzzleSolutionChecker(10f);
			List<float> angles = new List<float>();
			angles.Add(25f);
			angles.Add(26f);
			angles.Add(26.5f);
			angles.Add(27f);
			angles.Add(28f);
			angles.Add(29f);
			angles.Add(30f);
			angles.Add(32f);
			angles.Add(33f);

			bool isSolved = checker.CheckIfSolved(angles);
			Assert.IsTrue(isSolved);
		}

		[Test]
		public void CheckSolutionRangesScrambledShouldSucceed()
		{
			PuzzleSolutionChecker checker = new PuzzleSolutionChecker(10f);
			List<float> angles = new List<float>();
			angles.Add(30f);
			angles.Add(28f);
			angles.Add(33f);
			angles.Add(25f);

			bool isSolved = checker.CheckIfSolved(angles);
			Assert.IsTrue(isSolved);
		}
	}
}

