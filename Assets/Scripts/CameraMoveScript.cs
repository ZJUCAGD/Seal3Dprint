using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMoveScript : MonoBehaviour {
    //摄像机所要观察的物体
    public Transform target;
    //摄像机距离物体的距离  这里我们初始默认的是5
    public float distance = 5f;
    //摄像机相对于物体的旋转速度
    private float speedX = 240;
    private float speedY = 120;
    //限定摄像机只能旋转的角度
    private float minLimitY = -80f;
    private float maxLimitY = 240f;
    //这两个值用于保存摄像机自身的一个角度
    private float mX = 0.0f;
    private float mY = 0.0f;
    //这两个值用于限定摄像机距离物体的最大值和最小值
    public float maxDistance = 100;
    public float minDistance = 0.5f;
    //鼠标滚轮拉近镜头的速度
    private float zoomSpeed = 50f;
    public bool isNeedDamping = true;


    public float damping = 2.5f;


    void Start()
    {
        //mX = transform.eulerAngles.x;
        //mY = transform.eulerAngles.y;
    }


    void LateUpdate()
    {
        if (target != null && Input.GetMouseButton(1))
        {
            mX += Input.GetAxis("Mouse X") * speedX * Time.deltaTime;
            mY -= Input.GetAxis("Mouse Y") * speedY * Time.deltaTime;


            mY = ClampAngle(mY, minLimitY, maxLimitY);
        }


        distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);


        Quaternion mRotation = Quaternion.Euler(mY, mX, 0);
        Vector3 mPosition = mRotation * new Vector3(0, 0, -distance) + target.position+200*Vector3.up;


        if (isNeedDamping)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, mRotation, Time.deltaTime * damping);
            transform.position = Vector3.Lerp(transform.position, mPosition, Time.deltaTime * damping);
        }
        else
        {
            transform.rotation = mRotation;
            transform.position = mPosition;
        }
    }
    //限定角度
    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

}
