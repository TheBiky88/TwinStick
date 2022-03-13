using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public enum NPCState
{
    idle = 0,
    patroling,
    attacking,
    disabled,
    search
}
[RequireComponent(typeof(NavMeshAgent))]

public class NPC_AI : MonoBehaviour
{
    [SerializeField] private List<GameObject> m_PatrolPoints = new List<GameObject>();
    [SerializeField] private NPCState state = NPCState.idle;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private bool infiniteMagazines = false;
    [SerializeField] private bool infiniteAmmo = false;
    [SerializeField] private int scoreOnDeath = 1;
    [SerializeField] private string enemyName;
    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] public bool dropsGunOnDeath = true;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip audioClipSearch;
    [SerializeField] private AudioClip[] audioClipAlert;


    public GameObject m_equippedGun { private set; get; }
    private WeaponController m_equippedGunController;

    private float m_WaitTimer;
    private float m_SearchTimer;
    private float talkTimer = 0f;
    private GameObject m_NextPatrolPoint;
    private int m_Index = 0;
    private Transform m_Target;
    private StatManager m_statManager;
    private DropsOnDeath dropSystem;

    private void Start()
    {
        List<GameObject> childrenToRemove = new List<GameObject>();

        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GameObject child = gameObject.transform.GetChild(i).gameObject;

            if (child.name.Contains("Patrol"))
            {
                childrenToRemove.Add(child);
            }
        }

        foreach (GameObject child in childrenToRemove)
        {
            m_PatrolPoints.Add(child);
            child.GetComponent<AIPatrolPoint>().SetOwner(gameObject);
            child.SetActive(true);
            child.transform.parent = null;
        }

        m_NextPatrolPoint = m_PatrolPoints[m_Index];
        m_equippedGunController = gameObject.GetComponentInChildren<WeaponController>();
        m_equippedGun = m_equippedGunController.gameObject;

        Rigidbody gunRigBody = m_equippedGun.GetComponent<Rigidbody>();
        gunRigBody.isKinematic = true;
        gunRigBody.detectCollisions = false;

        m_equippedGunController.m_gunStatus = GunStatusEnum.equipped;

        m_statManager = FindObjectOfType<StatManager>();

        dropSystem = GetComponent<DropsOnDeath>();

        agent = gameObject.GetComponent<NavMeshAgent>();
    }


    public void AddPatrolPoint(GameObject patrolpoint)
    {
        m_PatrolPoints.Add(patrolpoint);
        patrolpoint.GetComponent<AIPatrolPoint>().SetOwner(gameObject);
    }

    public void OnDeath()
    {
        dropSystem.OnDeath();
        m_statManager.AddScore(scoreOnDeath);
    }

    void Update()
    {
        if (m_WaitTimer > 0)
        {
            m_WaitTimer -= Time.deltaTime;
            if (m_WaitTimer < 0)
            {
                m_WaitTimer = 0;
            }
        }

        if (m_SearchTimer > 0)
        {
            m_SearchTimer -= Time.deltaTime;
            if (m_SearchTimer < 0)
            {
                m_SearchTimer = 0;
            }
        }

        if (talkTimer > 0)
        {
            talkTimer -= Time.deltaTime;
            if (talkTimer < 0)
            {
                talkTimer = 0;
            }
        }

        if (state == NPCState.idle)
        {
            agent.isStopped = true;
            agent.ResetPath();
            if (m_WaitTimer == 0)
            {
                state = NPCState.patroling;
            }
        }

        else if (state == NPCState.patroling)
        {
            Patrol();
        }

        else if (state == NPCState.attacking)
        {
            if (m_WaitTimer != 0)
            {
                Attack();
            }
            else
            {
                m_WaitTimer = 20f;
                Search();
                m_SearchTimer = 2f;
            }
        }

        else if (state == NPCState.search)
        {
            if (m_SearchTimer == 0)
            {
                Search();
                m_SearchTimer = 0.75f;
            }

            if (m_WaitTimer == 0)
            {
                state = NPCState.patroling;
                m_Target = null;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == m_NextPatrolPoint)
        {

            m_WaitTimer = other.GetComponent<AIPatrolPoint>().m_waitTime;
            state = NPCState.idle;
            

            m_Index++;
            if (m_Index == m_PatrolPoints.Count) m_Index = 0;
            m_NextPatrolPoint = m_PatrolPoints[m_Index];
        }
    }

    private void Patrol()
    {
        agent.SetDestination(m_NextPatrolPoint.transform.position);
    }

    private void Attack()
    {
        Vector3 dir = (m_Target.position - transform.position).normalized;
        float dot = Vector3.Dot(dir, transform.forward);
        float step = rotationSpeed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, dir, step, 0.0f);

        if (dot < 0.9999)
        {
            transform.rotation = Quaternion.LookRotation(newDirection);
        }
        
        

        if (m_equippedGunController.m_bulletsInMag == 0 && !m_equippedGunController.m_reloading)
        {
            m_equippedGunController.Reload(infiniteMagazines);
        }

        if (dot > 0.9)
        {
            if (m_equippedGunController.AllowedToShoot())
            {
                m_equippedGunController.Shoot(gameObject, infiniteAmmo);
            }
        }
    }

    public void Alert(Transform target)
    {       
        state = NPCState.attacking;
        m_Target = target;
        m_WaitTimer = 0.5f;
        agent.isStopped = true;
        agent.ResetPath();

        if (talkTimer == 0f)
        {
            int index = Random.Range(0, audioClipAlert.Length);
            audioSource.clip = audioClipAlert[index];
            audioSource.Play();
            talkTimer = 4f;
        }
    }    

    public void wasDamaged(GameObject hitBy)
    {
        if (state != NPCState.attacking && state != NPCState.search)
        {
            state = NPCState.search;
            m_WaitTimer = 10f;
            m_Target = hitBy.transform;
            agent.SetDestination(m_Target.position);
        }
    }

    public void Search(Transform target = null, float waitTime = 0)
    {
        if (target != null)
        {
            m_Target = target;
        }

        if (waitTime > 0)
        {
            m_WaitTimer = waitTime;
        }
        state = NPCState.search;
        agent.SetDestination(m_Target.position);
        if (talkTimer == 0f)
        {
            audioSource.clip = audioClipSearch;
            audioSource.Play();
            talkTimer = 4f;
        }
    }

    public NPCState GetNPCState()
    {
        return state;
    }

    public string GetNPCName()
    {
        return enemyName;
    }
}