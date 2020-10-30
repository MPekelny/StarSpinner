using System;
using NUnit.Framework;

namespace Tests
{
	public class TupleExtensionTests
	{
		/// <summary>
		/// Testing two tuples that should not overlap.
		/// </summary>
		[Test]
		public void TuplesShouldNotOverlapTest()
		{
			// Test 1, a's range is lower than b's, should return false.
			Tuple<float, float> test1TupleA = new Tuple<float, float>(0f, 10f);
			Tuple<float, float> test1TupleB = new Tuple<float, float>(15f, 25f);
			bool overlaps = test1TupleA.Overlaps(test1TupleB);

			Assert.IsFalse(overlaps);

			// Test 2, same as 1, just the b tuple range is the lower one.
			Tuple<float, float> test2TupleA = new Tuple<float, float>(30f, 45f);
			Tuple<float, float> test2TupleB = new Tuple<float, float>(1f, 20f);
			overlaps = test2TupleA.Overlaps(test2TupleB);

			Assert.IsFalse(overlaps);
		}

		/// <summary>
		/// Testing 2 tuples that should overlap.
		/// </summary>
		[Test]
		public void TuplesShouldOverlapTest()
		{
			// Test 1, a's range is lower than b's, should return true.
			Tuple<float, float> test1TupleA = new Tuple<float, float>(0f, 10f);
			Tuple<float, float> test1TupleB = new Tuple<float, float>(9f, 25f);
			bool overlaps = test1TupleA.Overlaps(test1TupleB);

			Assert.IsTrue(overlaps);

			// Test 2, same as 1, just the b tuple range is the lower one.
			Tuple<float, float> test2TupleA = new Tuple<float, float>(15f, 45f);
			Tuple<float, float> test2TupleB = new Tuple<float, float>(1f, 20f);
			overlaps = test2TupleA.Overlaps(test2TupleB);

			Assert.IsTrue(overlaps);
		}

		/// <summary>
		/// Testing two tuples where one is completely contained in the other.
		/// </summary>
		[Test]
		public void ContainedTuplesTest()
		{
			// Test1, a's range contains b's range, should return true.
			Tuple<float, float> test1TupleA = new Tuple<float, float>(0f, 10f);
			Tuple<float, float> test1TupleB = new Tuple<float, float>(5f, 8f);
			bool overlaps = test1TupleA.Overlaps(test1TupleB);

			Assert.IsTrue(overlaps);

			// Test 2, same as 1 except b's range contains a, should return true.
			Tuple<float, float> test2TupleA = new Tuple<float, float>(8f, 15f);
			Tuple<float, float> test2TupleB = new Tuple<float, float>(1f, 20f);
			overlaps = test2TupleA.Overlaps(test2TupleB);

			Assert.IsTrue(overlaps);
		}

		/// <summary>
		/// Testing if one or both tuples have item2 as the low number instead of item1 and they should not overlap.
		/// </summary>
		[Test]
		public void FlippedTuplesNotOverlapTest()
		{
			// Test1, a' range is reversed, should return false.
			Tuple<float, float> test1TupleA = new Tuple<float, float>(10f, 0f);
			Tuple<float, float> test1TupleB = new Tuple<float, float>(15f, 25f);
			bool overlaps = test1TupleA.Overlaps(test1TupleB);

			Assert.IsFalse(overlaps);

			// Test 2, same as 1, just the b range is reversed
			Tuple<float, float> test2TupleA = new Tuple<float, float>(30f, 45f);
			Tuple<float, float> test2TupleB = new Tuple<float, float>(20f, 1f);
			overlaps = test2TupleA.Overlaps(test2TupleB);

			Assert.IsFalse(overlaps);

			// Test 3, same except both are reversed.
			Tuple<float, float> test3TupleA = new Tuple<float, float>(10f, 0f);
			Tuple<float, float> test3TupleB = new Tuple<float, float>(25f, 15f);
			overlaps = test1TupleA.Overlaps(test1TupleB);

			Assert.IsFalse(overlaps);
		}

		/// <summary>
		/// Testing if one or both tuples have item2 as the low number and they should overlap.
		/// </summary>
		[Test]
		public void FlippedTuplesOverlapTest()
		{
			// Test1, a' range is reversed, should return true.
			Tuple<float, float> test1TupleA = new Tuple<float, float>(20f, 0f);
			Tuple<float, float> test1TupleB = new Tuple<float, float>(15f, 25f);
			bool overlaps = test1TupleA.Overlaps(test1TupleB);

			Assert.IsTrue(overlaps);

			// Test 2, same as 1, just the b range is reversed
			Tuple<float, float> test2TupleA = new Tuple<float, float>(15f, 45f);
			Tuple<float, float> test2TupleB = new Tuple<float, float>(20f, 1f);
			overlaps = test2TupleA.Overlaps(test2TupleB);

			Assert.IsTrue(overlaps);

			// Test 3, same except both are reversed.
			Tuple<float, float> test3TupleA = new Tuple<float, float>(12f, 0f);
			Tuple<float, float> test3TupleB = new Tuple<float, float>(25f, 10f);
			overlaps = test1TupleA.Overlaps(test1TupleB);

			Assert.IsTrue(overlaps);
		}

		/// <summary>
		/// Testing if one or both have item2 as the low number and one is contained within the other.
		/// </summary>
		[Test]
		public void FlippedTuplesContainTest()
		{
			// Test1, a' range is reversed, should return true.
			Tuple<float, float> test1TupleA = new Tuple<float, float>(20f, 0f);
			Tuple<float, float> test1TupleB = new Tuple<float, float>(5f, 15f);
			bool overlaps = test1TupleA.Overlaps(test1TupleB);

			Assert.IsTrue(overlaps);

			// Test 2, same as 1, just the b range is reversed
			Tuple<float, float> test2TupleA = new Tuple<float, float>(15f, 45f);
			Tuple<float, float> test2TupleB = new Tuple<float, float>(25f, 20f);
			overlaps = test2TupleA.Overlaps(test2TupleB);

			Assert.IsTrue(overlaps);

			// Test 3, same except both are reversed.
			Tuple<float, float> test3TupleA = new Tuple<float, float>(30f, 0f);
			Tuple<float, float> test3TupleB = new Tuple<float, float>(25f, 10f);
			overlaps = test1TupleA.Overlaps(test1TupleB);

			Assert.IsTrue(overlaps);
		}

		/// <summary>
		/// Tests where one of the ranges' min and max are the same.
		/// </summary>
		[Test]
		public void ZeroRangeTest()
		{
			// Test 1, one is range zero and outside the other, should return false.
			Tuple<float, float> test1TupleA = new Tuple<float, float>(0f, 15f);
			Tuple<float, float> test1TupleB = new Tuple<float, float>(20f, 20f);
			bool overlaps = test1TupleA.Overlaps(test1TupleB);

			Assert.IsFalse(overlaps);

			// Test 2, one is range zero and inside the other, should return true.
			Tuple<float, float> test2TupleA = new Tuple<float, float>(0f, 15f);
			Tuple<float, float> test2TupleB = new Tuple<float, float>(10f, 10f);
			overlaps = test2TupleA.Overlaps(test2TupleB);

			Assert.IsTrue(overlaps);

			// Test 3, one is range zero and right on the edge of the other, should return true.
			Tuple<float, float> test3TupleA = new Tuple<float, float>(0f, 15f);
			Tuple<float, float> test3TupleB = new Tuple<float, float>(15f, 15f);
			overlaps = test3TupleA.Overlaps(test3TupleB);

			Assert.IsTrue(overlaps);
		}

		/// <summary>
		/// Tests cases where the overlap uses a tolerance value.
		/// </summary>
		[Test]
		public void ToleranceTest()
		{
			// Test 1, the ranges do not overlap and the tolerance is not enough to cause overlap. Should return false.
			Tuple<float, float> test1TupleA = new Tuple<float, float>(0f, 15f);
			Tuple<float, float> test1TupleB = new Tuple<float, float>(20f, 25f);
			bool overlaps = test1TupleA.Overlaps(test1TupleB, 2f);

			Assert.IsFalse(overlaps);

			// Test 2, the ranges do not overlap and the tolerance is enough to cause overlap. Should return true.
			Tuple<float, float> test2TupleA = new Tuple<float, float>(0f, 15f);
			Tuple<float, float> test2TupleB = new Tuple<float, float>(20f, 25f);
			overlaps = test2TupleA.Overlaps(test2TupleB, 5f);

			Assert.IsTrue(overlaps);

			// Test 3, the ranges do overlap and uses a tolerance value, should still return true.
			Tuple<float, float> test3TupleA = new Tuple<float, float>(0f, 15f);
			Tuple<float, float> test3TupleB = new Tuple<float, float>(10f, 12f);
			overlaps = test3TupleA.Overlaps(test3TupleB, 5f);

			Assert.IsTrue(overlaps);

			// Test 4,  ranges overlap and uses a sufficient negative tolerance, should return false.
			Tuple<float, float> test4TupleA = new Tuple<float, float>(0f, 15f);
			Tuple<float, float> test4TupleB = new Tuple<float, float>(12f, 20f);
			overlaps = test4TupleA.Overlaps(test4TupleB, -2f);

			Assert.IsFalse(overlaps);
		}

		/// <summary>
		/// Test the tuple.With extension.
		/// </summary>
		[Test]
		public void TupleWithTest()
		{
			// Test1, checks if using with overwrites the first value.
			Tuple<float, float> testing = new Tuple<float, float>(5f, 10f);
			Tuple<float, float> expected = new Tuple<float, float>(0f, 10f);
			testing = testing.With(0f);
			Assert.IsTrue(testing.Item1 == expected.Item1 && testing.Item2 == expected.Item2);

			// Test2, checks if using with overwrites the second value.
			Tuple<float, float> testing2 = new Tuple<float, float>(5f, 10f);
			Tuple<float, float> expected2 = new Tuple<float, float>(5f, 15f);
			testing2 = testing2.With(nItem2: 15f);
			Assert.IsTrue(testing2.Item1 == expected2.Item1 && testing2.Item2 == expected2.Item2);

			// Test3, checks if using with overwrites both values.
			Tuple<float, float> testing3 = new Tuple<float, float>(5f, 10f);
			Tuple<float, float> expected3 = new Tuple<float, float>(0f, 15f);
			testing3 = testing3.With(0f, 15f);
			Assert.IsTrue(testing3.Item1 == expected3.Item1 && testing3.Item2 == expected3.Item2);
		}
	}
}
