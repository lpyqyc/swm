using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Swm.InboundOrders.Mappings
{
    internal class InboundOrderMapping : ClassMapping<InboundOrder>
    {
        public InboundOrderMapping()
        {
            Table("InboundOrder");
            DynamicUpdate(true);
            BatchSize(10);
            Lazy(false);
            DiscriminatorValue(1);

            Id(cl => cl.InboundOrderId, id => id.Generator(Generators.Identity));
            Discriminator(dm => {
                dm.NotNullable(true);
            });

            NaturalId(npm => {
                npm.Property(cl => cl.InboundOrderCode, prop => prop.Update(false));
            }, m => m.Mutable(false));

            Version(cl => cl.v, v => v.Column("v"));

            Property(cl => cl.ctime, prop => prop.Update(false));
            Property(cl => cl.cuser, prop => prop.Update(false));
            Property(cl => cl.mtime);
            Property(cl => cl.muser);

            Property(cl => cl.BizType);
            Property(cl => cl.BizOrder);

            Property(cl => cl.Closed);
            Property(cl => cl.ClosedBy);
            Property(cl => cl.ClosedAt);

            Property(cl => cl.Comment);

            Set(cl => cl.Lines, set => {
                set.Inverse(true);
                set.Cascade(Cascade.All | Cascade.DeleteOrphans);
                set.BatchSize(10);
                set.Key(key => { 
                    key.Column("InboundOrderId");
                    key.NotNullable(true);
                    key.Update(false);
                });
            }, rel => rel.OneToMany());

        }
    }
}
