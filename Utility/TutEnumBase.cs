using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace TUT
{
	public class TutEnumCounter<EnumType> : TutSingleton<TutEnumCounter<EnumType>>
	{
		private int mCounter = -1; 
		
		private Dictionary<int,EnumType> mMembers = new Dictionary<int, EnumType>();

		public void IncCount()
		{
			mCounter++;
		}

		public void SetCount(int value)
		{
			mCounter = value;
		}

		public int Count
		{
			get
			{
				return mCounter;
			}
		}

		public void AddMember(int value,EnumType member)
		{
			if(!mMembers.ContainsKey(value))
			{
				mMembers.Add(value,member);
			}
			else
			{
				Debug.LogError("TutEnumCounter add member is failed : "+ member.GetType().ToString());
			}
		}

		public EnumType this[int index]
		{
			get
			{
				EnumType m = default (EnumType);
				mMembers.TryGetValue(index,out m);
				return m;
			}
		}
	}

	public class OhEnumBase<EnumType> : IComparable<OhEnumBase<EnumType>>, IEquatable<OhEnumBase<EnumType>> where EnumType : OhEnumBase<EnumType>
	{
		private int mEnumValue = -1;

		public int EnumValue
		{
			get
			{
				return mEnumValue;
			}
		}

		public OhEnumBase()
		{
			TutEnumCounter<EnumType>.Instance.IncCount ();
			mEnumValue = TutEnumCounter<EnumType>.Instance.Count;
			TutEnumCounter<EnumType>.Instance.AddMember (mEnumValue, (EnumType)this);
		}

		public OhEnumBase(int value)
		{
			TutEnumCounter<EnumType>.Instance.SetCount(value);
			mEnumValue = TutEnumCounter<EnumType>.Instance.Count;
			TutEnumCounter<EnumType>.Instance.AddMember (mEnumValue, (EnumType)this);
		}


		 public override string ToString ()
		{
			return mEnumValue.ToString ();
		}

		public static implicit operator OhEnumBase<EnumType>(int i)  
		{       
			EnumType std = TutEnumCounter<EnumType>.Instance [i];
			if(std != null)
			{
				return std;     
			}
			return (EnumType)System.Activator.CreateInstance (typeof(EnumType), new object[]{i});
		}

		public static implicit operator int(OhEnumBase<EnumType> e)        
		{        
			return e.EnumValue;     
		}
			        
		public int CompareTo(OhEnumBase<EnumType> other)
		{
			return this.EnumValue.CompareTo(other.EnumValue); 
		}
		 
			         
		public bool Equals(OhEnumBase<EnumType> other)   
		{        
			return this.EnumValue.Equals(other.EnumValue);
		}
		 
			         
		public override bool Equals(object obj)
		{            
			if (!(obj is OhEnumBase<EnumType>))              
				return false;      
			return this.EnumValue == ((OhEnumBase<EnumType>)obj).EnumValue;    
		}
		 
			         
		public override int GetHashCode()  
		{      
			EnumType std = TutEnumCounter<EnumType>.Instance [EnumValue];
			if(std != null)
			{
				return std.GetHashCode();
			}
			else
				return base.GetHashCode();
		}
		 
			        
		public static bool operator !=(OhEnumBase<EnumType> e1, OhEnumBase<EnumType> e2) 
		{ 
			if(  (object)e1  != null && (object)e2 != null)
				return e1.EnumValue != e2.EnumValue;  
			else
			if((object)e1  != null || (object)e2 != null )
					return true;
			return false;
		}
		     
		public static bool operator <(OhEnumBase<EnumType> e1, OhEnumBase<EnumType> e2)  
		{          
			if(  (object)e1  != null && (object)e2 != null)
			return e1.EnumValue < e2.EnumValue;    
			return false;
		}

		public static bool operator <=(OhEnumBase<EnumType> e1, OhEnumBase<EnumType> e2)     
		{
			if(  (object)e1  != null && (object)e2 != null)
				return e1.EnumValue <= e2.EnumValue;  
			return false;
		}
		 
		public static bool operator ==(OhEnumBase<EnumType> e1, OhEnumBase<EnumType> e2)
		{
			if(  (object)e1  != null && (object)e2 != null)
				return e1.EnumValue == e2.EnumValue;  
			else
				if((object)e1  != null || (object)e2 != null )
					return false;
			return true;
		}
		 
		public static bool operator >(OhEnumBase<EnumType> e1, OhEnumBase<EnumType> e2)  
		{
			if(  (object)e1  != null && (object)e2 != null)
			return e1.EnumValue > e2.EnumValue; 
			return false;
		}
		 
		public static bool operator >=(OhEnumBase<EnumType> e1, OhEnumBase<EnumType> e2)  
		{
			if(  (object)e1  != null && (object)e2 != null)
			return e1.EnumValue >= e2.EnumValue;  
			return false;
		}
	}
}
