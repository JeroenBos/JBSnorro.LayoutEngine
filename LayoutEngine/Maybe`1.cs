﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBSnorro
{
	/// <summary> 
	/// This type represents an instance of the type <code>T</code> and indicates whether it has such a value or not.
	/// So it is like a nullable type, but the also for reference types and it is like option in F#. 
	/// </summary>
	/// <typeparam name="T"> The type of the instance to represent. </typeparam>
	public struct Maybe<T> : IEquatable<Maybe<T>>
	{
		/// <summary> Gets the option representing no instance of type <code>T</code>. </summary>
		public static readonly Maybe<T> None = new Maybe<T>();
		/// <summary> Creates the option representing the specified instance. </summary>
		/// <param name="value"> The value represented by the returned option. </param>
		public static Maybe<T> Some(T value)
		{
			return new Maybe<T>(value);
		}

		/// <summary> The backingfield of the represented instance. Is <code>default(T)</code> for None. </summary>
		private readonly T value;
		/// <summary> Gets the value represented by this instance if it has one, or throws if it doesn't. </summary>
		public T Value
		{
			[DebuggerHidden]
			get
			{
				if (!this.HasValue)
					throw new InvalidOperationException("Option has no value");
				return this.value;
			}
		}
		/// <summary> Gets whether this instance represents an instance of type <typeparamref name="T"/>. </summary>
		[MemberNotNullWhen(returnValue: true, nameof(Value))]
		public bool HasValue { get; private set; }
		/// <summary>
		/// Gets the value of this option, of the specified default otherwise.
		/// </summary>
		/// <param name="defaultAlternative"> The default to return in case this option does not hold a value. </param>
		public T? ValueOrDefault(T? defaultAlternative = default)
		{
			if (this.HasValue)
				return this.value;
			return defaultAlternative;
		}

		/// <summary> Creates a new option representing the specified instance. </summary>
		/// <param name="value"> The value represented by this option. </param>
		public Maybe(T value)
			: this()
		{
			this.value = value;
			this.HasValue = true;
		}

		public static implicit operator Maybe<T>(T value)
		{
			return new Maybe<T>(value);
		}

		/// <summary> Gets whether the specified object is equal to this option or the value held by this option, if any. </summary>
		/// <param name="obj"> The object to compare for equality against. </param>
		public override bool Equals(object? obj)
		{
			if (obj is Maybe<T>)
			{
				return Equals((Maybe<T>)obj);
			}
			if (this.HasValue)
			{
				return object.Equals(this.value, obj);
			}
			return false;
		}
		/// <summary> Gets whether the specified option equals this option, where two values are compared with the default equality comparer. </summary>
		/// <param name="other"> The option to compare for equality against. </param>
		public bool Equals(Maybe<T> other)
		{
			return Equals(other, EqualityComparer<T>.Default);
		}

		/// <summary> Gets whether the specified option equals this option, where two values are compared with a specified equality comparer. </summary>
		/// <param name="other"> The option to compare for equality against. </param>
		/// <param name="equalityComparer"> The equality comparer to use for comparing for equality. </param>
		public bool Equals(Maybe<T> other, IEqualityComparer<T> equalityComparer)
		{
			if (equalityComparer == null) throw new ArgumentException(nameof(equalityComparer));

			if (!this.HasValue)
				return !other.HasValue;
			return other.HasValue && equalityComparer.Equals(this.value, other.value);
		}

		public override string? ToString()
		{
			if (this.HasValue)
				return this.Value.ToString();
			return "None";
		}
		public override int GetHashCode()
		{
			if (this.HasValue)
			{
				return this.Value.GetHashCode();
			}
			else
			{
				return 0; // assuming this is the hash code of null
			}
		}
	}
	public static class Option
	{
		/// <summary>
		/// Allows omitting the type parameter to option when using <see cref="Option{T}.Some(T)"/>
		/// </summary>
		public static Maybe<T> Some<T>(T t)
		{
			return Maybe<T>.Some(t);
		}
	}
}