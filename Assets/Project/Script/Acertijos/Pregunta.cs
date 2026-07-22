using System;
using UnityEngine;

[Serializable]
public class Pregunta
{
    [TextArea(2, 5)]
    public string acertijo;

    public string[] respuestas = new string[4];

    [Range(0, 3)]
    public int respuestaCorrecta;
}