using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorCollider : MonoBehaviour
{
    [SerializeField] private bool isBack;
    [SerializeField] private DoorCollider otherCollider;
    
    private Door door;
    private bool triggered;
    private bool AiTrigger;


    private void Start()
    {
        door = gameObject.transform.parent.GetComponentInParent<Door>();
    }

    private void Update()
    {
        if (triggered)
        {
            door.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Item" && other.tag != "Obstacle")
        {
            door.IsBack(isBack);

            if (other.tag == "Enemy")
            {
                door.AIOpenDoor();
                AiTrigger = true;
            }

            else if (other.tag == "Player")
            {
                triggered = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Enemy")
        {
            if (!otherCollider.AITriggered())
            {
                door.AICloseDoor();
            }

            AiTrigger = false;
        }

        else if (other.tag == "Player")
        {
            triggered = false;
        }
    }

    public bool AITriggered()
    {
        return AiTrigger;
    }
}
