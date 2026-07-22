using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Pregunta
{
    [TextArea(2, 5)]
    public string acertijo;

    public List<string> respuestas = new List<string>();

    [Range(0, 3)]
    public int respuestaCorrecta;
}