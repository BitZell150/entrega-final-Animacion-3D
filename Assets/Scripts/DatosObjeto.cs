using UnityEngine;
using UnityEngine.Video;

public class DatosObjeto : MonoBehaviour
{
    [Header("Configuración de Cinemática")]
    [Tooltip("Arrastra aquí el clip de video (.mp4) que quieres que se reproduzca al recoger este objeto.")]
    public VideoClip videoAsociado;
}
