using Xunit;

namespace Meedio.UnitTests;

public class UnitTests
{
	[Fact]
	public void StringValue()
	{
		Assert.Equal("()", Unit.Value.ToString());
	}

	[Fact]
	public void CompareTo()
	{
		var unit1 = Unit.Value;
		var unit2 = Unit.Value;

		Assert.StrictEqual(0, unit1.CompareTo(new object()));
		Assert.StrictEqual(0, unit1.CompareTo(unit2));
	}

	[Fact]
	public void UnitsAreEqual()
	{
		var unit1 = Unit.Value;
		var unit2 = Unit.Value;

		Assert.Equal(unit1, unit2);
		Assert.StrictEqual(unit1, unit2);
		Assert.True(unit1 == unit2);
		Assert.False(unit1 != unit2);
		Assert.True(unit1.Equals(unit2));
		Assert.True(unit1 == default);
	}

	[Fact]
	public void NonUnitEquals()
	{
		Assert.False(Unit.Value.Equals(new object()));
		Assert.False(Unit.Value.Equals(123));
		Assert.False(Unit.Value.Equals("abc"));
		Assert.False(Unit.Value.Equals(new List<Guid>()));
		Assert.False(Unit.Value.Equals(Guid.NewGuid()));
		Assert.False(Unit.Value.Equals(new int[5]));
	}

	[Fact]
	public void HashCode()
	{
		Assert.Equal(0, Unit.Value.GetHashCode());
	}

	[Fact]
	public void Comparisons()
	{
		var unit1 = Unit.Value;
		var unit2 = Unit.Value;

		Assert.False(unit1 < unit2);
		Assert.False(unit1 > unit2);
		Assert.True(unit1 <= unit2);
		Assert.True(unit1 >= unit2);
	}
}
