using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Swm.Model.Mappings
{
    internal class InboundLineMapping : ClassMapping<InboundLine>
    {
        public InboundLineMapping()
        {
            Table("InboundLines");
            DynamicUpdate(true);
            BatchSize(10);
            Lazy(false);
            DiscriminatorValue(1);

            Id(cl => cl.InboundLineId, id => id.Generator(Generators.Identity));
            Discriminator(dm => {
                dm.NotNullable(true);
            });

            ManyToOne(cl => cl.InboundOrder, m => {
                m.Column("InboundOrderId");
                m.Update(false);
            });

            ManyToOne(cl => cl.Material, m => {
                m.Column("MaterialId");
                m.Update(false);
            });

            Property(cl => cl.Batch);
            Property(cl => cl.StockStatus);
            Property(cl => cl.Uom);

            Property(cl => cl.QuantityExpected);
            Property(cl => cl.QuantityReceived);
            Property(cl => cl.Dirty);

            Property(cl => cl.Comment);
        }
    }
}
