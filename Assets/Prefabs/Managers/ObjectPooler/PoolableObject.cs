using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PoolableObject : MonoBehaviour
{
	private ObjectPool _owner = null;
	public ObjectPool Owner => _owner;

	/// <summary>
	/// Sets the owner of this poolableObject. This is meant to be only called by ObjectPool, but can't restrict what classes call a method.
	/// So, having this in a specific Set method instead of just the setter so it is more explicit that it is being called on purpose instead of something like using the setter by accident.
	/// </summary>
	/// <param name="ownerNew"></param>
	public void SetOwner(ObjectPool ownerNew)
	{
		if (ownerNew != null)
		{
			_owner = ownerNew;
		}
		else
		{
			Debug.LogWarning("Attempted to set a PoolableObject's owner to null.");
		}
	}

	public void ReturnToPool()
	{
		// In the case that the owner is not set, this object would not get put back into a pool and the code calling this would probably not handle that, so this object would be orphaned.
		// So if this is called without an owner being set, just destroy it.
		if (_owner == null)
		{
			Debug.LogWarning($"Tried to return a PoolableObject {name} to its pool without an owner being set.");
			Destroy(gameObject);
		}
		else 
		{
			_owner.ReturnObjectToPool(this);
		}
	}
}
