using System;
using System.Collections.Generic;
using System.Text;

namespace PokemonGame.Services.Services
{
    public enum ServiceErrorKind
    {
        NotFound,
        DatabaseError,
    }
    public sealed class ServiceError
    {
        public ServiceErrorKind Kind { get; }
        public string Message { get; }

        private ServiceError(ServiceErrorKind kind, string message)
        {
            Kind = kind;
            Message = message;
        }

        // ── Factory helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Use when a DB record with a known identifier cannot be located.
        /// Example: ServiceError.NotFound("move", "id", moveId)
        /// </summary>
        public static ServiceError NotFound(string entity, string keyName, object keyValue)
            => new(ServiceErrorKind.NotFound,
                   $"{entity} with {keyName} '{keyValue}' was not found.");

        /// <summary>
        /// Use when a DB record looked up by name cannot be located.
        /// Example: ServiceError.NotFound("move", moveName)
        /// </summary>
        public static ServiceError NotFound(string entity, string name)
            => new(ServiceErrorKind.NotFound,
                   $"{entity} '{name}' was not found.");

        /// <summary>Use for unexpected database / query failures.</summary>
        public static ServiceError DatabaseError(string message)
            => new(ServiceErrorKind.DatabaseError, message);

        public override string ToString() => $"[{Kind}] {Message}";
    }

    // ── Result wrapper ────────────────────────────────────────────────────────

    /// <summary>
    /// Represents either a successful value or a <see cref="ServiceError"/>.
    /// Services return this instead of null or throwing exceptions.
    /// <para>
    /// Usage:
    /// <code>
    ///   var result = moveService.GetMove("Tackle");
    ///   if (!result.IsSuccess) { ShowError(result.Error); return; }
    ///   Use(result.Value);
    /// </code>
    /// </para>
    /// </summary>
    public sealed class ServiceResult<T>
    {
        public bool IsSuccess { get; }
        public T? Value { get; }
        public ServiceError? Error { get; }

        private ServiceResult(T value)
        {
            IsSuccess = true;
            Value = value;
        }

        private ServiceResult(ServiceError error)
        {
            IsSuccess = false;
            Error = error;
        }

        // ── Implicit conversions – lets you write: return value; / return error; ─

        public static implicit operator ServiceResult<T>(T value)
            => new(value);

        public static implicit operator ServiceResult<T>(ServiceError error)
            => new(error);

        public override string ToString()
            => IsSuccess ? $"Ok({Value})" : $"Err({Error})";
    }
}
