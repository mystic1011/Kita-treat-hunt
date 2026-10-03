using System.Collections.Generic;
using UnityEngine;

public class KitaGame : MonoBehaviour
{
    private enum GameState { Menu, Playing, Won, Lost }
    private GameState state;
    private readonly PlayerStats stats = new PlayerStats();
    private readonly List<Collectible> treats = new List<Collectible>();
    private readonly HashSet<KeyCode> held = new HashSet<KeyCode>();
    private Vector2 player;
    private float remaining, elapsed, invulnerable;
    private string lossReason;
    private GUIStyle title, body, small, centered;
    private const int Target = 12;
    private static readonly Rect Arena = new Rect(70, 160, 860, 410);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Launch()
    {
        // A fresh empty Unity scene needs no manually assigned references.
        new GameObject("Kita Treat Hunt").AddComponent<KitaGame>();
    }

    private void Awake() { state = GameState.Menu; }
    // ABSTRACTION: a single operation builds and resets a complete round.
    private void StartRound()
    {
        foreach (Collectible item in treats) if (item != null) Destroy(item.gameObject);
        treats.Clear(); held.Clear(); stats.Reset();
        player = new Vector2(120, 365);
        remaining = 60f; elapsed = 0f; invulnerable = 0f;
        Vector2[] positions = {
            new Vector2(220,220), new Vector2(370,220), new Vector2(550,220),
            new Vector2(760,220), new Vector2(860,310), new Vector2(700,380),
            new Vector2(480,365), new Vector2(280,365), new Vector2(220,510),
            new Vector2(430,510), new Vector2(630,510), new Vector2(830,510) };
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject obj = new GameObject(i % 5 == 0 ? "Golden treat" : "Treat");
            obj.transform.SetParent(transform);
            Collectible item;
            if (i % 5 == 0) item = obj.AddComponent<GoldenTreat>();
            else item = obj.AddComponent<Treat>();
            item.Initialize(positions[i]); treats.Add(item);
        }
        state = GameState.Playing;
    }

    private Vector2 HazardPosition(int index)
    {
        return new Vector2(370 + index * 220, 365 + Mathf.Sin(elapsed * 1.5f + index * 2f) * 145);
    }
    private bool Down(KeyCode a, KeyCode b) { return held.Contains(a) || held.Contains(b); }
    private void Update()
    {
        if (state != GameState.Playing) return;
        float dt = Time.deltaTime;
        elapsed += dt; remaining = Mathf.Max(0, remaining - dt);
        invulnerable = Mathf.Max(0, invulnerable - dt);
        Vector2 direction = new Vector2(
            (Down(KeyCode.D, KeyCode.RightArrow) ? 1 : 0) - (Down(KeyCode.A, KeyCode.LeftArrow) ? 1 : 0),
            (Down(KeyCode.S, KeyCode.DownArrow) ? 1 : 0) - (Down(KeyCode.W, KeyCode.UpArrow) ? 1 : 0));
        player += direction.normalized * 230f * dt;
        player.x = Mathf.Clamp(player.x, Arena.xMin + 22, Arena.xMax - 22);
        player.y = Mathf.Clamp(player.y, Arena.yMin + 24, Arena.yMax - 22);
        foreach (Collectible item in treats)
            if (!item.Collected && Vector2.Distance(player, item.Position) < 30) item.Collect(stats);
        for (int i = 0; i < 3; i++)
            if (invulnerable <= 0 && Vector2.Distance(player, HazardPosition(i)) < 35)
            { stats.TakeDamage(); invulnerable = 1.5f; }
        if (stats.Health == 0) { lossReason = "The vacuum bots caught Kita!"; state = GameState.Lost; }
        else if (stats.Score >= Target) state = GameState.Won;
        else if (remaining <= 0) { lossReason = "Time ran out!"; state = GameState.Lost; }
    }
    private void OnApplicationFocus(bool focused) { if (!focused) held.Clear(); }
    private void OnApplicationPause(bool paused) { if (paused) held.Clear(); }

    private void Fill(Rect rect, Color color)
    {
        GUI.color = color; GUI.DrawTexture(rect, Texture2D.whiteTexture); GUI.color = Color.white;
    }
    private void Label(Rect rect, string text, GUIStyle style) { GUI.Label(rect, text, style); }
    private void InitStyles()
    {
        if (title != null) return;
        title = new GUIStyle(GUI.skin.label) { fontSize = 36, fontStyle = FontStyle.Bold };
        title.normal.textColor = new Color(0.23f, 0.25f, 0.21f);
        body = new GUIStyle(title) { fontSize = 22, fontStyle = FontStyle.Normal, wordWrap = true };
        small = new GUIStyle(body) { fontSize = 16 };
        centered = new GUIStyle(body) { alignment = TextAnchor.MiddleCenter };
    }
    private void DrawCat(Vector2 pos)
    {
        Color fur = new Color(0.57f, 0.37f, 0.23f);
        Fill(new Rect(pos.x-19,pos.y-17,38,34),fur);
        Fill(new Rect(pos.x-19,pos.y-26,12,12),fur);
        Fill(new Rect(pos.x+7,pos.y-26,12,12),fur);
        Fill(new Rect(pos.x-13,pos.y+8,26,9),Color.white);
        Fill(new Rect(pos.x-12,pos.y-5,5,5),new Color(0.2f,0.24f,0.18f));
        Fill(new Rect(pos.x+7,pos.y-5,5,5),new Color(0.2f,0.24f,0.18f));
        Fill(new Rect(pos.x-3,pos.y+2,6,4),new Color(0.94f,0.61f,0.62f));
        Fill(new Rect(pos.x-3,pos.y-17,6,6),new Color(0.35f,0.23f,0.15f));
    }
    private void OnGUI()
    {
        InitStyles();
        Event e = Event.current;
        // IMGUI keyboard events avoid requiring either Unity input package.
        if (e.type == EventType.KeyDown) held.Add(e.keyCode);
        else if (e.type == EventType.KeyUp) held.Remove(e.keyCode);
        Matrix4x4 previous = GUI.matrix;
        float scale = Mathf.Min(Screen.width / 1000f, Screen.height / 650f);
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width-1000*scale)/2,(Screen.height-650*scale)/2,0),Quaternion.identity,new Vector3(scale,scale,1));
        Fill(new Rect(0,0,1000,650),new Color(0.97f,0.95f,0.90f));
        Label(new Rect(70,35,860,55),"KITA'S TREAT HUNT",title);
        Label(new Rect(70,94,860,40),"A tiny adventure for one very curious tabby.",body);
        Fill(Arena,new Color(0.82f,0.87f,0.76f));
        for (int x=90;x<930;x+=70) for(int y=180;y<570;y+=70)
            Fill(new Rect(x,y,3,3),new Color(0.69f,0.76f,0.62f));
        if (state != GameState.Menu)
        {
            foreach (Collectible item in treats) if (!item.Collected)
            {
                Vector2 pos=item.Position;
                Fill(new Rect(pos.x-11,pos.y-11,22,22),item.Tint);
                Fill(new Rect(pos.x-4,pos.y-4,8,8),Color.white);
            }
            for(int i=0;i<3;i++)
            {
                Vector2 pos=HazardPosition(i);
                Fill(new Rect(pos.x-19,pos.y-17,38,34),new Color(0.48f,0.52f,0.62f));
                Fill(new Rect(pos.x-12,pos.y-5,24,5),new Color(0.24f,0.27f,0.34f));
            }
            if(invulnerable<=0 || Mathf.FloorToInt(invulnerable*10)%2==0) DrawCat(player);
            Label(new Rect(70,590,860,40),"Treat points: "+stats.Score+" / "+Target+"     Lives: "+stats.Health+"     Time: "+Mathf.CeilToInt(remaining)+"s",body);
        }
        if(state == GameState.Menu)
        {
            DrawCat(new Vector2(500,235));
            Label(new Rect(180,285,640,50),"Collect 12 treat points in 60 seconds.",centered);
            Label(new Rect(180,335,640,60),"WASD or arrow keys to move. Pink = 1; gold = 3.\nAvoid the moving vacuum bots. You have 3 lives.",centered);
            if(GUI.Button(new Rect(380,435,240,55),"Start Kita's adventure")) StartRound();
            Label(new Rect(180,590,640,30),"Click the Game view to give it keyboard focus.",centered);
        }
        else if(state != GameState.Playing)
        {
            Fill(new Rect(230,260,540,230),new Color(0.98f,0.97f,0.94f));
            Label(new Rect(250,275,500,55),state==GameState.Won ? "Kita found her treats!" : lossReason,centered);
            Label(new Rect(250,330,500,45),"Final score: "+stats.Score,centered);
            if(GUI.Button(new Rect(330,400,160,50),"Play again")) StartRound();
            if(GUI.Button(new Rect(510,400,160,50),"Main menu")) { held.Clear(); state=GameState.Menu; }
        }
        GUI.matrix = previous;
    }
}
