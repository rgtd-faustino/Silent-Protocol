using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CameraSystem : MonoBehaviour {
    public static CameraSystem Instance;

    [Header("Camera Registry")]
    [Tooltip("All SurveillanceCamera components in the scene. Populate via Inspector.")]
    [SerializeField] private SurveillanceCamera[] allCameras;

    [Header("Suspicion Accrual")]
    [Tooltip("Base suspicion per second while the camera view is open.")]
    private float baseSuspicionPerSecond = 0.008f;

    [Tooltip("After this many cumulative seconds of camera usage, the rate multiplier starts climbing.")]
    private float overuseThreshold = 30f;

    [Tooltip("Maximum multiplier applied to the suspicion rate (reached at 3× overuseThreshold).")]
    private float maxRateMultiplier = 4f;

    [Tooltip("Each session (open → close) adds this residual. Residual decays slowly when camera is closed.")]
    private float residualPerSession = 0.15f;

    [Tooltip("Each camera switch adds this residual heat.")]
    private float residualPerSwitch = 0.02f;

    [Tooltip("Rate at which residual heat decays per second when camera is closed.")]
    private float residualDecayRate = 0.02f;

    [Header("Signal Integrity")]
    [Tooltip("Signal integrity [0-1]. Drops with overuse, recovers when camera is closed.")]
    private float signalRecoveryRate = 0.05f;

    private int currentCameraIndex = 0;
    private bool isActive = false;             // is the player currently watching cameras?
    private float cumulativeWatchSeconds = 0f; // total seconds ever watched this day
    private float residualHeat = 0f;           // session residual, [0-1]
    private float signalIntegrity = 1f;        // public read via property
    private bool[] cameraUnlocked;             // per-camera lock state [false = locked]

    public bool IsActive => isActive;

    public float SignalIntegrity => signalIntegrity;

    public float ResidualHeat => residualHeat;

    public SurveillanceCamera ActiveCamera => (allCameras != null && allCameras.Length > 0) ? allCameras[currentCameraIndex] : null;

    public int CameraCount => allCameras?.Length ?? 0;
    public int CurrentCameraIndex => currentCameraIndex;

    void Awake() {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start() {
        // Initialise each camera
        if (allCameras != null) {
            cameraUnlocked = new bool[allCameras.Length];
            foreach (var cam in allCameras)
                cam.Initialise();
        }

        // Reset watch time each new day
        GameEvent.OnDayChanged += _ => ResetDailyUsage();
    }

    void OnDestroy() {
        GameEvent.OnDayChanged -= _ => ResetDailyUsage();
    }

    void Update() {
        if (isActive && !CameraHackPuzzle.Instance.IsOpen && IsUnlocked(currentCameraIndex)) {
            float dt = Time.deltaTime;
            cumulativeWatchSeconds += dt;

            // Rate multiplier: linear ramp past the threshold
            float overuseFactor = Mathf.Clamp01((cumulativeWatchSeconds - overuseThreshold) / (overuseThreshold * 2f));
            float rateMultiplier = Mathf.Lerp(1f, maxRateMultiplier, overuseFactor) + residualHeat;          // residual heat adds on top

            // Suspicion
            float tick = baseSuspicionPerSecond * rateMultiplier * dt;
            SuspicionManager.Instance.IncreaseSuspicion(
                Mathf.Clamp(rateMultiplier, 1f, 3f),
                GetInstanceID(),
                SuspicionManager.SuspicionSource.Camera);

            // Signal degrades as overuse grows
            float targetIntegrity = Mathf.Lerp(1f, 0.15f, overuseFactor + residualHeat * 0.4f);
            signalIntegrity = Mathf.MoveTowards(signalIntegrity, targetIntegrity, 0.3f * dt);

        } else {
            // Camera closed — stop suspicion source, recover signal & decay residual
            SuspicionManager.Instance.StopIncreasingSuspicion(GetInstanceID());

            signalIntegrity = Mathf.MoveTowards(signalIntegrity, 1f, signalRecoveryRate * Time.deltaTime);
            residualHeat = Mathf.MoveTowards(residualHeat, 0f, residualDecayRate * Time.deltaTime);
        }

    }

    public void OpenCameraView() {
        if (isActive) return;
        isActive = true;
        residualHeat = Mathf.Min(1f, residualHeat + residualPerSession);
        CameraViewUI.Instance.Show(this);
    }

    public void CloseCameraView() {
        if (!isActive) return;
        isActive = false;
        CameraViewUI.Instance.Hide();
        PlayerController.Instance.canMoveRotate = true;
    }

    public void NextCamera() {
        currentCameraIndex = (currentCameraIndex + 1) % allCameras.Length;
        if (IsUnlocked(currentCameraIndex))
            residualHeat = Mathf.Min(1f, residualHeat + residualPerSwitch);
        CameraViewUI.Instance.OnCameraChanged(allCameras[currentCameraIndex]);
    }

    public void PreviousCamera() {
        currentCameraIndex = (currentCameraIndex - 1 + allCameras.Length) % allCameras.Length;
        if (IsUnlocked(currentCameraIndex))
            residualHeat = Mathf.Min(1f, residualHeat + residualPerSwitch);
        CameraViewUI.Instance.OnCameraChanged(allCameras[currentCameraIndex]);
    }

    public void SwitchToCamera(int index) {
        if (allCameras == null || index < 0 || index >= allCameras.Length) return;
        currentCameraIndex = index;
        CameraViewUI.Instance.OnCameraChanged(allCameras[currentCameraIndex]);
    }

    private void ResetDailyUsage() {
        cumulativeWatchSeconds = 0f;
        // residual heat persists across days to simulate accumulating paranoia
    }

    public bool IsUnlocked(int index) =>
        cameraUnlocked != null && index >= 0 && index < cameraUnlocked.Length && cameraUnlocked[index];

    public void UnlockCamera(int index) {
        if (cameraUnlocked == null || index < 0 || index >= cameraUnlocked.Length) return;
        cameraUnlocked[index] = true;
    }
}
