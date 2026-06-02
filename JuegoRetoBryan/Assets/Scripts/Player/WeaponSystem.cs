using UnityEngine;

/// <summary>
/// Sistema de combate del jugador:
///   - Dispara proyectiles fisicos (Rigidbody) hacia arriba (Vector3.up).
///   - Cargador de 6 balas. Al llegar a 0, NO dispara hasta presionar R.
///   - Lanza eventos para que la UI se actualice sin acoplamiento directo.
///
/// Conecta este script al Player. Para visualizar el balanceo de la espada
/// se dispara un trigger en el Animator ("Attack") en cada disparo.
/// </summary>
public class WeaponSystem : MonoBehaviour
{
    [Header("Cargador")]
    [SerializeField] private int   maxAmmo  = 6;          // capacidad maxima

    [Header("Proyectil")]
    [SerializeField] private float damage        = 25f;
    [SerializeField] private float bulletSpeed   = 20f;   // m/s
    [SerializeField] private float bulletLife    = 3f;    // segundos antes de auto-destruir
    [SerializeField] private float fireRate      = 3f;    // disparos por segundo

    [Header("Origen y direccion")]
    [Tooltip("Punto de origen del disparo. Si esta vacio se usa la posicion del Player + 1m de altura.")]
    [SerializeField] private Transform  muzzlePoint;
    [Tooltip("Direccion del disparo. Por defecto VERTICAL hacia arriba.")]
    [SerializeField] private Vector3    shootDirection = Vector3.up;

    [Header("VFX (opcional)")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    public int  CurrentAmmo  { get; private set; }
    public int  MaxAmmo      => maxAmmo;
    public bool IsReloading  { get; private set; }
    public bool HasAmmo      => CurrentAmmo > 0;

    // Eventos para desacoplar de la UI / animator / SFX
    public event System.Action<int, int> OnAmmoChanged;
    public event System.Action           OnShoot;
    public event System.Action           OnReloadStart;
    public event System.Action           OnReloadEnd;
    public event System.Action           OnEmpty;

    private float    _nextFireTime;
    private Animator _animator;
    private static readonly int AttackHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        CurrentAmmo = maxAmmo;
        _animator   = GetComponent<Animator>();
    }

    private void Start()
    {
        OnAmmoChanged?.Invoke(CurrentAmmo, maxAmmo);
    }

    private void Update()
    {
        // Recarga manual: solo con R, nunca automatica
        if (Input.GetKeyDown(KeyCode.R) && !IsReloading && CurrentAmmo < maxAmmo)
        {
            Reload();
            return;
        }

        // Disparo (Click izquierdo o Fire1)
        if (Input.GetButtonDown("Fire1"))
            TryShoot();
    }

    private void TryShoot()
    {
        if (IsReloading)
            return;

        if (Time.time < _nextFireTime)
            return;

        if (!HasAmmo)
        {
            OnEmpty?.Invoke();
            return; // sin balas: no dispara, esperar R
        }

        _nextFireTime = Time.time + 1f / fireRate;
        Shoot();
    }

    private void Shoot()
    {
        CurrentAmmo--;
        OnAmmoChanged?.Invoke(CurrentAmmo, maxAmmo);
        OnShoot?.Invoke();

        // Trigger animacion de ataque (balanceo de espada)
        if (_animator != null && HasAttackParam())
            _animator.SetTrigger(AttackHash);

        // Crear y lanzar la bala
        Vector3 origin = GetMuzzlePosition();
        Vector3 dir    = shootDirection.normalized;

        GameObject bullet = BulletFactory.Create(origin, dir, damage, bulletLife);

        if (bullet.TryGetComponent<Rigidbody>(out var rb))
            rb.velocity = dir * bulletSpeed;

        // VFX en el cano
        if (muzzleFlashPrefab != null)
        {
            GameObject fx = Instantiate(muzzleFlashPrefab, origin, Quaternion.LookRotation(dir));
            Destroy(fx, 0.5f);
        }
    }

    /// <summary>Recarga inmediata: la animacion / espera se omiten para feedback rapido.</summary>
    private void Reload()
    {
        IsReloading = true;
        OnReloadStart?.Invoke();

        CurrentAmmo = maxAmmo;
        OnAmmoChanged?.Invoke(CurrentAmmo, maxAmmo);

        IsReloading = false;
        OnReloadEnd?.Invoke();
    }

    private Vector3 GetMuzzlePosition()
    {
        if (muzzlePoint != null)
            return muzzlePoint.position;

        return transform.position + Vector3.up * 1.5f;
    }

    private bool HasAttackParam()
    {
        foreach (var p in _animator.parameters)
            if (p.nameHash == AttackHash) return true;
        return false;
    }
}
