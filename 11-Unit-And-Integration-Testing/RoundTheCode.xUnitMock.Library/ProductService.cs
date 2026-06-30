namespace RoundTheCode.xUnitMock.Library
{
	public class ProductService : IProductService
	{
		private readonly ICategoryService _categoryService;

		public ProductService(ICategoryService categoryService)
		{
			_categoryService = categoryService;
		}

		public string GetProductCategory(int id, int categoryId)
		{
			return _categoryService.GetCategoryName(categoryId);
		}
	}
}
