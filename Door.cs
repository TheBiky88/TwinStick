using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorState
{
    Closed = 0,
    OpenFront,
    OpenBack,
    Locked,
    idle
}

public enum DoorColour
{
    NA = 0,
    Red,
    Blue,
    Yellow,
    Green
}

public class Door : MonoBehaviour
{
    private GameObject m_playerObject;

    [SerializeField] private DoorState DoorState;
    [SerializeField] private DoorColour DoorColour;
    [SerializeField] private KeyCode InteractKey;
    [SerializeField] private GameObject[] colourFrame;
    private Transform pivot;
    private float rotationSpeed = 4f;
    private Vector3 currentRotationV3;
    private Vector3 originalRotationV3;
    private Vector3 BackRotationV3;
    private Vector3 FrontRotationV3;
    private bool isBack;
    private float closeDoorTimer = 0f;
    private float closeDoorTargetTimer = 1.2f;
    private bool wasLocked;
    private bool isMoving;
    private float moveTimer = 0f;
    private float moveTargetTimer = 1f;

    private DoorAudioManager doorAudio;

    private bool moving = false;
    private bool playedSound = false;

    private void Start()
    {
        doorAudio = GetComponentInChildren<DoorAudioManager>();

        m_playerObject = GameObject.FindGameObjectWithTag("Player");
        originalRotationV3 = transform.eulerAngles;
        BackRotationV3 = new Vector3(originalRotationV3.x, originalRotationV3.y - 90, originalRotationV3.z);
        FrontRotationV3 = new Vector3(originalRotationV3.x, originalRotationV3.y + 90, originalRotationV3.z);
        currentRotationV3 = originalRotationV3;
        InteractKey = KeyCode.E;
        transform.eulerAngles = currentRotationV3;
        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject child = transform.GetChild(i).gameObject;

            if (child.name == "Pivot")
            {
                pivot = child.transform;
            }
        }
    }

    private void Update()
    {
        if (closeDoorTimer != 0)
        {
            closeDoorTimer -= Time.deltaTime;
            if (closeDoorTimer < 0)
            {
                closeDoorTimer = 0;

                if (wasLocked)
                {
                    DoorState = DoorState.Locked;
                }
                else
                {
                    DoorState = DoorState.Closed;
                }
            }
        }

        if (moveTimer != 0)
        {
            moveTimer -= Time.deltaTime;
            if (moveTimer < 0)
            {
                moveTimer = 0;
                isMoving = false;
            }
        }


        if (DoorState == DoorState.OpenFront)
        {
            {
                currentRotationV3 = Vector3.Lerp(currentRotationV3, FrontRotationV3, Time.deltaTime * rotationSpeed);
                pivot.eulerAngles = currentRotationV3;
            }
        }

        else if (DoorState == DoorState.OpenBack)
        {
            {
                currentRotationV3 = Vector3.Lerp(currentRotationV3, BackRotationV3, Time.deltaTime * rotationSpeed);
                pivot.eulerAngles = currentRotationV3;
            }
        }

        else if (DoorState == DoorState.Closed || DoorState == DoorState.Locked)
        {
            currentRotationV3 = Vector3.Lerp(currentRotationV3, originalRotationV3, Time.deltaTime * rotationSpeed);
            pivot.eulerAngles = currentRotationV3;
        }
    }

    public void Interact()
    {
        if (!isMoving)
        {
            if (Input.GetKeyDown(InteractKey))
            {
                if (DoorState == DoorState.Locked)
                {
                    doorAudio.PlaySoundWrong();
                }

                else
                {
                    CloseOpenDoor();
                    isMoving = true;
                    moveTimer = moveTargetTimer;
                }
            }
        }
    }

    public void CloseOpenDoor()
    {
        if (DoorState == DoorState.Closed)
        {
            doorAudio.PlaySoundOpen();

            if (isBack)
            {
                DoorState = DoorState.OpenFront;
            }
            else
            {
                DoorState = DoorState.OpenBack;
            }
        }
        else if (DoorState == DoorState.OpenFront || DoorState == DoorState.OpenBack)
        {
            DoorState = DoorState.Closed;

            doorAudio.Invoke("PlaySoundClose", 0.8f);
        }
    }

    public void AIOpenDoor()
    {
        if (DoorState == DoorState.Locked)
        {
            wasLocked = true;
        }

        if (DoorState == DoorState.Closed || DoorState == DoorState.Locked)
        {
            doorAudio.PlaySoundOpen();

            if (isBack)
            {
                DoorState = DoorState.OpenFront;
            }
            else
            {
                DoorState = DoorState.OpenBack;
            }
        }
    }

    public void AICloseDoor()
    {
        if (DoorState == DoorState.OpenFront || DoorState == DoorState.OpenBack)
        {
            if (wasLocked)
            {
                DoorState = DoorState.Locked;
                wasLocked = false;
            }

            else
            {
                DoorState = DoorState.Closed;
            }

            doorAudio.Invoke("PlaySoundClose", 0.8f);
        }
    }



    public DoorColour GetDoorColour()
    {
        return DoorColour;
    }
    public void IsBack(bool boolean)
    {
        isBack = boolean;
    }

    public void DisableTrap()
    {
        if (DoorState == DoorState.Locked)
        {
            DoorState = DoorState.Closed;
            foreach (GameObject frame in colourFrame)
            {
                frame.GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }
    }
}