using System;
using log4net.Config;
using NUnit.Framework;

namespace NHQueryRecorder.Tests
{
    /// <summary>
	/// Base class for writing tests in the when.. should.. format
	/// </summary>
	public class ContextSpecification
    {
        private static bool log4NetConfigured;

        [TestFixtureSetUp]
        protected void TestFixtureSetUp()
		{
		    SetupSpecification();
		}

	    [TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			TearDownSpecification();
		}

	    private void SetupSpecification()
		{
            ConfigureLog4Net();
			before_set_up_context();
			set_up_context();
			after_set_up_context();
			because();
		}

        private static void ConfigureLog4Net()
        {
            if(!log4NetConfigured)
            {
                BasicConfigurator.Configure();
                log4NetConfigured = true;
            } 
        }

        private void TearDownSpecification()
	    {
	        clean_up_context();
	    }

	    protected virtual void before_set_up_context()
		{
		}

	    protected virtual void after_set_up_context()
		{
		}

	    protected virtual void set_up_context()
		{
		}

	    protected virtual void because()
		{
		}

	    protected virtual void clean_up_context()
	    {
	        
	    }
    }
}
