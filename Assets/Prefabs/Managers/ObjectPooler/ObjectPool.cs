using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
	private Queue<PoolableObject> _pool = new Queue<PoolableObject>();
	private PoolableObject _objectPrefab = null;

	public void Init(PoolableObject prefab, int initialCount)
	{
		if (prefab == null)
		{
			throw new ArgumentException("Tried to create a pool with a null object.");
		}

		_objectPrefab = prefab;

		if (initialCount > 0)
		{
			CreateInitialPool(initialCount);
		}
	}

	/// <summary>
	/// Fetches an object from this pool. If there is an object available in the pool, it just returns one. Otherwise creates a new one and returns that.
	/// </summary>
	/// <param name="parentNew">The transform of the object that is to be the parent of the pooled object.</param>
	/// <returns></returns>
	public PoolableObject GetObjectFromPool(Transform parentNew)
	{
		if (_objectPrefab == null)
		{
			Debug.LogError($"Attempted to get an object from a pool with no prefab: {name}");
			return null;
		}

		PoolableObject item = null;
		if (_pool.Count == 0)
		{
			item = CreateObject();
		}
		else
		{
			item = _pool.Dequeue();
		}

		item.gameObject.SetActive(true);
		item.transform.SetParent(parentNew);

		return item;
	}

	/// <summary>
	/// This method gets passed to a poolable object so that it can be called by that object without having to make this method public. Makes it rather less likely that other code will try to return an object to the wrong pool.
	/// </summary>
	private void ReturnObjectToPool(PoolableObject obj)
	{
		if (obj == null) return;

		// Do not add an object to the pool if the object is not owned by this pool.
		// In the event we try to return an object this pool does not own, return it to the actual pool if it has an owner, or destroy it (so it is not orphaned) if it has no owner.
		if (obj.Owner == null)
		{
			Destroy(obj.gameObject);
		}
		else if (obj.Owner != this)
		{
			obj.Owner.ReturnObjectToPool(obj);
		}
		else
		{
			PutObjectIntoPool(obj);
		}
	}

	private void CreateInitialPool(int count)
	{
		for (int i = 0; i < count; i++)
		{
			PoolableObject obj = CreateObject();
			PutObjectIntoPool(obj);
		}
	}

	private PoolableObject CreateObject()
	{
		PoolableObject obj = Instantiate(_objectPrefab);
		obj.SetOwner(this, ReturnObjectToPool);

		return obj;
	}

	private void PutObjectIntoPool(PoolableObject obj)
	{
		obj.transform.SetParent(transform);
		obj.gameObject.SetActive(false);

		// If for some reason the object is already in the queue, don't enqueue it again.
		if (!_pool.Contains(obj))
		{
			_pool.Enqueue(obj);
		}
	}
}
