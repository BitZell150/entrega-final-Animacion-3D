using UnityEngine;

public class ObjetoRotatorio : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    // Velocidad en grados por segundo para cada eje
    public Vector3 velocidadRotacion = new Vector3(80f, 0f, 0f); 

    void Update()
    {
        // Multiplicamos la velocidad por Time.deltaTime para que la rotación 
        // sea fluida y constante en cualquier computadora (FPS independientes)
        transform.Rotate(velocidadRotacion * Time.deltaTime);
    }
}