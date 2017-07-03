using System;
using System.Collections.Generic;

namespace Messaging
{
    public class DefaultCorrelationProvider : ICorrelationIdProvider
    {
        static readonly IEnumerator<string> CurrentId = GetId().GetEnumerator();

        static DefaultCorrelationProvider()
        {
            CurrentId.MoveNext(); // Make sure the enumerator starts with a value
        }

        static IEnumerable<string> GetId()
        {
            while (true)
                yield return Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Switches to a new "correlation context"
        /// </summary>
        /// <returns>The correlation id of the new context</returns>
        public string NewContext()
        {
            CurrentId.MoveNext();
            return GetCurrentCorrelationId();
        }

        /// <summary>
        /// Gets the current correlation id
        /// </summary>
        /// <returns>The correlation id of the current correlation context</returns>
        public string GetCurrentCorrelationId()
        {
            return CurrentId.Current;
        }
    }
}
