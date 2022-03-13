using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisionNPC : MonoBehaviour
{
    [SerializeField] private float m_ViewRadius;
    [Range(0,360)]
    [SerializeField] private float m_ViewAngle;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    private List<Transform> m_VisibleTargets = new List<Transform>();

    private void Start()
    {
        StartCoroutine("FindTargetsWithDelay", .2f);
    }

    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }

    public void FindVisibleTargets()
    {
        m_VisibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, m_ViewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {
            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, dirToTarget) < m_ViewAngle / 2 || Vector3.Distance(transform.position, target.position) < 2f)
            {
                float dstToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    m_VisibleTargets.Add(target);

                    if (gameObject.GetComponentInParent<NPC_AI>() != null)
                    {
                        gameObject.GetComponentInParent<NPC_AI>().Alert(target);
                    }

                    else if (gameObject.GetComponentInParent<GunTurret>() != null)
                    {
                        gameObject.GetComponentInParent<GunTurret>().Alert(target);
                    }

                    else
                    {
                        break;
                    }
                }
            }
        }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public float GetViewRadius()
    {
        return m_ViewRadius;
    }

    public float GetViewAngle()
    {
        return m_ViewAngle;
    }

    public List<Transform> GetVisibleTargets()
    {
        return m_VisibleTargets;
    }
}
