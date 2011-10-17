using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;

namespace NHQueryRecorder.Tests.TestModels
{
    public class Mapping
    {
        public void ApplyTo(Configuration configuration)
        {
            var mapper = new ModelMapper();
            mapper.Class<Thing>(node =>
            {
                node.Id(x => x.Id, map =>
                {
                    map.Column("TestModel1Id");
                    map.Generator(Generators.GuidComb);
                });
                node.Property(x => x.StringProperty, map => map.Length(50));
                node.Property(x => x.BoolProperty);
            	node.Property(x => x.DateProperty);
				node.Property(x => x.IntProperty);
				node.Property(x => x.DecimalProperty);
            });

            configuration.AddMapping(mapper.CompileMappingForAllExplicitAddedEntities());
        }
    }
}