using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XInputium;

/// <summary>
/// Implements an <see cref="InputEvent"/> that can group 
/// children <see cref="InputEvent"/> instances, listen 
/// for their events and trigger an event when any of 
/// its children event is triggered.
/// </summary>
/// <seealso cref="InputEvent"/>
public class InputEventGroup : InputEvent,
    IReadOnlyCollection<InputEvent>, ICollection<InputEvent>
{


    #region Fields

    private readonly Lazy<HashSet<InputEvent>> _childrenLazy  // Lazy initializer for the value of Children property.
        = new(true);

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Initializes a new instance of an <see cref="InputEventGroup"/> 
    /// class.
    /// </summary>
    public InputEventGroup()
        : base()
    {

    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// Gets the collection that contains the child 
    /// <see cref="InputEvent"/> instances of the 
    /// <see cref="InputEventGroup"/>.
    /// </summary>
    private ISet<InputEvent> Children => _childrenLazy.Value;


    /// <summary>
    /// Gets the number of child <see cref="InputEvent"/> 
    /// instances in the <see cref="InputEventGroup"/>.
    /// </summary>
    /// <seealso cref="Add(InputEvent)"/>
    /// <seealso cref="Remove(InputEvent)"/>
    public int Count => Children.Count;


    bool ICollection<InputEvent>.IsReadOnly => false;

    #endregion Properties


    #region Methods

    /// <summary>
    /// Updates the children of the <see cref="InputEventGroup"/>. 
    /// Overrides <see cref="InputEvent.OnUpdate(TimeSpan)"/>
    /// </summary>
    /// <param name="time">The amount of time elapsed since 
    /// the last update operation.</param>
    /// <seealso cref="InputEvent.OnUpdate(TimeSpan)"/>
    /// <seealso cref="InputEvent.Update(TimeSpan)"/>
    protected override void OnUpdate(TimeSpan time)
    {
        foreach (var inputEvent in Children)
        {
            inputEvent.Update(time);
        }
    }


    private void OnEventRaised(object? sender, InputEventArgs e)
    {
        Raise(sender, e);
    }


    /// <summary>
    /// Adds the specified <see cref="InputEvent"/> instance 
    /// to the <see cref="InputEventGroup"/> children.
    /// </summary>
    /// <param name="inputEvent"><see cref="InputEvent"/> 
    /// instance to add.</param>
    /// <returns><see langword="true"/> if <paramref name="inputEvent"/> 
    /// is not already a child of the <see cref="InputEventGroup"/> 
    /// and was successfully added; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputEvent"/> is <see langword="null"/>.</exception>
    /// <seealso cref="Remove(InputEvent)"/>
    /// <seealso cref="Count"/>
    public bool Add(InputEvent inputEvent)
    {
        if (inputEvent is null)
            throw new ArgumentNullException(nameof(inputEvent));

        if (Children.Add(inputEvent))
        {
            inputEvent.AddHandler(OnEventRaised);
            return true;
        }
        return false;
    }


    void ICollection<InputEvent>.Add(InputEvent item)
    {
        Add(item);
    }


    /// <summary>
    /// Removes the specified <see cref="InputEvent"/> from 
    /// the <see cref="InputEventGroup"/>.
    /// </summary>
    /// <param name="inputEvent"><see cref="InputEvent"/> 
    /// instance to remove.</param>
    /// <returns><see langword="true"/> if <paramref name="inputEvent"/> 
    /// was a child of the <see cref="InputEventGroup"/> and 
    /// was successfully removed; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputEvent"/> is <see langword="null"/>.</exception>
    /// <seealso cref="Add(InputEvent)"/>
    /// <seealso cref="Clear()"/>
    public bool Remove(InputEvent inputEvent)
    {
        if (inputEvent is null)
            throw new ArgumentNullException(nameof(inputEvent));

        if (Children.Remove(inputEvent))
        {
            inputEvent.RemoveHandler(OnEventRaised);
            return true;
        }
        return false;
    }


    /// <summary>
    /// Removes all child <see cref="InputEvent"/> instances 
    /// from the <see cref="InputEventGroup"/>.
    /// </summary>
    /// <seealso cref="Remove(InputEvent)"/>
    /// <seealso cref="Count"/>
    public void Clear()
    {
        foreach (var child in Children)
        {
            child.RemoveHandler(OnEventRaised);
        }
        Children.Clear();
    }


    /// <summary>
    /// Determine if the specified <see cref="InputEvent"/> 
    /// instance is a child of the <see cref="InputEventGroup"/>.
    /// </summary>
    /// <param name="inputEvent"><see cref="InputEvent"/> 
    /// instance to check for.</param>
    /// <returns><see langword="true"/> if <paramref name="inputEvent"/> 
    /// is a child of the <see cref="InputEventGroup"/>; 
    /// otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="inputEvent"/> is <see langword="null"/>.</exception>
    /// <seealso cref="Add(InputEvent)"/>
    /// <seealso cref="Remove(InputEvent)"/>
    /// <seealso cref="Count"/>
    public bool Contains(InputEvent inputEvent)
    {
        if (inputEvent is null)
            throw new ArgumentNullException(nameof(inputEvent));

        return Children.Contains(inputEvent);
    }


    /// <summary>
    /// Gets an enumerator that iterates through the children 
    /// of the <see cref="InputEventGroup"/>.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate 
    /// through the children of <see cref="InputEventGroup"/>.</returns>
    /// <seealso cref="Count"/>
    /// <seealso cref="Contains(InputEvent)"/>
    public IEnumerator<InputEvent> GetEnumerator()
    {
        return Children.GetEnumerator();
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    void ICollection<InputEvent>.CopyTo(InputEvent[] array, int arrayIndex)
    {
        Children.CopyTo(array, arrayIndex);
    }

    #endregion Methods


}
