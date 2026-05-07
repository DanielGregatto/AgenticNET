namespace Util
{
    public interface IHandlerTempInfo
    {
        /// <summary>
        /// Retrieves the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key whose value to retrieve. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>The value associated with <paramref name="key"/>, or <see langword="null"/> if the key does not exist.</returns>
        string GetValue(string key);

        /// <summary>
        /// Retrieves the value associated with the specified key and deserializes it to the specified type.
        /// </summary>
        /// <typeparam name="T">The type to which the value will be deserialized.</typeparam>
        /// <param name="key">The key whose associated value is to be retrieved and deserialized. Cannot be <see langword="null"/> or
        /// empty.</param>
        /// <returns>The deserialized value of type <typeparamref name="T"/> associated with the specified key.</returns>
        T GetValueDescerialized<T>(string key);

        /// <summary>
        /// Sets the specified value for the given key, optionally specifying an expiration time in minutes.
        /// </summary>
        /// <param name="key">The key with which the value will be associated. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="value">The value to set for the specified key. Cannot be <see langword="null"/>.</param>
        /// <param name="addMinutes">The number of minutes until the value expires. If <see langword="null"/>, the value does not expire. Must be
        /// greater than zero if specified.</param>
        void SetValue(string key, string value, int? addMinutes);

        /// <summary>
        /// Sets the value associated with the specified key, serializing the value before storage.
        /// </summary>
        /// <param name="key">The key with which the value will be associated. Cannot be <see langword="null"/> or empty.</param>
        /// <param name="value">The object to serialize and store. Cannot be <see langword="null"/>.</param>
        /// <param name="addMinutes">The number of minutes to retain the value before it expires. If <see langword="null"/>, the value does not
        /// expire.</param>
        void SetValueSerialized(string key, object value, int? addMinutes);

        /// <summary>
        /// Removes the value with the specified key from the collection.
        /// </summary>
        /// <param name="key">The key of the element to remove. Cannot be <see langword="null"/>.</param>
        void Remove(string key);

        /// <summary>
        /// Determines whether a value is associated with the specified key.
        /// </summary>
        /// <param name="key">The key to locate in the collection. Cannot be <see langword="null"/>.</param>
        /// <returns><see langword="true"/> if the collection contains an element with the specified key; otherwise, <see
        /// langword="false"/>.</returns>
        bool Exists(string key);
    }
}