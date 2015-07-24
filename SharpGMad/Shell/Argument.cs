using System;

namespace SharpGMad.Shell
{
    abstract class Argument
    {
        public string Name { get; protected set; }
        protected object Value;
        protected string StringValue;
        public bool Mandatory { get; set; }
        protected Func<string, object> Projector;
        protected string BindErrorMessage;
        public Type ResultType { get; protected set; }
        protected Action<string> TypedSetValueDelegate;

        protected Argument()
        {
            this.Name = String.Empty;
            this.Mandatory = false;
            this.Projector = ((s) => { return s; });
            this.BindErrorMessage = String.Empty;
            this.ResultType = String.Empty.GetType();
            this.TypedSetValueDelegate = ((s) => this.BluntSetValue(s));

            Reset();
        }

        private void BluntSetValue(object value)
        {
            this.Value = value;
            this.StringValue = value.ToString();
        }

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
}
