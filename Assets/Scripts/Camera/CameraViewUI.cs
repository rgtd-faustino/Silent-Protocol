using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CameraViewUI : MonoBehaviour {
    public static CameraViewUI Instance;

    [SerializeField] private GameObject rootPanel;

    [SerializeField] private RawImage feedImage; // feed da câmara de vigilância  
    [SerializeField] private RawImage scanlineOverlay; // scan lines efeito
    [SerializeField] private RawImage staticNoiseImage; // imagem textura de noise
    [SerializeField] private RawImage chromaR;
    [SerializeField] private RawImage chromaB;

    [SerializeField] private Image switchFlash; // flash branco ao mudar de câmara

    [SerializeField] private TextMeshProUGUI cameraLabel; // "CAM-03 / F2"
    [SerializeField] private TextMeshProUGUI cameraIndexText; // "1 / 6"

    [SerializeField] private Image overuseVignette; // ecrã vermelho para indicar abuso de alternar entre câmaras
    [SerializeField] private TextMeshProUGUI overuseText;

    // para quando a câmara ainda não foi desbloqueada para mostrarmos exatamente isso ao jogador
    [SerializeField] private Image lockOverlay;
    [SerializeField] private TextMeshProUGUI lockLabel;

    [SerializeField] private TextMeshProUGUI navHint; // "[A] ANTERIOR | [D] PRÓXIMO | [E] SAIR"

    [SerializeField] private Texture2D[] noiseFrames;

    private float scanlineScrollSpeed = 0.08f; // velocidade a que as scanlines descem (baixo para não distrair demasiado)
    private float chromaMaxOffset = 0.018f; // limita o quanto as imagens dos canais vermelhos e azuis podem ser deslocados
    private float switchFlashDuration = 0.12f; // duração do flash branco ao mudar de câmara
    private int noiseFrameInterval = 3; // atualiza o noise a cada 3 frames com sinal perfeito, menos com sinal mau

    private CameraSystem system;
    private SurveillanceCamera currentCam;
    private bool isOpen = false;

    private float scanlineY = 0f;
    private int frameCount = 0;
    private int noiseIndex = 0;

    private Coroutine flashCoroutine;

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        rootPanel.SetActive(false);
    }

    void Update() {
        if (!isOpen || system == null)
            return;

        HandleInput();
        UpdateScanlines();
        UpdateNoise();
        UpdateChroma();
        UpdateHUD();
        UpdateOveruseWarning();
    }

    public void Show(CameraSystem cameraSystem) {
        system = cameraSystem;
        isOpen = true;
        rootPanel.SetActive(true);

        currentCam = system.ActiveCamera();
        ApplyCameraFeed(currentCam);
        RefreshHUDLabels();

        UpdateLockState();
    }

    public void Hide() {
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
    private void HandleInput() {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            system.PreviousCamera();

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            system.NextCamera();

        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.Escape))
            system.CloseCameraView();
    }

    // apanha a render texture da câmara e atribui-a ao feed image que é o painel que mostra o vídeo na UI
    // também prepara as cores vermelhas e azuis para distorcerem a imagem caso necessário
    private void ApplyCameraFeed(SurveillanceCamera cam) {
        feedImage.texture = cam.renderTexture;

        chromaR.texture = cam.renderTexture;
        chromaR.color = new Color(1f, 0.2f, 0.2f, 0.35f);

        chromaB.texture = cam.renderTexture;
        chromaB.color = new Color(0.2f, 0.2f, 1f, 0.35f);
    }

    private void UpdateScanlines() {
        scanlineY += scanlineScrollSpeed * Time.deltaTime;
        if (scanlineY > 1f)
            scanlineY -= 1f;

        // quanto pior o sinal, mais visíveis ficam as scanlines
        float integrity = system.signalIntegrity;
        scanlineOverlay.uvRect = new Rect(0f, scanlineY, 1f, 1f);

        Color c = scanlineOverlay.color;
        c.a = Mathf.Lerp(0.55f, 0.85f, 1f - integrity); // varia entre 55% e 85% de opacidade (nunca desaparece completamente)
        scanlineOverlay.color = c;
    }

    private void UpdateNoise() {
        frameCount++;

        // a textura de noise fica mais rápida e intensa à medida que o sinal piora
        float integrity = system.signalIntegrity;
        int interval = Mathf.Max(1, Mathf.RoundToInt(noiseFrameInterval * integrity));

        if (frameCount >= interval) {
            frameCount = 0;
            noiseIndex = (noiseIndex + 1) % noiseFrames.Length; // atualmente o jogo só tem uma textura de ruído, mas se tivesse mais isto voltava para a primeira
            staticNoiseImage.texture = noiseFrames[noiseIndex];

            // muda a UV para aparecer em posições diferentes
            float randomX = Random.Range(0f, 1f);
            float randomY = Random.Range(0f, 1f);
            staticNoiseImage.uvRect = new Rect(randomX, randomY, 1f, 1f);
        }

        Color nc = staticNoiseImage.color;
        nc.a = Mathf.Lerp(0f, 0.45f, 1f - integrity); // com sinal perfeito o noise é invisível, com sinal mau chega a 45% de opacidade
        staticNoiseImage.color = nc;
    }

    private void UpdateChroma() {
        float integrity = system.signalIntegrity;
        float heat = system.residualHeat;
        // o resíduo contribui com 30% para a distorção, menos que o sinal para não ser demasiado agressivo
        // se a integrity for 1 é perfeita e se for 0 é péssima, então ao fazer 1 - integrity invertemos o valor porque queremos o valor da distorção
        // depois somamos o resíduo 
        float distortion = Mathf.Lerp(0f, chromaMaxOffset, (1f - integrity) + heat * 0.3f);

        // este efeito faz com que dê um sentimento de analógico ou seja seno e cosseno geram valores que oscilam entre positivo e negativo constantemente
        // que dá uma sensação de tremor em vez de um deslocamento fixo
        float t = Time.time;
        float rx = Mathf.Sin(t * 1.3f) * distortion; // canal vermelho oscila a 1.3 rad/s
        float ry = Mathf.Cos(t * 0.9f) * distortion * 0.5f; // componente vertical é metade para parecer mais realista
        float bx = Mathf.Sin(t * 1.1f + 1.5f) * -distortion; // canal azul oscila na direção oposta com fase diferente (+ 1.5 rad)

        RectTransform rtR = chromaR.rectTransform;
        RectTransform rtB = chromaB.rectTransform;

        // multiplicamos pelos tamanhos dos ecrãs porque começam por um lerp então são valores pequenos
        rtR.anchoredPosition = new Vector2(rx * Screen.width, ry * Screen.height);
        rtB.anchoredPosition = new Vector2(bx * Screen.width, 0f);

        // a opacidade avança com a distorção (vermelho e azul a afastarem-se mais)
        Color cr = chromaR.color;
        cr.a = distortion / chromaMaxOffset * 0.35f;
        chromaR.color = cr;

        Color cb = chromaB.color;
        cb.a = distortion / chromaMaxOffset * 0.35f;
        chromaB.color = cb;
    }

    private void UpdateHUD() {
        cameraIndexText.text = (system.currentCameraIndex + 1) + " / " + system.CameraCount();
    }

    private void RefreshHUDLabels() {
        cameraLabel.text = currentCam.cameraLabel + "  //  F" + currentCam.floor;
    }

    // esta função segue a mesma base lógica do método UpdateChroma mas serve para deixar claro ao jogador que está a abusar das câmaras
    // então mostra a imagem de aviso e um texto caso isso aconteça
    private void UpdateOveruseWarning() {
        float heat = system.residualHeat;
        float integrity = system.signalIntegrity;
        float danger = Mathf.Clamp01((1f - integrity) * 1.5f + heat * 0.5f);

        bool highDanger = danger > 0.6f;

        Color vc = overuseVignette.color;
        vc.a = Mathf.Lerp(0f, 0.45f, danger); // vignette nunca passa de 45% para não tapar o ecrã completamente
        overuseVignette.color = vc;

        overuseText.gameObject.SetActive(highDanger);

        if (highDanger) {
            // faz piscar o texto de aviso
            Color tc = overuseText.color;
            tc.a = Mathf.Sin(Time.time * 4f * Mathf.PI) > 0f ? 1f : 0f;
            overuseText.color = tc;
            overuseText.text = "ABUSO DE CÂMARA!";
        }
    }

    private void TriggerSwitchFlash() {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    // para fazer um efeito visual de piscar o ecrã sempre que mudamos a câmara
    private IEnumerator FlashRoutine() {
        switchFlash.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < switchFlashDuration) {
            elapsed += Time.deltaTime;

            Color c = switchFlash.color;
            c.a = Mathf.Lerp(0.7f, 0f, elapsed / switchFlashDuration);
            switchFlash.color = c;

            yield return null;
        }

        switchFlash.gameObject.SetActive(false);
        flashCoroutine = null;
    }

    public void UpdateLockState() {
        if (system == null) return;
        bool locked = !system.IsUnlocked(system.currentCameraIndex);

        // oculta o feed quando locked
        feedImage.enabled = !locked;
        chromaR.enabled = !locked;
        chromaB.enabled = !locked;

        lockOverlay.gameObject.SetActive(locked);
        lockLabel.gameObject.SetActive(locked);

        // hint inclui tecla de hack quando locked
        navHint.text = "[A] PREV  |  [D] NEXT  |  [X] SAIR";
    }


}