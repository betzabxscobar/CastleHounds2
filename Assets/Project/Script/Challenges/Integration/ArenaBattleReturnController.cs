using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ArenaBattleReturnController : MonoBehaviour
{
    private const string BattleSceneName = "_DemoScene";
    private const string ReturnSceneName = "Demo";
    private const string PreviousVictorySceneName = "Ganaste";
    private const string BattleWolfName = "Enemy_Wolf_Model";
    private const string PlayerName = "Player_Dog_Model";
    private const string PortalName = "Portal";
    private const string PortalTriggerName = "Portal_Trigger";
    private const string PortalBlockerName = "Portal_Bloqueo";
    private const string PortalDoorId = "portal_arena";

    private static readonly Vector3 ReturnPosition = new Vector3(-0.105f, 0f, 0.7049999f);
    private static bool pendingReturnTeleport;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneHandlers()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;

        ZoneTrigger.OnBeforeSceneLoadRequested -= HandleBeforeSceneLoadRequested;
        ZoneTrigger.OnBeforeSceneLoadRequested += HandleBeforeSceneLoadRequested;

        GameEvents.OnEnemyDefeatedWithRole -= HandleEnemyDefeatedWithRole;
        GameEvents.OnEnemyDefeatedWithRole += HandleEnemyDefeatedWithRole;

        HandleSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private static void HandleBeforeSceneLoadRequested(string sceneName)
    {
        if (SceneManager.GetActiveScene().name == BattleSceneName && sceneName == ReturnSceneName)
        {
            pendingReturnTeleport = true;
        }
    }

    private static void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == BattleSceneName)
        {
            ConfigureBattleSceneReturn();
            return;
        }

        if (scene.name == ReturnSceneName && pendingReturnTeleport)
        {
            pendingReturnTeleport = false;
            TeleportPlayerToReturnPosition();
        }
    }

    private static void HandleEnemyDefeatedWithRole(EnemyHealth enemyHealth, EnemyRole role)
    {
        if (SceneManager.GetActiveScene().name != BattleSceneName || role != EnemyRole.RegularEnemy)
        {
            return;
        }

        UnlockPortalExit();
    }

    private static void ConfigureBattleSceneReturn()
    {
        DisableVictorySceneLoaders();
        ConfigureBattleWolfAsRegularEnemy();
        RedirectVictoryExitTriggersToDemo();
        ConfigurePortalExitTrigger();
    }

    private static void DisableVictorySceneLoaders()
    {
        foreach (VictoriaCargarEscena victoryLoader in Object.FindObjectsByType<VictoriaCargarEscena>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            victoryLoader.enabled = false;
        }
    }

    private static void ConfigureBattleWolfAsRegularEnemy()
    {
        GameObject wolf = GameObject.Find(BattleWolfName);
        if (wolf == null)
        {
            Debug.LogWarning($"ArenaBattleReturnController: no se encontro {BattleWolfName} en {BattleSceneName}.");
            return;
        }

        EnemyRoleMarker roleMarker = wolf.GetComponent<EnemyRoleMarker>();
        if (roleMarker == null)
        {
            roleMarker = wolf.AddComponent<EnemyRoleMarker>();
        }

        roleMarker.Configure(EnemyRole.RegularEnemy);

        EnemyHealth enemyHealth = wolf.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.ConfigureRole(EnemyRole.RegularEnemy);
        }
    }

    private static void RedirectVictoryExitTriggersToDemo()
    {
        int redirectedTriggers = 0;

        foreach (ZoneTrigger trigger in Object.FindObjectsByType<ZoneTrigger>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (!trigger.IsSceneLoadTarget(PreviousVictorySceneName))
            {
                continue;
            }

            trigger.ConfigureSceneLoadTarget(ReturnSceneName);
            redirectedTriggers++;
        }

        if (redirectedTriggers == 0)
        {
            Debug.LogWarning($"ArenaBattleReturnController: no se encontraron triggers de salida hacia {PreviousVictorySceneName} en {BattleSceneName}.");
        }
    }

    private static void ConfigurePortalExitTrigger()
    {
        GameObject portalTriggerObject = FindSceneGameObject(PortalTriggerName);
        if (portalTriggerObject == null)
        {
            GameObject portal = FindSceneGameObject(PortalName);
            if (portal == null)
            {
                Debug.LogWarning($"ArenaBattleReturnController: no se encontro {PortalName} en {BattleSceneName}.");
                return;
            }

            portalTriggerObject = new GameObject(PortalTriggerName);
            portalTriggerObject.transform.SetParent(portal.transform, false);
            portalTriggerObject.transform.localPosition = Vector3.zero;
            portalTriggerObject.transform.localRotation = Quaternion.identity;
            portalTriggerObject.transform.localScale = Vector3.one;
        }

        ActivateHierarchy(portalTriggerObject.transform);

        BoxCollider triggerCollider = portalTriggerObject.GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = portalTriggerObject.AddComponent<BoxCollider>();
            triggerCollider.size = new Vector3(2f, 3f, 2f);
        }

        triggerCollider.isTrigger = true;
        triggerCollider.enabled = true;

        ZoneTrigger zoneTrigger = portalTriggerObject.GetComponent<ZoneTrigger>();
        if (zoneTrigger == null)
        {
            zoneTrigger = portalTriggerObject.AddComponent<ZoneTrigger>();
        }

        zoneTrigger.ConfigureSceneLoadTarget(ReturnSceneName);
    }

    private static void UnlockPortalExit()
    {
        GameEvents.RaiseDoorShouldOpen(PortalDoorId);
        ConfigurePortalExitTrigger();

        GameObject blocker = FindSceneGameObject(PortalBlockerName);
        if (blocker == null)
        {
            return;
        }

        Collider blockerCollider = blocker.GetComponent<Collider>();
        if (blockerCollider != null)
        {
            blockerCollider.enabled = false;
        }
    }

    private static void TeleportPlayerToReturnPosition()
    {
        GameObject player = GameObject.Find(PlayerName);
        if (player == null)
        {
            Debug.LogError($"ArenaBattleReturnController: no se encontro {PlayerName} para teletransportar al volver a {ReturnSceneName}.");
            return;
        }

        Time.timeScale = 1f;

        PlayerControlLock controlLock = player.GetComponent<PlayerControlLock>();
        if (controlLock != null)
        {
            controlLock.ForceUnlockAll();
        }

        CharacterController characterController = player.GetComponent<CharacterController>();
        bool restoreCharacterController = characterController != null && characterController.enabled;
        if (restoreCharacterController)
        {
            characterController.enabled = false;
        }

        Rigidbody rigidbody = player.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.position = ReturnPosition;
        }

        player.transform.position = ReturnPosition;

        if (restoreCharacterController)
        {
            characterController.enabled = true;
        }

        Debug.Log($"ArenaBattleReturnController: jugador teletransportado a {ReturnPosition} al volver de {BattleSceneName}.");
    }

    private static GameObject FindSceneGameObject(string objectName)
    {
        foreach (GameObject sceneObject in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (sceneObject.name == objectName && sceneObject.scene.IsValid() && sceneObject.scene.name == SceneManager.GetActiveScene().name)
            {
                return sceneObject;
            }
        }

        return null;
    }

    private static void ActivateHierarchy(Transform target)
    {
        if (target.parent != null)
        {
            ActivateHierarchy(target.parent);
        }

        target.gameObject.SetActive(true);
    }
}
