using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum elevatorState
{
    open,
    closed
}

public class Elevator : MonoBehaviour
{
    private Transform doorLeft;
    private Transform doorRight;
    [SerializeField] private float doorTimer;
    [SerializeField] private elevatorState state = elevatorState.closed;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;

    private Vector3 targetPosLeft;
    private Vector3 targetPosRight;
    private Vector3 originalPosLeft;
    private Vector3 originalPosRight;
    private Vector3 currentPosLeft;
    private Vector3 currentPosRight;
    private bool moved = false;
    
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.name == "deur_links")
            {
                doorLeft = child.transform;
            }
            else if (child.name == "deur_rechts")
            {
                doorRight = child.transform;
            }
        }

        originalPosLeft = doorLeft.transform.localPosition;
        originalPosRight = doorRight.transform.localPosition;

        targetPosLeft = new Vector3(originalPosLeft.x + 1f, originalPosLeft.y, originalPosLeft.z);
        targetPosRight = new Vector3(originalPosRight.x - 1f, originalPosRight.y, originalPosRight.z);

        if (state == elevatorState.closed)
        {
            currentPosLeft = originalPosLeft;
            currentPosRight = originalPosRight;
            doorLeft.transform.localPosition = currentPosLeft;
            doorRight.transform.localPosition = currentPosRight;
        }

        else
        {
            currentPosLeft = targetPosLeft;
            currentPosRight = targetPosRight;
            doorLeft.transform.localPosition = currentPosLeft;
            doorRight.transform.localPosition = currentPosRight;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            doorTimer = 2.25f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (doorTimer > 0f)
        {
            doorTimer -= Time.deltaTime;
        }

        else if (doorTimer < 0f)
        {
            doorTimer = 0f;
            OpenCloseDoors();
            moved = true;
        }

        if (state == elevatorState.open)
        {
            doorLeft.transform.localPosition = Vector3.Lerp(currentPosLeft, targetPosLeft, Time.deltaTime);
            doorRight.transform.localPosition = Vector3.Lerp(currentPosRight, targetPosRight, Time.deltaTime);
            currentPosLeft = doorLeft.transform.localPosition;
            currentPosRight = doorRight.transform.localPosition;
        }

        else
        {
            doorLeft.transform.localPosition = Vector3.Lerp(currentPosLeft, originalPosLeft, Time.deltaTime);
            doorRight.transform.localPosition = Vector3.Lerp(currentPosRight, originalPosRight, Time.deltaTime);
            currentPosLeft = doorLeft.transform.localPosition;
            currentPosRight = doorRight.transform.localPosition;
        }
    }

    private void OpenCloseDoors()
    {
        if (!moved)
        {
            if (state == elevatorState.closed)
            {
                state = elevatorState.open;
                audioSource.clip = audioClips[0];
                audioSource.Play();
            }
            else
            {
                state = elevatorState.closed;
                audioSource.clip = audioClips[1];
                audioSource.Play();
            }
        }
    }
}
