using System;
using UnityEditor;
using UnityEngine.Assertions;

namespace JulianSchoenbaechler.Rendering.PlaygroundRP
{
    internal class SavedParameter<T> where T : IEquatable<T>
    {
        private readonly string key;

        internal delegate void SetParameter(string key, T value);
        internal delegate T GetParameter(string key, T defaultValue);

        readonly SetParameter setter;
        readonly GetParameter getter;

        private bool loaded;
        private T value;

        /// <summary>
        /// Gets or sets the value of this saved parameter.
        /// </summary>
        /// <value>The value of this saved parameter.</value>
        public T Value
        {
            get
            {
                Load();
                return this.value;
            }
            set
            {
                Load();

                if(this.value.Equals(value))
                    return;

                this.value = value;
                setter(key, value);
            }
        }

        /// <summary>
        /// Initializes an instance of the <see cref="SavedParamater{T}"/> class.
        /// </summary>
        /// <param name="key">The key of this saved parameter.</param>
        /// <param name="value">The value of this saved parameter.</param>
        /// <param name="getter">A getter delegate hook.</param>
        /// <param name="setter">A setter delegate hook.</param>
        internal SavedParameter(string key, T value, GetParameter getter, SetParameter setter)
        {
            Assert.IsNotNull(setter);
            Assert.IsNotNull(getter);

            this.key = key;
            this.loaded = false;
            this.value = value;
            this.setter = setter;
            this.getter = getter;
        }

        /// <summary>
        /// Load the saved parameter.
        /// </summary>
        private void Load()
        {
            if(loaded)
                return;

            loaded = true;
            value = getter(key, value);
        }
    }

    // Pre-specialized class for easier use and compatibility with existing code
    internal sealed class SavedBool : SavedParameter<bool>
    {
        internal SavedBool(string key, bool value)
            : base(key, value, EditorPrefs.GetBool, EditorPrefs.SetBool) { }
    }

    internal sealed class SavedInt : SavedParameter<int>
    {
        internal SavedInt(string key, int value)
            : base(key, value, EditorPrefs.GetInt, EditorPrefs.SetInt) { }
    }

    internal sealed class SavedFloat : SavedParameter<float>
    {
        internal SavedFloat(string key, float value)
            : base(key, value, EditorPrefs.GetFloat, EditorPrefs.SetFloat) { }
    }

    internal sealed class SavedString : SavedParameter<string>
    {
        internal SavedString(string key, string value)
            : base(key, value, EditorPrefs.GetString, EditorPrefs.SetString) { }
    }
}
