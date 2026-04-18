using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float laneDistance = 3f;
    public float laneChangeSpeed = 15f;

    [Header("Steering Wheel Settings")]
    public Transform steeringWheel;
    public float maxSteeringAngle = 45f;
    public float steeringRotationSpeed = 15f;

    private InputSystem_Actions controls;
    private int targetLane = 1; // 0=Left, 1=Middle, 2=Right
    private Vector2 moveInput;

    [Header("Zdravie (Health)")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Efekty Zničenia (Dýmenie a Výbuch)")]
    [Tooltip("Vylezie jemný dym pri zdraví 50 a menej")]
    public GameObject smokeLightEffect;
    
    [Tooltip("Hustý, rýchly dym pri zdraví 20 a menej")]
    public GameObject smokeHeavyEffect;

    [Tooltip("Explózia pri totálnom zničení (zdravie 0)")]
    public GameObject explosionEffect;

    [Header("Animácia nárazu (Skok)")]
    public float jumpHeight = 0.5f;
    public float jumpDuration = 0.3f;
    private bool isJumping = false;

    [Header("Nakláňanie (Tilt) pri zatáčaní")]
    [Tooltip("Maximálny uhol naklonenia auta pri zmene pruhu")]
    public float maxTiltAngle = 10f;
    [Tooltip("Rýchlosť, akou sa auto vracia do rovnej polohy")]
    public float tiltSmoothing = 10f;
    private float currentTilt = 0f;
    private float lastX;

    // Tento event hovorí nášmu UI Slideeru (ak vôbec v hre je), nech sa zmenší
    public event System.Action<int> OnHealthChanged;

    void Start()
    {
        currentHealth = maxHealth;
        lastX = transform.position.x;
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

        float targetX = (targetLane - 1) * laneDistance;
        float newX = Mathf.Lerp(transform.position.x, targetX, Time.deltaTime * laneChangeSpeed);
        
        // --- VÝPOČET NAKLÁŇANIA (TILT) ---
        // Zistíme smer a rýchlosť pohybu do boku
        float movementDelta = newX - lastX;
        // Ak ideme doprava (delta > 0), auto sa nakloní doľava a naopak (preto to mínus)
        float targetTilt = -(movementDelta / Time.deltaTime) * (maxTiltAngle / 2f);
        targetTilt = Mathf.Clamp(targetTilt, -maxTiltAngle, maxTiltAngle);

        // Plynulé vyhladenie náklonu
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSmoothing);
        
        // Aplikujeme pohyb aj rotáciu
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);

        // Steering Wheel Rotation
        if (steeringWheel != null)
        {
            float distanceToTarget = targetX - transform.position.x;
            float targetAngle = 0f;

            // Ak sa ešte hýbeme (sme ďalej ako 0.1 od cieľa), držíme plný vytočený volant
            if (Mathf.Abs(distanceToTarget) > 0.2f)
            {
                // Ak je distanceToTarget kladná, ideme doprava -> volant točíme doprava (záporný Z uhol)
                targetAngle = (distanceToTarget < 0) ? -maxSteeringAngle : maxSteeringAngle;
            }

            // Plynule interpolujeme do zvoleného uhla (0 alebo max)
            Quaternion targetRot = Quaternion.Euler(0, 0, targetAngle);
            steeringWheel.localRotation = Quaternion.Lerp(steeringWheel.localRotation, targetRot, Time.deltaTime * steeringRotationSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            // Zistíme si z neho presný Damage
            ObstacleData data = other.GetComponent<ObstacleData>();
            int damage = data != null ? data.damageAmount : 34;

            // --- REAKCIA PODĽA TYPU NÁRAZU ---
            if (damage >= 100)
            {
                // Čelný náraz do auta = Veľký výbuch (Puff)
                PlayParticle(explosionEffect);
            }
            else if (damage >= 50)
            {
                // Náraz do zátarasy = Tiež výbuch (Puff), presne ako si chcel
                PlayParticle(explosionEffect);
                // Môžeme k tomu pridať aj hustý dym pre efekt
                PlayParticle(smokeHeavyEffect);
            }
            else
            {
                // Malý okrajový náraz (Smetiak) = Podskočenie auta ako cez spomaľovač
                if (!isJumping) StartCoroutine(JumpRoutine());
                PlayParticle(smokeLightEffect);
            }

            // Aplikovanie rany
            TakeDamage(damage);

            // Zničenie prekážky
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
            
            // Auto bolo zničené! 
            PlayParticle(explosionEffect);

            if (GameManager.Instance != null) GameManager.Instance.GameOver();
        }
        else
        {
            // Auto dymí podľa HP
            RefreshLowHealthSmoke();
        }

        // Nakričíme UI Slideru, aby sa hneď zmenšil
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void RefreshLowHealthSmoke()
    {
        if (currentHealth <= 20)
        {
            if (smokeLightEffect != null) smokeLightEffect.SetActive(false);
            if (smokeHeavyEffect != null && !smokeHeavyEffect.activeSelf) PlayParticle(smokeHeavyEffect);
        }
        else if (currentHealth <= 50)
        {
            if (smokeLightEffect != null && !smokeLightEffect.activeSelf) PlayParticle(smokeLightEffect);
        }
    }

    // Pomocná funkcia, ktorá nielen zapne objekt, ale natvrdo prikáže dymu začať dymiť
    private void PlayParticle(GameObject obj)
    {
        if (obj == null) return;
        
        obj.SetActive(true); // Zapne objekt v hierarchii
        
        // Pokúsi sa nájsť a spustiť Particle System na samotnom objekte
        ParticleSystem ps = obj.GetComponent<ParticleSystem>();
        if (ps != null) 
        {
            ps.Play(true);
        }
        else 
        {
            // Ak je to zložitý prefab (napr. výbuch z viacerých častí), spustí všetky Particle Systémy vnútri
            ParticleSystem childPs = obj.GetComponentInChildren<ParticleSystem>();
            if (childPs != null) childPs.Play(true);
        }
    }

    // Coroutina pre "skok" cez prekážku
    private System.Collections.IEnumerator JumpRoutine()
    {
        isJumping = true;
        Vector3 startPos = transform.localPosition;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / jumpDuration;
            
            // Sinusoidový pohyb (hore a dole)
            float yOffset = Mathf.Sin(normalizedTime * Mathf.PI) * jumpHeight;
            transform.localPosition = new Vector3(startPos.x, startPos.y + yOffset, startPos.z);
            
            yield return null;
        }

        transform.localPosition = startPos;
        isJumping = false;
    }
}
