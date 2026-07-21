using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private ObjectPool poolProyectiles;
    [SerializeField] private Transform puntoDisparo;
    [SerializeField] private float cadencia = 0.5f;
    [SerializeField] private int dano = 25;

    private float _tiempoUltimoDisparo;

    private void Update()
    {
        if (Input.GetButton("Fire1"))
        {
            Disparar();
        }
    }

    private void Disparar()
    {
        if (Time.time - _tiempoUltimoDisparo < cadencia) return;
        if (poolProyectiles == null) return;

        _tiempoUltimoDisparo = Time.time;

        Vector3 posicion = puntoDisparo != null ? puntoDisparo.position : transform.position;
        Quaternion rotacion = puntoDisparo != null ? puntoDisparo.rotation : transform.rotation;

        GameObject proyectil = poolProyectiles.Obtener(posicion, rotacion);
        proyectil.GetComponent<LaserProjectile>()?.Inicializar(poolProyectiles);
    }

    public int Dano => dano;
}