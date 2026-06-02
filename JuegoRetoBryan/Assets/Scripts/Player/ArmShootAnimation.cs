using UnityEngine;

/// <summary>
/// Animacion procedural del brazo al disparar.
/// Levanta el hombro/brazo hacia adelante cuando WeaponSystem dispara
/// y lo retorna suavemente a su posicion original.
///
/// No requiere animaciones externas. Trabaja directamente sobre los huesos
/// del rig humanoid (Warrior / Cleric / etc.).
/// </summary>
public class ArmShootAnimation : MonoBehaviour
{
    [Header("Configuracion del Brazo")]
    [Tooltip("Hueso del brazo derecho. Si esta vacio se busca automaticamente.")]
    [SerializeField] private Transform rightArmBone;

    [Tooltip("Rotacion local extra cuando dispara (grados Euler).")]
    [SerializeField] private Vector3 shootRotation = new Vector3(-70f, 0f, 0f);

    [Tooltip("Velocidad de subida del brazo.")]
    [SerializeField] private float raiseSpeed = 20f;

    [Tooltip("Velocidad de retorno a la posicion original.")]
    [SerializeField] private float returnSpeed = 8f;

    [Tooltip("Cuanto tiempo (seg) se mantiene el brazo levantado tras disparar.")]
    [SerializeField] private float holdTime = 0.15f;

    private Quaternion _restRotation;
    private Quaternion _targetRotation;
    private float      _holdTimer;
    private bool       _isShooting;

    private void Start()
    {
        if (rightArmBone == null)
            rightArmBone = FindArmBone(transform);

        if (rightArmBone == null)
        {
            Debug.LogWarning("[ArmShootAnimation] No se encontro el hueso del brazo derecho.");
            enabled = false;
            return;
        }

        _restRotation   = rightArmBone.localRotation;
        _targetRotation = _restRotation;

        // Suscribirse al evento OnShoot del WeaponSystem
        WeaponSystem weapon = GetComponent<WeaponSystem>();
        if (weapon != null)
            weapon.OnShoot += OnShoot;
    }

    private void OnShoot()
    {
        _isShooting = true;
        _holdTimer  = holdTime;
        _targetRotation = _restRotation * Quaternion.Euler(shootRotation);
    }

    private void LateUpdate()
    {
        if (rightArmBone == null)
            return;

        if (_isShooting)
        {
            // Sube el brazo rapidamente
            rightArmBone.localRotation = Quaternion.Slerp(
                rightArmBone.localRotation, _targetRotation, raiseSpeed * Time.deltaTime);

            _holdTimer -= Time.deltaTime;
            if (_holdTimer <= 0f)
            {
                _isShooting = false;
                _targetRotation = _restRotation;
            }
        }
        else
        {
            // Regresa suavemente a posicion original
            rightArmBone.localRotation = Quaternion.Slerp(
                rightArmBone.localRotation, _restRotation, returnSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Busca recursivamente el hueso del brazo derecho.
    /// Soporta multiples convenciones de nombrado (Mixamo, Maya, Blender, etc.).
    /// </summary>
    private static Transform FindArmBone(Transform root)
    {
        string[] candidates = {
            "RightArm", "Right Arm", "Arm_R", "arm_r", "arm.R",
            "mixamorig:RightArm", "Bip01 R UpperArm", "R_Arm",
            "RightShoulder", "Right Shoulder", "Shoulder_R", "shoulder.R",
            "mixamorig:RightShoulder", "upper_arm.R", "UpperArm_R"
        };

        // Prioriza el brazo (no el hombro) para mas amplitud de movimiento
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name;
            foreach (string c in candidates)
            {
                if (n.Equals(c, System.StringComparison.OrdinalIgnoreCase))
                    return t;
            }
        }

        // Fallback: busca cualquier nombre que contenga "arm" + "r"
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            string n = t.name.ToLower();
            if ((n.Contains("arm") || n.Contains("shoulder")) && n.Contains("r"))
                return t;
        }

        return null;
    }
}
