using UnityEngine;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "WhyGame/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    [Header("Supervivencia")]
    public int maxLives = 3;
    
    [Header("Configuración del Nivel")]
    public int levelIndex;          
    public string levelName;        

    [Header("Dificultad")]
    public int targetFloors;       
    public float baseTimePerWord;   
    
    [Header("Restricciones de Palabras")]
    public int minWordLength;       
    public int maxWordLength;       
    
    [Header("Economía")]
    public int scoreBonus;          
}