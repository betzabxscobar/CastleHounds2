using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int cantidadInicial = 10;

    private readonly Queue<GameObject> _disponibles = new Queue<GameObject>();

    private void Awake()
    {
        for (int i = 0; i < cantidadInicial; i++)
        {
            GameObject obj = Instantiate(prefab, transform);
            obj.SetActive(false);
            _disponibles.Enqueue(obj);
        }
    }

    public GameObject Obtener(Vector3 posicion, Quaternion rotacion)
    {
        GameObject obj = _disponibles.Count > 0
            ? _disponibles.Dequeue()
            : Instantiate(prefab, transform);

        obj.transform.SetPositionAndRotation(posicion, rotacion);
        obj.SetActive(true);
        return obj;
    }

    public void Devolver(GameObject obj)
    {
        obj.SetActive(false);
        _disponibles.Enqueue(obj);
    }
}