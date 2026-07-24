using UnityEngine;

public class GiroFotoAvatar : MonoBehaviour
{
    [SerializeField] private float velocidad = 30f;

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
