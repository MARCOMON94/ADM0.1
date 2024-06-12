using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // Botones de la UI
    public Button moveButton;
    public Button attackButton;
    public Button passTurnButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Image gameOverImage;

    // Textos de salud de los personajes
    public TextMeshProUGUI[] characterHealthTexts;
    private PlayerMove currentPlayerMove;
    public TextMeshProUGUI roundText;

    // Indicadores de turno
    public Image[] turnIndicators;
    private Dictionary<string, int> characterTurnIndexMap = new Dictionary<string, int>();

    private Dictionary<CharacterStatsSO, CharacterStatsSnapshot> initialStats;

    // Introducción
    public List<Sprite> introImages;
    public Image displayImage; // UI Image component to show the sprites
    public AudioSource introMusic;
    public AudioSource sceneMusic;
    private int currentImageIndex = 0;
    private bool isIntroPlaying = false;

    void Awake()
    {
        /* 
        Inicializa la instancia del UIManager asegurando que solo haya 
        una instancia activa en el juego.
        */
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        /* 
        Inicializa las estadísticas de los personajes y añade listeners 
        a los botones para manejar los eventos de clic.
        */
        initialStats = new Dictionary<CharacterStatsSO, CharacterStatsSnapshot>();
        CharacterStatsSO[] allStats = Resources.FindObjectsOfTypeAll<CharacterStatsSO>();
        foreach (CharacterStatsSO stats in allStats)
        {
            initialStats[stats] = new CharacterStatsSnapshot(stats);
        }

        moveButton.onClick.AddListener(OnMoveButtonClicked);
        attackButton.onClick.AddListener(OnAttackButtonClicked);
        passTurnButton.onClick.AddListener(OnPassTurnButtonClicked);
        restartButton.onClick.AddListener(OnRestartButtonClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClicked);
        HidePlayerControls();
        HideGameOverScreen();

        if (PlayerPrefs.GetInt("HasPlayedIntro", 0) == 0)
        {
            StartCoroutine(PlayIntroSequence());
        }
        else
        {
            StartSceneMusic();
        }
    }

    private void StartSceneMusic()
    {
        /* 
        Inicia la música de la escena.
        */
        sceneMusic.Play();
    }

    private IEnumerator PlayIntroSequence()
    {
        /* 
        Reproduce la música de introducción y muestra las imágenes 
        secuencialmente al hacer clic.
        */
        isIntroPlaying = true;
        displayImage.gameObject.SetActive(true);
        introMusic.Play();
        displayImage.sprite = introImages[currentImageIndex];

        while (isIntroPlaying)
        {
            yield return null;
        }
        
        introMusic.Stop();
        sceneMusic.Play();
        PlayerPrefs.SetInt("HasPlayedIntro", 1);
    }

    void Update()
    {
        /* 
        Controla los clics del ratón para avanzar las imágenes de 
        introducción.
        */
        if (isIntroPlaying && Input.GetMouseButtonDown(0))
        {
            currentImageIndex++;
            if (currentImageIndex < introImages.Count)
            {
                displayImage.sprite = introImages[currentImageIndex];
            }
            else
            {
                EndIntroSequence();
            }
        }
    }

    private void EndIntroSequence()
    {
        /* 
        Termina la secuencia de introducción y oculta la imagen de 
        introducción.
        */
        isIntroPlaying = false;
        displayImage.gameObject.SetActive(false);
    }

    public void ShowGameOverScreen()
    {
        /* 
        Muestra la pantalla de fin de juego activando la imagen de fin 
        de juego y los botones de reinicio y menú principal.
        */
        gameOverImage.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
        mainMenuButton.gameObject.SetActive(true);
    }

    public void HideGameOverScreen()
    {
        /* 
        Oculta la pantalla de fin de juego desactivando la imagen de fin 
        de juego y los botones de reinicio y menú principal.
        */
        gameOverImage.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        mainMenuButton.gameObject.SetActive(false);
    }

    public void OnRestartButtonClicked()
    {
        /* 
        Restaura las estadísticas iniciales de los personajes, resetea 
        el TurnManager y recarga la escena actual.
        */
        foreach (var entry in initialStats)
        {
            entry.Value.Restore(entry.Key);
        }
        TurnManager.ResetTurnManager();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void OnMainMenuButtonClicked()
    {
        /* 
        Carga la escena del menú principal.
        */
        SceneManager.LoadScene("MainMenu");
    }

    public void SetCurrentPlayerMove(PlayerMove playerMove)
    {
        /* 
        Establece el movimiento actual del jugador.
        */
        currentPlayerMove = playerMove;
    }

    public PlayerMove GetCurrentPlayerMove()
    {
        /* 
        Devuelve el movimiento actual del jugador.
        */
        return currentPlayerMove;
    }

    public void OnMoveButtonClicked()
    {
        /* 
        Maneja el evento de clic del botón de movimiento. Establece la 
        acción de movimiento para el jugador actual.
        */
        if (currentPlayerMove != null)
        {
            currentPlayerMove.SetActionMove();
        }
    }

    public void TogglePlayerControls(bool enable)
    {
        /* 
        Activa o desactiva los controles del jugador.
        */
        moveButton.interactable = enable;
        attackButton.interactable = enable;
        passTurnButton.interactable = enable;
    }

    public void OnAttackButtonClicked()
    {
        /* 
        Maneja el evento de clic del botón de ataque. Establece la 
        acción de ataque para el jugador actual.
        */
        if (currentPlayerMove != null)
        {
            currentPlayerMove.SetActionAttack();
        }
    }

    public void OnPassTurnButtonClicked()
    {
        /* 
        Maneja el evento de clic del botón de pasar turno. Finaliza el 
        turno del jugador actual.
        */
        if (currentPlayerMove != null)
        {
            currentPlayerMove.EndTurn();
        }
    }

    public void UpdateCharacterHealth(int characterIndex, int health)
    {
        /* 
        Actualiza el texto de salud del personaje en la interfaz.
        */
        if (characterIndex >= 0 && characterIndex < characterHealthTexts.Length)
        {
            characterHealthTexts[characterIndex].text = health.ToString();
        }
    }

    public void ShowPlayerControls()
    {
        /* 
        Muestra los controles del jugador.
        */
        moveButton.gameObject.SetActive(true);
        attackButton.gameObject.SetActive(true);
        passTurnButton.gameObject.SetActive(true);
    }

    public void HidePlayerControls()
    {
        /* 
        Oculta los controles del jugador.
        */
        moveButton.gameObject.SetActive(false);
        attackButton.gameObject.SetActive(false);
        passTurnButton.gameObject.SetActive(false);
    }

    public void UpdateTurnFrame(string characterName)
    {
        /* 
        Actualiza el indicador de turno del personaje especificado.
        */
        int characterIndex;
        if (characterTurnIndexMap.TryGetValue(characterName, out characterIndex))
        {
            for (int i = 0; i < turnIndicators.Length; i++)
            {
                turnIndicators[i].gameObject.SetActive(i == characterIndex);
            }
        }
    }

    public void DeactivateTurnFrame(string characterName)
    {
        /* 
        Desactiva el indicador de turno del personaje especificado.
        */
        int characterIndex;
        if (characterTurnIndexMap.TryGetValue(characterName, out characterIndex))
        {
            turnIndicators[characterIndex].gameObject.SetActive(false);
        }
    }

    public void RegisterCharacterTurnIndicator(string characterName, int index)
    {
        /* 
        Registra el indicador de turno del personaje.
        */
        if (!characterTurnIndexMap.ContainsKey(characterName))
        {
            characterTurnIndexMap.Add(characterName, index);
        }
    }

    public void RemoveTurnIndicator(string characterName)
    {
        /* 
        Elimina el indicador de turno del personaje.
        */
        if (characterTurnIndexMap.ContainsKey(characterName))
        {
            int index = characterTurnIndexMap[characterName];
            turnIndicators[index].gameObject.SetActive(false);
            characterTurnIndexMap.Remove(characterName);
        }
    }

    public void UpdateRoundText(int round)
    {
        /* 
        Actualiza el texto de la ronda en la interfaz.
        */
        roundText.text = "" + round;
    }
}
