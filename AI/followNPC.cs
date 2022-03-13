using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class followNPC : MonoBehaviour
{
    private GameObject owner;
    private void Update()
    {
        if (owner == null) Destroy(gameObject);
        else transform.position = new Vector3(owner.transform.position.x, owner.transform.position.y + 0.6f, owner.transform.position.z);
    }

    private void Start()
    {
        owner = gameObject.transform.parent.gameObject;
        gameObject.transform.parent = null;
    }
}
