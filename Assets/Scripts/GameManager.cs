using UnityEngine;

// Create GameManager as a MonoBehaviour that persists between scenes
public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get { return instance; }
    }

    // Store selected car index
    private static int _selectedCarIndex;
    public static int selectedCarIndex
    {
        get { return _selectedCarIndex; }
        set { _selectedCarIndex = value; }
    }

    void Awake()
    {
        // Ensure only one instance exists between scenes
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
}