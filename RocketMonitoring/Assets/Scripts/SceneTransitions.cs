using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitions : MonoBehaviour
{
    public static SceneTransitions instance;

    public float animationTime = 0.25f;
    Animator animator;

    [SerializeField]
    GameObject exitMenu;
    bool isExitMenuActive = false;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        DeactivateExitMenu();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (isExitMenuActive == false)
                ActivateExitMenu();
            else if (isExitMenuActive == true)
                DeactivateExitMenu();
        }
    }


    public void DarkenGame()
    {
        animator.SetBool("SceneEnd", true);
        animator.SetBool("SceneStart", false);
    }

    public void LightenGame()
    {
        animator.SetBool("SceneEnd", false);
        animator.SetBool("SceneStart", true);
    }

    public void ActivateExitMenu()
    {
        exitMenu.gameObject.SetActive(true);
        isExitMenuActive = true;
    }

    public void DeactivateExitMenu()
    {
        exitMenu.gameObject.SetActive(false);
        isExitMenuActive = false;
    }

    public void ExitApplication()
    {
        Application.Quit();
    }
}
