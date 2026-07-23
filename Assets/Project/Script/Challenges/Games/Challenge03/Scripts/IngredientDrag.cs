using UnityEngine;
using UnityEngine.InputSystem;


public class IngredientDrag : MonoBehaviour
{

    private PotionInput input;


    private bool dragging;



    [Header("Nombre del ingrediente")]
    public string ingredientName;



    private Vector3 initialPosition;



    private bool placed;



    [Header("Entrada al caldero")]
    public float enterSpeed = 5f;



    private bool enteringCauldron;



    private Transform cauldronPoint;



    [Header("Altura al agarrar")]
    public float liftHeight = 2f;



    private float originalY;





    void Awake()
    {

        input = new PotionInput();


        input.Potion.Click.performed += ctx =>
        {
            TrySelect();
        };

    }





    void OnEnable()
    {
        input.Enable();
    }





    void OnDisable()
    {
        input.Disable();
    }





    void Start()
    {

        initialPosition = transform.position;

        originalY = transform.position.y;

    }





    void Update()
    {


        if (enteringCauldron)
        {

            transform.position =
            Vector3.MoveTowards(
                transform.position,
                cauldronPoint.position,
                enterSpeed * Time.deltaTime
            );



            if (Vector3.Distance(
                transform.position,
                cauldronPoint.position) < 0.05f)
            {

                FinishCauldronEnter();

            }


            return;

        }





        if (dragging)
        {


            Ray ray =
            Camera.main.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );



            Plane plane =
            new Plane(
                Vector3.up,
                Vector3.up * originalY
            );



            if (plane.Raycast(
                ray,
                out float distance))
            {


                Vector3 point =
                ray.GetPoint(distance);



                transform.position =
                new Vector3(
                    point.x,
                    originalY + liftHeight,
                    point.z
                );

            }





            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                Drop();
            }


        }

    }





    void TrySelect()
    {

        // Si ya fue usado no se puede agarrar
        if (placed)
            return;



        Ray ray =
        Camera.main.ScreenPointToRay(
            Mouse.current.position.ReadValue()
        );



        if (Physics.Raycast(
            ray,
            out RaycastHit hit))
        {


            if (hit.transform == transform)
            {


                dragging = true;



                transform.position =
                new Vector3(
                    transform.position.x,
                    originalY + liftHeight,
                    transform.position.z
                );


            }

        }

    }





    void Drop()
    {

        dragging = false;


        transform.position =
        initialPosition;

    }





    public void PlaceInCauldron()
    {

        if (placed)
            return;



        placed = true;


        dragging = false;



        gameObject.SetActive(false);



        if (PotionManager.Instance != null)
        {

            PotionManager.Instance.AddIngredient(
                ingredientName
            );

        }

    }





    public void EnterCauldron(Transform point)
    {

        if (placed)
            return;



        placed = true;


        dragging = false;



        cauldronPoint = point;


        enteringCauldron = true;


    }





    void FinishCauldronEnter()
    {

        enteringCauldron = false;



        gameObject.SetActive(false);



        if (PotionManager.Instance != null)
        {

            PotionManager.Instance.AddIngredient(
                ingredientName
            );

        }

    }





    public void ResetIngredient()
    {

        placed = false;


        dragging = false;


        enteringCauldron = false;



        gameObject.SetActive(true);



        transform.position =
        initialPosition;

    }

}