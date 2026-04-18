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

    [Header("Damage (Poškodenie)")]
    public int carDamage = 100;
    public int barricadeDamage = 50;
    public int edgeDamage = 30; // Používateľ chcel 30 pre kužele/okraje

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

        // BRUTÁLNY RESET: Vypneme dymy tak, že ich ani kamošova scéna neprebudí
        ForceStopAndHide(smokeLightEffect);
        ForceStopAndHide(smokeHeavyEffect);
        ForceStopAndHide(explosionEffect);
    }

    private void ForceStopAndHide(GameObject obj)
    {
        if (obj == null) return;
        
        // Nájdeme úplne všetky dymové systémy v objekte aj v jeho deťoch
        ParticleSystem[] allParticles = obj.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in allParticles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Clear(); // Okamžité vymazanie už existujúcich guličiek
            var main = ps.main;
            main.playOnAwake = false; // Poistka priamo v kóde
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

        // --- RESTORED TILT & LAST_X ---
        // Aplikujeme náklon celého auta
        transform.localRotation = Quaternion.Euler(0, 0, currentTilt);
        // Uložíme si pozíciu pre výpočet rýchlosti v ďalšom frame
        lastX = newX;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            // Zistíme si z neho typ a damage
            ObstacleData data = other.GetComponent<ObstacleData>();
            int damage = 30; // Default

            if (data != null)
            {
                // Priradíme damage podľa kategórie, ktorú máš v Inspectore
                switch (data.obstacleType)
                {
                    case ObstacleType.Car: damage = carDamage; break;
                    case ObstacleType.Barricade: damage = barricadeDamage; break;
                    case ObstacleType.Edge: damage = edgeDamage; break;
                    default: damage = data.damageAmount; break;
                }
            }

            // --- REAKCIA PODĽA TYPU NÁRAZU (Iba okamžité efekty podľa tvojho zoznamu) ---
            if (damage >= 100)
            {
                // Car prefarbs = Puff effect
                PlayParticle(explosionEffect);
            }
            else if (damage >= 50)
            {
                // Barricade prefabs = Puff effect
                PlayParticle(explosionEffect);
            }
            else
            {
                // Edge obstacles = Nadskocenie
                if (!isJumping) StartCoroutine(JumpRoutine());
            }

            // Aplikovanie rany
            TakeDamage(damage);

            // Zničenie prekážky
            Destroy(other.gameObject); 
        }
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        Debug.Log($"<color=red>[NÁRAZ]</color> Ubralo mi {dmg} HP. Zostáva mi: {currentHealth} HP.");

        // Spustíme trasenie kamery
        if (CameraShake.Instance != null && dmg > 0)
        {
            float shakePower = dmg >= 50 ? 0.6f : 0.25f;
            CameraShake.Instance.Shake(0.3f, shakePower);
        }
        
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            PlayParticle(explosionEffect);
            if (GameManager.Instance != null) GameManager.Instance.GameOver();
        }
        else
        {
            RefreshLowHealthSmoke();
        }

        OnHealthChanged?.Invoke(currentHealth);
    }

    private void RefreshLowHealthSmoke()
    {
        // 1. NAJPRV KRITICKÝ STAV (Hustý dym)
        if (currentHealth <= 30)
        {
            if (smokeLightEffect != null) smokeLightEffect.SetActive(false);
            if (smokeHeavyEffect != null && !smokeHeavyEffect.activeSelf) PlayParticle(smokeHeavyEffect);
            return; // Ak sme v kritickom stave, kód pre Light Smoke nižšie sa už ani nepozrie
        }
        
        // 2. POTOM ZLÝ STAV (Ľahký dym)
        if (currentHealth <= 50)
        {
            if (smokeHeavyEffect != null) smokeHeavyEffect.SetActive(false);
            if (smokeLightEffect != null && !smokeLightEffect.activeSelf) PlayParticle(smokeLightEffect);
            return;
        }

        // 3. ZDRAVÉ AUTO
        if (smokeLightEffect != null) smokeLightEffect.SetActive(false);
        if (smokeHeavyEffect != null) smokeHeavyEffect.SetActive(false);
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
