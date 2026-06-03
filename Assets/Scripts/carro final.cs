using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Requerido para modificar componentes de Texto y UI
using UnityEngine.InputSystem; // Importamos el nuevo Input System
using UnityEngine.Video; // Requerido para reproducir videos
using UnityEngine.SceneManagement; // Requerido para reiniciar la escena

public class carro : MonoBehaviour
{
    [Header("Configuración de UI Final")]
    public GameObject pantallaNegra; // El objeto (Panel) que tiene el fondo negro
    public Text textoFinal;          // El componente de Texto para el mensaje
    public Button botonReintentar;   // El componente Botón para reiniciar

    [Header("Configuración de Secuencia Final")]
    public SecuenciaCinematica secuenciaFinal; // Referencia al nuevo Asset de secuencia

    [Header("Configuración de Video (Legacy)")]
    public VideoClip videoFinal;
    private VideoPlayer videoPlayer; // Referencia al componente VideoPlayer de la cámara

    private bool jugadorCerca = false; // Indica si el jugador está en el rango de interacción

    private void Start()
    {
        // 1. Al empezar el juego, nos aseguramos de que TODO esté oculto
        if (pantallaNegra != null) 
            pantallaNegra.SetActive(false);
            
        if (textoFinal != null) 
            textoFinal.gameObject.SetActive(false);

        if (botonReintentar != null)
        {
            botonReintentar.gameObject.SetActive(false);
            botonReintentar.onClick.AddListener(ReiniciarJuego); // Asigna la función al hacer clic
        }
    }

    private void Update()
    {
        // Si el jugador está en el área y presiona la tecla E, termina el juego
        if (jugadorCerca && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log("Interacción con el carro detectada. Finalizando juego...");
            
            if (secuenciaFinal != null)
            {
                // Bloqueamos el movimiento y usamos el manejador central
                movimiento playerMove = FindObjectOfType<movimiento>();
                if (playerMove != null) playerMove.enabled = false;
                ManejadorCinematicas.Instancia.Reproducir(secuenciaFinal, FinalizarJuego);
            }
            else
            {
                PrepararYReproducirVideo();
            }
        }
    }

    private void PrepararYReproducirVideo()
    {
        // Buscamos el VideoPlayer en la cámara principal (igual que en movimiento.cs)
        if (Camera.main != null)
        {
            videoPlayer = Camera.main.GetComponent<VideoPlayer>();
        }

        if (videoPlayer != null && videoFinal != null)
        {
            // Desactivamos el script de movimiento para que el jugador no se mueva durante el video
            movimiento playerMove = FindObjectOfType<movimiento>();
            if (playerMove != null) playerMove.enabled = false;

            videoPlayer.clip = videoFinal;
            videoPlayer.targetCamera = null; // Evita frames estáticos previos
            
            videoPlayer.prepareCompleted -= AlCompletarPreparacion;
            videoPlayer.prepareCompleted += AlCompletarPreparacion;

            videoPlayer.Stop();
            videoPlayer.Prepare();
        }
        else
        {
            // Si no hay video asignado, vamos directo al final
            FinalizarJuego();
        }
    }

    private void AlCompletarPreparacion(VideoPlayer source)
    {
        source.prepareCompleted -= AlCompletarPreparacion;
        source.targetCamera = Camera.main;

        source.loopPointReached -= AlTerminarVideo;
        source.loopPointReached += AlTerminarVideo;

        source.Play();
    }

    private void AlTerminarVideo(VideoPlayer source)
    {
        source.loopPointReached -= AlTerminarVideo;
        source.Stop();
        source.targetCamera = null;

        // Una vez terminado el video, mostramos la pantalla de fin
        FinalizarJuego();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<movimiento>() != null || other.CompareTag("Player"))
        {
            jugadorCerca = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Si el jugador sale del área, ya no puede interactuar
        if (other.GetComponent<movimiento>() != null || other.CompareTag("Player"))
        {
            jugadorCerca = false;
        }
    }

    private void FinalizarJuego()
    {
        // 1. Mostramos el fondo negro
        if (pantallaNegra != null)
        {
            pantallaNegra.SetActive(true);
        }

        // 2. Mostramos el mensaje de texto
        if (textoFinal != null)
        {
            textoFinal.gameObject.SetActive(true);
            textoFinal.text = "¡terminaste el juego!";
            textoFinal.color = Color.white; // Aseguramos que el texto sea blanco para que resalte
            textoFinal.transform.SetAsLastSibling(); // Mueve el texto al frente de otros elementos en el Canvas
        }

        if (botonReintentar != null)
        {
            botonReintentar.gameObject.SetActive(true);
            botonReintentar.transform.SetAsLastSibling(); // Mueve el botón al frente para que el panel no lo bloquee
        }

        // 3. Detenemos el tiempo después de activar la UI
        // A veces pausar el tiempo antes de activar objetos UI puede causar problemas visuales
        Time.timeScale = 0f;

        // 4. Liberamos el cursor del mouse por si el jugador desea cerrar la aplicación
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReiniciarJuego()
    {
        // Es MUY importante devolver el tiempo a 1 antes de cargar la escena
        // de lo contrario, el juego empezará pausado.
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
