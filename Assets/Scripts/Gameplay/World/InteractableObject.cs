using System.Collections;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.InputManagerEntry;

public class InteractableObject : MonoBehaviour {
    [HideInInspector] public string objectName = "objeto";
    public string tooltipMessage = "E para interagir";

    // material com o shader de "glitch"/wireframe que é usado como highlight visual quando o jogador aponta para este objeto
    [SerializeField] private Material glitchMaterial;

    private MeshRenderer meshRenderer;

    // um MaterialPropertyBlock permite mudar uma propriedade (neste caso _Intensity) só neste objeto, sem clonar o material inteiro
    // se não usássemos isto, o Unity criava uma cópia do material sempre que mudássemos uma propriedade via código
    private MaterialPropertyBlock mpb;

    // 0 = invisível, 1 = totalmente visível
    private float currentIntensity = 0f;
    // valor de intensidade para onde estamos a fazer fade neste momento (usado para saber se já estamos a ir para lá, e evitar reiniciar o fade à toa)
    private float targetIntensity = 0f;

    private Coroutine glitchCoroutine;

    // índice do glitchMaterial dentro do array de materiais do MeshRenderer (um objeto pode ter vários materiais/submeshes)
    private int glitchMatIndex = -1;

    // "virtual" (em vez de um método normal) permite que classes filhas façam override deste Awake e corram lógica extra própria
    // sem perder esta inicialização desde que chamem base.Awake() dentro do override delas
    protected virtual void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
            meshRenderer = GetComponentInChildren<MeshRenderer>();

        if (meshRenderer == null || glitchMaterial == null)
            return;

        // percorremos os materiais do objeto à procura de qual slot corresponde ao glitchMaterial
        Material[] mats = meshRenderer.sharedMaterials;
        for (int i = 0; i < mats.Length; i++) {
            if (mats[i] == glitchMaterial) {
                glitchMatIndex = i;
                break;
            }
        }

        // se o material não estiver lá avisamos
        if (glitchMatIndex == -1) {
            Debug.LogWarning($"[{gameObject.name}] GlitchMaterial não está na lista de materiais do MeshRenderer. " +
                             "Adiciona-o no Inspector (MeshRenderer > Materials > +).");
            return;
        }

        // o shader de wireframe precisa de coordenadas baricêntricas por vértice para saber desenhar as arestas do triângulo
        // a mesh original não tem essa informação, por isso reconstruímo-la já com esses dados extra
        InjectBarycentricCoords();

        // criamos o PropertyBlock e inicializamos a intensidade do glitch a 0 (invisível), para o objeto começar "normal"
        mpb = new MaterialPropertyBlock();
        meshRenderer.GetPropertyBlock(mpb, glitchMatIndex);
        mpb.SetFloat("_Intensity", 0f);
        meshRenderer.SetPropertyBlock(mpb, glitchMatIndex);

    }
    //Se algum ou outro InteractableObject tiver o Awake() executado antes do Awake() do CameraScript correr e definir Instance = this,
    //então CameraScript.Instance ainda é null nesse instante, por isso comecamos a StartCoroutine no Start() em vez do Awake() para garantir
    //que a corrotina só começa depois do CameraScript estar pronto
    protected virtual void Start()
    {
        if (meshRenderer == null || glitchMatIndex == -1)
            return;

        StartCoroutine(GlowUpdateRoutine());
    }

    // comportamento base de interação, objetos filho fazem override disto para terem a sua própria lógica específica
    public virtual void Interact() {
        Debug.Log($"{objectName} sofreu interação");
    }

    // chamado pelo CameraScript quando o raycast do jogador está a apontar para este objeto
    public void ShowGlitch() {
        if (meshRenderer == null || glitchMatIndex == -1) 
            return;
        Swap(1f);
    }

    // chamado quando o jogador deixa de apontar para o objeto
    public void HideGlitch() {
        if (meshRenderer == null || glitchMatIndex == -1) 
            return;
        Swap(0f);
    }

    // inicia (ou reinicia) a animação de fade para o valor alvo pretendido
    private void Swap(float target) {
        // se já estamos a ir para este valor (ou já lá estamos), não faz sentido reiniciar a corrotina de fade
        if (Mathf.Approximately(targetIntensity, target))
            return;

        targetIntensity = target;

        // se a câmara mudar de alvo muito depressa (ex: passar rapidamente por vários objetos), cancelamos o fade anterior
        // antes de começar um novo, para não termos duas corrotinas a competir e a "puxar" a intensidade em direções diferentes
        if (glitchCoroutine != null)
            StopCoroutine(glitchCoroutine);

        glitchCoroutine = StartCoroutine(Fade(target));
    }

    // anima currentIntensity suavemente até "target" usando Lerp a cada frame para criar uma transição fluida
    private IEnumerator Fade(float target) {
        // continuamos a interpolar enquanto a diferença for percetível (>0.005), para evitar loop infinito por causa de imprecisão de float
        while (Mathf.Abs(currentIntensity - target) > 0.005f) {
            // Time.deltaTime * 9f controla a "velocidade" do fade
            currentIntensity = Mathf.Lerp(currentIntensity, target, Time.deltaTime * 9f);
            mpb.SetFloat("_Intensity", currentIntensity);
            meshRenderer.SetPropertyBlock(mpb, glitchMatIndex);
            yield return null;
        }

        // ao sair do loop, forçamos o valor exato final (o Lerp nunca chega a bater certo, só se aproxima)
        currentIntensity = target;
        mpb.SetFloat("_Intensity", currentIntensity);
        meshRenderer.SetPropertyBlock(mpb, glitchMatIndex);
        glitchCoroutine = null;
    }

    // por defeito nenhum objeto brilha sozinho, só brilha quando a câmara aponta para ele
    // classes filhas como ArchiveScript fazem override disto para o objeto brilhar sempre que certa condição for verdadeira
    // (ex: o arquivo brilha sempre que o jogador tem um documento na mão, mesmo sem estar a olhar diretamente para ele)
    protected virtual bool CheckShouldGlowByDefault() {
        return false;
    }

    // corrotina que verifica a cada 0.3s (em vez de todas as frames via Update) se este objeto deve estar a brilhar
    // 0.3s é suficientemente frequente para parecer responsivo, mas poupa muito processamento comparado a correr isto every frame
    private IEnumerator GlowUpdateRoutine() {
        WaitForSeconds wait = new WaitForSeconds(0.3f);
        while (true) {
            UpdateTaskGlow();
            yield return wait;
        }
    }

    private void UpdateTaskGlow() {
        if (meshRenderer == null || glitchMatIndex == -1) return;

        bool shouldGlow = CheckShouldGlowByDefault();
        if (shouldGlow) {
            ShowGlitch();

        } else {
            if (CameraScript.Instance.currentTarget != this) {
                HideGlitch();
            }
        }
    }

    // o shader de wireframe usado no glitch precisa de saber, para cada vértice de cada triângulo, a sua posição
    // "baricêntrica" dentro desse triângulo (ex: canto = (1,0,0), (0,1,0) ou (0,0,1)) para conseguir desenhar as arestas
    // um mesh normal partilha vértices entre triângulos vizinhos (para poupar memória), o que impossibilita dar a cada
    // vértice uma coordenada baricêntrica única por triângulo, por isso duplicamos todos os vértices, um conjunto
    // por cada triângulo, para que cada vértice pertença a um único triângulo e possa ter a sua própria coordenada
    private void InjectBarycentricCoords() {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null) mf = GetComponentInChildren<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        Mesh src = mf.sharedMesh;
        int[] srcTris = src.triangles; // lista de índices de vértices, agrupados de 3 em 3 (cada grupo = um triângulo)
        int idxCount = srcTris.Length; // número total de índices

        Vector3[] srcVerts = src.vertices;
        Vector3[] srcNorms = src.normals;
        Vector2[] srcUVs = src.uv;

        // arrays novos, um elemento por cada "vértice duplicado" (um por cada aparição num triângulo)
        Vector3[] newVerts = new Vector3[idxCount];
        Vector3[] newNorms = new Vector3[idxCount];
        Vector2[] newUVs = new Vector2[idxCount];
        Vector3[] newBary = new Vector3[idxCount]; // as coordenadas baricêntricas propriamente ditas
        int[] newTris = new int[idxCount];

        // as 3 coordenadas possíveis: cada vértice de um triângulo recebe uma destas, uma para cada "canto"
        Vector3[] baryTable = { new Vector3(1, 0, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1) };

        for (int i = 0; i < idxCount; i++) {
            // vamos buscar o vértice original que este índice representa, e copiamo-lo para a posição i (duplicando-o)
            newVerts[i] = srcVerts[srcTris[i]];
            newNorms[i] = srcNorms.Length > srcTris[i] ? srcNorms[srcTris[i]] : Vector3.up;
            newUVs[i] = srcUVs.Length > srcTris[i] ? srcUVs[srcTris[i]] : Vector2.zero;

            // i % 3 dá-nos 0, 1, 2, 0, 1, 2... — ou seja, a posição deste vértice dentro do triângulo atual (1º, 2º ou 3º canto)
            newBary[i] = baryTable[i % 3];

            // como duplicámos tudo, a nova lista de triângulos é simplesmente sequencial (0,1,2,3,4,5...)
            newTris[i] = i;
        }

        Mesh mesh = new Mesh();
        // UInt32 em vez do UInt16 porque a duplicação de vértices pode facilmente ultrapassar o limite de 65535 vértices
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = newVerts;
        mesh.normals = newNorms;
        mesh.uv = newUVs;
        mesh.triangles = newTris;
        mesh.SetUVs(1, newBary); // guardamos as coordenadas baricêntricas no canal de UV extra (UV1), que o shader depois lê
        mesh.RecalculateBounds();

        mf.mesh = mesh; // substituímos a mesh original (partilhada) por esta cópia própria e modificada deste objeto
    }
}