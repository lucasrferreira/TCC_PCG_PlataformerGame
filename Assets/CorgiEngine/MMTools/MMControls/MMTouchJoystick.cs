using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

namespace MoreMountains.Tools 
{
	[System.Serializable]
	public class JoystickEvent : UnityEvent<Vector2> {}

	/// <summary>
	/// Joystick input class.
	/// In charge of the behaviour of the joystick mobile touch input.
	/// Bind its actions from the inspector
	/// Handles mouse and multi touch
	/// </summary>
	[RequireComponent(typeof(Rect))]
	[RequireComponent(typeof(CanvasGroup))]
	public class MMTouchJoystick : MonoBehaviour, IDragHandler, IEndDragHandler
	{
		[Header("Camera")]
		public Camera TargetCamera;

		[Header("Pressed Behaviour")]
		[Information("Here you can set the opacity of the joystick when it's pressed. Useful for visual feedback.",InformationAttribute.InformationType.Info,false)]
		/// the new opacity to apply to the canvas group when the button is pressed
		public float PressedOpacity = 0.5f;

		[Header("Axis")]
		[Information("Choose if you want a joystick limited to one axis or not, and define the MaxRange. The MaxRange is the maximum distance from its initial center position you can drag the joystick to.",InformationAttribute.InformationType.Info,false)]
		/// Is horizontal axis allowed
		public bool HorizontalAxisEnabled = true;
		/// Is vertical axis allowed
		public bool VerticalAxisEnabled = true;
		/// The max range allowed
		[Information("And finally you can bind a function to get your joystick's values. Your method has to have a Vector2 as a parameter. Drag your object here and select the method.",InformationAttribute.InformationType.Info,true)]
		public float MaxRange = 1.5f;

		[Header("Binding")]
		/// The method(s) to call when the button gets pressed down
		public JoystickEvent JoystickValue;

		/// Store neutral position of the stick
		protected Vector2 _neutralPosition;
		/// Current horizontal and vertical values of the joystick (from -1 to 1)
		protected Vector2 _joystickValue;
		/// The canvas rect transform we're working with.
		protected RectTransform _canvasRectTransform;
		/// working vector
		protected Vector2 _newTargetPosition;
		protected Vector3 _newJoystickPosition;
		protected float _initialZPosition;
		protected RenderMode _parentCanvasRenderMode;


		/// <summary>
		/// On Start, we get our working canvas, and we set our neutral position
		/// </summary>
		protected virtual void Start()
		{
			_canvasRectTransform = GetComponentInParent<Canvas>().transform as RectTransform;
			SetNeutralPosition();
			if (TargetCamera == null)
			{
				throw new Exception("MMTouchJoystick : you have to set a target camera");
			}
			_parentCanvasRenderMode = GetComponentInParent<Canvas>().renderMode;
			_initialZPosition = transform.position.z;
		}

		/// <summary>
		/// On Update we check for an orientation change if needed, and send our input values.
		/// </summary>
		void Update()
		{
			if (JoystickValue != null)
			{
				if (HorizontalAxisEnabled || VerticalAxisEnabled)
				{
					JoystickValue.Invoke(_joystickValue);
				}
			}
		}

		/// <summary>
		/// Sets the neutral position of the joystick
		/// </summary>
		protected virtual void SetNeutralPosition()
		{
			_neutralPosition = GetComponent<RectTransform>().transform.position;
		}

		/// <summary>
		/// Handles dragging of the joystick
		/// </summary>
		public void OnDrag(PointerEventData eventData)
		{
			// if we're in "screen space - camera" render mode
			if (_parentCanvasRenderMode == RenderMode.ScreenSpaceCamera)
			{
					_newTargetPosition = TargetCamera.ScreenToWorldPoint(eventData.position);
			}
			// otherwise
			else
			{
				_newTargetPosition = eventData.position;
			}

			// We clamp the stick's position to let it move only inside its defined max range
			_newTargetPosition = Vector2.ClampMagnitude(_newTargetPosition - _neutralPosition, MaxRange);

			// If we haven't authorized certain axis, we force them to zero
			if (!HorizontalAxisEnabled)
			{
				_newTargetPosition.x = 0;
			}
			if (!VerticalAxisEnabled)
			{
				_newTargetPosition.y = 0;
			}
			// For each axis, we evaluate its lerped value (-1...1)
			_joystickValue.x = EvaluateInputValue(_newTargetPosition.x);
			_joystickValue.y = EvaluateInputValue(_newTargetPosition.y);

			_newJoystickPosition = _neutralPosition + _newTargetPosition;
			_newJoystickPosition.z = _initialZPosition;

			// We move the joystick to its dragged position
			transform.position = _newJoystickPosition;
		}

		/// <summary>
		/// What happens when the stick is released
		/// </summary>
		public void OnEndDrag(PointerEventData eventData)
		{
			// we reset the stick's position
			_newJoystickPosition = _neutralPosition;
			_newJoystickPosition.z = _initialZPosition;
			transform.position = _newJoystickPosition;
			_joystickValue.x = 0f;
			_joystickValue.y = 0f;
		}

		/// <summary>
		/// We compute the axis value from the interval between neutral position, current stick position (vectorPosition) and max range
		/// </summary>
		/// <returns>The axis value, a float between -1 and 1</returns>
		/// <param name="vectorPosition">stick position.</param>
		protected virtual float EvaluateInputValue(float vectorPosition)
		{
			return Mathf.InverseLerp(0, MaxRange, Mathf.Abs(vectorPosition)) * Mathf.Sign(vectorPosition);
		}
	}
}
