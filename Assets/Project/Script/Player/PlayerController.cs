using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // En la arena de combate la camara (CameraFollow) es cinematica: no gira
    // con el perro, solo lo sigue en posicion. Por eso el movimiento ahi no
    // puede basarse en hacia donde mira la camara (crearia un bucle o
    // controles invertidos segun donde se ubique la camara); en su lugar usa
    // ejes fijos del mundo alineados con el pasillo spawn -> Portal (eje Z).
    // En las demas escenas (Cinemachine u otra camara) se sigue usando el
    // movimiento relativo a la camara de siempre.
    private const string ArenaSceneName = "_DemoScene";
    private static readonly Vector3 ArenaForward = Vector3.back;
    private static readonly Vector3 ArenaRight = Vector3.right;
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private float acceleration = 7f;

    [Header("Rotación")]
    [SerializeField] private float rotationSpeed = 140f;

    [Header("Cámara")]
    [SerializeField] private Transform cameraTransform;

    [Header("Animaciones")]
    [SerializeField] private Animator animator;

    [Header("Gravedad")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float terminalVelocity = 25f;

    [Header("Anti-atravesar suelo")]
    [Tooltip("Si el jugador cae por debajo de esta Y (relativa a su Y inicial) se le vuelve a colocar en el último punto seguro sobre el suelo.")]
    [SerializeField] private float maxFallBelowStart = 15f;

    private CharacterController controller;
    private PlayerControls controls;
    private PlayerRespawn respawn;

    private Vector2 moveInput;
    private bool sprinting;

    private Vector3 currentVelocity;
    private float verticalVelocity;

    private Vector3 lastGroundedPosition;
    private bool hasSafetyPosition;
    private bool hasSpeedParam;
    private bool inputEnabled = true;
    private bool useFixedArenaAxes;



    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        respawn = GetComponent<PlayerRespawn>();
        useFixedArenaAxes = SceneManager.GetActiveScene().name == ArenaSceneName;

        if (animator != null)
        {
            foreach (var param in animator.parameters)
            {
                if (param.type == AnimatorControllerParameterType.Float && param.name == "Speed")
                {
                    hasSpeedParam = true;
                    break;
                }
            }
        }

        controls = new PlayerControls();


        controls.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
        };


        controls.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
        };


        controls.Player.Sprint.performed += ctx =>
        {
            sprinting = true;
        };


        controls.Player.Sprint.canceled += ctx =>
        {
            sprinting = false;
        };
    }



    private void OnEnable()
    {
        if (inputEnabled)
        {
            controls.Enable();
        }
    }


    private void OnDisable()
    {
        controls.Disable();
    }



    private void Update()
    {
        if (Time.deltaTime <= 0f)
        {
            return;
        }

        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
            }
            else
            {
                Debug.LogError("PlayerController no tiene cameraTransform ni existe Camera.main.", this);
                enabled = false;
                return;
            }
        }

        // La primera vez que corre Update (ya reposicionado por PlayerRespawn),
        // se toma esa posición como referencia segura para la red de seguridad anti-caída.
        if (!hasSafetyPosition)
        {
            lastGroundedPosition = transform.position;
            hasSafetyPosition = true;
        }


        float horizontal = moveInput.x;
        float vertical = moveInput.y;



        // =========================
        // ANIMACIONES
        // =========================

        if (animator != null && hasSpeedParam)
        {
            float animSpeed = moveInput.magnitude * (sprinting ? 2f : 1f);
            animator.SetFloat("Speed", animSpeed);
        }




        // =========================
        // GRAVEDAD
        // =========================

        Vector3 feetPosition = transform.position + controller.center - Vector3.up * (controller.height * 0.5f);

        // Sondeo de suelo con Raycast (no CheckSphere/OverlapSphere: esas tienen un
        // bug conocido de Unity/PhysX y NO detectan MeshColliders no convexos, como
        // el Plane del piso, cuando el punto de chequeo ya los está tocando).
        // Con el Raycast, en vez de dejar que la física "resuelva" el contacto con el
        // suelo, se calcula exactamente cuánto hay que moverse en Y para tocarlo y se
        // aplica ese valor directamente: así la Y queda pegada al suelo en vez de
        // depender de la resolución de colisión del CharacterController.
        const float groundSnapThreshold = 0.35f;
        const float groundProbeDistance = 50f;
        bool foundGround = Physics.Raycast(feetPosition + Vector3.up * 0.1f, Vector3.down, out RaycastHit groundHit, groundProbeDistance, ~0, QueryTriggerInteraction.Ignore);
        float heightAboveGround = foundGround ? (feetPosition.y - groundHit.point.y) : float.PositiveInfinity;

        bool grounded = foundGround && heightAboveGround <= groundSnapThreshold;

        float verticalMove;

        if (grounded)
        {
            // Se pega la Y exactamente al suelo detectado (sube si estaba hundido, baja si flotaba un poco).
            verticalMove = -heightAboveGround;
            verticalVelocity = -1f;
        }
        else
        {
            if (verticalVelocity < 0)
            {
                verticalVelocity = Mathf.Min(verticalVelocity, -1f);
            }

            verticalVelocity += gravity * Time.deltaTime;
            verticalVelocity = Mathf.Max(verticalVelocity, -terminalVelocity);

            verticalMove = verticalVelocity * Time.deltaTime;

            // No dejar que la caída de este frame atraviese el suelo detectado más abajo.
            if (foundGround && -verticalMove > heightAboveGround)
            {
                verticalMove = -heightAboveGround;
                verticalVelocity = verticalMove / Time.deltaTime;
            }
        }


        // =========================
        // RED DE SEGURIDAD
        // =========================
        // Si aun así el jugador terminó por debajo del suelo, se le devuelve
        // al último punto seguro (o al spawn) en vez de dejarlo caer para siempre.

        if (grounded)
        {
            lastGroundedPosition = transform.position;
        }
        else if (transform.position.y < lastGroundedPosition.y - maxFallBelowStart)
        {
            if (respawn != null)
            {
                respawn.Respawn();
            }
            else
            {
                controller.enabled = false;
                transform.position = lastGroundedPosition;
                controller.enabled = true;
            }

            verticalVelocity = -2f;
            return;
        }




        // =========================
        // MOVIMIENTO
        // =========================

        Vector3 moveForward;
        Vector3 moveRight;

        if (useFixedArenaAxes)
        {
            // Ejes fijos del mundo: la camara de la arena es cinematica y no
            // gira con el jugador, asi que no sirve como referencia direccional.
            moveForward = ArenaForward;
            moveRight = ArenaRight;
        }
        else
        {
            moveForward = cameraTransform.forward;
            moveRight = cameraTransform.right;

            moveForward.y = 0;
            moveRight.y = 0;

            moveForward.Normalize();
            moveRight.Normalize();
        }

        Vector3 movement =
            moveForward * vertical +
            moveRight * horizontal;



        if (movement.sqrMagnitude > 1)
            movement.Normalize();




        // =========================
        // ROTACIÓN NATURAL
        // =========================

        if (movement.sqrMagnitude > 0.01f)
        {
            // No rota si solamente va hacia atrás
            if (vertical >= 0)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(movement);

                transform.rotation =
                    Quaternion.RotateTowards(
                        transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );
            }
        }



        // =========================
        // VELOCIDAD SUAVE
        // =========================


        float speed = sprinting ? runSpeed : walkSpeed;


        Vector3 targetVelocity =
            movement * speed;


        currentVelocity =
            Vector3.Lerp(
                currentVelocity,
                targetVelocity,
                acceleration * Time.deltaTime
            );



        // Aplicar gravedad / snap al suelo
        currentVelocity.y = verticalMove / Time.deltaTime;




        // =========================
        // MOVER
        // =========================

        controller.Move(
            currentVelocity *
            Time.deltaTime
        );
    }

    public void SetInputEnabled(bool enabledInput)
    {
        inputEnabled = enabledInput;

        if (!enabledInput)
        {
            moveInput = Vector2.zero;
            sprinting = false;
            currentVelocity = Vector3.zero;
            controls.Disable();
            return;
        }

        if (isActiveAndEnabled)
        {
            controls.Enable();
        }
    }
}
