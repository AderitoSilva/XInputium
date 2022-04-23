using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace XInputium;

/// <summary>
/// Provides the base class for objects that need to implement 
/// <see cref="INotifyPropertyChanged"/> interface and offers 
/// event dispatching functionality. This is an abstract class.
/// </summary>
/// <remarks>
/// Objects that inherit from <see cref="EventDispatcherObject"/> 
/// can either invoke events immediately or enqueue them for 
/// sequential invocation at a specific moment. In the context 
/// of the <see cref="EventDispatcherObject"/>, postponing events 
/// for later invocation is called Event Dispatching. Event 
/// dispatching allows for several changes to be made to an 
/// object before effectively reporting them to event listeners. 
/// This is useful when the state of an object needs to be 
/// fully configured before the state change is reported to 
/// outside code.
/// <br/><br/>
/// To use <see cref="EventDispatcherObject"/>, inheritors 
/// specify the event dispatching mode to use, when instantiating 
/// the base <see cref="EventDispatcherObject"/> through its 
/// constructor. If inheritors specify
/// <see cref="EventDispatchMode.Immediate"/> mode, the object 
/// instance will behave like other conventional .NET objects, 
/// raising events immediately as changes occur. If inheritors 
/// specify <see cref="EventDispatchMode.Deferred"/>, all event 
/// invocations will be enqueued. Enqueued event invocations 
/// are invoked when inheritors call <see cref="DispatchEvents()"/> 
/// protected method. To register an event invocation with the 
/// <see cref="EventDispatcherObject"/>, inheritors call the 
/// protected <see cref="RaiseEvent(Action)"/> method to raise 
/// an event, instead of raising the events directly. This allows 
/// internal logic to know what events inheritors want to invoke 
/// and allows inheritors to invoke conventional events. Only 
/// events you pass to <see cref="RaiseEvent(Action)"/> method 
/// will be eligible for postponing. Events raised directly, 
/// will be invoked immediately, regardless of the event 
/// dispatching mode in use. If you need to cancel all queued 
/// events from being invoked in the next event dispatching, 
/// use <see cref="ClearEvents()"/> method.
/// <br/><br/>
/// If inheritors specify <see cref="EventDispatchMode.Deferred"/> 
/// event dispatching mode, and they enqueue events using 
/// <see cref="RaiseEvent(Action)"/> method and they never call 
/// <see cref="DispatchEvents()"/> method, enqueued events will 
/// never be dequeued and memory consumption will keep growing 
/// as the queue grows. This is a potential memory leak inheritors 
/// need to avoid.
/// <br/><br/>
/// <see cref="EventDispatcherObject"/> implements 
/// <see cref="INotifyPropertyChanged"/> interface, enabling 
/// consumers to get notified when the value of a property 
/// changes. You can use the protected 
/// <see cref="SetProperty{T}(ref T, in T, string?)"/> method 
/// to change the value of a property and automatically notify 
/// consumers as necessary. In other cases, you call 
/// <see cref="OnPropertyChanged(string?)"/> method to notify 
/// consumers about a specific property change. You can also 
/// compare property names using one of the protected static 
/// <see cref="PropertyNameEquals(string?, string?)"/> method 
/// overloads.
/// </remarks>
/// <seealso cref="INotifyPropertyChanged"/>
public abstract class EventDispatcherObject
    : INotifyPropertyChanged
{


    #region Fields and constants

    private const EventDispatchMode DefaultEventDispatchMode = EventDispatchMode.Immediate;

    private static readonly PropertyChangedEventArgs EmptyPropertyChangedEventArgs = new(null);
    private static readonly StringComparer PropertyNameComparer = StringComparer.Ordinal;

    private List<Action>? _events;  // Stores the queued events, enqueued for dispatching.

    #endregion Fields and constants


    #region Constructors

    /// <summary>
    /// Initializes a new instance of an <see cref="EventDispatcherObject"/> 
    /// class, that uses the specified event dispatching mode.
    /// </summary>
    /// <param name="eventDispatchMode">Event dispatching mode 
    /// to use in the new instance.</param>
    /// <exception cref="ArgumentException"><paramref name="eventDispatchMode"/> 
    /// is not a defined constant of an 
    /// <see cref="XInputium.EventDispatchMode"/> enumeration.</exception>
    protected EventDispatcherObject(EventDispatchMode eventDispatchMode)
    {
        if (!Enum.IsDefined(eventDispatchMode))
            throw new ArgumentException(
                $"'{eventDispatchMode}' is not a defined constant " +
                $"of an '{nameof(XInputium.EventDispatchMode)}' enumeration.",
                nameof(eventDispatchMode));

        EventDispatchMode = eventDispatchMode;
    }


    /// <summary>
    /// Initializes a new instance of an <see cref="EventDispatcherObject"/> 
    /// class, that uses <see cref="XInputium.EventDispatchMode.Immediate"/> 
    /// as its event dispatching mode.
    /// </summary>
    public EventDispatcherObject()
        : this(DefaultEventDispatchMode)
    {

    }

    #endregion Constructors


    #region Events

    /// <summary>
    /// It's invoked when the value of a property in the 
    /// <see cref="EventDispatcherObject"/> changes.
    /// </summary>
    /// <seealso cref="OnPropertyChanged(PropertyChangedEventArgs?)"/>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion Events


    #region Properties

    /// <summary>
    /// Gets the event dispatch mode in use for the current 
    /// <see cref="EventDispatcherObject"/>.
    /// </summary>
    protected EventDispatchMode EventDispatchMode { get; }


    /// <summary>
    /// Gets a <see cref="bool"/> indicating if there are 
    /// currently any events enqueued for dispatching.
    /// </summary>
    /// <seealso cref="EventDispatchMode"/>
    /// <seealso cref="DispatchEvents()"/>
    protected bool HasQueuedEvents => _events is not null && _events.Count > 0;

    #endregion Properties


    #region Methods

    /// <summary>
    /// Adds the specified action as event raising code to 
    /// the event dispatching queue. Or, if the current event 
    /// dispatch mode is set to <see cref="EventDispatchMode.Immediate"/>, 
    /// immediately invokes the event code, without queuing 
    /// it.
    /// </summary>
    /// <param name="action">An <see cref="Action"/> delegate 
    /// containing code that raises a specific event of the 
    /// current <see cref="EventDispatcherObject"/>.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="action"/> is <see langword="null"/>.</exception>
    protected virtual void RaiseEvent(Action action)
    {
        if (action is null)
            throw new ArgumentNullException(nameof(action));

        if (EventDispatchMode == EventDispatchMode.Immediate)
        {
            action.Invoke();
        }
        else if (EventDispatchMode == EventDispatchMode.Deferred)
        {
            _events ??= new();
            _events.Add(action);
        }
    }


    /// <summary>
    /// Removes all enqueued events from the event dispatching 
    /// queue.
    /// </summary>
    /// <seealso cref="RaiseEvent(Action)"/>
    /// <seealso cref="DispatchEvents()"/>
    protected void ClearEvents()
    {
        _events?.Clear();
    }


    /// <summary>
    /// Dispatches all events enqueued in the event dispatching 
    /// queue, if there are any, and removes them from the 
    /// queue.
    /// </summary>
    /// <remarks>
    /// This method invokes all event raising code enqueued 
    /// by <see cref="RaiseEvent(Action)"/> method and 
    /// clears the event dispatching queue.
    /// </remarks>
    /// <seealso cref="RaiseEvent(Action)"/>
    /// <seealso cref="ClearEvents()"/>
    protected virtual void DispatchEvents()
    {
        if (_events is not null && _events.Count > 0)
        {
            try
            {
                // We are looping through the list this way, because the list
                // may change during the invocation of an event. Events that
                // are enqueued by one of the event handlers will be added to 
                // the end of the queue and, therefore, they will be invoked
                // before the loop finishes.
                int index = 0;
                while (index < _events.Count)
                {
                    _events[index].Invoke();
                    index++;
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                _events.Clear();
            }
        }
    }


    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="e"><see cref="PropertyChangedEventArgs"/> instance 
    /// containing information about the event or <see langword="null"/> 
    /// to use a default empty instance.</param>
    /// <seealso cref="PropertyChanged"/>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs? e)
    {
        RaiseEvent(() => PropertyChanged?.Invoke(this, e ?? EmptyPropertyChangedEventArgs));
    }


    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event using the specified 
    /// property name.
    /// </summary>
    /// <param name="propertyName">Name of the changed property or 
    /// <see langword="null"/> to specify no specific property. If omitted, 
    /// this parameter will be the name of the caller property or method.</param>
    /// <seealso cref="OnPropertyChanged(PropertyChangedEventArgs?)"/>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(propertyName is null
            ? EmptyPropertyChangedEventArgs
            : new PropertyChangedEventArgs(propertyName));
    }


    /// <summary>
    /// Sets the specified property with the specified value, and raises 
    /// the <see cref="PropertyChanged"/> event if the values differ.
    /// </summary>
    /// <typeparam name="T">Type of the property value.</typeparam>
    /// <param name="oldValue">Original value of the property. This is a 
    /// reference to the member that will be set with the new value.</param>
    /// <param name="newValue">New value to set to the property.</param>
    /// <param name="propertyName">Optional. Name of the property that is 
    /// being set, or <see langword="null"/> to specify no specific property. 
    /// If omitted, the name of the caller member is used as value for this 
    /// parameter.</param>
    /// <returns><see langword="true"/> if <paramref name="newValue"/> is 
    /// different from <paramref name="oldValue"/> and the property was 
    /// successfully set; otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="OnPropertyChanged(PropertyChangedEventArgs?)"/>
    /// <seealso cref="PropertyChanged"/>
    protected bool SetProperty<T>(ref T oldValue, in T newValue,
        [CallerMemberName] string? propertyName = null)
    {
        return SetProperty(ref oldValue, in newValue,
            propertyName is null
                ? EmptyPropertyChangedEventArgs
                : new PropertyChangedEventArgs(propertyName));
    }


    /// <summary>
    /// Sets the specified property with the specified value, and raises 
    /// the <see cref="PropertyChanged"/> event if the values differ.
    /// </summary>
    /// <typeparam name="T">Type of the property value.</typeparam>
    /// <param name="oldValue">Original value of the property. This is a 
    /// reference to the member that will be set with the new value.</param>
    /// <param name="newValue">New value to set to the property.</param>
    /// <param name="eventArgs"><see cref="PropertyChangedEventArgs"/> 
    /// instance that will be used as arguments for the event, if the 
    /// property being set is changed, or <see langword="null"/> to specify 
    /// no specific property.</param>
    /// <returns><see langword="true"/> if <paramref name="newValue"/> is 
    /// different from <paramref name="oldValue"/> and the property was 
    /// successfully set; otherwise, <see langword="false"/>.</returns>
    /// <seealso cref="OnPropertyChanged(PropertyChangedEventArgs?)"/>
    /// <seealso cref="PropertyChanged"/>
    protected bool SetProperty<T>(ref T oldValue, in T newValue,
        PropertyChangedEventArgs? eventArgs)
    {
        if (Equals(oldValue, newValue))
            return false;

        oldValue = newValue;
        OnPropertyChanged(eventArgs ?? EmptyPropertyChangedEventArgs);
        return true;
    }


    /// <summary>
    /// Determines if both specified property names are equal.
    /// </summary>
    /// <param name="propertyX">First property name to compare.</param>
    /// <param name="propertyY">Second property name to compare.</param>
    /// <returns><see langword="true"/> if <paramref name="propertyX"/> 
    /// is identical to <paramref name="propertyY"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <remarks>
    /// This method compares property names using case-sensitive 
    /// ordinal comparison.
    /// <br/>
    /// Property names are trimmed of leading and trailing white spaces 
    /// before comparison.
    /// </remarks>
    /// <seealso cref="PropertyNameEquals(PropertyChangedEventArgs, string?)"/>
    protected static bool PropertyNameEquals(string? propertyX, string? propertyY)
    {
        if (propertyX is null && propertyY is null)
            return true;
        else if (propertyX is null || propertyY is null)
            return false;
        else
            return PropertyNameComparer.Equals(propertyX.Trim(), propertyY.Trim());
    }


    /// <summary>
    /// Determines if the property name associated with the specified 
    /// <see cref="PropertyChangedEventArgs"/> is identical to the specified 
    /// property name.
    /// </summary>
    /// <param name="e"><see cref="PropertyChangedEventArgs"/> instance containing a 
    /// property name to compare.</param>
    /// <param name="propertyName">Property name to compare with the property name 
    /// associated with <paramref name="e"/>.</param>
    /// <returns><see langword="true"/> if the property name associated with 
    /// <paramref name="e"/> is identical to <paramref name="propertyName"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="e"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// This method compares property names using case-sensitive 
    /// ordinal comparison.
    /// <br/>
    /// Property names are trimmed of leading and trailing white spaces 
    /// before comparison.
    /// </remarks>
    /// <seealso cref="PropertyNameEquals(string?, string?)"/>
    protected static bool PropertyNameEquals(PropertyChangedEventArgs e, string? propertyName)
    {
        if (e is null)
            throw new ArgumentNullException(nameof(e));

        return PropertyNameEquals(e.PropertyName, propertyName);
    }

    #endregion Methods


}
