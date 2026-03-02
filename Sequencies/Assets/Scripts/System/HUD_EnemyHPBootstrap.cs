using UnityEngine;

public class HUD_EnemyHPBootstrap : MonoBehaviour
{
    private void Start()
    {
        EnemyHealth.PushSavedToUIIfPresent();
    }
}