using UnityEngine;
using System.Collections;
using System;
using System.Reflection;

namespace TUT.AI
{

	public class TutAIStrategy
	{
		public Component StrategyParam = null;

		protected TutAIer mAIer = null;

		public delegate void InitFinish(TutAIStrategy strategy);

		public void InitStrategy(TutAIer ai,GameObject bind_obj)
		{
			mAIer = ai;
			if(bind_obj == null)
			{
				return;
			}
			TutAIParameter p = null;
			foreach (Attribute attr in this.GetType().GetCustomAttributes(false))
			{
				if (attr.GetType() == typeof(TutAIParameter))
				{
					p = attr as TutAIParameter;
					if(p.ParamType != null)
					{
						StrategyParam = bind_obj.GetComponent(p.ParamType);
					}
				}
			}
		}

		public virtual void BlockStrategy()
		{
				
		}

		public virtual IEnumerator ExeStrategy()
		{
			yield break;
		}

		public virtual void OnDestroy()
		{
			StrategyParam = null;
			mAIer = null;
		}
	}
}


