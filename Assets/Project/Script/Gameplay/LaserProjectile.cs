using UnityEngine;

public class LaserProjectile : MonoBehaviour
{
    [SerializeField] private float velocidad = 20f;
    [SerializeField] private float duracion = 3f;

    private ObjectPool _pool;
    private float _tiempoActivo;

    public void Inicializar(ObjectPool pool)
    {
        _pool = pool;
    }

    private void OnEnable()
    {
        _tiempoActivo = 0f;
    }

    private void Update()
    {
        transform.Translate(Vector3.forward * velocidad * Time.deltaTime);

        _tiempoActivo += Time.deltaTime;
        if (_tiempoActivo >= duracion)
        {
            DevolverAlPool();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;
        DevolverAlPool();
    }

    private void DevolverAlPool()
    {
        if (_pool != null)
        {
            _pool.Devolver(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}