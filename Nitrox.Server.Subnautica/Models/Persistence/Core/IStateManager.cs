using System;
using System.Threading;
using System.Threading.Tasks;

namespace Nitrox.Server.Subnautica.Models.Persistence.Core;

/// <summary>
///     Provides instantiation and synchronized access for an <see cref="ObjectState" />.
/// </summary>
public interface IStateManager
{
    TaskCompletionSource<object> CompletionSource { get; set; }

    /// <summary>
    ///     The shared key used by all instances of the state.
    /// </summary>
    string GroupKey => StateType?.Name;

    /// <summary>
    ///     The unique instance key (within a group) of the state.
    /// </summary>
    string SubKey => GetType().Name;

    Type StateType => ObjectState?.GetType();

    object ObjectState { get; protected set; }

    object CreateDefault();

    /// <summary>
    ///     Sets the value as given or calls <see cref="CreateDefault" /> to reset the <see cref="ObjectState" /> to initial
    ///     values.
    /// </summary>
    void SetOrDefault(object value = null)
    {
        ObjectState = value ?? CreateDefault();
        CompletionSource?.TrySetResult(ObjectState);
    }
}

/// <inheritdoc cref="IStateManager" />
public interface IStateManager<TState> : IStateManager where TState : class, new()
{
    TState State { get; protected set; }

    string IStateManager.GroupKey => StateType.Name;

    object IStateManager.ObjectState
    {
        get => State;
        set
        {
            State = (TState)value;
            CompletionSource ??= new TaskCompletionSource<object>();
            CompletionSource?.TrySetResult(State);
        }
    }

    Type IStateManager.StateType => typeof(TState);

    /// <summary>
    ///     Waits for the state to become available.
    /// </summary>
    /// <remarks>
    ///     The state is either loaded from a file, see <see cref="Nitrox.Server.Subnautica.Services.PersistenceService"/>, or initialized to default value.
    /// </remarks>
    Task<TState> GetStateAsync(CancellationToken cancellationToken = default)
    {
        CompletionSource ??= new TaskCompletionSource<object>();
        cancellationToken.Register((o, token) => (o as TaskCompletionSource)?.TrySetCanceled(token), CompletionSource);
        return CompletionSource.Task.ContinueWith(static t => t.Result as TState, cancellationToken);
    }

    new TState CreateDefault() => new();

    void IStateManager.SetOrDefault(object value) => ObjectState = value ?? CreateDefault();

    object IStateManager.CreateDefault() => CreateDefault();
}
