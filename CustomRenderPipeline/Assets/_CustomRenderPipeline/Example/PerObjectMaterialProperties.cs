using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    //https://docs.unity3d.com/ScriptReference/MaterialPropertyBlock.html
    static MaterialPropertyBlock block;
    static int cutoffId = Shader.PropertyToID("_Cutoff");

    static int baseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField]
    Color baseColor = Color.white;


    [SerializeField, Range(0f, 1f)]
    float cutoff = 0.5f;

    void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
        }
        block.SetColor(baseColorId, baseColor);
        block.SetFloat(cutoffId, cutoff);
        GetComponent<Renderer>().SetPropertyBlock(block);
    }
}