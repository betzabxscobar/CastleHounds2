using System.Collections;
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
    private const int ReturnTeleportFrameCount = 5;
    private const float GroundProbeHeight = 30f;
    private const float GroundProbeDistance = 80f;

    private static readonly Vector3 ReturnPosition = new Vector3(-0.105f, 0f, 0.7049999f);
    private static ArenaBattleReturnController runner;
    private static bool pendingReturnTeleport;

    // El portal de salida solo se activa cuando el enemigo de la arena muere.
    // Mientras esto sea false, el collider del trigger queda desactivado y
    // entrar a la zona del portal no hace nada.
    private static bool portalUnlocked;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneHandlers()
    {
        EnsureRunner();

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
            EnsureRunner();
            runner.StartCoroutine(ApplyReturnTeleportAfterDemoStartup());
        }
    }

    private static void EnsureRunner()
    {
        if (runner != null)
        {
            return;
        }

        GameObject runnerObject = new GameObject(nameof(ArenaBattleReturnController));
        runner = runnerObject.AddComponent<ArenaBattleReturnController>();
        DontDestroyOnLoad(runnerObject);
    }

    private static IEnumerator ApplyReturnTeleportAfterDemoStartup()
    {
        for (int frame = 0; frame < ReturnTeleportFrameCount; frame++)
        {
            yield return null;
            SkipDemoIntroIfPresent();

            yield return new WaitForEndOfFrame();
            TeleportPlayerToReturnPosition();
        }
    }

    private static void SkipDemoIntroIfPresent()
    {
        CinematicIntroController introController = Object.FindAnyObjectByType<CinematicIntroController>();
        if (introController != null)
        {
            introController.SkipIntro();
        }
    }

    private static void HandleEnemyDefeatedWithRole(EnemyHealth enemyHealth, EnemyRole role)
    {
        if (SceneManager.GetActiveScene().name != BattleSceneName || role != EnemyRole.RegularEnemy)
        {
            Debug.Log($"ArenaBattleReturnController: derrota de {enemyHealth?.name} ignorada (escena={SceneManager.GetActiveScene().name}, rol={role}).");
            return;
        }

        Debug.Log($"ArenaBattleReturnController: {enemyHealth.name} derrotado como RegularEnemy, desbloqueando portal.");
        UnlockPortalExit();
    }

    private static void ConfigureBattleSceneReturn()
    {
        // Cada vez que se (re)carga la arena, el portal arranca bloqueado:
        // solo se desbloquea al morir el enemigo.
        portalUnlocked = false;

        DisableVictorySceneLoaders();
        ConfigureBattleWolfAsRegularEnemy();
        RedirectVictoryExitTriggersToDemo();
        ConfigurePortalExitTrigger();
    }

    private static void DisableVictorySceneLoaders()
    {
        foreach (VictoriaCargarEscena victoryLoader in Object.FindObjectsByType<VictoriaCargarEscena>(FindObjectsInactive.Include))
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

        foreach (ZoneTrigger trigger in Object.FindObjectsByType<ZoneTrigger>(FindObjectsInactive.Include))
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

        ZoneTrigger zoneTrigger = portalTriggerObject.GetComponent<ZoneTrigger>();
        if (zoneTrigger == null)
        {
            zoneTrigger = portalTriggerObject.AddComponent<ZoneTrigger>();
        }

        zoneTrigger.ConfigureSceneLoadTarget(ReturnSceneName);

        // ConfigureSceneLoadTarget reactiva el collider; se fuerza aqui al final
        // para que el portal solo quede activo cuando el enemigo ya murio.
        triggerCollider.enabled = portalUnlocked;

        Debug.Log($"ArenaBattleReturnController: {PortalTriggerName} configurado -> destino={ReturnSceneName}, desbloqueado={portalUnlocked}, colliderEnabled={triggerCollider.enabled}, activo={portalTriggerObject.activeInHierarchy}.");
    }

    private static void UnlockPortalExit()
    {
        // A partir de aqui el portal queda habilitado (enemigo derrotado).
        portalUnlocked = true;

        GameEvents.RaiseDoorShouldOpen(PortalDoorId);
        ConfigurePortalExitTrigger();

        GameObject blocker = FindSceneGameObject(PortalBlockerName);
        if (blocker == null)
        {
            Debug.LogWarning($"ArenaBattleReturnController: no se encontro {PortalBlockerName} en {BattleSceneName}, no se puede confirmar el desbloqueo.");
            return;
        }

        Collider blockerCollider = blocker.GetComponent<Collider>();
        if (blockerCollider != null)
        {
            blockerCollider.enabled = false;
            Debug.Log($"ArenaBattleReturnController: {PortalBlockerName} desactivado, paso libre hacia {PortalTriggerName}.");
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
        RestorePlayerControl(player);

        PlayerControlLock controlLock = player.GetComponent<PlayerControlLock>();
        if (controlLock != null)
        {
            controlLock.ForceUnlockAll();
        }

        CharacterController characterController = player.GetComponent<CharacterController>();
        bool restoreCharacterController = characterController != null && characterController.enabled;
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        Vector3 groundedReturnPosition = ResolveGroundedReturnPosition(ReturnPosition, characterController);

        Rigidbody rigidbody = player.GetComponent<Rigidbody>();
        if (rigidbody != null)
        {
            rigidbody.linearVelocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.position = groundedReturnPosition;
        }

        player.transform.position = groundedReturnPosition;

        if (characterController != null)
        {
            characterController.enabled = restoreCharacterController;
        }

        Debug.Log($"ArenaBattleReturnController: jugador teletransportado a {groundedReturnPosition} al volver de {BattleSceneName}.");
    }

    private static void RestorePlayerControl(GameObject player)
    {
        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = true;
        }

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = true;
            playerController.SetInputEnabled(true);
        }

        MouseBasicAttack mouseBasicAttack = player.GetComponent<MouseBasicAttack>();
        if (mouseBasicAttack != null)
        {
            mouseBasicAttack.enabled = true;
        }
    }

    private static Vector3 ResolveGroundedReturnPosition(Vector3 desiredPosition, CharacterController characterController)
    {
        Vector3 probeOrigin = desiredPosition + Vector3.up * GroundProbeHeight;
        if (!Physics.Raycast(probeOrigin, Vector3.down, out RaycastHit hit, GroundProbeDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            return desiredPosition;
        }

        float groundedY = hit.point.y;
        if (characterController != null)
        {
            groundedY += characterController.height * 0.5f - characterController.center.y;
        }

        return new Vector3(desiredPosition.x, groundedY, desiredPosition.z);
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
