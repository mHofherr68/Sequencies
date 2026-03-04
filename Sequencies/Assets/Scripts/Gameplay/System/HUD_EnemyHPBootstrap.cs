using UnityEngine;

/// <summary>
/// Ensures that the enemy HP UI is initialized correctly when a scene loads.
/// 
/// This component is typically placed on a UI object in scenes where the enemy
/// itself may not exist yet (for example at scene start). It pushes the last
/// saved enemy health value from <see cref="EnemyHealth"/> to the HUD so the
/// health bar displays the correct value immediately.
/// </summary>
public class HUD_EnemyHPBootstrap : MonoBehaviour
{
    private void Start()
    {
        // Apply previously saved enemy HP values to the UI if they exist.
        EnemyHealth.PushSavedToUIIfPresent();
    }
}