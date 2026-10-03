using UnityEngine;
// INHERITANCE
public class Treat : Collectible
{
    public override Color Tint { get { return new Color(0.96f, 0.64f, 0.65f); } }
    // POLYMORPHISM
    public override void ApplyEffect(PlayerStats stats) { stats.AddScore(1); }
}
