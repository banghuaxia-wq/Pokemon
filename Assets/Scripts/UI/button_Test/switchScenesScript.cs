using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class switchScenesScript : MonoBehaviour
{
    public Animator startScreenAnimator = null;
    public int sceneIndex;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void startGame()
    {
        startScreenAnimator.SetBool("isPlay", true);
        //gameObject.GetComponent<Button>().interactable = false;
        Invoke("goToScene", 1.1f);
    }

    public void goToScene()
    {
        SceneManager.LoadScene(sceneIndex);
        //gameObject.GetComponent<Button>().interactable = true;
    }
}
