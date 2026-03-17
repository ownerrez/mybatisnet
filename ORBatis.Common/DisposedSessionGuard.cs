using System;

namespace IBatisNet.Common
{
    /// <summary>
    /// Implemented only by session types (e.g. ISqlMapSession) that can be disposed. Used by DisposedSessionGuard so the guard does not depend on mapper-specific types. Mapper types must not implement this; they validate the session parameter via the guard instead.
    /// </summary>
    public interface IDisposedSession
    {
        bool IsDisposed { get; }
        string DisposedAtStackTrace { get; }
    }

    /// <summary>
    /// Shared helper for throwing ObjectDisposedException when a session is null or used after dispose. Callers must pass the session argument (e.g. ISqlMapSession), not the mapper instance. Performs null check and disposed check via IDisposedSession.
    /// </summary>
    public static class DisposedSessionGuard
    {
        /// <summary>
        /// Throws if session is null or if session.IsDisposed is true. Uses session.DisposedAtStackTrace for the exception message and Data when disposed. Pass objectName "Session" so the message indicates the session has been disposed, not the mapper.
        /// </summary>
        public static void ThrowIfDisposed(IDisposedSession session, string objectName)
        {
            if (session != null && !session.IsDisposed)
                return;

            ObjectDisposedException ex;
            string message = $"{objectName} is {(session != null ? "disposed" : "null")} and cannot be used.";

            if (session != null && !string.IsNullOrEmpty(session.DisposedAtStackTrace))
            {
                message += " Disposed at: " + session.DisposedAtStackTrace;
            }

            ex = new ObjectDisposedException(objectName, message);

            if (session != null && !string.IsNullOrEmpty(session.DisposedAtStackTrace))
            {
                ex.Data["DisposedAtStackTrace"] = session.DisposedAtStackTrace;
            }

            throw ex;
        }
    }
}
