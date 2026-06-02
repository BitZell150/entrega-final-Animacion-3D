using UnityEngine;
using UnityEngine.Video;

public class DatosObjeto : MonoBehaviour
{
    [Header("Video")]
    public VideoClip videoAsociado;

    public void ReproducirVideo(Camera camara)
    {
        if (videoAsociado == null)
        {
            Debug.LogWarning($"[DatosObjeto] '{gameObject.name}' no tiene un VideoClip asignado.");
            return;
        }

        // Busca el VideoPlayer en la cámara o lo crea si no existe
        VideoPlayer vp = camara.GetComponent<VideoPlayer>();
        if (vp == null)
            vp = camara.gameObject.AddComponent<VideoPlayer>();

        // Renderiza el video ENCIMA de todo lo que ve la cámara, sin Canvas
        vp.renderMode    = VideoRenderMode.CameraFarPlane;
        vp.targetCamera  = camara;
        vp.targetCameraAlpha = 1f;          // Opacidad total (0 = transparente, 1 = sólido)
        vp.audioOutputMode = VideoAudioOutputMode.Direct;
        vp.playOnAwake   = false;
        vp.isLooping     = false;
        vp.clip          = videoAsociado;

        // Limpia suscripción anterior por si el jugador interactuó antes
        vp.loopPointReached -= AlTerminar;
        vp.loopPointReached += AlTerminar;

        vp.Play();
    }

    private void AlTerminar(VideoPlayer source)
    {
        source.loopPointReached -= AlTerminar;
        source.Stop();
        source.targetCamera = null; // Desconecta la cámara → el video desaparece
    }
}
