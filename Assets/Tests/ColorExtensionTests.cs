using NUnit.Framework;
using UnityEngine;

namespace Tests.UtilityTests
{
	public class ColorExtensionTests
	{
		[Test]
		public void TestColorWithNoChanges()
		{
			Color testColor = new Color(0.5f, 0.5f, 0.5f, 1f);
			testColor = testColor.With();

			float expectedR = 0.5f;
			float expectedG = 0.5f;
			float expectedB = 0.5f;
			float expectedA = 1f;

			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.r, expectedR));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.g, expectedG));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.b, expectedB));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.a, expectedA));
		}

		[Test]
		public void TestColorWithChangeRValue()
		{
			Color testColor = new Color(0.5f, 0.5f, 0.5f, 1f);
			testColor = testColor.With(nR: 0.8f);

			float expectedR = 0.8f;
			float expectedG = 0.5f;
			float expectedB = 0.5f;
			float expectedA = 1f;

			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.r, expectedR));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.g, expectedG));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.b, expectedB));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.a, expectedA));
		}

		[Test]
		public void TestColorWithChangeGValue()
		{
			Color testColor = new Color(0.5f, 0.5f, 0.5f, 1f);
			testColor = testColor.With(nG: 0.3f);

			float expectedR = 0.5f;
			float expectedG = 0.3f;
			float expectedB = 0.5f;
			float expectedA = 1f;

			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.r, expectedR));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.g, expectedG));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.b, expectedB));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.a, expectedA));
		}

		[Test]
		public void TestColorWithChangeBValue()
		{
			Color testColor = new Color(0.5f, 0.5f, 0.5f, 1f);
			testColor = testColor.With(nB: 0.75f);

			float expectedR = 0.5f;
			float expectedG = 0.5f;
			float expectedB = 0.75f;
			float expectedA = 1f;

			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.r, expectedR));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.g, expectedG));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.b, expectedB));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.a, expectedA));
		}

		[Test]
		public void TestColorWithChangeAValue()
		{
			Color testColor = new Color(0.5f, 0.5f, 0.5f, 1f);
			testColor = testColor.With(nA: 0f);

			float expectedR = 0.5f;
			float expectedG = 0.5f;
			float expectedB = 0.5f;
			float expectedA = 0f;

			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.r, expectedR));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.g, expectedG));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.b, expectedB));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.a, expectedA));
		}

		[Test]
		public void TestColorWithChangeAllValues()
		{
			Color testColor = new Color(0.5f, 0.5f, 0.5f, 1f);
			testColor = testColor.With(0.25f, 1f, 0.6f, 0.8f);

			float expectedR = 0.25f;
			float expectedG = 1f;
			float expectedB = 0.6f;
			float expectedA = 0.8f;

			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.r, expectedR));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.g, expectedG));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.b, expectedB));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testColor.a, expectedA));
		}
	}
}

