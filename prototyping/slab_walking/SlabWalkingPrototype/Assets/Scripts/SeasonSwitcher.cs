
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine;

public class SeasonSwitcher : MonoBehaviour
{
    public enum Season { Winter, Spring, Summer, Autumn }

    [Header("Ground & Material")]
    [SerializeField] private Renderer groundRenderer;   // Assign your plane's Renderer
    [SerializeField] private float blendDuration = 2.5f; // seconds for cross-fade

    [Header("Season Albedos")]
    [SerializeField] private Texture2D winterAlbedo;
    [SerializeField] private Texture2D springAlbedo;
    [SerializeField] private Texture2D summerAlbedo;
    [SerializeField] private Texture2D autumnAlbedo;

    [Header("Optional Season Normal Maps")]
    [SerializeField] private Texture2D winterNormal;
    [SerializeField] private Texture2D springNormal;
    [SerializeField] private Texture2D summerNormal;
    [SerializeField] private Texture2D autumnNormal;
    [SerializeField] private bool useNormals = false;

    [Header("Prefabs")]
    [SerializeField] private GameObject SpringGrasses;
    [SerializeField] private GameObject SummerGrasses;
    [SerializeField] private GameObject AutumnGrasses;

    private static readonly int _SeasonA = Shader.PropertyToID("_SeasonA");
    private static readonly int _SeasonB = Shader.PropertyToID("_SeasonB");
    private static readonly int _Blend = Shader.PropertyToID("_Blend");
    private static readonly int _NormalA = Shader.PropertyToID("_NormalA");
    private static readonly int _NormalB = Shader.PropertyToID("_NormalB");
    private MaterialPropertyBlock _mpb;
    private Season _current = Season.Winter;
    private bool _isBlending = false;

    void Awake()
    {
        if (!groundRenderer)
        {
            Debug.LogError("SeasonSwitcher: Ground Renderer not assigned.");
            enabled = false;
            return;
        }

        _mpb = new MaterialPropertyBlock();
        groundRenderer.GetPropertyBlock(_mpb);

        // Initialize with the starting season
        ApplySeasonTexturesA(_current);
        _mpb.SetFloat(_Blend, 0f);
        groundRenderer.SetPropertyBlock(_mpb);
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !_isBlending)
        {
            Debug.Log("SeasonSwitcher: Space key pressed, switching season.");
            if (_current == Season.Winter)
            {
                SpringGrasses.SetActive(true);
            }
            else if (_current == Season.Spring)
            {
                SpringGrasses.SetActive(false);
                SummerGrasses.SetActive(true);
            }
            else if (_current == Season.Summer)
            {
                SummerGrasses.SetActive(false);
                AutumnGrasses.SetActive(true);
            }
            else if (_current == Season.Autumn)
            {
                AutumnGrasses.SetActive(false);
            }
            Season next = (Season)(((int)_current + 1) % 4);
            StartCoroutine(BlendToNextSeason(next));
        }

    }

    private IEnumerator BlendToNextSeason(Season next)
    {
        _isBlending = true;

        // Set A = current, B = next before we start increasing _Blend
        ApplySeasonTexturesA(_current);
        ApplySeasonTexturesB(next);

        float t = 0f;
        while (t < blendDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / blendDuration);
            _mpb.SetFloat(_Blend, k);
            groundRenderer.SetPropertyBlock(_mpb);
            yield return null;
        }

        // Finish: make 'next' become the new 'A' and reset Blend to 0
        _current = next;
        ApplySeasonTexturesA(_current);
        _mpb.SetFloat(_Blend, 0f);
        groundRenderer.SetPropertyBlock(_mpb);

        _isBlending = false;
    }

    private void ApplySeasonTexturesA(Season s)
    {
        _mpb.SetTexture(_SeasonA, GetAlbedo(s));
        if (useNormals)
            _mpb.SetTexture(_NormalA, GetNormal(s));
    }

    private void ApplySeasonTexturesB(Season s)
    {
        _mpb.SetTexture(_SeasonB, GetAlbedo(s));
        if (useNormals)
            _mpb.SetTexture(_NormalB, GetNormal(s));
    }

    private Texture2D GetAlbedo(Season s)
    {
        switch (s)
        {
            case Season.Winter: return winterAlbedo;
            case Season.Spring: return springAlbedo;
            case Season.Summer: return summerAlbedo;
            case Season.Autumn: return autumnAlbedo;
            default: return winterAlbedo;
        }
    }

    private Texture2D GetNormal(Season s)
    {
        switch (s)
        {
            case Season.Winter: return winterNormal;
            case Season.Spring: return springNormal;
            case Season.Summer: return summerNormal;
            case Season.Autumn: return autumnNormal;
            default: return winterNormal;
        }
    }
}
