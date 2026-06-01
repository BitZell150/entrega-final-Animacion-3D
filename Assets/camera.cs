using UnityEngine;
using UnityEngine.InputSystem;

public class CamaraOrbital : MonoBehaviour
{
    [Header("Referencias")]
    public Transform objetivo; // Arrastra aquí a tu personaje "bonytesy"

    [Header("Configuracion de Orbita")]
    public float distancia = 3.0f;    // Más cerca para estilo RE4
    public float lateralOffset = 0.6f; // Desplazamiento al hombro (derecha)
    public float verticalOffset = 0.3f; // Elevación sobre el hombro
    public float velocidadX = 10.0f; // Velocidad de rotación horizontal ajustada
    public float velocidadY = 7.5f;  // Velocidad de rotación vertical ajustada
    public float horizontalFOV = 80f; // Field of View horizontal deseado

    [Header("Limites")]
    public float limiteYMin = -20f;   // Límite para no enterrarse en el suelo
    public float limiteYMax = 80f;    // Límite para no dar la vuelta completa por arriba

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        // Obtenemos las rotaciones iniciales de la cámara
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        // Opcional: Esconde el cursor del ratón para que no moleste al jugar
        Cursor.lockState = CursorLockMode.Locked;

        // Configuramos el FOV horizontal. 
        // Unity usa FOV vertical, así que lo convertimos usando la relación de aspecto actual.
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = Camera.HorizontalToVerticalFieldOfView(horizontalFOV, cam.aspect);
        }
    }

    // Usamos LateUpdate para que la cámara se mueva DESPUÉS de que el personaje se haya movido
    void LateUpdate()
    {
        if (objetivo != null && Mouse.current != null)
        {
            // 1. Leer el movimiento del ratón (Nuevo Input System)
            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            x += mouseDelta.x * velocidadX * 0.01f;
            y -= mouseDelta.y * velocidadY * 0.01f;

            // 2. Limitar la rotación vertical para que no se voltee la cámara
            y = Mathf.Clamp(y, limiteYMin, limiteYMax);

            // 3. Convertir los ángulos en una rotación de Unity (Cuaternión)
            Quaternion rotacion = Quaternion.Euler(y, x, 0);

            // 4. Calcular la nueva posición aplicando el desplazamiento lateral y vertical (RE4 style)
            Vector3 posicionDefinida = rotacion * new Vector3(lateralOffset, verticalOffset, -distancia) + objetivo.position;

            // 5. Aplicar la rotación y posición a la cámara
            transform.rotation = rotacion;
            transform.position = posicionDefinida;
        }
    }
}