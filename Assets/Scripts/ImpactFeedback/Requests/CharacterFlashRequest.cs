using UnityEngine;

// Character FlashÇÃé¿çsê›íË
public readonly struct CharacterFlashRequest
{
    public Material FlashMaterial { get; }
    public int FlashCount { get; }
    public float VisibleDuration { get; }
    public float HiddenDuration { get; }

    public CharacterFlashRequest(Material flashMaterial, int flashCount, float visibleDuration, float hiddenDuration)
    {
        FlashMaterial = flashMaterial;
        FlashCount = flashCount;
        VisibleDuration = visibleDuration;
        HiddenDuration = hiddenDuration;
    }
}