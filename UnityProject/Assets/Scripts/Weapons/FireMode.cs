namespace Weapons
{
    public enum FireMode
    {
        Manual,   // Nur wenn Button gedrückt wird
        AutoHold, // Dauerfeuer solange Button gehalten wird
        AutoFire  // Feuert von selbst im Cooldown (z.B. Turret)
    }
}