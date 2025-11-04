using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class switchScenesScript : MonoBehaviour
{
    public Animator startScreenAnimator;
    public void startGame()
    {
        startScreenAnimator.SetBool("isPlay", true);
        //gameObject.GetComponent<Button>().interactable = false;
        Invoke("goToGameScene", 1.1f);
    }

    public void goToGameScene()
    {
        SceneManager.LoadScene(1);
        //gameObject.GetComponent<Button>().interactable = true;
    }
}
