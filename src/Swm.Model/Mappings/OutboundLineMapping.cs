using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Swm.Model.Mappings
{
    internal class OutboundLineMapping : ClassMapping<OutboundLine>
    {
        public OutboundLineMapping()
        {
            Table("OutboundLines");
            DynamicUpdate(true);
            BatchSize(10);
            Lazy(false);
            DiscriminatorValue(1);

            Id(cl => cl.OutboundLineId, id => id.Generator(Generators.Identity));
            Discriminator(dm =>
            {
                dm.NotNullable(true);
            });

            ManyToOne(cl => cl.OutboundOrder, m =>
            {
                m.Column("OutboundOrderId");
                m.Update(false);
            });

            ManyToOne(cl => cl.Material, m =>
            {
                m.Column("MaterialId");
                m.Update(false);
            });

            Property(cl => cl.Batch);
            Property(cl => cl.StockStatus);

            Property(cl => cl.Uom);
            Property(cl => cl.QuantityRequired);
            Property(cl => cl.QuantityDelivered);
            Property(cl => cl.QuantityUndelivered);
            Property(cl => cl.Dirty);

            Property(cl => cl.Comment);

            Set(cl => cl.Allocations, set => {
                set.Table("OutboundLineAllocations");
                set.BatchSize(10);
                set.Key(key => key.Column("OutboundLineId"));
            }, rel => rel.Component(comp =>
            {
                comp.ManyToOne(cl => cl.UnitloadItem, m => {
                    m.Column("UnitloadItemId");
                    m.NotNullable(true);
                    m.Update(false);
                });
                comp.Property(cl => cl.Quantity);
            }));

        }
    }


}
