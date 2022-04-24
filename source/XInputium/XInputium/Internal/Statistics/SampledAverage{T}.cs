using System;
using System.Collections;
using System.Collections.Generic;

namespace XInputium.Internal.Statistics;

// TODO Consider using a circular array, instead of a linked list, for the Sampled Average algorithm. Or, depending on the maximum length, a mix of both.

/// <summary>
/// Provides an efficient algorithm to determine the average value 
/// of a collection of values, as these values are continuously added. 
/// This is an abstract class.
/// </summary>
/// <typeparam name="T">Type of the sample values.</typeparam>
internal abstract class SampledAverage<T>
    : IReadOnlyCollection<T> where T : notnull
{


    #region Fields

    private readonly LinkedList<T> _samples = new();
    private T _totalSum;
    private T _average;

    #endregion Fields


    #region Constructors

    /// <summary>
    /// Initializes a new instance of a <see cref="SampledAverage{T}"/> 
    /// class, that has the specified maximum capacity.
    /// </summary>
    /// <param name="maxSampleLength">A number greater than 0, that 
    /// specifies how many sample values the <see cref="SampledAverage{T}"/> 
    /// instance can hold.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxSampleLength"/> is lower than 1.</exception>
    /// <seealso cref="MaxSampleLength"/>
    public SampledAverage(int maxSampleLength)
    {
        if (maxSampleLength < 1)
            throw new ArgumentOutOfRangeException(nameof(maxSampleLength));

        MaxSampleLength = maxSampleLength;
        _totalSum = Zero;
        _average = Zero;
    }

    #endregion Constructors


    #region Properties

    /// <summary>
    /// When overridden in a derived class, gets the zero 
    /// value for <typeparamref name="T"/>.
    /// </summary>
    protected abstract T Zero { get; }


    /// <summary>
    /// Gets the maximum number of sample values the 
    /// <see cref="SampledAverage{T}"/> can hold.
    /// </summary>
    /// <returns>An <see cref="int"/> greater than 0.</returns>
    /// <remarks>
    /// This property returns the maximum number of sample 
    /// values that can be stored in the current 
    /// <see cref="SampledAverage{T}"/> instance — that is, it 
    /// returns its maximum capacity.
    /// <see cref="MaxSampleLength"/> is always equal to or 
    /// greater than <see cref="Count"/>. If 
    /// <see cref="IsFull"/> property returns 
    /// <see langword="true"/> — meaning <see cref="Count"/> 
    /// equals to <see cref="MaxSampleLength"/> — and you add 
    /// a new sample value to the <see cref="SampledAverage{T}"/>, 
    /// all of the oldest excess samples will be automatically 
    /// removed.
    /// <br/><br/>
    /// The initial value of <see cref="MaxSampleLength"/> is 
    /// specified when the <see cref="SampledAverage{T}"/> is 
    /// instantiated. However, you can set a new maximum 
    /// capacity using <see cref="SetMaxSampleLength(int)"/> 
    /// method. See <see cref="SetMaxSampleLength(int)"/> for 
    /// more information.
    /// </remarks>
    /// <seealso cref="SetMaxSampleLength(int)"/>
    /// <seealso cref="Count"/>
    /// <seealso cref="IsFull"/>
    public int MaxSampleLength { get; private set; }


    /// <summary>
    /// Gets the number of sample values currently stored in 
    /// the <see cref="SampledAverage{T}"/>.
    /// </summary>
    /// <returns>An <see cref="int"/> equal to or greater 
    /// than 0, and equal to or lower than 
    /// <see cref="MaxSampleLength"/>.</returns>
    /// <seealso cref="MaxSampleLength"/>
    /// <seealso cref="IsFull"/>
    public int Count => _samples.Count;


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the 
    /// number of sample values in the 
    /// <see cref="SampledAverage{T}"/> is at its maximum capacity.
    /// </summary>
    /// <remarks>
    /// When the <see cref="SampledAverage{T}"/> is full, any 
    /// new sample values you add will make oldest samples to 
    /// get removed to make room for the new ones. This is the 
    /// intended behavior. This property allows you to 
    /// determine if the calculation of the sample average has 
    /// already reached its intended precision.
    /// </remarks>
    /// <seealso cref="Count"/>
    /// <seealso cref="MaxSampleLength"/>
    /// <seealso cref="IsEmpty"/>
    public bool IsFull => Count == MaxSampleLength;


    /// <summary>
    /// Gets a <see cref="bool"/> that indicates if the 
    /// <see cref="SampledAverage{T}"/> has currently no stored 
    /// sample values.
    /// </summary>
    /// <seealso cref="IsFull"/>
    /// <seealso cref="Count"/>
    public bool IsEmpty => Count == 0;


    /// <summary>
    /// Gets the sum of all sample values currently stored in 
    /// the <see cref="SampledAverage{T}"/>.
    /// </summary>
    /// <seealso cref="Average"/>
    /// <seealso cref="Count"/>
    public T TotalSum => _totalSum;


    /// <summary>
    /// Gets the average value of the sample values currently 
    /// stored in the <see cref="SampledAverage{T}"/>.
    /// </summary>
    /// <seealso cref="TotalSum"/>
    /// <seealso cref="Count"/>
    public T Average => _average;

    #endregion Properties


    #region Methods

    /// <summary>
    /// It's called whenever samples in the <see cref="SampledAverage{T}"/> 
    /// are added, removed, or cleared. When overridden in derived 
    /// classes, enables inheritors to determine additional values 
    /// for the derived class.
    /// </summary>
    /// <seealso cref="Add(T)"/>
    /// <seealso cref="Clear()"/>
    protected virtual void OnUpdated()
    {

    }


    /// <summary>
    /// When overridden in a derived class, gets the average sample 
    /// value, given the provided number of samples and the 
    /// provided samples sum.
    /// </summary>
    /// <param name="samplesCount">Number of samples to get the 
    /// average value of.</param>
    /// <param name="totalSum">Total sum of all the samples.</param>
    /// <returns>The calculated average value.</returns>
    protected abstract T GetAverage(int samplesCount, T totalSum);


    /// <summary>
    /// Calculates the sum of both specified values.
    /// </summary>
    /// <param name="x">First value of the sum.</param>
    /// <param name="y">Second value of the sum.</param>
    /// <returns>The sum of <paramref name="x"/> and 
    /// <paramref name="y"/>.</returns>
    /// <seealso cref="SubtractValue(T, T)"/>
    protected abstract T SumValue(T x, T y);


    /// <summary>
    /// Calculates the subtraction of <paramref name="y"/> 
    /// from <paramref name="x"/>.
    /// </summary>
    /// <param name="x">Value to subtract from.</param>
    /// <param name="y">Value to subtract by.</param>
    /// <returns>The result of 
    /// <paramref name="x"/>-<paramref name="y"/>.</returns>
    /// <seealso cref="SumValue(T, T)"/>
    protected abstract T SubtractValue(T x, T y);


    /// <summary>
    /// Adds a new sample into the <see cref="SampledAverage{T}"/>,
    /// that has the specified value.
    /// </summary>
    /// <param name="value">The sample value to add.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="value"/> is <see langword="null"/>.</exception>
    public void Add(T value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));

        LinkedListNode<T> node;
        if (_samples.Count == MaxSampleLength)
        {
            // Remove oldest node.
            node = _samples.First!;
            _totalSum = SubtractValue(_totalSum, node.Value);
            _samples.RemoveFirst();
            // Recycle the removed node and use it as the new node.
            node.Value = value;
            _samples.AddLast(value);
            _totalSum = SumValue(_totalSum, value);
        }
        else
        {
            // Create and add a new node.
            node = new(value);
            _samples.AddLast(node);
            _totalSum = SumValue(_totalSum, value);
        }

        _average = GetAverage(_samples.Count, _totalSum);
        OnUpdated();
    }


    /// <summary>
    /// Adds all the sample values in the specified collection 
    /// to the <see cref="SampledAverage{T}"/>.
    /// </summary>
    /// <param name="values">Collection that contains the 
    /// sample values to add.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="values"/> is <see langword="null"/>.</exception>
    public void Add(IEnumerable<T> values)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));

        foreach (var value in values)
        {
            Add(value);
        }
    }


    /// <summary>
    /// Removes all samples from the <see cref="SampledAverage{T}"/>.
    /// </summary>
    /// <seealso cref="Add(T)"/>
    public void Clear()
    {
        _samples.Clear();
        _totalSum = Zero;
        _average = Zero;

        OnUpdated();
    }


    /// <summary>
    /// Sets the maximum number of samples that the 
    /// <see cref="SampledAverage{T}"/> can hold.
    /// </summary>
    /// <param name="maxSampleLength">A number greater than 0, that 
    /// specifies how many sample values the 
    /// <see cref="SampledAverage{T}"/> can store.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="maxSampleLength"/> is less than 1.</exception>
    /// <remarks>
    /// This method can be used to increase or decrease the 
    /// current capacity of the <see cref="SampledAverage{T}"/> 
    /// instance. If you set a maximum sample length that is 
    /// lower than the current number of stored samples, all 
    /// of the oldest excess samples will be removed.
    /// <br/><br/>
    /// To determine the current capacity of the 
    /// <see cref="SampledAverage{T}"/> instance, use 
    /// <see cref="MaxSampleLength"/> property.
    /// </remarks>
    /// <seealso cref="MaxSampleLength"/>
    /// <seealso cref="Clear()"/>
    public void SetMaxSampleLength(int maxSampleLength)
    {
        if (maxSampleLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxSampleLength));

        MaxSampleLength = maxSampleLength;

        // Trim excess samples, if any.
        RemoveWhile((_) => _samples.Count > maxSampleLength);
    }


    /// <summary>
    /// Gets an enumerator that iterates though the 
    /// sample values in the <see cref="SampledAverage{T}"/>.
    /// </summary>
    /// <returns>An enumerator that can be used to 
    /// iterate though the sample values in the 
    /// <see cref="SampledAverage{T}"/>.</returns>
    public IEnumerator<T> GetEnumerator()
    {
        return _samples.GetEnumerator();
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }


    /// <summary>
    /// Removes all of the oldest sample values that match the 
    /// specified predicate, until the predicate returns 
    /// <see langword="false"/>.
    /// </summary>
    /// <param name="predicate">Predicate that will select the 
    /// samples to remove.</param>
    /// <returns>The number of samples removed.</returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="predicate"/> is <see langword="null"/>.</exception>
    public int RemoveWhile(Predicate<T> predicate)
    {
        if (predicate is null)
            throw new ArgumentNullException(nameof(predicate));

        int removedCount = 0;
        while (_samples.Count > 0 && predicate(_samples.First!.Value))
        {
            _totalSum = SubtractValue(_totalSum, _samples.First!.Value);
            _samples.RemoveFirst();
            removedCount++;
        }
        if (removedCount > 0)
        {
            _average = GetAverage(_samples.Count, _totalSum);
            OnUpdated();
        }
        return removedCount;
    }

    #endregion Methods


}
