namespace RoundTheCode.xUnitMock
{
	using Moq;
	using RoundTheCode.xUnitMock.Library;

	public class ProductTests
	{
		private readonly Mock<ProductService> _productServiceMock;
		private readonly Mock<ICategoryService> _categoryServiceMock;

		public ProductTests()
		{
			_categoryServiceMock = new Mock<ICategoryService>();
			_productServiceMock = new Mock<ProductService>(
				_categoryServiceMock.Object);

			_categoryServiceMock.Setup(s => s.GetCategoryName(1))
				.Returns("Games");
			_categoryServiceMock.Setup(s => s.GetCategoryName(2))
				.Returns("Computers");
		}

		[Fact]
		public void GetProductCategory_CategoryIdOf1_ShouldReturnGames()
		{
			var productCategoryName =
				_productServiceMock.Object.GetProductCategory(1, 1);

			Assert.Equal("Games", productCategoryName);
		}

		[Fact]
		public void GetProductCategory_CategoryIdOf1_ShouldNotReturnComputers()
		{
			var productCategoryName =
				_productServiceMock.Object.GetProductCategory(1, 1);

			Assert.NotEqual("Computers", productCategoryName);
		}

		[Fact]
		public void GetProductCategory_CategoryIdOf1_CallsGetCategoryNameOnce()
		{
			var productCategoryName =
	_productServiceMock.Object.GetProductCategory(1, 1);

			_categoryServiceMock.Verify(s => s.GetCategoryName(1), Times.Once);
		}

		[Fact]
		public void GetProductCategory_CategoryIdOf3_ShouldThrowException()
		{
			_categoryServiceMock.Setup(s => s.GetCategoryName(3)).Throws<Exception>();

			var productCategoryName = () => _productServiceMock.Object
			.GetProductCategory(1, 3);

			Assert.Throws<Exception>(productCategoryName);
		}
	}
}
