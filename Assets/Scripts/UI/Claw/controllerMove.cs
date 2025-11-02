using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class controllerMove : MonoBehaviour
{
    public Animator controllerAnimator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        controllerAD();
    }

    public void controllerDirection()
    {
        float horizontal = Input.GetAxis("Horizontal");
        controllerAnimator.SetFloat("horizontalMove", horizontal);
    }

    public void controllerAD()
    {
        if (Input.GetKey(KeyCode.A))
        {
            controllerAnimator.SetBool("isLeft", true);
            controllerAnimator.SetBool("isRight", false);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            controllerAnimator.SetBool("isRight", true);
            controllerAnimator.SetBool("isLeft", false);
        }
        else
        {
            controllerAnimator.SetBool("isLeft", false);
            controllerAnimator.SetBool("isRight", false);
        }
    }
}
