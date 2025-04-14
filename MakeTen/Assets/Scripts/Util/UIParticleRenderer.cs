using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(CanvasRenderer))]
public class UIParticleRenderer : MaskableGraphic
{
    [SerializeField] private ParticleSystem particleSystemPrefab;
    private ParticleSystem particleSystemInstance;
    private ParticleSystemRenderer particleRenderer;
    private ParticleSystem.Particle[] particles;

    [Header("Simulation Settings")]
    [SerializeField] private float simulationSpeed = 1.0f;

    private bool needsRedraw = false;

    protected override void Awake()
    {
        base.Awake();
        InitializeParticleSystem();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        //InitializeParticleSystem();
        //needsRedraw = true;

        Canvas.willRenderCanvases += OnWillRenderCanvases;
        InitializeParticleSystem();
        needsRedraw = true;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        //if (particleSystemInstance != null)
        //    particleSystemInstance.gameObject.SetActive(false);

        Canvas.willRenderCanvases -= OnWillRenderCanvases;
        if (particleSystemInstance != null)
            particleSystemInstance.gameObject.SetActive(false);
    }

    protected override void OnDestroy()
    {
        if (Application.isPlaying && particleSystemInstance != null)
            Destroy(particleSystemInstance.gameObject);
        else if (particleSystemInstance != null)
            DestroyImmediate(particleSystemInstance.gameObject);
    }

    private void InitializeParticleSystem()
    {
        if (particleSystemPrefab == null || particleSystemInstance != null)
            return;

        particleSystemInstance = Instantiate(particleSystemPrefab, transform);
        particleSystemInstance.gameObject.hideFlags = HideFlags.DontSave;
        particleRenderer = particleSystemInstance.GetComponent<ParticleSystemRenderer>();
        particleRenderer.enabled = false; // World Renderer 비활성화
        particles = new ParticleSystem.Particle[particleSystemInstance.main.maxParticles];
    }

    void LateUpdate()
    {
        if (particleSystemInstance == null || !particleSystemInstance.IsAlive())
            return;

        float deltaTime = Application.isPlaying ? Time.unscaledDeltaTime : Time.deltaTime;

        particleSystemInstance.Simulate(deltaTime * simulationSpeed, true, false);
        particleSystemInstance.GetParticles(particles);

        needsRedraw = true;
    }
    //private void OnEnable()
    //{
    //    Canvas.willRenderCanvases += OnWillRenderCanvases;
    //    InitializeParticleSystem();
    //    needsRedraw = true;
    //}

    //private void OnDisable()
    //{
    //    Canvas.willRenderCanvases -= OnWillRenderCanvases;
    //    if (particleSystemInstance != null)
    //        particleSystemInstance.gameObject.SetActive(false);
    //}

    private void OnWillRenderCanvases()
    {
        if (needsRedraw)
        {
            SetVerticesDirty();
            needsRedraw = false;
        }
    }
    public override void Rebuild(CanvasUpdate update)
    {
        base.Rebuild(update);

        //if (update == CanvasUpdate.PreRender && needsRedraw)
        //{
        //    SetVerticesDirty();
        //    needsRedraw = false;
        //}
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (particleSystemInstance == null || particles == null)
            return;

        var main = particleSystemInstance.main;
        var texture = main.startColor.mode == ParticleSystemGradientMode.TwoColors
            ? material.mainTexture
            : material.mainTexture;

        if (texture == null) return;

        Vector4 uv = new Vector4(0, 0, 1, 1);
        Vector3 scale = transform.lossyScale;

        for (int i = 0; i < particles.Length; i++)
        {
            ParticleSystem.Particle particle = particles[i];
            if (particle.remainingLifetime <= 0)
                continue;

            Vector3 position = particle.position;
            float size = particle.GetCurrentSize(particleSystemInstance);
            Color32 color = particle.GetCurrentColor(particleSystemInstance);

            AddQuad(vh, position, size * 0.5f, color, uv);
        }
    }

    private void AddQuad(VertexHelper vh, Vector3 position, float halfSize, Color32 color, Vector4 uv)
    {
        int vertIndex = vh.currentVertCount;

        Vector3 bottomLeft = position + new Vector3(-halfSize, -halfSize);
        Vector3 topLeft = position + new Vector3(-halfSize, halfSize);
        Vector3 topRight = position + new Vector3(halfSize, halfSize);
        Vector3 bottomRight = position + new Vector3(halfSize, -halfSize);

        vh.AddVert(bottomLeft, color, new Vector2(uv.x, uv.y));
        vh.AddVert(topLeft, color, new Vector2(uv.x, uv.w));
        vh.AddVert(topRight, color, new Vector2(uv.z, uv.w));
        vh.AddVert(bottomRight, color, new Vector2(uv.z, uv.y));

        vh.AddTriangle(vertIndex, vertIndex + 1, vertIndex + 2);
        vh.AddTriangle(vertIndex, vertIndex + 2, vertIndex + 3);
    }

    public override Texture mainTexture
    {
        get
        {
            if (particleSystemInstance != null)
            {
                var textureSheet = particleSystemInstance.textureSheetAnimation;
                if (textureSheet.enabled && textureSheet.spriteCount > 0)
                {
                    return textureSheet.GetSprite(0).texture;
                }
            }
            return base.mainTexture ?? Texture2D.whiteTexture;
        }
    }
}
