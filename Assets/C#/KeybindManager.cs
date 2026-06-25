using UnityEngine;

public class KeybindManager : MonoBehaviour
{
    public static KeybindManager Instance;

    public KeyCode panjatPinangSingle = KeyCode.Space;
    public KeyCode balapKarungSingle = KeyCode.Space;
    public KeyCode tarikTambangSingle = KeyCode.F;
    public KeyCode makanKerupukSingle = KeyCode.Space;
    public KeyCode pakuBotolSingle = KeyCode.Space;
    public KeyCode lemparSandalSingle = KeyCode.Space;
    public KeyCode kelerengSingle = KeyCode.Space;
    public KeyCode lompatTaliSingle = KeyCode.Space;
    public KeyCode egrangSingleLeft = KeyCode.A;
    public KeyCode egrangSingleRight = KeyCode.D;
    public KeyCode bakiakSingleLeft = KeyCode.A;
    public KeyCode bakiakSingleRight = KeyCode.D;
    public KeyCode panjatPinangPlayer1 = KeyCode.Space;
    public KeyCode panjatPinangPlayer2 = KeyCode.RightShift;
    public KeyCode balapKarungPlayer1 = KeyCode.Space;
    public KeyCode balapKarungPlayer2 = KeyCode.RightShift;
    public KeyCode tarikTambangPlayer1 = KeyCode.F;
    public KeyCode tarikTambangPlayer2 = KeyCode.L;
    public KeyCode makanKerupukPlayer1 = KeyCode.Space;
    public KeyCode makanKerupukPlayer2 = KeyCode.RightShift;
    public KeyCode pakuBotolPlayer1 = KeyCode.Space;
    public KeyCode pakuBotolPlayer2 = KeyCode.RightShift;
    public KeyCode lemparSandalPlayer1 = KeyCode.Space;
    public KeyCode lemparSandalPlayer2 = KeyCode.RightShift;
    public KeyCode kelerengPlayer1 = KeyCode.Space;
    public KeyCode kelerengPlayer2 = KeyCode.RightShift;
    public KeyCode lompatTaliPlayer1 = KeyCode.Space;
    public KeyCode lompatTaliPlayer2 = KeyCode.RightShift;
    public KeyCode egrangPlayer1Left = KeyCode.A;
    public KeyCode egrangPlayer1Right = KeyCode.D;
    public KeyCode egrangPlayer2Left = KeyCode.LeftArrow;
    public KeyCode egrangPlayer2Right = KeyCode.RightArrow;
    public KeyCode bakiakPlayer1Left = KeyCode.A;
    public KeyCode bakiakPlayer1Right = KeyCode.D;
    public KeyCode bakiakPlayer2Left = KeyCode.LeftArrow;
    public KeyCode bakiakPlayer2Right = KeyCode.RightArrow;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadBindings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveBindings()
    {
        PlayerPrefs.SetString("PanjatPinang_Single", panjatPinangSingle.ToString());
        PlayerPrefs.SetString("BalapKarung_Single", balapKarungSingle.ToString());
        PlayerPrefs.SetString("TarikTambang_Single", tarikTambangSingle.ToString());
        PlayerPrefs.SetString("MakanKerupuk_Single", makanKerupukSingle.ToString());
        PlayerPrefs.SetString("PakuBotol_Single", pakuBotolSingle.ToString());
        PlayerPrefs.SetString("LemparSandal_Single", lemparSandalSingle.ToString());
        PlayerPrefs.SetString("Kelereng_Single", kelerengSingle.ToString());
        PlayerPrefs.SetString("LompatTali_Single", lompatTaliSingle.ToString());
        PlayerPrefs.SetString("Egrang_Single_Left", egrangSingleLeft.ToString());
        PlayerPrefs.SetString("Egrang_Single_Right", egrangSingleRight.ToString());
        PlayerPrefs.SetString("Bakiak_Single_Left", bakiakSingleLeft.ToString());
        PlayerPrefs.SetString("Bakiak_Single_Right", bakiakSingleRight.ToString());
        PlayerPrefs.SetString("PanjatPinang_Player1", panjatPinangPlayer1.ToString());
        PlayerPrefs.SetString("PanjatPinang_Player2", panjatPinangPlayer2.ToString());
        PlayerPrefs.SetString("BalapKarung_Player1", balapKarungPlayer1.ToString());
        PlayerPrefs.SetString("BalapKarung_Player2", balapKarungPlayer2.ToString());
        PlayerPrefs.SetString("TarikTambang_Player1", tarikTambangPlayer1.ToString());
        PlayerPrefs.SetString("TarikTambang_Player2", tarikTambangPlayer2.ToString());
        PlayerPrefs.SetString("MakanKerupuk_Player1", makanKerupukPlayer1.ToString());
        PlayerPrefs.SetString("MakanKerupuk_Player2", makanKerupukPlayer2.ToString());
        PlayerPrefs.SetString("PakuBotol_Player1", pakuBotolPlayer1.ToString());
        PlayerPrefs.SetString("PakuBotol_Player2", pakuBotolPlayer2.ToString());
        PlayerPrefs.SetString("LemparSandal_Player1", lemparSandalPlayer1.ToString());
        PlayerPrefs.SetString("LemparSandal_Player2", lemparSandalPlayer2.ToString());
        PlayerPrefs.SetString("Kelereng_Player1", kelerengPlayer1.ToString());
        PlayerPrefs.SetString("Kelereng_Player2", kelerengPlayer2.ToString());
        PlayerPrefs.SetString("LompatTali_Player1", lompatTaliPlayer1.ToString());
        PlayerPrefs.SetString("LompatTali_Player2", lompatTaliPlayer2.ToString());
        PlayerPrefs.SetString("Egrang_Player1_Left", egrangPlayer1Left.ToString());
        PlayerPrefs.SetString("Egrang_Player1_Right", egrangPlayer1Right.ToString());
        PlayerPrefs.SetString("Egrang_Player2_Left", egrangPlayer2Left.ToString());
        PlayerPrefs.SetString("Egrang_Player2_Right", egrangPlayer2Right.ToString());
        PlayerPrefs.SetString("Bakiak_Player1_Left", bakiakPlayer1Left.ToString());
        PlayerPrefs.SetString("Bakiak_Player1_Right", bakiakPlayer1Right.ToString());
        PlayerPrefs.SetString("Bakiak_Player2_Left", bakiakPlayer2Left.ToString());
        PlayerPrefs.SetString("Bakiak_Player2_Right", bakiakPlayer2Right.ToString());

        PlayerPrefs.Save();
    }

    public void LoadBindings()
    {
        if (PlayerPrefs.HasKey("PanjatPinang_Single"))
            panjatPinangSingle = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("PanjatPinang_Single"));

        if (PlayerPrefs.HasKey("BalapKarung_Single"))
            balapKarungSingle = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BalapKarung_Single"));

        if (PlayerPrefs.HasKey("TarikTambang_Single"))
            tarikTambangSingle = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("TarikTambang_Single"));

        if (PlayerPrefs.HasKey("MakanKerupuk_Single"))
            makanKerupukSingle = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("MakanKerupuk_Single"));

        if (PlayerPrefs.HasKey("PakuBotol_Single"))
            pakuBotolSingle = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("PakuBotol_Single"));

        if (PlayerPrefs.HasKey("LemparSandal_Single"))
            lemparSandalSingle = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LemparSandal_Single"));

        if (PlayerPrefs.HasKey("Kelereng_Single"))
            kelerengSingle = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Kelereng_Single"));

        if (PlayerPrefs.HasKey("LompatTali_Single"))
            lompatTaliSingle = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LompatTali_Single"));

        if (PlayerPrefs.HasKey("Egrang_Single_Left"))
            egrangSingleLeft = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Egrang_Single_Left"));

        if (PlayerPrefs.HasKey("Egrang_Single_Right"))
            egrangSingleRight = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Egrang_Single_Right"));

        if (PlayerPrefs.HasKey("Bakiak_Single_Left"))
            bakiakSingleLeft = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Bakiak_Single_Left"));

        if (PlayerPrefs.HasKey("Bakiak_Single_Right"))
            bakiakSingleRight = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Bakiak_Single_Right"));
            
        if (PlayerPrefs.HasKey("PanjatPinang_Player1"))
            panjatPinangPlayer1 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("PanjatPinang_Player1"));

        if (PlayerPrefs.HasKey("PanjatPinang_Player2"))
            panjatPinangPlayer2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("PanjatPinang_Player2"));
            
        if (PlayerPrefs.HasKey("BalapKarung_Player1"))
            balapKarungPlayer1 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BalapKarung_Player1"));

        if (PlayerPrefs.HasKey("BalapKarung_Player2"))
             balapKarungPlayer2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("BalapKarung_Player2"));

        if (PlayerPrefs.HasKey("TarikTambang_Player1"))
            tarikTambangPlayer1 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("TarikTambang_Player1"));

        if (PlayerPrefs.HasKey("TarikTambang_Player2"))
            tarikTambangPlayer2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("TarikTambang_Player2"));
   
        if (PlayerPrefs.HasKey("MakanKerupuk_Player1"))
            makanKerupukPlayer1 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("MakanKerupuk_Player1"));

        if (PlayerPrefs.HasKey("MakanKerupuk_Player2"))
            makanKerupukPlayer2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("MakanKerupuk_Player2"));

        if (PlayerPrefs.HasKey("PakuBotol_Player1"))
            pakuBotolPlayer1 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("PakuBotol_Player1"));

        if (PlayerPrefs.HasKey("PakuBotol_Player2"))
            pakuBotolPlayer2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("PakuBotol_Player2"));

        if (PlayerPrefs.HasKey("LemparSandal_Player1"))
            lemparSandalPlayer1 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LemparSandal_Player1"));

        if (PlayerPrefs.HasKey("LemparSandal_Player2"))
            lemparSandalPlayer2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LemparSandal_Player2"));

        if (PlayerPrefs.HasKey("Kelereng_Player1"))
            kelerengPlayer1 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Kelereng_Player1"));

        if (PlayerPrefs.HasKey("Kelereng_Player2"))
            kelerengPlayer2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Kelereng_Player2"));

        if (PlayerPrefs.HasKey("LompatTali_Player1"))
            lompatTaliPlayer1 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LompatTali_Player1"));

        if (PlayerPrefs.HasKey("LompatTali_Player2"))
            lompatTaliPlayer2 = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LompatTali_Player2"));

        if (PlayerPrefs.HasKey("Egrang_Player1_Left"))
            egrangPlayer1Left = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Egrang_Player1_Left"));

        if (PlayerPrefs.HasKey("Egrang_Player1_Right"))
            egrangPlayer1Right = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Egrang_Player1_Right"));

        if (PlayerPrefs.HasKey("Egrang_Player2_Left"))
            egrangPlayer2Left = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Egrang_Player2_Left"));

        if (PlayerPrefs.HasKey("Egrang_Player2_Right"))
            egrangPlayer2Right = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Egrang_Player2_Right"));

        if (PlayerPrefs.HasKey("Bakiak_Player1_Left"))
            bakiakPlayer1Left = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Bakiak_Player1_Left"));

        if (PlayerPrefs.HasKey("Bakiak_Player1_Right"))
            bakiakPlayer1Right = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Bakiak_Player1_Right"));

        if (PlayerPrefs.HasKey("Bakiak_Player2_Left"))
            bakiakPlayer2Left = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Bakiak_Player2_Left"));

        if (PlayerPrefs.HasKey("Bakiak_Player2_Right"))
            bakiakPlayer2Right = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Bakiak_Player2_Right"));

    }
}
