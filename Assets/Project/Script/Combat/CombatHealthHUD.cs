using UnityEngine;

/// <summary>
/// Dibuja barras de vida sencillas sin necesitar un Canvas ni prefabs de UI.
/// </summary>
public sealed class CombatHealthHUD : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DogHealth dogHealth;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private CombatGameManager combatGameManager;

    [Header("Apariencia")]
    [SerializeField, Min(60f)] private float barWidth = 120f;
    [SerializeField, Min(10f)] private float barHeight = 16f;
    [SerializeField, Min(0f)] private float heightAboveCharacter = 0.15f;
    [SerializeField] private Color dogColor = new Color(0.15f, 0.75f, 0.25f);
    [SerializeField] private Color enemyColor = new Color(0.85f, 0.15f, 0.15f);
    [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);

    private GUIStyle labelStyle;
    private GUIStyle resultStyle;
    private Renderer[] dogRenderers;
    private Renderer[] enemyRenderers;

    private void Awake()
    {
        dogRenderers = dogHealth != null ? dogHealth.GetComponentsInChildren<Renderer>() : null;
        enemyRenderers = enemyHealth != null ? enemyHealth.GetComponentsInChildren<Renderer>() : null;
    }

    private void OnGUI()
    {
        EnsureStyles();

        if (combatGameManager != null && combatGameManager.HasCombatStarted)
        {
            DrawCharacterHealth(dogHealth, "PERRO", dogColor);
            DrawCharacterHealth(enemyHealth, "ENEMIGO", enemyColor);
        }

        DrawCombatResult();
    }

    private void DrawCharacterHealth(MonoBehaviour health, string title, Color fillColor)
    {
        if (health == null || Camera.main == null)
        {
            return;
        }

        Renderer[] renderers = health is DogHealth ? dogRenderers : enemyRenderers;
        Vector3 worldPosition = GetTopOfCharacter(health.transform, renderers);
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);

        // No dibuja la barra cuando el personaje esta detras de la camara.
        if (screenPosition.z <= 0f)
        {
            return;
        }

        float currentHealth;
        float maxHealth;

        if (health is DogHealth dog)
        {
            currentHealth = dog.CurrentHealth;
            maxHealth = dog.MaxHealth;
        }
        else if (health is EnemyHealth enemy)
        {
            currentHealth = enemy.CurrentHealth;
            maxHealth = enemy.MaxHealth;
        }
        else
        {
            return;
        }

        Rect rect = new Rect(
            screenPosition.x - barWidth * 0.5f,
            Screen.height - screenPosition.y - barHeight * 0.5f,
            barWidth,
            barHeight);

        DrawHealthBar(rect, title, currentHealth, maxHealth, fillColor);
    }

    private Vector3 GetTopOfCharacter(Transform character, Renderer[] renderers)
    {
        bool hasBounds = false;
        Bounds combinedBounds = new Bounds();

        if (renderers != null)
        {
            foreach (Renderer characterRenderer in renderers)
            {
                if (characterRenderer == null || !characterRenderer.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    combinedBounds = characterRenderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    combinedBounds.Encapsulate(characterRenderer.bounds);
                }
            }
        }

        if (!hasBounds)
        {
            return character.position + Vector3.up * (1f + heightAboveCharacter);
        }

        return new Vector3(
            combinedBounds.center.x,
            combinedBounds.max.y + heightAboveCharacter,
            combinedBounds.center.z);
    }

    private void DrawCombatResult()
    {
        if (combatGameManager == null || combatGameManager.IsCombatActive)
        {
            return;
        }

        string result = combatGameManager.IsVictory ? "VICTORIA" : "DERROTA";
        resultStyle.normal.textColor = combatGameManager.IsVictory ? Color.yellow : Color.red;
        Rect resultRect = new Rect(0f, Screen.height * 0.35f, Screen.width, 80f);
        GUI.Label(resultRect, result, resultStyle);
    }

    private void DrawHealthBar(Rect rect, string title, float currentHealth, float maxHealth, Color fillColor)
    {
        float healthPercent = maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

        Color previousColor = GUI.color;
        GUI.color = backgroundColor;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        GUI.color = fillColor;
        Rect fillRect = new Rect(rect.x, rect.y, rect.width * healthPercent, rect.height);
        GUI.DrawTexture(fillRect, Texture2D.whiteTexture);

        GUI.color = Color.white;
        GUI.Label(rect, $"{title}  {currentHealth:0}/{maxHealth:0}", labelStyle);
        GUI.color = previousColor;
    }

    private void EnsureStyles()
    {
        if (labelStyle != null)
        {
            return;
        }

        labelStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 11
        };
        labelStyle.normal.textColor = Color.white;

        resultStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 30
        };
    }

    private void OnValidate()
    {
        barWidth = Mathf.Max(60f, barWidth);
        barHeight = Mathf.Max(10f, barHeight);
        heightAboveCharacter = Mathf.Max(0f, heightAboveCharacter);
    }
}
