using System.Collections;
using UnityEngine;

public class SingletonMonoBehaviourDontDestroy<T> : MonoBehaviour where T : SingletonMonoBehaviourDontDestroy<T>
	{
		protected static T instance;
		public static T Instance
		{
			get
			{
				if (instance == null)
				{
					instance = (T)FindObjectOfType(typeof(T));

					if (instance == null)
					{
						Debug.LogWarning(typeof(T) + "is nothing");
					}
				}

				return instance;
			}
		}

		protected void Awake()
		{
			CheckInstance();

		}

		protected bool CheckInstance()
		{
			if (instance == null)
			{
				instance = (T)this;
				return true;
			}
			else if (Instance == this)
			{
				return true;
			}

			Destroy(this);

			// シーン遷移では破棄させない
			DontDestroyOnLoad(gameObject); // global

			return false;
		}
	}