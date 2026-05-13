using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CameraViewUI : MonoBehaviour
{
    public static CameraViewUI Instance;

    [Header("Root")]
    [SerializeField] private GameObject rootPanel;          // the whole overlay

    [Header("Feed Layers")]
    [SerializeField] private RawImage feedImage;            // Layer 0: camera RT
    [SerializeField] private RawImage scanlineOverlay;      // Layer 1: scrolling scanlines
    [SerializeField] private RawImage staticNoiseImage;     // Layer 2: static noise sprite
    [SerializeField] private RawImage chromaR;              // Layer 3a: red channel offset
    [SerializeField] private RawImage chromaB;              // Layer 3b: blue channel offset

    [Header("Signal Flash")]
    [SerializeField] private Image switchFlash;             // Layer 5: white flash on switch

    [Header("HUD Chrome")]
    [SerializeField] private TextMeshProUGUI cameraLabel;   // "CAM-03 / F2"
    [SerializeField] private TextMeshProUGUI cameraIndexText; // "1 / 6"

    [Header("Overuse Warning")]
    [SerializeField] private Image overuseVignette;         // Layer 7: red vignette
    [SerializeField] private TextMeshProUGUI overuseText;   // "SURVEILLANCE DETECTED"

    [Header("Lock State")]
    [Tooltip("Overlay escuro opcional mostrado quando a câmara está locked.")]
    [SerializeField] private Image lockOverlay;
    [Tooltip("Label opcional — ex: 'SINAL ENCRIPTADO — [H] PARA HACKEAR'.")]
    [SerializeField] private TextMeshProUGUI lockLabel;

    [Header("Navigation Hint")]
    [SerializeField] private TextMeshProUGUI navHint;       // "[A] ANTERIOR | [D] PRÓXIMO | [E] SAIR"

    [Header("Noise Textures")]
    [Tooltip("Array of small noise/static sprites. Cycled rapidly for the static effect.")]
    [SerializeField] private Texture2D[] noiseFrames;

    [Header("Tuning")]
    [SerializeField] private float scanlineScrollSpeed = 0.08f;
    [SerializeField] private float chromaMaxOffset     = 0.018f;
    [SerializeField] private float ghostPulseSpeed     = 1.8f;
    [SerializeField] private float switchFlashDuration = 0.12f;
    [SerializeField] private int   noiseFrameInterval  = 3;     // frames between noise update

    private CameraSystem system;
    private SurveillanceCamera currentCam;
    private bool isOpen = false;

    private float scanlineY   = 0f;
    private int   frameCount  = 0;
    private int   noiseIndex  = 0;

    private Coroutine flashCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        rootPanel.SetActive(false);
    }

    void Update()
    {
        if (!isOpen || system == null) return;

        HandleInput();
        UpdateScanlines();
        UpdateNoise();
        UpdateChroma();
        UpdateHUD();
        UpdateOveruseWarning();
    }

    public void Show(CameraSystem cameraSystem)
    {
        system = cameraSystem;
        isOpen = true;
        rootPanel.SetActive(true);

        currentCam = system.ActiveCamera;
        ApplyCameraFeed(currentCam);
        RefreshHUDLabels();

        UpdateLockState();
    }

    public void Hide()
    {
        isOpen = false;
        rootPanel.SetActive(false);
    }

    public void OnCameraChanged(SurveillanceCamera newCam) {
        currentCam = newCam;
        ApplyCameraFeed(newCam);
        RefreshHUDLabels();
        TriggerSwitchFlash();
        UpdateLockState();
    }
    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            system.PreviousCamera();

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            system.NextCamera();

        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
            system.CloseCameraView();
    }

    private void ApplyCameraFeed(SurveillanceCamera cam)
    {
        if (feedImage == null || cam == null) return;
        feedImage.texture = cam.renderTexture;

        // Chroma offset images share the same RT
        chromaR.texture = cam.renderTexture; chromaR.color = new Color(1f, 0.2f, 0.2f, 0.35f);
        chromaB.texture = cam.renderTexture; chromaB.color = new Color(0.2f, 0.2f, 1f, 0.35f);
    }

    private void UpdateScanlines()
    {
        if (scanlineOverlay == null) return;

        scanlineY += scanlineScrollSpeed * Time.deltaTime;
        if (scanlineY > 1f) scanlineY -= 1f;

        // Degrade speed & opacity with signal integrity
        float integrity = system.SignalIntegrity;
        scanlineOverlay.uvRect = new Rect(0f, scanlineY, 1f, 1f);
        Color c = scanlineOverlay.color;
        c.a = Mathf.Lerp(0.55f, 0.85f, 1f - integrity);  // more visible when degraded
        scanlineOverlay.color = c;
    }

    private void UpdateNoise()
    {
        frameCount++;
        // Noise flickers faster as signal degrades
        float integrity = system.SignalIntegrity;
        int interval = Mathf.Max(1, Mathf.RoundToInt(noiseFrameInterval * integrity));

        if (frameCount >= interval)
        {
            frameCount = 0;
            noiseIndex = (noiseIndex + 1) % noiseFrames.Length;
            staticNoiseImage.texture = noiseFrames[noiseIndex];

            // Offset UV randomly for chaos
            staticNoiseImage.uvRect = new Rect(
                Random.Range(0f, 1f), Random.Range(0f, 1f), 1f, 1f);
        }

        Color nc = staticNoiseImage.color;
        nc.a = Mathf.Lerp(0f, 0.45f, 1f - integrity);
        staticNoiseImage.color = nc;
    }

    private void UpdateChroma()
    {

        float integrity = system.SignalIntegrity;
        float heat      = system.ResidualHeat;
        float distortion = Mathf.Lerp(0f, chromaMaxOffset, (1f - integrity) + heat * 0.3f);

        // Subtle sinusoidal drift for the "analog" feel
        float t = Time.time;
        float rx = Mathf.Sin(t * 1.3f) * distortion;
        float ry = Mathf.Cos(t * 0.9f) * distortion * 0.5f;
        float bx = Mathf.Sin(t * 1.1f + 1.5f) * -distortion;

        RectTransform rtR = chromaR.rectTransform;
        RectTransform rtB = chromaB.rectTransform;

        rtR.anchoredPosition = new Vector2(rx * Screen.width, ry * Screen.height);
        rtB.anchoredPosition = new Vector2(bx * Screen.width, 0f);

        // Opacity scales with distortion
        Color cr = chromaR.color; cr.a = distortion / chromaMaxOffset * 0.35f; chromaR.color = cr;
        Color cb = chromaB.color; cb.a = distortion / chromaMaxOffset * 0.35f; chromaB.color = cb;
    }

    private void UpdateHUD()
    {
            cameraIndexText.text = $"{system.CurrentCameraIndex + 1} / {system.CameraCount}";
    }

    private void RefreshHUDLabels()
    {
        cameraLabel.text = $"{currentCam.cameraLabel}  //  F{currentCam.floor}";
    }

    private void UpdateOveruseWarning()
    {
        float heat      = system.ResidualHeat;
        float integrity = system.SignalIntegrity;
        float danger    = Mathf.Clamp01((1f - integrity) * 1.5f + heat * 0.5f);

        bool highDanger = danger > 0.6f;

            Color vc = overuseVignette.color;
            vc.a = Mathf.Lerp(0f, 0.45f, danger);
            overuseVignette.color = vc;

            overuseText.gameObject.SetActive(highDanger);
            if (highDanger)
            {
                // Blink
                float blink = Mathf.Sin(Time.time * 4f * Mathf.PI) > 0f ? 1f : 0f;
                Color tc = overuseText.color; tc.a = blink; overuseText.color = tc;
                overuseText.text = "ABUSO DE CÂMARA!";
            }
    }

    private void TriggerSwitchFlash()
    {
        if (switchFlash == null) return;
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        switchFlash.gameObject.SetActive(true);
        Color c = switchFlash.color; c.a = 0.7f; switchFlash.color = c;
        float elapsed = 0f;
        while (elapsed < switchFlashDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0.7f, 0f, elapsed / switchFlashDuration);
            switchFlash.color = c;
            yield return null;
        }
        switchFlash.gameObject.SetActive(false);
        flashCoroutine = null;
    }

    private void UpdateLockState() {
        if (system == null) return;
        bool locked = !system.IsUnlocked(system.CurrentCameraIndex);

        // Oculta o feed quando locked
        feedImage.enabled = !locked;
        chromaR.enabled = !locked;
        chromaB.enabled = !locked;

        // Overlay e label opcionais
        lockOverlay.gameObject.SetActive(locked);
        lockLabel.gameObject.SetActive(locked);

        // Hint inclui tecla de hack quando locked
        navHint.text = "[A] PREV  |  [D] NEXT  |  [X] SAIR";
    }


    public void ShowLockedState() => UpdateLockState();
}
