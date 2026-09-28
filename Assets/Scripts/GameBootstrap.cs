using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameBootstrap : MonoBehaviour
{
    public static bool JuegoActivo { get; private set; }

    private const int VIDAS_INICIALES = 3;
    private const float ALTURA_CAIDA = -8f;

    private static GameBootstrap instancia;

    private PlayerController jugador1;
    private PlayerController jugador2;
    private Vector3 spawnJugador1;
    private Vector3 spawnJugador2;
    private Quaternion rotJugador1;
    private Quaternion rotJugador2;
    private int vidasJugador1 = VIDAS_INICIALES;
    private int vidasJugador2 = VIDAS_INICIALES;
    private bool bloqueandoRespawn1;
    private bool bloqueandoRespawn2;

    private Canvas canvas;
    private GameObject panelMenu;
    private GameObject panelComoJugar;
    private GameObject panelGameOver;
    private GameObject hud;
    private Text vidas1Texto;
    private Text vidas2Texto;
    private Text gameOverTexto;
    private Font fuente;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CrearAutomaticamente()
    {
        if (FindFirstObjectByType<GameBootstrap>() != null) return;
        GameObject objeto = new GameObject("GameManager");
        instancia = objeto.AddComponent<GameBootstrap>();
    }

    private void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }

        instancia = this;
        fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private void Start()
    {
        PrepararJugadores();
        PrepararObstaculos();
        MejorarAmbiente();
        PrepararInterfaz();
        MostrarMenu();
    }

    private void Update()
    {
        if (!JuegoActivo) return;
        RevisarCaida(jugador1, 1);
        RevisarCaida(jugador2, 2);
    }

    private void PrepararJugadores()
    {
        PlayerController[] jugadores = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        jugador1 = jugadores.FirstOrDefault(j => j.NumeroJugador == 1);
        jugador2 = jugadores.FirstOrDefault(j => j.NumeroJugador == 2);

        if (jugador1 != null)
        {
            spawnJugador1 = jugador1.transform.position;
            rotJugador1 = jugador1.transform.rotation;
        }

        if (jugador2 != null)
        {
            spawnJugador2 = jugador2.transform.position;
            rotJugador2 = jugador2.transform.rotation;
        }
    }

    private void PrepararObstaculos()
    {
        Transform[] objetos = FindObjectsByType<Transform>(FindObjectsSortMode.None);
        int indiceGiro = 0;

        foreach (Transform objeto in objetos.Where(t => t.name.StartsWith("ObscatuloGiratorio")))
        {
            RotatingObstacle giro = objeto.GetComponent<RotatingObstacle>();
            if (giro == null) giro = objeto.gameObject.AddComponent<RotatingObstacle>();
            float sentido = indiceGiro % 2 == 0 ? 1f : -1f;
            giro.Configurar(sentido * (95f + (indiceGiro % 3) * 15f));
            indiceGiro++;
        }

        List<Transform> plataformas = objetos
            .Where(t => t.name.StartsWith("Cube") && t.position.z >= 195f && t.position.z <= 248f && Mathf.Abs(t.position.y) < 1.5f)
            .OrderBy(t => t.position.z)
            .ThenBy(t => t.position.x)
            .ToList();

        for (int i = 0; i < plataformas.Count; i++)
        {
            MovingPlatform movimiento = plataformas[i].GetComponent<MovingPlatform>();
            if (movimiento == null) movimiento = plataformas[i].gameObject.AddComponent<MovingPlatform>();
            movimiento.Configurar(2.2f + (i % 3) * 0.35f, 0.9f + (i % 4) * 0.12f, i * 0.45f);
        }
    }

    private void MejorarAmbiente()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.12f, 0.16f, 0.24f);
        RenderSettings.fogStartDistance = 160f;
        RenderSettings.fogEndDistance = 520f;
        RenderSettings.ambientIntensity = 1.15f;

        Light luz = FindFirstObjectByType<Light>();
        if (luz != null)
        {
            luz.intensity = 1.25f;
            luz.shadows = LightShadows.Soft;
        }

        foreach (Camera camara in FindObjectsByType<Camera>(FindObjectsSortMode.None))
            camara.backgroundColor = new Color(0.08f, 0.1f, 0.18f);

        CrearArcoMeta();
    }

    private void CrearArcoMeta()
    {
        if (GameObject.Find("META_DECORATIVA") != null) return;
        GameObject raiz = new GameObject("META_DECORATIVA");
        Material rosa = CrearMaterial(new Color(1f, 0.12f, 0.55f));
        Material cyan = CrearMaterial(new Color(0.05f, 0.85f, 1f));
        CrearBloque(raiz.transform, "Meta_Izquierda", new Vector3(-30f, 18f, 363f), new Vector3(2f, 12f, 2f), rosa);
        CrearBloque(raiz.transform, "Meta_Derecha", new Vector3(30f, 18f, 363f), new Vector3(2f, 12f, 2f), cyan);
        CrearBloque(raiz.transform, "Meta_Arriba", new Vector3(0f, 29f, 363f), new Vector3(32f, 1.5f, 2f), rosa);
    }

    private Material CrearMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = color;
        return material;
    }

    private void CrearBloque(Transform padre, string nombre, Vector3 posicion, Vector3 escala, Material material)
    {
        GameObject bloque = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bloque.name = nombre;
        bloque.transform.SetParent(padre);
        bloque.transform.position = posicion;
        bloque.transform.localScale = escala;
        Renderer renderer = bloque.GetComponent<Renderer>();
        if (renderer != null) renderer.material = material;
    }

    private void PrepararInterfaz()
    {
        Canvas existente = GameObject.Find("Canvas_Juego")?.GetComponent<Canvas>();
        if (existente != null)
        {
            canvas = existente;
            panelMenu = BuscarHijo(canvas.transform, "MenuPrincipal")?.gameObject;
            panelComoJugar = BuscarHijo(canvas.transform, "ComoJugar")?.gameObject;
            panelGameOver = BuscarHijo(canvas.transform, "GameOver")?.gameObject;
            hud = BuscarHijo(canvas.transform, "HUD")?.gameObject;
            vidas1Texto = BuscarHijo(canvas.transform, "Vidas_J1")?.GetComponent<Text>();
            vidas2Texto = BuscarHijo(canvas.transform, "Vidas_J2")?.GetComponent<Text>();
            gameOverTexto = BuscarHijo(canvas.transform, "Resultado")?.GetComponent<Text>();

            ConectarBoton("Boton_JUGAR", IniciarJuego);
            ConectarBoton("Boton_COMO SE JUEGA", MostrarComoJugar);
            ConectarBoton("Boton_SALIR", Salir);
            ConectarBoton("Boton_VOLVER", MostrarMenu);
            ConectarBoton("Boton_REINICIAR", Reiniciar);
            ConectarBoton("Boton_MENU", MostrarMenu);
            ActualizarHUD();
            return;
        }

        CrearInterfazFallback();
    }

    private Transform BuscarHijo(Transform raiz, string nombre)
    {
        foreach (Transform hijo in raiz.GetComponentsInChildren<Transform>(true))
            if (hijo.name == nombre) return hijo;
        return null;
    }

    private void ConectarBoton(string nombre, UnityEngine.Events.UnityAction accion)
    {
        Transform t = BuscarHijo(canvas.transform, nombre);
        if (t == null) return;
        Button boton = t.GetComponent<Button>();
        if (boton == null) return;
        boton.onClick.RemoveAllListeners();
        boton.onClick.AddListener(accion);
    }

    private void CrearInterfazFallback()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject sistemaEventos = new GameObject("EventSystem", typeof(EventSystem));
            sistemaEventos.AddComponent<InputSystemUIInputModule>();
        }

        GameObject objetoCanvas = new GameObject("Canvas_Juego", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = objetoCanvas.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = objetoCanvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        CrearHUD();
        CrearMenuPrincipal();
        CrearPanelComoJugar();
        CrearPanelGameOver();
    }

    private void CrearHUD()
    {
        hud = CrearPanel("HUD", canvas.transform, new Color(0, 0, 0, 0));
        ConfigurarRect(hud.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        vidas1Texto = CrearTexto(hud.transform, "Vidas_J1", "JUGADOR 1   ♥ ♥ ♥", 34, TextAnchor.MiddleLeft);
        ConfigurarRect(vidas1Texto.rectTransform, new Vector2(0.02f, 0.91f), new Vector2(0.45f, 0.99f), Vector2.zero, Vector2.zero);
        vidas2Texto = CrearTexto(hud.transform, "Vidas_J2", "JUGADOR 2   ♥ ♥ ♥", 34, TextAnchor.MiddleRight);
        ConfigurarRect(vidas2Texto.rectTransform, new Vector2(0.55f, 0.91f), new Vector2(0.98f, 0.99f), Vector2.zero, Vector2.zero);
        ActualizarHUD();
    }

    private void CrearMenuPrincipal()
    {
        panelMenu = CrearPanel("MenuPrincipal", canvas.transform, new Color(0.025f, 0.035f, 0.07f, 0.94f));
        ConfigurarRect(panelMenu.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Text titulo = CrearTexto(panelMenu.transform, "Titulo", "PRACTICA MOVIMIENTO", 72, TextAnchor.MiddleCenter);
        titulo.fontStyle = FontStyle.Bold;
        ConfigurarRect(titulo.rectTransform, new Vector2(0.15f, 0.70f), new Vector2(0.85f, 0.90f), Vector2.zero, Vector2.zero);
        Text subtitulo = CrearTexto(panelMenu.transform, "Subtitulo", "CARRERA DE OBSTACULOS - 2 JUGADORES", 28, TextAnchor.MiddleCenter);
        ConfigurarRect(subtitulo.rectTransform, new Vector2(0.15f, 0.62f), new Vector2(0.85f, 0.70f), Vector2.zero, Vector2.zero);
        CrearBoton(panelMenu.transform, "JUGAR", new Vector2(0.36f, 0.47f), new Vector2(0.64f, 0.56f), IniciarJuego);
        CrearBoton(panelMenu.transform, "COMO SE JUEGA", new Vector2(0.36f, 0.35f), new Vector2(0.64f, 0.44f), MostrarComoJugar);
        CrearBoton(panelMenu.transform, "SALIR", new Vector2(0.36f, 0.23f), new Vector2(0.64f, 0.32f), Salir);
    }

    private void CrearPanelComoJugar()
    {
        panelComoJugar = CrearPanel("ComoJugar", canvas.transform, new Color(0.025f, 0.035f, 0.07f, 0.97f));
        ConfigurarRect(panelComoJugar.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        Text titulo = CrearTexto(panelComoJugar.transform, "Titulo", "COMO SE JUEGA", 62, TextAnchor.MiddleCenter);
        titulo.fontStyle = FontStyle.Bold;
        ConfigurarRect(titulo.rectTransform, new Vector2(0.15f, 0.76f), new Vector2(0.85f, 0.9f), Vector2.zero, Vector2.zero);
        string instrucciones = "JUGADOR 1\nW A S D = Moverse     ESPACIO = Saltar\n\n" +
            "JUGADOR 2\nFLECHAS = Moverse     ENTER = Saltar\n\n" +
            "Cada jugador comienza con 3 vidas.\nSi caes de la pista pierdes una vida y reapareces.\n" +
            "Evita los palos giratorios y usa las plataformas moviles para avanzar.";
        Text info = CrearTexto(panelComoJugar.transform, "Instrucciones", instrucciones, 30, TextAnchor.MiddleCenter);
        info.horizontalOverflow = HorizontalWrapMode.Wrap;
        info.verticalOverflow = VerticalWrapMode.Overflow;
        ConfigurarRect(info.rectTransform, new Vector2(0.16f, 0.27f), new Vector2(0.84f, 0.73f), Vector2.zero, Vector2.zero);
        CrearBoton(panelComoJugar.transform, "VOLVER", new Vector2(0.40f, 0.11f), new Vector2(0.60f, 0.20f), MostrarMenu);
        panelComoJugar.SetActive(false);
    }

    private void CrearPanelGameOver()
    {
        panelGameOver = CrearPanel("GameOver", canvas.transform, new Color(0.02f, 0.02f, 0.04f, 0.92f));
        ConfigurarRect(panelGameOver.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        gameOverTexto = CrearTexto(panelGameOver.transform, "Resultado", "FIN DE LA PARTIDA", 60, TextAnchor.MiddleCenter);
        gameOverTexto.fontStyle = FontStyle.Bold;
        ConfigurarRect(gameOverTexto.rectTransform, new Vector2(0.15f, 0.55f), new Vector2(0.85f, 0.75f), Vector2.zero, Vector2.zero);
        CrearBoton(panelGameOver.transform, "REINICIAR", new Vector2(0.36f, 0.39f), new Vector2(0.64f, 0.48f), Reiniciar);
        CrearBoton(panelGameOver.transform, "MENU", new Vector2(0.36f, 0.27f), new Vector2(0.64f, 0.36f), MostrarMenu);
        panelGameOver.SetActive(false);
    }

    private GameObject CrearPanel(string nombre, Transform padre, Color color)
    {
        GameObject panel = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(padre, false);
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private Text CrearTexto(Transform padre, string nombre, string contenido, int tamano, TextAnchor alineacion)
    {
        GameObject objeto = new GameObject(nombre, typeof(RectTransform), typeof(Text));
        objeto.transform.SetParent(padre, false);
        Text texto = objeto.GetComponent<Text>();
        texto.font = fuente;
        texto.text = contenido;
        texto.fontSize = tamano;
        texto.alignment = alineacion;
        texto.color = Color.white;
        texto.resizeTextForBestFit = true;
        texto.resizeTextMinSize = Mathf.Max(14, tamano / 2);
        texto.resizeTextMaxSize = tamano;
        return texto;
    }

    private Button CrearBoton(Transform padre, string texto, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction accion)
    {
        GameObject objeto = new GameObject("Boton_" + texto, typeof(RectTransform), typeof(Image), typeof(Button));
        objeto.transform.SetParent(padre, false);
        ConfigurarRect(objeto.GetComponent<RectTransform>(), anchorMin, anchorMax, Vector2.zero, Vector2.zero);
        Image imagen = objeto.GetComponent<Image>();
        imagen.color = new Color(0.9f, 0.06f, 0.25f, 0.96f);
        Button boton = objeto.GetComponent<Button>();
        ColorBlock colores = boton.colors;
        colores.normalColor = new Color(0.9f, 0.06f, 0.25f, 1f);
        colores.highlightedColor = new Color(0.05f, 0.75f, 0.95f, 1f);
        colores.pressedColor = new Color(0.7f, 0.03f, 0.16f, 1f);
        boton.colors = colores;
        boton.onClick.AddListener(accion);
        Text etiqueta = CrearTexto(objeto.transform, "Texto", texto, 32, TextAnchor.MiddleCenter);
        etiqueta.fontStyle = FontStyle.Bold;
        ConfigurarRect(etiqueta.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return boton;
    }

    private void ConfigurarRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = offsetMin;
        rt.offsetMax = offsetMax;
    }

    public void MostrarMenu()
    {
        JuegoActivo = false;
        Time.timeScale = 0f;
        if (panelMenu != null) panelMenu.SetActive(true);
        if (panelComoJugar != null) panelComoJugar.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (hud != null) hud.SetActive(false);
    }

    public void MostrarComoJugar()
    {
        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelComoJugar != null) panelComoJugar.SetActive(true);
    }

    public void IniciarJuego()
    {
        vidasJugador1 = VIDAS_INICIALES;
        vidasJugador2 = VIDAS_INICIALES;
        bloqueandoRespawn1 = false;
        bloqueandoRespawn2 = false;
        Reaparecer(jugador1, spawnJugador1, rotJugador1);
        Reaparecer(jugador2, spawnJugador2, rotJugador2);
        ActualizarHUD();
        if (panelMenu != null) panelMenu.SetActive(false);
        if (panelComoJugar != null) panelComoJugar.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (hud != null) hud.SetActive(true);
        Time.timeScale = 1f;
        JuegoActivo = true;
    }

    private void RevisarCaida(PlayerController jugador, int numero)
    {
        if (jugador == null || jugador.transform.position.y >= ALTURA_CAIDA) return;
        if (numero == 1)
        {
            if (bloqueandoRespawn1) return;
            bloqueandoRespawn1 = true;
            vidasJugador1--;
            ActualizarHUD();
            if (vidasJugador1 <= 0) TerminarPartida("JUGADOR 1 SE QUEDO SIN VIDAS");
            else { Reaparecer(jugador1, spawnJugador1, rotJugador1); bloqueandoRespawn1 = false; }
        }
        else
        {
            if (bloqueandoRespawn2) return;
            bloqueandoRespawn2 = true;
            vidasJugador2--;
            ActualizarHUD();
            if (vidasJugador2 <= 0) TerminarPartida("JUGADOR 2 SE QUEDO SIN VIDAS");
            else { Reaparecer(jugador2, spawnJugador2, rotJugador2); bloqueandoRespawn2 = false; }
        }
    }

    private void Reaparecer(PlayerController jugador, Vector3 posicion, Quaternion rotacion)
    {
        if (jugador == null) return;
        Rigidbody cuerpo = jugador.GetComponent<Rigidbody>();
        if (cuerpo != null)
        {
            cuerpo.linearVelocity = Vector3.zero;
            cuerpo.angularVelocity = Vector3.zero;
            cuerpo.position = posicion;
            cuerpo.rotation = rotacion;
        }
        else jugador.transform.SetPositionAndRotation(posicion, rotacion);
    }

    private void ActualizarHUD()
    {
        if (vidas1Texto != null) vidas1Texto.text = "JUGADOR 1   " + Corazones(vidasJugador1);
        if (vidas2Texto != null) vidas2Texto.text = "JUGADOR 2   " + Corazones(vidasJugador2);
    }

    private string Corazones(int cantidad) => string.Join(" ", Enumerable.Repeat("♥", Mathf.Max(0, cantidad)));

    private void TerminarPartida(string mensaje)
    {
        JuegoActivo = false;
        Time.timeScale = 0f;
        if (hud != null) hud.SetActive(false);
        if (gameOverTexto != null) gameOverTexto.text = mensaje;
        if (panelGameOver != null) panelGameOver.SetActive(true);
    }

    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Salir()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
