using System;
using System.Collections.Generic;
using System.Configuration;
using HibernatingRhinos.Profiler.Appender.NHibernate;
using NHibernate.Driver;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Tool.hbm2ddl;
using Configuration = NHibernate.Cfg.Configuration;

namespace NHQueryRecorder.Tests.TestModels
{
    public class TestConfigurationSource
    {
        private static readonly Lazy<ISessionFactory> LazySessionFactory =
            new Lazy<ISessionFactory>(CreateSessionFactory);

        public static ISessionFactory SessionFactory
        {
            get { return LazySessionFactory.Value; }
        }

        private static ISessionFactory CreateSessionFactory()
        {
            if (ConfigurationManager.AppSettings["UseNHibernateProfiler"] == "true")
            {
                NHibernateProfiler.Initialize();
            }

            var configuration = new Configuration()
                .DataBaseIntegration(db =>
                {
                    db.Dialect<MsSql2008Dialect>();
                    db.ConnectionStringName = "sqlexpress";
                    db.BatchSize = 100;
					db.LogFormattedSql = false;
                    db.LogSqlInConsole = false;
                    db.Driver<Sql2008ClientDriver>();
                }).AddProperties(new Dictionary<string, string> { { "generate_statistics", "true"} });
            new Mapping().ApplyTo(configuration);
            BuildSchema(configuration);
            
            return configuration.BuildSessionFactory();
        }

        private static void BuildSchema(Configuration config)
        {
            new SchemaExport(config).Create(true, true);
        }
    }
}