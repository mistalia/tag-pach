using UnityEngine;

public class FieldTile : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material highlightMaterial;

    // ç¿ïWèÓïÒ
    [SerializeField]private Vector2Int coordinate;
    public Stone PutedStone {get; private set; } = null;

    private void Start()
    {
        this.gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
    }

    public bool PutStone(Stone stone)
    {
        const int MATCH_COUNT = 3;
        if (PutedStone != null)
        {
            return false;
        }
        PutedStone = stone;
        PutedStone.transform.position = this.transform.position + new Vector3(0, (this.transform.localScale.y * 0.5f) + PutedStone.transform.localScale.y, 0);

        MatchCount count = FieldManager.Instance.CheckMatch(this.coordinate);

        if(count.lineDir_x1_y0 >= MATCH_COUNT)
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
        if (highlightMaterial != null)
        {
            this.gameObject.GetComponent<MeshRenderer>().material = highlightMaterial;
        }
    }

    public void ResetTileColor()
    {
        if (defaultMaterial != null)
        {
            this.gameObject.GetComponent<MeshRenderer>().material = defaultMaterial;
        }
    }

    public void ClearStone()
    {
        if (PutedStone != null)
        {
            Destroy(PutedStone.gameObject);
            PutedStone = null;
        }
    }
}
