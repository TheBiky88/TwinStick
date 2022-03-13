using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIPatrolPoint : MonoBehaviour
{
    [SerializeField] protected float waitTime = 0f;
    protected GameObject owner;

    public float m_waitTime { get { return waitTime; } private set { waitTime = value; } }

    protected virtual void Update()
    {
        if (owner == null)
        {
            Destroy(gameObject);
        }
    }

    public void SetOwner(GameObject gameObject)
    {
        owner = gameObject;
    }
}