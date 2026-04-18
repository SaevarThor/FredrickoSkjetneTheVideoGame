using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Attach to your Main Camera.
/// Applies PS2-era visual effects: low resolution rendering, dithering,
/// color banding, and scanlines via a fullscreen blit.
/// Works with Unity's URP pipeline.
/// </summary>
[RequireComponent(typeof(Camera))]
public class PS2PostProcess : MonoBehaviour
{
    [Header("Resolution")]
    [Tooltip("Render at this internal resolution then upscale — gives the chunky PS2 pixel look")]
    [SerializeField] private Vector2Int internalResolution = new Vector2Int(480, 272); // PSP/PS2 native-ish

    [Header("Color")]
    [SerializeField, Range(2, 32)] private int colorBands = 16;     // Quantize color depth
    [SerializeField, Range(0f, 1f)] private float ditherStrength = 0.4f;

    [Header("Scanlines")]
    [SerializeField] private bool enableScanlines = true;
    [SerializeField, Range(0f, 1f)] private float scanlineStrength = 0.15f;
    [SerializeField, Range(1, 4)] private int scanlineThickness = 1;

    [Header("Geometry Wobble")]
    [Tooltip("Jitters vertices — applied via PS2VertexJitter shader on materials")]
    [SerializeField, Range(0f, 1f)] public float wobbleIntensity = 0.003f;

    private Material _ps2Material;
    private RenderTexture _lowResRT;

    private static readonly int ColorBandsProp     = Shader.PropertyToID("_ColorBands");
    private static readonly int DitherStrengthProp = Shader.PropertyToID("_DitherStrength");
    private static readonly int ScanlinesProp      = Shader.PropertyToID("_ScanlineStrength");
    private static readonly int ScanlineThickProp  = Shader.PropertyToID("_ScanlineThickness");
    private static readonly int ResolutionProp     = Shader.PropertyToID("_Resolution");

    private void Awake()
    {
        // Create material from shader (shader file below)
        Shader shader = Shader.Find("Custom/PS2PostProcess");
        if (shader == null)
        {
            Debug.LogError("[PS2PostProcess] Could not find 'Custom/PS2PostProcess' shader. " +
                           "Make sure PS2PostProcess.shader is in your project.");
            enabled = false;
            return;
        }
        _ps2Material = new Material(shader);
        CreateRenderTexture();
    }

    private void CreateRenderTexture()
    {
        if (_lowResRT != null)
            _lowResRT.Release();

        _lowResRT = new RenderTexture(internalResolution.x, internalResolution.y, 24);
        _lowResRT.filterMode = FilterMode.Point; // No bilinear smoothing — keep pixels crisp
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (_ps2Material == null) { Graphics.Blit(src, dest); return; }

        // Pass settings to shader
        _ps2Material.SetFloat(ColorBandsProp, colorBands);
        _ps2Material.SetFloat(DitherStrengthProp, ditherStrength);
        _ps2Material.SetFloat(ScanlinesProp, enableScanlines ? scanlineStrength : 0f);
        _ps2Material.SetFloat(ScanlineThickProp, scanlineThickness);
        _ps2Material.SetVector(ResolutionProp, new Vector4(internalResolution.x, internalResolution.y, 0, 0));

        // Downscale → upscale with point filtering for pixelated look
        Graphics.Blit(src, _lowResRT);
        Graphics.Blit(_lowResRT, dest, _ps2Material);
    }

    private void OnDestroy()
    {
        if (_lowResRT != null)
            _lowResRT.Release();
        if (_ps2Material != null)
            Destroy(_ps2Material);
    }

    // Expose wobble to vertex shader via global property
    private void Update()
    {
        Shader.SetGlobalFloat("_PS2WobbleIntensity", wobbleIntensity);
        Shader.SetGlobalFloat("_PS2Time", Time.time);
    }
}
