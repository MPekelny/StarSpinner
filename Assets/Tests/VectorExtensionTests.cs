using NUnit.Framework;
using UnityEngine;

namespace Tests.UtilityTests
{
	public class VectorExtensionTests
	{
		[Test]
		public void TestVectorWithChangesAllValues()
		{
			Vector3 testVector = new Vector3(5f, 10f, 15f);
			testVector = testVector.With(7f, 12f, 17f);

			float expectedX = 7f;
			float expectedY = 12f;
			float expectedZ = 17f;

			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.x, expectedX));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.y, expectedY));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.z, expectedZ));
		}

		[Test]
		public void TestVectorWithChangesXValue()
		{
			Vector3 testVector = new Vector3(5f, 10f, 15f);
			testVector = testVector.With(nX: 7f);

			float expectedX = 7f;
			float expectedY = 10f;
			float expectedZ = 15f;

			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.x, expectedX));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.y, expectedY));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.z, expectedZ));
		}

		[Test]
		public void TestVectorWithChangesYValue()
		{
			Vector3 testVector = new Vector3(5f, 10f, 15f);
			testVector = testVector.With(nY: 12f);

			float expectedX = 5f;
			float expectedY = 12f;
			float expectedZ = 15f;

			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.x, expectedX));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.y, expectedY));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.z, expectedZ));
		}

		[Test]
		public void TestVectorWithChangesZValue()
		{
			Vector3 testVector = new Vector3(5f, 10f, 15f);
			testVector = testVector.With(nZ: 17f);

			float expectedX = 5f;
			float expectedY = 10f;
			float expectedZ = 17f;

			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.x, expectedX));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.y, expectedY));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.z, expectedZ));
		}

		public void TestVectorWithChangesNoValues()
		{
			Vector3 testVector = new Vector3(5f, 10f, 15f);
			testVector = testVector.With();

			float expectedX = 5f;
			float expectedY = 10f;
			float expectedZ = 15f;

			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.x, expectedX));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.y, expectedY));
			Assert.IsTrue(HelperMethods.EpsilonCheck(testVector.z, expectedZ));
		}
	}
}

