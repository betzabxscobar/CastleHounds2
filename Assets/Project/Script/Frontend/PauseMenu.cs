using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool CambiandoDeEscena { get; private set; }

    [Header("Paneles")]
    public GameObject panelPausa;
    public GameObject panelMenu;
    public GameObject panelOpciones;
    public GameObject panelControles;

    [Header("Botones")]
    public Button btnPausa;

    private bool juegoPausado = false;
    private PlayerControlLock controlLock;
    private bool cursorAnteriorVisible;
    private CursorLockMode cursorAnteriorModo;

    void Start()
    {
        CambiandoDeEscena = false;

        controlLock = GetComponent<PlayerControlLock>();

        ResolvePanelReferences();
        WireButtons();

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

    private void ResolvePanelReferences()
    {
        Transform t = transform;

        if (panelPausa == null)
            panelPausa = FindChild(t, "PanelPausa");
        if (panelMenu == null)
            panelMenu = FindChild(t, "PanelMenu");
        if (panelOpciones == null)
            panelOpciones = FindChild(t, "PanelOpciones");
        if (panelControles == null)
            panelControles = FindChild(t, "PanelControles");

        if (btnPausa == null)
        {
            GameObject btn = FindChild(t, "Btn_Pausa");
            if (btn != null)
                btnPausa = btn.GetComponent<Button>();
        }
    }

    private void WireButtons()
    {
        WireButton("Btn_Continuar", ContinuarJuego);
        WireButton("Btn_Opciones", AbrirOpciones);
        WireButton("Btn_Controles", AbrirControles);
        WireButton("Btn_MenuPrincipal", IrMenuPrincipal);
        WireButton("BtnMusica", CambiarMusica);
        WireButton("BtnSonidos", CambiarSonidos);
        WireButton("BtnPantallaCompleta", CambiarPantallaCompleta);
        WireButton("Btn_Volver_Op", VolverMenu);
        WireButton("Btn_Volver_Cont", VolverMenu);

        if (btnPausa != null)
        {
            btnPausa.onClick.RemoveAllListeners();
            btnPausa.onClick.AddListener(PausarJuego);
        }
    }

    private void WireButton(string name, UnityEngine.Events.UnityAction action)
    {
        GameObject btnObj = FindChild(transform, name);
        if (btnObj == null)
            return;

        Button btn = btnObj.GetComponent<Button>();
        if (btn == null)
            return;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(action);
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

    private static GameObject FindChild(Transform parent, string name)
    {
        Transform result = parent.Find(name);
        if (result != null)
            return result.gameObject;

        foreach (Transform child in parent)
        {
            GameObject found = FindChild(child, name);
            if (found != null)
                return found;
        }

        return null;
    }
}
