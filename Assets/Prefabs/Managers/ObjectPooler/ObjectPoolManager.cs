using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
	[Serializable]
	public class PoolData
	{
		[SerializeField] private string _poolName = "";
		[SerializeField] private PoolableObject _prefabForPool = null;
		[SerializeField] private int _initialCount = 1;

		public string PoolName => _poolName;
		public PoolableObject PrefabForPool => _prefabForPool;
		public int InitialCount => _initialCount;
	}

	[SerializeField] PoolData[] _poolDatas = null;

	private Dictionary<string, ObjectPool> _objectPools = new Dictionary<string, ObjectPool>();

	public void Start()
	{
		CreatePools();
	}

	public PoolableObject GetObjectFromPool(string poolName, Transform parentNew)
	{
		if (string.IsNullOrEmpty(poolName))
		{
			Debug.LogError($"Attempted to get an object from a pool using a blank poolName.");
			return null;
		}
		else if (!_objectPools.ContainsKey(poolName))
		{
			Debug.LogError($"Attempted to get an object from pool {poolName}, but a pool of that name does not exist.");
			return null;
		}

		PoolableObject obj = _objectPools[poolName].GetObjectFromPool(parentNew);
		return obj;
	}

	private void CreatePools()
	{
		foreach (PoolData p in _poolDatas)
		{
			if (string.IsNullOrEmpty(p.PoolName))
			{
				Debug.LogError("Attempted to make an ObjectPool with a blank name.");
				continue;
			}
			else if (p.PrefabForPool == null)
			{
				Debug.LogError($"Attempted to create an Object pool with name {p.PoolName} with no prefab set.");
				continue;
			}

			GameObject obj = new GameObject($"{p.PoolName} Pool");
			obj.transform.SetParent(transform);
			ObjectPool pool = obj.AddComponent<ObjectPool>();
			pool.Init(p.PrefabForPool, p.InitialCount);
			_objectPools.Add(p.PoolName, pool);
		}
	}
}
