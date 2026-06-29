using RoundTheCode.xUnit.Methods;

namespace RoundTheCode.xUnit
{
    public class NumberHelperTheoryTests
    {
        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(7)]
        public void IsOddNumber_ValueOf3_ShouldReturnTrue(int number)
        {
            Assert.True(NumberHelper.IsOddNumber(number));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(6)]
        public void IsOddNumber_ValueOf6_ShouldReturnFalse(int number)
        {
            Assert.False(NumberHelper.IsOddNumber(number));
        }

        [Theory]
        [InlineData(3)]
        [InlineData(5)]
        [InlineData(7)]
        public void IsEvenNumber_ValueOf3_ShouldReturnFalse(int number)
        {
            Assert.False(NumberHelper.IsEvenNumber(number));
        }

        [Theory]
        [InlineData(2)]
        [InlineData(4)]
        [InlineData(6)]
        public void IsEvenNumber_ValueOf6_ShouldReturnTrue(int number)
        {
            Assert.True(NumberHelper.IsEvenNumber(number));
        }

        [Theory]
        [InlineData(2, "Bad")]
        [InlineData(5, "Ok")]
        [InlineData(9, "Great")]
        public void RatingScore_Values_EqualCorrectRating
            (int number, string rating)
        {
            Assert.Equal(rating, NumberHelper.RatingScore(number));
        }
    }
}
