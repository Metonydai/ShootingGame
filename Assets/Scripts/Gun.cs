using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public enum FireMode { Auto, Burst, Single };
    public FireMode fireMode;

    public Transform[] projectileSpawn;
    public Projectile projectile;
    public float msBetweenShots = 100;
    public float muzzleVelocity = 35;
    public int burstCount;
    public int projectilePerMag;
    bool isReloading;
    public float reloadTime = .3f;
    
    [Header("Recoil")]
    public Vector2 kickMinMax = new Vector2(.05f, .2f);
    public Vector2 recoilAngleMinMax = new Vector2(3, 5);
    public float recoilMoveSettleTime = .1f;
    public float recoilRotationSettleTime = .1f;

    [Header("Effects")]
    public Transform shell;
    public Transform shellEjection;
    public AudioClip shootAudio;
    public AudioClip reloadAudio;

    Muzzleflash muzzleflash;

    float nextShotTime;

    bool triggerReleasedSinceLastShot;
    int shotsRemainingInBurst;
    int projectileRemainInMag;

    Vector3 recoilSmoothDampVelocity;
    float recoilRotSmoothDampVeclocity;
    float recoilAngle;

    void Start()
    {
        muzzleflash = GetComponent<Muzzleflash>();
        shotsRemainingInBurst = burstCount;
        projectileRemainInMag = projectilePerMag;
    }

    void LateUpdate()
    {
        // animate recoil
        transform.localPosition = Vector3.SmoothDamp(transform.localPosition, Vector3.zero, ref recoilSmoothDampVelocity, recoilMoveSettleTime);
        recoilAngle = Mathf.SmoothDamp(recoilAngle, 0, ref recoilRotSmoothDampVeclocity, recoilRotationSettleTime);
        transform.localEulerAngles += Vector3.left * recoilAngle;

        if (!isReloading && projectileRemainInMag == 0)
        {
            Reload();
        }
    }

    void Shoot()
    {
        float currentTime = Time.time;
        if (!isReloading && currentTime > nextShotTime && projectileRemainInMag > 0)
        {
            if (fireMode == FireMode.Burst)
            {
                if (shotsRemainingInBurst == 0)
                {
                    return;
                }
                shotsRemainingInBurst--;
            }
            else if (fireMode == FireMode.Single)
            {
                if (!triggerReleasedSinceLastShot)
                {
                    return;
                }
            }
            for (int i = 0; i < projectileSpawn.Length; i++)
            {
                if (projectileRemainInMag == 0)
                {
                    break;
                }
                projectileRemainInMag--;
                nextShotTime = currentTime + msBetweenShots / 1000;
                Projectile newProjectile = Instantiate(projectile, projectileSpawn[i].position, projectileSpawn[i].rotation);
                newProjectile.SetSpeed(muzzleVelocity);
            }
            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleflash.Activate();
            transform.localPosition -= Vector3.forward * Random.Range(kickMinMax.x, kickMinMax.y);
            recoilAngle += Random.Range(recoilAngleMinMax.x, recoilAngleMinMax.y);
            recoilAngle = Mathf.Clamp(recoilAngle, 0, 30);

            AudioManager.instance.PlaySound(shootAudio, transform.position);
        }
    }

    public void Reload()
    {
        if (!isReloading && projectileRemainInMag != projectilePerMag)
        {
            StartCoroutine(AnimateReload());
            AudioManager.instance.PlaySound(reloadAudio, transform.position);
        }
    }

    IEnumerator AnimateReload()
    {
        isReloading = true;
        yield return new WaitForSeconds(.2f);

        
        float reloadSpeed = 1 / reloadTime;

        Vector3 initialRot = transform.localEulerAngles;
        float maxReloadAngle = 30;

        float percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * reloadSpeed;
            float interpolation = 4 * (-percent*percent + percent);
            float reloadAngle = Mathf.Lerp(0, maxReloadAngle, interpolation);

            transform.localEulerAngles = initialRot + Vector3.left * reloadAngle;

            yield return null;
            
        }

        isReloading = false;
        projectileRemainInMag = projectilePerMag;
    }

    public void Aim(Vector3 aimPoint)
    {
        if (!isReloading)
            transform.LookAt(aimPoint);
    }

    public void OnTriggerHold()
    {
        Shoot();
        triggerReleasedSinceLastShot = false;
    }

    public void OnTriggerRelease()
    {
        triggerReleasedSinceLastShot = true;
        shotsRemainingInBurst = burstCount;
    }

}