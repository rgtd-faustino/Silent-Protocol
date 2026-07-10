using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraSystem : MonoBehaviour {
    public static CameraSystem Instance;

    public SurveillanceCamera[] allCameras; // todas as camaras de vigilancia

    private float overuseThreshold = 30f; // a partir de 30 segundos acumulados a ver câmaras a suspeita começa a subir mais depressa
    private float maxRateMultiplier = 4f; // no pior caso a suspeita sobe 4x mais depressa que o normal
    private float residualPerSession = 0.15f; // cada vez que o jogador abre as câmaras adiciona 15% de calor residual
    private float residualPerSwitch = 0.02f; // trocar de câmara adiciona 2%
    private float residualDecayRate = 0.02f; // o calor residual demora ~50 segundos a desaparecer completamente (1 / 0.02)
    private float signalRecoveryRate = 0.05f; // o sinal demora ~20 segundos a recuperar completamente (1 / 0.05)

    [HideInInspector] public int currentCameraIndex = 0;
    [HideInInspector] public bool isActive = false; // para ver se o jgoador está atualmente a ver camaras
    private float cumulativeWatchSeconds = 0f; // quantidade total de segundos vistos nas camaras durante o dia atual
    [HideInInspector] public float residualHeat = 0f; // resíduo por sessão
    [HideInInspector] public float signalIntegrity = 1f;
    [HideInInspector] public bool[] cameraUnlocked; // para dizer se a camara já está desbloeuqada ou ainda nao

    public SurveillanceCamera ActiveCamera() {
        return allCameras[currentCameraIndex];
    }

    public int CameraCount() {
        return allCameras.Length;
    }

    public void SetCameraUnlocked(bool[] data) { 
        cameraUnlocked = data; 
    }

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start() {
        // inicializar cada camara
        cameraUnlocked = new bool[allCameras.Length];

        foreach (var cam in allCameras)
            cam.Initialise();

        // reset do tempo a cada novo dia
        GameEvent.OnDayChanged += OnDayChangedHandler;
    }

    void OnDestroy() {
        GameEvent.OnDayChanged -= OnDayChangedHandler;
    }

    // função simples para resetar o tempo assistido
    void OnDayChangedHandler(int unused) {
        cumulativeWatchSeconds = 0f;
    }

    void Update() {
        if (isActive && !CameraHackPuzzle.Instance.active && IsUnlocked(currentCameraIndex)) {
            float dt = Time.deltaTime;
            cumulativeWatchSeconds += dt;

            // multiplicador linear após atingir o threshold
            float overuseFactor = Mathf.Clamp01((cumulativeWatchSeconds - overuseThreshold) / (overuseThreshold * 2f));
            float rateMultiplier = Mathf.Lerp(1f, maxRateMultiplier, overuseFactor) + residualHeat; // o resíduo adiciona por cima

            SuspicionManager.Instance.IncreaseSuspicion(Mathf.Clamp(rateMultiplier, 1f, 3f), GetInstanceID(), SuspicionManager.SuspicionSource.Camera);

            // sinal piora à medida que o jogador abusa 
            float targetIntegrity = Mathf.Lerp(1f, 0.15f, overuseFactor + residualHeat * 0.4f);
            signalIntegrity = Mathf.MoveTowards(signalIntegrity, targetIntegrity, 0.3f * dt);

        } else {
            // camara fechada, deixa de ser uma origem de suspeita e recobre o sinal e resíduo cai
            SuspicionManager.Instance.StopIncreasingSuspicion(GetInstanceID());
            signalIntegrity = Mathf.MoveTowards(signalIntegrity, 1f, signalRecoveryRate * Time.deltaTime);
            residualHeat = Mathf.MoveTowards(residualHeat, 0f, residualDecayRate * Time.deltaTime);
        }

    }

    public void OpenCameraView() {
        if (isActive) 
            return;

        isActive = true;
        residualHeat = Mathf.Min(1f, residualHeat + residualPerSession);
        // som de acesso ao computador de câmaras
        SoundManager.Instance.PlaySound(SoundManager.Instance.audioSource2DCameras, SoundManager.Instance.cameraComputer);
        CameraViewUI.Instance.Show(this);
    }

    public void CloseCameraView() {
        if (!isActive) 
            return;

        isActive = false;
        CameraViewUI.Instance.Hide();
        SoundManager.Instance.audioSource2DCameras.Stop();
        PlayerController.Instance.canMoveRotate = true;
    }

    public void NextCamera() {
        currentCameraIndex = (currentCameraIndex + 1) % allCameras.Length;

        if (IsUnlocked(currentCameraIndex))
            residualHeat = Mathf.Min(1f, residualHeat + residualPerSwitch);

        // som de trocar de câmara
        SoundManager.Instance.audioSource2D.PlayOneShot(SoundManager.Instance.cameraSwitch);

        CameraViewUI.Instance.OnCameraChanged(allCameras[currentCameraIndex]);
    }

    public void PreviousCamera() {
        currentCameraIndex = (currentCameraIndex - 1 + allCameras.Length) % allCameras.Length;

        if (IsUnlocked(currentCameraIndex))
            residualHeat = Mathf.Min(1f, residualHeat + residualPerSwitch);

        // som de trocar de câmara
        SoundManager.Instance.audioSource2D.PlayOneShot(SoundManager.Instance.cameraSwitch);

        CameraViewUI.Instance.OnCameraChanged(allCameras[currentCameraIndex]);
    }

    public void SwitchToCamera(int index) {
        if (index < 0 || index >= allCameras.Length) 
            return;

        currentCameraIndex = index;
        CameraViewUI.Instance.OnCameraChanged(allCameras[currentCameraIndex]);
    }


    public bool IsUnlocked(int index) {
        return index >= 0 && index < cameraUnlocked.Length && cameraUnlocked[index];
    }

    public void UnlockCamera(int index) {
        if (index < 0 || index >= cameraUnlocked.Length)
            return;

        cameraUnlocked[index] = true;
    }
}
