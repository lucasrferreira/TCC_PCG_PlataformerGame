using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// This class allows you to have a pool of various objects to pool from.
	/// </summary>
	public class MultipleObjectPooler : ObjectPooler
	{
	    /// the list of game objects we'll instantiate 
		public GameObject[] GameObjectsToPool;	
		/// For each type of object, the number we need to pool
		public int[] PoolSize;
	    /// if true, the pool will automatically add objects to the itself if needed
		public bool PoolCanExpand = true;

	    /// this object is just used to group the pooled objects
	    protected GameObject _waitingPool;
	    /// the actual object pool
		protected List<GameObject> _pooledGameObjects;
	    
	    /// <summary>
	    /// Fills the object pool with the amount of objects you specified in the inspector.
	    /// </summary>
	    protected override void FillObjectPool()
	    {
	        // we create a container that will hold all the instances we create
	        _waitingPool = new GameObject("[MultipleObjectPooler] " + this.name);
	        // we initialize the pool
			_pooledGameObjects = new List<GameObject>();

	        int i = 0;
	        // for each type of object specified in the inspector
			foreach (GameObject pooledGameObject in GameObjectsToPool)
	        {
	            // if there's no specified number of objects to pool for that type of object, we do nothing and exit
				if (i > PoolSize.Length) { return; }

				// we add, one by one, the number of objects of that type, as specified in the inspector
	            for (int j = 0; j < PoolSize[i]; j++)
	            {
					AddOneObjectToThePool(pooledGameObject);
				}
				i++;
	        }
	    }
	    
	    /// <summary>
	    /// Adds one object of the specified type to the object pool.
	    /// </summary>
	    /// <returns>The object that just got added.</returns>
	    /// <param name="typeOfObject">The type of object to add to the pool.</param>
		protected virtual GameObject AddOneObjectToThePool(GameObject typeOfObject)
		{
			GameObject newGameObject = (GameObject)Instantiate(typeOfObject);
			newGameObject.gameObject.SetActive(false);
			newGameObject.transform.parent = _waitingPool.transform;
			newGameObject.name=typeOfObject.name+"-"+_pooledGameObjects.Count;
			_pooledGameObjects.Add(newGameObject);	
			return newGameObject;
		}
		
		/// <summary>
		/// Gets a random object from the pool.
		/// </summary>
		/// <returns>The pooled game object.</returns>
		public override GameObject GetPooledGameObject()
		{
			// we get a random index 
			int randomIndex = Random.Range(0, _pooledGameObjects.Count);
			// this counter is used to avoid an infinite loop while searching
			int preventOverflowCounter=0;
					
			// as long as we don't find an inactive object we can pool, we keep randomizing new indexes. 		
			// we only do that as many times as there are items in the pool, to avoid infinite loops.
			while ( (_pooledGameObjects[randomIndex].gameObject.activeInHierarchy) && (preventOverflowCounter<_pooledGameObjects.Count) )
			{
				randomIndex = Random.Range(0, _pooledGameObjects.Count);
				preventOverflowCounter++;
			}
			
			// once we're done looping randomly through the pool, if the item we've picked is inactive, that means the pool is empty.
			if (_pooledGameObjects[randomIndex].gameObject.activeInHierarchy)
			{	
				// if the pool is allowed to grow (this is set in the inspector if you're wondering)
				if (PoolCanExpand)
				{
					// we get a random type of object from the ones already in the pool and add it to the pool
					randomIndex = Random.Range(0, GameObjectsToPool.Length);
					return AddOneObjectToThePool(GameObjectsToPool[randomIndex]);						 	
				}
				else
				{
					// if it's not allowed to grow, we return nothing.
					return null;
				}
			}
			else
			{			
				// if the pool wasn't empty, we return the random object we've found.
				return _pooledGameObjects[randomIndex];   
			}
			
			 
		}
		
		/// <summary>
		/// Gets an object of the specified type from the pool
		/// </summary>
		/// <returns>The pooled game object of type.</returns>
		/// <param name="type">Type.</param>
		public virtual GameObject GetPooledGameObjectOfType(string type)
	    {
			GameObject correspondingGameObject=null;

			// we go through the object pool looking for an inactive object of the specified type.
			for (int i = 0; i < _pooledGameObjects.Count; i++)
	        {
	        	// if we find an object inside the pool that matches the asked type
				if (_pooledGameObjects[i].name == type)
	            {
	            	// and if that object is inactive right now
					if (!_pooledGameObjects[i].gameObject.activeInHierarchy)
	                {
	                	// we return it
						return _pooledGameObjects[i];
					}
					else
					{
						// if the object is active, we store its type
						correspondingGameObject = _pooledGameObjects[i];
					}
	            }            
	        }

			// if we've not returned the object, that means the pool is empty (at least it means it doesn't contain any object of that specific type)
			// so if the pool is allowed to expand
	        if (PoolCanExpand && correspondingGameObject!=null)
	        {
	        	// we create a new game object of that type, we add it to the pool for further use, and return it.
				GameObject newGameObject = (GameObject)Instantiate(correspondingGameObject);
	            _pooledGameObjects.Add(newGameObject);
	            return newGameObject;
	        }

			// if the pool was empty for that object and not allowed to expand, we return nothing.
	        return null;
	    }
	}
}