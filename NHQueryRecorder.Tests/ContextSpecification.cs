using log4net.Config;
using NUnit.Framework;

namespace NHQueryRecorder.Tests
{
    /// <summary>
	/// Base class for writing tests in the when.. should.. format
	/// </summary>
	public class ContextSpecification
    {
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
	        BasicConfigurator.Configure();
			before_set_up_context();
			set_up_context();
			after_set_up_context();
			because();
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
