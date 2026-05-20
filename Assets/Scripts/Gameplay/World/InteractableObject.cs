using System.Collections;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [HideInInspector] public string objectName = "objeto";
    [Header("Interaction Info")]
    public string tooltipMessage = "E para interagir";

    // Arrasta aqui o GlitchMaterial criado no Unity (Create > Material, shader SilentProtocol/GlitchHighlight)
    // Adiciona tambm esse material ao MeshRenderer do objeto (slot extra, a seguir ao material base)
    [SerializeField] private Material glitchMaterial;

    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock mpb;
    private float currentIntensity = 0f;
    private Coroutine glitchCoroutine;
    private int glitchMatIndex = -1;

    protected virtual void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = GetComponentInChildren<MeshRenderer>();

        if (meshRenderer == null || glitchMaterial == null)
            return;

        // descobre em que index est o glitchMaterial no MeshRenderer
        Material[] mats = meshRenderer.sharedMaterials;
        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] == glitchMaterial)
            {
                glitchMatIndex = i;
                break;
            }
        }

        if (glitchMatIndex == -1)
        {
            Debug.LogWarning($"[{gameObject.name}] GlitchMaterial no est na lista de materiais do MeshRenderer. " +
                             "Adiciona-o no Inspector (MeshRenderer > Materials > +).");
            return;
        }

        InjectBarycentricCoords();

        mpb = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(mpb, glitchMatIndex);
        mpb.SetFloat("_Intensity", 0f);
        meshRenderer.SetPropertyBlock(mpb, glitchMatIndex);

        StartCoroutine(GlowUpdateRoutine());
    }

    public virtual void Interact()
    {
        Debug.Log($"{objectName} sofreu interao");
    }

    public void ShowGlitch()
    {
        if (meshRenderer == null || glitchMatIndex == -1) return;
        Swap(1f);
    }

    public void HideGlitch()
    {
        if (meshRenderer == null || glitchMatIndex == -1) return;
        Swap(0f);
    }

    private float targetIntensity = 0f;

    private void Swap(float target)
    {
        if (Mathf.Approximately(targetIntensity, target)) return;
        targetIntensity = target;
        if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
        glitchCoroutine = StartCoroutine(Fade(target));
    }

    private IEnumerator Fade(float target)
    {
        while (Mathf.Abs(currentIntensity - target) > 0.005f)
        {
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

    protected virtual bool CheckShouldGlowByDefault()
    {
        return false;
    }

    private IEnumerator GlowUpdateRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.3f);
        while (true)
        {
            UpdateTaskGlow();
            yield return wait;
        }
    }

    private void UpdateTaskGlow()
    {
        if (meshRenderer == null || glitchMatIndex == -1) return;

        bool shouldGlow = CheckShouldGlowByDefault();
        if (shouldGlow)
        {
            ShowGlitch();
        }
        else
        {
            if (CameraScript.Instance.currentTarget != this)
            {
                HideGlitch();
            }
        }
    }

    private void InjectBarycentricCoords()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) mf = GetComponentInChildren<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        Mesh src = mf.sharedMesh;
        int[] srcTris = src.triangles;
        int triCount = srcTris.Length;          // 1 vrtice por ndice de tringulo

        Vector3[] srcVerts = src.vertices;
        Vector3[] srcNorms = src.normals;
        Vector2[] srcUVs = src.uv;

        Vector3[] newVerts = new Vector3[triCount];
        Vector3[] newNorms = new Vector3[triCount];
        Vector2[] newUVs = new Vector2[triCount];
        Vector3[] newBary = new Vector3[triCount];
        int[] newTris = new int[triCount];

        Vector3[] baryTable = { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) };

        for (int i = 0; i < triCount; i++)
        {
            newVerts[i] = srcVerts[srcTris[i]];
            newNorms[i] = srcNorms.Length > srcTris[i] ? srcNorms[srcTris[i]] : Vector3.up;
            newUVs[i] = srcUVs.Length > srcTris[i] ? srcUVs[srcTris[i]] : Vector2.zero;
            newBary[i] = baryTable[i % 3];
            newTris[i] = i;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // suporta meshes grandes
        mesh.vertices = newVerts;
        mesh.normals = newNorms;
        mesh.uv = newUVs;
        mesh.triangles = newTris;
        mesh.SetUVs(1, newBary);
        mesh.RecalculateBounds();

        mf.mesh = mesh;
    }
}