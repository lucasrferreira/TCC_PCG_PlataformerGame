using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace MoreMountains.CorgiEngine
{
	public class LevelSelectorGUI : MonoBehaviour 
	{
		public Image LevelNamePanel;
		public Text LevelNameText;
		public Vector2 LevelNameOffset;

		protected virtual void Start ()
	    {
	       GUIManager.Instance.SetHUDActive(false);

	       if (LevelNamePanel!=null && LevelNameText!=null)
			{
				LevelNamePanel.enabled=false;
				LevelNameText.enabled=false;
	       }
	    }

	    public virtual void SetLevelName(string levelName)
		{
			LevelNameText.text=levelName;
			LevelNamePanel.enabled=true;
			LevelNameText.enabled=true;
		}

		public virtual void TurnOffLevelName()
		{
			LevelNamePanel.enabled=false;
			LevelNameText.enabled=false;
		}
	}

}