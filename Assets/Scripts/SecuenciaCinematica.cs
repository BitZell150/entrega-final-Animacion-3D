using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NuevaSecuencia", menuName = "Cinematica/Secuencia de Imagenes")]
public class SecuenciaCinematica : ScriptableObject
{
    [Header("Configuración de Animación")]
    [Tooltip("Puedes arrastrar los sprites aquí o usar el botón de carga automática si tienes el script de Editor.")]
    public Sprite[] frames;
    
    [Tooltip("Segundos entre cada imagen")]
    public float tiempoPorFrame = 0.1f;
}