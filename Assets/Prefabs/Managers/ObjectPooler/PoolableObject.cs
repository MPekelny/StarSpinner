using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PoolableObject : MonoBehaviour
{
	private ObjectPool _owner = null;
	public ObjectPool Owner => _owner;

	Action<PoolableObject> _poolReturnCb = null;

	/// <summary>
	/// Sets the owner of this poolable object as well as the method for returning this object to the pool (this way, only this object can call the return method directly).
	/// This method should only be called by ObjectPool.
	/// </summary>
	/// <param name="ownerRef">The reference to the ObjectPool that owns this object, so that when it is returned to its pool, the pool can double check that it actually owns this object.</param>
	/// <param name="poolReturnCb">The method to be called when this object is to be returned to the object pool.</param>
	public void SetOwner(ObjectPool ownerRef, Action<PoolableObject> poolReturnCb)
	{
		if (ownerRef == null || poolReturnCb == null)
		{
			throw new ArgumentException($"Called SetOwner on a {name} object with a null ownerRef or poolReturnCb.");
		}

		_owner = ownerRef;
		_poolReturnCb = poolReturnCb;
	}

	public void ReturnToPool()
	{
		// In the case that the owner is not set, this object would not get put back into a pool and the code calling this would probably not handle that, so this object would be orphaned.
		// So if this is called without an owner being set, just destroy it.
		if (_poolReturnCb == null)
		{
			Debug.LogWarning($"Tried to return a PoolableObject {name} to its pool without a return to pool callback being set. Destroying object so it does not hang around.");
			Destroy(gameObject);
		}
		else 
		{
			ReturnToPoolCleanup();
			_poolReturnCb(this);
		}
	}

	public virtual void ReturnToPoolCleanup() { }
}
