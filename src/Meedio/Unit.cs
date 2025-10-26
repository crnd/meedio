namespace Meedio;

public readonly struct Unit : IComparable, IComparable<Unit>, IEquatable<Unit>
{
	public static readonly Unit Value = new();

	public override string ToString() => "()";

	public int CompareTo(object? obj) => 0;

	public int CompareTo(Unit other) => 0;

	public bool Equals(Unit other) => true;

	public override bool Equals(object? obj) => obj is Unit;

	public override int GetHashCode() => 0;

	public static bool operator ==(Unit left, Unit right) => true;

	public static bool operator !=(Unit left, Unit right) => false;

	public static bool operator <(Unit left, Unit right) => false;

	public static bool operator <=(Unit left, Unit right) => true;

	public static bool operator >(Unit left, Unit right) => false;

	public static bool operator >=(Unit left, Unit right) => true;
}
