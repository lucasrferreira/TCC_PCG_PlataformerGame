using UnityEngine;

public class Exit : MonoBehaviour
{

    public InfinitePhasesLvlManager InfinitePhasesLvlManager;

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("Player"))
        {
            //InfinitePhasesLvlManager.LvlFinish();
            Debug.Log("FINISH");
        }
        
    }
}
