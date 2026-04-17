using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float laneDistance = 3f;
    public float laneChangeSpeed = 15f;

    private InputSystem_Actions controls;
    private int targetLane = 1; // 0=Left, 1=Middle, 2=Right
    private Vector2 moveInput;

    [Header("Zdravie (Health)")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Nesmrteľnosť (I-Frames) po náraze")]
    [Tooltip("Ako dlho po náraze bude auto presvitať stredom prekážok bez zranenia")]
    public float invincibilityDuration = 1.5f;
    private bool isInvincible = false;
    public float blinkInterval = 0.1f; // Rýchlosť blikania auta
    
    // Budeme vypípať celú viditeľnosť modelu auta pri blikaní
    private MeshRenderer[] carRenderers;

    [Header("Efekty Zničenia (Dýmenie a Výbuch)")]
    [Tooltip("Vylezie jemný dym pri zdraví 50 a menej")]
    public GameObject smokeLightEffect;
    
    [Tooltip("Hustý, rýchly dym pri zdraví 20 a menej")]
    public GameObject smokeHeavyEffect;

    [Tooltip("Explózia pri totálnom zničení (zdravie 0)")]
    public GameObject explosionEffect;

    // Tento event hovorí nášmu UI Slideeru (ak vôbec v hre je), nech sa zmenší
    public event System.Action<int> OnHealthChanged;

    void Start()
    {
        currentHealth = maxHealth;
        
        // Najde úplne všetky viditeľné časti modelu auta a jeho kolies atď.
        carRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    void Awake()
    {
        controls = new InputSystem_Actions();

        // Register for lane switching events
        controls.Player.Move.performed += ctx => OnMove(ctx.ReadValue<Vector2>());

        // Natvrdo nielen skryjeme GameObjekty, ale ak majú bežiaci ParticleSystem,
        // tak ho brutálne zastavíme a vymažeme jeho už vygenerované častice z pamäte.
        ForceStopParticle(smokeLightEffect);
        ForceStopParticle(smokeHeavyEffect);
        ForceStopParticle(explosionEffect);
    }

    private void ForceStopParticle(GameObject obj)
    {
        if (obj == null) return;
        
        // Zastavenie a vyčistenie partiklov (vymaže ten puff na prvom frame)
        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
        else
        {
            // Ak je to zložitý prefab, skúsime nájsť aspoň v deťoch
            ParticleSystem childPs = obj.GetComponentInChildren<ParticleSystem>();
            if (childPs != null) childPs.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        obj.SetActive(false);
    }

    void OnEnable()
    {
        controls.Enable();
    }

    void OnDisable()
    {
        controls.Disable();
    }

    private void OnMove(Vector2 direction)
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        // Detect discrete lane switch based on X input
        if (direction.x < -0.5f)
        {
            if (targetLane > 0) targetLane--;
        }
        else if (direction.x > 0.5f)
        {
            if (targetLane < 2) targetLane++;
        }
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.isGameOver) return;

        // Calculate target X position
        float targetX = (targetLane - 1) * laneDistance;
        
        // Smoothly interpolate
        float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * laneChangeSpeed);
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            // Ak je nesmrteľný a bliká, náraz úplne odignorujeme!
            if (isInvincible) return; 

            // Zistíme si z neho presný Damage
            ObstacleData data = other.GetComponent<ObstacleData>();
            int damage = data != null ? data.damageAmount : 100; // Ak nemá dáta, default zabije

            // Aplikovanie rany
            TakeDamage(damage);

            // Aby naša hra mala ten "Juicy feeling", prekážku okamžite rozbijeme/zmažeme,
            // čím dáme pocit, že sme do nej buchli, a nezasekneme sa dnu v jej collideroch
            Destroy(other.gameObject); 
        }
    }

    public void TakeDamage(int dmg)
    {
        // Spustíme trasenie kamery: silnejšie pri autách (Instant kill), jemnejšie pri smetiakoch
        if (CameraShake.Instance != null && dmg > 0)
        {
            float shakePower = dmg >= 50 ? 0.6f : 0.25f;
            CameraShake.Instance.Shake(0.3f, shakePower);
        }

        currentHealth -= dmg;
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            
            // Auto bolo zničené! Zapneme výbuch a skryjeme normálne dymenie
            if (explosionEffect != null) explosionEffect.SetActive(true);
            if (smokeLightEffect != null) smokeLightEffect.SetActive(false);
            if (smokeHeavyEffect != null) smokeHeavyEffect.SetActive(false);

            if (GameManager.Instance != null) GameManager.Instance.GameOver();
        }
        else
        {
            // Ešte nám ostal Health, spustíme nesmrteľné blikanie
            StartCoroutine(InvincibilityRoutine());

            // --- SMOKE LOGIKA PODĽA HP ---
            if (currentHealth <= 20)
            {
                // Kritický stav: Vypneme jemný dym, zapneme silný dym
                if (smokeLightEffect != null) smokeLightEffect.SetActive(false);
                if (smokeHeavyEffect != null) smokeHeavyEffect.SetActive(true);
            }
            else if (currentHealth <= 50)
            {
                // Zlý stav: Iba jemný dym
                if (smokeLightEffect != null) smokeLightEffect.SetActive(true);
                // Pre istotu, ak si nabral lekárničku a vrátil sa z <20 späť nad 20:
                if (smokeHeavyEffect != null) smokeHeavyEffect.SetActive(false);
            }
        }

        // Nakričíme UI Slideru, aby sa hneď zmenšil
        OnHealthChanged?.Invoke(currentHealth);
    }

    // Kúzelná Coroutina, ktorá nám striedavo vypína a zapína zobrazenie modelu auta
    private System.Collections.IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;

        while (elapsed < invincibilityDuration)
        {
            // Kúzlo z matematiky Múdrych: toto vytvorí spravodlivý cyklus TRUE-FALSE-TRUE podľa času
            bool isVisible = (elapsed % (blinkInterval * 2)) < blinkInterval;
            SetRenderers(isVisible);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Musíme sa ubezpečiť, že auto určiťe nezostane zacyklené v neviditeľnosti
        SetRenderers(true);
        isInvincible = false;
    }

    private void SetRenderers(bool state)
    {
        if (carRenderers == null) return;
        // Skryje / odkryje auto na obrazovke
        foreach (var r in carRenderers)
        {
            if (r != null) r.enabled = state;
        }
    }
}
