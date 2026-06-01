using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.UI; // Requerido para modificar el texto de la UI

public class movimiento : MonoBehaviour
{
    public float speed = 2.0f; 
    public float jumpHeight = 1.5f; 
    public float gravity = -15f;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private Transform cam;
    private Camera mainCam; // Referencia al componente Camera
    private GameObject objetoCercano; // Objeto que está en el rango de alcance

    [Header("UI e Interacción")]
    public GameObject uiInteractuar; // El texto de "Presiona E..."

    [Header("Cinemática")]
    public VideoPlayer videoPlayer; // El componente VideoPlayer en la cámara

    void Start()
    {
        controller = GetComponent<CharacterController>();
        mainCam = Camera.main;
        cam = mainCam.transform;

        // Ocultar el texto al empezar
        if (uiInteractuar != null)
            uiInteractuar.SetActive(false);

        // El VideoPlayer se queda enabled, pero sin cámara asignada para que no renderice nada
        if (videoPlayer != null)
        {
            videoPlayer.targetCamera = null;
        }
    }
    
    void Update()
    {
        // --- Lógica de Suelo ---
        isGrounded = controller.isGrounded;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }

        // --- Movimiento WASD (Velocidad Constante) ---
        float horizontal = 0f;
        float vertical = 0f;
        if (Keyboard.current != null)
        {
            horizontal = (Keyboard.current.dKey.isPressed ? 1f : 0f) - (Keyboard.current.aKey.isPressed ? 1f : 0f);
            vertical = (Keyboard.current.wKey.isPressed ? 1f : 0f) - (Keyboard.current.sKey.isPressed ? 1f : 0f);
        }

        // --- Lógica de Interacción (Tecla E) ---
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame && objetoCercano != null)
        {
            // Primero verificamos que el VideoPlayer esté asignado para evitar el error
            if (videoPlayer != null)
            {
                // Intentamos obtener el video específico del objeto
                DatosObjeto datos = objetoCercano.GetComponent<DatosObjeto>();
                if (datos != null && datos.videoAsociado != null)
                {
                    videoPlayer.clip = datos.videoAsociado;
                    ActivarVideo();
                }
                else
                {
                    ActivarVideo(); // Si no tiene video específico, usa el que ya está por defecto
                }
            }
            else
            {
                Debug.LogError("¡ERROR! El slot 'Video Player' está vacío en el Inspector del personaje (" + gameObject.name + "). Arrastra la Main Camera ahí.");
            }
            
            // Destruimos el objeto y limpiamos la UI
            Destroy(objetoCercano);
            objetoCercano = null;

            if (uiInteractuar != null)
                uiInteractuar.SetActive(false);
        }

        // --- Lógica de Movimiento Relativo a la Cámara ---
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        // Eliminamos el componente Y para que el personaje no intente "volar" si miras hacia arriba
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // Calculamos la dirección del movimiento multiplicando la entrada por la orientación de la cámara
        Vector3 move = (camForward * vertical + camRight * horizontal);
        
        if (move.magnitude > 1f) move.Normalize();

        // Aplicar movimiento
        controller.Move(move * speed * Time.deltaTime);

        // Rotar al personaje hacia la dirección de movimiento
        if (move != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // --- Salto con Barra Espaciadora ---
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // --- Aplicar Gravedad ---
        playerVelocity.y += gravity * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }
    
    private void ActivarVideo()
    {
        if (videoPlayer != null)
        {
            // Desconectamos la cámara temporalmente para evitar frames estáticos
            videoPlayer.targetCamera = null;
            
            // Limpiamos suscripciones previas por seguridad
            videoPlayer.prepareCompleted -= AlCompletarPreparacion;
            videoPlayer.prepareCompleted += AlCompletarPreparacion;
            
            // Forzamos la preparación del motor de video
            videoPlayer.Stop();
            videoPlayer.Prepare();
        }
    }

    private void AlCompletarPreparacion(VideoPlayer source)
    {
        source.prepareCompleted -= AlCompletarPreparacion;

        // AHORA que el motor confirma que los datos están listos, conectamos la cámara
        source.targetCamera = mainCam;

        source.loopPointReached -= AlTerminarVideo;
        source.loopPointReached += AlTerminarVideo;
        
        source.Play();
    }

    private void AlTerminarVideo(VideoPlayer source)
    {
        source.loopPointReached -= AlTerminarVideo;
        source.Stop(); // Detenemos la reproducción por completo
        source.targetCamera = null;
    }

    // Detectar cuando entramos en el rango de un objeto recolectable
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            objetoCercano = other.gameObject;

            if (uiInteractuar != null)
            {
                Text textoUI = uiInteractuar.GetComponentInChildren<Text>();
                if (textoUI != null)
                {
                    textoUI.text = "Presiona E para recoger";
                }

                uiInteractuar.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (objetoCercano == other.gameObject)
        {
            objetoCercano = null;
            if (uiInteractuar != null)
                uiInteractuar.SetActive(false); // Oculta el mensaje si te alejas
        }
    }
}
