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
        /// Throws if isDisposed is true. Uses the same exception message and DisposedAtStackTrace pattern as the session overload. Use for non-session disposed state.
        /// </summary>
        public static void ThrowIfDisposed(bool isDisposed, string objectName, string disposedAtStackTrace)
        {
            if (!isDisposed)
                return;

            ThrowDisposed(objectName, disposedAtStackTrace);
        }

        /// <summary>
        /// Throws if session is null or if session.IsDisposed is true. Uses session.DisposedAtStackTrace for the exception message and Data when disposed. Pass objectName "Session" so the message indicates the session has been disposed, not the mapper.
        /// </summary>
        public static void ThrowIfDisposed(IDisposedSession session, string objectName)
        {
            if (session == null)
                throw new ObjectDisposedException(objectName, "Session is null.");

            if (!session.IsDisposed)
                return;

            ThrowDisposed(objectName, session.DisposedAtStackTrace);
        }

        static void ThrowDisposed(string objectName, string disposedAtStackTrace)
        {
            string message = $"{objectName} has been disposed and cannot be used.";
            if (!string.IsNullOrEmpty(disposedAtStackTrace))
                message += " Disposed at: " + disposedAtStackTrace;

            var ex = new ObjectDisposedException(objectName, message);
            if (!string.IsNullOrEmpty(disposedAtStackTrace))
                ex.Data["DisposedAtStackTrace"] = disposedAtStackTrace;

            throw ex;
        }
    }
}
