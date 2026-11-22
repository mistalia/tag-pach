using UnityEngine;

public class FieldTile : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material spotLightMaterial;

    [SerializeField] private Material holoMaterial;

    private bool holo = false;

    [Header("Effet key")]
    [SerializeField] private string effect_Red = "Field_Red";
    [SerializeField] private string effect_Blue = "Field_Blue";

    // À•Wî•ñ
    [Header("Coordinate")]
    [SerializeField]private Vector2Int coordinate;
    public Stone PutedStone {get; private set; } = null;

    private void Start()
    {
        this.gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
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
                this.gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(PutedStone.Owner.TeamColor, defaultMaterial.color,PutedStone.CurrentProg);
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
            this.gameObject.GetComponent<MeshRenderer>().material.color = Color.Lerp(PutedStone.Owner.TeamColor, highlightMaterial.color,PutedStone.CurrentProg);
        }
        else if (highlightMaterial != null)
        {
            this.gameObject.GetComponent<MeshRenderer>().material = highlightMaterial;
        }
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
            this.gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
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

        MeshRenderer renderer = GetComponent<MeshRenderer>();

        if (active)
        {
            renderer.material = holoMaterial;
        }
        else
        {
            // ホログラム終了時は必ず通常マテリアルへ
            renderer.material = defaultMaterial;
        }

        holo = active;
        
    }
    
    public void SetSpotLightMaterial()
    {
        if (spotLightMaterial != null)
        {
            this.gameObject.GetComponent<MeshRenderer>().material = spotLightMaterial;
        }
    }
}
