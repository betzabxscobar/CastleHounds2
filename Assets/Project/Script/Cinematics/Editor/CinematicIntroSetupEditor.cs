using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;

public static class CinematicIntroSetupEditor
{
    private const string MenuPath = "Castle Hounds/Cinematica/Crear entrada cinematografica";

    [MenuItem(MenuPath)]
    public static void CreateCinematicIntro()
    {
        GameObject dog = GameObject.Find("Player_Dog_Model");
        GameObject castle = GameObject.Find("Castle");
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            GameObject mainCameraObject = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(mainCameraObject, "Crear Main Camera");
            mainCameraObject.tag = "MainCamera";
            mainCamera = mainCameraObject.AddComponent<Camera>();
            mainCameraObject.AddComponent<AudioListener>();
            mainCamera.transform.SetPositionAndRotation(new Vector3(-2.6f, 1.8f, -8.8f), Quaternion.Euler(10f, 0f, 0f));
        }

        CinemachineBrain brain = mainCamera.GetComponent<CinemachineBrain>();
        if (brain == null)
        {
            brain = Undo.AddComponent<CinemachineBrain>(mainCamera.gameObject);
        }

        brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1.5f);

        Transform castleCenter = GetOrCreateRoot("CentroRotacionCastillo").transform;
        castleCenter.position = castle != null ? GetBoundsCenter(castle) : Vector3.zero;

        Transform dogTransform = dog != null ? dog.transform : null;
        Transform dogStartPoint = GetOrCreateRoot("PuntoInicioPerro").transform;
        Transform dogCameraPoint = GetOrCreateChild(dogTransform, "PuntoCamaraPerro", new Vector3(0f, 1.15f, -3.2f));
        Transform dogLookPoint = GetOrCreateChild(dogTransform, "PuntoMiradaPerro", new Vector3(0f, 0.55f, 0.2f));
        Transform dogEntryPoint = GetOrCreateRoot("PuntoEntradaPerro").transform;
        PositionDogRoutePoints(dogStartPoint, dogEntryPoint);

        if (dogTransform != null)
        {
            dogTransform.position = dogStartPoint.position;
            Vector3 direction = dogEntryPoint.position - dogStartPoint.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                dogTransform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            }
        }

        GameObject introObject = GetOrCreateRoot("InicioCinematografico");
        PlayableDirector director = introObject.GetComponent<PlayableDirector>();
        if (director == null)
        {
            director = Undo.AddComponent<PlayableDirector>(introObject);
        }

        CinematicIntroController controller = introObject.GetComponent<CinematicIntroController>();
        if (controller == null)
        {
            controller = Undo.AddComponent<CinematicIntroController>(introObject);
        }

        CinemachineCamera castleCamera = CreateOrGetCamera(
            "CamaraVistaCastillo",
            castleCenter.position + new Vector3(-8f, 5f, -8f),
            castleCenter,
            castleCenter);
        CinemachineCamera entranceCamera = CreateOrGetCamera(
            "CamaraEntradaCastillo",
            castleCenter.position + new Vector3(0f, 2.2f, -6f),
            null,
            castleCenter);
        CinemachineCamera dogCamera = CreateOrGetCamera(
            "CamaraPresentacionPerro",
            dogTransform != null ? dogTransform.position + new Vector3(-2f, 1.4f, -3f) : new Vector3(-2f, 1.4f, -3f),
            dogTransform,
            dogLookPoint);
        CinemachineCamera explorationCamera = CreateOrGetCamera(
            "CamaraExploracion",
            dogCameraPoint != null ? dogCameraPoint.position : new Vector3(0f, 1.5f, -4f),
            dogTransform,
            dogLookPoint != null ? dogLookPoint : dogTransform);

        CastleOrbitCameraRig orbit = castleCamera.GetComponent<CastleOrbitCameraRig>();
        if (orbit == null)
        {
            orbit = Undo.AddComponent<CastleOrbitCameraRig>(castleCamera.gameObject);
        }

        DogCinematicAutoMover mover = null;
        if (dog != null)
        {
            mover = dog.GetComponent<DogCinematicAutoMover>();
            if (mover == null)
            {
                mover = Undo.AddComponent<DogCinematicAutoMover>(dog);
            }
        }

        ConfigureThirdPerson(explorationCamera);
        AssignReferences(controller, director, brain, dogTransform, dogStartPoint, dogCameraPoint, dogLookPoint, castleCamera, entranceCamera, dogCamera, explorationCamera, orbit, mover);
        AssignBool(controller, "playOnStart", true);
        AssignBool(controller, "moveDogToStartOnIntro", true);
        AssignBool(controller, "allowSkip", true);
        AssignObjectReference(orbit, "target", castleCenter);
        if (mover != null)
        {
            AssignObjectReference(mover, "target", dogEntryPoint);
            Animator animator = dog.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                AssignObjectReference(mover, "animator", animator);
                AssignObjectReference(controller, "dogAnimator", animator);
            }
        }

        Selection.activeGameObject = introObject;
        introObject.SetActive(true);
        EditorUtility.SetDirty(introObject);
        EditorSceneManager.MarkSceneDirty(introObject.scene);
        Debug.Log("Entrada cinematografica creada. Se ejecutara automaticamente al presionar Play.");
    }

    private static CinemachineCamera CreateOrGetCamera(string name, Vector3 position, Transform follow, Transform lookAt)
    {
        GameObject cameraObject = GameObject.Find(name);
        if (cameraObject == null)
        {
            cameraObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(cameraObject, "Crear " + name);
        }

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
            camera = Undo.AddComponent<CinemachineCamera>(cameraObject);
        }

        camera.Priority = 0;
        camera.Follow = follow;
        camera.LookAt = lookAt;
        camera.Lens.FieldOfView = 55f;

        CinemachineRotationComposer composer = cameraObject.GetComponent<CinemachineRotationComposer>();
        if (composer == null)
        {
            composer = Undo.AddComponent<CinemachineRotationComposer>(cameraObject);
        }

        composer.Damping = new Vector2(0.35f, 0.35f);
        return camera;
    }

    private static void ConfigureThirdPerson(CinemachineCamera camera)
    {
        if (camera == null)
        {
            return;
        }

        CinemachineThirdPersonFollow thirdPerson = camera.GetComponent<CinemachineThirdPersonFollow>();
        if (thirdPerson == null)
        {
            thirdPerson = Undo.AddComponent<CinemachineThirdPersonFollow>(camera.gameObject);
        }

        thirdPerson.Damping = new Vector3(0.25f, 0.35f, 0.25f);
        thirdPerson.ShoulderOffset = new Vector3(0f, 0.55f, 0f);
        thirdPerson.VerticalArmLength = 0.25f;
        thirdPerson.CameraDistance = 3.2f;
        thirdPerson.CameraSide = 0.5f;
    }

    private static void AssignReferences(
        CinematicIntroController controller,
        PlayableDirector director,
        CinemachineBrain brain,
        Transform dog,
        Transform dogStartPoint,
        Transform dogCameraPoint,
        Transform dogLookPoint,
        CinemachineCamera castleCamera,
        CinemachineCamera entranceCamera,
        CinemachineCamera dogCamera,
        CinemachineCamera explorationCamera,
        CastleOrbitCameraRig orbit,
        DogCinematicAutoMover mover)
    {
        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("timelineDirector").objectReferenceValue = director;
        serialized.FindProperty("cinemachineBrain").objectReferenceValue = brain;
        serialized.FindProperty("dog").objectReferenceValue = dog;
        serialized.FindProperty("dogStartPoint").objectReferenceValue = dogStartPoint;
        serialized.FindProperty("castleCamera").objectReferenceValue = castleCamera;
        serialized.FindProperty("entranceCamera").objectReferenceValue = entranceCamera;
        serialized.FindProperty("dogPresentationCamera").objectReferenceValue = dogCamera;
        serialized.FindProperty("explorationCamera").objectReferenceValue = explorationCamera;
        serialized.FindProperty("castleOrbit").objectReferenceValue = orbit;
        serialized.FindProperty("dogAutoMover").objectReferenceValue = mover;
        serialized.FindProperty("dogCameraPoint").objectReferenceValue = dogCameraPoint;
        serialized.FindProperty("dogLookPoint").objectReferenceValue = dogLookPoint;
        serialized.ApplyModifiedProperties();
    }

    private static void AssignObjectReference(Object target, string propertyName, Object value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
            serialized.ApplyModifiedProperties();
        }
    }

    private static void AssignBool(Object target, string propertyName, bool value)
    {
        SerializedObject serialized = new SerializedObject(target);
        SerializedProperty property = serialized.FindProperty(propertyName);
        if (property != null)
        {
            property.boolValue = value;
            serialized.ApplyModifiedProperties();
        }
    }

    private static GameObject GetOrCreateRoot(string name)
    {
        GameObject found = GameObject.Find(name);
        if (found != null)
        {
            return found;
        }

        GameObject created = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(created, "Crear " + name);
        return created;
    }

    private static void PositionDogRoutePoints(Transform dogStartPoint, Transform dogEntryPoint)
    {
        GameObject entranceFloor = GameObject.Find("PisoEntrada (1)");
        Vector3 floorPosition = entranceFloor != null ? entranceFloor.transform.position : new Vector3(-2.69f, 0.12f, -7.22f);
        float dogRootHeight = 0.03f;

        dogStartPoint.position = new Vector3(floorPosition.x, dogRootHeight, floorPosition.z - 1.4f);
        dogEntryPoint.position = new Vector3(floorPosition.x, dogRootHeight, floorPosition.z + 1.8f);

        Vector3 direction = dogEntryPoint.position - dogStartPoint.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            dogStartPoint.rotation = rotation;
            dogEntryPoint.rotation = rotation;
        }
    }

    private static Transform GetOrCreateChild(Transform parent, string name, Vector3 localPosition)
    {
        if (parent == null)
        {
            return GetOrCreateRoot(name).transform;
        }

        Transform existing = parent.Find(name);
        if (existing != null)
        {
            existing.localPosition = localPosition;
            return existing;
        }

        GameObject child = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(child, "Crear " + name);
        child.transform.SetParent(parent, false);
        child.transform.localPosition = localPosition;
        child.transform.localRotation = Quaternion.identity;
        return child.transform;
    }

    private static Vector3 GetBoundsCenter(GameObject root)
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
}
