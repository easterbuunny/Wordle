using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    private static readonly KeyCode[] SUPPORTED_KEYS = new KeyCode[]
    {
        KeyCode.A,KeyCode.B,KeyCode.C,KeyCode.D,KeyCode.E,
        KeyCode.F,KeyCode.G,KeyCode.H,KeyCode.I,KeyCode.J,
        KeyCode.K,KeyCode.L,KeyCode.M,KeyCode.N,KeyCode.O,
        KeyCode.P,KeyCode.Q,KeyCode.R,KeyCode.S,KeyCode.T,
        KeyCode.U,KeyCode.V,KeyCode.W,KeyCode.X,KeyCode.Y,
        KeyCode.Z
    };

    private Row[] rows;

    private string[] solutions;
    private string[] validWords;
    private string word;

    private int rowIndex;
    private int columnIndex;
    private int score = 0;

    [Header("States")]
    public Tile.State emptyState;
    public Tile.State occupiedState;
    public Tile.State correctState;
    public Tile.State wrongSpotState;
    public Tile.State incorrectState;

    [Header("UI")]
    public TextMeshProUGUI invalidWordText;
    public TextMeshProUGUI titleWordText;
    public TextMeshProUGUI scoreText;
    public Button tryAgainButton;
    public Button newWordButton;


    private void Awake()
    {
        rows = GetComponentsInChildren<Row>();
    }

    private void Start()
    {
        LoadData();
        PlayGame();
    }

    private void LoadData()
    {
        TextAsset textfile = Resources.Load("official_wordle_all") as TextAsset;
        validWords = textfile.text.Split('\n');

        textfile = Resources.Load("official_wordle_common") as TextAsset;
        solutions = textfile.text.Split('\n');
    }

    public void PlayGame()
    {
        NewGame();
        enabled = true;
    }

    public void NewGame()
    {
        scoreText.text = "Score : "+score.ToString();
        CleanBoard();
        SetRandomWord();
        enabled = true;
    }

    public void TryAgain()
    {
        score = 0;
        scoreText.text = "Score : "+score.ToString();
        CleanBoard();
        SetRandomWord();
        enabled = true;
    }

    private void SetRandomWord()
    {
        word = solutions[Random.Range(0, solutions.Length)];
        word = word.ToLower().Trim();
    }
    // Update is called once per frame
    private void Update()
    {

        Row currentRow = rows[rowIndex];
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            columnIndex = Mathf.Max(columnIndex-1,0);
            currentRow.tiles[columnIndex].SetLetter('\0');
            currentRow.tiles[columnIndex].SetState(emptyState);
            //apparition d'objet dans le jeu
            invalidWordText.gameObject.SetActive(false);

        }
        else if (columnIndex >= currentRow.tiles.Length)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SubmitRow(currentRow);
            }
        }
        else
        {
            for (int i = 0; i < SUPPORTED_KEYS.Length; i++)
            {
                if (Input.GetKeyDown(SUPPORTED_KEYS[i]))
                {
                    currentRow.tiles[columnIndex].SetLetter((char)SUPPORTED_KEYS[i]);
                    currentRow.tiles[columnIndex].SetState(occupiedState);
                    columnIndex++;
                    break;
                }
            }
        }
    }

    /**
     * Si les lettre sont a la bonne position elle apparait verte
     * Si les lettre sont correcte mais pas a la bonne position elle apparait jaune
     * Si la lettre n'apparait dans le mot elle apparait rouge
     */
    private void SubmitRow(Row row)
    {
        if (!IsValidWord(row.word))
        {
            invalidWordText.gameObject.SetActive(true);
            return;
        }
            string remaining = word;

        for(int i = 0; i< row.tiles.Length; i++)
        {
            Tile tile = row.tiles[i];
            //Lettre a la bonne position
            if(tile.letter == word[i])
            {
                tile.SetState(correctState);
                remaining = remaining.Remove(i,1);
                remaining = remaining.Insert(i, " ");
            }
            //Lettre à la mauvaise position
            else if(!word.Contains(tile.letter))
            {
                tile.SetState(incorrectState);
            }
            
        }
        for(int i = 0; i< row.tiles.Length; i++)
        {
            Tile tile = row.tiles[i];
            //Lettre n'existe pas la dans le mot
            if(tile.state != correctState && tile.state != incorrectState)
            {
                if (remaining.Contains(tile.letter))
                {
                    tile.SetState(wrongSpotState);
                    int index = remaining.IndexOf(tile.letter);
                    remaining = remaining.Remove(i, 1);
                    remaining = remaining.Insert(i, " ");
                }
                else
                {
                    tile.SetState(incorrectState);
                }
            }
        }

        if (HasWon(row))
        {
            enabled = false;
            if (Input.GetKeyDown(KeyCode.Space))
            {
                NewGame();
            }
        }

        rowIndex++;
        columnIndex = 0;

        if( rowIndex >= rows.Length)
        {
            enabled = false;
        }
        
    }

    //Verifie le mot est un mot correcte
    private bool IsValidWord(string word)
    {
        for(int i = 0; i<validWords.Length; i++)
        {
            if (validWords[i] == word)
            {
                return true;
            }
        }
        return false;
    }
    
    //Verifie si toutes les case de ligne sont verte
    private bool HasWon(Row row)
    {
        for(int i = 0; i<row.tiles.Length; i++)
        {
            if(row.tiles[i].state != correctState)
            {
                return false;
            }
        }
        score++;
        scoreText.text = "Score : "+score.ToString();
        titleWordText.text = "WIN";
        newWordButton.gameObject.SetActive(true);
        return true;
    }

    //Nettoie le tableau (recommencer)
    private void CleanBoard()
    {
        for (int row = 0; row < rows.Length; row++)
        {
            for (int col = 0; col < rows[row].tiles.Length; col++)
            {
                rows[row].tiles[col].SetLetter('\0');
                rows[row].tiles[col].SetState(emptyState);
            }
        }

        rowIndex = 0;
        columnIndex = 0;
        titleWordText.text = "Wordle";
    }

    private void OnEnable()
    {
        tryAgainButton.gameObject.SetActive(false);
        newWordButton.gameObject.SetActive(false);
        invalidWordText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        tryAgainButton.gameObject.SetActive(true);
    }
}
