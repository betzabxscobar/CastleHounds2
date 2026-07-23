using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
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
    private const string RuntimeNavMeshName = "Arena_RuntimeNavMesh";
    private const string CombatLimitsName = "Arena_CombatLimits";
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
        GameObject player = GameObject.Find(PlayerName);
        if (wolf == null)
        {
            Debug.LogWarning($"ArenaBattleReturnController: no se encontro {BattleWolfName} en {BattleSceneName}.");
            return;
        }

        if (player == null)
        {
            Debug.LogError($"ArenaBattleReturnController: no se encontro {PlayerName}; la IA enemiga no puede configurarse.");
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

        ConfigureEnemyCpu(wolf, player, enemyHealth);
    }

    private static void ConfigureEnemyCpu(GameObject wolf, GameObject player, EnemyHealth enemyHealth)
    {
        DogHealth playerHealth = player.GetComponent<DogHealth>();
        CombatGameManager combatManager = Object.FindAnyObjectByType<CombatGameManager>();
        BasicAttack attack = wolf.GetComponent<BasicAttack>();
        CombatAnimation combatAnimation = wolf.GetComponent<CombatAnimation>();

        if (enemyHealth == null || playerHealth == null || combatManager == null || attack == null)
        {
            Debug.LogError(
                $"ArenaBattleReturnController: referencias de combate incompletas. " +
                $"EnemyHealth={enemyHealth != null}, PlayerHealth={playerHealth != null}, " +
                $"CombatManager={combatManager != null}, BasicAttack={attack != null}.", wolf);
            return;
        }

        // La pelea debe comenzar activa cada vez que se carga la arena. BasicAttack
        // y la IA consultan este estado antes de perseguir o aplicar dano.
        combatManager.ResetCombat();
        combatManager.BeginCombat();
        enemyHealth.ResetHealth();
        playerHealth.ResetHealth();

        EnemyAutoAttack oldAutoAttack = wolf.GetComponent<EnemyAutoAttack>();
        if (oldAutoAttack != null) oldAutoAttack.enabled = false;
        attack.ConfigureAttack(10f, 1.2f);
        attack.ResetCooldown();

        BuildRuntimeArenaNavMesh(wolf, player);

        NavMeshAgent agent = wolf.GetComponent<NavMeshAgent>();
        if (agent == null) agent = wolf.AddComponent<NavMeshAgent>();
        agent.speed = 3.5f;
        agent.acceleration = 12f;
        agent.angularSpeed = 720f;
        agent.stoppingDistance = 1.4f;
        agent.autoBraking = true;
        agent.updatePosition = true;
        agent.updateRotation = false;
        agent.radius = 0.38f;
        agent.height = 1.2f;

        if (NavMesh.SamplePosition(wolf.transform.position, out NavMeshHit navHit, 4f, NavMesh.AllAreas))
        {
            agent.Warp(navHit.position);
        }

        Animator animator = wolf.GetComponentInChildren<Animator>(true);
        if (animator != null) animator.applyRootMotion = false;
        combatAnimation?.ConfigureStateNames("Idle_New", "Running_New", "AttackR", "GetHit_2", "dead");
        combatAnimation?.ConfigurePresentation(0.9f, 0.1f);

        Rigidbody enemyBody = wolf.GetComponent<Rigidbody>();
        if (enemyBody != null)
        {
            enemyBody.isKinematic = true;
            enemyBody.useGravity = false;
            enemyBody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        EnemyAIController ai = wolf.GetComponent<EnemyAIController>();
        if (ai == null) ai = wolf.AddComponent<EnemyAIController>();
        Animator playerAnimator = player.GetComponentInChildren<Animator>(true);
        Transform playerPositionReference = playerAnimator != null ? playerAnimator.transform : player.transform;
        ai.SetTarget(player.transform, playerHealth, playerPositionReference);
        ai.ConfigureCombat(combatManager);
        ai.ConfigureBehavior(25f, 2.2f, 3.2f, 8f, 1.7f);
        ai.enabled = true;

        float initialDistance = Vector3.Distance(wolf.transform.position, player.transform.position);
        Transform targetRoot = player.transform.root;
        string animatorControllerName = animator != null && animator.runtimeAnimatorController != null
            ? animator.runtimeAnimatorController.name
            : "NINGUNO";
        int animatorParameterCount = animator != null ? animator.parameters.Length : 0;
        Debug.Log(
            $"IA ENEMIGA CONFIGURADA | enemigo={wolf.name} | jugador={player.name} | " +
            $"distancia={initialDistance:F2} | AI={ai.enabled} | Agent={agent.enabled} | " +
            $"isOnNavMesh={agent.isOnNavMesh} | Animator={animator != null} | " +
            $"Controller={animatorControllerName} | parametros={animatorParameterCount} | " +
            $"estadoInicial=Idle | enemigoPos={wolf.transform.position} | objetivoRootPos={player.transform.position} | " +
            $"objetivoVisible={playerPositionReference.name} | objetivoVisiblePos={playerPositionReference.position} | " +
            $"objetivoRoot={targetRoot.name} | pathStatus={agent.pathStatus} | hasPath={agent.hasPath} | " +
            $"remainingDistance={agent.remainingDistance:F2} | timeScale={Time.timeScale:F2}", wolf);
    }

    private static void BuildRuntimeArenaNavMesh(GameObject wolf, GameObject player)
    {
        Bounds arenaBounds = ResolveArenaBounds();
        CreateCombatLimits(arenaBounds);

        GameObject surfaceObject = GameObject.Find(RuntimeNavMeshName);
        if (surfaceObject == null) surfaceObject = new GameObject(RuntimeNavMeshName);

        NavMeshSurface surface = surfaceObject.GetComponent<NavMeshSurface>();
        if (surface == null) surface = surfaceObject.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Volume;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.layerMask = ~0;
        surface.center = arenaBounds.center - surfaceObject.transform.position;
        surface.size = new Vector3(
            Mathf.Max(1f, arenaBounds.size.x - 1.2f),
            Mathf.Max(4f, arenaBounds.size.y + 4f),
            Mathf.Max(1f, arenaBounds.size.z - 1.2f));

        Collider[] dynamicColliders = CombineColliders(
            player.GetComponentsInChildren<Collider>(true),
            wolf.GetComponentsInChildren<Collider>(true));
        bool[] previousStates = new bool[dynamicColliders.Length];

        try
        {
            for (int i = 0; i < dynamicColliders.Length; i++)
            {
                previousStates[i] = dynamicColliders[i].enabled;
                dynamicColliders[i].enabled = false;
            }

            surface.BuildNavMesh();
        }
        finally
        {
            for (int i = 0; i < dynamicColliders.Length; i++)
                dynamicColliders[i].enabled = previousStates[i];
        }
    }

    private static Bounds ResolveArenaBounds()
    {
        GameObject arenaRoot = GameObject.Find("Gladiator Low Poly Arena");
        if (arenaRoot != null)
        {
            foreach (Renderer renderer in arenaRoot.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer.name == "Arena") return renderer.bounds;
            }
        }

        Debug.LogWarning("ArenaBattleReturnController: no se encontró el Renderer Arena; se usan límites seguros de respaldo.");
        return new Bounds(Vector3.zero, new Vector3(13f, 4f, 13f));
    }

    private static void CreateCombatLimits(Bounds arenaBounds)
    {
        GameObject limitsRoot = GameObject.Find(CombatLimitsName);
        if (limitsRoot == null) limitsRoot = new GameObject(CombatLimitsName);

        const float thickness = 0.5f;
        const float height = 4f;
        float halfX = Mathf.Max(2f, arenaBounds.extents.x - 0.6f);
        float halfZ = Mathf.Max(2f, arenaBounds.extents.z - 0.6f);
        float centerY = arenaBounds.min.y + height * 0.5f;

        CreateLimit(limitsRoot.transform, "Limit_North",
            new Vector3(arenaBounds.center.x, centerY, arenaBounds.center.z + halfZ),
            new Vector3(halfX * 2f, height, thickness));
        CreateLimit(limitsRoot.transform, "Limit_South",
            new Vector3(arenaBounds.center.x, centerY, arenaBounds.center.z - halfZ),
            new Vector3(halfX * 2f, height, thickness));
        CreateLimit(limitsRoot.transform, "Limit_East",
            new Vector3(arenaBounds.center.x + halfX, centerY, arenaBounds.center.z),
            new Vector3(thickness, height, halfZ * 2f));
        CreateLimit(limitsRoot.transform, "Limit_West",
            new Vector3(arenaBounds.center.x - halfX, centerY, arenaBounds.center.z),
            new Vector3(thickness, height, halfZ * 2f));
    }

    private static void CreateLimit(Transform parent, string limitName, Vector3 position, Vector3 size)
    {
        Transform existing = parent.Find(limitName);
        GameObject limit = existing != null ? existing.gameObject : new GameObject(limitName);
        limit.transform.SetParent(parent, true);
        limit.transform.position = position;
        limit.transform.rotation = Quaternion.identity;
        BoxCollider collider = limit.GetComponent<BoxCollider>();
        if (collider == null) collider = limit.AddComponent<BoxCollider>();
        collider.isTrigger = false;
        collider.size = size;
    }

    private static Collider[] CombineColliders(Collider[] first, Collider[] second)
    {
        Collider[] result = new Collider[first.Length + second.Length];
        first.CopyTo(result, 0);
        second.CopyTo(result, first.Length);
        return result;
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

        GameObject combatLimits = GameObject.Find(CombatLimitsName);
        if (combatLimits != null) combatLimits.SetActive(false);

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
