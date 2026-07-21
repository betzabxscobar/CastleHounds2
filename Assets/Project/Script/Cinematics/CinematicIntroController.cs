using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DefaultExecutionOrder(-1000)]
public sealed class CinematicIntroController : MonoBehaviour
{
    private const string IntroObjectName = "InicioCinematografico";
    private const string CastleObjectName = "Castle";
    private const string DogObjectName = "Player_Dog_Model";
    private const string CastleCenterName = "CentroRotacionCastillo";
    private const string DogStartPointName = "PuntoInicioPerro";
    private const string DogEntryPointName = "PuntoEntradaPerro";
    private const string DogCameraPointName = "PuntoCamaraPerro";
    private const string DogLookPointName = "PuntoMiradaPerro";
    private const string CastleCameraName = "CamaraVistaCastillo";
    private const string EntranceCameraName = "CamaraEntradaCastillo";
    private const string DogPresentationCameraName = "CamaraPresentacionPerro";
    private const string ExplorationCameraName = "CamaraExploracion";

    [Header("Referencias principales")]
    [SerializeField] private PlayableDirector timelineDirector;
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private Transform dog;
    [SerializeField] private Animator dogAnimator;
    [SerializeField] private MonoBehaviour[] dogControlScripts;
    [SerializeField] private GameObject[] inputObjectsToDisable;

    [Header("Camaras Cinemachine 3")]
    [SerializeField] private CinemachineCamera castleCamera;
    [SerializeField] private CinemachineCamera entranceCamera;
    [SerializeField] private CinemachineCamera dogPresentationCamera;
    [SerializeField] private CinemachineCamera explorationCamera;

    [Header("Movimiento cinematico")]
    [SerializeField] private CastleOrbitCameraRig castleOrbit;
    [SerializeField] private DogCinematicAutoMover dogAutoMover;
    [SerializeField] private Transform dogStartPoint;
    [SerializeField] private Transform dogCameraPoint;
    [SerializeField] private Transform dogLookPoint;
    [SerializeField] private float finalCameraDistance = 3.2f;
    [SerializeField] private float finalCameraHeight = 1.15f;
    [SerializeField] private float finalLookHeight = 0.55f;

    [Header("Tiempos")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private float castleShotDuration = 5f;
    [SerializeField] private float entranceShotDuration = 2.5f;
    [SerializeField] private float dogShotDuration = 3f;
    [SerializeField] private float finalBlendDuration = 1.5f;
    [SerializeField] private float defaultBlendDuration = 1.5f;

    [Header("Opciones")]
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private bool moveDogToStartOnIntro = true;
    [SerializeField] private KeyCode legacySkipKey = KeyCode.Escape;
    [SerializeField] private int inactivePriority = 0;
    [SerializeField] private int activePriority = 20;

    private Coroutine introRoutine;
    private bool introRunning;
    private bool introFinished;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateIntroIfMissing()
    {
        if (FindAnyObjectByType<CinematicIntroController>() != null)
        {
            return;
        }

        if (GameObject.Find(CastleObjectName) == null || GameObject.Find(DogObjectName) == null)
        {
            return;
        }

        GameObject introObject = new GameObject(IntroObjectName);
        introObject.AddComponent<PlayableDirector>();
        introObject.AddComponent<CinematicIntroController>();
    }

    private void Awake()
    {
        introRunning = false;
        introFinished = false;
        introRoutine = null;
        ResolveSceneReferences();
    }

    private void Start()
    {
        ResolveSceneReferences();

        if (playOnStart)
        {
            StartIntro();
        }
    }

    private void Update()
    {
        if (!introRunning || !allowSkip)
        {
            return;
        }

        if (SkipPressed())
        {
            SkipIntro();
        }
    }

    public void StartIntro()
    {
        if (introRunning || introFinished)
        {
            return;
        }

        ResolveSceneReferences();
        introRoutine = StartCoroutine(PlayIntroRoutine());
    }

    public void SkipIntro()
    {
        if (introFinished)
        {
            return;
        }

        if (introRoutine != null)
        {
            StopCoroutine(introRoutine);
            introRoutine = null;
        }

        if (timelineDirector != null && timelineDirector.state == PlayState.Playing)
        {
            timelineDirector.Stop();
        }

        FinishIntro(true);
    }

    private IEnumerator PlayIntroRoutine()
    {
        introRunning = true;
        Debug.Log("Iniciando cinematica inicial de Castle Hounds.");
        LockDogControl(true);
        MoveDogToCinematicStart();
        ConfigureBrainBlend(defaultBlendDuration);
        PrepareCameras();

        if (timelineDirector != null && timelineDirector.playableAsset != null)
        {
            bool timelineStopped = false;
            timelineDirector.stopped += OnTimelineStopped;
            timelineDirector.Play();

            while (!timelineStopped && timelineDirector.state == PlayState.Playing)
            {
                yield return null;
            }

            timelineDirector.stopped -= OnTimelineStopped;

            void OnTimelineStopped(PlayableDirector director)
            {
                if (director == timelineDirector)
                {
                    timelineStopped = true;
                }
            }
        }
        else
        {
            yield return PlayFallbackSequence();
        }

        ActivateCamera(explorationCamera);
        PositionExplorationCameraBehindDog();
        yield return new WaitForSeconds(Mathf.Max(0f, finalBlendDuration));

        FinishIntro(false);
    }

    private IEnumerator PlayFallbackSequence()
    {
        if (castleOrbit != null)
        {
            castleOrbit.IsRotating = true;
        }

        ActivateCamera(castleCamera);
        yield return new WaitForSeconds(Mathf.Max(0f, castleShotDuration));

        if (castleOrbit != null)
        {
            castleOrbit.IsRotating = false;
        }

        ActivateCamera(entranceCamera);
        yield return new WaitForSeconds(Mathf.Max(0f, entranceShotDuration));

        ActivateCamera(dogPresentationCamera);
        if (dogAutoMover != null)
        {
            dogAutoMover.BeginMove();
        }

        float elapsed = 0f;
        while (elapsed < dogShotDuration)
        {
            if (dogAutoMover != null && dogAutoMover.HasArrived)
            {
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void FinishIntro(bool skipped)
    {
        introRunning = false;
        introFinished = true;
        introRoutine = null;

        if (castleOrbit != null)
        {
            castleOrbit.IsRotating = false;
        }

        if (dogAutoMover != null)
        {
            dogAutoMover.StopMove();
        }

        PositionExplorationCameraBehindDog();
        ActivateCamera(explorationCamera);
        DisableCinematicCameras();
        LockDogControl(false);

        if (skipped)
        {
            Debug.Log("Cinematica inicial saltada. Control del perro restaurado.");
        }
    }

    private void PrepareCameras()
    {
        EnableCameraObject(castleCamera);
        EnableCameraObject(entranceCamera);
        EnableCameraObject(dogPresentationCamera);
        EnableCameraObject(explorationCamera);
        SetCameraPriority(castleCamera, inactivePriority);
        SetCameraPriority(entranceCamera, inactivePriority);
        SetCameraPriority(dogPresentationCamera, inactivePriority);
        SetCameraPriority(explorationCamera, inactivePriority);
    }

    private void ActivateCamera(CinemachineCamera cameraToActivate)
    {
        if (cameraToActivate == null)
        {
            return;
        }

        SetCameraPriority(castleCamera, inactivePriority);
        SetCameraPriority(entranceCamera, inactivePriority);
        SetCameraPriority(dogPresentationCamera, inactivePriority);
        SetCameraPriority(explorationCamera, inactivePriority);
        SetCameraPriority(cameraToActivate, activePriority);
        cameraToActivate.gameObject.SetActive(true);
    }

    private void DisableCinematicCameras()
    {
        if (castleCamera != null)
        {
            castleCamera.gameObject.SetActive(false);
        }

        if (entranceCamera != null)
        {
            entranceCamera.gameObject.SetActive(false);
        }

        if (dogPresentationCamera != null)
        {
            dogPresentationCamera.gameObject.SetActive(false);
        }

        if (explorationCamera != null)
        {
            explorationCamera.gameObject.SetActive(true);
            SetCameraPriority(explorationCamera, activePriority);
        }
    }

    private void PositionExplorationCameraBehindDog()
    {
        if (explorationCamera == null || dog == null)
        {
            return;
        }

        Transform followTarget = dog;
        Transform lookTarget = dogLookPoint != null ? dogLookPoint : dog;

        explorationCamera.Follow = followTarget;
        explorationCamera.LookAt = lookTarget;

        Vector3 finalPosition = dogCameraPoint != null
            ? dogCameraPoint.position
            : dog.position - dog.forward * finalCameraDistance + Vector3.up * finalCameraHeight;

        Vector3 lookPosition = dogLookPoint != null ? dogLookPoint.position : dog.position + Vector3.up * finalLookHeight;
        Vector3 direction = lookPosition - finalPosition;
        if (direction.sqrMagnitude <= 0.0001f)
        {
            direction = dog.forward;
        }

        explorationCamera.transform.SetPositionAndRotation(finalPosition, Quaternion.LookRotation(direction.normalized, Vector3.up));
        explorationCamera.ForceCameraPosition(explorationCamera.transform.position, explorationCamera.transform.rotation);
    }

    private void LockDogControl(bool locked)
    {
        if (dogControlScripts != null)
        {
            foreach (MonoBehaviour controlScript in dogControlScripts)
            {
                if (controlScript != null)
                {
                    controlScript.enabled = !locked;
                }
            }
        }

        if (inputObjectsToDisable != null)
        {
            foreach (GameObject inputObject in inputObjectsToDisable)
            {
                if (inputObject != null)
                {
                    inputObject.SetActive(!locked);
                }
            }
        }

        if (dogAnimator != null && locked)
        {
            dogAnimator.applyRootMotion = false;
        }
    }

    private void MoveDogToCinematicStart()
    {
        if (!moveDogToStartOnIntro || dog == null || dogStartPoint == null)
        {
            return;
        }

        dog.SetPositionAndRotation(dogStartPoint.position, dogStartPoint.rotation);

        Transform target = dogAutoMover != null ? dogAutoMover.Target : null;
        if (target == null)
        {
            return;
        }

        Vector3 direction = target.position - dog.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.0001f)
        {
            dog.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }

    private void ConfigureBrainBlend(float duration)
    {
        if (cinemachineBrain == null)
        {
            Camera mainCamera = Camera.main;
            cinemachineBrain = mainCamera != null ? mainCamera.GetComponent<CinemachineBrain>() : null;
        }

        if (cinemachineBrain == null)
        {
            return;
        }

        cinemachineBrain.DefaultBlend = new CinemachineBlendDefinition(
            CinemachineBlendDefinition.Styles.EaseInOut,
            Mathf.Max(0f, duration));
    }

    private void SetCameraPriority(CinemachineCamera cameraToSet, int priority)
    {
        if (cameraToSet != null)
        {
            cameraToSet.Priority = priority;
        }
    }

    private void EnableCameraObject(CinemachineCamera cameraToEnable)
    {
        if (cameraToEnable != null)
        {
            cameraToEnable.gameObject.SetActive(true);
        }
    }

    private void ResolveSceneReferences()
    {
        GameObject dogObject = dog != null ? dog.gameObject : GameObject.Find(DogObjectName);
        GameObject castleObject = GameObject.Find(CastleObjectName);

        if (dog == null && dogObject != null)
        {
            dog = dogObject.transform;
        }

        if (dogAnimator == null && dogObject != null)
        {
            dogAnimator = dogObject.GetComponentInChildren<Animator>();
        }

        if (timelineDirector == null)
        {
            timelineDirector = GetComponent<PlayableDirector>();
        }

        if (cinemachineBrain == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cinemachineBrain = mainCamera.GetComponent<CinemachineBrain>();
                if (cinemachineBrain == null)
                {
                    cinemachineBrain = mainCamera.gameObject.AddComponent<CinemachineBrain>();
                }
            }
        }

        Transform castleCenter = FindOrCreateRoot(CastleCenterName);
        if (castleObject != null)
        {
            castleCenter.position = GetBoundsCenter(castleObject);
        }

        dogStartPoint ??= FindOrCreateRoot(DogStartPointName);
        Transform dogEntryPoint = FindOrCreateRoot(DogEntryPointName);
        PositionDogRoutePoints(dogStartPoint, dogEntryPoint);

        if (dog != null)
        {
            dogCameraPoint ??= FindOrCreateChild(dog, DogCameraPointName, new Vector3(0f, 1.15f, -3.2f));
            dogLookPoint ??= FindOrCreateChild(dog, DogLookPointName, new Vector3(0f, 0.55f, 0.2f));
        }

        castleCamera ??= FindOrCreateCamera(CastleCameraName, castleCenter.position + new Vector3(-8f, 5f, -8f), castleCenter, castleCenter);
        entranceCamera ??= FindOrCreateCamera(EntranceCameraName, castleCenter.position + new Vector3(0f, 2.2f, -6f), null, castleCenter);
        dogPresentationCamera ??= FindOrCreateCamera(
            DogPresentationCameraName,
            dog != null ? dog.position + new Vector3(-2f, 1.4f, -3f) : new Vector3(-2f, 1.4f, -3f),
            dog,
            dogLookPoint != null ? dogLookPoint : dog);
        explorationCamera ??= FindOrCreateCamera(
            ExplorationCameraName,
            dogCameraPoint != null ? dogCameraPoint.position : new Vector3(0f, 1.5f, -4f),
            dog,
            dogLookPoint != null ? dogLookPoint : dog);

        castleOrbit ??= castleCamera != null ? castleCamera.GetComponent<CastleOrbitCameraRig>() : null;
        if (castleOrbit == null && castleCamera != null)
        {
            castleOrbit = castleCamera.gameObject.AddComponent<CastleOrbitCameraRig>();
        }

        if (castleOrbit != null)
        {
            castleOrbit.Target = castleCenter;
        }

        if (dogAutoMover == null && dogObject != null)
        {
            dogAutoMover = dogObject.GetComponent<DogCinematicAutoMover>();
            if (dogAutoMover == null)
            {
                dogAutoMover = dogObject.AddComponent<DogCinematicAutoMover>();
            }
        }

        if (dogAutoMover != null)
        {
            dogAutoMover.Target = dogEntryPoint;
        }

        ConfigureExplorationCamera();
    }

    private CinemachineCamera FindOrCreateCamera(string cameraName, Vector3 position, Transform follow, Transform lookAt)
    {
        GameObject cameraObject = GameObject.Find(cameraName);
        if (cameraObject == null)
        {
            cameraObject = new GameObject(cameraName);
        }

        cameraObject.SetActive(true);
        cameraObject.transform.position = position;

        if (lookAt != null)
        {
            Vector3 direction = lookAt.position - position;
            if (direction.sqrMagnitude > 0.0001f)
            {
                cameraObject.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        CinemachineCamera camera = cameraObject.GetComponent<CinemachineCamera>();
        if (camera == null)
        {
            camera = cameraObject.AddComponent<CinemachineCamera>();
        }

        camera.Follow = follow;
        camera.LookAt = lookAt;
        camera.Lens.FieldOfView = 55f;

        CinemachineRotationComposer composer = cameraObject.GetComponent<CinemachineRotationComposer>();
        if (composer == null)
        {
            composer = cameraObject.AddComponent<CinemachineRotationComposer>();
        }

        composer.Damping = new Vector2(0.35f, 0.35f);
        return camera;
    }

    private void ConfigureExplorationCamera()
    {
        if (explorationCamera == null)
        {
            return;
        }

        CinemachineThirdPersonFollow thirdPerson = explorationCamera.GetComponent<CinemachineThirdPersonFollow>();
        if (thirdPerson == null)
        {
            thirdPerson = explorationCamera.gameObject.AddComponent<CinemachineThirdPersonFollow>();
        }

        thirdPerson.Damping = new Vector3(0.25f, 0.35f, 0.25f);
        thirdPerson.ShoulderOffset = new Vector3(0f, 0.55f, 0f);
        thirdPerson.VerticalArmLength = 0.25f;
        thirdPerson.CameraDistance = finalCameraDistance;
        thirdPerson.CameraSide = 0.5f;
    }

    private Transform FindOrCreateRoot(string objectName)
    {
        GameObject existing = GameObject.Find(objectName);
        if (existing != null)
        {
            return existing.transform;
        }

        return new GameObject(objectName).transform;
    }

    private Transform FindOrCreateChild(Transform parent, string childName, Vector3 localPosition)
    {
        Transform existing = parent.Find(childName);
        if (existing != null)
        {
            existing.localPosition = localPosition;
            return existing;
        }

        GameObject child = new GameObject(childName);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = localPosition;
        child.transform.localRotation = Quaternion.identity;
        return child.transform;
    }

    private void PositionDogRoutePoints(Transform startPoint, Transform entryPoint)
    {
        GameObject entranceFloor = GameObject.Find("PisoEntrada (1)");
        Vector3 floorPosition = entranceFloor != null ? entranceFloor.transform.position : new Vector3(-2.69f, 0.12f, -7.22f);
        float dogRootHeight = dog != null ? dog.position.y : 0.03f;

        startPoint.position = new Vector3(floorPosition.x, dogRootHeight, floorPosition.z - 1.4f);
        entryPoint.position = new Vector3(floorPosition.x, dogRootHeight, floorPosition.z + 1.8f);

        Vector3 direction = entryPoint.position - startPoint.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            startPoint.rotation = rotation;
            entryPoint.rotation = rotation;
        }
    }

    private Vector3 GetBoundsCenter(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            return root.transform.position;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        return bounds.center;
    }

    private bool SkipPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame))
        {
            return true;
        }
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
        return Input.GetKeyDown(legacySkipKey) || Input.GetKeyDown(KeyCode.Space);
#else
        return false;
#endif
    }

    private void OnValidate()
    {
        castleShotDuration = Mathf.Max(0f, castleShotDuration);
        entranceShotDuration = Mathf.Max(0f, entranceShotDuration);
        dogShotDuration = Mathf.Max(0f, dogShotDuration);
        finalBlendDuration = Mathf.Max(0f, finalBlendDuration);
        defaultBlendDuration = Mathf.Max(0f, defaultBlendDuration);
        finalCameraDistance = Mathf.Max(0.5f, finalCameraDistance);
        finalCameraHeight = Mathf.Max(0.1f, finalCameraHeight);
        finalLookHeight = Mathf.Max(0f, finalLookHeight);
        activePriority = Mathf.Max(inactivePriority + 1, activePriority);
    }
}
