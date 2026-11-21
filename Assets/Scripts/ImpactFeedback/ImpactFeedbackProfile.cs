using System.Collections.Generic;
using UnityEngine;

// 1‚Â‚ÌImpact‰‰o‚ÅÀs‚·‚éEffect‚Ì‘g‚İ‡‚í‚¹‚ğ•Û‚·‚é
[CreateAssetMenu(fileName = "ImpactFeedbackProfile", menuName = "Impact Feedback/Profile")]
public sealed class ImpactFeedbackProfile : ScriptableObject
{
    [SerializeField]
    private ImpactEffect[] effects;

    public IReadOnlyList<ImpactEffect> Effects => effects;
}
