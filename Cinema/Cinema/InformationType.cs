namespace Cinema
{
    /// <summary>
    /// Перечисление для определения типа выводимой информации.
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// Обычное сообщение.
        /// </summary>
        None,
        /// <summary>
        /// Информация об ошибке.
        /// </summary>
        Error,
        /// <summary>
        /// Информация об успешном действии.
        /// </summary>
        Success,
        /// <summary>
        /// Обычная информация или инструкция.
        /// </summary>
        Info,
        /// <summary>
        /// Заголовок.
        /// </summary>
        Title
    }
}
