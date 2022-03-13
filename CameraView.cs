using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraView : MonoBehaviour
{
    private GameObject player;

    public float DistanceToPlayer = 0f;
    public Material TransparentMaterial = null;
    public float FadeInTimeout = 0.6f;
    public float FadeOutTimeout = 0.2f;
    public float TargetTransparency = 0.3f;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    private void Update()
    {
        DistanceToPlayer = Vector3.Distance(player.transform.position, transform.position);


        RaycastHit[] hits; 
        hits = Physics.RaycastAll(transform.position, transform.forward, DistanceToPlayer);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.tag != "Player" && hit.transform.tag != "Enemy" && hit.transform.tag != "Weapon" && hit.transform.tag != "Item" && hit.transform.tag != "Opaque")
            {
                Renderer R = hit.collider.GetComponent<Renderer>();
                if (R == null)
                {
                    continue;
                }
                AutoTransparent AT = R.GetComponent<AutoTransparent>();
                if (AT == null)
                {
                    AT = R.gameObject.AddComponent<AutoTransparent>();
                    AT.TransparentMaterial = TransparentMaterial;
                    AT.FadeInTimeout = FadeInTimeout;
                    AT.FadeOutTimeout = FadeOutTimeout;
                    AT.TargetTransparency = TargetTransparency;
                }
                AT.BeTransparent();
            }
        }
    }
}