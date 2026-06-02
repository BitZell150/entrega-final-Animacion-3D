using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using UnityEngine.UI; // Requerido para modificar el texto de la UI

public class movimiento : MonoBehaviour
{
    public float speed = 2.0f;
    public float sprintSpeed = 4.0f;
    public float jumpHeight = 1.5f;
    public float jumpImpulseDelay = 0.12f;
    public float gravity = -15f;
    public float doubleTapThreshold = 0.25f;
    public float sprintDuration = 0.75f;

    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private Animator animator;
    private Transform cam;
    private Camera mainCam; // Referencia al componente Camera
    private GameObject objetoCercano; // Objeto que está en el rango de alcance
    private float lastForwardTapTime = -10f;
    private float sprintTimer;
    private float jumpTimer = -1f;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int JumpHash = Animator.StringToHash("Jump");
    private static readonly int BackflipHash = Animator.StringToHash("Backflip");
    private static readonly int WarmupHash = Animator.StringToHash("Warmup");

    [Header("UI e Interacción")]
    public GameObject uiInteractuar; // El texto de "Presiona E..."

    [Header("Cinemática")]
    public VideoPlayer videoPlayer; // El componente VideoPlayer en la cámara

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("No se encontro Animator en el objeto del personaje. Los triggers de animacion no se ejecutaran.");
        }

        // Buscamos la cámara principal en tiempo de ejecución
        if (mainCam == null)
            mainCam = Camera.main;

        if (mainCam != null)
        {
            cam = mainCam.transform;
            
            // Si el VideoPlayer no fue asignado manualmente, lo buscamos en la cámara
            if (videoPlayer == null)
            {
                videoPlayer = mainCam.GetComponent<VideoPlayer>();
            }
        }
        else
        {
            Debug.LogError("No se encontró una Main Camera en la escena. Asegúrate de que tu cámara tenga el Tag 'MainCamera'.");
        }

        // Ocultar el texto al empezar
        if (uiInteractuar != null)
            uiInteractuar.SetActive(false);

        // Si encontramos un VideoPlayer, nos aseguramos de que empiece limpio
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

            if (Keyboard.current.wKey.wasPressedThisFrame)
            {
                if (Time.time - lastForwardTapTime <= doubleTapThreshold)
                {
                    sprintTimer = sprintDuration;
                }

                lastForwardTapTime = Time.time;
            }
        }

        // --- Integración con Animator ---
        float animatorSpeed = 0f;
        bool isSprinting = (sprintTimer > 0f && vertical > 0f) || (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed);
        if (horizontal != 0f || vertical != 0f)
        {
            animatorSpeed = (isSprinting && vertical > 0f) ? 2f : 1f;
        }

        bool jumpPressed = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        bool backflipPressed = Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame;
        bool warmupPressed = Keyboard.current != null && Keyboard.current.cKey.wasPressedThisFrame;

        // Detectamos si estamos en la animación de Backflip O si estamos transicionando hacia/desde ella
        bool estaEnBackflip = false;
        if (animator != null)
        {
            // También verificamos IsInTransition para evitar que el script tome el control antes de que termine de volver al Idle
            estaEnBackflip = animator.GetCurrentAnimatorStateInfo(0).IsName("Backflip") || 
                             animator.GetNextAnimatorStateInfo(0).IsName("Backflip") ||
                             (animator.IsInTransition(0) && animator.GetNextAnimatorStateInfo(0).IsName("Idle"));
        }

        // Solo permitimos activar el backflip si no estamos ya haciendo uno
        if (backflipPressed && !estaEnBackflip)
        {
            Debug.Log("BACKFLIP");
            animatorSpeed = 0f;
        }
        else if (estaEnBackflip)
        {
            animatorSpeed = 0f;
        }

        if (animator != null)
        {
            // Enviamos el valor directamente. 
            // La suavidad ahora la controlará el "Transition Duration" que configuramos en el Animator.
            animator.SetFloat(SpeedHash, animatorSpeed);

            if (jumpPressed && isGrounded)
            {
                animator.ResetTrigger(JumpHash);
                animator.SetTrigger(JumpHash);
                jumpTimer = jumpImpulseDelay;
            }

            if (backflipPressed && !estaEnBackflip)
            {
                animator.ResetTrigger(BackflipHash);
                animator.SetTrigger(BackflipHash);
            }

            if (warmupPressed)
            {
                animator.ResetTrigger(WarmupHash);
                animator.SetTrigger(WarmupHash);
            }
        }

        if (jumpTimer >= 0f)
        {
            jumpTimer -= Time.deltaTime;
            if (jumpTimer <= 0f && isGrounded)
            {
                playerVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                jumpTimer = -1f;
            }
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
                Debug.LogError("¡ERROR! No se encontró un VideoPlayer. Asegúrate de que la Main Camera tenga un componente VideoPlayer.");
            }

            // Destruimos el objeto y limpiamos la UI
            Destroy(objetoCercano);
            objetoCercano = null;

            if (uiInteractuar != null)
                uiInteractuar.SetActive(false);
        }

        // Si no hay cámara, no podemos calcular el movimiento relativo
        if (cam == null) return;

        // --- Lógica de Movimiento Relativo a la Cámara ---
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        // Eliminamos el componente Y para que el personaje no intente "volar" si miras hacia arriba
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        // Si está haciendo el backflip, anulamos el input de movimiento para que no se deslice por el suelo
        float moveVertical = estaEnBackflip ? 0f : vertical;
        float moveHorizontal = estaEnBackflip ? 0f : horizontal;

        // Calculamos la dirección del movimiento multiplicando la entrada por la orientación de la cámara
        Vector3 move = (camForward * moveVertical + camRight * moveHorizontal);

        if (move.magnitude > 1f) move.Normalize();

        // Aplicar movimiento
        float currentSpeed = speed;
        if (isSprinting && vertical > 0f)
        {
            currentSpeed = sprintSpeed;
            if (sprintTimer > 0f)
            {
                sprintTimer -= Time.deltaTime;
            }
        }
        else
        {
            sprintTimer = 0f;
        }

        controller.Move(move * currentSpeed * Time.deltaTime);

        // Rotar al personaje hacia la dirección de movimiento
        if (move != Vector3.zero && !estaEnBackflip)
        {
            Quaternion targetRotation = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // --- Aplicar Gravedad ---
        // Si estamos en Backflip, pausamos la gravedad para que la animación pueda elevarse si es necesario
        if (!estaEnBackflip)
        {
            playerVelocity.y += gravity * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);
        }
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