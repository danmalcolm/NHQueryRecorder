using System;

namespace NHQueryRecorder.Tests.TestModels
{
	public class Thing
	{
		public virtual Guid Id { get; protected set; }

		public virtual string StringProperty { get; set; }

		public virtual bool BoolProperty { get; set; }

		public virtual DateTime DateProperty { get; set; }

		public virtual int IntProperty { get; set; }

		public virtual decimal DecimalProperty { get; set; }

		public virtual void CopyPropertiesFrom(Thing other)
		{
			this.StringProperty = other.StringProperty;
			this.BoolProperty = other.BoolProperty;
			this.DateProperty = other.DateProperty;
			this.IntProperty = other.IntProperty;
			this.DecimalProperty = other.DecimalProperty;
		}
	}
}