﻿using UnityEngine;
using System.Collections;

public class Driving : MonoBehaviour
{
    public GameObject brakeLine;
    private GameControls gc;
    public GameObject leftLight;
    public GameObject rightLight;
    public Light leftLightColor;
    public Light rightLightColor;
    public ParticleEmitter leftSmoke;
    public ParticleEmitter rightSmoke;
    public UILabel gear;
    public AudioClip soundEngine;
    public AudioClip soundBrake;
    public Transform speedPointTrans;
    public WheelCollider flWheelCollider;
    public WheelCollider frWheelCollider;
    public WheelCollider rlWheelCollider;
    public WheelCollider rrWheelCollider;
    public Transform flWheelTrans;
    public Transform frWheelTrans;

    public Transform[] wheelTrans;
    public float motorTorque = 10;
    public float steerAngle = 10;
    public Transform centerOfMass;
    public float speed;
    private float wheelAngel;

    private GameObject firstCamera, thirdCamera,finishCamera;
    private bool isFirstPersonView=false;

    private Sound soundManager;
    public float maxSpeed = 60;
    private AudioSource audioSource;

    public Vector3 startPos;
    public Quaternion startQua;
    private bool isReset = false;
    private bool isFire = false;
    private float fireTime = 0;
    // Use this for initialization
    void Start()
    {
        startPos = transform.position;
        startQua = transform.rotation;
        gc = GameObject.Find("Finish").GetComponent<GameControls>();
        rigidbody.centerOfMass = centerOfMass.localPosition;
        thirdCamera = GameObject.Find("ThirdCamera");
        firstCamera = GameObject.Find("FirstCamera");
        finishCamera = GameObject.Find("FinishCamera");
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = soundEngine;
        leftLightColor = leftLight.GetComponent<Light>();
        rightLightColor = rightLight.GetComponent<Light>();
    }
    private float resetTime = 0;
    // Update is called once per frame
    void Update()
    {
        leftLightColor.color = Color.yellow;
        rightLightColor.color = Color.yellow;
        if(isFire)//漂移后的加速过程
        {
            leftLightColor.color = Color.blue;
            leftLight.SetActive(true);
            rightLightColor.color = Color.blue;
            rightLight.SetActive(true);

            fireTime += Time.deltaTime;
            if (speed < maxSpeed)
            {
                flWheelCollider.motorTorque = 4 * motorTorque;
                frWheelCollider.motorTorque = 4 * motorTorque;
            }
            if(fireTime>0.5f)
            {
                fireTime = 0;
                isFire = false;
            }
            print("Aaaa");
            return;
        }
        if (isReset)
        {
            resetTime += Time.deltaTime;
            if(resetTime>1)
            {
                rigidbody.isKinematic = false;
                resetTime = 0;
                isReset = false;
            }
        }
            
        if (!gc.isStart)
        {
            finishCamera.SetActive(false);
            firstCamera.SetActive(false);
            thirdCamera.SetActive(true);
            return;
        }
        speed = flWheelCollider.rpm * (flWheelCollider.radius * 2 * Mathf.PI) * 60 / 1000;
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isFire = true;
        }
        if (speed > 0)
            speedPointTrans.eulerAngles = new Vector3(0, 0, -130 - speed * 27 / 14);//仪表盘
        else
            speedPointTrans.eulerAngles = new Vector3(0, 0, -130);

        leftLight.SetActive(false);
        rightLight.SetActive(false);

        #region 视角控制
        if (gc.isOver)
        {
            finishCamera.SetActive(true);
            firstCamera.SetActive(false);
            thirdCamera.SetActive(false);
            audio.volume = (30 - Vector3.Distance(transform.position, thirdCamera.transform.position)) / 30;
            BrakeCar();
            return;
        }
        else
        {
            finishCamera.SetActive(false);
            if (Input.GetKeyDown(KeyCode.V))
            {
                isFirstPersonView = !isFirstPersonView;
            }
            if (isFirstPersonView)
            {
                firstCamera.SetActive(true);
                thirdCamera.SetActive(false);
            }
            else
            {
                firstCamera.SetActive(false);
                thirdCamera.SetActive(true);
            }
        }
        #endregion

        #region 换挡控制
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            maxSpeed = 80;
            gear.text = "1";
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            maxSpeed = 120;
            gear.text = "2";
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            maxSpeed = 160;
            gear.text = "3";
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            maxSpeed = 200;
            gear.text = "4";
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            maxSpeed = 280;
            gear.text = "5";
        }
        #endregion

        float axisV = Input.GetAxis("Vertical");
        float axisH = Input.GetAxis("Horizontal");

        if (Input.GetKey(KeyCode.R))
        {
            rigidbody.isKinematic = true;
            transform.localPosition = startPos;
            transform.localRotation = startQua;            
            isReset = true;
        }
        //加速、倒车
        
        if (axisV > 0)
        {
            leftSmoke.emit = true;
            rightSmoke.emit = true;         
        }
        else
        {
            leftSmoke.emit = false;
            rightSmoke.emit = false;
            if(axisV<0)
            {
                leftLight.SetActive(true);
                rightLight.SetActive(true);
            }
        }
        //漂移
        wheelAngel = axisH * steerAngle;

        WheelFrictionCurve a = new WheelFrictionCurve();
        a.asymptoteSlip = 2;
        a.extremumSlip = 1;
        a.stiffness = 1;
        WheelFrictionCurve b = a;
        bool isPY = false;

        Transform t = transform;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            leftLightColor.color = Color.red;
            leftLight.SetActive(true);
            rightLightColor.color = Color.red;
            rightLight.SetActive(true);

            //Quaternion q = Quaternion.Lerp(transform.rotation, t.rotation, 0);
            //print(q.eulerAngles.x + "  " + q.eulerAngles.y + "  " + q.eulerAngles.z);
            //Instantiate(brakeLine, rlWheelCollider.transform.position + Vector3.down * 0.46f, q);
            //Instantiate(brakeLine, rrWheelCollider.transform.position+Vector3.down*0.46f , rrWheelCollider.transform.rotation);

            t = transform;
            isPY = true;
            b.extremumValue = 0;
            b.asymptoteValue = 0;
            a.extremumValue = 0;
            a.asymptoteValue = 0;
            //BrakeCar();
            if (audioSource.clip != soundBrake)
            {
                audioSource.clip = soundBrake;
            }

            audioSource.pitch = 1;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            b.extremumValue = 20000;
            b.asymptoteValue = 10000;
            a.extremumValue = 20000;
            a.asymptoteValue = 10000;
        }


        flWheelCollider.sidewaysFriction = b;
        frWheelCollider.sidewaysFriction = b;
        rlWheelCollider.sidewaysFriction = a;
        rrWheelCollider.sidewaysFriction = a;
        ////
        flWheelCollider.steerAngle = wheelAngel;
        frWheelCollider.steerAngle = wheelAngel;
        if (speed * axisV < 0)
        {
            BrakeCar();
        }
        else
        {
            if(!isPY)
            audioSource.clip = soundEngine;
            if (speed > maxSpeed)
                speed = maxSpeed;
            audio.pitch = maxSpeed/300 + speed / maxSpeed;
            flWheelCollider.brakeTorque = 0;
            frWheelCollider.brakeTorque = 0;
            rlWheelCollider.brakeTorque = 0;
            rrWheelCollider.brakeTorque = 0;
            if(!audio.isPlaying)
            audio.Play();
        }
        if (speed < maxSpeed)
        {
            flWheelCollider.motorTorque = axisV * motorTorque * (100+speed) / maxSpeed;
            frWheelCollider.motorTorque = axisV * motorTorque * (100 + speed) / maxSpeed;
        }
        else
        {
            flWheelCollider.motorTorque = 0;
            frWheelCollider.motorTorque = 0;
        }

        WheelRotated(axisV);//控制轮子转动和转向     
    }
    float pyTime = 0;
    private void BrakeCar()
    {
        if (speed == 0)
        {
            leftLight.SetActive(false);
            rightLight.SetActive(false);
            audioSource.enabled = false;
            audio.Stop();
            return;
        }
        leftSmoke.emit = false;
        rightSmoke.emit = false;
        leftLight.SetActive(true);
        rightLight.SetActive(true);

        flWheelCollider.motorTorque = 0;
        frWheelCollider.motorTorque = 0;

        flWheelCollider.brakeTorque = 100;
        frWheelCollider.brakeTorque = 100;
        //audio.Stop();
        if (audioSource.clip != soundBrake)
        {
            audioSource.clip = soundBrake;
        }

        audioSource.pitch = 1;
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    private void WheelRotated(float axisV)
    {
        //转动 
        foreach (Transform wheel in wheelTrans)
        {
            wheel.Rotate(flWheelCollider.rpm * Time.deltaTime * 6, 0, 0);
        }
        //拐弯
        flWheelTrans.localEulerAngles = new Vector3(0, wheelAngel * 3, 0);
        frWheelTrans.localEulerAngles = new Vector3(0, wheelAngel * 3, 0);
    }
}
