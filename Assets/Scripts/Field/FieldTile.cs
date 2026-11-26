using UnityEngine;

public class FieldTile : MonoBehaviour
{
    [Header("View")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color highlightColor;

    [SerializeField] private Material holoMaterial;

    private bool holo = false;

    [Header("Effet key")]
    [SerializeField] private string effect_Red = "Field_Red";
    [SerializeField] private string effect_Blue = "Field_Blue";

    [Header("Coordinate")]
    [SerializeField]private Vector2Int coordinate;
    public Stone PutedStone {get; private set; } = null;

    private Material defaultMaterial;
    private Color defaultColor;
    private int baseColorProperty;

    private void Start()
    {
       if(meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if(meshRenderer != null)
        {
            defaultMaterial = meshRenderer.material;
            baseColorProperty = Shader.PropertyToID(defaultMaterial.HasProperty("_Base_Color") ? "_Base_Color" : "_BaseColor");
            defaultColor = defaultMaterial.GetColor(baseColorProperty);
        }
    }

    private void Update()
    {
        if (holo)
        {
            return;
        }

        if (PutedStone != null)
        {
            if(PutedStone.IsTransitioning)
            {
                meshRenderer.material.SetColor(baseColorProperty, Color.Lerp(PutedStone.Owner.TeamColor, defaultColor,PutedStone.CurrentProg));
            }
        }
    }

    public bool PutStone(Stone stone, string effectColor = null)
    {
        const int MATCH_COUNT = 3;
        if (PutedStone != null)
        {
            return false;
        }
        PutedStone = stone;

        var position = this.transform.position + new Vector3(0, (this.transform.localScale.y * 0.5f), 0);
        PutedStone.transform.position = position;

        if(effectColor == "Red")
        {
            EffectManager.Instance.PlayEffect(effect_Red, position);
        }
        else if(effectColor == "Blue")
        {
            EffectManager.Instance.PlayEffect(effect_Blue, position);
        }

        FieldManager.Instance.AddStoneCount();

        MatchCount count = FieldManager.Instance.CheckMatch(this.coordinate);

        FieldCamera.Instance?.CameraShake(0.1f, 0.2f);

        if (count.lineDir_x1_y0 >= MATCH_COUNT)
        {
            MatchStorage.Instance.AddMatchData(count.lineDir_x1_y0, this.coordinate, new Vector2Int(1, 0), PutedStone.Owner);
        }
        if(count.lineDir_x0_y1 >= MATCH_COUNT)
        {
            MatchStorage.Instance.AddMatchData(count.lineDir_x0_y1, this.coordinate, new Vector2Int(0, 1), PutedStone.Owner);
        }
        if(count.lineDir_x1_y1 >= MATCH_COUNT)
        {
            MatchStorage.Instance.AddMatchData(count.lineDir_x1_y1, this.coordinate, new Vector2Int(1, 1), PutedStone.Owner);
        }
        if(count.lineDir_x1_yn1 >= MATCH_COUNT)
        {
            MatchStorage.Instance.AddMatchData(count.lineDir_x1_yn1, this.coordinate, new Vector2Int(1, -1), PutedStone.Owner);
        }


        return true;
    }

    public bool CanPutStone()
    {
        return PutedStone == null;
    }

    public void SetCoodinate(float x, float y)
    {
        coordinate = new Vector2Int((int)x, (int)y);
    }
    public Vector2Int GetCoordinate()
    {
        return coordinate;
    }

    public void ShowColor()
    {
        if(PutedStone != null)
        {
            PutedStone.RevealColor(PutedStone.Owner.TeamColor);
        }
    }

    public void LightUpTile()
    {
        if (holo)
        {
            return;
        }

        if (PutedStone && PutedStone.IsTransitioning)
        {
            meshRenderer.material.SetColor(baseColorProperty, Color.Lerp(PutedStone.Owner.TeamColor, highlightColor, PutedStone.CurrentProg));
            
        }
        
        meshRenderer.material.SetColor(baseColorProperty, highlightColor);
    }

    public void ResetTileColor()
    {
        // ホログラム表示中は変更しない
        if (holo)
        {
            return;
        }

        if (defaultMaterial != null)
        {
            meshRenderer.material.SetColor(baseColorProperty, defaultColor);
        }
    }

    public void RemoveStone()
    {
        if (PutedStone != null)
        {
            FieldManager.Instance.SubtractStoneCount();
            Destroy(PutedStone.gameObject);
            PutedStone = null;
        }
    }

    public void SetHolo(bool active)
    {
        if (holo == active)
        {
            return;
        }

        if (active)
        {
            meshRenderer.material = holoMaterial;
        }
        else
        {
            // ホログラム終了時は必ず通常マテリアルへ
            meshRenderer.material = defaultMaterial;
        }

        holo = active;
        
    }

    public void SetFrameColor(Color color)
    {
        var material = meshRenderer.material;
        if (material.HasProperty("_Frame_Color"))
            material.SetColor("_Frame_Color", color);
    }
}
