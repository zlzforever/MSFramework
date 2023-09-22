using MicroserviceFramework.Domain;
using MongoDB.Bson;
using Template.Domain.Aggregates.Project.Events;

namespace Template.Domain.Aggregates.Project
{
    /// <summary>
    /// 使用充血模型
    /// </summary>
    public class Product : ModificationAggregateRoot
    {
        private Product(ObjectId id) : base(id)
        {
        }

        public static Product New(string name, int price, ProductType productType)
        {
            var product = new Product(ObjectId.GenerateNewId())
            {
                Name = name, Price = price, ProductType = productType,
            };
            product.AddDomainEvent(new ProjectCreatedEvent { Id = product.Id });
            return product;
        }

        public string Name { get; private set; }

        public int Price { get; private set; }

        public ProductType ProductType { get; private set; }

        public void SetName(string name)
        {
            if (!Name.Equals(name))
            {
                Name = name;
            }
        }

        public override string ToString()
        {
            return $"[ENTITY {GetType().Name}] Id = {Id}, Name = {Name}, Price = {Price}, ProductType = {ProductType}";
        }

        public void Delete()
        {
            AddDomainEvent(new ProjectDeletedEvent { Id = Id });
        }
    }
}
