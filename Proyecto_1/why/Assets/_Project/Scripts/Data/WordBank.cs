using System.Collections.Generic;
using System.Linq; 
using UnityEngine;

public static class WordBank
{
    private static List<string> rawDictionary = new List<string>()
    {
        // --- NIVEL 1: 5 LETRAS ---
        "PERRO", "GATO", "ARBOL", "LIBRO", "PLAYA", 
        "CAMPO", "HOTEL", "RELOJ", "LAPIZ", "SILLA",
        "APPLE", "MONTE", "VALLE", "NOCHE", "TARDE",

        // --- NIVEL 2: 8 LETRAS ---
        "EDIFICIO", "PROYECTO", "AVENTURA", "ELEFANTE", "HISTORIA", 
        "UNIVERSO", "GUITARRA", "LENGUAJE", "NOSOTROS", "CUADERNO",
        "MONTAÑAS", "PERFECTO", "PROGRAMA", "TROPICAL", "VALIENTE",

        // --- NIVEL 3: 12 LETRAS (Modo Dios) ---
        "CONSTRUCCION", "INTELIGENCIA", "ARQUITECTURA", "PROGRAMACION", 
        "ESPECTACULAR", "PRESIDENCIAL", "CONSECUENCIA", "TRANSPARENTE",
        "INDEPENDENCIA", "UNIVERSITARIO", "FOTOGRAFICOS", "ELECTRICIDAD"
    };

    public static List<string> GetWordsForLevel(int minLength, int maxLength)
    {
        List<string> filteredWords = rawDictionary
            .Where(w => w.Length >= minLength && w.Length <= maxLength)
            .ToList();

        if (filteredWords.Count == 0)
        {
            Debug.LogError($"¡ERROR CRÍTICO! No encontré palabras de {minLength}-{maxLength} letras.");
            return new List<string>() { "ERROR", "FALTA" };
        }

        return filteredWords;
    }
}