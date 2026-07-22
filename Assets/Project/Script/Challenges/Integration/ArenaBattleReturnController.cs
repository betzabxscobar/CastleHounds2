using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class ArenaBattleReturnController : MonoBehaviour
{
    private const string BattleSceneName = "_DemoScene";
    private const string ReturnSceneName = "Demo";
    private const string PreviousVictorySceneName = "Ganaste";
    private const string BattleWolfName = "Enemy_Wolf_Model";
    private const string PlayerName = "Player_Dog_Model";

    private static readonly Vector3 ReturnPosition = new Vector3(-0.105f, 0f, 0.7049999f);
    private static bool pendingReturnTeleport;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void RegisterSceneHandlers()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        SceneManager.sceneLoaded += HandleSceneLoaded;

        ZoneTrigger.OnBeforeSceneLoadRequested -= HandleBeforeSceneLoadRequested;
        ZoneTrigger.OnBeforeSceneLoadRequested += HandleBeforeSceneLoadRequested;

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

    private static void ConfigureBattleSceneReturn()
    {
        DisableVictorySceneLoaders();
        ConfigureBattleWolfAsRegularEnemy();
        RedirectVictoryExitTriggersToDemo();
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
}
