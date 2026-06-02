using System.Collections;
using UnityEngine;

/// <summary>
/// Sistema de disparo con Physics.Raycast, gestion de municion y recarga.
/// Asigna hitEffectPrefab con un ParticleSystem para VFX en el punto de impacto.
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    [Header("Disparo")]
    [SerializeField] private float damage      = 25f;
    [SerializeField] private float range       = 100f;
    [SerializeField] private float fireRate    = 2f;   // disparos por segundo
    [SerializeField] private LayerMask shootMask = ~0; // todo por defecto

    [Header("Municion")]
    [SerializeField] private int   maxAmmo    = 12;
    [SerializeField] private float reloadTime = 1.5f;

    [Header("VFX")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private float      hitEffectDuration = 1f;

    public int  CurrentAmmo  { get; private set; }
    public int  MaxAmmo      => maxAmmo;
    public bool IsReloading  { get; private set; }

    // Eventos para desacoplar la UI de la logica de arma
    public event System.Action<int, int> OnAmmoChanged;
    public event System.Action           OnReloadStart;
    public event System.Action           OnReloadEnd;
    public event System.Action           OnShoot;

    private float  _nextFireTime;
    private Camera _cam;

    private void Awake()
    {
        _cam        = Camera.main;
        CurrentAmmo = maxAmmo;
    }

    private void Update()
    {
        if (Input.GetButton("Fire1") && !IsReloading)
            TryShoot();

        if (Input.GetKeyDown(KeyCode.R) && !IsReloading && CurrentAmmo < maxAmmo)
            StartCoroutine(Reload());
    }

    private void TryShoot()
    {
        if (CurrentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Time.time < _nextFireTime)
            return;

        _nextFireTime = Time.time + 1f / fireRate;
        Shoot();
    }

    private void Shoot()
    {
        CurrentAmmo--;
        OnAmmoChanged?.Invoke(CurrentAmmo, maxAmmo);
        OnShoot?.Invoke();

        // Rayo desde el centro exacto de la pantalla
        Ray ray = _cam.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f));

        if (Physics.Raycast(ray, out RaycastHit hit, range, shootMask))
        {
            SpawnHitEffect(hit.point, hit.normal);

            // El sistema de dano usa IDamageable para no depender de tipos concretos
            if (hit.collider.TryGetComponent<IDamageable>(out var target))
                target.TakeDamage(damage);
        }
    }

    private void SpawnHitEffect(Vector3 position, Vector3 normal)
    {
        if (hitEffectPrefab == null)
            return;

        GameObject fx = Instantiate(hitEffectPrefab, position, Quaternion.LookRotation(normal));
        Destroy(fx, hitEffectDuration);
    }

    private IEnumerator Reload()
    {
        IsReloading = true;
        OnReloadStart?.Invoke();

        yield return new WaitForSeconds(reloadTime);

        CurrentAmmo = maxAmmo;
        IsReloading = false;
        OnAmmoChanged?.Invoke(CurrentAmmo, maxAmmo);
        OnReloadEnd?.Invoke();
    }
}
