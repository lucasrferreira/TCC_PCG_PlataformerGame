using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MoreMountains.CorgiEngine
{

	[CustomEditor (typeof(CharacterBehavior))]
	[CanEditMultipleObjects]

	/// <summary>
	/// Adds custom labels to the CorgiController inspector
	/// </summary>

	public class CharacterBehaviorInspector : Editor 
	{
		
		void onEnable()
		{
			// nothing
		}
		
		/// <summary>
		/// When inspecting a Corgi Controller, we add to the regular inspector some labels, useful for debugging
		/// </summary>
		public override void OnInspectorGUI()
		{
			CharacterBehavior behavior = (CharacterBehavior)target;
			if (behavior.BehaviorState!=null)
			{
				EditorGUILayout.LabelField("Jumping",behavior.BehaviorState.Jumping.ToString());
				EditorGUILayout.LabelField("DoubleJumping",behavior.BehaviorState.DoubleJumping.ToString());
				EditorGUILayout.LabelField("Dashing",behavior.BehaviorState.Dashing.ToString());
				EditorGUILayout.LabelField("Running",behavior.BehaviorState.Running.ToString());
				EditorGUILayout.LabelField("WallClinging",behavior.BehaviorState.WallClinging.ToString());
				EditorGUILayout.LabelField("LadderClimbing",behavior.BehaviorState.LadderClimbing.ToString());
				EditorGUILayout.LabelField("LadderColliding",behavior.BehaviorState.LadderColliding.ToString());
				EditorGUILayout.LabelField("LadderTopColliding",behavior.BehaviorState.LadderTopColliding.ToString());
				EditorGUILayout.LabelField("LadderClimbingSpeed",behavior.BehaviorState.LadderClimbingSpeed.ToString());
				EditorGUILayout.LabelField("Gripping",behavior.BehaviorState.Gripping.ToString());
				EditorGUILayout.LabelField("Dangling",behavior.BehaviorState.Dangling.ToString());
			}
			DrawDefaultInspector();
			
			
		}
	}
}