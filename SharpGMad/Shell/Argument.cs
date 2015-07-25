using System;

namespace SharpGMad.Shell
{
    /// <summary>
    /// The abstract base class to represent a command's argument
    /// </summary>
    abstract class Argument
    {
        /// <summary>
        /// The name of the argument
        /// </summary>
        public string Name { get; protected set; }
        /// <summary>
        /// The value of the argument
        /// </summary>
        protected object Value;

        public string Description;

        /// <summary>
        /// Whether the argument is mandatory - if yes, it must be assigned a value before executing the command
        /// </summary>
        public bool Mandatory { get; set; }
        /// <summary>
        /// Marks the argument to act as a "params parameter". Every value will be parsed into a typed array
        /// </summary>
        public bool MultiParams { get; protected set; }

        /// <summary>
        /// The real type of the argument's value
        /// </summary>
        public Type ResultType { get; protected set; }

        /// <summary>
        /// The error message to give if setting the value fails
        /// </summary>
        protected string BindErrorMessage;

        // These are needed to access the generics inherited from this base class
        /// <summary>
        /// The delegate over which the value can be set
        /// </summary>
        protected Action<string> TypedSetValueDelegate;
        /// <summary>
        /// The delegate over which it can be determined if a value exists
        /// </summary>
        protected Func<bool> TypedHasValueDelegate;

        protected Argument()
        {
            this.Name = String.Empty;
            this.Mandatory = false;
            this.MultiParams = false;
            this.BindErrorMessage = String.Empty;
            this.ResultType = String.Empty.GetType();
            this.TypedSetValueDelegate = ((s) => this.BluntSetValue(s));
            this.TypedHasValueDelegate = (() => this.BluntHasValue());

            Reset();
        }

        private void BluntSetValue(object value)
        {
            this.Value = value;
        }

        private bool BluntHasValue()
        {
            return this.Value != null;
        }

        /// <summary>
        /// True if a value is bound to this argument
        /// </summary>
        public bool HasValue { get { return this.TypedHasValueDelegate(); } }

        /// <summary>
        /// Get the value bound to this argument
        /// </summary>
        /// <returns>The value boxed as an Object.
        /// The executing command must make sure to cast the value to its real type.</returns>
        public object GetValue()
        {
            return this.Value;
        }

        /// <summary>
        /// Bind the given value to this argument
        /// </summary>
        /// <param name="value">The value as a string to be parsed</param>
        public void SetValue(string value)
        {
            this.TypedSetValueDelegate(value);
        }

        /// <summary>
        /// Unbind the argument's value
        /// </summary>
        public void Reset()
        {
            this.Value = null;
        }
    }

    /// <summary>
    /// A command's argument of arbitrary type
    /// </summary>
    /// <typeparam name="T">type of argument</typeparam>
    class Argument<T> : Argument
    {
        /// <summary>
        /// A function which converts the shell-given string value to the argument's real type
        /// </summary>
        private Func<string, T> TypedProjector;

        /// <summary>
        /// Create a new argument
        /// </summary>
        /// <param name="name">The name of the argument</param>
        /// <param name="proj">A function that converts a String value to the type of this argument</param>
        /// <param name="bindErrMsg">An error message to show if the conversion by proj fails</param>
        public Argument(string name, Func<string, T> proj, string bindErrMsg = "")
            : base()
        {
            base.Name = name;
            this.TypedProjector = proj;
            base.ResultType = proj.Method.ReturnType;
            base.BindErrorMessage = bindErrMsg;
            base.TypedSetValueDelegate = ((s) => this.TypedSetValue(s));
        }

        /// <summary>
        /// Bind the given value to this argument
        /// </summary>
        /// <param name="value">The value as a string to be parsed</param>
        private void TypedSetValue(string value)
        {
            try
            {
                this.Value = this.TypedProjector(value);
            }
            catch (Exception e)
            {
                throw new ArgumentException(this.BindErrorMessage, e);
            }
        }
    }

    /// <summary>
    /// An arbitrary type command argument having MultiParams set.
    /// These arguments hold the bound values in an array T[] instead of having only one value bound.
    /// </summary>
    /// <typeparam name="T">type of argument</typeparam>
    class ParamsArgument<T> : Argument
    {
        /// <summary>
        /// A function which converts the shell-given string value to the argument's real type
        /// </summary>
        private Func<string, T> TypedProjector;

        /// <summary>
        /// Create a new "params-like" argument
        /// </summary>
        /// <param name="name">The name of the argument</param>
        /// <param name="proj">A function that converts a String value to the type of this argument</param>
        /// <param name="bindErrMsg">An error message to show if the conversion by proj fails</param>
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

        /// <summary>
        /// Bind the given value to this argument.
        /// Because this is a MultiParams argument, the bound value will be added to the array of values.
        /// </summary>
        /// <param name="value">The value as a string to be parsed</param>
        private void AddValue(string value)
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

        /// <summary>
        /// Returns whether the MultiParams argument has at least one value bound
        /// </summary>
        private bool ParamsHasValue()
        {
            if (this.Value == null)
                return false;
            else
                return (((T[])this.Value).Length > 0);
        }
    }
}
