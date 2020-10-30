using NUnit.Framework;

public class HelperMethodTests
{
	[Test]
	public void TestEpsilonCheckReturnsTrue()
	{
		float testValueA = 15f;
		float testValueB = 15.000001f;

		Assert.IsTrue(HelperMethods.EpsilonCheck(testValueA, testValueB));
	}

	[Test]
	public void TestEpsilonCheckReturnsFalse()
	{
		float testValueA = 15f;
		float testValueB = 15.01f;

		Assert.IsFalse(HelperMethods.EpsilonCheck(testValueA, testValueB));
	}
}
