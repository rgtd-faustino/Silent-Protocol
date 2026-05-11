using System.Collections;
using UnityEngine;

public class InteractableObject : MonoBehaviour {
    [HideInInspector] public string objectName = "objeto";

    // Arrasta aqui o GlitchMaterial criado no Unity (Create > Material, shader SilentProtocol/GlitchHighlight)
    // Adiciona também esse material ao MeshRenderer do objeto (slot extra, a seguir ao material base)
    [SerializeField] private Material glitchMaterial;

    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock mpb;
    private float currentIntensity = 0f;
    private Coroutine glitchCoroutine;
    private int glitchMatIndex = -1;

    void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = GetComponentInChildren<MeshRenderer>();

        if (meshRenderer == null || glitchMaterial == null)
            return;

        // descobre em que index está o glitchMaterial no MeshRenderer
        Material[] mats = meshRenderer.sharedMaterials;
        for (int i = 0; i < mats.Length; i++) {
            if (mats[i] == glitchMaterial) {
                glitchMatIndex = i;
                break;
            }
        }

        if (glitchMatIndex == -1) {
            Debug.LogWarning($"[{gameObject.name}] GlitchMaterial não está na lista de materiais do MeshRenderer. " +
                             "Adiciona-o no Inspector (MeshRenderer > Materials > +).");
            return;
        }

        InjectBarycentricCoords();

        mpb = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(mpb, glitchMatIndex);
        mpb.SetFloat("_Intensity", 0f);
        meshRenderer.SetPropertyBlock(mpb, glitchMatIndex);
    }

    public virtual void Interact() {
        Debug.Log($"{objectName} sofreu interação");
    }

    public void ShowGlitch() {
        if (meshRenderer == null || glitchMatIndex == -1) return;
        Swap(1f);
    }

    public void HideGlitch() {
        if (meshRenderer == null || glitchMatIndex == -1) return;
        Swap(0f);
    }

    private void Swap(float target) {
        if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
        glitchCoroutine = StartCoroutine(Fade(target));
    }

    private IEnumerator Fade(float target) {
        while (Mathf.Abs(currentIntensity - target) > 0.005f) {
            currentIntensity = Mathf.Lerp(currentIntensity, target, Time.deltaTime * 9f);
            mpb.SetFloat("_Intensity", currentIntensity);
            meshRenderer.SetPropertyBlock(mpb, glitchMatIndex);
            yield return null;
        }
        currentIntensity = target;
        mpb.SetFloat("_Intensity", currentIntensity);
        meshRenderer.SetPropertyBlock(mpb, glitchMatIndex);
        glitchCoroutine = null;
    }

    private void InjectBarycentricCoords() {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) mf = GetComponentInChildren<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        Mesh mesh = Instantiate(mf.sharedMesh);
        int[] tris = mesh.triangles;
        Vector3[] bary = new Vector3[mesh.vertexCount];

        for (int i = 0; i < tris.Length; i += 3) {
            if (bary[tris[i]].sqrMagnitude < 0.01f) bary[tris[i]] = new Vector3(1, 0, 0);
            if (bary[tris[i + 1]].sqrMagnitude < 0.01f) bary[tris[i + 1]] = new Vector3(0, 1, 0);
            if (bary[tris[i + 2]].sqrMagnitude < 0.01f) bary[tris[i + 2]] = new Vector3(0, 0, 1);
        }

        mesh.SetUVs(1, bary);
        mf.mesh = mesh;
    }
}