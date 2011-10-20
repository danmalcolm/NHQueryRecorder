NH QUERY RECORDER


INTRODUCTION

Records SQL logged to the NHibernate.SQL log and converts each logged statement and parameter values to executable SQL. This is useful for recording a sequence of inserts / updates into a file, allowing it to be replayed, perhaps to generate a common set of demo data. 

USEFUL SCENARIOS

On the project that motivated the creation of NHQueryRecorder we are using Fluent NHibernate automapping against a rapidly changing object model. When the model changes we use NHibernate's SchemaExport and NHQueryRecorder to update source-controlled schema and test data creation scripts. All developers work against databases on their local SQL Server instance, which they can regenerate in seconds by running RoundHousE against these artifacts.

EXAMPLE USAGE

	public void Im_just_going_to_generate_some_data_off_of_our_domain_model_here()
	{		
		var session = SessionFactory.OpenSession();
		using (var recorder = new SqlCommandRecorder())
		{
			...
			GenerateSomeTestData(session);

			session.Flush();
			var recording = recorder.GetRecording();
			recording.WriteExecutableSqlToFile(@"..\..\temp\data\testdata.sql");
		}
	}

LIMITATIONS

There are a few! This is at proof of concept stage and is not recommended for production use.
Supports Sql Server only (only tested against Sql2008 dialect)
Date param values are only logged to second precision
Only works with log4net logging

TODO

Replace Regex with parser - Sprache
Support more dialects
Build script
Use file for SqlCommandProcessor tests, e.g. table with expected output for each dialect
NuGet package


ALTERNATIVES

NHProfiler records SQL and generates executable SQL, which you can copy / paste. I think you can automate it also. 


LICENSE

NHQueryRecorder is ©2011 Dan Malcolm and contributors under the BSD license. See https://github.com/danmalcolm/NHQueryRecorder/blob/master/LICENCE.txt.
