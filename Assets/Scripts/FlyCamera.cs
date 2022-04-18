using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEditor.PackageManager;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class FlyCamera : MonoBehaviour
{
    private const float MaxEffectiveTPF = 1.0f;

    protected enum FlyCameraAction
    {
        //Rotation
        Left,
        Right,
        UP,
        Down,

        // Translation
        Forward,
        Backward,
        Leftward,
        Rightward,
        Rise,
        Lower,

        //Zoom
        ZoomIn,
        ZoomOut,

        Count
    }

    public float MoveSpend = 15;

    public float RotaionSpend = 25;

    public float ZoomSpeed = 1;

    private Vector3 _upVector;

    private float[] _actionStatus;

    private int _oldMouseX;
    private int _oldMouseY;
    private bool _rightMouseDown = false;
    // Start is called before the first frame update
    void Start()
    {
        _upVector = Vector3.up;
        _actionStatus = new float[(int)FlyCameraAction.Count];
    }

    // Update is called once per frame
    void Update()
    {
        _keyPressed(this);
        _keyReleased(this);
        _mouseMoved(this, (int) Input.mousePosition.x, (int) Input.mousePosition.y);
        _mouseScrolled(this);
        float effectiveTPF = Math.Min(Time.deltaTime, MaxEffectiveTPF);

        if (_actionStatus[(int)FlyCameraAction.Left] != 0)
        {
            _rotate_camera(transform, -_actionStatus[(int)FlyCameraAction.Left] * RotaionSpend, Vector3.up, _upVector);
            _actionStatus[(int)FlyCameraAction.Left] = 0;
        }

        if (_actionStatus[(int)FlyCameraAction.Right] != 0)
        {
            _rotate_camera(transform, _actionStatus[(int)FlyCameraAction.Right] * RotaionSpend, Vector3.up, _upVector);
            _actionStatus[(int)FlyCameraAction.Right] = 0;
        }
        if (_actionStatus[(int)FlyCameraAction.UP] != 0)
        {
            Vector3 camDir = transform.forward;
            float dotvy = Vector3.Dot(Vector3.up, camDir);
            Vector3 axis = Vector3.Cross(camDir, _upVector).normalized;
            if (dotvy > -0.99f)
            {
                _rotate_camera(transform, -_actionStatus[(int)FlyCameraAction.UP] * RotaionSpend, axis, _upVector);
            }
            _actionStatus[(int)FlyCameraAction.UP] = 0;
        }
        if (_actionStatus[(int)FlyCameraAction.Down] != 0)
        {
            Vector3 camDir = transform.forward;
            float dotvy = Vector3.Dot(Vector3.up, camDir);
            Vector3 axis = Vector3.Cross(camDir, _upVector).normalized;
            if (dotvy < 0.99f)
            {
                _rotate_camera(transform, _actionStatus[(int)FlyCameraAction.Down] * RotaionSpend, axis, _upVector);
            }
            _actionStatus[(int)FlyCameraAction.Down] = 0;
        }

        if (_actionStatus[(int)FlyCameraAction.Forward] != 0)
            _move_camera(transform, _actionStatus[(int)FlyCameraAction.Forward] * MoveSpend * effectiveTPF, transform.forward);
        if (_actionStatus[(int) FlyCameraAction.Backward] != 0)
            _move_camera(transform, -1 * (_actionStatus[(int) FlyCameraAction.Backward] * MoveSpend * effectiveTPF), transform.forward);
        
        if (_actionStatus[(int) FlyCameraAction.Leftward] != 0)
        {
            Vector3 direction = transform.forward;
            Vector3 up = transform.up;
            Vector3 left = Vector3.Cross(up, direction);
            _move_camera(transform, -1 * _actionStatus[(int)FlyCameraAction.Leftward] * MoveSpend * effectiveTPF, left);
        }

        if (_actionStatus[(int) FlyCameraAction.Rightward] != 0)
        {
            Debug.Log(transform.position);
            Vector3 right = Vector3.Cross(transform.up, transform.forward);
            _move_camera(transform, _actionStatus[(int)FlyCameraAction.Rightward] * MoveSpend * effectiveTPF, right);
            Debug.Log(transform.position);
        }

        if(_actionStatus[(int)FlyCameraAction.Lower] != 0)
            _move_camera(transform, - _actionStatus[(int)FlyCameraAction.Lower] * MoveSpend * effectiveTPF, Vector3.up);
        if(_actionStatus[(int)FlyCameraAction.Rise] != 0)
            _move_camera(transform, _actionStatus[(int)FlyCameraAction.Rise] * MoveSpend * effectiveTPF, Vector3.up);

        if (_actionStatus[(int) FlyCameraAction.ZoomIn] != 0)
        {
            _zoom_camera(transform, -_actionStatus[(int)FlyCameraAction.ZoomIn] * ZoomSpeed);
            _actionStatus[(int)FlyCameraAction.ZoomIn] = 0;
        }

        if (_actionStatus[(int) FlyCameraAction.ZoomOut] != 0)
        {
            _zoom_camera(transform, _actionStatus[(int)FlyCameraAction.ZoomOut] * ZoomSpeed);
            _actionStatus[(int) FlyCameraAction.ZoomOut] = 0;
        }
        
    }

    static void _flyCameraAction(FlyCamera flyCam, FlyCameraAction action, float value)
    {
        flyCam._actionStatus[(int)action] = value;
    }

    private void _keyPressed(FlyCamera flycam)
    {
        if (Input.GetKeyDown(KeyCode.W))
            _flyCameraAction(flycam, FlyCameraAction.Forward, 1);
        if (Input.GetKeyDown(KeyCode.S))
            _flyCameraAction(flycam, FlyCameraAction.Backward, 1);
        if (Input.GetKeyDown(KeyCode.A))
            _flyCameraAction(flycam, FlyCameraAction.Leftward, 1);
        if (Input.GetKeyDown(KeyCode.D))
            _flyCameraAction(flycam, FlyCameraAction.Rightward, 1);
        if (Input.GetKeyDown(KeyCode.Q))
            _flyCameraAction(flycam, FlyCameraAction.Lower, 1);
        if (Input.GetKeyDown(KeyCode.E))
            _flyCameraAction(flycam, FlyCameraAction.Rise, 1);
    }

    private void _keyReleased(FlyCamera flycam)
    {
        if (Input.GetKeyUp(KeyCode.W))
            _flyCameraAction(flycam, FlyCameraAction.Forward, 0);
        if (Input.GetKeyUp(KeyCode.S))
            _flyCameraAction(flycam, FlyCameraAction.Backward, 0);
        if (Input.GetKeyUp(KeyCode.A))
            _flyCameraAction(flycam, FlyCameraAction.Leftward, 0);
        if (Input.GetKeyUp(KeyCode.D))
            _flyCameraAction(flycam, FlyCameraAction.Rightward, 0);
        if (Input.GetKeyUp(KeyCode.Q))
            _flyCameraAction(flycam, FlyCameraAction.Lower, 0);
        if (Input.GetKeyUp(KeyCode.E))
            _flyCameraAction(flycam, FlyCameraAction.Rise, 0);
    }

    static void _mouseMoved(FlyCamera flycam, int x, int y)
    {
        if (Input.GetMouseButtonDown(1))
        {
            flycam._rightMouseDown = true;
            flycam._oldMouseX = x;
            flycam._oldMouseY = y;
        }

        if (Input.GetMouseButtonUp(1))
            flycam._rightMouseDown = false;
        if (flycam._rightMouseDown && (flycam._oldMouseX != x || flycam._oldMouseY != y))
        {
            int offsetX = x - flycam._oldMouseX;
            int offsetY = y - flycam._oldMouseY;
            if(offsetX > 0)
                _flyCameraAction(flycam,FlyCameraAction.Right, offsetX / 10.0f);
            else
                _flyCameraAction(flycam, FlyCameraAction.Left, -offsetX / 10.0f);
            if(offsetY > 0)
                _flyCameraAction(flycam, FlyCameraAction.Down, offsetY / 10.0f );
            else
                _flyCameraAction(flycam, FlyCameraAction.UP, -offsetY / 10.0f );
        }

        flycam._oldMouseX = x;
        flycam._oldMouseY = y;
    }

    static void _mouseScrolled(FlyCamera cam)
    {
        float signal = Input.GetAxis("Mouse ScrollWheel");
        if(signal > 0)
            _flyCameraAction(cam, FlyCameraAction.ZoomIn, (float)signal);
        if(signal < 0)
            _flyCameraAction(cam, FlyCameraAction.ZoomOut, -(float)signal);
    }

    static void _move_camera(Transform cam, float value, Vector3 axis)
    {
        Vector3 tmp;
        Vector3 pos = cam.position;
        tmp.x = axis.x * value;
        tmp.y = axis.y * value;
        tmp.z = axis.z * value;
        tmp += pos;
        cam.position = tmp;
    }

    static void _rotate_camera(Transform cam, float value, Vector3 axis, Vector3 up)
    {
        Quaternion quatSrc = cam.rotation;
        Quaternion quat = Quaternion.AngleAxis(value, axis);
        quat *= quatSrc;
        cam.rotation = quat;

        //Vector3 direction = cam.forward;
        //Vector3 left = Vector3.Cross(up, direction).normalized;
        //Vector3 fixedUp = Vector3.Cross(direction, left);
        //cam.up = fixedUp;
        //cam.forward = direction;
    }

    static void _zoom_camera(Transform cam, float value)
    {
        Camera c = cam.GetComponent<Camera>();
        if (!c)
        {
            return;
        }

        float zoom = 1.0f + 1.0f / 5.0f * value;
        if (c.orthographic)
        {
            c.orthographicSize = c.orthographicSize * zoom;
        }
        else
        {
            c.fieldOfView = c.fieldOfView * zoom;
        }
    }
}
