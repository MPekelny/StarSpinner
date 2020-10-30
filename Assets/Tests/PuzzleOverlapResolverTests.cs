using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
	public class PuzzleOverlapResolverTests
	{
		[Test]
		public void CheckResolverDoesNotAdjustNonOverlapping()
		{
			PuzzleOverlapResolver resolver = new PuzzleOverlapResolver(5f);

			List<Transform> testTransforms = MakeListOfTransforms(4);
			testTransforms[0].localEulerAngles = testTransforms[0].localEulerAngles.With(nZ: 5f);
			testTransforms[1].localEulerAngles = testTransforms[1].localEulerAngles.With(nZ: 25f);
			testTransforms[2].localEulerAngles = testTransforms[2].localEulerAngles.With(nZ: 40f);
			testTransforms[3].localEulerAngles = testTransforms[3].localEulerAngles.With(nZ: 50f);

			resolver.ResolveOverlaps(testTransforms, testTransforms[0]);

			float expectedAngle = 5f;
			Assert.IsTrue(HelperMethods.EpsilonCheck(testTransforms[0].localEulerAngles.z, expectedAngle));
		}

		[Test]
		public void CheckResolverAdjustsOverlapping()
		{
			PuzzleOverlapResolver resolver = new PuzzleOverlapResolver(5f);

			List<Transform> testTransforms = MakeListOfTransforms(4);
			testTransforms[0].localEulerAngles = testTransforms[0].localEulerAngles.With(nZ: 22f);
			testTransforms[1].localEulerAngles = testTransforms[1].localEulerAngles.With(nZ: 25f);
			testTransforms[2].localEulerAngles = testTransforms[2].localEulerAngles.With(nZ: 40f);
			testTransforms[3].localEulerAngles = testTransforms[3].localEulerAngles.With(nZ: 50f);

			resolver.ResolveOverlaps(testTransforms, testTransforms[0]);

			float expectedAngle = 15f;
			Assert.IsTrue(HelperMethods.EpsilonCheck(testTransforms[0].localEulerAngles.z, expectedAngle));
		}

		[Test]
		public void CheckResolverAdjustsOverlappingBetweenTwoOthers()
		{
			PuzzleOverlapResolver resolver = new PuzzleOverlapResolver(5f);

			List<Transform> testTransforms = MakeListOfTransforms(4);
			testTransforms[0].localEulerAngles = testTransforms[0].localEulerAngles.With(nZ: 30f);
			testTransforms[1].localEulerAngles = testTransforms[1].localEulerAngles.With(nZ: 25f);
			testTransforms[2].localEulerAngles = testTransforms[2].localEulerAngles.With(nZ: 40f);
			testTransforms[3].localEulerAngles = testTransforms[3].localEulerAngles.With(nZ: 50f);

			resolver.ResolveOverlaps(testTransforms, testTransforms[0]);

			float expectedAngle = 15f;
			Assert.IsTrue(HelperMethods.EpsilonCheck(testTransforms[0].localEulerAngles.z, expectedAngle));
		}

		[Test]
		public void CheckResolverAdjustMultiple()
		{
			PuzzleOverlapResolver resolver = new PuzzleOverlapResolver(5f);

			List<Transform> testTransforms = MakeListOfTransforms(4);
			testTransforms[0].localEulerAngles = testTransforms[0].localEulerAngles.With(nZ: 25f);
			testTransforms[1].localEulerAngles = testTransforms[1].localEulerAngles.With(nZ: 26f);
			testTransforms[2].localEulerAngles = testTransforms[2].localEulerAngles.With(nZ: 27f);
			testTransforms[3].localEulerAngles = testTransforms[3].localEulerAngles.With(nZ: 28f);

			resolver.ResolveOverlaps(testTransforms);

			float expectedAngle0 = 16f;
			float expectedAngle1 = 38f;
			float expectedAngle2 = 48f;
			float expectedAngle3 = 28f;
			Assert.IsTrue(HelperMethods.EpsilonCheck(testTransforms[0].localEulerAngles.z, expectedAngle0));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testTransforms[1].localEulerAngles.z, expectedAngle1));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testTransforms[2].localEulerAngles.z, expectedAngle2));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testTransforms[3].localEulerAngles.z, expectedAngle3));
		}

		[Test]
		public void CheckResolverDoesNotAdjustMultiple()
		{
			PuzzleOverlapResolver resolver = new PuzzleOverlapResolver(5f);

			List<Transform> testTransforms = MakeListOfTransforms(4);
			testTransforms[0].localEulerAngles = testTransforms[0].localEulerAngles.With(nZ: 25f);
			testTransforms[1].localEulerAngles = testTransforms[1].localEulerAngles.With(nZ: 40f);
			testTransforms[2].localEulerAngles = testTransforms[2].localEulerAngles.With(nZ: 55f);
			testTransforms[3].localEulerAngles = testTransforms[3].localEulerAngles.With(nZ: 70f);

			resolver.ResolveOverlaps(testTransforms);

			float expectedAngle0 = 25f;
			float expectedAngle1 = 40f;
			float expectedAngle2 = 55f;
			float expectedAngle3 = 70f;
			Assert.IsTrue(HelperMethods.EpsilonCheck(testTransforms[0].localEulerAngles.z, expectedAngle0));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testTransforms[1].localEulerAngles.z, expectedAngle1));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testTransforms[2].localEulerAngles.z, expectedAngle2));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testTransforms[3].localEulerAngles.z, expectedAngle3));
		}

		private List<Transform> MakeListOfTransforms(int numTransforms)
		{
			List<Transform> transforms = new List<Transform>();
			for (int i = 0; i < numTransforms; i++)
			{
				GameObject g = new GameObject($"Simulated Spinner {i + 1}");
				transforms.Add(g.transform);
			}

			return transforms;
		}
	}
}

