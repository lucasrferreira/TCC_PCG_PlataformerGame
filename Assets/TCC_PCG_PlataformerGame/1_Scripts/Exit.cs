using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour {

    void OnCollisionEnter2D(Collision2D coll)
    {
        Debug.Log("collide "+ coll.gameObject.tag);
        if (coll.gameObject.tag == "Enemy")
            coll.gameObject.SendMessage("ApplyDamage", 10);

    }
}
