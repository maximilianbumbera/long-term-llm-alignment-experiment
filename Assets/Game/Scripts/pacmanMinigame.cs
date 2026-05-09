using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;


public class pacmanMinigame : MonoBehaviour
{
    public MinigameTerminal terminal;

    public Transform tilesParent;

    public GameObject tilePrefab;
    public Color wallColor = Color.blue;
    public Color floorColor = Color.black;
    public Color batteryColor = Color.green;


    public TMP_Text countdownText;
    bool gameStarted = false;

    //hráč
    public RectTransform player;
    int playerX = 1;
    int playerY = 1;
    public Image playerImage;
    public Color flashColor =  Color.cyan;

    //pohyb hráča
    int dx = 0;
    int dy = 0;
    Vector2 targetPosition;
    float Speed = 400f;
    bool isMoving = false;

    //energia (skóre)
    int totalBatteries = 0;
    const int maxEnergy = 350;
    int energy = 0;
    public TMP_Text energyText;


    // 1. nepriateľ (randomer)
    public RectTransform enemyRandomer;
    int randomerX;
    int randomerY;
    int randx = 0;
    int randy = 0;

    Vector2 randomerTargetPosition;
    bool randomerMoving = false;
    float randomerSpeed = 250f;

    // 2. nepriateľ (chaser)
    public RectTransform enemyChaser;
    int chaserX;
    int chaserY;
    int chaserDX = 0;
    int chaserDY = 0;

    Vector2 chaserTargetPosition;
    bool chaserMoving = false;
    float chaserSpeed = 250f;


    // 3. nepriateľ (sneaky)
    public RectTransform enemySneaky;
    int sneakyX;
    int sneakyY;
    int sneakyDX = 0;
    int sneakyDY = 0;

    Vector2 sneakyTargetPosition;
    bool sneakyMoving = false;
    float sneakySpeed = 250f;

    //zvuk
    public AudioSource eatAudioSource;
    public AudioClip eatAudio;


    string[] mapTemplate =
    {   //"1111111111111111111W1W1111111111111111111",
        "11111111111111111111111111111111111111111",
        "12222222122222122221212222122222122222221",
        "12111112221112121121212112121112221111121",
        "12122222122222122221212222122222122222121",
        "12121111121111121121212112111112111112121",
        "12221222221222221222222212222212222212221",
        "11121211111211111211111211111211111212111",
        "12221211111211111211111211111211111212221",
        "12122222222222222222222222222222222222121",
        "12221211111211111211111211111211111212221",
        "11121211111211111211111211111211111212111",
        "12221222221222221222222212222212222212221",
        "12121111121111121121212112111112111112121",
        "12122222122222122221212222122222122222121",
        "12111112221112121121212112121112221111121",
        "12222222122222122221212222122222122222221",
        "11111111111111111111111111111111111111111"
      //"1111111111111111111W1W1111111111111111111",
    };

    string[] map =
  {   //"1111111111111111111W1W1111111111111111111",
        "11111111111111111111111111111111111111111",
        "12222222122222122221212222122222122222221",
        "12111112221112121121212112121112221111121",
        "12122222122222122221212222122222122222121",
        "12121111121111121121212112111112111112121",
        "12221222221222221222222212222212222212221",
        "11121211111211111211111211111211111212111",
        "12222222222222222222222222222222222222221",
        "12121111111111111111211111111111111112121",
        "12222222222222222222222222222222222222221",
        "11121211111211111211111211111211111212111",
        "12221222221222221222222212222212222212221",
        "12121111121111121121212112111112111112121",
        "12122222122222122221212222122222122222121",
        "12111112221112121121212112121112221111121",
        "12222222122222122221212222122222122222221",
        "11111111111111111111111111111111111111111"
      //"1111111111111111111W1W1111111111111111111",
    };


    public void StartGame()
    {
        for (int i = 0; i < mapTemplate.Length; i++)
        {
            map[i] = mapTemplate[i];
        }

        totalBatteries = CountBatteries();
        GenerateBoard(); //vyresetuje a vygeneruje sa mapa

        //player initialization
        playerX = 1;
        playerY = 8;
        player.anchoredPosition = GetPosition(playerX, playerY);

        dx = 0;
        dy = 0;
        isMoving = false;
        targetPosition = player.anchoredPosition;

        //initialize energy
        energy = 0;
        energyText.text = "Energy: " + energy;

        // spawn enemies
        //randomer initialization
        randomerX = 20; 
        randomerY = 10;
        enemyRandomer.anchoredPosition = GetPosition(randomerX, randomerY);

        chaserX = 24;
        chaserY = 10;
        enemyChaser.anchoredPosition = GetPosition(chaserX, chaserY);

        sneakyX = 22;
        sneakyY = 8;
        enemySneaky.anchoredPosition = GetPosition(sneakyX, sneakyY);

        randomerMoving = false;
        chaserMoving = false;
        sneakyMoving = false;


        gameStarted = false;
        StartCoroutine(StartCountdown());
    }

  

    int CountBatteries()
    {
        int k = 0;

        for (int y = 0; y < map.Length; y++)
        {
            for (int x = 0; x < map[y].Length; x++)
            {
                if (map[y][x] == '2') k++;
            }
        }
        return k;
    }


    /*void EndGame(bool win)
    {
        float percent = totalBatteries > 0 ? (float)energy / totalBatteries : 0f;

        int reward = 300 + Mathf.RoundToInt(percent * 300f);

        terminal.OnMinigameFinished(reward);
    }*/

    void EndGame(bool win)
    {
        float percent = totalBatteries > 0 ? (float)energy / totalBatteries : 0f;
        percent = Mathf.Clamp01(percent); //hodnota na 0-1, v pripade bugu, (boli anomalie že hrač dostal prilis velku odmenu)

        int reward = 300 + Mathf.RoundToInt(percent * 300f);
        terminal.OnMinigameFinished(reward);

    }


    void Update()
    {
        if (gameObject.activeSelf == false)
        {
            return;
        }

        if (!gameStarted)
        {
            return;
        }


        updateRandomer();
        updateChaser();
        updateSneaky();
        checkCollision();

        if (Input.GetKeyDown(KeyCode.W))
        {
            if (map[playerY - 1][playerX] != '1') { dx = 0; dy = -1; }
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (map[playerY + 1][playerX] != '1') { dx = 0; dy = 1; }
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (map[playerY][playerX - 1] != '1') { dx = -1; dy = 0; }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (map[playerY][playerX + 1] != '1') { dx = 1; dy = 0; }
        }

        if (isMoving)
        {
            player.anchoredPosition = Vector2.MoveTowards(player.anchoredPosition, targetPosition, Speed * Time.deltaTime);

            if (Vector2.Distance(player.anchoredPosition, targetPosition) < 0.03f)
            {
                player.anchoredPosition = targetPosition;
                isMoving = false;
            }
        }
        if (isMoving)
        {
            return;
        }

        if (dx == 0 && dy == 0)
        {
            return;
        }

        int nextX = playerX + dx;
        int nextY = playerY + dy;

        if (map[nextY][nextX] == '1')
        {
            return;
        }

        playerX = nextX;
        playerY = nextY;

        targetPosition = GetPosition(playerX, playerY);

        isMoving = true;

        if (map[playerY][playerX] == '2')
        {

            if (eatAudioSource != null && eatAudio != null)
            {
                eatAudioSource.PlayOneShot(eatAudio);
            }

            string row = map[playerY];
            string before = row.Substring(0, playerX);
            string after = row.Substring(playerX + 1);
            map[playerY] = before + "0" + after;

            energy++;
            energyText.text = "Energy: " + energy;

            if (energy >= totalBatteries)
            {
                EndGame(true);
                return;
            }


            int i = playerY * map[0].Length + playerX;

            Transform tile = tilesParent.GetChild(i);

            if (tile.childCount > 0)
            {
                //Destroy(tile.GetChild(0).gameObject);
                //?!?!
                tile.GetChild(0).gameObject.SetActive(false);
            }
        }


    }

    void updateChaser()
    {
        if (chaserMoving)
        {
            enemyChaser.anchoredPosition = Vector2.MoveTowards(
                enemyChaser.anchoredPosition,
                chaserTargetPosition,
                chaserSpeed * Time.deltaTime
            );

            if (Vector2.Distance(enemyChaser.anchoredPosition, chaserTargetPosition) < 0.01f)
            {
                enemyChaser.anchoredPosition = chaserTargetPosition;
                chaserMoving = false;
            }

            return;
        }

        List<Vector2Int> directions = new List<Vector2Int>();

        // hore
        if (map[chaserY - 1][chaserX] != '1' && !(chaserDX == 0 && chaserDY == 1))
            directions.Add(new Vector2Int(0, -1));

        // dole
        if (map[chaserY + 1][chaserX] != '1' && !(chaserDX == 0 && chaserDY == -1))
            directions.Add(new Vector2Int(0, 1));

        // vlavo
        if (map[chaserY][chaserX - 1] != '1' && !(chaserDX == 1 && chaserDY == 0))
            directions.Add(new Vector2Int(-1, 0));

        // vpravo
        if (map[chaserY][chaserX + 1] != '1' && !(chaserDX == -1 && chaserDY == 0))
            directions.Add(new Vector2Int(1, 0));

        // slepá ulička → povoľ otočenie
        if (directions.Count == 0)
        {
            if (map[chaserY - 1][chaserX] != '1') directions.Add(new Vector2Int(0, -1));
            if (map[chaserY + 1][chaserX] != '1') directions.Add(new Vector2Int(0, 1));
            if (map[chaserY][chaserX - 1] != '1') directions.Add(new Vector2Int(-1, 0));
            if (map[chaserY][chaserX + 1] != '1') directions.Add(new Vector2Int(1, 0));
        }

        if (directions.Count > 0)
        {
            Vector2Int best = directions[0];
            float bestDist = Vector2.Distance(
                new Vector2(chaserX + best.x, chaserY + best.y),
                new Vector2(playerX, playerY)
            );

            for (int i = 1; i < directions.Count; i++)
            {
                Vector2Int d = directions[i];

                float dist = Vector2.Distance(
                    new Vector2(chaserX + d.x, chaserY + d.y),
                    new Vector2(playerX, playerY)
                );

                if (dist < bestDist)
                {
                    best = d;
                    bestDist = dist;
                }
            }

            chaserDX = best.x;
            chaserDY = best.y;

            int nextX = chaserX + chaserDX;
            int nextY = chaserY + chaserDY;

            chaserX = nextX;
            chaserY = nextY;

            chaserTargetPosition = GetPosition(chaserX, chaserY);
            chaserMoving = true;
        }
    }

    void updateSneaky()
    {
        if (sneakyMoving)
        {
            enemySneaky.anchoredPosition = Vector2.MoveTowards(
                enemySneaky.anchoredPosition,
                sneakyTargetPosition,
                sneakySpeed * Time.deltaTime
            );

            if (Vector2.Distance(enemySneaky.anchoredPosition, sneakyTargetPosition) < 0.01f)
            {
                enemySneaky.anchoredPosition = sneakyTargetPosition;
                sneakyMoving = false;
            }

            return;
        }

        Vector2Int sneakyTarget = GetSneakyTarget();

        List<Vector2Int> directions = new List<Vector2Int>();

        if (map[sneakyY - 1][sneakyX] != '1' && !(sneakyDX == 0 && sneakyDY == 1))
            directions.Add(new Vector2Int(0, -1));

        if (map[sneakyY + 1][sneakyX] != '1' && !(sneakyDX == 0 && sneakyDY == -1))
            directions.Add(new Vector2Int(0, 1));

        if (map[sneakyY][sneakyX - 1] != '1' && !(sneakyDX == 1 && sneakyDY == 0))
            directions.Add(new Vector2Int(-1, 0));

        if (map[sneakyY][sneakyX + 1] != '1' && !(sneakyDX == -1 && sneakyDY == 0))
            directions.Add(new Vector2Int(1, 0));

        if (directions.Count == 0)
        {
            if (map[sneakyY - 1][sneakyX] != '1') directions.Add(new Vector2Int(0, -1));
            if (map[sneakyY + 1][sneakyX] != '1') directions.Add(new Vector2Int(0, 1));
            if (map[sneakyY][sneakyX - 1] != '1') directions.Add(new Vector2Int(-1, 0));
            if (map[sneakyY][sneakyX + 1] != '1') directions.Add(new Vector2Int(1, 0));
        }

        if (directions.Count > 0)
        {
            Vector2Int best = directions[0];

            float bestDist = Vector2.Distance(
                new Vector2(sneakyX + best.x, sneakyY + best.y),
                new Vector2(sneakyTarget.x, sneakyTarget.y)
            );

            for (int i = 1; i < directions.Count; i++)
            {
                Vector2Int d = directions[i];

                float dist = Vector2.Distance(
                    new Vector2(sneakyX + d.x, sneakyY + d.y),
                    new Vector2(sneakyTarget.x, sneakyTarget.y)
                );

                if (dist < bestDist)
                {
                    best = d;
                    bestDist = dist;
                }
            }

            sneakyDX = best.x;
            sneakyDY = best.y;

            sneakyX += sneakyDX;
            sneakyY += sneakyDY;

            sneakyTargetPosition = GetPosition(sneakyX, sneakyY);
            sneakyMoving = true;
        }
    }


    void checkCollision()
    {
        if (Vector2.Distance(player.anchoredPosition, enemyChaser.anchoredPosition) < 20f)
        {
            EndGame(false);
        }

        if (Vector2.Distance(player.anchoredPosition, enemyRandomer.anchoredPosition) < 20f)
        {
            EndGame(false);
        }

        if (Vector2.Distance(player.anchoredPosition, enemySneaky.anchoredPosition) < 20f)
        {
            EndGame(false);
            return;
        }
    }


    void updateRandomer()
    {
        if (randomerMoving)
        {
            enemyRandomer.anchoredPosition = Vector2.MoveTowards(enemyRandomer.anchoredPosition,randomerTargetPosition,randomerSpeed * Time.deltaTime);

            if (Vector2.Distance(enemyRandomer.anchoredPosition, randomerTargetPosition) < 0.01f)
            {
                enemyRandomer.anchoredPosition = randomerTargetPosition;
                randomerMoving = false;
            }

            return;
        }

        List<Vector2Int> directions = new List<Vector2Int>(); //možné smery

        if (map[randomerY - 1][randomerX] != '1' && (randx != 0 || randy != 1)) //ak je hore voľné políčko
        {                                                                       //funguje len ak hráč nemá aktuálne pohyb vľavo/vpravo/dole
            directions.Add(new Vector2Int(0, -1));
        }


        if (map[randomerY + 1][randomerX] != '1' && (randx != 0 || randy != -1)) //ak je DOLE voľné políčko
            directions.Add(new Vector2Int(0, 1));                                //funguje len ak hráč nemá aktuálne pohyb vľavo/vpravo/HORE

        if (map[randomerY][randomerX - 1] != '1' && (randx != 1 || randy != 0)) //ak je VLAVO voľné políčko
            directions.Add(new Vector2Int(-1, 0));

        if (map[randomerY][randomerX + 1] != '1' && (randx != -1 || randy != 0)) //ak je VPRAVO voľné políčko
            directions.Add(new Vector2Int(1, 0));

        // slepá ulička
        if (directions.Count == 0)
        {
            if (map[randomerY - 1][randomerX] != '1') directions.Add(new Vector2Int(0, -1));
            if (map[randomerY + 1][randomerX] != '1') directions.Add(new Vector2Int(0, 1));
            if (map[randomerY][randomerX - 1] != '1') directions.Add(new Vector2Int(-1, 0));
            if (map[randomerY][randomerX + 1] != '1') directions.Add(new Vector2Int(1, 0));
        }

        if (directions.Count > 0)
        {
            Vector2Int chosen = directions[Random.Range(0, directions.Count)];

            randx = chosen.x;
            randy = chosen.y;

            int nextX = randomerX + randx;
            int nextY = randomerY + randy;

            if (nextX == playerX && nextY == playerY)
            {
                EndGame(false);
                return;
            }

            randomerX = nextX;
            randomerY = nextY;

            randomerTargetPosition = GetPosition(randomerX, randomerY);
            randomerMoving = true;
        }
    }


    Vector2 GetPosition(int X, int Y)
    {
        float tileSize = 48f;

        float boardWidth = map[0].Length * tileSize;
        float boardHeight = map.Length * tileSize;

        float startX = -boardWidth / 2f;
        float startY = boardHeight / 2f;

        float finalX = startX + X * tileSize + tileSize / 2f;
        float finalY = startY - Y * tileSize - tileSize / 2f;

        return new Vector2(finalX, finalY);
    }


    void GenerateBoard()
    {
        foreach (Transform child in tilesParent)
        {
            Destroy(child.gameObject);
        }

        for (int y = 0; y < map.Length; y++)
        {
            for (int x = 0; x < map[y].Length; x++)
            {
                Image tile = Instantiate(tilePrefab, tilesParent).GetComponent<Image>();

                if (map[y][x] == '1')
                {
                    tile.color = wallColor;
                }
                else if (map[y][x] == '2')
                {
                    tile.color = floorColor;

                    GameObject battery = new GameObject("battery");
                    battery.transform.SetParent(tile.transform, false);

                    Image img = battery.AddComponent<Image>();
                    img.color = batteryColor;
                    img.raycastTarget = false; //optimalizacne, zlepšenie fps

                    RectTransform rt = battery.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.5f, 0.5f);
                    rt.anchorMax = new Vector2(0.5f, 0.5f);
                    rt.sizeDelta = new Vector2(12, 12);
                    rt.anchoredPosition = Vector2.zero;
                }
            }
        }
    }


    //skontroluj
    Vector2Int GetSneakyTarget()
    {
        int backX = -dx;
        int backY = -dy;

        int targetX = playerX;
        int targetY = playerY;

        for (int i = 0; i < 7; i++)
        {
            int nextX = targetX + backX;
            int nextY = targetY + backY;

            if (map[nextY][nextX] == '1')
                break;

            targetX = nextX;
            targetY = nextY;
        }

        return new Vector2Int(targetX, targetY);
    }

    IEnumerator PlayerFlash()
    {
        Image img = player.GetComponent<Image>();

        Color originalColor = img.color;
        Vector3 originalScale = player.localScale;

        for (int i = 0; i < 5; i++)
        {
            img.color = flashColor;
            player.localScale = originalScale * 1.35f;
            yield return new WaitForSeconds(0.18f);

            img.color = originalColor;
            player.localScale = originalScale;
            yield return new WaitForSeconds(0.18f);
        }

        img.color = originalColor;
        player.localScale = originalScale;
    }

    IEnumerator StartCountdown()
    {

        countdownText.gameObject.SetActive(true);
        StartCoroutine(PlayerFlash());

        countdownText.text = "3";
        yield return new WaitForSeconds(1f);

        countdownText.text = "2";
        yield return new WaitForSeconds(1f);

        countdownText.text = "1";
        yield return new WaitForSeconds(1f);

        countdownText.gameObject.SetActive(false);

        gameStarted = true;
    }
}
