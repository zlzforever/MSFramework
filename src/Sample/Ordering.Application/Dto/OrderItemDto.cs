namespace Ordering.Application.Dto;

public class OrderItemDto
{
    /// <summary>
    /// 销售产品
    /// </summary>
    public OrderProductDto Product { get; private set; }

    /// <summary>
    /// 单价
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// 数量
    /// </summary>
    public int Units { get; private set; }

    /// <summary>
    /// 折扣
    /// </summary>
    public decimal Discount { get; private set; }

    public class OrderProductDto
    {
        /// <summary>
        /// 产品标识
        /// </summary>
        public string ProductId { get; set; }


        public string Name { get; set; }

        /// <summary>
        /// 图片链接
        /// </summary>
        public string PictureUrl { get; set; }
    }
}
