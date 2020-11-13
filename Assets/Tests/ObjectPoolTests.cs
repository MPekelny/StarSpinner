using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tests.ManagerTests
{
	public class ObjectPoolTests
	{
		/// <summary>
		/// A test that checks an object gets obtained from and returned to a pool correctly.
		/// </summary>
		[UnityEngine.TestTools.UnityTest]
		public System.Collections.IEnumerator TestObjectPooling()
		{
			GameObject gameObject = (GameObject)GameObject.Instantiate(AssetDatabase.LoadAssetAtPath("Assets/Tests/TestScenes/TestObjectPooler.prefab", typeof(GameObject)));
			ObjectPoolManager poolManager = gameObject.GetComponent<ObjectPoolManager>();

			yield return new WaitForEndOfFrame();

			Assert.IsTrue(poolManager.GetCountInPool("TestStar") == 0, "Test pool is not initially empty."); // The pool in this case should start out empty.

			PoolableObject objectFromPool = poolManager.GetObjectFromPool("TestStar", null);
			Assert.IsTrue(poolManager.GetCountInPool("TestStar") == 0, "Test pool is not empty after object pulled from pool."); // Because the new object is pulled out straight away, the pool should still be empty.
			Assert.IsNotNull(objectFromPool, "Did not get an object from the pool.");
			Assert.IsNull(objectFromPool.transform.parent, "Object pulled from pool has a parent when it shouldn't for the test."); // Because the transform argument for getting the object from the pool was null, the pooled object should not have a parent.

			objectFromPool.ReturnToPool();
			Assert.IsTrue(poolManager.GetCountInPool("TestStar") == 1, "TestPool does not have 1 object in it after returning test object to pool"); // After returning the object, there should be exactly 1 object in the pool.
			Assert.IsTrue(objectFromPool != null, "The test pooled object was destroyed after returning it to the pool."); // After returning the object to the pool, it should still exist.
			Assert.IsNotNull(objectFromPool.transform.parent, "Object did not have a parent after returning it to the pool."); // After returning the object to the pool, its parent should be the pool. But, since we can't get the actual parent, just check if it is not null now.
		}
	}
}

