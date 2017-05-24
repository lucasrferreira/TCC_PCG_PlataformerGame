using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerformanceTester : MonoBehaviour
{

    public GameObject prefab;

    public List<Sprite> sprites;

    public int countOfObjects;
	// Use this for initialization
	void Start ()
	{
	    StartCoroutine(SpawGameObjects());
	}

    IEnumerator SpawGameObjects()
    {
        var instatiated = prefab;
        for (int index = 0; index < countOfObjects; index++)
        {
            instatiated = Instantiate(prefab, transform, false);
            instatiated.GetComponent<SpriteRenderer>().sprite = sprites[Random.Range(0, sprites.Count)];
            yield return null;
        }
        yield return null;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
