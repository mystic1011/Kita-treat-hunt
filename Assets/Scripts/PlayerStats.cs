using UnityEngine;

public class PlayerStats
{
    private int score;
    private int health;
    // ENCAPSULATION: setters protect the data; callers use these properties.
    public int Score { get { return score; } private set { score = Mathf.Max(0, value); } }
    public int Health { get { return health; } private set { health = Mathf.Clamp(value, 0, 3); } }
    public void Reset() { Score = 0; Health = 3; }
    public void AddScore(int amount) { if (amount > 0) Score += amount; }
    public void TakeDamage() { Health -= 1; }
}
