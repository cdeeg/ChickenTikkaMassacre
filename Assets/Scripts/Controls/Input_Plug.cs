using UnityEngine;
using System.Collections;

namespace Input_Plug
{
	//struct for identifying the right button/key for an action//
	//can be ussed as an array for layered-buttonlayout;
	public struct Btn_id
	{
		public string	m_string;
		public KeyCode	m_keycode;
		
		public Btn_id(string m_string, KeyCode m_keycode)
		{
			this.m_string	= m_string;
			this.m_keycode	= m_keycode;
		}
	}

	//A Button or a key//
	//responsable to keep track of button states and values;
	//used to trigger (player)actions.
	public struct Button
	{
		public Btn_id id;				//button to look for
		public float time_pressed;		//timeStamp when button was pressed, for timed events or combo-click-frame ( <- needs different name);

		public Button(string m_string, KeyCode m_keycode)
		{
			id = new Btn_id(m_string, m_keycode);
			time_pressed = 0.0f;
		}

		public  bool just_pressed		// check -> Get_Down();
		{
			set{}
			get
			{
				if( Input.GetButtonDown( id.m_string ) || Input.GetKeyDown( id.m_keycode ) ) return true;
				else return false;
			}
		}

		public bool just_released		// checl -> Get_Up();
		{
			set{}
			get
			{
				if( Input.GetButtonUp( id.m_string ) || Input.GetKeyUp( id.m_keycode )) return true;
				else return false;
				
			}
		}

		public bool isPressed			// is true as long the button is down
		{
			set{}
			get
			{	//return toReturn = ( Input.getButton(id.m_string) || Input.GetKey(id.m_keycode) );
				bool toReturn = ( Input.GetButton(id.m_string) || Input.GetKey(id.m_keycode) );
				return toReturn;				
			}
		} 	

		public float time_duration	//difference between time_pressed & now;
		{
			set{}
			get{return Time.time - time_pressed;}
		}	

	}
	// Analog stick of a controller //
	// consists of two axis which are represented with values between -1 and 1;
	//it can be used as a button also;

	//Axis represented via keyboard/buttons
	public struct ButtonAxis
	{
		public readonly KeyCode Xn; //x_axis negative value -> -1.
		public readonly KeyCode Xp;	//x_axis positive value ->  1.
		public readonly KeyCode Yn;	//y_axis negative value -> -1.
		public readonly KeyCode Yp;	//y_axis positive value -> -1.

		public ButtonAxis(KeyCode Xn, KeyCode Xp, KeyCode Yn, KeyCode Yp)
		{
			this.Xn = Xn;
			this.Xp = Xn;
			this.Yn = Xn;
			this.Yp = Xn;
		}
	}

	public struct Analog_Stick
	{

		private string	axisName_x;	//raw x-axis-name;
		private string	axisName_y;	//raw y-axis-name;
		public Button	stickClick;	//hehe
		ButtonAxis		buttonAxis;	//for alternative input via keyboard or mouse

		public Analog_Stick(string axisName_x, string axisName_y, ButtonAxis buttonAxis, Button stickClick = new Button()) // (!* Button optional but could cause an error when wrongly used? *!)
		{
			this.axisName_x = axisName_x;
			this.axisName_y = axisName_y;
			this.buttonAxis = buttonAxis;
			this.stickClick  = stickClick;
		}

		public float x	// X value of the 2D_input
		{
			set{}
			get
			{
				float Xn = (Input.GetKey(buttonAxis.Xn)) ? -1.0f : 0.0f;
				float Xp = (Input.GetKey(buttonAxis.Xp)) ?  1.0f : 0.0f;
				float button_x =  Xn + Xp;
				float v =  Input.GetAxis(axisName_x) + button_x;

				return v;
			}
		}

		public float y	// Y value of the 2D_input
		{
			set{}
			get
			{
				int Yn = Input.GetKey(buttonAxis.Yn) ? -1 : 0;
				int Yp = Input.GetKey(buttonAxis.Yp) ?  1 : 0;
				float button_y =  Yn + Yp;
				float v =  Input.GetAxis(axisName_y) + button_y;
				
				return v;
			}
		}

		public float inputIntensity	//averange force of input - ranging from 0-1;
		{
			set{}
			get
			{
				return (Mathf.Abs(x) + Mathf.Abs(y)) / 2;
			}
		}//

		public Vector2 nVector		//normalized 2D_Vector for direction calculations
		{
			set{}
			get
			{
				Vector2 n = new Vector2(x,y);
				return n.normalized;
			}
		}
	}
}