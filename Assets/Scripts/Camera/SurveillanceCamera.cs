using UnityEngine;

[RequireComponent(typeof(Camera))]
public class SurveillanceCamera : MonoBehaviour
{
    [Header("Identity")]
    public string cameraLabel = "CAM-01";
    public int floor = 1;

    [Header("Render Texture")]
    [Tooltip("Assign a RenderTexture asset (e.g. 512×512). The UI will read from it.")]
    public RenderTexture renderTexture;

    [Header("Patrol Pan")]
    [Tooltip("The transform this camera's Y-rotation pivots around (usually this transform).")]
    [SerializeField] private Transform pivotTransform;

    [Tooltip("Horizontal sweep range in degrees from the rest angle.")]
    [SerializeField] private float panRange = 40f;

    [Tooltip("Seconds for a full sweep (left to right).")]
    [SerializeField] private float panDuration = 12f;
    [Tooltip("If false, the camera stays locked at its rest angle.")]
    [SerializeField] public bool panEnabled = true;

    private Camera cam;
    private float baseYRotation;    // rest angle (set in Awake from initial rotation)
    private float panPhase = 0f;    // [0-1] drives a sine sweep
    private bool _initialised = false;

    public void Initialise()
    {
        cam = GetComponent<Camera>();

        if (renderTexture != null)
            cam.targetTexture = renderTexture;

        if (pivotTransform == null)
            pivotTransform = transform;

        baseYRotation = pivotTransform.eulerAngles.y;
        _initialised = true;
    }

    void Update()
    {
        if (!_initialised) return;
        UpdatePan();
    }

    private void UpdatePan() {
        if (!panEnabled) {
            // snap de volta ao centro se o pan for desligado em runtime
            Vector3 rest = pivotTransform.eulerAngles;
            rest.y = Mathf.LerpAngle(rest.y, baseYRotation, Time.deltaTime * 2f);
            pivotTransform.eulerAngles = rest;
            return;
        }

        // panPhase começa a 0 → sin(0) = 0 → arranca sempre do centro
        panPhase += Time.deltaTime / panDuration;
        float sine = Mathf.Sin(panPhase * Mathf.PI * 2f);
        float targetY = baseYRotation + sine * panRange;
        Vector3 e = pivotTransform.eulerAngles;
        e.y = Mathf.LerpAngle(e.y, targetY, Time.deltaTime * 2f);
        pivotTransform.eulerAngles = e;
    }

}
