using UnityEngine;

/// <summary>
/// HUD basico que muestra munición, recarga y salud usando OnGUI.
/// Se suscribe a los eventos de WeaponSystem y PlayerHealth (desacoplado).
/// </summary>
public class HUDDisplay : MonoBehaviour
{
    [SerializeField] private WeaponSystem weapon;
    [SerializeField] private PlayerHealth health;

    private int   _ammo;
    private int   _maxAmmo;
    private bool  _reloading;
    private float _hp;
    private float _maxHp;

    private GUIStyle _bigStyle;
    private GUIStyle _reloadStyle;
    private GUIStyle _hpStyle;

    private void Start()
    {
        if (weapon == null) weapon = FindObjectOfType<WeaponSystem>();
        if (health == null) health = FindObjectOfType<PlayerHealth>();

        if (weapon != null)
        {
            _ammo    = weapon.CurrentAmmo;
            _maxAmmo = weapon.MaxAmmo;

            weapon.OnAmmoChanged += OnAmmoChanged;
            weapon.OnReloadStart += () => _reloading = true;
            weapon.OnReloadEnd   += () => _reloading = false;
        }

        if (health != null)
        {
            _hp    = health.CurrentHealth;
            _maxHp = health.MaxHealth;
            health.OnHealthChanged += OnHealthChanged;
        }
    }

    private void OnAmmoChanged(int current, int max)
    {
        _ammo = current;
        _maxAmmo = max;
    }

    private void OnHealthChanged(float current, float max)
    {
        _hp = current;
        _maxHp = max;
    }

    private void OnGUI()
    {
        // Estilos (inicializar solo una vez)
        if (_bigStyle == null)
        {
            _bigStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
            _reloadStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.yellow }
            };
            _hpStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                normal = { textColor = Color.white }
            };
        }

        // Fondo semi-transparente para legibilidad
        GUI.color = new Color(0f, 0f, 0f, 0.5f);
        GUI.DrawTexture(new Rect(10, Screen.height - 100, 250, 90), Texture2D.whiteTexture);
        GUI.color = Color.white;

        // Munición (abajo izquierda)
        GUI.Label(new Rect(25, Screen.height - 90, 240, 40),
            $"MUNICIÓN: {_ammo} / {_maxAmmo}", _bigStyle);

        // Barra de salud
        DrawHealthBar();

        // Indicador de RECARGANDO (centro de pantalla)
        if (_reloading)
        {
            GUI.color = new Color(0f, 0f, 0f, 0.7f);
            GUI.DrawTexture(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 30, 300, 60), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height / 2 - 25, 300, 60),
                "RECARGANDO...", _reloadStyle);
        }

        // Mira / Crosshair
        DrawCrosshair();
    }

    private void DrawHealthBar()
    {
        float pct = _maxHp > 0 ? _hp / _maxHp : 0f;
        Rect bg   = new Rect(25, Screen.height - 45, 230, 20);
        Rect fill = new Rect(25, Screen.height - 45, 230 * pct, 20);

        GUI.color = Color.black;
        GUI.DrawTexture(bg, Texture2D.whiteTexture);

        GUI.color = Color.Lerp(Color.red, Color.green, pct);
        GUI.DrawTexture(fill, Texture2D.whiteTexture);

        GUI.color = Color.white;
        GUI.Label(new Rect(30, Screen.height - 47, 230, 22),
            $"HP: {Mathf.RoundToInt(_hp)} / {Mathf.RoundToInt(_maxHp)}", _hpStyle);
    }

    private void DrawCrosshair()
    {
        float size = 8f;
        float cx = Screen.width  / 2f;
        float cy = Screen.height / 2f;

        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(cx - size, cy - 1, size * 2, 2), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(cx - 1, cy - size, 2, size * 2), Texture2D.whiteTexture);
    }
}
