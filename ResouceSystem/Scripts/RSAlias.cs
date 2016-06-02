using UnityEngine;
using System.Collections;

namespace TUT.RSystem
{
	public class RSAlias
	{
		protected string mAliasTarget = string.Empty;

		protected RSAlias(string target)
		{
			mAliasTarget = target;		
		}

		public string target
		{
			get
			{
				return mAliasTarget;
			}
		}

		public override string ToString ()
		{
			return mAliasTarget;
		}

		public static string operator +(string lhs, RSAlias rhs)
		{
			return lhs +  rhs.ToString();
		}

		public static string operator +(RSAlias lhs, string rhs)
		{
			return lhs.ToString() +  rhs;
		}
	}
}