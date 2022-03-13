using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TurretState
{
    watching,
    shooting,
    disabled,
    idle
}

public class GunTurret : MonoBehaviour
{
    [SerializeField] private GameObject BulletPrefab;
    [SerializeField] private GameObject ExitPoint;
    [SerializeField] private GameObject TurretHeadPivot;
    [SerializeField] private GameObject TurretHead;
    [SerializeField] private Transform Target;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float bulletSpread;
    [SerializeField] private int bulletDamage;
    [SerializeField] private TurretState state;
    [SerializeField] private Light light;
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip gunSound;
    [SerializeField] private GameObject soundPrefabHit;

    private float idleTimer;
    private float watchTimer;
    private float fireTimer;
    private float fireCooldown = 0.2f;

    private bool allowedToShoot;

    private void Start()
    {
        state = TurretState.idle;
        light.enabled = false;
        fireTimer = fireCooldown;
    }

    private void Update()
    {
        if (idleTimer != 0)
        {
            idleTimer -= Time.deltaTime;

            if (idleTimer < 0)
            {
                idleTimer = 0;
                state = TurretState.watching;
            }
        }

        if (watchTimer != 0)
        {
            watchTimer -= Time.deltaTime;

            if (watchTimer < 0)
            {
                watchTimer = 0;
                state = TurretState.idle;
            }
        }

        if (fireTimer != 0)
        {
            fireTimer -= Time.deltaTime;

            if (fireTimer < 0)
            {
                allowedToShoot = true;
                fireTimer = 0;
            }
        }

        if (light.enabled)
        {
            Invoke("DisableFlash", 0.05f);
        }

        if (state == TurretState.idle)
        {
            idleTimer = 2f;
        }

        else if (state == TurretState.watching)
        {
            Watch();
            watchTimer = 4f;
        }

        else if (state == TurretState.shooting)
        {
            Attack();
        }
    }

    public void Alert(Transform target)
    {
        state = TurretState.shooting;
        Target = target;
        idleTimer = 3f;
    }

    private void Attack()
    {
        if (Target.GetComponent<Health>().GetHealth() >= 0f)
        {
            Vector3 dir = (Target.position - TurretHead.transform.position).normalized;
            float dot = Vector3.Dot(dir, TurretHead.transform.forward);
            float step = rotationSpeed * Time.deltaTime;
            Vector3 newDirection = Vector3.RotateTowards(TurretHead.transform.forward, dir, step, 0.0f);

            if (dot < 0.9999)
            {
                TurretHead.transform.rotation = Quaternion.LookRotation(newDirection);
            }

            if (dot > 0.9)
            {
                if (allowedToShoot)
                {
                    Shoot();
                    allowedToShoot = false;
                    light.enabled = true;
                    fireTimer = fireCooldown;
                }
            }
        }
    }

    private void Shoot()
    {
        if (gunSound != null)
        {
            audioSource.clip = gunSound;
            audioSource.Play();
        }

        GameObject bullet = Instantiate(BulletPrefab, ExitPoint.transform.position, ExitPoint.transform.rotation);
        Rigidbody bulletRigBody = bullet.GetComponent<Rigidbody>();
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.m_damage = bulletDamage;
        bulletScript.SetOwner(gameObject);
        string ownerName = "Turret";
        
        bulletScript.SetShooterName(ownerName, tag);

        GameObject soundHitBullet = Instantiate(soundPrefabHit, ExitPoint.transform.position, ExitPoint.transform.rotation);
        soundHitBullet.transform.parent = bullet.transform;

        Vector3 bulletTransform = TurretHead.transform.eulerAngles;
        if (bulletSpread != 0)
        {
            //bullet inaccuracy if there is a spread;
            float newY = bulletTransform.y + Random.Range(-bulletSpread / 2, bulletSpread / 2);
            float newX = bulletTransform.x + Random.Range(-bulletSpread / 2, bulletSpread / 2);
            bulletTransform = new Vector3(newX, newY, bulletTransform.z);
        }

        bullet.transform.eulerAngles = bulletTransform;

        bulletRigBody.AddForce(bullet.transform.forward * bulletSpeed, ForceMode.Impulse);
    }

    private void Watch()
    {
        TurretHeadPivot.transform.Rotate(Vector3.up);
    }

    private void DisableFlash()
    {
        light.enabled = false;
    }
}