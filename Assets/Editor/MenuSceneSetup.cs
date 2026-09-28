#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[InitializeOnLoad]
public static class MenuSceneSetup
{
    private const string SceneName = "SampleScene";
    private const string SpritePath = "Assets/Resources/MenuFondo.png";

    static MenuSceneSetup()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.delayCall += RevisarEscenaActiva;
    }

    private static void RevisarEscenaActiva()
    {
        Scene escena = SceneManager.GetActiveScene();
        if (escena.IsValid() && escena.name == SceneName)
            CrearMenuSiHaceFalta(escena);
    }

    private static void OnSceneOpened(Scene escena, OpenSceneMode modo)
    {
        if (escena.name == SceneName)
            EditorApplication.delayCall += () => CrearMenuSiHaceFalta(escena);
    }

    private static void CrearMenuSiHaceFalta(Scene escena)
    {
        if (Application.isPlaying || !escena.IsValid()) return;
        if (GameObject.Find("Canvas_Juego") != null) return;

        PrepararSprite();
        Sprite fondo = AssetDatabase.LoadAssetAtPath<Sprite>(SpritePath);
        Font fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        GameObject manager = GameObject.Find("GameManager");
        if (manager == null) manager = new GameObject("GameManager");
        if (manager.GetComponent<GameBootstrap>() == null) manager.AddComponent<GameBootstrap>();

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem));
            es.AddComponent<InputSystemUIInputModule>();
        }

        GameObject canvasObj = new GameObject("Canvas_Juego", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        CrearHUD(canvasObj.transform, fuente);
        CrearMenu(canvasObj.transform, fondo, fuente);
        CrearComoJugar(canvasObj.transform, fuente);
        CrearGameOver(canvasObj.transform, fuente);

        EditorSceneManager.MarkSceneDirty(escena);
        EditorSceneManager.SaveScene(escena);
        Selection.activeGameObject = canvasObj;
        Debug.Log("Canvas_Juego creado y guardado dentro de SampleScene.");
    }

    private static void PrepararSprite()
    {
        AssetDatabase.ImportAsset(SpritePath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(SpritePath) as TextureImporter;
        if (importer != null && importer.textureType != TextureImporterType.Sprite)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
    }

    private static void CrearHUD(Transform padre, Font fuente)
    {
        GameObject hud = Panel("HUD", padre, new Color(0, 0, 0, 0));
        Stretch(hud.GetComponent<RectTransform>());
        Text j1 = Texto(hud.transform, "Vidas_J1", "JUGADOR 1   ♥ ♥ ♥", 34, TextAnchor.MiddleLeft, fuente);
        Anchors(j1.rectTransform, new Vector2(.02f, .91f), new Vector2(.45f, .99f));
        Text j2 = Texto(hud.transform, "Vidas_J2", "JUGADOR 2   ♥ ♥ ♥", 34, TextAnchor.MiddleRight, fuente);
        Anchors(j2.rectTransform, new Vector2(.55f, .91f), new Vector2(.98f, .99f));
        hud.SetActive(false);
    }

    private static void CrearMenu(Transform padre, Sprite fondo, Font fuente)
    {
        GameObject menu = Panel("MenuPrincipal", padre, Color.white);
        Stretch(menu.GetComponent<RectTransform>());
        Image imagen = menu.GetComponent<Image>();
        imagen.sprite = fondo;
        imagen.preserveAspect = false;

        GameObject sombra = Panel("OscurecerFondo", menu.transform, new Color(0.02f, 0.03f, 0.06f, .42f));
        Stretch(sombra.GetComponent<RectTransform>());

        Text titulo = Texto(menu.transform, "Titulo", "PRACTICA MOVIMIENTO", 72, TextAnchor.MiddleCenter, fuente);
        titulo.fontStyle = FontStyle.Bold;
        Anchors(titulo.rectTransform, new Vector2(.15f, .70f), new Vector2(.85f, .90f));

        Text sub = Texto(menu.transform, "Subtitulo", "CARRERA DE OBSTACULOS - 2 JUGADORES", 28, TextAnchor.MiddleCenter, fuente);
        Anchors(sub.rectTransform, new Vector2(.15f, .62f), new Vector2(.85f, .70f));

        Boton(menu.transform, "JUGAR", new Vector2(.36f, .47f), new Vector2(.64f, .56f), fuente);
        Boton(menu.transform, "COMO SE JUEGA", new Vector2(.36f, .35f), new Vector2(.64f, .44f), fuente);
        Boton(menu.transform, "SALIR", new Vector2(.36f, .23f), new Vector2(.64f, .32f), fuente);
    }

    private static void CrearComoJugar(Transform padre, Font fuente)
    {
        GameObject panel = Panel("ComoJugar", padre, new Color(.025f, .035f, .07f, .97f));
        Stretch(panel.GetComponent<RectTransform>());
        Text titulo = Texto(panel.transform, "Titulo", "COMO SE JUEGA", 62, TextAnchor.MiddleCenter, fuente);
        titulo.fontStyle = FontStyle.Bold;
        Anchors(titulo.rectTransform, new Vector2(.15f, .76f), new Vector2(.85f, .90f));
        string info = "JUGADOR 1\nW A S D = Moverse     ESPACIO = Saltar\n\nJUGADOR 2\nFLECHAS = Moverse     ENTER = Saltar\n\nCada jugador comienza con 3 vidas.\nSi caes de la pista pierdes una vida y reapareces.\nEvita los palos giratorios y usa las plataformas moviles para avanzar.";
        Text instrucciones = Texto(panel.transform, "Instrucciones", info, 30, TextAnchor.MiddleCenter, fuente);
        Anchors(instrucciones.rectTransform, new Vector2(.16f, .27f), new Vector2(.84f, .73f));
        Boton(panel.transform, "VOLVER", new Vector2(.40f, .11f), new Vector2(.60f, .20f), fuente);
        panel.SetActive(false);
    }

    private static void CrearGameOver(Transform padre, Font fuente)
    {
        GameObject panel = Panel("GameOver", padre, new Color(.02f, .02f, .04f, .92f));
        Stretch(panel.GetComponent<RectTransform>());
        Text resultado = Texto(panel.transform, "Resultado", "FIN DE LA PARTIDA", 60, TextAnchor.MiddleCenter, fuente);
        resultado.fontStyle = FontStyle.Bold;
        Anchors(resultado.rectTransform, new Vector2(.15f, .55f), new Vector2(.85f, .75f));
        Boton(panel.transform, "REINICIAR", new Vector2(.36f, .39f), new Vector2(.64f, .48f), fuente);
        Boton(panel.transform, "MENU", new Vector2(.36f, .27f), new Vector2(.64f, .36f), fuente);
        panel.SetActive(false);
    }

    private static GameObject Panel(string nombre, Transform padre, Color color)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(padre, false);
        go.GetComponent<Image>().color = color;
        return go;
    }

    private static Text Texto(Transform padre, string nombre, string contenido, int tamano, TextAnchor alineacion, Font fuente)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(padre, false);
        Text t = go.GetComponent<Text>();
        t.font = fuente;
        t.text = contenido;
        t.fontSize = tamano;
        t.alignment = alineacion;
        t.color = Color.white;
        t.resizeTextForBestFit = true;
        t.resizeTextMinSize = Mathf.Max(14, tamano / 2);
        t.resizeTextMaxSize = tamano;
        return t;
    }

    private static void Boton(Transform padre, string texto, Vector2 min, Vector2 max, Font fuente)
    {
        GameObject go = new GameObject("Boton_" + texto, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(padre, false);
        Anchors(go.GetComponent<RectTransform>(), min, max);
        Image img = go.GetComponent<Image>();
        img.color = new Color(.9f, .06f, .25f, .96f);
        Button boton = go.GetComponent<Button>();
        ColorBlock c = boton.colors;
        c.normalColor = new Color(.9f, .06f, .25f, 1f);
        c.highlightedColor = new Color(.05f, .75f, .95f, 1f);
        c.pressedColor = new Color(.7f, .03f, .16f, 1f);
        boton.colors = c;
        Text label = Texto(go.transform, "Texto", texto, 32, TextAnchor.MiddleCenter, fuente);
        label.fontStyle = FontStyle.Bold;
        Stretch(label.rectTransform);
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static void Anchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
#endif
