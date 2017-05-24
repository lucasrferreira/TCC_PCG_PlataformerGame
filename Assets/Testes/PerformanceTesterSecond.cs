using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceTesterSecond : MonoBehaviour
{

    //public GameObject prefab;

    public List<GameObject> prefabs;

    public int countOfObjects;
	// Use this for initialization
	void Start ()
	{
	    StartCoroutine(SpawGameObjects());
	}

    IEnumerator SpawGameObjects()
    {
        for (int index = 0; index < countOfObjects; index++)
        {
            Instantiate(prefabs[Random.Range(0, prefabs.Count)], transform, false);
            yield return null;
        }
        yield return null;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
