// Character ShakeÇÃé¿çsê›íË
public readonly struct CharacterShakeRequest
{
    public float Duration { get; }
    public CharacterShakeMode Mode { get; }
    public float PositionStrength { get; }
    public float RotationStrength { get; }
    public float Frequency { get; }

    public CharacterShakeRequest(float duration, CharacterShakeMode mode, float positionStrength, float rotationStrength, float frequency)
    {
        Duration = duration;
        Mode = mode;
        PositionStrength = positionStrength;
        RotationStrength = rotationStrength;
        Frequency = frequency;
    }
}