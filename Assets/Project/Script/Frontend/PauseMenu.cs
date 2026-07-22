using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool CambiandoDeEscena { get; private set; }

    [Header("Paneles")]
    public GameObject panelPausa;
    public GameObject panelMenu;
    public GameObject panelOpciones;
    public GameObject panelControles;

    [Header("Botones")]
    public UnityEngine.UI.Button btnPausa;

    private bool juegoPausado = false;
    private PlayerControlLock controlLock;
    private bool cursorAnteriorVisible;
    private CursorLockMode cursorAnteriorModo;

    void Start()
    {
        CambiandoDeEscena = false;

        controlLock = GetComponent<PlayerControlLock>();

        if (btnPausa != null)
            btnPausa.onClick.AddListener(PausarJuego);

        SetPanelActive(panelPausa, false);
        SetPanelActive(panelMenu, true);
        SetPanelActive(panelOpciones, false);
        SetPanelActive(panelControles, false);

        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (juegoPausado)
                ContinuarJuego();
            else
                PausarJuego();
        }
    }

    public void PausarJuego()
    {
        CursorAntesDePausa();
        BloquearControlJugador(true);

        SetPanelActive(panelPausa, true);
        SetPanelActive(panelMenu, true);
        SetPanelActive(panelOpciones, false);
        SetPanelActive(panelControles, false);

        MostrarBtnPausa(false);

        Time.timeScale = 0f;
        juegoPausado = true;

        ReproducirClick();
    }

    public void ContinuarJuego()
    {
        SetPanelActive(panelPausa, false);
        SetPanelActive(panelMenu, false);
        SetPanelActive(panelOpciones, false);
        SetPanelActive(panelControles, false);

        MostrarBtnPausa(true);

        Time.timeScale = 1f;
        juegoPausado = false;

        BloquearControlJugador(false);
        CursorDespuesDePausa();

        ReproducirClick();
    }

    public void AbrirOpciones()
    {
        SetPanelActive(panelMenu, false);
        SetPanelActive(panelOpciones, true);

        ReproducirClick();
    }

    public void AbrirControles()
    {
        SetPanelActive(panelMenu, false);
        SetPanelActive(panelControles, true);

        ReproducirClick();
    }

    public void VolverMenu()
    {
        SetPanelActive(panelMenu, true);
        SetPanelActive(panelOpciones, false);
        SetPanelActive(panelControles, false);

        ReproducirClick();
    }

    public void IrMenuPrincipal()
    {
        if (CambiandoDeEscena)
            return;

        CambiandoDeEscena = true;
        Time.timeScale = 1f;

        ReproducirClick();

        SceneManager.LoadScene("MenuPrincipal");
    }

    public void CambiarMusica()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMusica();
        }

        ReproducirClick();
    }

    public void CambiarSonidos()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleSonidos();
        }

        ReproducirClick();
    }

    public void CambiarPantallaCompleta()
    {
        Screen.fullScreen = !Screen.fullScreen;

        ReproducirClick();
    }

    private void CursorAntesDePausa()
    {
        cursorAnteriorVisible = Cursor.visible;
        cursorAnteriorModo = Cursor.lockState;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void CursorDespuesDePausa()
    {
        Cursor.visible = cursorAnteriorVisible;
        Cursor.lockState = cursorAnteriorModo;
    }

    private void BloquearControlJugador(bool bloquear)
    {
        if (controlLock == null)
            return;

        if (bloquear)
            controlLock.LockControl("pausa");
        else
            controlLock.UnlockControl("pausa");
    }

    private void ReproducirClick()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ReproducirClick();
        }
    }

    private void MostrarBtnPausa(bool visible)
    {
        if (btnPausa != null)
            btnPausa.gameObject.SetActive(visible);
    }

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}
