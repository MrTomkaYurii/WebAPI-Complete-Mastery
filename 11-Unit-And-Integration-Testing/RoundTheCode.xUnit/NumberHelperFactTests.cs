using RoundTheCode.xUnit.Methods;

namespace RoundTheCode.xUnit
{
    public class NumberHelperFactTests
    {
        [Fact]
        public void IsOddNumber_ValueOf3_ShouldReturnTrue()
        {
            Assert.True(NumberHelper.IsOddNumber(3));
        }

        [Fact]
        public void IsOddNumber_ValueOf6_ShouldReturnFalse()
        {
            Assert.False(NumberHelper.IsOddNumber(6));
        }

        [Fact]
        public void IsEvenNumber_ValueOf3_ShouldReturnFalse()
        {
            Assert.False(NumberHelper.IsEvenNumber(3));
        }

        [Fact]
        public void IsEvenNumber_ValueOf6_ShouldReturnTrue()
        {
            Assert.True(NumberHelper.IsEvenNumber(6));
        }

        [Fact]
        public void RatingScore_ValueOf7_EqualsGreat()
        {
            Assert.Equal("Great", NumberHelper.RatingScore(7));
        }

        [Fact]
        public void RatingScore_ValueOf7_DoesNotEqualsBad()
        {
            Assert.NotEqual("Bad", NumberHelper.RatingScore(7));
        }
    }
}
