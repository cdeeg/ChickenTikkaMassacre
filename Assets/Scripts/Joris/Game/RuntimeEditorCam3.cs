//=================================================================================================================

using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

//=================================================================================================================

[RequireComponent(typeof(Camera))]
public class RuntimeEditorCam3 : MonoBehaviour
{
	#region fields

	public Transform	zoomOrigin;						//	ground position
	public float 		minZoom				= 5f;		//	min zoom to target
	public float 		maxZoom				= 50f;		//	max zoom to target
	public float 		minOrthoZoom		= 1;
	public float 		maxOrthoZoom		= 20;
	public float 		orthoXAngle			= 85;
	public float 		zoomSpeedOrtho		= 4f;
	public float 		zoomSpeed			= 60f;		//	zoom speed
	public float 		horizontalMove		= -1.5f;	//	middle mouse horizontal pan
	public float 		verticalMove		= -1.5f;	//	middle mouse vertical pan
	public float 		moveSpeed			= 5f;		//	speed of WSADQE movement
	public float 		moveEasing			= 0.7f;		//	easing of WSADQE movement
	public float 		pitchSpeed			= -5f;		//	right mouse camera x rotation
	public float 		yawSpeed			= 8f;		//	right mouse camera y rotation
	public bool			useAltKeyRotation	= true;		//	if true, left/right ALT must be pressed for camera rotation
	
	Camera			cam;


	float 			pitch;							//	current camera x rotation
	float 			yaw;							//	current camera y rotation
	Vector3 		lastMousePos;					//	last screenspace mouse position
	bool 			hasControlPermission;			//	can the player control the camera?

	float 			defaultFOV;

	static RuntimeEditorCam3 _instance;

	private bool Alt { get { return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt); } }

	#endregion

	#region IGameCamera

	/// <summary>
	/// Occurs whenever the camera changes its pan, zoom or rotation.
	/// </summary>
	public event System.Action<Camera> CameraChanged;

    public float ZoomFactor
    {
        get
        {
            return zoomFactor;
        }
    }

	public bool isAnimating
	{
		get { return false; }
	}

	public Camera GetCamera()
	{
		return cam;
	}
	
	#endregion

	//=================================================================================================================

	#region init

	void Start () 
	{
		Vector3 euler 			= transform.eulerAngles;
		pitch					= euler.x;
		yaw						= euler.y;
		cam						= GetComponent<Camera>();
		defaultFOV				= cam.fieldOfView;
		_instance				= this;

		lastMousePos			= Input.mousePosition;
		hasControlPermission	= true;

		if(zoomOrigin == null)
		{

		}

		//	calc zoom factor
		float d 	= zoomOrigin.position.y - transform.position.y;
		zoomFactor 	= jUtility.Math.MapF(Mathf.Abs(d), minZoom, maxZoom, 0, 1, true);
	}

	#endregion

	//=================================================================================================================

	#region interface

	//	set camera active / inactive
	public void SetControlPermission(bool b)
	{
		if(!b)
		{
			dragMouse = false;
		}
		hasControlPermission = b;
	}

	public void SetFOV(float fov, float time=0.3f)
	{
		StopAllCoroutines();
		StartCoroutine(_instance._animateFOV(fov, time));
	}
	public void SetFOVRelativeToDefault(float fov, float time=0.3f)
	{
		StopAllCoroutines();
		StartCoroutine(_instance._animateFOV(_instance.defaultFOV+fov, time));
	}
	public void SetFOVRelative(float fov, float time=0.3f)
	{
		StopAllCoroutines();
		StartCoroutine(_instance._animateFOV(_instance.cam.fieldOfView+fov, time));
	}
	public void SetFOVDefault(float time=0.3f)
	{
		StopAllCoroutines();
		StartCoroutine(_instance._animateFOV(_instance.defaultFOV, time));
	}

	IEnumerator _animateFOV(float targetFOV, float time)
	{
//		Debug.Log("animate FOV: " + targetFOV + " start: " + cam.fieldOfView);
		float startT 	= Time.time;
		float startFOV	= cam.fieldOfView;

		while(Time.time < startT)
		{
			cam.fieldOfView = Easing.EaseMovement(EaseType.CubicIN, Time.time-startT, startFOV, targetFOV-startFOV, time);
			yield return null;
		}
		cam.fieldOfView = targetFOV;
	}

	#endregion

	//------------------------------------------------------------------

	#region controls

	bool changedCamera;

	//	update on end of frame
	void LateUpdate () 
	{
		#if UNITY_EDITOR
		
		//	prevent camera controls in editor if gameview isn't focused
		if(EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text != "Game")
			return;
		
		#endif

		changedCamera = false;

		Zoom();
		Pan();
		Orbit();

		if(changedCamera && CameraChanged != null)
		{
			CameraChanged(GetComponent<Camera>());
		}

		lastMousePos = Input.mousePosition;
	}

    //------------------------------------------------------------------

    /// <summary>
    /// normalized zoom between minimum/maximum height
    /// </summary>
    float zoomFactor;

	//	zoom camera with scrollwheel
	float zoomEndT;
	void Zoom()
	{
		if(hasControlPermission )
		{
			if(!cam.orthographic)
			{
				//	perspective zoom
				float factor = 1;
				if(zoomOrigin != null)
				{
					float d 	= zoomOrigin.position.y - transform.position.y;
					factor 		= Easing.EaseMovement(EaseType.QuadraticIN, d, 0.5f, 2, 15);

					zoomFactor 	= jUtility.Math.MapF(Mathf.Abs(d), minZoom, maxZoom, 0, 1, true);
				}

				float axis	= Input.GetAxis("Mouse ScrollWheel");
				if(axis < 0 && zoomFactor >= 1)
				{
					return;
				}
				else if(axis > 0 && zoomFactor <= 0)
				{
					return;
				}

				Vector3 v 	= transform.forward * axis * Time.deltaTime * zoomSpeed * factor;

				float max = Mathf.Clamp(0.75f * factor, 0.2f, 1.8f);
				if(v.magnitude > max)
					v = v.normalized * max;
				transform.Translate(v, Space.World);

				if(v.magnitude > 0)
				{
					changedCamera = true;
				}
			}
			else
			{
				//	ortho zoom
				float axis	= Input.GetAxis("Mouse ScrollWheel");
				if(axis < 0 && zoomFactor >= 1)
				{
					return;
				}
				else if(axis > 0 && zoomFactor <= 0)
				{
					return;
				}
				float zoom = Mathf.Clamp( cam.orthographicSize - (axis * zoomSpeedOrtho * Time.deltaTime), minOrthoZoom, maxOrthoZoom);
				zoomFactor = jUtility.Math.MapF(zoom, minOrthoZoom, maxOrthoZoom, 0, 1);

				if(zoom != cam.orthographicSize)
				{
					changedCamera = true;
				}
				cam.orthographicSize = zoom;
			}
		}
	}

	//------------------------------------------------------------------

	//	drag camera with middle mouse / move with WSADQE
	bool dragMouse;
	Vector3 panMove;
	void Pan()
	{
		if(hasControlPermission && Input.GetMouseButtonDown(2))
		{
			dragMouse = true;
		}
		if(Input.GetMouseButtonUp(2))
		{
			dragMouse = false;
		}

		if(hasControlPermission)
		{
			if(dragMouse)
			{
				Vector3 delta 	= Input.mousePosition - lastMousePos;
				delta			/= 100;
				if(delta.magnitude > 0.01f)
				{
					transform.Translate(Vector3.right * delta.x * horizontalMove +
					                    Vector3.up * delta.y * verticalMove, Space.Self);
					changedCamera = true;
				}
			}
			else if(!cam.orthographic)
			{
				if(Input.GetKey(KeyCode.W))	panMove += transform.forward * moveSpeed * Time.deltaTime;
				if(Input.GetKey(KeyCode.S))	panMove -= transform.forward * moveSpeed * Time.deltaTime;
				if(Input.GetKey(KeyCode.A))	panMove -= transform.right 	 * moveSpeed * Time.deltaTime;
				if(Input.GetKey(KeyCode.D))	panMove += transform.right   * moveSpeed * Time.deltaTime;
				if(Input.GetKey(KeyCode.E))	panMove -= transform.up		 * moveSpeed * Time.deltaTime;
				if(Input.GetKey(KeyCode.Q))	panMove += transform.up		 * moveSpeed * Time.deltaTime;
			}
			else 
			{
				if(Input.GetKey(KeyCode.W))	panMove += transform.up * moveSpeed * Time.deltaTime;
				if(Input.GetKey(KeyCode.S))	panMove -= transform.up * moveSpeed * Time.deltaTime;
				if(Input.GetKey(KeyCode.A))	panMove -= transform.right 	 * moveSpeed * Time.deltaTime;
				if(Input.GetKey(KeyCode.D))	panMove += transform.right   * moveSpeed * Time.deltaTime;
			}
		}

		//	ease pan
		panMove *= moveEasing;
		if(panMove.magnitude > 0.005f)
			transform.position += panMove;
		else
			panMove = Vector3.zero;
	}

	//------------------------------------------------------------------

	//	right mouse xy rotation - if useAltKeyRotation is checked,
	//	left/right ALT key must be pressed for rotation
	void Orbit()
	{
		if(!cam.orthographic)
		{
			if((useAltKeyRotation ? Alt : true) && Input.GetMouseButton(1))
			{
				float yAxis	= Input.GetAxis("Mouse Y");
				float xAxis	= Input.GetAxis("Mouse X");
				pitch 		+= yAxis * pitchSpeed;
				yaw			+= xAxis * yawSpeed;

				transform.rotation = Quaternion.Euler(pitch, yaw, 0);

				if(xAxis != 0 || yAxis != 0)
				{
					changedCamera = true;
				}
			}
		}
		else
		{
			transform.rotation = Quaternion.AngleAxis(orthoXAngle, Vector3.right);
		}
	}

	#endregion
}


//=================================================================================================================