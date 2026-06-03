using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ManejadorCinematicas : MonoBehaviour
{
    public static ManejadorCinematicas Instancia { get; private set; }

    [Header("Referencias UI")]
    public Image pantallaUI;
    public GameObject fondoNegro;

    private void Awake()
    {
        if (Instancia == null) Instancia = this;
        else Destroy(gameObject);

        // Asegurarse de que empiece oculto
        if (pantallaUI != null) pantallaUI.gameObject.SetActive(false);
        if (fondoNegro != null) fondoNegro.SetActive(false);
    }

    public void Reproducir(SecuenciaCinematica secuencia, Action alTerminar = null)
    {
        if (secuencia == null || secuencia.frames.Length == 0)
        {
            alTerminar?.Invoke();
            return;
        }
        StartCoroutine(RutinaSecuencia(secuencia, alTerminar));
    }

    private IEnumerator RutinaSecuencia(SecuenciaCinematica secuencia, Action alTerminar)
    {
        if (fondoNegro != null) fondoNegro.SetActive(true);
        if (pantallaUI != null) pantallaUI.gameObject.SetActive(true);

        foreach (Sprite frame in secuencia.frames)
        {
            if (pantallaUI != null) pantallaUI.sprite = frame;
            yield return new WaitForSeconds(secuencia.tiempoPorFrame);
        }

        if (pantallaUI != null) pantallaUI.gameObject.SetActive(false);
        if (fondoNegro != null) fondoNegro.SetActive(false);

        alTerminar?.Invoke();
    }
}