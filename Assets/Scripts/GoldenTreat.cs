using UnityEngine;
// INHERITANCE
public class GoldenTreat : Collectible
{
    public override Color Tint { get { return new Color(1f, 0.79f, 0.24f); } }
    // POLYMORPHISM: the same method gives a different reward.
    public override void ApplyEffect(PlayerStats stats) { stats.AddScore(3); }
}
