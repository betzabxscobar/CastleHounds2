using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 8f;
    [SerializeField] private float acceleration = 12f;

    [Header("Rotación")]
    [SerializeField] private float rotationSpeed = 360f;

    [Header("Cámara")]
    [SerializeField] private Transform cameraTransform;

    [Header("Animaciones")]
    [SerializeField] private Animator animator;

    [Header("Gravedad")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.3f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float terminalVelocity = 25f;


    private CharacterController controller;
    private PlayerControls controls;

    private Vector2 moveInput;
    private bool sprinting;

    private Vector3 currentVelocity;
    private float verticalVelocity;



    private void Awake()
    {
        controller = GetComponent<CharacterController>();

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
        controls.Enable();
    }


    private void OnDisable()
    {
        controls.Disable();
    }



    private void Update()
    {

        float horizontal = moveInput.x;
        float vertical = moveInput.y;



        // =========================
        // ANIMACIONES
        // =========================

        if (animator != null)
        {
            animator.SetFloat("Horizontal", horizontal);
            animator.SetFloat("Vertical", vertical);

            animator.SetBool("Running", sprinting);
        }




        // =========================
        // GRAVEDAD
        // =========================

        bool grounded = groundCheck != null && Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundLayer
        );


        if (grounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }


        verticalVelocity += gravity * Time.deltaTime;
        verticalVelocity = Mathf.Max(verticalVelocity, -terminalVelocity);




        // =========================
        // MOVIMIENTO SEGÚN CÁMARA
        // =========================


        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;


        camForward.y = 0;
        camRight.y = 0;


        camForward.Normalize();
        camRight.Normalize();



        Vector3 movement =
            camForward * vertical +
            camRight * horizontal;



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



        // Aplicar gravedad
        currentVelocity.y = verticalVelocity;




        // =========================
        // MOVER
        // =========================

        controller.Move(
            currentVelocity *
            Time.deltaTime
        );
    }
}