using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Lomont.WPF2048
{
    class NotifiableBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Safely raises the property changed event.
        /// </summary>
        /// <param name="propertyName">The name of the property to raise.</param>
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName); // this is only called in Debug
            var handler = PropertyChanged;
            if (handler != null) { handler(this, new PropertyChangedEventArgs(propertyName)); }
        }

        /// <summary>
        /// Safely raises the property changed event.
        /// </summary>
        /// <param name="selectorExpression">An expression like ()=>PropName giving the name of the property to raise.</param>
        protected virtual void NotifyPropertyChanged<T>(Expression<Func<T>> selectorExpression)
        {
            if (selectorExpression == null)
                throw new ArgumentNullException("selectorExpression");
            var body = selectorExpression.Body as MemberExpression;
            if (body == null)
                throw new ArgumentException("The body must be a member expression");
            NotifyPropertyChanged(body.Member.Name);
        }


        /// <summary>
        /// While in debug, check a string to make sure it is a valid property name. 
        /// </summary>
        /// <param name="propertyName">The name of the property to check.</param>
        [System.Diagnostics.Conditional("DEBUG")]
        private void VerifyPropertyName(string propertyName)
        {
            var type = this.GetType();
            var propInfo = type.GetProperty(propertyName);
            System.Diagnostics.Debug.Assert(propInfo != null, propertyName + " is not a property of " + type.FullName);
        }

        /// <summary>
        /// Set a field if it is not already equal. Return true if there was a change.
        /// </summary>
        /// <param name="field">The field backing to update on change</param>
        /// <param name="value">The new value</param>
        /// <param name="propertyName">The member name, filled in automatically in C# 5.0 and higher.</param>
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            // avoid possible infinite loops
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Set a field if it is not already equal. Return true if there was a change.
        /// </summary>
        /// <param name="field">The field backing to update on change</param>
        /// <param name="value">The new value</param>
        /// <param name="selectorExpression">An expression like ()=>PropName giving the name of the property to raise.</param>
        protected bool SetField<T>(ref T field, T value, Expression<Func<T>> selectorExpression)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            NotifyPropertyChanged(selectorExpression);
            return true;
        }

        #endregion
    }
}
