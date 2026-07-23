using UnityEngine;

public class Giro360 : MonoBehaviour
{
    [SerializeField] private float velocidad = 40f;

    private void Update()
    {
        transform.Rotate(
            0f,
            -velocidad * Time.deltaTime,
            0f,
            Space.Self
        );
    }
}