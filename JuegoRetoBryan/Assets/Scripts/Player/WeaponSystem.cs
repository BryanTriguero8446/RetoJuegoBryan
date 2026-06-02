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

    [Header("Origen del disparo")]
    [Tooltip("Punto de origen del disparo. Si esta vacio se usa la posicion del Player + 1.2m de altura.")]
    [SerializeField] private Transform  muzzlePoint;

    [Tooltip("Si esta marcado, la direccion del disparo se aplana en Y (100% horizontal).")]
    [SerializeField] private bool       forceHorizontal = true;

    [Header("Debug")]
    [SerializeField] private bool       drawDebugRay = false;

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

        // Direccion 100% HORIZONTAL hacia donde mira el jugador
        Vector3 dir = transform.forward;
        if (forceHorizontal)
        {
            dir.y = 0f;                // aplastar componente vertical
            dir.Normalize();           // re-normalizar tras aplastar
        }

        Vector3 origin = GetMuzzlePosition();

        if (drawDebugRay)
        {
            Debug.DrawRay(origin, dir * 10f, Color.green, 1.5f);
            Debug.Log($"[WeaponSystem] Disparo dir={dir} (y={dir.y:F3} debe ser 0)");
        }

        GameObject bullet = BulletFactory.Create(origin, dir, damage, bulletLife);

        if (bullet.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.useGravity = false;     // garantia: la bala no cae
            rb.velocity   = dir * bulletSpeed;
        }

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

        // Altura del pecho + 0.5m al frente para no chocar con el propio cuerpo
        return transform.position + Vector3.up * 1.2f + transform.forward * 0.5f;
    }

    private bool HasAttackParam()
    {
        foreach (var p in _animator.parameters)
            if (p.nameHash == AttackHash) return true;
        return false;
    }

    // Gizmo en el editor: dibuja una flecha verde indicando hacia donde sale la bala
    private void OnDrawGizmosSelected()
    {
        Vector3 origin = muzzlePoint != null
            ? muzzlePoint.position
            : transform.position + Vector3.up * 1.2f + transform.forward * 0.5f;

        Vector3 dir = transform.forward;
        if (forceHorizontal) { dir.y = 0f; dir.Normalize(); }

        Gizmos.color = Color.green;
        Gizmos.DrawLine(origin, origin + dir * 5f);
        Gizmos.DrawSphere(origin + dir * 5f, 0.15f);
    }
}
