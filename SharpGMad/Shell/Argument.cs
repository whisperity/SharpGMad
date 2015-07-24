using System;

namespace SharpGMad.Shell
{
    abstract class Argument
    {
        public string Name { get; protected set; }
        protected object Value;
        protected string StringValue;
        public bool Mandatory { get; set; }
        public bool MultiParams { get; protected set; }
        protected Func<string, object> Projector;
        protected string BindErrorMessage;
        public Type ResultType { get; protected set; }
        protected Action<string> TypedSetValueDelegate;
        protected Func<bool> TypedHasValueDelegate;

        protected Argument()
        {
            this.Name = String.Empty;
            this.Mandatory = false;
            this.MultiParams = false;
            this.Projector = ((s) => { return s; });
            this.BindErrorMessage = String.Empty;
            this.ResultType = String.Empty.GetType();
            this.TypedSetValueDelegate = ((s) => this.BluntSetValue(s));
            this.TypedHasValueDelegate = (() => this.BluntHasValue());

            Reset();
        }

        private void BluntSetValue(object value)
        {
            this.Value = value;
            this.StringValue = value.ToString();
        }

        private bool BluntHasValue()
        {
            return this.Value != null;
        }

        public bool HasValue { get { return this.TypedHasValueDelegate(); } }

        public object GetValue()
        {
            return this.Value;
        }

        public void SetValue(string value)
        {
            this.TypedSetValueDelegate(value);
        }

        public void Reset()
        {
            this.Value = null;
            this.StringValue = String.Empty;
        }
    }

    class Argument<T> : Argument
    {
        private Func<string, T> TypedProjector;

        public Argument(string name, Func<string, T> proj, string bindErrMsg = "")
            : base()
        {
            base.Name = name;
            this.TypedProjector = proj;
            base.ResultType = proj.Method.ReturnType;
            base.BindErrorMessage = bindErrMsg;
            base.TypedSetValueDelegate = ((s) => this.TypedSetValue(s));
        }

        public Argument(string name, string value, Func<string, T> proj, string bindErrMsg = "")
            : this(name, proj, bindErrMsg)
        {
            if (proj == null)
                throw new ArgumentNullException("proj", "Projection delegate must be set. " +
                    "If you intend a string argument, use the base type Argument.");
            try
            {
                base.Value = proj(value);
            }
            catch (Exception e)
            {
                throw new ArgumentException(bindErrMsg, "value", e);
            }

            base.StringValue = value;
        }

        public Argument(string name, T value)
        {
            base.Name = name;
            base.Value = value;
            base.StringValue = value.ToString();
            base.ResultType = value.GetType();
        }

        public void TypedSetValue(string value)
        {
            try
            {
                this.Value = this.TypedProjector(value);
            }
            catch (Exception e)
            {
                throw new ArgumentException(this.BindErrorMessage, e);
            }

            this.StringValue = value;
        }
    }

    class ParamsArgument<T> : Argument
    {
        private Func<string, T> TypedProjector;

        public ParamsArgument(string name, Func<string, T> proj, string bindErrMsg = "")
            : base()
        {
            base.Name = name;
            this.TypedProjector = proj;
            base.ResultType = proj.Method.ReturnType;
            base.BindErrorMessage = bindErrMsg;
            base.TypedSetValueDelegate = ((s) => this.AddValue(s));
            base.TypedHasValueDelegate = (() => this.ParamsHasValue());
            base.Value = new T[0];
            base.MultiParams = true;
        }

        public ParamsArgument(string name, string value, Func<string, T> proj, string bindErrMsg = "")
            : this(name, proj, bindErrMsg)
        {
            if (proj == null)
                throw new ArgumentNullException("proj", "Projection delegate must be set. " +
                    "If you intend a string argument, use the base type Argument.");
            try
            {
                base.Value = new T[] { proj(value) };
            }
            catch (Exception e)
            {
                throw new ArgumentException(bindErrMsg, "value", e);
            }
        }

        public ParamsArgument(string name, T[] value)
        {
            base.Name = name;
            base.Value = value;
            base.ResultType = value.GetType();
        }

        public void AddValue(string value)
        {
            try
            {
                // Copy the old array into one that is one element longer and add the current value there.
                T[] oldArray = (T[])this.Value;
                if (oldArray == null)
                    oldArray = new T[] { };

                T[] newValueArray = new T[oldArray.Length + 1];
                Array.Copy(oldArray, newValueArray, oldArray.Length);
                newValueArray[oldArray.Length] = this.TypedProjector(value);

                this.Value = newValueArray;
            }
            catch (Exception e)
            {
                throw new ArgumentException(this.BindErrorMessage, e);
            }
        }

        private bool ParamsHasValue()
        {
            if (this.Value == null)
                return false;
            else
                return (((T[])this.Value).Length > 0);
        }
    }
}
