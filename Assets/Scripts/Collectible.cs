using UnityEngine;

// INHERITANCE: Treat and GoldenTreat share this parent class.
public abstract class Collectible : MonoBehaviour
{
    public Vector2 Position { get; private set; }
    public bool Collected { get; private set; }
    public abstract Color Tint { get; }
    public void Initialize(Vector2 position) { Position = position; Collected = false; }
    // POLYMORPHISM: the manager invokes this on a Collectible reference.
    public abstract void ApplyEffect(PlayerStats stats);
    // ABSTRACTION: hides applying the effect and deactivating the pickup.
    public void Collect(PlayerStats stats)
    {
        if (Collected) return;
        ApplyEffect(stats);
        Collected = true;
        gameObject.SetActive(false);
    }
}
