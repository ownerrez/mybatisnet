using System;

namespace IBatisNet.Common
{
    /// <summary>
    /// Implemented only by session types (e.g. ISqlMapSession) that can be closed or disposed. Used by DisposedSessionGuard so the guard does not depend on mapper-specific types. Mapper types must not implement this; they validate the session parameter via the guard instead.
    /// </summary>
    public interface IClosedSession
    {
        bool IsClosed { get; }
        string ClosedAtStackTrace { get; }
    }

    /// <summary>
    /// Shared helper for throwing ObjectDisposedException when a session is null or used after close/dispose. Callers must pass the session argument (e.g. ISqlMapSession), not the mapper instance. Performs null check and closed check via IClosedSession.
    /// </summary>
    public static class DisposedSessionGuard
    {
        /// <summary>
        /// Throws if isClosed is true. Uses the same exception message and ClosedAtStackTrace pattern as the session overload. Use for mapper or other non-session disposed state.
        /// </summary>
        public static void ThrowIfClosed(bool isClosed, string objectName, string closedAtStackTrace)
        {
            if (!isClosed)
                return;

            ThrowClosed(objectName, closedAtStackTrace);
        }

        /// <summary>
        /// Throws if session is null or if session.IsClosed is true. Uses session.ClosedAtStackTrace for the exception message and Data when closed.
        /// </summary>
        public static void ThrowIfClosed(IClosedSession session, string objectName)
        {
            if (session == null)
                throw new ObjectDisposedException(objectName, "Session is null.");

            if (!session.IsClosed)
                return;

            ThrowClosed(objectName, session.ClosedAtStackTrace);
        }

        static void ThrowClosed(string objectName, string closedAtStackTrace)
        {
            string message = $"{objectName} has been closed and cannot be used.";
            if (!string.IsNullOrEmpty(closedAtStackTrace))
                message += " Closed at: " + closedAtStackTrace;

            var ex = new ObjectDisposedException(objectName, message);
            if (!string.IsNullOrEmpty(closedAtStackTrace))
                ex.Data["ClosedAtStackTrace"] = closedAtStackTrace;

            throw ex;
        }
    }
}
