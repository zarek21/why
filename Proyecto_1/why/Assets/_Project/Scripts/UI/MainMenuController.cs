using UnityEngine;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    private UIDocument uiDocument;
    private Label subtitleLabel; 
    private bool isTransitioning = false;

    void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        VisualElement root = uiDocument.rootVisualElement;

        subtitleLabel = root.Q<Label>("SubtitleLabel");
    }

    void Update()
    {
        // Detectar pulsación solo si no estamos ya transicionando
        if (!isTransitioning && Input.anyKeyDown)
        {
            isTransitioning = true;
            
            // Ocultar el texto de UI Toolkit
            if (subtitleLabel != null)
            {
                subtitleLabel.style.display = DisplayStyle.None;
            }

            Debug.Log("¡Transición a los botones iniciada!");
            
            // Aquí llamaremos a la función que muestra los botones "Jugar", "Opciones"
        }
    }
}