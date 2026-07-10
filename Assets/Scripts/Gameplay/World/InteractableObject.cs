using System.Collections;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [HideInInspector] public string objectName = "objeto";
    [Header("Interaction Info")]
    public string tooltipMessage = "E para interagir";

    // a referência do material de glitch tem de ser arrastada à mão no Inspector para cada prefab. isto fica no slot extra do MeshRenderer
    [SerializeField] private Material glitchMaterial;

    private MeshRenderer meshRenderer;
    // instanciamos o PropertyBlock no Awake para alterar o _Intensity do shader sem forçar o Unity a clonar o material completo
    private MaterialPropertyBlock mpb;
    private float currentIntensity = 0f;
    private Coroutine glitchCoroutine;
    // guardamos logo o index do glitchMaterial porque fazer buscas ao array no Update ia rebentar com a performance
    private int glitchMatIndex = -1;

    // reconstruímos a mesh logo no início para enfiar as coordenadas baricêntricas. é impossível usar este shader de wireframe sem isto
    protected virtual void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = GetComponentInChildren<MeshRenderer>();

        if (meshRenderer == null || glitchMaterial == null)
            return;

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
            Debug.LogWarning($"[{gameObject.name}] GlitchMaterial não está na lista de materiais do MeshRenderer. " +
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

    // método genérico. deixamos virtual para que cenas específicas tipo camas ou fechaduras possam reescrever a lógica de interação
    public virtual void Interact()
    {
        Debug.Log($"{objectName} sofreu interação");
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

    // interseta a coroutine de fade anterior para garantir que a transição de intensidade não passa por cima de si mesma se a câmara mudar bué rápido
    private void Swap(float target)
    {
        if (Mathf.Approximately(targetIntensity, target)) return;
        targetIntensity = target;
        if (glitchCoroutine != null) StopCoroutine(glitchCoroutine);
        glitchCoroutine = StartCoroutine(Fade(target));
    }

    // iteramos a variável com um Lerp pesado no delta time. isto cria um efeito fluido na intensidade do material PropertyBlock
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

    // decidimos usar uma coroutine espaçada a 0.3s em vez do Update normal para poupar processamento. só precisamos de ver para onde a câmara olha
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

    // algoritmo bué complexo mas a matemática é direta. espetamos com os vetores 1,0,0 nos UVs extra da mesh para o Unity conseguir pintar as arestas no shader
    private void InjectBarycentricCoords()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) mf = GetComponentInChildren<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        Mesh src = mf.sharedMesh;
        int[] srcTris = src.triangles;
        int triCount = srcTris.Length;

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
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = newVerts;
        mesh.normals = newNorms;
        mesh.uv = newUVs;
        mesh.triangles = newTris;
        mesh.SetUVs(1, newBary);
        mesh.RecalculateBounds();

        mf.mesh = mesh;
    }
}